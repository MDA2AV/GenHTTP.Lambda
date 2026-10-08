import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Enterprise',
  title: 'Coba gratis, jalankan sendiri',
  intro:
    'Semua di sini gratis, tanpa akun. Kalau tim Anda butuh aplikasi yang online selamanya dan dilindungi login sendiri, pakai instalasi milik Anda sendiri, di cloud atau on-premise.',

  free: 'Gratis',
  freeTagline: 'Untuk coba-coba',
  forever: 'selamanya',
  buildOne: 'Buat aplikasi',
  freeFeatures: (offline, removed) => [
    'Lambda tanpa batas, tanpa akun',
    'Agen bawaan, atau agen Anda sendiri lewat MCP',
    'Online selama dipakai',
    `Offline setelah ${offline} hari tanpa kunjungan, dihapus setelah ${removed} hari`,
    'Diakses lewat subdomain dari domain bersama',
  ],
  freeNote: 'Tanpa kartu kredit, tanpa daftar. Buat lambda, dan langsung jadi milik Anda.',

  name: 'Enterprise',
  tagline: 'Untuk tim yang ingin instance sendiri',
  perUser: 'per pengguna / bulan',
  contact: 'Hubungi kami',
  features: [
    'Instance sendiri, di cloud atau on-premise',
    'Satu layanan menjalankan semua aplikasi',
    'Login dengan SSO Anda sendiri',
    'Aturan tata kelola dan kepatuhan Anda sudah tertanam',
    'Aplikasi online selamanya, tidak pernah dihapus',
    'Pakai agen Anda sendiri lewat MCP',
    'Dukungan prioritas',
  ],
  users: (count) => <>{count} pengguna</>,
  perMonth: ' / bulan',
  price: (amount) => `$${amount}`,
  perUserPrice: (amount) => `$${amount} / pengguna / bulan`,

  compareTitle: 'Bandingkan paket',
  compareText: 'Keduanya berjalan di platform yang sama. Bedanya: berapa lama aplikasi Anda disimpan, dan di mana.',
  included: 'Termasuk',
  notIncluded: 'Tidak termasuk',
  groups: (offline, removed) => [
    {
      title: 'Pembuatan',
      rows: [
        ['Lambda', 'Tanpa batas', 'Tanpa batas'],
        ['Agen bawaan', true, false],
        ['Agen sendiri lewat MCP', true, true],
        ['Editor, versi, dan log', true, true],
        ['Showcase', true, 'Milik sendiri'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Offline jika tidak dipakai', `Setelah ${offline} hari`, 'Tidak pernah'],
        ['Dihapus jika tidak dipakai', `Setelah ${removed} hari`, 'Tidak pernah'],
        ['Instance', 'Bersama', 'Milik sendiri'],
        ['Berjalan di', 'Cloud kami', 'Cloud atau on-premise'],
        ['Yang Anda kelola', 'Tidak ada', 'Satu layanan'],
        ['Domain kustom', false, true],
      ],
    },
    {
      title: 'Kontrol',
      rows: [
        ['Login', 'Tidak perlu', 'SSO Anda sendiri'],
        ['Aturan tata kelola dan kepatuhan Anda untuk agen', false, true],
        ['Konsol admin', false, true],
        ['Data terpisah dari pelanggan lain', false, true],
        ['Dukungan', 'Komunitas', 'Prioritas'],
      ],
    },
  ],

  questionsTitle: 'Pertanyaan',
  questions: [
    ['Perlu akun untuk mulai?', 'Tidak. Lambda gratis hanya butuh link editor yang Anda dapat saat membuatnya.'],
    [
      'Siapa yang dihitung sebagai pengguna di Enterprise?',
      'Semua orang yang login lewat SSO Anda, baik untuk membuat di editor maupun untuk memakai aplikasi yang di-deploy di instalasi Anda. Siapa pun yang membuka aplikasi tanpa login tidak dihitung.',
    ],
    [
      'Apakah agen bawaan termasuk di Enterprise?',
      'Tidak. Tim Anda memakai agen sendiri, seperti Claude, Claude Code, atau apa pun yang mendukung MCP, dan menghubungkannya ke instalasi Anda. Agennya tetap memakai paket yang sudah Anda punya dari vendornya.',
    ],
    [
      'Bagaimana agen mempelajari aturan kepatuhan kami?',
      'Kami memasukkan aturan tata kelola dan kepatuhan Anda ke dalam informasi yang diberikan platform ke agen lewat MCP. Setiap agen yang dihubungkan tim Anda mendapatkannya saat menulis kode. Jadi aplikasinya sudah mengikuti aturan Anda, tanpa semua orang harus menghafalnya.',
    ],
    [
      'Perlu Kubernetes atau cluster?',
      'Tidak. Semua aplikasi berjalan di dalam satu layanan, jadi tidak ada pod yang perlu disebar dan tidak ada yang perlu diorkestrasi per aplikasi. Menjalankan instalasinya berarti menjalankan satu layanan itu saja.',
    ],
    [
      'Di mana instalasi Enterprise berjalan?',
      'Terserah Anda. Kami bisa meng-host-nya untuk Anda di cloud kami. Bisa juga di akun cloud Anda atau di server Anda sendiri, di mana saja yang bisa menjalankan container. Apa pun pilihannya, kami bantu menyiapkannya dan terus memperbaruinya.',
    ],
  ],
  anythingElse: (mail) => <>Ada pertanyaan lain? Kirim email ke {mail}.</>,
};
