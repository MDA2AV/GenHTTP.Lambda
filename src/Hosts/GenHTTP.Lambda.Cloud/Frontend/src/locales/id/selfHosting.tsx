import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Self-hosting',

  eyebrow: 'Open source · cloud atau self-hosted',
  title: 'Pakai di sini,',
  titleAccent: 'atau self-host sendiri.',
  intro:
    'GenHTTP Lambda adalah open source dengan lisensi MIT. genhttp.dev menjalankannya untuk dipakai siapa saja, dan edisi Community adalah platform yang sama sebagai image Docker, bagi siapa saja yang lebih suka menjalankannya sendiri. Keduanya membuat dan menjalankan aplikasi yang sama; bedanya ada pada tempat aplikasi berada dan siapa yang mengurusnya.',

  cloud: {
    kicker: 'Cloud',
    name: 'genhttp.dev',
    summary: 'Buka situs ini dan mulai. Server, pembaruannya, dan batasannya kami yang mengurus.',
    points: ['Tidak ada yang perlu dipasang atau dijalankan', 'Aplikasi menjawab di alamat di genhttp.run', 'Termasuk agen pembuat'],
    action: 'Buat sesuatu',
  },
  community: {
    kicker: 'Self-hosted',
    name: 'Edisi Community',
    summary: 'Satu container Docker di mesin Anda. Server, pembaruannya, dan batasannya Anda yang mengurus.',
    points: ['Satu container, satu volume, tanpa server database', 'Aplikasi menjawab di alamat Anda', 'Agen pembuat sebagai container tambahan'],
    action: 'Jalankan sendiri',
  },

  sameTitle: 'Sama di keduanya',
  sameText: 'Edisi Community bukan versi yang lebih kecil. Setiap aplikasi mendapat hal yang sama di mana pun ia berjalan:',
  same: [
    'Agen pembuat',
    'Agen Anda sendiri lewat MCP',
    'REST API',
    'Editor, tampilan sederhana dan lengkap',
    'Draf dengan data uji',
    'Versi, dan jalan kembali',
    'Catatan, file, kunci dan kata sandi',
    'Analitik tanpa cookie',
    'Domain sendiri',
    'Git dan unduhan',
    'Showcase dan kode yang dipublikasikan',
  ],

  differTitle: 'Yang berbeda',
  aspect: 'Pertanyaan',
  rows: (offline) => [
    ['Tempat aplikasi berjalan', 'Di server kami', 'Di server Anda, dalam satu container dengan satu volume'],
    ['Cara memulai', 'Buka halaman buat - tanpa akun', 'Jalankan image-nya, lalu buka alamatnya'],
    ['Siapa yang mengurus server', 'Kami', 'Anda'],
    ['Biayanya', 'Gratis untuk dicoba; produksi untuk organisasi nirlaba atas permintaan, untuk perusahaan sesuai kesepakatan', 'Tidak ada; server Anda bayar sendiri'],
    ['Tempat aplikasi menjawab', 'Di subdomain genhttp.run', 'Di bawah alamat Anda, atau di subdomain dari domain yang Anda berikan'],
    ['Domain sendiri', 'Untuk aplikasi produksi, didaftarkan oleh Anda', 'Untuk aplikasi apa pun yang Anda pindahkan ke produksi'],
    ['Aplikasi yang tidak dipakai', `Dimatikan setelah ${offline} hari tanpa kunjungan atau perubahan, kecuali dalam produksi`, 'Sesuai pengaturan Anda'],
    ['Batasan dan aturan', 'Dari kami, beserta ketentuan layanan kami', 'Dari Anda, diatur di panel admin'],
    ['Agen pembuat', 'Termasuk, dengan sejumlah pembuatan per hari', 'Opsional, dengan langganan Claude atau kunci API Anda sendiri'],
    ['Laporan penggunaan', 'Tidak ada', 'Sekali sehari: versi, mesin, opsi, dan jumlah aplikasi - tidak pernah isi aplikasi atau data orang. Bisa Anda matikan.'],
  ],

  moveTitle: 'Berpindah di antara keduanya',
  move:
    'Aplikasi yang sama berjalan di keduanya, dan agen Anda memindahkannya dengan git: ia membuat aplikasi di sisi lain, menyalin file aplikasi lama, lalu mem-push-nya. Unduhan berupa proyek .NET yang berjalan di mana saja, dengan catatannya dalam file SQLite.',

  pickTitle: 'Mana yang cocok untuk Anda',
  pickCloud: {
    title: 'genhttp.dev, jika',
    points: [
      'Anda ingin aplikasi online tanpa menjalankan apa pun',
      'orang yang memakainya ada di mana saja',
      'Anda tidak mau mengurus server',
    ],
  },
  pickCommunity: {
    title: 'Edisi Community, jika',
    points: [
      'aplikasi dan catatannya harus tetap di mesin Anda',
      'aplikasinya alat untuk tim Anda, di dalam jaringan sendiri',
      'Anda ingin mengatur batasan dan aturannya sendiri',
    ],
  },

  startTitle: 'Cara memulai',
  startCloud: {
    title: 'Di genhttp.dev',
    text: 'Jelaskan apa yang Anda butuhkan di halaman buat, atau hubungkan agen yang sudah Anda pakai. Tidak perlu membuat akun.',
  },
  startCommunity: {
    title: 'Di server Anda',
    text: 'Satu perintah menjalankannya. Buka alamatnya, hubungkan agen Anda, lalu mulai membuat - README menjelaskan sisanya.',
  },
  connect: 'Hubungkan agen Anda',
  readme: 'Baca README',
  dockerHub: 'Image di Docker Hub',

  sourceTitle: 'Open source',
  source: (github) => (
    <>
      GenHTTP Lambda dirilis dengan lisensi MIT. Kodenya, isu-isunya, dan rilisnya ada di {github('GitHub')}.
    </>
  ),
};
