import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Log gagal dibaca.',
  hint: (capturing) =>
    'Request, apa yang dicetak lambda, dan apa yang error, secara langsung.' +
    (capturing ? '' : ' Instalasi ini tidak menyimpan apa yang dicetak lambda, jadi hanya request dan error yang muncul.') +
    ' Disimpan di memori dan dipakai bersama semua lambda di sini, jadi hanya mencakup beberapa menit sampai beberapa jam terakhir, dan kosong setelah restart. Alamat pengunjung tidak ditampilkan.',
  featureHint: (capturing) =>
    'Jawaban, apa yang dicetak, dan error dari pratinjau draf ini, secara langsung.' +
    (capturing ? '' : ' Instalasi ini tidak menyimpan apa yang dicetak lambda, jadi hanya request dan error yang muncul.') +
    ' Dipisahkan dari log lambda sendiri, yang tidak pernah menampilkan pratinjau. Disimpan di memori, jadi hanya mencakup beberapa menit sampai beberapa jam terakhir.',
  nothingPreview: 'Belum ada apa-apa. Buka pratinjau draf, dan request-nya akan muncul di sini.',
  search: 'Cari',
  searchLabel: 'Cari di log',
  resume: 'Tampilkan baris baru saat masuk',
  pause: 'Berhenti menambah baris baru selama Anda membaca',
  paused: 'Dijeda',
  live: 'Live',
  show: 'Tampilkan',
  all: 'Semua',
  requests: 'Request',
  output: 'Yang dicetak',
  problems: 'Masalah',
  reading: 'Membaca log…',
  noProblems: 'Tidak ada error yang masih tercatat di log.',
  nothing: 'Belum ada apa-apa. Buka alamat lambda, dan request-nya akan muncul di sini.',
  noMatch: 'Tidak ada yang cocok.',
  identical: (count) => `${count} baris identik`,
  at: (domain) => `, di ${domain}`,
  from: (country) => `, dari ${country}`,
};
