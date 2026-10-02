import type { EditorMessages } from '../../en/editor';
import { list } from './language';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Showcase gagal dimuat.',
  loading: 'Memuat…',
  title: 'judul',
  description: 'deskripsi',
  picture: 'gambar',
  updated: 'Entri showcase sudah diperbarui.',
  listed: 'Sekarang sudah tampil di halaman showcase.',
  waiting: 'Tersimpan. Akan tampil di halaman showcase begitu lambda online.',
  saveFailed: 'Entri showcase gagal disimpan.',
  removed: 'Sudah dihapus dari halaman showcase.',
  removeFailed: 'Entri showcase gagal dihapus.',
  wrongType: 'Itu bukan gambar PNG, JPEG, GIF, atau WebP.',
  tooLarge: (size, limit) => `Ukurannya ${size}, padahal gambar maksimal ${limit}.`,
  unreadable: 'File itu gagal dibaca.',
  hint: (tool) => (
    <>
      Halaman showcase menampilkan lambda yang dipamerkan pemiliknya, yang baru-baru ini dipakai lebih dulu. Hanya
      pemegang kunci editor yang bisa memasang lambda di sana atau menariknya lagi, dan lambda hanya tampil selama
      online. Agen juga bisa melakukannya dengan tool {tool}.
    </>
  ),
  open: 'Buka showcase',
  switch: 'Tampilkan lambda ini di halaman showcase',
  listedNow: 'Sudah tampil. Siapa pun yang melihat showcase bisa membukanya.',
  notListed: 'Tersimpan, tapi belum tampil: lambda sedang offline. Akan muncul lagi begitu di-deploy ulang.',
  off: 'Nonaktif. Tidak ada info tentang lambda ini yang ditampilkan di mana pun sampai Anda mengaktifkannya dan menyimpan.',
  offline: 'Lambda sedang offline, jadi entri ini menunggu sampai di-deploy. Hanya lambda yang merespons yang ditampilkan.',
  titleLabel: 'Judul',
  titlePlaceholder: 'Papan skor kuis kafe',
  descriptionLabel: 'Deskripsi',
  descriptionPlaceholder:
    'Tim mengisi jawaban di HP masing-masing, pembawa acara menilainya, dan papan skor langsung diperbarui untuk semua orang di ruangan.',
  save: 'Simpan perubahan',
  add: 'Tambahkan ke showcase',
  takeOff: 'Hapus dari showcase',
  needs: (missing) => `Masih perlu ${list(missing)}.`,
  tooLong: 'Ada yang terlalu panjang.',
  allSaved: 'Semua sudah tersimpan.',
  preview: 'Pratinjau',
  card: (address) => <>Ini kartu yang dilihat pengunjung. Kartu ini membuka {address}.</>,
  confirm: 'Hapus dari showcase?',
  keep: 'Tetap tampilkan',
  confirmText: 'Judul, deskripsi, dan gambarnya dihapus. Lambda-nya sendiri tetap persis seperti sekarang.',
  pictureLabel: 'Gambar',
  formats: (limit) => `PNG, JPEG, GIF, atau WebP, maksimal ${limit}`,
  notSaved: 'belum disimpan',
  replace: 'Seret gambar baru ke sini untuk menggantinya.',
  drop: 'Seret gambar ke sini.',
  advice: 'Screenshot, atau GIF pendek saat aplikasinya dipakai, paling bagus dengan rasio 16:10.',
  another: 'Pilih yang lain',
  choose: 'Pilih file',
  keepSaved: 'Pakai yang tersimpan',
  clear: 'Kosongkan',
};
