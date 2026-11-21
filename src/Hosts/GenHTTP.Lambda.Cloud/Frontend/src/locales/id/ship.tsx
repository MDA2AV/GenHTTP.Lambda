import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hosting gratis untuk vibe coding',
  title: 'Dari localhost ke layar semua orang.',
  intro:
    'Anda sudah membuat aplikasi dengan Claude Code, Codex, atau Cursor, tapi baru bisa jalan di komputer Anda sendiri. Minta agen Anda men-deploy-nya di sini. Beberapa menit kemudian, aplikasinya punya link publik untuk siapa saja, database sendiri, dan koneksi live ke semua orang yang sedang membukanya, jadi orang bisa main, chat, dan posting bersama di dalamnya.',
  facts: ['Gratis', 'Tanpa daftar', 'Tanpa kartu kredit', 'Tanpa instalasi'],

  stepsTitle: 'Tiga langkah dari localhost ke link publik',
  step: (n) => `Langkah ${n}`,
  steps: [
    {
      title: 'Hubungkan sekali',
      body: 'Tambahkan satu alamat, yaitu MCP server remote, ke Claude Code, Codex, Cursor, atau agen apa pun yang Anda pakai. Kurang dari semenit, dan cukup sekali saja.',
    },
    {
      title: 'Minta dipublikasikan',
      body: 'Minta agen Anda membuat aplikasinya online di sini. Agen akan mengemasnya, men-deploy-nya, lalu mengecek apakah aplikasinya sudah merespons. Tanpa repo GitHub, tanpa pipeline deploy, tanpa Docker.',
    },
    {
      title: 'Bagikan link-nya',
      body: 'Anda mendapat alamat publik dan link editor privat. Kirim yang pertama ke siapa saja. Simpan yang kedua, karena lewat link itulah Anda bisa mengubah aplikasinya nanti.',
    },
  ],

  togetherTitle: 'Bukan sekadar hosting. Database dan multiplayer sudah termasuk.',
  together:
    'Kebanyakan hosting memberi setiap pengunjung salinan aplikasi sendiri-sendiri, jadi semua orang main sendirian: apa yang disimpan satu browser di localStorage tidak pernah terlihat oleh browser lain. Di sini, setiap aplikasi punya database sendiri dan koneksi live ke semua orang yang sedang membukanya. Apa yang dilakukan satu orang langsung terlihat oleh yang lain, dan apa yang mereka posting masih ada besok.',
  sayLike: 'Lalu, di proyek Anda, tinggal bilang seperti ini',
  asks: [
    'Publikasikan aplikasi ini di GenHTTP Lambda, lalu kirim link-nya ke saya.',
    'Buat skor tertingginya tersimpan bersama, jadi semua orang melihat papan peringkat yang sama.',
  ],

  questionsTitle: 'Sebelum men-deploy',
  questions: (offline, removed, showcase, terms, selfHosting) => [
    [
      'Benar-benar gratis?',
      <>
        Ya. Tanpa daftar, tanpa kartu kredit, tanpa masa percobaan. Aplikasi Anda tetap online selama masih dipakai. Setelah{' '}
        {offline} hari tanpa satu pun kunjungan atau perubahan, aplikasinya dimatikan, dan setelah {removed} hari
        dihapus.
      </>,
    ],
    [
      'Bisakah Claude Code, Codex, atau Cursor men-deploy aplikasi saya di sini?',
      'Bisa, begitu juga agen lain yang bisa menambahkan MCP server remote. Hubungkan sekali dengan alamat di atas, lalu minta agen memublikasikannya: agen akan men-deploy aplikasinya, mengecek apakah sudah merespons, dan mengirim link-nya ke Anda.',
    ],
    [
      'Teman saya tidak bisa membuka link localhost saya. Bagaimana cara mengonlinekannya?',
      'Localhost adalah komputer Anda sendiri: alamat itu hanya berfungsi di sana, dan hanya selama aplikasinya berjalan. Tunnel meminjamkan alamat publik selama laptop Anda menyala. Kalau di-deploy di sini, aplikasinya berjalan di server kami, dengan link yang tetap bisa dibuka saat laptop Anda tertutup.',
    ],
    [
      'Apakah saya butuh server, backend, atau Supabase?',
      'Tidak. Setiap aplikasi punya database sendiri, penyimpanan file, dan koneksi live ke semua orang yang sedang membukanya. Tidak ada server yang perlu disewa, tidak ada layanan kedua yang perlu disiapkan, dan tidak ada yang perlu tetap berjalan di sisi Anda.',
    ],
    [
      'Bisakah game saya jadi multiplayer tanpa menjalankan server sendiri?',
      'Bisa. Apa yang disimpan satu browser di localStorage tidak pernah terlihat oleh browser lain, jadi bagian yang dipakai bersama harus ada di server, dan di sini server-nya milik kami. Minta agen Anda membuat game-nya multiplayer, maka setiap langkah sampai ke semua orang yang sedang membukanya.',
    ],
    [
      'Apakah aplikasi saya harus dibuat dengan cara tertentu?',
      'Tidak, itu urusan agen Anda. Halaman, gambar, dan style diunggah apa adanya, dan apa pun yang harus berjalan di server akan disesuaikan agen untuk platform ini. Anda cukup menjelaskan apa yang harus dilakukan aplikasinya. Agen yang menerjemahkannya.',
    ],
    [
      'Bagaimana cara mengubahnya nanti?',
      'Dengan link editor yang Anda dapat saat aplikasinya dipublikasikan. Berikan ke agen Anda bersama perubahan berikutnya, atau buka di browser. Setiap perubahan jadi versi baru di alamat yang sama, dan Anda bisa kembali ke versi lama kapan saja.',
    ],
    [
      'Bisakah saya melakukan push ke sini dengan git?',
      'Bisa. Setiap aplikasi juga merupakan repositori git: kloning dari alamat di bawah Kloning di editor, ubah dengan alat atau agen Anda sendiri, lalu push. Setiap commit yang di-push ke main menjadi versi berikutnya, dan branch yang Anda push menjadi draf dengan pratinjau sendiri.',
    ],
    [
      'Di mana API key saya disimpan?',
      'Tidak di dalam kode. Agen Anda meminta key berdasarkan namanya, lalu Anda mengetik nilainya di editor. Tidak ada yang bisa membacanya lagi, baik editor maupun agen.',
    ],
    [
      'Bisakah saya membawa kode saya?',
      'Bisa, kodenya milik Anda. Unduh dari editor kapan saja, sebagai proyek yang bisa berjalan sendiri, lengkap dengan database-nya - atau kloning dengan git, lengkap dengan setiap versinya.',
    ],
    [
      'Bisakah saya menjalankannya di server saya sendiri?',
      <>
        Bisa. GenHTTP Lambda adalah open source, dan edisi Community-nya adalah platform yang sama dalam satu container Docker, untuk
        server dan agen Anda sendiri. {selfHosting('Cloud atau self-hosted')} menjelaskan apa bedanya.
      </>,
    ],
    [
      'Siapa yang bisa melihat aplikasi saya?',
      <>
        Siapa saja yang Anda beri link-nya. Aplikasi Anda tidak tercantum di mana pun, kecuali Anda sendiri menambahkannya
        ke {showcase('showcase')}.
      </>,
    ],
    [
      'Ada yang tidak boleh dipublikasikan?',
      <>
        Ada beberapa, misalnya apa pun yang merugikan atau menipu orang. {terms('Ketentuannya')} singkat dan pakai bahasa
        sehari-hari.
      </>,
    ],
  ],
  noAgent: 'Belum punya agen? Buat di sini',

};
