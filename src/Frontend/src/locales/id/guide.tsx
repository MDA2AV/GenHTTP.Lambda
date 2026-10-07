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
    written: 'Dokumentasi dan pengujian',
    features: 'Mengubah dengan aman',
    files: 'Kode dan sumber daya',
    page: 'Menyajikan halaman',
    spa: 'Front end, langkah demi langkah',
    built: 'Bahan pembuatnya',
    storage: 'Versi dan datanya',
    database: 'Menyimpan catatan',
    keeping: 'Menyimpan file',
    secrets: 'Kunci dan kata sandi',
    sockets: 'Websocket',
    limits: 'Yang tidak diizinkan',
    away: 'Membawa kode Anda keluar',
    git: 'Mengerjakannya dengan git',
    open: 'Memublikasikan kode',
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
    ['Ringkasan', () => <>Apa aplikasinya, apakah lambda online, berapa request hari ini dan berapa yang gagal, perubahan terakhir, dan sisa ruang penyimpanan.</>],
    ['Dokumentasi', () => <>Apa aplikasinya, untuk siapa dan mengapa, dan mengapa aplikasi dibuat seperti itu - ditulis oleh agen, disimpan bersama setiap versi.</>],
    [
      'Ubah',
      (k) => (
        <>
          Tulis apa yang perlu diubah, dan agen di server ini akan mengerjakannya sementara Anda melihat. Agen mencoba
          perubahan itu di sebuah draf - salinan dengan alamatnya sendiri - lalu menjadikannya online begitu berhasil. Matikan{' '}
          {k.b('Langsung online setelah selesai')} kalau Anda ingin mencoba drafnya sendiri dulu.
          Agen hanya mengerjakan aplikasi Anda: permintaan yang tidak berkaitan dengannya, atau yang bertujuan merugikan, akan ditolak beserta alasannya.
        </>
      ),
    ],
    ['Draf', () => <>Perubahan yang dicoba sebelum dijadikan online, masing-masing di alamat sendiri dan dengan data uji sendiri. Saat dibuka, draf punya kode, data uji, dan log-nya sendiri. Bagian ini muncul setelah ada draf.</>],
    ['Data', () => <>Apa yang disimpan lambda selama berjalan, dipakai bersama oleh semua versi: database, workspace, dan kunci dan kata sandi, masing-masing di tab sendiri. Lihat tabel dan file, unggah file, atur kunci dan kata sandi, atau aktifkan dan nonaktifkan suatu jenis. Tampilan sederhana menampilkannya begitu aplikasi menyimpan sesuatu.</>],
    ['Versi', () => <>Apa yang diubah setiap versi dan apa yang diminta, serta bedanya dengan versi sebelumnya. Deploy atau rollback dari sini, atau mulai draf dari versi mana pun.</>],
    ['Deployment', () => <>Apa yang online dan kapan, dan apa yang membuatnya berhenti.</>],
    ['Statistik', () => <>Request, kegagalan, waktu respons, dan path yang paling sering diminta, selama satu jam atau satu hari terakhir.</>],
    ['Log', () => <>Request yang masuk, apa yang dicetak lambda, dan stack trace dari setiap error, secara langsung.</>],
    [
      'Kode',
      (k) => (
        <>
          Setiap file sebuah versi, kode dan sumber dayanya, dalam pohon di samping editor - beserta ruang yang
          dipakainya dan apakah publik bisa mengaksesnya. Pilih versi yang lebih lama di atasnya untuk membacanya.{' '}
          {k.b('Periksa')} mengompilasi, {k.b('Simpan')} membuat versi, {k.b('Deploy')} menjadikannya online. Di draf,{' '}
          {k.b('Simpan')} menyimpannya di draf dan menampilkannya di alamat draf itu. {k.code('Ctrl-S')} menyimpan;{' '}
          {k.code('F12')} membuka deklarasi.
        </>
      ),
    ],
    ['Pengujian', () => <>Bagaimana aplikasi diuji secara otomatis, dengan script dan data uji untuk itu. Hanya di tampilan lengkap.</>],
  ],
  sections: (k) => (
    <>
      Setiap bagian bekerja dengan cara yang sama: judulnya, tombol {k.b('ⓘ')} yang menjelaskannya, aksinya di kanan,
      dan (kalau punya lebih dari satu tampilan) deretan tab di bawahnya. Tampilan lengkap mengelompokkan bagian-bagiannya:
      cara orang menemukannya, tempat perubahan dibuat, versi dan datanya, dan cara berjalannya.
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
    change: 'Menyimpan entri di database supaya tetap ada setelah restart',
  },
  why2: (k) => (
    <>
      Agen mengirim dua field yang sama ke {k.code('write_code')}. Di {k.b('Kode')}, saat menyimpan Anda diminta
      mengisi perubahannya. Keduanya opsional. Spesifikasi yang terlalu panjang dipotong di 4.000 karakter, dan
      perubahan di 500 karakter, bukan ditolak. Draf menyimpan dua catatannya sendiri, dan versi hasil penggabungannya
      mengambil alih catatan itu.
    </>
  ),

  written: (k) => (
    <>
      Setiap versi menyimpan apa yang ditulis tentangnya di samping programnya: {k.b('dokumentasinya')} (apa
      aplikasinya, untuk siapa dan mengapa, dan mengapa aplikasi dibuat seperti itu) dan {k.b('pengujiannya')}: cara
      memeriksa secara otomatis bahwa aplikasi berfungsi, dengan script dan data uji untuk itu. Agen menulis keduanya
      bersama lambda baru dan memperbaruinya di setiap perubahan. Agen berikutnya yang mengubah lambda membacanya lebih
      dulu, sehingga tahu untuk apa aplikasi itu dan apa yang harus tetap berfungsi - hal yang tidak bisa diketahui dari
      kodenya saja.
    </>
  ),
  writtenFiles: [
    ['docs/product.md', 'apa aplikasinya, untuk siapa, apa yang dilakukan orang dengannya, dan mengapa'],
    ['docs/decisions.md', 'keputusan teknis, dan alasan keputusan itu diambil'],
    ['tests/README.md', 'bagaimana aplikasi diuji secara otomatis, dan cara menjalankan pengujiannya'],
    ['tests/…', 'script dan data uji yang dipakai pengujian'],
  ],
  written2: (k) => (
    <>
      Keduanya adalah file kode versi seperti file lainnya, di folder {k.code('docs')} dan {k.code('tests')}: riwayat
      menunjukkan apa yang diubah sebuah versi di dalamnya, rollback mengembalikan dokumentasi yang berlaku untuk versi
      itu, dan draf punya salinannya sendiri yang ikut online bersamanya. Keduanya tidak pernah dikompilasi dan tidak
      pernah disajikan, dan dihitung dalam batas ukuran sebuah versi.
    </>
  ),
  written3: (k) => (
    <>
      Di pusat kontrol, {k.b('Dokumentasi')} menampilkan halaman untuk dibaca, dan {k.b('Pengujian')} menampilkan cara
      aplikasi diuji beserta file di sampingnya; versinya dipilih seperti untuk file-filenya. Halaman juga bisa diedit
      di sana, yang akan menyimpan versi berikutnya. Tampilan sederhana menyebut dokumentasi {k.b('Tentang')} dan hanya
      menampilkan untuk apa aplikasi itu. Untuk memperbaikinya, sampaikan kepada agen.
    </>
  ),
  writtenAside:
    'Keduanya ditulis dalam bahasa yang Anda pakai dengan agen, untuk siapa pun yang mengubah aplikasi berikutnya - manusia atau agen. Bukan salinan kode, melainkan untuk apa aplikasi itu, dan mengapa.',

  features: (k) => (
    <>
      Versi tidak pernah berubah setelah disimpan, dan justru itulah yang membuat setiap versi layak disimpan: versi
      mana pun bisa dibandingkan, dan dijadikan online lagi persis seperti semula. Untuk mengubah lambda yang sedang
      dipakai orang, coba dulu perubahannya di {k.b('draf')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Mulai dari versi mana pun di {k.b('Versi')}, atau biarkan agen memulainya. Draf adalah salinan kode dan sumber
        daya versi itu, termasuk dokumentasi dan pengujiannya, serta salinan data lambda.
      </>
    ),
    (k) => (
      <>
        Ubah sesering yang diperlukan, di {k.b('Kode')} atau dengan meminta agen. Pratinjaunya menjawab di alamatnya
        sendiri, {k.code('/features/…/')}, dengan data uji miliknya sendiri. Pengunjung lambda tidak melihat apa pun,
        dan tidak ada yang ditulisnya yang sampai ke data lambda.
      </>
    ),
    (k) => (
      <>
        {k.b('Jadikan online')} setelah hasilnya pas: draf menjadi versi berikutnya, lengkap dengan catatannya, dan
        langsung online. Drafnya ikut hilang - termasuk pratinjau dan data ujinya.
      </>
    ),
  ],
  featureSample: 'Papan peringkat',
  featuresAside: () => (
    <>
      Beberapa draf bisa dikerjakan sekaligus. Hanya draf yang sudah terbaru dengan versi terbaru yang bisa dijadikan
      online, supaya tidak pernah membatalkan versi yang disimpan setelah draf dimulai. Jika draf lain lebih dulu
      online, masukkan perubahannya - atau minta agen melakukannya - lalu tandai draf sebagai sudah terbaru. Tidak ada
      yang online dengan sendirinya; itu disengaja. API menyebut draf sebagai feature, dan menjadikannya online
      sebagai merge.
    </>
  ),

  files: (k) => (
    <>
      Sebuah versi terdiri dari file dalam jumlah berapa pun, dalam dua bagian. {k.b('Kode')}-nya adalah setiap file
      kecuali sumber dayanya: file {k.code('.cs')} di bagian atas dikompilasi, dan setiap file lain - di folder mana
      pun, jenis apa pun - disimpan bersama versi dan tidak pernah dikompilasi atau disajikan: dokumentasinya,
      pengujiannya, bahan pembuat front end. {k.b('Sumber daya')}-nya, di {k.code('resources/')}, adalah apa yang
      dibaca dan disajikannya selama berjalan - halaman, script, stylesheet, gambar, migrasi database - dan dijangkau
      dari kode sebagai {k.code('Resources')}.
    </>
  ),
  files2: (k) => (
    <>
      Tipe tidak harus berada di bawah kode yang memakainya. Di {k.b('Kode')}, tekan {k.b('+')} di samping kode, lalu
      ketik sebuah nama: file {k.code('.cs')} di bagian atas dikompilasi bersama snippet, di namespace yang sama, jadi
      tidak perlu import apa pun untuk mengaksesnya. Nama tanpa ekstensi dan tanpa folder dianggap C#.
    </>
  ),
  filesAside:
    'Bagaimana sisa kode ditata terserah penulisnya - satu folder untuk sumber front end, satu lagi untuk script. Kode dan sumber daya sebuah versi berbagi satu jatah ruang, yang ditampilkan di ringkasan.',

  page: 'Ada dua cara untuk menyajikan halaman, dan satu cara lagi untuk file yang diunggah orang di sampingnya.',
  inlineTitle: 'Satu halaman, ditulis inline',
  inline: 'Cocok untuk yang kecil. Halamannya jadi bagian dari snippet.',
  folderTitle: 'Folder berisi file sungguhan',
  folder:
    'Pilihan tepat untuk apa pun yang punya stylesheet dan script. File-nya adalah sumber daya versi, dan disajikan persis seperti yang ditulis. Tidak ada yang mengompilasinya.',
  workspaceTitle: 'File unggahan, dari data',
  workspace:
    'Untuk apa yang diunggah orang atau dibuat lambda (gambar, dokumen), disajikan di samping aplikasinya. Bukan untuk halaman aplikasinya sendiri: halaman itu tempatnya di sumber daya, di mana halaman ikut masuk versi bersama kode yang membutuhkannya.',

  spa: (k) => (
    <>
      Cara kedua, secara lengkap. Setiap demo menyajikan halamannya dengan cara ini, dari {k.code('resources/web')}.
      Buka {k.link('/editor/demo-crud', 'demo-crud')} untuk melihat contohnya. Demo hanya bisa dibaca;
      kunci editornya adalah namanya sendiri.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Di {k.b('Kode')}, tekan {k.b('+')} di samping sumber daya, lalu ketik {k.code('site/index.html')}: file itu
        menjadi {k.code('resources/site/index.html')}. Nama dengan garis miring menaruh file di dalam folder. Nama dengan
        ekstensi dianggap sebagai jenis file sesuai ekstensinya.
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
        unggah di samping sumber daya. File itu akan masuk ke folder yang sama. PNG tidak bisa diketik di editor teks,
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
  built: (k) => (
    <>
      Sebagian lambda mungkin dibuat oleh alat build, bukan ditulis apa adanya untuk disajikan atau dikompilasi:
      dikompilasi, digabungkan, atau dihasilkan. Versi menyimpan hasil buatan alat itu - sebagai sumber dayanya, atau
      sebagai kodenya - dan file bahan pembuatnya adalah bagian dari kodenya, di folder tersendiri: misalnya{' '}
      {k.code('frontend/')}, dengan README yang menjelaskan cara membangunnya. Agen Anda mengubah file-file tersebut,
      menjalankan build di tempatnya bekerja, dan menyimpan keduanya di versi yang sama. Platform ini tidak membangun
      apa pun.
    </>
  ),
  built2: (k) => (
    <>
      Seperti dokumentasi, semuanya milik versinya: dibandingkan di riwayat, dikembalikan, disalin ke draf, dikloning,
      diunduh, dan dipublikasikan bersama kode lainnya - dan tidak pernah dikompilasi atau disajikan. Di pusat kontrol,
      semuanya ada di {k.b('Kode')}, bersama setiap file lain dari versi itu.
    </>
  ),
  builtAside:
    'Yang ditulis apa adanya untuk disajikan atau dikompilasi tidak membutuhkannya. Apa yang dipasang atau disimpan sebuah build untuk dirinya sendiri - node_modules, misalnya - tidak pernah menjadi bagian dari versi: sebuah .gitignore di foldernya menjauhkannya.',

  storage: (k) => (
    <>
      Lambda menyimpan file di dua tempat, dan editor menampilkannya terpisah: {k.b('Kode')} berisi file dari sebuah
      versi (programnya), dan {k.b('Data')} berisi workspace (apa yang disimpan program itu). Bedanya ada di{' '}
      {k.em('milik siapa')}. File dari sebuah versi milik versi itu; data milik lambda, dan dipakai bersama oleh semua
      versi. Masing-masing punya satu jatah ruang: kode dan sumber daya sebuah versi berbagi satu, dan database serta
      workspace lambda berbagi yang lain.
    </>
  ),
  savedWithCode: 'Di sebuah versi',
  workspaceColumn: 'Di data',
  table: [
    ['isinya', 'kode dan sumber daya: programnya, termasuk front end - beserta dokumentasi, pengujian, dan apa pun bahan pembuatnya', 'apa pun yang ditulis lambda, atau diunggah seseorang'],
    ['kapan berubah', 'tidak pernah: perubahan menjadi versi baru', 'begitu ada yang ditulis ke dalamnya'],
    ['saat deploy', 'file inilah yang persis dibuat online', 'tidak pernah disentuh'],
    ['rollback', 'file lama kembali', 'tidak berpengaruh: semua versi memakainya bersama'],
    ['draf', 'dimulai sebagai salinannya', 'memakai salinannya'],
    ['kapan hilang', 'bersama versi lama, setelah melewati batas', 'bersama lambda, atau saat Anda menonaktifkannya'],
  ],
  reachedAs: 'diakses dari kode sebagai',
  storageAside:
    'Keduanya tidak bisa jadi satu tempat. Kalau jadi satu, deploy akan menghapus semua yang sudah ditulis lambda sejak deploy sebelumnya, atau tidak ada yang bisa dihapus dari file yang dibawanya. Game yang menyimpan papan peringkat butuh yang kedua; halaman yang disajikannya butuh yang pertama. Jadi halamannya masuk ke versi, dan papan peringkatnya ke data.',

  database: (k) => (
    <>
      Catatan – entri, akun, pesanan, hasil voting – tempatnya di {k.b('database')}: database SQLite milik lambda itu
      sendiri, yang diaktifkan di {k.b('Data')}. Kode membuka koneksi dengan {k.code('Database.GetConnection()')} lalu
      membaca dan menulis datanya melalui {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, dengan
      konteks miliknya sendiri yang memetakan tabel-tabelnya:
    </>
  ),
  database2: (k) => (
    <>
      Tabelnya dibuat oleh {k.b('migrasi')}: file SQL yang dibawa versi di {k.code('resources/migrations/')}, diterapkan
      berurutan oleh {k.link('https://evolve-db.netlify.app/', 'Evolve')} saat lambda dimulai – masing-masing sekali
      saja, jadi versi baru hanya menjalankan yang baru. Jangan pernah mengubah migrasi yang sudah diterapkan; perubahan
      pada tabel dibuat sebagai file berikutnya.
    </>
  ),
  database3: (k) => (
    <>
      Seperti semua data, database dipakai bersama oleh semua versi, tidak disentuh oleh deploy maupun rollback, dan draf
      bekerja pada salinannya. Di {k.b('Data')} Anda bisa melihat tabel dan isinya – tampilan sederhana menyebutnya
      catatan. {k.b('Unduh sebagai proyek .NET')} menyertakannya sebagai file SQLite biasa.
    </>
  ),
  databaseAside: (k) => (
    <>
      Buat konteks di tempat Anda membutuhkannya, lalu dispose setelah selesai, dan pakai secara sinkron –{' '}
      {k.code('ToList')} dan {k.code('SaveChanges')}, bukan {k.code('ToListAsync')} dan {k.code('SaveChangesAsync')}.
      Tabel dibuat oleh migrasi, tidak pernah oleh Entity Framework. Demo {k.link('/editor/demo-crud', 'demo-crud')}{' '}
      melakukan semua ini.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} adalah direktori privat yang boleh dibaca dan ditulis lambda Anda: tempat untuk file –
      gambar yang diunggah seseorang, dokumen yang dibuatnya, model yang dimuatnya. Catatan tempatnya di database, dan
      apa yang diketahui tentang sebuah file – siapa yang mengunggahnya, kapan – juga termasuk catatan.
    </>
  ),
  keeping2: (k) => (
    <>
      Ada juga {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, dan {k.code('Tree')}/{k.code('Files')}/{k.code('App')} untuk menyajikannya. Selain itu,
      tidak ada bagian file system yang bisa diakses.
    </>
  ),

  secrets: (k) => (
    <>
      Kunci API, kata sandi, atau token tempatnya di {k.b('rahasia')}, bukan di kode – di sana setiap versi, setiap
      unduhan, dan siapa pun yang membaca riwayatnya akan memilikinya. Kode membaca rahasia berdasarkan namanya:
    </>
  ),
  secrets2: (k) => (
    <>
      Aktifkan rahasia di {k.b('Data')} dan atur nilainya di sana. Setelah disimpan, nilainya tidak pernah ditampilkan
      lagi – tidak kepada Anda, tidak kepada agen; Anda hanya bisa menggantinya. Daftarnya menunjukkan nama yang dibaca
      kode tetapi belum punya nilai, dan ikhtisar memintanya. {k.code('Secret.Exists')} memberi tahu apakah rahasia
      sudah diatur, untuk kode yang tetap berjalan tanpanya. Seperti semua data, rahasia dipakai bersama oleh semua
      versi, dan draf bekerja pada salinannya.
    </>
  ),
  secretsAside: (k) => (
    <>
      Rahasia disimpan terenkripsi, dengan kunci yang tidak ada di database. Di proyek yang diunduh,{' '}
      {k.code('Secret.Read("NAME")')} membaca variabel lingkungan {k.code('NAME')} – nilainya sendiri tetap di sini.
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
  sockets2: (k) => (
    <>
      Jika halaman hanya mendengarkan (jumlah, umpan, papan skor), server-sent events lebih sederhana: satu respons
      panjang yang terus ditulis oleh server dan tersambung kembali dengan sendirinya oleh browser. Demo{' '}
      {k.link('/editor/demo-live', 'demo-live')} mengirim setiap suara kepada semua orang yang menonton dengan cara
      ini. Dengan cara mana pun, server mendorong apa yang berubah. Halaman yang bertanya lagi setiap beberapa detik
      mengirim request setiap kali, entah ada yang berubah atau tidak, dan tetap terlambat.
    </>
  ),

  limits:
    'Kode Anda berjalan di server bersama, jadi sebagian C# ditolak sebelum dikompilasi: menjalankan proses, membuka socket sendiri, memuat assembly, mengakses file system di luar workspace Anda, dan reflection yang dipakai untuk mengakali semua itu. Begitu pula menunggu task dengan .Result atau .Wait() alih-alih await: request berjalan di satu thread per core, dan task tersebut harus selesai justru di thread yang sedang menunggunya.',
  limits2:
    'Semua yang lain tersedia, termasuk seluruh API modul GenHTTP. Kalau ada yang ditolak, Anda diberi tahu baris mana dan alasannya, bukan sekadar pesan gagal.',

  away: (k) => (
    <>
      {k.b('Unduh sebagai proyek .NET')} di editor memberi Anda semuanya: solution yang bisa Anda buka, jalankan dengan{' '}
      {k.code('dotnet run')}, dan simpan. Ia hanya membutuhkan paket GenHTTP, dan disertai {k.code('Dockerfile')}{' '}
      untuk membangun dan menjalankannya sebagai container.
    </>
  ),
  away2: (k) => (
    <>
      Snippet Anda menjadi {k.code('Project.cs')}, dan {k.code('Program.cs')} menyajikan apa yang dikembalikannya. File
      Anda yang lain ikut persis seperti yang Anda tulis, di tempatnya semula: sumber daya di {k.code('resources')},
      dokumentasi di {k.code('docs')}, pengujian di {k.code('tests')}. {k.code('Workspace')} dan{' '}
      {k.code('Resources')} menjadi dua folder di samping program, dengan method yang sama, terpisah di folder{' '}
      {k.code('Platform')}, jadi tidak ada yang perlu diubah di kode Anda.
      {' '}{k.code('Secret')} di sana membaca variabel lingkungan dengan nama yang sama; nilainya tetap di sini.
      {' '}{k.code('Database')} membuka {k.code('database/database.db')}, yang ikut terunduh bersama catatan yang
      disimpan aplikasi Anda.
    </>
  ),
  awayAside:
    'Penting diketahui sebelum Anda membangun apa pun di sini: yang Anda tulis adalah milik Anda, dan bisa dibawa keluar utuh. Menjalankannya di server ini tidak membuatnya terkunci di server ini.',
  git: (k) => (
    <>
      Setiap lambda juga merupakan repositori git. {k.b('Kloning')}, di ringkasan pusat kontrol dan di samping kodenya,
      memuat alamatnya - alamat editor Anda diikuti nama aplikasi - dan {k.code('git clone')} memberi Anda proyek yang
      sama dengan yang diberikan {k.b('Unduh')}, dengan setiap versi sebagai commit di {k.code('main')}, diberi tag{' '}
      {k.code('v1')}, {k.code('v2')}, dan seterusnya, dan setiap draf sebagai branch. Buka di editor Anda sendiri,
      serahkan ke agen coding Anda, jalankan dengan {k.code('dotnet run')}.
    </>
  ),
  git2: (k) => (
    <>
      Push, dan langsung ada di sini. Setiap commit yang di-push ke {k.code('main')} menjadi versi berikutnya, baris
      pertamanya sebagai perubahan yang dibuatnya - dikompilasi dulu, dan ditolak jika tidak bisa dikompilasi - dan{' '}
      {k.code('git push -o deploy')} menjadikannya online. Branch yang Anda push menjadi draf, dengan pratinjau online di
      alamatnya sendiri; push ke {k.code('main')}, atau tambahkan {k.code('-o merge')} pada push terakhirnya, dan ia
      menjadi versi berikutnya. Apa yang ditambahkan platform di sekitar kode Anda agar menjadi proyek -{' '}
      {k.code('Program.cs')}, file proyek, {k.code('Platform')} - bukan bagian dari aplikasi Anda, jadi push yang
      mengubahnya ditolak disertai alasannya. {k.code('AGENTS.md')} di dalam repositori memberi tahu agen coding
      sisanya.
    </>
  ),
  gitAside:
    'Alamat ini memuat kunci editor Anda, seperti alamat editor: siapa pun yang memilikinya bisa melakukan push. Apa yang disimpan aplikasi Anda - catatan, file, kunci dan kata sandinya - tidak pernah ada di repositori.',

  open: (k) => (
    <>
      Kalau yang Anda buat bisa berguna bagi orang lain, publikasikan kodenya: buka {k.b('Open Source')} di pusat
      kontrol, pilih lisensi - MIT, kecuali Anda menginginkan yang lain - lalu aktifkan. Kodenya mendapat halaman
      sendiri di antara {k.link('/source', 'aplikasi Open Source')}, tempat siapa pun bisa membacanya, memberinya
      bintang, mengunduh versi mana pun sebagai proyek yang sama dengan yang Anda dapat dari {k.b('Unduh')}, lengkap
      dengan lisensinya, atau mengkloning setiap versinya dengan git.
    </>
  ),
  open2: () => (
    <>
      Setiap versi dipublikasikan, termasuk yang lebih lama, beserta seluruh kodenya - dokumentasi dan pengujiannya
      termasuk di dalamnya - dan perubahan yang dibuat masing-masing. Apa yang disimpan aplikasi tidak pernah
      dipublikasikan - catatannya, file yang disimpannya, nilai kunci dan kata sandinya - begitu pula apa yang Anda minta
      dengan kata-kata Anda sendiri, atau siapa yang memakai aplikasinya. Nonaktifkan, dan halamannya hilang; bintangnya
      tetap disimpan untuk saat Anda memublikasikannya lagi.
    </>
  ),
  openAside:
    'Semua yang ada di kode menjadi publik, termasuk versi-versi sebelumnya. Kunci atau kata sandi tempatnya di kunci dan kata sandi di bagian Data, tidak pernah di kode - dipublikasikan atau tidak.',

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
      memantau hal yang sama di pusat kontrol. Agen menulis dokumentasi dan pengujian sambil bekerja, membacanya
      sebelum mengubah apa pun, dan menjalankan pengujian terhadap alamat draf sebelum menjadikan draf itu online.
      Halaman yang dibuat untuk ditemukan orang diberi judul, deskripsi, ikon, dan pratinjau yang muncul saat link-nya
      dibagikan. Di bagian bawah halaman yang dibuatnya, agen menambahkan satu baris kecil yang menyatakan halaman itu
      dibuat dengan GenHTTP Lambda. Katakan kepadanya jika Anda tidak menginginkannya, lalu agen akan menghapusnya.
    </>
  ),
  more: 'Selengkapnya →',
  make: 'Buat lambda',
};
