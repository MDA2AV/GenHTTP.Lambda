import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Buat website dengan AI.',
  intro:
    'Ceritakan website atau aplikasi yang Anda bayangkan dengan kata-kata Anda sendiri. AI membuatkannya untuk Anda, kami yang meng-hosting-nya, dan website online dalam hitungan menit, lengkap dengan link yang bisa Anda kirim ke siapa saja. Gratis, tanpa coding, tanpa daftar.',
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

};
