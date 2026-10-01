import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hosting gratis untuk vibe coding',
  title: 'Dari localhost ke layar semua orang.',
  intro:
    'Anda sudah membuat aplikasi dengan Claude Code, Codex, atau Cursor, tapi baru bisa jalan di komputer Anda sendiri. Minta agen Anda men-deploy-nya di sini. Beberapa menit kemudian, aplikasinya punya link publik untuk siapa saja, database sendiri, dan koneksi live ke semua orang yang sedang membukanya, jadi orang bisa main, chat, dan posting bersama di dalamnya.',
  facts: ['Gratis', 'Tanpa daftar', 'Tanpa kartu kredit', 'Tanpa instalasi'],
  connect: 'Hubungkan agen Anda',
  seeOthers: 'Lihat karya orang lain',

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
  together2:
    'Tidak perlu daftar Supabase atau Firebase, tidak perlu menyambungkan backend, tidak perlu sewa server. Minta saja, seperti Anda menjelaskannya ke teman.',
  kinds: [
    { name: 'Game multiplayer', ask: 'Buat supaya maksimal delapan teman bisa ikut ronde yang sama dan melihat langkah masing-masing secara live.' },
    { name: 'Ruang chat', ask: 'Tambahkan ruang chat untuk semua orang yang punya link-nya, dan simpan seratus pesan terakhir.' },
    { name: 'Daftar bersama', ask: 'Ubah daftar barang bawaan jadi daftar yang bisa diedit seluruh tim sekaligus.' },
    { name: 'Papan peringkat', ask: 'Buat papan peringkat berisi waktu terbaik tiap orang, dan tampilkan sepuluh besar di layar awal.' },
    { name: 'Jejaring sosial mini', ask: 'Buat supaya tamu pernikahan bisa posting foto di satu dinding dan saling kasih like.' },
  ],
  quote: (text) => `“${text}”`,

  connectTitle: 'Hubungkan Claude Code, Codex, atau Cursor, sekali saja',
  connectText:
    'Berikan alamat MCP server kami ke agen Anda. Setelah itu, agen Anda tahu cara men-deploy aplikasi di sini. Tanpa API key, tanpa login.',
  sayLike: 'Lalu, di proyek Anda, tinggal bilang seperti ini',
  asks: [
    'Publikasikan aplikasi ini di GenHTTP Lambda, lalu kirim link-nya ke saya.',
    'Buat skor tertingginya tersimpan bersama, jadi semua orang melihat papan peringkat yang sama.',
  ],

  domainChip: 'Kalau mulai populer',
  domainTitle: 'Pakai nama domain sendiri',
  domainText:
    'Aplikasi yang sama, link editor yang sama, tapi di alamat milik Anda. Lebih gampang diucapkan, lebih mudah diingat, dan terlihat meyakinkan saat orang mulai membagikannya.',
  domainSubject: 'Domain untuk aplikasi saya',
  domainAsk: 'Tanya soal domain',

  questionsTitle: 'Sebelum men-deploy',
  questions: (offline, removed, showcase, terms) => [
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
      'Di mana API key saya disimpan?',
      'Tidak di dalam kode. Agen Anda meminta key berdasarkan namanya, lalu Anda mengetik nilainya di editor. Tidak ada yang bisa membacanya lagi, baik editor maupun agen.',
    ],
    [
      'Bisakah saya membawa kode saya?',
      'Bisa, kodenya milik Anda. Unduh dari editor kapan saja, sebagai proyek yang bisa berjalan sendiri, lengkap dengan database-nya.',
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

  closeTitle: 'Di laptop Anda sudah jalan.',
  closeAccent: 'Sekarang giliran mereka.',
  noAgent: 'Belum punya agen? Buat di sini',
  closeFacts: 'Gratis. Tanpa daftar. Tanpa instalasi.',

  scene: {
    label:
      'Agen diminta memublikasikan sebuah aplikasi. Alamatnya berubah dari localhost jadi link publik, lalu orang-orang bergabung.',
    ask: 'Publikasikan game kuis saya supaya teman-teman bisa ikut main.',
    live: 'Sudah online. Ini link-nya.',
    publishing: 'Memublikasikan…',
    public: 'Publik',
    onlyYou: 'Hanya Anda',
    app: 'Kuis Jumat malam',
    playing: (count) => <>{count} pemain</>,
    you: 'Anda',
  },

  compareTitle: 'Cara tercepat dari “sudah jalan” ke “coba deh”',
  compareText:
    'Vercel, Cloudflare, dan Lovable adalah tempat yang bagus untuk menjalankan aplikasi. Tapi semuanya dimulai dengan formulir pendaftaran. Begitu aplikasi Anda perlu berbagi data antarpengunjung, Anda juga harus menyiapkan layanan kedua. Beginilah perbandingannya kalau Anda mulai dari nol.',
  rows: [
    'Mulai tanpa akun',
    'Publikasikan dari agen yang sudah Anda pakai',
    'Database dan data live: chat, multiplayer, rekor',
    'Biaya sampai dapat link pertama',
  ],
  us: ['Ya', 'Hubungkan sekali, lalu tinggal minta', 'Sudah ada di setiap aplikasi', 'Gratis'],
  rivals: [
    ['Harus daftar', 'Setelah login ke tool-nya', 'Tambah layanan database', 'Paket gratis'],
    ['Harus daftar', 'Setelah login ke tool-nya', 'Bisa, perlu setup', 'Paket gratis'],
    ['Harus daftar', 'Dibuat di editornya sendiri', 'Lewat backend yang terhubung', 'Paket gratis, kredit terbatas'],
  ],
  compareNote:
    'Per September 2026, untuk orang yang belum punya akun di mana pun. Paket dan fitur layanan lain bisa berubah, jadi cek langsung ke mereka untuk detailnya.',

};
