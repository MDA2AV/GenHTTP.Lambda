/** Een aantal met het juiste woord: "1 bestand", "3 bestanden". */
export const many = (count: number, one: string, more: string) => (count === 1 ? `1 ${one}` : `${count} ${more}`);
