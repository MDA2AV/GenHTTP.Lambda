/** Un nombre et son nom, au singulier pour 0 et 1 comme on le dit en français. */
export const count = (value: number, one: string, many: string) => `${value} ${Math.abs(value) < 2 ? one : many}`;
