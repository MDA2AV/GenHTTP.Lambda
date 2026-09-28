import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Belum aktif di sini',
  off: (write, mcp) => (
    <>
      Instalasi ini tidak punya agen build. Anda tetap bisa {write('menulisnya sendiri')}, atau arahkan Claude Anda
      sendiri ke {mcp}.
    </>
  ),

  title: 'Ceritakan yang Anda mau.',
  intro:
    'Aplikasinya dibuat, langsung online, dan Anda mendapat link yang bisa dikirim ke siapa saja. Tanpa akun, tanpa instalasi. Aplikasinya juga bisa menyimpan data, seperti skor, pesan, atau entri, jadi semua yang membukanya melihat hal yang sama.',
  placeholder: 'buatkan…',
  working: 'sedang dikerjakan…',
  shortcut: 'ctrl + enter',
  building: 'Sedang dibuat',
  buildIt: 'Buat sekarang',
  builtBy: 'Dibuat oleh',
  password: 'kata sandi',
  fable:
    'Fable masih dalam uji coba, jadi dilindungi kata sandi. Fable berjalan tanpa batas waktu. Ia terus bekerja sampai aplikasinya selesai, bukan sampai waktunya habis.',
  onlyNew:
    'Halaman ini hanya untuk membuat aplikasi baru. Untuk mengubah yang sudah Anda buat, buka link editornya, lalu tulis apa yang perlu diubah di bagian “Ubah”.',
  ideas: [
    'dinding tempat siapa pun bisa menulis pesan satu baris',
    'papan skor tertinggi untuk game dadu',
    'polling tempat orang bisa vote dan melihat total suaranya',
    'buku tamu untuk pernikahan saya',
    'hitung mundur ke tanggal tertentu yang bisa dilihat semua orang',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Tinggal satu build lagi, lalu giliran Anda.' : `Ada ${waiting} build di depan Anda.`,
  starting: 'Memulai…',

  yourApp: 'Aplikasi Anda',
  further: 'Untuk mengembangkannya lagi',
  keep: 'Simpan baik-baik. Ini satu-satunya cara untuk masuk lagi, dan tidak bisa dipulihkan. Kami pun tidak bisa. Bookmark dulu sebelum menutup tab ini.',
  change:
    'Untuk mengubahnya, buka link editornya, lalu tulis apa yang perlu diubah di bagian “Ubah”, sama seperti di sini. Agen coding Anda sendiri juga bisa melakukannya, seperti dijelaskan di bawah.',
  copyLink: 'Salin link editor',
  lifetime: (offline, removed) =>
    `Aplikasi ini tetap online selama dipakai. Setelah ${offline} hari tanpa kunjungan atau perubahan, aplikasinya jadi offline, dan setelah ${removed} hari dihapus. Buka editor dan tekan Deploy untuk membuatnya online lagi.`,
  openEditor: 'Buka editor',
  another: 'Buat yang lain',

  keepGoing: 'Lanjutkan dengan agen Anda sendiri',
  orOwn: 'Atau pakai agen Anda sendiri',
  ownText:
    'Kotak di atas adalah Claude yang berjalan di server ini. Kalau Anda sudah punya agen sendiri, arahkan saja ke sini. Agen itu bisa melakukan hal yang sama: membuat lambda, menulis kode, dan membuatnya online. Tanpa batas harian, dan tanpa lewat halaman ini.',
  thenAsk: 'Lalu minta apa yang Anda mau, sama seperti di sini.',
  claudeWeb: 'Claude di web',
  claudeWebHow:
    'Pengaturan, lalu “Connectors”, lalu “Add custom connector”. Tempel alamat di atas sebagai URL server MCP remote. Tanpa API key, tanpa login.',
  howToChange:
    'Begitu juga cara mengubah aplikasi yang sudah jadi: berikan link editornya ke agen Anda, lalu jelaskan apa yang perlu dilakukan.',
  more: 'Selengkapnya tentang memakai agen di sini',

  failedToStart: 'Permintaan gagal dikirim.',
  noAnswer: 'Selesai, tapi tanpa keterangan apa yang terjadi.',
  failed: 'Tidak berhasil.',
};
