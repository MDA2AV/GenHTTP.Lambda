import type { EditorMessages } from '../en/editor';

/** Daftar dengan koma dan "dan" sebelum yang terakhir: "judul, deskripsi, dan gambar". */
const list = (items: string[]) =>
  items.length > 2 ? `${items.slice(0, -1).join(', ')}, dan ${items[items.length - 1]}` : items.join(' dan ');

/** Teks editor dalam bahasa Indonesia. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 'dtk', min: 'mnt', h: 'jam', d: 'hari' },
    amount: (value, unit) => `${value} ${unit}`,
    pair: (larger, smaller) => `${larger} ${smaller}`,
    never: 'tidak pernah',
    justNow: 'baru saja',
    ago: (span) => `${span} lalu`,
    in: (span) => `${span} lagi`,
    origins: {
      agent: 'agen',
      template: 'template',
      admin: 'operator',
      system: 'platform',
      api: 'API / editor',
      unknown: 'tidak diketahui',
    },
    endings: {
      replaced: 'diganti deployment yang lebih baru',
      stopped: 'dimatikan',
      expired: 'kedaluwarsa karena tidak dipakai',
      admin: 'dimatikan oleh operator',
      ended: 'berakhir',
    },
    whatThisIs: 'Apa ini',
    byAgent: 'oleh agen',
    writtenByAgent: 'Ditulis oleh agen',
    more: 'Lainnya',
    of: (used, total) => `${used} dari ${total}`,
    online: (version) => `Online · v${version}`,
    onlineTitle: (version) => `Online, menyajikan versi ${version}`,
    offline: 'Offline',
    offlineTitle: 'Offline: tidak ada yang disajikan',
    premium:
      'Premium: bisa diakses di domain sendiri, punya ruang lebih besar untuk kode, aset, dan data, serta tetap online sesepi apa pun',
    demo: 'Demo: dijaga tetap online oleh instalasi ini dan hanya bisa dibaca',
    tier: (tier) => `Paket ${tier}`,
    entrances: {
      title: 'Diakses lewat',
      note: 'Sejak server dimulai, termasuk koneksi websocket.',
    },
    chart: {
      showChart: 'Tampilkan grafik',
      showValues: 'Tampilkan nilai',
      none: 'Belum ada data.',
      time: 'Waktu',
    },
    diagnostics: {
      compiles: 'Kode berhasil dikompilasi.',
      none: 'Belum ada pesan. Periksa atau deploy untuk mengompilasi kode Anda.',
      line: (line) => `baris ${line}`,
    },
  },

  frame: {
    title: 'Editor',
    sections: {
      overview: 'Ringkasan',
      showcase: 'Showcase',
      domain: 'Domain',
      files: 'File',
      versions: 'Versi',
      deployments: 'Deployment',
      stats: 'Statistik',
      logs: 'Log',
      code: 'Kode',
    },
    sectionsLabel: 'Bagian',
    loadFailed: 'Lambda ini gagal dimuat.',
    online: (version) => `Versi ${version} sudah online.`,
    deployFailed: 'Lambda gagal di-deploy.',
    offline: 'Sudah dimatikan. Kodenya masih ada.',
    offlineFailed: 'Lambda gagal dimatikan.',
    leave: 'Perubahan kode yang belum disimpan akan hilang. Tetap keluar?',
    nothingTitle: 'Link ini tidak membuka apa pun',
    createNew: 'Buat lambda baru',
    loading: 'Memuat lambda Anda…',
    moreActions: 'Aksi lainnya',
    redeploy: (version) => `Deploy ulang versi ${version}`,
    takeOffline: 'Matikan',
    copyLink: 'Salin link',
    copyPrivate: 'Salin link privat',
    privateLink: 'Siapa pun yang punya link ini bisa mengubah lambda. Jangan dibagikan.',
    rename: 'Ganti alamat',
    download: 'Unduh sebagai proyek .NET',
    delete: 'Hapus lambda ini',
    deploy: (version) => `Deploy versi ${version}`,
    problems: 'Ada masalah baru-baru ini',
    demoTitle: 'Demo, dijaga tetap online oleh instalasi ini dan hanya bisa dibaca.',
    demo: (start) => (
      <>
        Baca kodenya, riwayatnya, apa yang disimpannya, dan log-nya. Untuk itulah demo ini ada. Untuk mengubahnya,{' '}
        {start('buat lambda Anda sendiri dari demo ini')}.
      </>
    ),
    keep: 'Simpan link ini. Hanya lewat link ini Anda bisa masuk lagi ke lambda ini.',
    gotIt: 'Mengerti',
    rejected: (version) => `Versi ${version} gagal online`,
    refused: 'Deployment ditolak',
    openCode: 'Buka kode',
    close: 'Tutup',
    notCompiling: 'Kode tidak bisa dikompilasi. Versi yang sebelumnya online tetap online.',
    moved: (path) => `Sekarang di ${path}.`,
    deleteTitle: 'Hapus lambda ini?',
    cancel: 'Batal',
    deleteForGood: 'Hapus permanen',
    deleteFailed: 'Lambda gagal dihapus.',
    deleteText: (key) => (
      <>Semua versi, file, riwayat, dan alamat {key} ikut terhapus. Tindakan ini tidak bisa dibatalkan.</>
    ),
    openInTab: 'Buka di tab baru',
    open: (address) => `Buka ${address} di tab baru`,
    copyAddress: 'Salin alamat',
    renameFailed: 'Alamat gagal diganti.',
    moveIt: 'Pindahkan',
    renameText: 'Alamat lama langsung berhenti berfungsi, jadi perbarui apa pun yang menautkan ke sana.',
  },

  summary: {
    reading: 'Membaca kondisinya…',
    hint: (since, kept, retention, tier) =>
      `Trafik dihitung sejak server terakhir dimulai (${since}). ` +
      (kept
        ? `Lambda tetap online selama dipakai, dan dihapus setelah ${retention} hari tanpa kunjungan dan tanpa perubahan.`
        : `Lambda ini ada di paket ${tier}, yang membuatnya tetap online dan tersimpan sesepi apa pun.`),
    onlineFor: (duration, version) => (
      <>
        Online selama {duration('beberapa waktu')}, menyajikan versi {version}.
      </>
    ),
    offline: 'Offline. Tidak ada yang disajikan sampai sebuah versi di-deploy.',
    nothing: 'Belum ada kode yang ditulis.',
    requestsToday: 'request hari ini',
    lastHour: (count) => `${count} dalam satu jam terakhir`,
    hourly: 'Request per jam selama sehari terakhir',
    failed: 'gagal',
    failedTitle: (failed, rejected) =>
      `${failed} error server, ${rejected} tidak ditemukan atau ditolak, selama sehari terakhir`,
    average: 'rata-rata waktu respons',
    noneYet: 'belum ada',
    lastVisit: 'kunjungan terakhir',
    problems: 'Ada masalah baru-baru ini',
    openLog: 'Buka log',
    latest: 'Perubahan terakhir',
    allVersions: 'Semua versi',
    noDescription: 'Tanpa deskripsi',
    version: (version) => `Versi ${version}`,
    notOnline: 'belum online',
    wanted: 'Yang diminta',
    noVersions: 'Belum ada versi.',
    storage: 'Penyimpanan',
    browse: 'Lihat',
    code: 'Kode',
    codeWhy: 'C# dikompilasi, tidak pernah disajikan.',
    characters: 'karakter',
    assets: 'Aset',
    assetsPublic: 'Publik: disajikan oleh kode.',
    assetsPrivate: 'Tidak disajikan oleh kode.',
    data: 'Data',
    dataPublic: 'Publik: kode menyajikan workspace.',
    dataPrivate: 'Privat, hanya untuk lambda ini.',
  },

  files: {
    hint: (b) => (
      <>
        {b('Kode')} dikompilasi dan tidak pernah disajikan. {b('Aset')} (halaman, style, gambar) disimpan bersama setiap
        versi, dan bersifat publik kalau kode menyajikannya. {b('Data')} adalah apa yang ditulis lambda selama berjalan.
        Data bukan bagian dari versi mana pun, dan hanya publik kalau kode menyajikannya.
      </>
    ),
    edit: 'Edit versi ini',
    version: 'Versi',
    shown: (version, online, newest) => `Versi ${version}${online ? ', online' : newest ? ', terbaru' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'Versi itu gagal dibaca.',
    dataFailed: 'Data gagal dibaca.',
    noVersion: 'Belum ada versi untuk ditampilkan.',
    label: 'File',
    code: 'Kode',
    codeWhy: 'Dikompilasi ke dalam lambda, tidak pernah disajikan.',
    count: (files) => `${files} file`,
    codeUsage: (files, used, of) => `${files}, ${used} dari ${of} karakter`,
    usage: (files, used, of) => `${files}, ${used} dari ${of}`,
    noCode: 'Tidak ada kode di versi ini.',
    assets: 'Aset',
    assetsPublic: 'Publik: versi ini menyajikannya lewat Assets.',
    assetsPrivate: 'Disimpan bersama kode, tapi tidak disajikan oleh versi ini.',
    noAssets: 'Tidak ada di versi ini.',
    data: 'Data',
    dataPublic: 'Publik: versi ini menyajikannya lewat Workspace.',
    dataPrivate: 'Privat, hanya untuk lambda ini. Bukan bagian dari versi mana pun.',
    uploadFailed: (path) => `Gagal mengunggah ${path}.`,
    deleteFolder: (path, held) =>
      held > 0 ? `Hapus ${path} beserta ${held} file di dalamnya?` : `Hapus folder ${path}?`,
    deleteFile: (path) => `Hapus ${path}? Lambda tidak akan bisa menemukannya lagi.`,
    deleteFailed: 'Gagal dihapus.',
    full: 'Ruang data sudah penuh',
    uploadInto: (folder) => `Unggah ke ${folder}`,
    upload: 'Unggah',
    reading: 'Membaca…',
    noData: 'Belum ada apa-apa. Apa yang disimpan lambda selama berjalan akan muncul di sini.',
    delete: (path) => `Hapus ${path}`,
    deleteShort: 'Hapus',
    fileFailed: 'File gagal dibaca.',
    pick: 'Pilih file untuk melihat isinya.',
    tooLarge: (name, size) => (
      <>
        {name} berukuran {size}, terlalu besar untuk ditampilkan di sini.
      </>
    ),
    download: 'Unduh',
    readingFile: (name) => `Membaca ${name}…`,
    missing: (name) => `Versi ini tidak punya file bernama ${name}.`,
    saved: 'tersimpan',
    notText: 'Bukan teks. Unduh untuk melihat isinya.',
  },

  versions: {
    hint: (limit) =>
      `Setiap versi menyimpan apa yang diminta dan apa yang diubah, kalau penulisnya mencatatnya. Versi terlama dihapus begitu jumlahnya lebih dari ${limit}; versi yang sedang online tidak pernah dihapus.`,
    none: 'Belum ada versi.',
    noDescription: 'Tanpa deskripsi',
    online: 'online',
    putOnline: 'Deploy versi ini',
    rollBackTitle: 'Jadikan versi lama ini online lagi',
    deploy: 'Deploy',
    rollBack: 'Rollback',
    readFailed: 'Versi ini gagal dibaca.',
    comparing: 'Membandingkan…',
    unchanged: 'Tidak ada yang berubah dari versi sebelumnya.',
    first: 'Versi pertama.',
    status: { added: 'ditambahkan', removed: 'dihapus', changed: 'diubah', same: 'sama' },
    browse: 'Lihat file-nya',
    edit: 'Edit dari sini',
    binary: 'Bukan teks, jadi tidak ada baris untuk dibandingkan.',
    tooLarge: 'Terlalu besar untuk dibandingkan baris per baris.',
  },

  deployments: {
    hint: (until) =>
      `Deployment tetap online selama dipakai${until ? ` (kalau tidak ada yang memakai, sampai ${until})` : ''}. Deploy ulang, atau kunjungan apa pun, memulai ulang hitungan itu.`,
    takeOffline: 'Matikan',
    readFailed: 'Riwayat gagal dibaca.',
    reading: 'Membaca riwayat…',
    none: 'Belum ada yang di-deploy.',
    noDescription: 'Tanpa deskripsi',
    deployed: (when, by) => `Di-deploy pada ${when} oleh ${by}`,
    duration: 'Berapa lama online',
    online: 'online',
    short: {
      replaced: 'diganti',
      stopped: 'dimatikan',
      expired: 'kedaluwarsa',
      admin: 'oleh operator',
      ended: 'berakhir',
    },
    putBack: (version) => `Jadikan versi ${version} online lagi`,
    timeline: 'Yang online selama tujuh hari terakhir',
    block: (version, from, to) => `Versi ${version}, ${from} sampai ${to ?? 'sekarang'}`,
    weekAgo: 'seminggu lalu',
    now: 'sekarang',
  },

  stats: {
    readFailed: 'Statistik gagal dibaca.',
    range: 'Rentang waktu',
    lastHour: '1 jam terakhir',
    lastDay: '1 hari terakhir',
    hint: (since) =>
      `Dihitung di memori sejak server terakhir dimulai, ${since}. Setelah restart, angka ini mulai dari nol lagi.`,
    reading: 'Membaca statistik…',
    requests: 'request',
    websockets: (count) => `dan ${count} koneksi websocket`,
    failed: 'gagal',
    serverErrors: (count) => `${count} error server`,
    rejected: 'tidak ditemukan atau ditolak',
    average: 'rata-rata waktu respons',
    sent: (amount) => `${amount} terkirim`,
    nobody: (hour) =>
      hour ? 'Tidak ada yang mengaksesnya dalam satu jam terakhir.' : 'Tidak ada yang mengaksesnya dalam sehari terakhir.',
    requestsTitle: 'Request',
    per: (hour) => (hour ? 'Per menit.' : 'Per 15 menit.'),
    answered: 'Dijawab',
    rejectedSeries: 'Tidak ditemukan atau ditolak',
    failedSeries: 'Gagal',
    timeTitle: 'Waktu respons',
    averagePer: (hour) => (hour ? 'Rata-rata per menit.' : 'Rata-rata per 15 menit.'),
    averageSeries: 'Rata-rata',
    mostAsked: 'Paling sering diminta',
    path: 'Path',
    requestsColumn: 'Request',
    failedColumn: 'Gagal',
    averageColumn: 'Rata-rata',
    since: 'Sejak server dimulai.',
  },

  logs: {
    readFailed: 'Log gagal dibaca.',
    hint: (capturing) =>
      'Request, apa yang dicetak lambda, dan apa yang error, secara langsung.' +
      (capturing ? '' : ' Instalasi ini tidak menyimpan apa yang dicetak lambda, jadi hanya request dan error yang muncul.') +
      ' Disimpan di memori dan dipakai bersama semua lambda di sini, jadi hanya mencakup beberapa menit sampai beberapa jam terakhir, dan kosong setelah restart. Alamat pengunjung tidak ditampilkan.',
    search: 'Cari',
    searchLabel: 'Cari di log',
    resume: 'Tampilkan baris baru saat masuk',
    pause: 'Berhenti menambah baris baru selama Anda membaca',
    paused: 'Dijeda',
    live: 'Live',
    show: 'Tampilkan',
    all: 'Semua',
    requests: 'Request',
    output: 'Yang dicetak',
    problems: 'Masalah',
    reading: 'Membaca log…',
    noProblems: 'Tidak ada error yang masih tercatat di log.',
    nothing: 'Belum ada apa-apa. Buka alamat lambda, dan request-nya akan muncul di sini.',
    noMatch: 'Tidak ada yang cocok.',
    identical: (count) => `${count} baris identik`,
    at: (domain) => `, di ${domain}`,
    from: (country) => `, dari ${country}`,
  },

  showcase: {
    loadFailed: 'Showcase gagal dimuat.',
    loading: 'Memuat…',
    title: 'judul',
    description: 'deskripsi',
    picture: 'gambar',
    updated: 'Entri showcase sudah diperbarui.',
    listed: 'Sekarang sudah tampil di halaman showcase.',
    waiting: 'Tersimpan. Akan tampil di halaman showcase begitu lambda online.',
    saveFailed: 'Entri showcase gagal disimpan.',
    removed: 'Sudah dihapus dari halaman showcase.',
    removeFailed: 'Entri showcase gagal dihapus.',
    wrongType: 'Itu bukan gambar PNG, JPEG, GIF, atau WebP.',
    tooLarge: (size, limit) => `Ukurannya ${size}, padahal gambar maksimal ${limit}.`,
    unreadable: 'File itu gagal dibaca.',
    hint: (tool) => (
      <>
        Halaman showcase menampilkan lambda yang dipamerkan pemiliknya, yang baru-baru ini dipakai lebih dulu. Hanya
        pemegang kunci editor yang bisa memasang lambda di sana atau menariknya lagi, dan lambda hanya tampil selama
        online. Agen juga bisa melakukannya dengan tool {tool}.
      </>
    ),
    open: 'Buka showcase',
    switch: 'Tampilkan lambda ini di halaman showcase',
    listedNow: 'Sudah tampil. Siapa pun yang melihat showcase bisa membukanya.',
    notListed: 'Tersimpan, tapi belum tampil: lambda sedang offline. Akan muncul lagi begitu di-deploy ulang.',
    off: 'Nonaktif. Tidak ada info tentang lambda ini yang ditampilkan di mana pun sampai Anda mengaktifkannya dan menyimpan.',
    offline: 'Lambda sedang offline, jadi entri ini menunggu sampai di-deploy. Hanya lambda yang merespons yang ditampilkan.',
    titleLabel: 'Judul',
    titlePlaceholder: 'Papan skor kuis kafe',
    descriptionLabel: 'Deskripsi',
    descriptionPlaceholder:
      'Tim mengisi jawaban di HP masing-masing, pembawa acara menilainya, dan papan skor langsung diperbarui untuk semua orang di ruangan.',
    save: 'Simpan perubahan',
    add: 'Tambahkan ke showcase',
    takeOff: 'Hapus dari showcase',
    needs: (missing) => `Masih perlu ${list(missing)}.`,
    tooLong: 'Ada yang terlalu panjang.',
    allSaved: 'Semua sudah tersimpan.',
    preview: 'Pratinjau',
    card: (address) => <>Ini kartu yang dilihat pengunjung. Kartu ini membuka {address}.</>,
    confirm: 'Hapus dari showcase?',
    keep: 'Tetap tampilkan',
    confirmText: 'Judul, deskripsi, dan gambarnya dihapus. Lambda-nya sendiri tetap persis seperti sekarang.',
    pictureLabel: 'Gambar',
    formats: (limit) => `PNG, JPEG, GIF, atau WebP, maksimal ${limit}`,
    notSaved: 'belum disimpan',
    replace: 'Seret gambar baru ke sini untuk menggantinya.',
    drop: 'Seret gambar ke sini.',
    advice: 'Screenshot, atau GIF pendek saat aplikasinya dipakai, paling bagus dengan rasio 16:10.',
    another: 'Pilih yang lain',
    choose: 'Pilih file',
    keepSaved: 'Pakai yang tersimpan',
    clear: 'Kosongkan',
  },

  domain: {
    readFailed: 'Domain gagal dibaca.',
    reaching: (domain) => `Request ke ${domain} sekarang sampai ke lambda ini.`,
    saveFailed: 'Domain gagal disimpan.',
    removed: 'Domain sudah dihapus. Lambda tetap bisa diakses di alamatnya di sini.',
    removeFailed: 'Domain gagal dihapus.',
    hint: 'Lambda premium bisa diakses di domain sendiri (seluruhnya, mulai dari root), selain di alamatnya di sini. Arahkan domain ke server ini, masukkan di sini, dan request ke domain itu akan sampai ke lambda.',
    loading: 'Memuat…',
    example: 'domain-anda.com',
    open: (domain) => `Buka ${domain}`,
    label: 'Domain yang dipakai',
    serving: (domain) => <>Sekarang juga melayani {domain}, selain alamatnya di sini.</>,
    none: 'Belum ada. Bisa subdomain seperti shop.example.com, atau domain penuh seperti example.com.',
    change: 'Ganti',
    use: 'Pakai domain ini',
    remove: 'Hapus',
    confirm: 'Hapus domain?',
    keep: 'Tetap pakai',
    confirmText: (domain) => (
      <>
        Request ke {domain} langsung berhenti sampai ke lambda ini. Alamatnya di sini tetap sama, begitu juga pengaturan
        DNS domain itu.
      </>
    ),
    point: 'Arahkan domain ke server ini',
    check: 'Cek lagi',
    records:
      'Di penyedia DNS domain Anda, tambahkan dua record ini. Lewati record AAAA kalau Anda tidak ingin bisa diakses lewat IPv6.',
    type: 'Tipe',
    name: 'Nama',
    value: 'Nilai',
    pointsHere: (domain) => <>{domain} sudah mengarah ke sini.</>,
    alsoElsewhere: (addresses) =>
      ` Domain ini juga mengarah ke ${addresses}, yang bukan server ini. Pengunjung yang diarahkan ke sana tidak akan sampai ke lambda.`,
    elsewhere: (addresses) => `Domain ini masih mengarah ke ${addresses}, bukan ke server ini.`,
    wait: 'Perubahan bisa butuh waktu sampai terlihat di mana-mana, paling lama sesuai TTL record yang lama.',
    cname: 'Pakai record CNAME sebagai gantinya',
    cnameText: (target) => (
      <>
        Subdomain juga bisa diarahkan ke {target} dengan record CNAME. Dengan begitu, subdomain ikut menyesuaikan kalau
        alamat server ini berubah. Tapi ada kekurangannya:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Tidak bisa dipakai untuk domain penuh ({example} itu sendiri): standarnya tidak mengizinkan CNAME berdampingan
        dengan record yang dimiliki setiap domain di root-nya. Beberapa penyedia menawarkan record ALIAS, ANAME, atau
        “flattened” yang bisa dipakai di sana.
      </>
    ),
    cnameAlone: 'Tidak boleh ada record lain di nama yang sama: tidak ada record MX untuk email, tidak ada record TXT untuk verifikasi.',
    cnameLookup: 'Resolver pengunjung perlu satu lookup tambahan sebelum sampai.',
    copy: 'Salin',
    copyValue: (value) => `Salin ${value}`,
  },

  code: {
    title: 'Kode',
    version: (version) => `versi ${version}`,
    edited: ', diedit',
    online: ', online',
    loadFailed: 'Versi itu gagal dimuat.',
    compiles: 'Berhasil dikompilasi.',
    notYet: 'Belum bisa dikompilasi.',
    checkFailed: 'Kode gagal diperiksa.',
    saved: (version) => `Disimpan sebagai versi ${version}.`,
    isOnline: (version) => `Versi ${version} sudah online.`,
    notOnline: 'Gagal online. Lihat pesan compiler di bawah.',
    failed: 'Tidak berhasil.',
    unchanged: 'Tidak ada perubahan sejak terakhir disimpan.',
    demo: 'Ini demo, jadi semuanya hanya bisa dibaca. Untuk mengubahnya, buat lambda Anda sendiri dari demo ini. ',
    edit: 'Edit kode secara manual. Menyimpan membuat versi baru tanpa mengubah yang sedang online; deploy membuatnya online. ',
    files: (entry, cs) => (
      <>
        {entry} mengembalikan apa yang disajikan, file {cs} lainnya berisi tipe, dan file lain disajikan apa adanya.
        Ctrl-S untuk menyimpan, F12 untuk membuka deklarasi.
      </>
    ),
    newer: (version) => ` Versi ${version} lebih baru dari yang terbuka di sini.`,
    check: 'Periksa',
    save: 'Simpan',
    deploy: 'Deploy',
    binary: (size) => `Bukan teks, jadi tidak ada yang bisa diedit. File ini disajikan apa adanya, ukurannya ${size} kB.`,
    saveAndDeploy: 'Simpan dan deploy',
    saveVersion: 'Simpan versi baru',
    cancel: 'Batal',
    what: 'Apa yang diubah? Opsional, akan ditampilkan di riwayat.',
    placeholder: 'Menambahkan formulir kontak',
    goToDefinition: 'Buka definisi',
  },

  tabs: {
    codeName: 'Huruf, angka, tanda hubung, dan garis bawah, diakhiri .cs',
    slashes: 'Tanpa garis miring di awal atau akhir, dan kurang dari 120 karakter.',
    deep: 'Maksimal enam level folder.',
    characters: 'Huruf, angka, tanda hubung, garis bawah, dan titik, dipisahkan garis miring.',
    extension: 'Perlu ekstensi, supaya bisa disajikan dengan tipe yang benar.',
    exists: 'Sudah ada file dengan nama itu.',
    remove: (name) => `Hapus ${name}? Isinya ikut terhapus.`,
    there: (name) => `${name} sudah ada.`,
    entry: 'Snippet utama: apa yang dikembalikannya, itulah yang disajikan',
    errors: 'ada error',
    removeFile: (name) => `Hapus ${name}`,
    removeTitle: 'Hapus file ini',
    placeholder: 'Types.cs atau site/index.html',
    newFile: 'File baru',
    uploadTitle: 'Unggah file: gambar, font, atau halaman',
    upload: 'Unggah file',
  },
};
