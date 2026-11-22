import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Saya ingin aplikasi yang…',
  password: 'kata sandi',
  unlimited:
    'Model ini memerlukan kata sandi, lalu berjalan tanpa batas waktu: terus bekerja sampai aplikasi Anda selesai, bukan sampai waktunya habis.',
  ideas: [
    'aplikasi untuk komunitas kami tempat anggota mendaftar acara',
    'daftar bawaan untuk acara makan bersama kami, supaya tidak ada yang membawa masakan yang sama',
    'buku tamu untuk pernikahan kami',
    'polling tempat orang memilih dan melihat hasilnya',
    'papan peringkat untuk malam kuis mingguan kami',
    'halaman ulang tahun tempat teman-teman menulis ucapan',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Ada satu aplikasi sebelum milik Anda. Setelah itu giliran Anda.' : `Ada ${waiting} aplikasi sebelum milik Anda.`,
  starting: 'Memulai…',
  online: 'Aplikasi Anda sudah online',
  notOnline: 'Aplikasi Anda sudah dibuat, tetapi belum online.',
  open: 'Buka aplikasi Anda',
  steps: {
    guide: 'Bersiap-siap',
    examples: 'Melihat contoh',
    create: 'Memilih alamat untuk aplikasi Anda',
    write: 'Menulis aplikasi Anda',
    improve: 'Menyempurnakan aplikasi Anda',
    check: 'Memeriksa kesalahan',
    online: 'Menjadikannya online',
    trying: 'Mencobanya',
    looking: 'Memeriksa aplikasi Anda',
    forRecords: 'Menyiapkan tempat untuk catatannya',
    forKeys: 'Menyiapkan tempat untuk kunci dan kata sandi',
    forFiles: 'Menyiapkan tempat untuk yang disimpannya',
    records: 'Melihat catatannya',
    keys: 'Memeriksa kunci dan kata sandi yang dibutuhkan',
    addFile: 'Menambahkan file',
    removeFile: 'Menghapus file',
    files: 'Melihat yang sudah disimpannya',
    services: 'Meminta izin untuk menggunakan layanan lain',
    site: (site) => `Melihat ${site}`,
  },
  further: 'Link editor Anda',
  keep:
    'Simpan link itu di tempat yang aman. Itu satu-satunya cara untuk masuk ke aplikasi Anda dari perangkat lain, dan tidak bisa dipulihkan. Kami pun tidak bisa.',
  copyLink: 'Salin link editor',
  lifetime: (offline, removed) =>
    `Kami menjaganya tetap online selama masih dipakai: setelah ${offline} hari tanpa kunjungan atau perubahan, aplikasi dimatikan, dan setelah ${removed} hari dihapus. Anda bisa menjadikannya online lagi di sini, atau di editornya.`,

  failedToStart: 'Permintaan gagal dikirim.',
  noAnswer: 'Selesai, tapi tanpa keterangan apa yang terjadi.',
  failed: 'Tidak berhasil.',
  profiles: {
    enterprise: {
      placeholder: 'Kami membutuhkan aplikasi yang…',
      ideas: [
        'formulir pengajuan cuti, disetujui oleh atasan tim',
        'kalender pemesanan untuk ruang rapat kami',
        'daftar periksa untuk menyambut rekan kerja baru',
        'inventaris peralatan kami dan siapa yang memegangnya',
        'daftar permintaan dukungan yang masih terbuka dan siapa yang menanganinya',
        'formulir umpan balik untuk sesi pelatihan kami',
      ],
    },
  },
};
