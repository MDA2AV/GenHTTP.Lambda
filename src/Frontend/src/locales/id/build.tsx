import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Dari ide jadi website.',
  intro:
    'Ceritakan website atau aplikasi yang Anda bayangkan. AI membuatkannya untuk Anda, kami meng-hosting-nya di server kami, dan website langsung online, lengkap dengan link yang bisa Anda kirim ke siapa saja. Tanpa coding, tanpa repot mengatur hosting, tanpa akun.',
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
    'buku tamu untuk pernikahan kami',
    'polling tempat orang memilih dan melihat hasilnya',
    'papan skor untuk malam kuis mingguan kami',
    'hitung mundur menuju hari pembukaan kami yang bisa dilihat semua orang',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Ada satu website sebelum milik Anda. Setelah itu giliran Anda.' : `Ada ${waiting} website sebelum milik Anda.`,
  starting: 'Memulai…',

  points: [
    {
      title: 'Cukup diceritakan, tanpa coding',
      text: 'Jelaskan dengan kata-kata Anda sendiri apa yang harus dilakukan website Anda. Tidak perlu bisa coding atau paham teknis.',
    },
    {
      title: 'Sudah termasuk hosting',
      text: 'Website Anda berjalan di server kami. Hosting, keamanan, dan pembaruan kami yang urus. Tidak ada yang perlu Anda atur atau rawat.',
    },
    {
      title: 'Online dalam hitungan menit',
      text: 'Anda langsung mendapat link untuk dibagikan. Website juga bisa menyimpan data, seperti pendaftaran, suara, atau skor, jadi semua orang melihat hal yang sama.',
    },
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
