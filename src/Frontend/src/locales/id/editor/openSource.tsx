import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Memuat…',
  loadFailed: 'Status publikasi kodenya gagal dibaca.',
  hint: (tool) => (
    <>
      Lambda yang dipublikasikan bisa dibaca, diberi bintang, dan diunduh oleh siapa pun di halamannya di bagian Open
      Source - setiap versinya, dengan lisensi yang Anda pilih, dan tidak pernah dengan data yang disimpannya. Hanya
      pemegang kunci editor yang bisa memublikasikannya atau menghentikan publikasinya. Agen juga bisa melakukannya
      dengan tool {tool}.
    </>
  ),
  hintSimple:
    'Siapa pun bisa membaca bagaimana aplikasi Anda dibuat di halamannya sendiri, dan mengembangkannya lebih lanjut dengan lisensi yang Anda pilih - tetapi tidak pernah dengan apa yang disimpannya. Hanya Anda yang bisa memublikasikannya atau menghentikan publikasinya.',
  open: 'Buka halaman kode sumber',
  switch: 'Publikasikan kode aplikasi ini',
  publishedNow: (license) => `Dipublikasikan dengan lisensi ${license}. Siapa pun bisa membaca dan mengunduhnya.`,
  off: 'Nonaktif. Tidak ada yang bisa melihat kodenya sampai Anda memublikasikannya.',
  keptStars: (stars) => `${stars} bintang yang diterimanya tetap disimpan untuk saat Anda memublikasikannya lagi.`,
  published: 'Sudah dipublikasikan. Sekarang siapa pun bisa membaca kodenya.',
  saved: 'Tersimpan.',
  saveFailed: 'Kode gagal dipublikasikan.',
  withdrawn: 'Publikasi dihentikan. Halamannya sudah tidak ada.',
  withdrawFailed: 'Publikasi gagal dihentikan.',
  whatTitle: 'Yang dipublikasikan',
  what: [
    'Kodenya - seperti sekarang, dan setiap keadaan sebelumnya',
    'Semua yang ditampilkannya: halaman, gaya tampilan, dan gambarnya',
    'Apa yang ditulis tentangnya: untuk apa aplikasinya, dan bagaimana diuji',
    'Setiap perubahan yang dialaminya, masing-masing dalam satu baris',
  ],
  neverTitle: 'Yang tidak pernah dipublikasikan',
  never: [
    'Datanya: catatannya, file yang disimpannya, serta kunci dan kata sandinya',
    'Apa yang Anda minta, dengan kata-kata Anda sendiri',
    'Siapa yang memakainya: pengunjungnya dan apa yang mereka lakukan',
    'Link editor',
  ],
  careful:
    'Semua yang ada di kode menjadi publik, termasuk keadaan sebelumnya. Kata sandi atau kunci tidak pernah boleh ada di kode - tempatnya di kunci dan kata sandi di bagian Data, yang tidak pernah dipublikasikan.',
  licenseLabel: 'Lisensi',
  licenseHint:
    'Apa yang boleh dilakukan orang lain dengan kodenya. MIT, yang paling umum, membolehkan siapa pun melakukan hampir apa saja dengannya selama nama Anda tetap tercantum.',
  readLicense: 'Baca lisensinya',
  authorLabel: 'Nama di lisensi',
  optional: 'opsional',
  authorPlaceholder: (key) => `Para penulis ${key}`,
  authorHint:
    'Nama Anda atau nama organisasi Anda, ditampilkan di halaman kode sumber dan di lisensi. Jika dikosongkan, lisensi menyebut para penulis aplikasi ini.',
  publish: 'Publikasikan',
  save: 'Simpan perubahan',
  allSaved: 'Semua sudah tersimpan.',
  takeDown: 'Hentikan publikasi',
  confirm: 'Hentikan publikasi kodenya?',
  confirmText:
    'Halaman dan unduhannya langsung hilang. Siapa pun yang sudah mengunduhnya tetap memilikinya dengan lisensi yang menyertainya. Bintangnya tetap disimpan untuk saat Anda memublikasikannya lagi.',
  keep: 'Tetap publikasikan',
  stars: (count) => `${count} bintang`,
  sidebar: 'Kode sumber',
};
