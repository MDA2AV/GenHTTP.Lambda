import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'AI pembuat aplikasi + hosting',
  title: 'Membuatnya cuma semenit.',
  titleAccent: 'Selebihnya sudah tersedia.',
  intro:
    'Ceritakan aplikasi Anda kepada agen AI kami - atau kepada Claude Code, Codex atau Cursor - dan dalam hitungan menit aplikasinya sudah online, di-hosting untuk Anda, lengkap dengan link untuk dibagikan. Semua yang membuatnya tetap berguna sudah tersedia: catatannya, versinya dan tujuannya, siapa yang mengunjunginya, serta domain sendiri begitu aplikasinya mulai ramai.',
  build: 'Buat sesuatu',
  ownAgent: 'Pakai agen Anda sendiri',
  free: 'Gratis. Tanpa daftar, tanpa kartu kredit.',
  heroAlt: 'Editor aplikasi pemesanan lapangan: online di domain milik klub, berapa orang yang memakainya hari ini, dan perubahan terbarunya',
  example: 'Semua gambar di halaman ini menampilkan aplikasi contoh yang sama: pemesanan lapangan sebuah klub tenis.',

  contents: 'Di halaman ini',

  makeTitle: 'Buat, lalu ubah',
  makeText: 'Katakan apa yang Anda butuhkan, dan terus katakan. Tidak perlu menulis atau membaca kode untuk keduanya.',

  behindTitle: 'Database, riwayat versi, dan analitik untuk setiap aplikasi',
  behindText:
    'Yang membuat aplikasi tetap berguna adalah semua yang ada di sekitar kodenya - tersedia untuk siapa pun yang membuatnya, bukan hanya yang bisa membaca kode.',

  yoursTitle: 'Bisa Anda kembangkan sendiri',
  yoursText: 'Untuk developer, dan siapa pun yang ingin aplikasinya tetap hidup di luar situs ini.',

  features: {
    build: {
      name: 'Buat',
      title: 'Jelaskan, lalu langsung online',
      text: 'Katakan kepada agen kami apa yang Anda butuhkan, dengan kata-kata Anda sendiri - atau minta Claude Code, Codex, atau Cursor. Beberapa menit kemudian aplikasinya berjalan di alamat sendiri, siap dibagikan. Tidak ada server, database, atau akun yang perlu disiapkan.',
      points: ['Agen kami di halaman buat, atau agen Anda sendiri lewat MCP', 'Link untuk dibagikan dalam hitungan menit', 'Gratis, tanpa daftar'],
      alt: 'Halaman pemesanan lapangan: empat lapangan, jam demi jam, dengan jam yang sudah dipesan lengkap dengan nama pemesannya',
    },
    change: {
      name: 'Ubah',
      title: 'Tanyakan, lalu ubah',
      text: 'Aplikasi Anda tetap tercantum di halaman build, masing-masing dengan percakapannya. Tanyakan keadaannya - siapa yang berkunjung, apa yang diisi orang, apakah ada yang salah - atau katakan apa yang harus berbeda, lalu agen menjawab atau mengubahnya. Atau berikan link editornya ke agen Anda sendiri.',
      points: ['Jawaban tentang pengunjung, catatan, dan error', 'Online sendiri begitu berfungsi, atau disimpan sebagai draf agar Anda coba dulu', 'Kodenya tidak terlihat kecuali Anda mau'],
      alt: 'Halaman build: klub bertanya berapa lapangan yang dipesan minggu ini, agen menjawab, dan pelajaran dari pelatih siap dicoba dalam sebuah draf',
    },
    drafts: {
      name: 'Draf',
      title: 'Coba perubahan sebelum ada yang melihatnya',
      text: 'Perubahan yang lebih besar dikerjakan di draf: aplikasi seperti yang akan jadi, di alamat sendiri, dengan salinan catatan untuk mencobanya. Apa pun yang dilakukan di sana tidak menyentuh catatan yang asli. Kalau sudah benar, satu klik menjadikannya online.',
      points: ['Alamat sendiri, untuk dicoba dan ditunjukkan ke orang lain', 'Data uji: salinan catatan, bukan catatan aslinya', 'Jadikan online kalau sudah benar, atau buang'],
      alt: 'Draf halaman pemesanan, tempat pelajaran dari pelatih tampil berwarna ungu - di salinan data pemesanan',
    },
    history: {
      name: 'Riwayat',
      title: 'Setiap perubahan jadi versi, dengan jalan kembali',
      text: 'Setiap perubahan menjadi versi baru, yang dijelaskan lewat apa yang diubahnya. Kalau ada yang salah, versi sebelumnya hanya satu klik jauhnya - di alamat yang sama, dengan catatan yang sama.',
      points: ['Apa yang diminta, dan apa yang berubah', 'Kembali ke versi sebelumnya dengan satu klik', 'Catatan dipakai bersama oleh semua versi, jadi kembali tidak menghilangkan apa pun'],
      alt: 'Riwayat aplikasi: perubahan-perubahannya, berurutan dari atas ke bawah, yang terbaru sudah online',
    },
    about: {
      name: 'Tentang',
      title: 'Aplikasi tahu dirinya sendiri',
      text: 'Setiap kali ada perubahan, agen menuliskan untuk apa aplikasinya, siapa yang memakainya, dan mengapa ia bekerja seperti itu. Anda membacanya dengan bahasa sehari-hari, dan agen berikutnya membacanya sebelum mengubah apa pun - jadi perubahan dibangun di atas yang sudah ada, bukan berdasarkan tebakan.',
      points: ['Apa fungsinya dan untuk siapa, dengan bahasa sederhana', 'Keputusan di baliknya, untuk siapa pun yang mengubahnya berikutnya', 'Cara mengujinya, disimpan bersama setiap versi'],
      alt: 'Tentang aplikasi Anda: pemesanan lapangan dijelaskan dengan bahasa sederhana, siapa yang memakainya dan apa fungsinya',
    },
    records: {
      name: 'Catatan',
      title: 'Catatan di database sendiri',
      text: 'Aplikasi yang menyimpan sesuatu - pemesanan, pendaftaran, skor - menyimpannya di database sendiri, dan semuanya tetap ada saat aplikasinya berubah. Baca tabel demi tabel di editor, di samping file yang disimpannya, seperti daftar mingguan untuk papan pengumuman.',
      points: ['Database untuk setiap aplikasi yang membutuhkannya', 'Setiap tabel, bisa dibaca di editor', 'File yang disimpan aplikasi, ditampilkan di sampingnya'],
      alt: 'Catatan aplikasi: data pemesanan dan anggotanya, dibaca tabel demi tabel',
    },
    keys: {
      name: 'Kunci dan kata sandi',
      title: 'Kunci dan kata sandi tetap rahasia',
      text: 'Kalau aplikasi butuh kata sandi atau kunci ke layanan lain, agen hanya memintanya berdasarkan nama. Anda mengetik nilainya di editor, dan sejak itu tidak ada yang melihatnya - bukan Anda, bukan agen. Hanya aplikasi yang membacanya.',
      points: ['Agen tidak pernah melihat nilainya', 'Disimpan terenkripsi, tidak pernah ditampilkan lagi', 'Bisa diganti kapan pun Anda mau'],
      alt: 'Kunci dan kata sandi: kata sandi meja depan sudah diatur, dan nilainya disembunyikan',
    },
    analytics: {
      name: 'Analitik',
      title: 'Lihat siapa yang berkunjung, tanpa cookie',
      text: 'Aktifkan analitik dan lihat berapa orang yang membuka aplikasi, dari mana asal mereka, dan di perangkat apa - dihitung di server kami, tanpa cookie dan tanpa skrip di halaman Anda.',
      points: ['Pengunjung, tampilan halaman, dan asal mereka', 'Tanpa cookie, dan tidak ada yang menyebut identitas pengunjung', 'Mati sampai Anda mengaktifkannya'],
      alt: 'Analitik: pengunjung dalam tiga puluh hari terakhir, hari demi hari',
    },
    domain: {
      name: 'Domain',
      title: 'Domain sendiri',
      text: 'Setiap aplikasi dimulai di alamat dari kami. Kalau sudah populer, aplikasinya bisa pindah ke domain milik Anda: aplikasi dan editor yang sama, di alamat yang mudah diingat. Link ke alamat lama tetap berfungsi - semuanya diarahkan ke yang baru.',
      points: ['Untuk aplikasi Premium: hubungi kami', 'Aplikasi, editor, dan catatan yang sama', 'Alamat lama meneruskan pengunjung'],
      alt: 'Alamat aplikasi pindah dari alamatnya di genhttp.run ke domain milik klub',
    },
    code: {
      name: 'Kode dan git',
      title: 'Kodenya, kapan pun Anda mau',
      text: 'Alihkan editor ke tampilan lengkap, dan kode setiap versi ada di sana, beserta lognya dan angka-angkanya. Kloning aplikasi dengan git lalu push perubahan, atau unduh sebagai proyek .NET yang berjalan di mana saja. Itu kode Anda.',
      points: ['Setiap file, dibandingkan dari versi ke versi', 'Git: commit ke main menjadi versi, branch menjadi draf', 'Unduhan yang berjalan sendiri, lengkap dengan catatannya'],
      alt: 'Tampilan lengkap editor: file-file aplikasi dan kodenya',
    },
    publish: {
      name: 'Showcase dan source',
      title: 'Tampilkan, atau publikasikan kodenya',
      text: 'Masukkan aplikasi ke showcase agar orang lain bisa menemukannya, atau publikasikan kodenya dengan lisensi open source agar siapa pun bisa membaca, memberi bintang, dan mengunduhnya. Keduanya pilihan Anda, dan tidak ada yang membagikan catatannya.',
      points: ['Di showcase, dengan gambar dan deskripsi', 'Kodenya dengan lisensi MIT atau lisensi lain', 'Tidak pernah catatan, kunci, atau pengunjungnya'],
      alt: 'Kode pemesanan lapangan yang dipublikasikan: file, dokumentasi, dan versinya, dengan lisensi MIT',
    },
  },

  draftBadge: 'Draf',
  domainOwn: 'Premium',
  domainSubject: 'Domain untuk aplikasi saya',
  domainAsk: 'Tanya soal domain',
  zoom: 'Perbesar',
  close: 'Tutup',

  closeTitle: 'Gratis untuk dicoba.',
  closeAccent: 'Tanpa daftar.',
  closeBuild: 'Buat sesuatu',
  closeShip: 'Publikasikan hasil buatan Anda',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda adalah open source. Pakai di sini, atau {selfHosting('jalankan di server Anda sendiri')} - kodenya ada di{' '}
      {github('GitHub')}.
    </>
  ),
};
