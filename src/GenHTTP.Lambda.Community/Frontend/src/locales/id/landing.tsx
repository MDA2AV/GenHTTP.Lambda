import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Edisi Community',
  headline: 'Platform tempat agen Anda membangun aplikasi.',
  headlineAccent: 'Berjalan di server Anda sendiri.',
  intro:
    'Jelaskan sebuah aplikasi dan agen pembangun akan menulisnya - atau biarkan Claude Code, Codex atau Cursor mengerjakannya lewat MCP. Setiap aplikasi adalah handler GenHTTP berbasis C#, dikompilasi di server ini dan online dalam hitungan detik, lengkap dengan database, kunci dan kata sandi, versi, dan git.',
  describe: 'Jelaskan sebuah aplikasi',
  connect: 'Hubungkan agen Anda',
  write: 'Tulis kodenya sendiri',

  buildTitle: 'Jelaskan saja',
  buildText: 'Satu kalimat sudah cukup. Agen pembangun menulis aplikasinya, menjadikannya online, dan memberi Anda link untuk mengubahnya nanti.',
  noAgent: 'Belum ada agen pembangun yang terhubung ke instalasi ini. Hubungkan agen Anda sendiri di bawah - hasilnya sama.',

  connectTitle: 'Hubungkan agen Anda',
  connectText:
    'Agen pemrograman Anda mendapatkan platform ini sebagai alat: membuat aplikasi, menulis dan memeriksa kodenya, mencoba perubahan di draf dengan salinan data, menjadikannya online, dan membaca lognya.',
  connectThen: 'Lalu minta sesuatu padanya, misalnya: “Buatkan buku tamu, dengan halaman untuk menyetujui entri.”',

  featuresTitle: 'Yang didapat setiap aplikasi',
  features: [
    { title: 'Sebuah alamat', text: 'Online di bawah /lambda/ pada server ini begitu berhasil dikompilasi - atau di domain sendiri.' },
    { title: 'Catatan', text: 'Database SQLite tersendiri, tabelnya dibuat lewat migrasi, dibaca dan ditulis dengan Entity Framework Core.' },
    { title: 'File', text: 'Sebuah folder untuk apa yang disimpan aplikasi: unggahan, ekspor, apa pun yang bukan catatan.' },
    { title: 'Kunci dan kata sandi', text: 'Disimpan tersegel di server. Agen menyebutkan mana yang dibutuhkan, Anda memasukkannya, dan tidak ada yang bisa membacanya kembali.' },
    { title: 'Draf dan versi', text: 'Perubahan dicoba di draf dengan salinan data, lalu dijadikan online. Setiap versi disimpan, cukup satu klik untuk kembali.' },
    { title: 'Git', text: 'Setiap aplikasi adalah repositori: kloning, push commit untuk menjadikannya online, push branch untuk memulai draf.' },
    { title: 'Pengunjung', text: 'Tayangan halaman, asal pengunjung, dan negaranya - dihitung di server, tanpa cookie, setelah Anda mengaktifkannya.' },
    { title: 'Kode Anda', text: 'Unduh aplikasi apa pun sebagai proyek .NET biasa dan jalankan di mana saja, atau publikasikan kodenya agar dibaca orang lain.' },
  ],

  runsTitle: 'Cara kerjanya',
  runs: [
    { title: 'Satu container', text: 'Server, editor, API, dan semua aplikasi dalam satu proses, dan semua yang mereka simpan dalam satu folder.' },
    {
      title: 'Lewat path atau host',
      text: 'Aplikasi menjawab di bawah /lambda/ pada alamat ini. Beri instalasi sebuah domain untuk mereka, dan setiap aplikasi mendapat subdomain sendiri.',
    },
    {
      title: 'Kode yang Anda percaya',
      text: 'Aplikasi dikompilasi ke dalam server, dan kodenya diperiksa agar tidak melakukan hal terlarang - tetapi mereka berbagi proses dengannya. Jalankan hanya yang akan Anda jalankan sendiri.',
    },
    { title: 'Open source', text: 'Berlisensi MIT: baca, ubah, dan jalankan di mana pun Anda suka.' },
  ],
  repository: 'Kode sumber di GitHub',

  aroundTitle: 'Lihat-lihat',
  around: {
    demos: { title: 'Mulai dari demo', text: 'API di atas catatan, login, pembaruan langsung, sebuah game: masing-masing aplikasi jadi untuk disalin dan diubah.' },
    showcase: { title: 'Showcase', text: 'Aplikasi yang dipilih pemiliknya untuk ditampilkan di sini.' },
    guide: { title: 'Panduan', text: 'Apa yang bisa dilakukan aplikasi dan caranya, berurutan sesuai kebutuhan Anda.' },
    api: { title: 'REST API', text: 'Sama seperti alat-alatnya, lewat HTTP, dijelaskan dalam OpenAPI.' },
    sources: { title: 'Kode sumber terpublikasi', text: 'Aplikasi yang pemiliknya mempublikasikan kodenya, untuk dibaca dan diunduh.' },
    admin: { title: 'Administrasi', text: 'Aplikasi, batasan, pemblokiran, dan log, di balik token admin.' },
  },
};
