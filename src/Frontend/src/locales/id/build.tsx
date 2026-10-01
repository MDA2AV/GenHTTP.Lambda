import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Buat website dengan AI.',
  intro:
    'Ceritakan website atau aplikasi yang Anda bayangkan dengan kata-kata Anda sendiri. AI membuatkannya untuk Anda, kami yang meng-hosting-nya, dan website online dalam hitungan menit, lengkap dengan link yang bisa Anda kirim ke siapa saja. Gratis, tanpa coding, tanpa daftar.',
  placeholder: 'Saya ingin website yang…',
  working: 'sedang dikerjakan…',
  shortcut: 'ctrl + enter',
  building: 'Sedang dibuat',
  buildIt: 'Buat website saya',
  builtBy: 'Dibuat oleh',
  password: 'kata sandi',
  fable:
    'Fable masih dalam uji coba, jadi dilindungi kata sandi. Fable berjalan tanpa batas waktu. Ia terus bekerja sampai aplikasinya selesai, bukan sampai waktunya habis.',
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

  points: [
    {
      title: 'Tanpa coding, cukup diceritakan',
      text: 'Jelaskan dengan kata-kata Anda sendiri apa yang harus dilakukan website Anda, seperti bercerita ke teman. AI yang membuatkannya, tanpa perlu paham teknis.',
    },
    {
      title: 'Sudah termasuk hosting gratis',
      text: 'Website Anda berjalan di server kami. Tidak perlu beli paket hosting, server, atau domain, dan tidak ada yang perlu diinstal. Keamanan dan pembaruan kami yang urus.',
    },
    {
      title: 'Online dalam hitungan menit',
      text: 'Anda langsung mendapat link untuk dibagikan. Website menyimpan apa yang diisi orang, seperti pendaftaran, suara, pesan, atau skor, jadi semua orang melihat hal yang sama.',
    },
  ],

  questionsTitle: 'Sebelum mulai',
  questions: (offline, removed) => [
    [
      'Apakah AI benar-benar bisa membuat website gratis untuk saya?',
      `Ya. Ceritakan dengan kata-kata Anda sendiri, lalu AI membuatnya, menjadikannya online, dan memberi Anda link-nya. Tanpa daftar, tanpa kartu kredit, tanpa masa percobaan. Website tetap online selama masih dipakai: setelah ${offline} hari tanpa kunjungan atau perubahan, website dimatikan, dan setelah ${removed} hari dihapus.`,
    ],
    [
      'Apakah bisa membuat website tanpa hosting, server, atau domain?',
      'Bisa. Website Anda berjalan di server kami, sudah termasuk hosting, keamanan, dan pembaruan. Anda langsung mendapat link, jadi tidak perlu beli domain juga.',
    ],
    [
      'Bisakah membuat aplikasi tanpa coding?',
      'Bisa. Anda tidak akan melihat kode sama sekali. Ceritakan apa yang harus dilakukan, seperti bercerita ke teman, dan AI yang mengerjakan sisanya: website, aplikasi kecil, atau game.',
    ],
    [
      'Bisakah orang mendaftar, memberi suara, atau mengirim pesan?',
      'Bisa. Website Anda menyimpan apa yang diisi orang, jadi semua orang yang membuka link-nya melihat pendaftaran, suara, dan skor yang sama.',
    ],
    [
      'Bagaimana orang lain membukanya?',
      'Lewat link-nya, di browser apa pun, di ponsel atau komputer. Tidak ada yang perlu diinstal, dan tidak perlu lewat app store.',
    ],
    [
      'Bagaimana cara mengubahnya nanti?',
      'Buka link editor yang Anda dapat bersama website Anda, lalu jelaskan apa yang perlu diganti, sama seperti di sini. Kalau tidak suka dengan perubahannya, Anda bisa mengembalikannya seperti semula.',
    ],
  ],

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
