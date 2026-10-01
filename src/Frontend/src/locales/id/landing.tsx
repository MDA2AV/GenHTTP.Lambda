import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'AI pembuat aplikasi + hosting',
  headline: 'Ceritakan ide aplikasi Anda.',
  headlineAccent: 'Agen Anda yang membuatnya online.',
  intro:
    'Polling, buku tamu, papan peringkat, game multiplayer. Ceritakan yang Anda butuhkan ke agen AI kami atau ke agen yang sudah Anda pakai. Hasilnya: aplikasi yang langsung jalan dan di-hosting oleh kami, lengkap dengan link untuk dibagikan. Aplikasinya tetap bisa diubah, jadi versi pertama barulah permulaan.',
  build: 'Buat aplikasi',
  ownAgent: 'Pakai agen Anda sendiri',
  free: 'Gratis. Tanpa daftar, tanpa kartu kredit, tanpa instalasi.',
  seeIt: 'Lihat cara kerjanya',

  videoTitle: 'Dari satu kalimat jadi aplikasi online',
  videoText:
    'Jendela browser incognito, tanpa akun, dan satu permintaan di halaman Buat. Setelah itu, aplikasi yang sudah jadi dibuka lewat link-nya, sama seperti pengunjung biasa.',
  videoNote: 'Bagian build dipercepat. Sisanya real-time.',
  tryIt: 'Coba sendiri',

  oneShotTitle: 'Bukan generator aplikasi sekali pakai',
  oneShotText:
    'Kebanyakan generator memberi hasil, lalu Anda ditinggal begitu saja. Di sini, aplikasinya tetap berjalan di tempat ia dibuat, jadi Anda dan agen Anda bisa terus mengembangkannya.',
  steps: [
    {
      title: 'Ceritakan yang Anda mau',
      body: 'Pakai bahasa sehari-hari, ke agen di situs ini atau ke agen yang sudah Anda pakai. Tanpa kode, tanpa setup, tanpa akun.',
      alt: 'Halaman Buat dengan permintaan polling makan siang yang sudah diketik',
    },
    {
      title: 'Dapatkan aplikasi jadi dan link-nya',
      body: 'Aplikasinya dibuat dan di-hosting, lalu Anda dapat alamat publik untuk dibagikan. Tidak perlu menyiapkan server, paket hosting, domain, atau database, karena bagian itu kami yang urus. Datanya tetap tersimpan (vote, skor, pesan), jadi semua orang yang membukanya melihat data yang sama.',
      alt: 'Polling makan siang yang sudah jadi, terbuka di browser',
    },
    {
      title: 'Terus kembangkan',
      body: 'Setiap aplikasi punya link editor privat. Berikan ke agen Anda bersama perubahan berikutnya, atau buka sendiri. Tiap perubahan jadi versi baru, dan alamatnya tetap sama.',
      alt: 'Pusat kontrol polling: daftar versinya, masing-masing dengan apa yang diminta, apa yang diubah, dan bedanya dengan versi sebelumnya',
    },
  ],
  weekLater: 'Seminggu kemudian',
  weekAsk:
    'Ini link editor polling makan siang saya. Tolong tutup voting tiap Jumat jam 11, dan tampilkan pemenangnya di paling atas.',
  weekAnswer:
    'Beres. Versi 4 sudah online di alamat yang sama. Versi 3 masih ada kalau Anda mau kembali.',

  agentsTitle: 'Bawa agen Anda sendiri: Claude, Codex, Cursor',
  agentsText:
    'Sudah vibe coding dengan Claude Code, Codex, Cursor, atau asisten lain? Hubungkan ke alamat ini (MCP server remote, tanpa API key), lalu agen Anda bisa membuat, men-deploy, dan memperbarui aplikasi di sini. Langsung dari percakapan yang sedang Anda buka.',
  thenAsk: (em) => (
    <>Lalu tinggal minta: {em('buatkan form pendaftaran online untuk acara tim kami')}.</>
  ),
  hostIt: (link) => (
    <>Aplikasi Anda baru jalan di localhost? {link('Deploy aplikasi hasil vibe coding Anda di sini')}.</>
  ),

  contactTitle: 'Hubungi kami',
  contactText:
    'Butuh bantuan, punya rencana yang lebih besar, atau mencari solusi yang dibuat khusus untuk Anda? Kami tunggu kabar dari Anda.',
  mailTitle: 'Kirim email',
  mailText: 'Untuk proyek, pertanyaan, dan hal lain yang ingin Anda bahas secara pribadi.',
  discordTitle: 'Gabung ke Discord',
  discordText: 'Pamerkan yang sudah Anda buat, minta bantuan untuk langkah berikutnya, dan ngobrol langsung dengan tim.',
  discordLink: 'Discord GenHTTP',
};
