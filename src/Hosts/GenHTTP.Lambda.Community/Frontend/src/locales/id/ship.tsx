import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hubungkan agen Anda',
  title: 'Publikasikan dari agen yang sudah Anda pakai.',
  intro:
    'Claude Code, Codex, Cursor, atau agen apa pun yang mendukung MCP mendapatkan instalasi ini sebagai alat: agen membuat aplikasi, mencoba perubahan di draf, menjadikannya online, dan membaca lognya. Setiap aplikasi dijalankan di alamatnya sendiri, dengan database-nya sendiri.',
  stepsTitle: 'Tiga langkah menuju aplikasi online',
  step: (n) => `Langkah ${n}`,
  steps: [
    {
      title: 'Hubungkan sekali saja',
      body: 'Tambahkan alamat server MCP instalasi ini ke agen Anda. Tidak perlu kunci: siapa pun yang memegang kunci editor sebuah aplikasi dapat mengubah aplikasi itu, dan tidak yang lain.',
    },
    {
      title: 'Minta agen membuat atau mempublikasikan',
      body: 'Jelaskan aplikasi baru, atau minta agen mempublikasikan aplikasi yang ada di proyek Anda. Agen menulis kodenya, memeriksa bahwa kodenya bisa dikompilasi, lalu menjadikannya online.',
    },
    {
      title: 'Simpan link editor',
      body: 'Anda mendapatkan alamat aplikasi dan link editornya. Alamat itu untuk semua orang; link editor adalah cara Anda - atau agen Anda - mengubah aplikasi nanti.',
    },
  ],
  sayLike: 'Lalu katakan, misalnya',
  asks: ['Buatkan buku tamu, dengan halaman tempat saya menyetujui isiannya.', 'Publikasikan aplikasi ini di sini dan kirimkan link-nya kepada saya.'],
  noAgent: 'Tidak punya agen? Pakai agen build',

  questionsTitle: 'Perlu diketahui',
  questions: (create) => [
    [
      'Agen apa saja yang bisa terhubung?',
      'Agen apa pun yang bisa menambahkan server MCP jarak jauh lewat HTTP: Claude Code, Claude, Codex, Cursor, VS Code, dan lainnya, di alamat di atas.',
    ],
    [
      'Bagaimana cara mengubah aplikasi nanti?',
      'Berikan link editor aplikasi itu kepada agen Anda bersama perubahan berikutnya, atau buka sendiri link-nya: editor menampilkan versi, kode, catatan, dan lognya. Perubahan dicoba di draf, dengan salinan data, sebelum menjadi online.',
    ],
    [
      'Bisakah saya mengerjakan aplikasi dengan git?',
      'Bisa. Setiap aplikasi adalah repositori git: lakukan kloning dari editornya, push commit ke main untuk menyimpan versi, atau push branch untuk memulai draf dengan pratinjaunya sendiri.',
    ],
    [
      'Ke mana kunci API dan kata sandi disimpan?',
      'Bukan di kode. Agen membacanya lewat nama, dan Anda memasukkan nilainya di editor, tempat tidak ada yang membacanya kembali - agen pun tidak.',
    ],
    [
      'Bisakah saya menulis aplikasi tanpa agen?',
      <>Bisa: {create('tulis kodenya sendiri')}, mulai dari salah satu demo. Aplikasi adalah handler GenHTTP berbahasa C#, yang dikompilasi di server ini.</>,
    ],
  ],
  profiles: {
    enterprise: {
      asks: [
        'Buatkan formulir pengajuan cuti, dengan halaman tempat atasan tim menyetujui setiap pengajuan.',
        'Publikasikan aplikasi ini di sini dan kirimkan link-nya kepada saya.',
      ],
    },
  },
};
