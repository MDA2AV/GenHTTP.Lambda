import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Huruf, angka, tanda hubung, dan garis bawah, diakhiri .cs',
  slashes: 'Tanpa garis miring di awal atau akhir, dan kurang dari 120 karakter.',
  deep: 'Maksimal enam level folder.',
  characters: 'Huruf, angka, tanda hubung, garis bawah, dan titik, dipisahkan garis miring.',
  extension: 'Perlu ekstensi, supaya bisa disajikan dengan tipe yang benar.',
  context: 'Di .lambda/, hanya docs/ dan tests/ - huruf, angka, tanda hubung, garis bawah, dan titik, dipisahkan garis miring.',
  contextFiles: 'Dokumentasi dan pengujian: bagian dari versi, tidak pernah dikompilasi atau disajikan',
  exists: 'Sudah ada file dengan nama itu.',
  remove: (name) => `Hapus ${name}? Isinya ikut terhapus.`,
  there: (name) => `${name} sudah ada.`,
  entry: 'Snippet utama: apa yang dikembalikannya, itulah yang disajikan',
  errors: 'ada error',
  removeFile: (name) => `Hapus ${name}`,
  removeTitle: 'Hapus file ini',
  placeholder: 'Types.cs, site/index.html, atau .lambda/docs/api.md',
  newFile: 'File baru',
  uploadTitle: 'Unggah file: gambar, font, atau halaman',
  upload: 'Unggah file',
};
