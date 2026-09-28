import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Cara kerjanya',
  intro:
    'Anda menulis snippet C#. Apa pun yang dikembalikannya di-host di alamat publik, lewat HTTPS, dalam hitungan detik. Inilah semuanya, sesuai urutan yang akan Anda temui.',
  contents: 'Daftar isi',

  parts: {
    what: 'Apa itu lambda',
    first: 'Lambda pertama Anda',
    editor: 'Pusat kontrol',
    why: 'Mencatat alasan',
    features: 'Mengubah dengan aman',
    files: 'Lebih dari satu file',
    page: 'Menyajikan halaman',
    spa: 'Front end, langkah demi langkah',
    storage: 'Dua tempat file disimpan',
    keeping: 'Menyimpan data',
    sockets: 'Websocket',
    limits: 'Yang tidak diizinkan',
    away: 'Membawa kode Anda keluar',
    agents: 'Menyerahkannya ke agen',
  },

  what: [
    (k) => (
      <>
        Lambda adalah snippet yang mengembalikan handler GenHTTP. Platform mengompilasinya, memuatnya, lalu memasang apa
        pun yang dikembalikannya di bawah alamat Anda sendiri. Tidak ada proyek, tidak ada file build, dan tidak ada
        statement {k.code('using')}. Semua modul GenHTTP sudah di-import untuk Anda.
      </>
    ),
    (k) => (
      <>
        Itu sudah lambda yang lengkap. Setelah di-deploy di {k.code('/lambda/your-key/')}, lambda ini menjawab setiap
        request dengan kata hello.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Snippet ini berisi {k.em('statement')}, bukan class. Hal terakhir yang dilakukannya adalah mengembalikan sesuatu
      yang bisa melayani request: handler, atau builder untuk handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Tekan {k.b('Buat lambda')}. Anda mendapat alamat publik dan kunci editor. Kunci itu satu-satunya jalan untuk masuk
        lagi, jadi simpan baik-baik. Tidak ada yang bisa memulihkannya untuk Anda.
      </>
    ),
    () => (
      <>
        Anda masuk ke pusat kontrolnya, dengan layanan REST kecil yang sudah tertulis sebagai versi pertama. Itu hanya
        titik awal.
      </>
    ),
    (k) => (
      <>
        Berikan kunci editor ke agen dan jelaskan apa yang harus dibuat. Agen menulis versi-versi baru lewat{' '}
        {k.link('/#agents', 'MCP')}. Atau buka {k.b('Kode')} dan tulis sendiri:{' '}
        {k.b('Periksa')} mengompilasi tanpa menyimpan apa pun, lalu menunjukkan pesan compiler, lengkap dengan file dan
        barisnya.
      </>
    ),
    (k) => (
      <>
        Tekan {k.b('Deploy')}. Sekarang lambda Anda online. Sebelum itu, tidak ada yang bisa diakses. Deploy ulang
        memperpanjang masa online-nya.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Link editor membuka pusat kontrol, bukan kotak teks. Sebagian besar kode di sini ditulis oleh agen, jadi yang
      pertama tampil di layar adalah kondisi lambda Anda. Sidebar berisi lambda-nya (apakah online, alamatnya, dan tombol
      kalau ada versi lebih baru yang menunggu untuk online) beserta bagian-bagiannya. Hal yang jarang dilakukan, seperti
      mengganti alamat atau menghapus lambda, ada di menu {k.b('⋯')} di sana.
    </>
  ),
  bits: [
    ['Ringkasan', () => <>Apakah lambda online, berapa request hari ini dan berapa yang gagal, perubahan terakhir, dan sisa ruang penyimpanan.</>],
    [
      'Ubah',
      (k) => (
        <>
          Tulis apa yang perlu diubah, dan agen di server ini akan mengerjakannya sementara Anda melihat. Agen bekerja di
          sebuah draf, mencobanya di sana, lalu menggabungkannya menjadi versi berikutnya begitu berhasil. Matikan{' '}
          {k.b('Langsung online setelah selesai')} kalau Anda ingin mencoba drafnya sendiri dulu.
        </>
      ),
    ],
    ['Draf', () => <>Perubahan yang dikerjakan di samping lambda: masing-masing dicoba di alamatnya sendiri dan digabungkan menjadi versi berikutnya setelah hasilnya pas. Saat dibuka, draf punya kode, data, dan log-nya sendiri.</>],
    ['File', () => <>File dari sebuah versi: kode dan asetnya, yaitu programnya sendiri. Ikon gembok atau globe menunjukkan apakah publik bisa mengaksesnya.</>],
    ['Data', () => <>Apa yang disimpan lambda selama berjalan, dipakai bersama oleh semua versi: workspace. Lihat isinya, unggah dan hapus file, atau nonaktifkan.</>],
    ['Versi', () => <>Apa yang diubah setiap versi dan apa yang diminta, serta bedanya dengan versi sebelumnya. Deploy atau rollback dari sini, atau mulai draf dari versi mana pun.</>],
    ['Deployment', () => <>Apa yang online dan kapan, dan apa yang membuatnya berhenti.</>],
    ['Statistik', () => <>Request, kegagalan, waktu respons, dan path yang paling sering diminta, selama satu jam atau satu hari terakhir.</>],
    ['Log', () => <>Request yang masuk, apa yang dicetak lambda, dan stack trace dari setiap error, secara langsung.</>],
    [
      'Kode',
      (k) => (
        <>
          Menulis kode secara manual. {k.b('Periksa')} mengompilasi, {k.b('Simpan')} membuat versi, {k.b('Deploy')}{' '}
          membuatnya online. Di draf, {k.b('Simpan')} menyimpannya di draf dan {k.b('Deploy pratinjau')} membuatnya
          online di alamat draf itu. {k.code('Ctrl-S')} menyimpan; {k.code('F12')} membuka deklarasi.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Setiap bagian bekerja dengan cara yang sama: judulnya, tombol {k.b('ⓘ')} yang menjelaskannya, aksinya di kanan,
      dan (kalau punya lebih dari satu tampilan) deretan tab di bawahnya. Tab di bagian kode adalah file-filenya.
    </>
  ),
  editorAside:
    'Trafik dan log hanya ada di memori, untuk dipantau, bukan untuk diarsipkan: restart server memulainya dari awal. Versi dan riwayat deployment disimpan permanen.',

  why: (k) => (
    <>
      Sebuah versi berisi kode, plus dua catatan opsional: {k.b('spesifikasi')}, yaitu apa yang diinginkan pengguna dan
      alasannya, sebisa mungkin dengan kata-kata mereka sendiri, dan {k.b('perubahan')}, satu baris tentang apa yang
      dilakukan versi itu. Keduanya ditampilkan di samping diff di riwayat versi, jadi {k.em('alasannya')} tetap
      tercatat di samping {k.em('isinya')}. Berguna untuk Anda, dan untuk agen berikutnya yang membaca riwayat sebelum
      mengubah apa pun.
    </>
  ),
  whySample: {
    specification: 'Buku tamu yang bisa diisi orang; entri harus tetap ada setelah restart',
    change: 'Menyimpan entri di workspace supaya tetap ada setelah restart',
  },
  why2: (k) => (
    <>
      Agen mengirim dua field yang sama ke {k.code('write_code')}. Di {k.b('Kode')}, saat menyimpan Anda diminta
      mengisi perubahannya. Keduanya opsional. Spesifikasi yang terlalu panjang dipotong di 4.000 karakter, dan
      perubahan di 500 karakter, bukan ditolak. Draf menyimpan dua catatannya sendiri, dan versi hasil penggabungannya
      mengambil alih catatan itu.
    </>
  ),

  features: (k) => (
    <>
      Versi tidak pernah berubah setelah disimpan, dan justru itulah yang membuat setiap versi layak disimpan: versi
      mana pun bisa dibandingkan, dan dijadikan online lagi persis seperti semula. Untuk mengubah lambda yang sedang
      dipakai orang, mulai {k.b('draf')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Mulai di {k.b('Draf')}, atau dari versi mana pun. Draf adalah salinan kode dan aset versi itu, dan salinan data
        lambda.
      </>
    ),
    (k) => (
      <>
        Ubah sesering yang diperlukan, di {k.b('Kode')} atau dengan meminta agen. {k.b('Deploy pratinjau')} membuatnya
        online di alamatnya sendiri, {k.code('/features/…/')}, dengan salinan datanya sendiri. Pengunjung lambda tidak
        melihat apa pun, dan tidak ada yang ditulisnya yang sampai ke data lambda.
      </>
    ),
    (k) => (
      <>
        {k.b('Gabungkan')} setelah hasilnya pas: draf menjadi versi berikutnya, lengkap dengan catatannya, dan bisa
        langsung online kalau Anda mau. Drafnya ikut hilang, termasuk pratinjau dan salinan datanya.
      </>
    ),
  ],
  featureSample: 'Papan peringkat',
  featuresAside: () => (
    <>
      Beberapa draf bisa dikerjakan sekaligus. Hanya draf yang berbasis versi terbaru yang bisa digabungkan, supaya
      penggabungan tidak pernah membatalkan versi yang disimpan setelah draf itu dimulai. Kalau ada draf lain yang
      digabungkan lebih dulu, masukkan perubahannya (atau minta agen melakukannya), lalu jadikan versi terbaru sebagai
      dasar draf. Tidak ada yang tergabung dengan sendirinya; itu disengaja.
    </>
  ),

  files: (k) => (
    <>
      Tipe tidak harus berada di bawah kode yang memakainya. Di {k.b('Kode')}, tekan {k.b('+')} di samping daftar file,
      dan file baru itu dikompilasi bersama snippet, di namespace yang sama, jadi tidak perlu import apa pun untuk
      mengaksesnya. Nama tanpa ekstensi dianggap C#.
    </>
  ),

  page: 'Ada dua cara untuk menyajikan halaman, dan satu cara lagi untuk file yang diunggah orang di sampingnya.',
  inlineTitle: 'Satu halaman, ditulis inline',
  inline: 'Cocok untuk yang kecil. Halamannya jadi bagian dari snippet.',
  folderTitle: 'Folder berisi file sungguhan',
  folder:
    'Pilihan tepat untuk apa pun yang punya stylesheet dan script. File ditambahkan dengan cara yang sama seperti file C#, dan disajikan persis seperti yang ditulis. Tidak ada yang mengompilasinya.',
  workspaceTitle: 'File unggahan, dari data',
  workspace:
    'Untuk apa yang diunggah orang atau dibuat lambda (gambar, dokumen), disajikan di samping aplikasinya. Bukan untuk halaman aplikasinya sendiri: halaman itu tempatnya di folder file, di mana halaman ikut masuk versi bersama kode yang membutuhkannya.',

  spa: (k) => (
    <>
      Cara kedua, secara lengkap. Setiap demo menyajikan halamannya dengan cara ini, dari folder bernama{' '}
      {k.code('web')}. Buka {k.link('/editor/demo-crud', 'demo-crud')} untuk melihat contohnya. Demo hanya bisa dibaca;
      kunci editornya adalah namanya sendiri.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Di {k.b('Kode')}, tekan {k.b('+')} di samping daftar file, lalu ketik {k.code('site/index.html')}. Nama dengan
        garis miring menaruh file di dalam folder. Nama dengan ekstensi dianggap sebagai jenis file sesuai ekstensinya.
      </>
    ),
    (k) => (
      <>
        Tambahkan {k.code('site/app.css')} dan {k.code('site/app.js')} dengan cara yang sama. Halaman Anda merujuk
        keduanya dengan nama, seperti {k.code('href="app.css"')}, karena folder itu adalah root dari apa yang disajikan,
        bukan bagian dari alamat.
      </>
    ),
    (k) => (
      <>
        Untuk apa pun yang bukan teks, seperti gambar atau font, buka sebuah file di {k.code('site')}, lalu tekan tombol
        unggah di samping daftar file. File itu akan masuk ke folder yang sama. PNG tidak bisa diketik di editor teks,
        jadi pakai tombol itu.
      </>
    ),
    (k) => <>Di {k.code('lambda.cs')}, sajikan foldernya:</>,
    (k) => (
      <>
        Tekan {k.b('Deploy')}. {k.code('site/index.html')} menjawab di {k.code('/')}, {k.code('site/app.css')} di{' '}
        {k.code('/app.css')}, dan alamat apa pun yang tidak cocok dengan file mana pun dijawab dengan halaman itu. Jadi
        front end yang mengatur routing sendiri tetap jalan saat seseorang me-reload halaman di deep link.
      </>
    ),
    () => <>Tambahkan API di sampingnya, dan halaman itu punya lawan bicara:</>,
  ],

  storage: (k) => (
    <>
      Lambda menyimpan file di dua tempat, dan editor menampilkannya terpisah: {k.b('File')} berisi file dari sebuah
      versi (programnya), dan {k.b('Data')} berisi workspace (apa yang disimpan program itu). Bedanya ada di{' '}
      {k.em('milik siapa')}. File dari sebuah versi milik versi itu; data milik lambda, dan dipakai bersama oleh semua
      versi.
    </>
  ),
  savedWithCode: 'Di sebuah versi',
  workspaceColumn: 'Di data',
  table: [
    ['isinya', 'kode dan aset: programnya, termasuk front end', 'apa pun yang ditulis lambda, atau diunggah seseorang'],
    ['kapan berubah', 'tidak pernah: perubahan menjadi versi baru', 'begitu ada yang ditulis ke dalamnya'],
    ['saat deploy', 'file inilah yang persis dibuat online', 'tidak pernah disentuh'],
    ['rollback', 'file lama kembali', 'tidak berpengaruh: semua versi memakainya bersama'],
    ['draf', 'dimulai sebagai salinannya', 'memakai salinannya'],
    ['kapan hilang', 'bersama versi lama, setelah melewati batas', 'bersama lambda, atau saat Anda menonaktifkannya'],
  ],
  reachedAs: 'diakses dari kode sebagai',
  storageAside:
    'Keduanya tidak bisa jadi satu tempat. Kalau jadi satu, deploy akan menghapus semua yang sudah ditulis lambda sejak deploy sebelumnya, atau tidak ada yang bisa dihapus dari file yang dibawanya. Game yang menyimpan papan peringkat butuh yang kedua; halaman yang disajikannya butuh yang pertama. Jadi halamannya masuk ke versi, dan papan peringkatnya ke data.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} adalah direktori privat yang boleh dibaca dan ditulis lambda Anda. Di situlah tempat untuk
      apa pun yang harus bertahan lebih lama dari satu request, atau satu deployment.
    </>
  ),
  keeping2: (k) => (
    <>
      Ada juga {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, dan {k.code('Tree')}/{k.code('Files')}/{k.code('App')} untuk menyajikannya. Selain itu,
      tidak ada bagian file system yang bisa diakses.
    </>
  ),

  sockets: (k) => (
    <>
      Didukung, dan bukan sekadar tambahan. Demo {k.link('/editor/demo-game', 'demo-game')} memasangkan pemain dan
      menjalankan setiap permainan di server. Bentuk paling sederhananya berupa tiga callback:
    </>
  ),
  socketsAside: (k) => (
    <>
      Satu hal yang sering menjebak: browser tidak bisa menambahkan header pada handshake websocket. Kirim yang
      dibutuhkan handler lewat query, yang dibacanya dari {k.code('connection.Request.Header.Query')}, atau kirim data
      rahasia sebagai pesan pertama.
    </>
  ),

  limits:
    'Kode Anda berjalan di server bersama, jadi sebagian C# ditolak sebelum dikompilasi: menjalankan proses, membuka socket sendiri, memuat assembly, mengakses file system di luar workspace Anda, dan reflection yang dipakai untuk mengakali semua itu.',
  limits2:
    'Semua yang lain tersedia, termasuk seluruh API modul GenHTTP. Kalau ada yang ditolak, Anda diberi tahu baris mana dan alasannya, bukan sekadar pesan gagal.',

  away: (k) => (
    <>
      {k.b('Unduh sebagai proyek .NET')} di editor memberi Anda semuanya: solution yang bisa Anda buka, jalankan dengan{' '}
      {k.code('dotnet run')}, dan simpan. Isinya satu package reference, tanpa jejak platform ini sama sekali.
    </>
  ),
  away2: (k) => (
    <>
      Snippet Anda menjadi isi {k.code('Program.cs')}, dibungkus host yang menyajikan apa yang dikembalikannya. File Anda
      yang lain ikut persis seperti yang Anda tulis. {k.code('Workspace')} dan {k.code('Assets')} menjadi dua folder di
      samping kode, dengan method yang sama, jadi tidak ada yang perlu diubah di kode Anda.
    </>
  ),
  awayAside:
    'Penting diketahui sebelum Anda membangun apa pun di sini: yang Anda tulis adalah milik Anda, dan bisa dibawa keluar utuh. Menjalankannya di server ini tidak membuatnya terkunci di server ini.',

  agents: (k) => (
    <>
      Ada endpoint MCP di {k.code('/mcp')}. Arahkan agen ke sana, dan agen itu bisa melakukan semua yang bisa dilakukan
      editor: membaca panduan, membaca demo secara lengkap, menulis file, mengompilasinya, dan men-deploy. Di baliknya,
      API-nya sama.
    </>
  ),
  agents2: (k) => (
    <>
      Agen mencatat alasannya sambil bekerja ({k.code('write_code')} menerima spesifikasi dan perubahan), dan bisa melihat
      apa yang sudah di-deploy-nya. {k.code('read_logs')} menjawab dengan request terbaru lambda, apa yang dicetaknya, dan
      stack trace dari setiap exception. Dari situ agen tahu kodenya benar-benar jalan, bukan sekadar menebak. Anda
      memantau hal yang sama di pusat kontrol.
    </>
  ),
  more: 'Selengkapnya →',
  make: 'Buat lambda',
};
