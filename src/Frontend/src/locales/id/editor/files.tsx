import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      File dari satu versi, yaitu programnya. {b('Kode')} dikompilasi dan tidak pernah disajikan. {b('Aset')}{' '}
      (halaman, script, style, gambar) disimpan bersama kode, ikut di-deploy dan di-rollback bersamanya, dan bersifat
      publik kalau kode menyajikannya. Apa yang disimpan lambda selama berjalan tidak ada di sini: itu ada di{' '}
      {b('Data')}.
    </>
  ),
  scope: (version, data) => (
    <>
      File ini milik versi {version} dan ikut berubah bersamanya. Apa yang disimpan lambda selama berjalan sama untuk
      semua versi, dan ada di {data('Data')}.
    </>
  ),
  edit: 'Edit versi ini',
  version: 'Versi',
  shown: (version, online, newest) => `Versi ${version}${online ? ', online' : newest ? ', terbaru' : ''}`,
  optionOnline: ' (online)',
  readFailed: 'Versi itu gagal dibaca.',
  noVersion: 'Belum ada versi untuk ditampilkan.',
  label: 'File',
  code: 'Kode',
  codeWhy: 'Dikompilasi ke dalam lambda, tidak pernah disajikan.',
  count: (files) => `${files} file`,
  codeUsage: (files, used, of) => `${files}, ${used} dari ${of} karakter`,
  usage: (files, used, of) => `${files}, ${used} dari ${of}`,
  noCode: 'Tidak ada kode di versi ini.',
  assets: 'Aset',
  assetsPublic: 'Publik: versi ini menyajikannya lewat Assets.',
  assetsPrivate: 'Disimpan bersama kode, tapi tidak disajikan oleh versi ini.',
  noAssets: 'Tidak ada di versi ini.',
  context: 'Dokumentasi dan pengujian',
  contextWhy: 'Tidak pernah dikompilasi dan tidak pernah disajikan: apa yang ditulis tentang versi ini, untuk siapa pun yang membaca atau mengubahnya.',
  contextUsage: (files, size) => `${files}, ${size} - dihitung bersama aset`,
  noContext: 'Belum ada yang ditulis tentang versi ini.',
  build: 'Build',
  buildWhy: 'Tidak pernah dikompilasi dan tidak pernah disajikan: bahan pembuat kode atau aset, bagi siapa pun yang mengubahnya.',
  data: 'Data',
  dataPublic: 'Publik: kode yang online menyajikannya lewat Workspace.',
  dataPrivate: 'Privat, hanya untuk lambda ini. Bukan bagian dari versi mana pun.',
  uploadFailed: (path) => `Gagal mengunggah ${path}.`,
  deleteFolder: (path, held) =>
    held > 0 ? `Hapus ${path} beserta ${held} file di dalamnya?` : `Hapus folder ${path}?`,
  deleteFile: (path) => `Hapus ${path}? Lambda tidak akan bisa menemukannya lagi.`,
  deleteFailed: 'Gagal dihapus.',
  full: 'Ruang data sudah penuh',
  uploadInto: (folder) => `Unggah ke ${folder}`,
  upload: 'Unggah',
  reading: 'Membaca…',
  noData: 'Belum ada apa-apa. Apa yang disimpan lambda selama berjalan akan muncul di sini.',
  delete: (path) => `Hapus ${path}`,
  deleteShort: 'Hapus',
  fileFailed: 'File gagal dibaca.',
  pick: 'Pilih file untuk melihat isinya.',
  tooLarge: (name, size) => (
    <>
      {name} berukuran {size}, terlalu besar untuk ditampilkan di sini.
    </>
  ),
  download: 'Unduh',
  readingFile: (name) => `Membaca ${name}…`,
  missing: (name) => `Versi ini tidak punya file bernama ${name}.`,
  saved: 'tersimpan',
  notText: 'Bukan teks. Unduh untuk melihat isinya.',
};
