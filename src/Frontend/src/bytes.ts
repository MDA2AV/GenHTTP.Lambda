/**
 * Moving files between the browser and an API that carries them as base64.
 */

/**
 * Whether bytes are text somebody could reasonably edit.
 *
 * A NUL settles it - no text file has one - and so does anything that is not
 * valid UTF-8. Guessing from the extension would be wrong for exactly the
 * files it matters for: a .txt full of bytes and a .dat full of JSON.
 */
export function readable(bytes: Uint8Array): boolean {
  if (bytes.includes(0)) {
    return false;
  }

  try {
    new TextDecoder('utf-8', { fatal: true }).decode(bytes);
    return true;
  } catch {
    return false;
  }
}

/** Base64 of bytes in hand, in chunks so a large file does not blow the stack. */
export function encodeBytes(bytes: Uint8Array): string {
  let binary = '';

  for (let i = 0; i < bytes.length; i += 0x8000) {
    binary += String.fromCharCode(...bytes.subarray(i, i + 0x8000));
  }

  return btoa(binary);
}

/** Reads a file as base64, without the data URL prefix the reader adds. */
export function encode(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = () => resolve(String(reader.result).split(',')[1] ?? '');
    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(file);
  });
}

export function decode(content: string): ArrayBuffer {
  const binary = atob(content);
  const buffer = new ArrayBuffer(binary.length);
  const bytes = new Uint8Array(buffer);

  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }

  return buffer;
}

/** Hands the browser bytes to save, rather than a link to somewhere that serves them. */
export function download(name: string, content: Uint8Array<ArrayBuffer>) {
  const url = URL.createObjectURL(new Blob([content]));
  const link = document.createElement('a');

  link.href = url;
  link.download = name.split('/').pop() ?? name;
  link.click();

  URL.revokeObjectURL(url);
}
