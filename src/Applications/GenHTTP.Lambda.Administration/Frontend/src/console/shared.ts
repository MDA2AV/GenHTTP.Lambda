import { ApiError, api, type SharedFile, type SharedFiles } from '@genhttp-lambda/kit/api';

/*
 * What the panel's sections share about sharing a file: what it would be
 * called, and why it would not be taken.
 */

/** The name a file of this computer is shared under: its own, in the letters a name may have. */
export function sharedName(name: string): string {
  return name
    .normalize('NFKD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^A-Za-z0-9._-]+/g, '-')
    .replace(/-*\.-*/g, '.')
    .replace(/^[^A-Za-z0-9]+/, '');
}

/** What the end of a name says the file is, or nothing for one that is not shared. */
export function kindOf(name: string, limits: SharedFiles['limits']): SharedFile['kind'] | null {
  const end = name.split('.').pop()?.toLowerCase() ?? '';

  return (Object.entries(limits.types) as [SharedFile['kind'], string[]][]).find(([, ends]) => ends.includes(end))?.[0] ?? null;
}

/** Why a file cannot be shared, where that is known before it is sent. */
export function refusal(name: string, file: File, limits: SharedFiles['limits']): string | null {
  if (kindOf(name, limits) === null) {
    return `${file.name}: a shared file is a picture, a font, a style sheet or a text.`;
  }

  if (file.size > limits.bytes) {
    return `${file.name}: a shared file may be ${Math.round(limits.bytes / 1024 / 1024)} MB at most.`;
  }

  return name.length > limits.name ? `${file.name}: its name may be ${limits.name} characters at most.` : null;
}

/** Shares a file, or says why it was not taken. */
export async function share(token: string, name: string, file: File): Promise<SharedFile | string> {
  try {
    return await api.admin.shareFile(token, name, file);
  } catch (error) {
    if (error instanceof ApiError && error.status !== 401 && error.status !== 403) {
      return error.message;
    }

    throw error;
  }
}

/**
 * Whether a file of this computer is the shared one, by what it holds - which
 * the address of the shared one names. Not known, and so not the same, on a
 * page a browser does not trust with it: one that is neither https nor local.
 */
export async function isSame(file: File, shared: SharedFile): Promise<boolean> {
  if (!window.crypto?.subtle || file.size !== shared.bytes) {
    return false;
  }

  const hash = new Uint8Array(await window.crypto.subtle.digest('SHA-256', await file.arrayBuffer()));
  const hex = [...hash].map((byte) => byte.toString(16).padStart(2, '0')).join('');

  return hex.startsWith(new URL(shared.versionedPath, window.location.origin).searchParams.get('v') ?? '-');
}
