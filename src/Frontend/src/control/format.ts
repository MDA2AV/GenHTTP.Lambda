/**
 * How the control center says numbers and times. One place, so a byte count
 * reads the same on every tab and "3 min ago" means the same thing twice.
 */

export function bytes(value: number): string {
  if (value < 1024) {
    return `${Math.round(value)} B`;
  }

  if (value < 1024 * 1024) {
    return `${(value / 1024).toFixed(1)} kB`;
  }

  return `${(value / 1024 / 1024).toFixed(1)} MB`;
}

/** Counts that can get large, shortened only once they do. */
export function count(value: number): string {
  if (value < 10_000) {
    return value.toLocaleString();
  }

  if (value < 1_000_000) {
    return `${(value / 1000).toFixed(value < 100_000 ? 1 : 0)}k`;
  }

  return `${(value / 1_000_000).toFixed(1)}M`;
}

export function millis(value: number): string {
  if (value < 1) {
    return `${value.toFixed(2)} ms`;
  }

  if (value < 1000) {
    return `${value.toFixed(value < 10 ? 1 : 0)} ms`;
  }

  return `${(value / 1000).toFixed(2)} s`;
}

export function percent(part: number, whole: number): string {
  if (whole === 0) {
    return '0%';
  }

  const share = (part / whole) * 100;

  // a single failure in ten thousand should not read as none
  return share > 0 && share < 0.1 ? '<0.1%' : `${share.toFixed(share < 10 ? 1 : 0)}%`;
}

/** A span of time as somebody would say it: 3 d 4 h, 12 min, 40 s. */
export function span(seconds: number): string {
  const s = Math.max(0, Math.round(seconds));

  if (s < 60) {
    return `${s} s`;
  }

  const minutes = Math.floor(s / 60);

  if (minutes < 60) {
    return `${minutes} min`;
  }

  const hours = Math.floor(minutes / 60);

  if (hours < 48) {
    const rest = minutes % 60;
    return rest > 0 && hours < 10 ? `${hours} h ${rest} min` : `${hours} h`;
  }

  const days = Math.floor(hours / 24);
  const rest = hours % 24;

  return rest > 0 && days < 10 ? `${days} d ${rest} h` : `${days} d`;
}

/** How long ago, or how long from now, in the same words. */
export function ago(when: string | Date | null | undefined, now = Date.now()): string {
  if (!when) {
    return 'never';
  }

  const at = typeof when === 'string' ? new Date(when).getTime() : when.getTime();
  const seconds = (now - at) / 1000;

  if (Math.abs(seconds) < 10) {
    return 'just now';
  }

  return seconds > 0 ? `${span(seconds)} ago` : `in ${span(-seconds)}`;
}

export function stamp(when: string | Date): string {
  return new Date(when).toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function clock(when: string | Date, seconds = false): string {
  return new Date(when).toLocaleTimeString(undefined, {
    hour: '2-digit',
    minute: '2-digit',
    ...(seconds ? { second: '2-digit' } : {}),
  });
}

/** The words for where something came from. */
export function origin(value?: string | null): string {
  switch (value) {
    case 'agent':
      return 'agent';
    case 'template':
      return 'template';
    case 'admin':
      return 'operator';
    case 'system':
      return 'platform';
    case 'api':
      return 'API / editor';
    default:
      return 'unknown';
  }
}

/** The words for why something stopped being online. */
export function ending(value?: string | null): string {
  switch (value) {
    case 'replaced':
      return 'replaced by a newer deployment';
    case 'stopped':
      return 'taken offline';
    case 'expired':
      return 'expired after going unused';
    case 'admin':
      return 'taken offline by the operator';
    default:
      return 'ended';
  }
}

/**
 * A log line with the lambda's own address taken out of its paths. Inside
 * the lambda's own view "/lambda/my-app/api" says nothing "/api" does not.
 */
export function local(text: string, publicKey: string): string {
  return text.split(`/lambda/${publicKey}/`).join('/').split(`/lambda/${publicKey} `).join('/ ');
}

/**
 * Whether the code asks for a directory to be served. The same test the
 * server makes for the summary, so the two never disagree.
 */
export const servesAssets = (code: string) => /\bAssets\s*\.\s*(App|Files|Tree)\s*\(/.test(code);

export const servesWorkspace = (code: string) => /\bWorkspace\s*\.\s*(App|Files|Tree)\s*\(/.test(code);
