import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Saya ingin website yang…',
  shortcut: 'ctrl + enter',
  buildIt: 'Buat website saya',
  builtBy: 'Dibuat oleh',
  password: 'kata sandi',
  fable:
    'Fable masih dalam uji coba, jadi dilindungi kata sandi. Fable berjalan tanpa batas waktu. Ia terus bekerja sampai situs web Anda selesai, bukan sampai waktunya habis.',
  onlyNew:
    'Di sini Anda membuat website baru. Untuk mengubah website yang sudah ada, buka link editornya dan jelaskan di bagian “Ubah” apa yang perlu diganti.',
  ideas: [
    'website untuk komunitas kami tempat anggota mendaftar acara',
    'daftar bawaan untuk acara makan bersama kami, supaya tidak ada yang membawa masakan yang sama',
    'buku tamu untuk pernikahan kami',
    'polling tempat orang memilih dan melihat hasilnya',
    'papan peringkat untuk malam kuis mingguan kami',
    'halaman ulang tahun tempat teman-teman menulis ucapan',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Ada satu website sebelum milik Anda. Setelah itu giliran Anda.' : `Ada ${waiting} website sebelum milik Anda.`,
  starting: 'Memulai…',
  asked: 'Permintaan Anda',
  leaveOpen: 'Biarkan halaman ini tetap terbuka. Link untuk mengubah website Anda nanti hanya ditampilkan di sini setelah selesai.',
  log: 'Yang dikerjakan AI',
  online: 'Website Anda sudah online',
  notOnline: 'Website Anda sudah dibuat, tetapi belum online.',
  open: 'Buka website Anda',
  steps: {
    guide: 'Bersiap-siap',
    examples: 'Melihat contoh',
    create: 'Memilih alamat untuk website Anda',
    write: 'Menulis website Anda',
    improve: 'Menyempurnakan website Anda',
    check: 'Memeriksa kesalahan',
    online: 'Menjadikannya online',
    trying: 'Mencobanya',
    looking: 'Memeriksa website Anda',
    forRecords: 'Menyiapkan tempat untuk catatannya',
    forKeys: 'Menyiapkan tempat untuk kunci dan kata sandi',
    forFiles: 'Menyiapkan tempat untuk yang disimpannya',
    records: 'Melihat catatannya',
    keys: 'Memeriksa kunci dan kata sandi yang dibutuhkan',
    addFile: 'Menambahkan file',
    removeFile: 'Menghapus file',
    files: 'Melihat yang sudah disimpannya',
  },

  yourApp: 'Website Anda',
  further: 'Untuk mengubahnya nanti',
  keep:
    'Simpan baik-baik. Ini satu-satunya cara untuk masuk lagi, dan tidak bisa dipulihkan. Kami pun tidak bisa. Bookmark dulu sebelum menutup tab ini.',
  change:
    'Untuk mengubah website Anda, buka link editor dan jelaskan di bagian “Ubah” apa yang perlu diganti, sama seperti di sini. Asisten AI Anda sendiri juga bisa melakukannya, seperti dijelaskan di bawah.',
  copyLink: 'Salin link editor',
  lifetime: (offline, removed) =>
    `Kami menjaganya tetap online selama masih dipakai: setelah ${offline} hari tanpa kunjungan atau perubahan, website dimatikan, dan setelah ${removed} hari dihapus. Buka editor untuk menyalakannya lagi.`,
  openEditor: 'Buka editor',
  another: 'Buat website lain',

  keepGoing: 'Lanjutkan dengan asisten AI Anda sendiri',
  orOwn: 'Atau pakai asisten AI Anda sendiri',
  ownText:
    'Sudah memakai Claude atau asisten AI lain? Hubungkan di sini, dan asisten itu bisa membuat serta mengubah website untuk Anda dengan cara yang sama. Kami yang meng-hosting-nya, jadi tetap tidak ada yang perlu Anda atur. Tanpa batas harian.',
  ownTitle: 'Buat website dengan asisten AI Anda',
  ownOnly:
    'Hubungkan Claude atau asisten AI lain ke alamat di bawah, lalu ceritakan website yang Anda inginkan. Asisten membuatnya, kami meng-hosting-nya di server kami, dan website langsung online dengan link untuk dibagikan.',
  thenAsk:
    'Lalu sampaikan apa yang Anda mau, misalnya: “Buatkan website untuk paduan suara kami dengan kalender konser kami.”',
  howToChange:
    'Begitu juga cara mengubah website nanti: berikan link editor ke asisten Anda dan sampaikan apa yang perlu diganti.',

  failedToStart: 'Permintaan gagal dikirim.',
  noAnswer: 'Selesai, tapi tanpa keterangan apa yang terjadi.',
  failed: 'Tidak berhasil.',
};
