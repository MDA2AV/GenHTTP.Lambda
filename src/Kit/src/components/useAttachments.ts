import { useState } from 'react';

import { api, ApiError, type AttachmentPolicy, type RunFile } from '../api';
import { bytes } from '../ui/format';

/** What a page says when a file cannot go with a message. */
export interface AttachWords {
  /** More files than one message takes. */
  tooMany: (most: number) => string;
  /** A file larger than the agent takes. */
  tooLarge: (name: string, most: string) => string;
  /** A kind of file the agent takes none of. */
  kind: (name: string) => string;
  /** It could not be uploaded. */
  failed: (name: string) => string;
}

/** Whether a media type is one of the policy's, a range such as image/* included. */
export function accepts(policy: AttachmentPolicy, type: string): boolean {
  const kind = type.toLowerCase();

  return policy.types.some((allowed) => {
    const range = allowed.toLowerCase();

    return range.endsWith('/*') ? kind.startsWith(range.slice(0, -1)) : kind === range;
  });
}

/**
 * The files the next message goes with: uploaded as they are picked, so the
 * message names them by id - checked here first against what the agent
 * takes, so nobody waits for an upload that is refused. A file picked and
 * not sent is removed by the platform after an hour.
 */
export function useAttachments(policy: AttachmentPolicy | null | undefined, words: AttachWords) {
  const [files, setFiles] = useState<RunFile[]>([]);
  const [uploading, setUploading] = useState(0);
  const [problem, setProblem] = useState<string | null>(null);

  async function add(picked: File[]) {
    if (!policy || picked.length === 0) {
      return;
    }

    setProblem(null);

    const room = policy.maxCount - files.length;

    if (picked.length > room) {
      setProblem(words.tooMany(policy.maxCount));
      picked = picked.slice(0, Math.max(0, room));
    }

    for (const file of picked) {
      if (!accepts(policy, file.type || 'application/octet-stream')) {
        setProblem(words.kind(file.name));
        continue;
      }

      if (file.size > policy.maxBytes) {
        setProblem(words.tooLarge(file.name, bytes(policy.maxBytes)));
        continue;
      }

      setUploading((n) => n + 1);

      try {
        const kept = await api.attachments.upload(file);

        setFiles((was) => [...was, kept]);
      } catch (error) {
        setProblem(error instanceof ApiError && error.status === 413 ? words.tooLarge(file.name, bytes(policy.maxBytes)) : words.failed(file.name));
      } finally {
        setUploading((n) => n - 1);
      }
    }
  }

  return {
    policy,
    files,
    uploading: uploading > 0,
    problem,
    add: (picked: File[]) => void add(picked),
    remove: (file: RunFile) => setFiles((was) => was.filter((f) => f.id !== file.id)),
    clear: () => {
      setFiles([]);
      setProblem(null);
    },
    /** What a message names them by. */
    ids: files.map((file) => file.id),
    /** What the file picker offers. */
    accept: policy?.types.join(',') ?? '',
  };
}

export type Attachments = ReturnType<typeof useAttachments>;
