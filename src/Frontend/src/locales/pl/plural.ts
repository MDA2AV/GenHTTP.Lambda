/**
 * Polish plurals: one form for 1, one for 2-4 (22-24, 32-34 and so on, but
 * not 12-14), and one for everything else (0, 5-21, 25-31 …).
 *
 * Only for words drawn in the browser, like the editor's: a prerendered page
 * has placeholders in place of some numbers, and must not choose its words by
 * them.
 */
export function plural(count: number, one: string, few: string, many: string): string {
  const n = Math.abs(count);

  if (n === 1) {
    return one;
  }

  const units = n % 10;
  const tens = n % 100;

  return units >= 2 && units <= 4 && (tens < 12 || tens > 14) ? few : many;
}

/** The number with its noun: "1 plik", "3 pliki", "12 plików". */
export function counted(count: number, one: string, few: string, many: string): string {
  return `${count} ${plural(count, one, few, many)}`;
}
