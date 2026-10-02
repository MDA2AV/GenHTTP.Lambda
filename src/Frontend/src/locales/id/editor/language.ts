/** Daftar dengan koma dan "dan" sebelum yang terakhir: "judul, deskripsi, dan gambar". */
export const list = (items: string[]) =>
  items.length > 2 ? `${items.slice(0, -1).join(', ')}, dan ${items[items.length - 1]}` : items.join(' dan ');
