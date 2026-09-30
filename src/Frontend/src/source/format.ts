import { useLanguage } from '../i18n';
import { tagOf, type Language } from '../i18n/languages';

/*
 * Times and sizes as the pages of the published sources say them, in the
 * language of the page. The browser knows how every language says "3 days
 * ago" and "12 Sept 2026", so it is asked rather than taught.
 */

const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
  ['year', 365 * 24 * 3600],
  ['month', 30 * 24 * 3600],
  ['week', 7 * 24 * 3600],
  ['day', 24 * 3600],
  ['hour', 3600],
  ['minute', 60],
];

/** How long ago something was, as the language says it: "3 days ago", "vor 3 Tagen". */
export function ago(when: string, language: Language, now = Date.now()): string {
  const seconds = (new Date(when).getTime() - now) / 1000;

  const format = new Intl.RelativeTimeFormat(tagOf(language), { numeric: 'auto' });

  for (const [unit, size] of UNITS) {
    if (Math.abs(seconds) >= size) {
      return format.format(Math.round(seconds / size), unit);
    }
  }

  // "now" in every language, where "this minute" reads as "within a minute" in some
  return format.format(0, 'second');
}

/** The day something was, as the language writes it. */
export function day(when: string, language: Language): string {
  return new Intl.DateTimeFormat(tagOf(language), { dateStyle: 'medium' }).format(new Date(when));
}

/** The moment something was, to the minute, for a title on hover. */
export function moment(when: string, language: Language): string {
  return new Intl.DateTimeFormat(tagOf(language), { dateStyle: 'long', timeStyle: 'short' }).format(new Date(when));
}

/** A number as the language groups its digits. */
export function count(value: number, language: Language): string {
  return new Intl.NumberFormat(tagOf(language)).format(value);
}

/** A size in bytes, in the unit that reads best. */
export function size(bytes: number, language: Language): string {
  const format = (value: number, unit: string) =>
    `${new Intl.NumberFormat(tagOf(language), { maximumFractionDigits: value < 10 ? 1 : 0 }).format(value)} ${unit}`;

  if (bytes < 1024) {
    return format(bytes, 'B');
  }

  if (bytes < 1024 * 1024) {
    return format(bytes / 1024, 'KB');
  }

  return format(bytes / 1024 / 1024, 'MB');
}

/** The formatters above, bound to the language of the page. */
export function useFormat() {
  const language = useLanguage();

  return {
    language,
    ago: (when: string) => ago(when, language),
    day: (when: string) => day(when, language),
    moment: (when: string) => moment(when, language),
    count: (value: number) => count(value, language),
    size: (bytes: number) => size(bytes, language),
  };
}
