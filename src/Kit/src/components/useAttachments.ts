import { useRef, useState } from 'react';

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

/**
 * The kinds of file people attach, by their extension. A browser takes a
 * file for what the machine it runs on says, and that is often nothing or a
 * stream of bytes (a Word document where no office suite is installed) or
 * something else (a CSV file where Excel is) - so where the extension is one
 * of these, it decides.
 */
const KINDS: Record<string, string> = {
  png: 'image/png',
  jpg: 'image/jpeg',
  jpeg: 'image/jpeg',
  gif: 'image/gif',
  webp: 'image/webp',
  svg: 'image/svg+xml',
  txt: 'text/plain',
  md: 'text/markdown',
  csv: 'text/csv',
  html: 'text/html',
  htm: 'text/html',
  json: 'application/json',
  pdf: 'application/pdf',
  docx: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  xlsx: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  pptx: 'application/vnd.openxmlformats-officedocument.presentationml.presentation',
  odt: 'application/vnd.oasis.opendocument.text',
  ods: 'application/vnd.oasis.opendocument.spreadsheet',
  odp: 'application/vnd.oasis.opendocument.presentation',
};

/** The kind of a file, as it is sent: by its extension where that is known, else as the browser says. */
export function kindOf(file: File): string {
  const dot = file.name.lastIndexOf('.');

  return (dot > 0 && KINDS[file.name.slice(dot + 1).toLowerCase()]) || file.type || 'application/octet-stream';
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
 * What the file picker offers: the kinds, and the extensions they stand for,
 * since a picker that knows no kind for .docx would hide every Word document.
 */
function pickable(policy: AttachmentPolicy): string {
  const extensions = Object.entries(KINDS)
    .filter(([, kind]) => accepts(policy, kind))
    .map(([extension]) => `.${extension}`);

  return [...policy.types, ...extensions].join(',');
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

  // what is on its way counts as well: a second pick while the first uploads
  // must not end with more files than a message takes
  const inFlight = useRef(0);

  async function add(picked: File[]) {
    if (!policy || picked.length === 0) {
      return;
    }

    setProblem(null);

    const room = policy.maxCount - files.length - inFlight.current;

    if (picked.length > room) {
      setProblem(words.tooMany(policy.maxCount));
      picked = picked.slice(0, Math.max(0, room));
    }

    for (const file of picked) {
      const kind = kindOf(file);

      if (!accepts(policy, kind)) {
        setProblem(words.kind(file.name));
        continue;
      }

      if (file.size > policy.maxBytes) {
        setProblem(words.tooLarge(file.name, bytes(policy.maxBytes)));
        continue;
      }

      setUploading((n) => n + 1);
      inFlight.current++;

      try {
        const kept = await api.attachments.upload(file, kind);

        setFiles((was) => [...was, kept]);
      } catch (error) {
        setProblem(error instanceof ApiError && error.status === 413 ? words.tooLarge(file.name, bytes(policy.maxBytes)) : words.failed(file.name));
      } finally {
        inFlight.current--;
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
    accept: policy ? pickable(policy) : '',
  };
}

export type Attachments = ReturnType<typeof useAttachments>;
