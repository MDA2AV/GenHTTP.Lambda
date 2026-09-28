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
        Berikan kunci editor ke agen dan jelaskan apa yang harus dibuat. Agen menulis versi baru lewat{' '}
        {k.link('/#agents', 'MCP')}. Atau buka {k.b('Kode')} dan tulis sendiri: {k.b('Periksa')} mengompilasi tanpa
        menyimpan apa pun, lalu menunjukkan pesan compiler, lengkap dengan file dan barisnya.
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
      kalau ada versi baru yang menunggu untuk online) beserta bagian-bagiannya. Hal yang jarang dilakukan, seperti
      mengganti alamat atau menghapus lambda, ada di menu {k.b('⋯')} di sana.
    </>
  ),
  bits: [
    ['Ringkasan', () => <>Apakah lambda online, berapa request hari ini dan berapa yang gagal, perubahan terakhir, dan sisa ruang penyimpanan.</>],
    [
      'Ubah',
      (k) => (
        <>
          Tulis apa yang perlu diubah, dan agen di server ini akan mengerjakannya sementara Anda melihat: membaca kode,
          mengubahnya, memastikan kodenya bisa dikompilasi, lalu membuatnya online sebagai versi baru. Matikan{' '}
          {k.b('Langsung online setelah selesai')} kalau Anda ingin memeriksanya dulu.
        </>
      ),
    ],
    ['File', () => <>File dari sebuah versi, beserta datanya: apa yang disimpan lambda selama berjalan. Ikon gembok atau globe menunjukkan apakah publik bisa mengaksesnya.</>],
    ['Versi', () => <>Apa yang diubah setiap versi dan apa yang diminta, serta bedanya dengan versi sebelumnya. Deploy atau rollback dari sini.</>],
    ['Deployment', () => <>Apa yang online dan kapan, dan apa yang membuatnya berhenti.</>],
    ['Statistik', () => <>Request, kegagalan, waktu respons, dan path yang paling sering diminta, selama satu jam atau satu hari terakhir.</>],
    ['Log', () => <>Request yang masuk, apa yang dicetak lambda, dan stack trace dari setiap error, secara langsung.</>],
    [
      'Kode',
      (k) => (
        <>
          Menulis kode secara manual. {k.b('Periksa')} mengompilasi, {k.b('Simpan')} membuat versi, {k.b('Deploy')}{' '}
          membuatnya online. {k.code('Ctrl-S')} menyimpan; {k.code('F12')} membuka deklarasi.
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
      Agen mengirim dua field yang sama ke {k.code('write_code')}. Di {k.b('Kode')}, Anda akan diminta mengisi
      perubahannya saat menyimpan. Keduanya opsional. Spesifikasi yang terlalu panjang dipotong di 4.000 karakter, dan
      perubahan di 500 karakter, bukan ditolak.
    </>
  ),

  files: (k) => (
    <>
      Tipe tidak harus berada di bawah kode yang memakainya. Di {k.b('Kode')}, tekan {k.b('+')} di samping daftar file,
      dan file baru itu dikompilasi bersama snippet, di namespace yang sama, jadi tidak perlu import apa pun untuk
      mengaksesnya. Nama tanpa ekstensi dianggap C#.
    </>
  ),

  page: 'Ada tiga cara, dan pilihannya tergantung di mana halamannya berada.',
  inlineTitle: 'Satu halaman, ditulis inline',
  inline: 'Cocok untuk yang kecil. Halamannya jadi bagian dari snippet.',
  folderTitle: 'Folder berisi file sungguhan',
  folder:
    'Pilihan tepat untuk apa pun yang punya stylesheet dan script. File ditambahkan dengan cara yang sama seperti file C#, dan disajikan persis seperti yang ditulis. Tidak ada yang mengompilasinya.',
  workspaceTitle: 'Dari workspace',
  workspace: 'Kalau halamannya diunggah, bukan ditulis, dan harus bisa diubah tanpa deploy ulang.',

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
      Bagian {k.b('File')} menampilkan keduanya (file dari sebuah versi, dan workspace sebagai {k.b('Data')}) dan
      menunjukkan mana yang bisa diakses publik. File kode diubah di {k.b('Kode')}; data bisa diunggah dan dihapus di{' '}
      {k.b('File')}. Tapi keduanya bukan hal yang sama, dan bedanya ada di {k.em('kapan masing-masing berubah')}.
    </>
  ),
  savedWithCode: 'Disimpan bersama kode',
  workspaceColumn: 'Workspace',
  table: [
    ['isinya', 'semua file lambda Anda, termasuk C#-nya', 'apa pun yang sudah ditulis atau diunggah'],
    ['kapan berubah', 'saat Anda menekan Simpan atau Deploy', 'begitu ada yang ditulis ke dalamnya'],
    ['saat deploy', 'diganti seluruhnya', 'tidak pernah disentuh'],
    ['rollback versi', 'file lama kembali', 'tidak berpengaruh'],
    ['clone lambda', 'ikut disalin', 'tidak ikut'],
  ],
  reachedAs: 'diakses dari kode sebagai',
  storageAside:
    'Keduanya tidak bisa jadi satu direktori. Kalau jadi satu, deploy akan menghapus semua yang sudah ditulis lambda sejak deploy sebelumnya, atau tidak ada yang bisa dihapus dari file yang dibawanya. Game yang menyimpan papan peringkat butuh yang kedua; halaman yang disajikannya butuh yang pertama.',

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
