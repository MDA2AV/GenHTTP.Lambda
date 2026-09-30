import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navigasi utama',
  build: 'Buat website',
  ship: 'Publikasikan',
  showcase: 'Showcase',
  enterprise: 'Enterprise',
  docs: 'Panduan',
  admin: 'Admin',
  lightMode: 'Ganti ke mode terang',
  darkMode: 'Ganti ke mode gelap',
  openMenu: 'Buka menu',
  closeMenu: 'Tutup menu',
  language: 'Bahasa',
  terms: 'Ketentuan layanan',
  privacy: 'Kebijakan privasi',
  imprint: 'Informasi hukum',
  writeCode: 'Tulis kodenya sendiri',
  contact: 'Kontak',
};

export const common: Messages['common'] = {
  loading: 'Memuat…',
  loadingEditor: 'Memuat editor…',
  editorFailed: 'Editor gagal dimuat',
  pageFailed: 'Halaman gagal dimuat',
  editorFailedWhy: 'Biasanya ini terjadi karena situs diperbarui saat tab ini masih terbuka.',
  reload: 'Muat ulang halaman',
  backToStart: 'Kembali ke beranda',
  tryAgain: 'Coba lagi',
  copy: 'Salin',
  copied: 'Disalin',
  copyToClipboard: 'Salin ke papan klip',
  openInNewTab: 'Buka di tab baru',
  close: 'Tutup',
  operatorCountry: 'Jerman',
};

export const notFound: Messages['notFound'] = {
  title: 'Halaman tidak ditemukan',
  heading: 'Halaman ini tidak ada',
  text: 'Link ini mungkin sudah tidak berlaku, atau lambda yang dituju sudah dihapus.',
};

export const missing: Messages['missing'] = {
  title: 'Tidak ada yang berjalan di sini',
  heading: 'Tidak ada yang berjalan di sini',
  notDeployed: (key) => (
    <>
      Ada lambda di {key}, tapi saat ini sedang tidak di-deploy. Di paket gratis, deployment tetap online selama
      dipakai, dan dimatikan setelah sebulan tanpa kunjungan atau perubahan. Siapa pun yang memegang link editor bisa
      membuatnya online lagi.
    </>
  ),
  unknown: (key) => (
    <>
      Tidak ada lambda di {key}. Mungkin kunci ini tidak pernah ada, atau lambda di baliknya sudah dihapus.
    </>
  ),
  create: 'Buat lambda di sini',
};

export const abuse: Messages['abuse'] = {
  report: 'Laporkan penyalahgunaan',
  title: 'Laporkan lambda',
  write: 'Kirim email ke kami',
  subject: 'Laporan penyalahgunaan',
  intro:
    'Siapa pun bisa menaruh kode online di sini. Artinya, kadang ada yang memasang sesuatu yang tidak semestinya. Kalau ada halaman di sini yang menipu orang, menyerang sesuatu, atau memakai materi tanpa hak, beri tahu kami, dan kami akan menurunkannya.',
  how: (mailbox, strong, path) => (
    <>
      Kirim email ke {mailbox} dan sertakan {strong('alamat halamannya')} (bentuknya seperti {path}), plus satu kalimat
      tentang masalahnya. Screenshot akan membantu. Anda tidak perlu akun, dan tidak harus jadi pengguna situs ini.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Selanjutnya.')} Laporan Anda dibaca langsung oleh manusia. Kalau lambda itu melanggar{' '}
      {terms('ketentuan layanan')}, lambda itu kami matikan, biasanya dalam sehari. Kami tidak akan memberi tahu
      siapa yang memasangnya, dan kami tidak bisa berjanji membalas setiap laporan. Tapi semuanya kami baca.
    </>
  ),
  danger:
    'Kalau ada orang dalam bahaya, atau sedang terjadi tindak kejahatan, hubungi juga pihak berwajib setempat. Kami bisa menghapus halaman, tapi tidak lebih dari itu.',
};
