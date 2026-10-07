import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'Versi',
  shown: (version, online, newest) => `Versi ${version}${online ? ', online' : newest ? ', terbaru' : ''}`,
  optionOnline: ' (online)',
  count: (files) => `${files} file`,
  usage: (files, used, of) => `${files}, ${used} dari ${of}`,
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
