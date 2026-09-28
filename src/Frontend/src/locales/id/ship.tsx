import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Dari laptop Anda ke layar semua orang.',
  intro:
    'Anda sudah membuat sesuatu dengan agen coding, tapi baru bisa jalan di komputer Anda sendiri. Minta agen Anda memublikasikannya di sini. Beberapa menit kemudian, aplikasinya punya link publik yang bisa dibuka siapa saja. Aplikasinya juga bisa menyimpan data, jadi orang bisa main, chat, dan posting bersama di dalamnya.',
  facts: ['Gratis', 'Tanpa akun', 'Tanpa instalasi'],
  connect: 'Hubungkan agen Anda',
  seeOthers: 'Lihat karya orang lain',

  stepsTitle: 'Tiga langkah, dan salah satunya cuma satu kalimat',
  step: (n) => `Langkah ${n}`,
  steps: [
    {
      title: 'Hubungkan sekali',
      body: 'Tambahkan satu alamat ke Claude, Cursor, atau agen apa pun yang Anda pakai. Kurang dari semenit, dan cukup sekali saja.',
    },
    {
      title: 'Minta dipublikasikan',
      body: 'Minta agen Anda membuat aplikasinya online di sini. Agen akan mengemasnya, memublikasikannya, lalu mengecek apakah aplikasinya sudah merespons.',
    },
    {
      title: 'Bagikan link-nya',
      body: 'Anda mendapat alamat publik dan link editor privat. Kirim yang pertama ke siapa saja. Simpan yang kedua, karena lewat link itulah Anda bisa mengubah aplikasinya nanti.',
    },
  ],

  togetherTitle: 'Bukan sekadar halaman. Tempat orang berkumpul.',
  together:
    'Kebanyakan hosting memberi setiap pengunjung salinan aplikasi sendiri-sendiri, jadi semua orang main sendirian. Di sini, setiap aplikasi punya memori sendiri dan koneksi live ke semua orang yang sedang membukanya. Apa yang dilakukan satu orang langsung terlihat oleh yang lain, dan apa yang mereka posting masih ada besok.',
  together2:
    'Tidak perlu daftar layanan database, tidak perlu menyambungkan layanan kedua. Minta saja, seperti Anda menjelaskannya ke teman.',
  kinds: [
    { name: 'Game multiplayer', ask: 'Buat supaya maksimal delapan teman bisa ikut ronde yang sama dan melihat langkah masing-masing secara live.' },
    { name: 'Ruang chat', ask: 'Tambahkan ruang chat untuk semua orang yang punya link-nya, dan simpan seratus pesan terakhir.' },
    { name: 'Daftar bersama', ask: 'Ubah daftar barang bawaan jadi daftar yang bisa diedit seluruh tim sekaligus.' },
    { name: 'Skor dan rekor', ask: 'Buat papan peringkat berisi waktu terbaik tiap orang, dan tampilkan sepuluh besar di layar awal.' },
    { name: 'Jejaring sosial mini', ask: 'Buat supaya tamu pernikahan bisa posting foto di satu dinding dan saling kasih like.' },
  ],
  quote: (text) => `“${text}”`,

  connectTitle: 'Hubungkan agen Anda, sekali saja',
  connectText:
    'Berikan alamat ini ke agen Anda. Setelah itu, agen Anda tahu cara memublikasikan di sini. Tanpa API key, tanpa login.',
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

  questionsTitle: 'Sebelum Anda bertanya',
  questions: (offline, removed, showcase, terms) => [
    [
      'Benar-benar gratis?',
      <>
        Ya. Tanpa kartu kredit, tanpa masa percobaan, tanpa akun. Aplikasi Anda tetap online selama masih dipakai. Setelah{' '}
        {offline} hari tanpa satu pun kunjungan atau perubahan, aplikasinya dimatikan, dan setelah {removed} hari
        dihapus.
      </>,
    ],
    [
      'Apakah aplikasi saya harus dibuat dengan cara tertentu?',
      'Tidak, itu urusan agen Anda. Halaman, gambar, dan style diunggah apa adanya, dan apa pun yang harus berjalan di server akan disesuaikan agen untuk platform ini. Anda cukup menjelaskan apa yang harus dilakukan aplikasinya. Agen yang menerjemahkannya.',
    ],
    [
      'Bagaimana cara mengubahnya nanti?',
      'Dengan link editor yang Anda dapat saat aplikasinya dipublikasikan. Berikan ke agen Anda bersama perubahan berikutnya, atau buka di browser. Setiap perubahan yang Anda minta jadi versinya sendiri di alamat yang sama, dan Anda bisa kembali ke versi lama kapan saja.',
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
  closeFacts: 'Gratis. Tanpa akun. Tanpa instalasi.',

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
    'Data bersama secara live: chat, multiplayer, rekor',
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

  yourAgent: 'Agen Anda',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Jalankan ini sekali di terminal. Setelah itu, setiap proyek yang Anda buka bisa dipublikasikan di sini.',
    claude: (strong) => (
      <>
        Di Claude versi web atau desktop, buka {strong('Pengaturan')}, lalu {strong('Connectors')}, dan pilih{' '}
        {strong('Add custom connector')}. Tempel alamat di atas, lalu simpan. Selesai.
      </>
    ),
    cursor: 'Tambahkan ini ke pengaturan MCP di Cursor, atau ke file di bawah, lalu muat ulang.',
    vscode: 'Simpan ini di proyek Anda, lalu jalankan server dari tampilan MCP di Copilot Chat.',
  },
  elsewhere:
    'Pakai tool lain? Windsurf, Codex, Zed, dan kebanyakan agen lain bisa menambahkan server MCP remote di pengaturannya. Berikan alamat di atas.',
};
