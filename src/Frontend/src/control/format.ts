import { SHARED, type SharedWords } from './words';

/**
 * How the control center says numbers and times. One place, so a byte count
 * reads the same on every tab and "3 min ago" means the same thing twice.
 *
 * What is said in words takes them from the caller, in the language it is
 * showing - the console leaves them out and is English.
 */

export function bytes(value: number): string {
  if (value < 1024) {
    return `${Math.round(value)} B`;
  }

  if (value < 1024 * 1024) {
    return `${(value / 1024).toFixed(1)} kB`;
  }

  if (value < 1024 * 1024 * 1024) {
    return `${(value / 1024 / 1024).toFixed(1)} MB`;
  }

  return `${(value / 1024 / 1024 / 1024).toFixed(1)} GB`;
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
export function span(seconds: number, words: SharedWords = SHARED): string {
  const { units, amount, pair } = words;
  const s = Math.max(0, Math.round(seconds));

  if (s < 60) {
    return amount(s, units.s);
  }

  const minutes = Math.floor(s / 60);

  if (minutes < 60) {
    return amount(minutes, units.min);
  }

  const hours = Math.floor(minutes / 60);

  if (hours < 48) {
    const rest = minutes % 60;
    return rest > 0 && hours < 10 ? pair(amount(hours, units.h), amount(rest, units.min)) : amount(hours, units.h);
  }

  const days = Math.floor(hours / 24);
  const rest = hours % 24;

  return rest > 0 && days < 10 ? pair(amount(days, units.d), amount(rest, units.h)) : amount(days, units.d);
}

/** How long ago, or how long from now, in the same words. */
export function ago(when: string | Date | null | undefined, words: SharedWords = SHARED, now = Date.now()): string {
  if (!when) {
    return words.never;
  }

  const at = typeof when === 'string' ? new Date(when).getTime() : when.getTime();
  const seconds = (now - at) / 1000;

  if (Math.abs(seconds) < 10) {
    return words.justNow;
  }

  return seconds > 0 ? words.ago(span(seconds, words)) : words.in(span(-seconds, words));
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
export function origin(value?: string | null, words: SharedWords = SHARED): string {
  return (value && words.origins[value]) || words.origins.unknown;
}

/** The words for why something stopped being online. */
export function ending(value?: string | null, words: SharedWords = SHARED): string {
  return (value && words.endings[value]) || words.endings.ended;
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
