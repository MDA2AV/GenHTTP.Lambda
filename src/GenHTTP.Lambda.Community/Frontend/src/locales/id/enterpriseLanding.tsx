import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Aplikasi untuk pekerjaan Anda',
  headline: 'Jelaskan aplikasi yang dibutuhkan pekerjaan Anda.',
  headlineAccent: 'Agen AI membangunnya dan menjadikannya online di sini.',
  intro:
    'Formulir, kalender pemesanan, daftar untuk tim Anda: sampaikan dengan kata-kata Anda sendiri apa yang harus dilakukannya. Dalam hitungan menit Anda mendapat link untuk dibagikan kepada rekan kerja - dan Anda mengubah aplikasinya nanti dengan cara yang sama, tanpa menulis kode apa pun.',
  describe: 'Jelaskan sebuah aplikasi',
  own: 'Gunakan agen AI Anda sendiri',

  examplesTitle: 'Apa yang dibangun orang di sini',
  examples: [
    { title: 'Formulir dan permohonan', text: 'Cuti, pembelian, masukan: orang mengisinya, dan pihak yang memutuskan menyetujuinya.' },
    { title: 'Pemesanan', text: 'Ruang rapat, meja kerja, peralatan: siapa memakai apa, dan kapan.' },
    { title: 'Daftar dan pelacak', text: 'Permintaan dukungan, inventaris, onboarding: apa yang masih terbuka, dan siapa yang menanganinya.' },
    { title: 'Halaman untuk tim', text: 'Jadwal, buku panduan, berita: satu link yang dibuka semua orang.' },
  ],

  stepsTitle: 'Cara kerjanya',
  steps: [
    {
      title: 'Jelaskan',
      text: 'Sampaikan apa yang harus dilakukan aplikasi, seperti Anda menjelaskannya kepada rekan kerja. Ide-ide di bawah kotak adalah titik awal.',
    },
    {
      title: 'Bagikan link-nya',
      text: 'Beberapa menit kemudian aplikasinya online di sini. Kirim link-nya kepada orang yang akan memakainya, dan simpan link editor Anda: itulah cara Anda mengubah aplikasi.',
    },
    {
      title: 'Minta lebih banyak',
      text: 'Di kotak yang sama: sesuatu yang baru, sesuatu yang berbeda, atau bagaimana kabarnya - siapa yang memakainya dan apa yang dimasukkan. Versi sebelumnya disimpan, sehingga versi sebelumnya bisa dijadikan online kembali.',
    },
  ],

  questionsTitle: 'Perlu diketahui',
  questions: (showcase) => [
    ['Apakah saya perlu bisa membuat kode?', 'Tidak. Anda menjelaskan apa yang harus dilakukan aplikasi, dan agen yang menulisnya. Anda tidak perlu melihat kodenya.'],
    ['Siapa yang bisa membuka aplikasi saya?', 'Siapa pun yang memiliki link-nya dan dapat menjangkau situs ini. Kirim link-nya kepada orang yang seharusnya memakainya.'],
    ['Di mana data yang dimasukkan orang disimpan?', 'Setiap aplikasi menyimpan catatannya sendiri di instalasi ini, yang dijalankan oleh organisasi Anda.'],
    [
      'Bagaimana cara kembali ke aplikasi saya?',
      'Browser ini mengingat aplikasi yang Anda bangun di sini, di samping kotak tempat Anda membangunnya. Di perangkat lain, buka link editor yang Anda terima bersama aplikasi - simpan di tempat yang aman, karena tidak ada yang dapat memulihkannya.',
    ],
    [
      'Ada yang tidak berfungsi. Apa yang harus dilakukan?',
      'Beri tahu agen apa yang salah, dengan cara yang sama seperti saat Anda meminta aplikasinya. Jika suatu perubahan membuat keadaan lebih buruk, jadikan versi sebelumnya online kembali di editor aplikasi.',
    ],
    ['Apa yang telah dibangun rekan kerja?', <>Aplikasi yang mereka pilih untuk ditampilkan ada di {showcase('Showcase')}.</>],
  ],
};
