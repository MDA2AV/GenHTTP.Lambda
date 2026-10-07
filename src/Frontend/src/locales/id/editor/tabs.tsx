import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Nama file C# terdiri dari huruf, angka, tanda hubung, garis bawah, dan titik, diawali huruf dan diakhiri .cs, paling banyak 40 karakter.',
  name: 'Huruf, angka, dan - _ . + @ ( ) [ ] { } $ ~, folder dipisahkan garis miring, tanpa spasi dan tanpa nama yang diakhiri titik.',
  taken: 'Lambda yang diekspor atau dikloning punya file atau folder dengan nama itu di bagian atas. Taruh di dalam folder, atau beri nama lain.',
  lambda: 'Lambda tidak lagi punya folder .lambda/: dokumentasinya ada di docs/, pengujiannya di tests/.',
  assets: 'Apa yang disajikan lambda kini ada di sumber dayanya - tambahkan di sana.',
  resourceName: 'Huruf, angka, tanda hubung, garis bawah, dan titik, dipisahkan garis miring, paling dalam enam folder - dan sebuah ekstensi, agar disajikan sebagai jenis yang benar.',
  exists: 'Sudah ada file dengan nama itu.',
  remove: (name) => `Hapus ${name}? Isinya ikut terhapus.`,
  removeFolder: (name, files) => `Hapus ${name} beserta ${files === 1 ? '1 file' : `${files} file`} di dalamnya?`,
  there: (name) => `${name} sudah ada.`,
  entry: 'Snippet utama: apa yang dikembalikannya, itulah yang disajikan',
  errors: 'ada error',
  removeFile: (name) => `Hapus ${name}`,
  removeTitle: 'Hapus',
  codePlaceholder: 'Store.cs, models/Item.cs atau docs/notes.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'Unggah file',
};
