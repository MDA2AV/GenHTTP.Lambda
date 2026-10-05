import type { SourceMessages } from '../en/source';

/** Teks halaman kode sumber yang dipublikasikan (/source dan setiap proyek di bawahnya) dalam bahasa Indonesia. */
export const source: SourceMessages = {
  shell: {
    section: 'Open Source',
    home: 'GenHTTP Lambda, beranda',
  },

  lambda: {
    label: 'Apa itu lambda?',
    text: 'Aplikasi web di GenHTTP Lambda: seseorang menyampaikan apa yang diinginkannya, agen AI menulisnya dalam C#, dan dalam hitungan menit aplikasi itu online di alamatnya sendiri - setiap versinya disimpan, lengkap dengan apa yang diubahnya.',
    build: 'Buat aplikasi Anda sendiri',
  },

  catalog: {
    eyebrow: 'Open Source',
    title: 'Lihat bagaimana aplikasi di sini dibuat',
    intro:
      'Lambda yang kodenya dipublikasikan oleh pemiliknya: setiap versi, apa yang diubahnya, dokumentasi, dan pengujiannya. Baca di sini, atau unduh proyek yang bisa dijalankan di mana pun .NET berjalan.',
    searchLabel: 'Cari proyek',
    searchPlaceholder: 'Cari berdasarkan nama atau fungsinya',
    orderLabel: 'Urutan',
    orders: {
      stars: 'Bintang terbanyak',
      updated: 'Baru diubah',
      published: 'Baru dipublikasikan',
    },
    counted: (total) => `${total} proyek`,
    failed: 'Proyek gagal dimuat.',
    loadingMore: 'Memuat lebih banyak…',
    showMore: 'Tampilkan lebih banyak',
    nothingTitle: 'Belum ada yang dipublikasikan',
    nothing: (tab) => (
      <>
        Sudah membuat sesuatu yang bisa menjadi bahan belajar orang lain? Buka pusat kontrolnya, pilih{' '}
        {tab('Open Source')}, pilih lisensi, dan kodenya akan muncul di sini.
      </>
    ),
    noMatchTitle: 'Tidak ada yang cocok',
    noMatch: (query) => `Tidak ada proyek yang dipublikasikan yang menyebut “${query}”.`,
    clear: 'Tampilkan semua proyek',
    yoursTitle: 'Publikasikan milik Anda',
    yours: (tab) => (
      <>
        Buka pusat kontrol lambda Anda dan pilih {tab('Open Source')}, atau minta agen yang membuatnya untuk
        memublikasikannya. Hanya pemegang kunci editor yang bisa melakukannya, dengan lisensi yang dipilihnya - dan apa
        yang disimpan aplikasinya, yaitu catatan, file, dan kuncinya, tidak pernah ikut dipublikasikan.
      </>
    ),
    build: 'Buat aplikasi',
    online: 'Online',
    offline: 'Offline',
    changed: (ago) => `diubah ${ago}`,
    stars: (count) => `${count} bintang`,
  },

  project: {
    loading: 'Memuat kode sumber…',
    failed: 'Kode sumber gagal dimuat.',
    missingTitle: 'Tidak ada kode sumber yang dipublikasikan di sini',
    missing: 'Pemiliknya mungkin sudah menariknya, atau memang tidak pernah ada lambda di alamat ini.',
    all: 'Semua proyek',
    by: (name) => `oleh ${name}`,
    versions: (count) => `${count} versi`,
    onlineAt: (address) => <>Online di {address}</>,
    offline: 'Sedang offline',
    openApp: 'Buka aplikasi',
    opens: (address) => `Membuka ${address} di tab baru`,
    published: (ago) => `Dipublikasikan ${ago}`,
    changed: (ago) => `Diubah ${ago}`,
    picture: (name) => `Tampilan ${name}`,
    tabsLabel: 'Yang bisa dibaca',
    tabs: {
      code: 'Kode',
      docs: 'Dokumentasi',
      tests: 'Pengujian',
      changes: 'Perubahan',
    },
  },

  versions: {
    label: 'Versi',
    choose: 'Baca versi lain',
    newest: 'terbaru',
    online: 'online',
    older: (version, ago, newest) =>
      `Anda sedang membaca versi ${version}, yang disimpan ${ago}. Versi terbaru adalah versi ${newest}.`,
    toNewest: 'Baca yang terbaru',
    noChange: 'Tidak ada catatan tentang apa yang diubahnya',
  },

  star: {
    star: 'Beri bintang',
    add: 'Beri bintang untuk proyek ini',
    remove: 'Tarik kembali bintang Anda',
    count: (count) => `${count} bintang`,
    failed: 'Bintang gagal disimpan.',
  },
  clone: {
    button: 'Kode',
    title: 'Kloning dengan git',
    what: (oldest, newest) =>
      oldest === newest
        ? `Versinya adalah commit di main, diberi tag v${newest}.`
        : `Setiap versi ikut sebagai commit di main, diberi tag v${oldest} sampai v${newest} - main adalah yang terbaru.`,
    readOnly:
      'Hanya bisa dibaca. Untuk membangun di atasnya, mulai lambda Anda sendiri dan pindahkan file-file ini - AGENTS.md di dalam kloning menjelaskan caranya, dan lisensinya apa yang boleh Anda lakukan.',
  },

  download: {
    title: (version) => `Versi ${version} sebagai proyek`,
    what:
      'Proyek .NET 10 dengan Dockerfile, dokumentasi, pengujian, dan lisensinya. Apa yang disimpan aplikasi - catatannya, file yang disimpannya, kuncinya - tidak ikut di dalamnya.',
    zip: 'Unduh ZIP',
    preparing: 'Menyiapkan proyek…',
    slow: 'Unduhan pertama sebuah versi dikemas selagi Anda menunggu.',
    failed: 'Proyek gagal disiapkan. Coba lagi sebentar lagi.',
    run: 'Jalankan',
    local: 'Dengan .NET 10 SDK:',
    container: 'Atau di dalam container:',
    agent: 'Atau serahkan foldernya ke agen coding Anda dan kembangkan lebih lanjut - dengan tetap mematuhi lisensinya.',
    copy: 'Salin',
    copied: 'Tersalin',
  },

  tree: {
    label: 'File',
    files: (count) => `${count} file`,
    packing: 'Mengemas versi ini…',
    packingSlow: 'Sebuah versi dikemas saat pertama kali ada yang membacanya, dan untuk versi yang besar ini butuh sedikit waktu.',
    failed: 'File versi ini gagal dimuat.',
    legend: 'Keterangan',
    kinds: {
      code: 'Kode lambda itu sendiri',
      asset: 'Yang disajikannya: halaman, script, style, gambar - dan migrasi database-nya',
      docs: 'Apa aplikasinya, dan mengapa dibuat seperti ini',
      tests: 'Cara pengujiannya',
      dev: 'Bahan pembuat kode atau asetnya dengan alat build',
      platform: 'Pengganti platform',
      project: 'Host, build, container, dan lisensi',
    },
    short: {
      code: 'Kode',
      asset: 'Disajikan',
      docs: 'Dokumen',
      tests: 'Pengujian',
      dev: 'Build',
      platform: 'Platform',
      project: 'Proyek',
    },
  },

  file: {
    loading: 'Memuat…',
    failed: 'File ini gagal dimuat.',
    missing: (path) => `Tidak ada ${path} di versi ini.`,
    binary: 'File ini bukan teks.',
    tooLarge: 'File ini terlalu panjang untuk ditampilkan di sini.',
    download: 'Unduh',
    raw: 'Raw',
    rawTitle: 'Buka file apa adanya',
    copy: 'Salin',
    copied: 'Tersalin',
    lines: (count) => `${count} baris`,
    plain: 'Ditampilkan tanpa warna karena terlalu panjang.',
    line: (line) => `Baris ${line}`,
  },

  docs: {
    pages: 'Halaman',
    product: 'Apa aplikasinya',
    decisions: 'Keputusan',
    loading: 'Memuat…',
    failed: 'Halaman ini gagal dimuat.',
    noneTitle: 'Belum ada yang ditulis tentang versi ini',
    none: 'Dokumentasinya seharusnya ada di docs/: apa aplikasinya, untuk siapa, dan mengapa dibuat seperti ini.',
  },

  tests: {
    files: 'Script dan data',
    noneTitle: 'Versi ini tidak menjelaskan apa pun tentang pengujiannya',
    none: 'Cara pengujiannya seharusnya ada di tests/README.md, dengan script yang dijalankannya di sampingnya.',
  },

  changes: {
    title: 'Semua versi, yang terbaru di atas',
    intro: 'Versi tidak pernah berubah setelah disimpan. Masing-masing menjelaskan dalam satu baris apa yang diubahnya.',
    agent: 'Ditulis oleh agen',
    online: 'online',
    browse: 'Baca kodenya',
    noChange: 'Tanpa catatan',
  },

  licenses: {
    MIT: 'Siapa pun boleh memakai, mengubah, dan membagikannya, untuk apa pun, selama lisensi dan pemberitahuan hak ciptanya tetap disertakan.',
    'Apache-2.0': 'Seperti MIT, ditambah lisensi paten dari semua kontributornya, dan perubahan harus ditandai sebagai perubahan.',
    'BSD-3-Clause': 'Seperti MIT, dan tidak seorang pun boleh memakai nama para penulisnya untuk mempromosikan hasil turunannya.',
    'MPL-2.0': 'Perubahan pada file-file ini tetap berada di bawah lisensi yang sama; file-file ini boleh digabungkan dengan kode berlisensi apa pun.',
    'GPL-3.0-or-later': 'Siapa pun yang membagikannya, diubah atau tidak, wajib membagikan kode sumbernya dengan lisensi yang sama.',
    'AGPL-3.0-or-later': 'Seperti GPL, dan menyediakan salinan yang diubah kepada orang lain lewat jaringan juga dihitung sebagai membagikannya.',
    Unlicense: 'Diserahkan ke domain publik: siapa pun boleh melakukan apa saja dengannya, tanpa syarat.',
  },

  kinds: {
    Permissive: 'Permisif',
    Copyleft: 'Copyleft',
    PublicDomain: 'Domain publik',
  },
};
