import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Kebijakan privasi',
  binding: (english) => (
    <>Terjemahan ini hanya untuk informasi. Yang berlaku secara hukum adalah {english('versi bahasa Inggris')}.</>
  ),
  intro:
    'Apa yang diketahui situs ini tentang Anda, apa yang dilakukannya dengan informasi itu, berapa lama informasi itu disimpan, dan siapa lagi yang bisa melihatnya. Singkatnya: tidak ada akun, tidak ada iklan, dan tidak ada pelacakan. Server mencatat siapa meminta apa, supaya server tetap berjalan dan penyalahgunaan bisa dilacak. Apa yang Anda minta dari agen build dikirim ke Anthropic, karena model Anthropic-lah yang menulis aplikasinya.',
  sections: {
    whoTitle: 'Siapa yang bertanggung jawab',
    who: 'Situs ini dijalankan oleh orang di bawah ini. Dialah yang bertanggung jawab atas data pribadi yang diproses situs ini menurut Peraturan Perlindungan Data Umum Uni Eropa (GDPR). Untuk apa pun yang ada di halaman ini, hubungi alamat berikut:',

    requestsTitle: 'Yang dicatat server di setiap permintaan',
    requests: [
      'Setiap permintaan ke situs ini, dan ke setiap lambda yang di-host di sini, ditulis ke log server: alamat IP asalnya, alamat IP yang disebut sebagai asal sebenarnya kalau permintaan itu diteruskan, browser atau program yang mengirimnya, alamat yang diminta, waktunya, dan bagaimana permintaan itu dijawab. Server juga mencari negara, kota, dan jaringan tempat alamat IP itu berada, di database yang disimpannya sendiri. Tidak ada pihak lain yang ditanya.',
      'Dengan cara inilah gangguan ditemukan, server yang kelebihan beban dilacak ke penyebabnya, dan penyalahgunaan yang dilaporkan kepada kami ditelusuri. Alamat IP juga dipakai, hanya di memori, untuk membatasi berapa banyak permintaan dan build yang bisa dibuat satu pengunjung. Permintaan tidak bisa dijawab tanpa data ini. Dasar hukumnya adalah kepentingan yang sah dari kami untuk menjalankan layanan ini dan menjaganya tetap aman (Pasal 6 ayat (1) huruf f GDPR).',
      'Administrator bisa membaca semuanya. Pemilik lambda bisa melihat negara dan browser dari setiap permintaan ke lambda miliknya, tapi tidak bisa melihat alamat IP-nya.',
    ],

    logsTitle: 'Berapa lama log disimpan',
    logs: 'Log disimpan di dua tempat: di memori server, yang dikosongkan setiap kali server dimulai ulang, dan di output konsol server, yang dihapus setiap kali server diperbarui. Ukuran keduanya tetap, jadi setiap baris baru menggeser baris yang paling lama. Berapa lama satu baris bertahan tergantung seberapa sibuk situs ini. Tidak ada isi log yang disalin ke arsip.',

    contentTitle: 'Yang Anda taruh di sini',
    content: (days) =>
      `Lambda terdiri dari kodenya, file-filenya, pengaturannya, dan catatan yang disimpan bersama setiap versinya tentang apa yang diminta dan apa yang diubah. Semuanya disimpan di server supaya bisa dijalankan dan diedit. Lambda gratis dihapus beserta semua versinya sekitar ${days} hari setelah terakhir kali diubah atau dikunjungi, dan langsung dihapus kalau pemegang link editornya menghapusnya. Siapa pun yang punya link editor bisa membaca semuanya, apa yang Anda pasang di showcase bisa dilihat semua orang, dan administrator melihat isi lambda kalau memang perlu, untuk menangani laporan atau menjaga server tetap aman. Dasar hukumnya adalah penyediaan layanan yang Anda minta (Pasal 6 ayat (1) huruf b GDPR).`,

    agentTitle: 'Yang Anda minta dari agen build',
    agent: (policy) => (
      <>
        Apa yang Anda ketik di kotak build, atau di bagian “Ubah” pada editor sebuah lambda, dikirim ke Anthropic PBC di
        Amerika Serikat, yang menjalankan Claude, model yang menulis aplikasinya. Untuk membuat perubahan, agen juga
        membaca lambda itu: kodenya, catatan pada versi-versinya, dan log-nya, yang berisi request ke lambda itu dan apa
        yang dicetaknya, tetapi tidak berisi alamat IP pengunjungnya. Apa yang dibaca agen juga dikirim ke sana. Apa yang
        dilakukan Anthropic dengan data itu diatur oleh {policy('kebijakan privasinya sendiri')}. Perlindungan data
        pribadi di Amerika Serikat tidak setara dengan di Uni Eropa. Permintaan Anda dikirim ke sana karena itu diperlukan
        untuk membuat atau mengubah apa yang Anda minta (Pasal 6 ayat (1) huruf b dan Pasal 49 ayat (1) huruf b GDPR).
        Jadi, jangan masukkan apa pun yang tidak ingin Anda bagikan.
      </>
    ),
    agentKept:
      'Agen menyimpan permintaan Anda, sering kali dengan kata-katanya sendiri, sebagai catatan di versi yang ditulisnya. Permintaan itu sendiri dicatat utuh di log server, dan beberapa ratus karakter pertamanya di log layanan build; ukuran keduanya juga tetap. Kalau Anda memakai agen Anda sendiri, seperti Claude atau Claude Code, apa yang Anda katakan kepadanya dikirim ke penyedia agen itu, bukan ke kami. Kami hanya menerima kode dan catatan yang dikirim agen itu ke sini.',

    lambdasTitle: 'Apa yang dilakukan lambda ditentukan pemiliknya',
    lambdas:
      'Lambda ditulis oleh pemegang link editornya, bukan oleh kami. Apa yang diminta lambda dari pengunjungnya dan apa yang dilakukannya dengan data itu ditentukan oleh mereka, dan halaman ini tidak mencakupnya. Satu-satunya pengecualian adalah log permintaan di atas, yang disimpan server untuk setiap lambda. Ketentuan layanan melarang penggunaan lambda untuk mengumpulkan data pribadi orang lain. Kalau Anda menemukan lambda yang melakukannya, tolong laporkan.',

    mailTitle: 'Kalau Anda menghubungi kami',
    mail: 'Kalau Anda mengirim email kepada kami, untuk melaporkan penyalahgunaan atau untuk hal lain, kami memakai alamat dan pesan Anda untuk membalas dan menindaklanjuti isinya. Keduanya kami hapus begitu tidak diperlukan lagi untuk itu (Pasal 6 ayat (1) huruf f GDPR).',

    storageTitle: 'Cookie dan browser Anda',
    storage:
      'Hanya ada satu cookie, namanya lang. Cookie ini mengingat bahasa yang Anda pilih, supaya alamat yang tidak mencantumkan bahasa terbuka dalam bahasa itu, dan berlaku selama satu tahun. Penyimpanan bawaan browser mengingat mode terang atau gelap, beberapa pengaturan di halaman yang Anda pakai, dan, bagi administrator, token mereka. Tidak ada yang dipakai untuk melacak Anda, dan tidak ada yang dikirim ke pihak lain: tidak ada analitik, tidak ada iklan, dan tidak ada yang dimuat dari situs lain, bahkan font sekalipun. Karena semuanya hanya menjalankan apa yang Anda minta, tidak diperlukan persetujuan (§ 25 ayat (2) angka 2 TDDDG, undang-undang Jerman).',

    hostingTitle: 'Tempat semuanya disimpan',
    hosting:
      'Server tempat semua ini berjalan disewa dari penyedia hosting di Uni Eropa, dan semua yang dijelaskan di halaman ini disimpan di sana.',

    rightsTitle: 'Hak Anda',
    rights: (mailbox) => (
      <>
        Anda bisa menanyakan data apa yang kami simpan tentang Anda dan meminta salinannya, meminta data itu diperbaiki,
        dihapus, atau dibatasi penggunaannya, dan mengajukan keberatan atas apa pun yang kami lakukan berdasarkan
        kepentingan yang sah dari kami (Pasal 15 sampai 21 GDPR). Kirim email ke {mailbox}. Karena tidak ada akun, kami
        hanya bisa menemukan data milik Anda kalau Anda memberi tahu cara menemukannya: alamat IP yang Anda pakai dan
        kira-kira kapan, atau alamat lambda Anda. Tidak ada keputusan yang menimbulkan akibat hukum atau dampak serupa
        yang signifikan bagi Anda yang diambil secara otomatis (Pasal 22 GDPR).
      </>
    ),
    complaint:
      'Anda juga bisa mengadu ke otoritas perlindungan data, di tempat Anda tinggal atau di tempat kami berada. Otoritas yang membawahi kami adalah Komisioner Perlindungan Data dan Kebebasan Informasi negara bagian Baden-Württemberg di Jerman (LfDI Baden-Württemberg).',
  },
  change: 'Kebijakan ini ikut berubah kalau situsnya berubah. Versi yang berlaku adalah versi yang ada di halaman ini.',
  updated: 'Terakhir diubah pada 30 September 2026.',
};
