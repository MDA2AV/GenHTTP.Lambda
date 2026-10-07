import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Dokumentasi',
    titleSimple: 'Tentang aplikasi Anda',
    hint: 'Apa aplikasi ini, untuk siapa dan mengapa - dan mengapa aplikasi ini dibuat seperti ini. Agen menulisnya di setiap perubahan dan dokumentasi disimpan bersama setiap versi, jadi versi lama kembali bersama dokumentasi yang berlaku untuknya.',
    hintSimple: 'Untuk apa aplikasi Anda dan mengapa, sesuai pemahaman agen dari apa yang Anda minta. Agen memperbaruinya di setiap perubahan.',
    inDraft: 'Dokumentasi draf ini. Dokumentasi ini menjadi milik aplikasi Anda saat draf dijadikan online.',
    pages: { product: 'Produk', decisions: 'Keputusan' },
    emptyTitle: 'Belum ada yang ditulis',
    emptyText: (code) => (
      <>
        Agen menulis dokumentasi bersama perubahannya: apa aplikasinya, untuk siapa dan mengapa di{' '}
        {code('docs/product.md')}, dan mengapa aplikasi dibuat seperti ini di {code('decisions.md')}.
        Dokumentasi adalah bagian dari versi, di samping kode.
      </>
    ),
    emptySimpleTitle: 'Belum ada yang ditulis tentang aplikasi Anda',
    emptySimple: 'Agen dapat menjelaskan untuk apa aplikasi Anda dan mengapa, berdasarkan apa yang Anda minta - lalu terus memperbarui penjelasan itu.',
    ask: 'Minta agen menulisnya',
    describe: 'Minta agen menjelaskannya',
    writePrompt: 'Tulis dokumentasi aplikasi ini: apa aplikasinya, untuk siapa dan mengapa, serta keputusan teknis di baliknya.',
    describePrompt: 'Jelaskan untuk apa aplikasi ini dan mengapa, agar bisa saya baca di bagian Tentang.',
    decisionsPrompt: 'Tuliskan keputusan teknis di balik aplikasi ini, dan alasan keputusan itu diambil.',
    missingProduct: 'Belum ada halaman produk',
    missingProductText: 'Apa aplikasinya, untuk siapa, apa yang dilakukan orang dengannya, dan mengapa - dengan kata-kata orang yang memintanya.',
    missingDecisions: 'Belum ada keputusan yang dicatat',
    missingDecisionsText: 'Bagaimana aplikasi dibuat dan mengapa: bagaimana datanya disimpan, apa saja yang dibutuhkannya, apa yang sengaja ditinggalkan. Hal yang perlu diketahui siapa pun yang mengubahnya berikutnya.',
    correctText: 'Agen menulis ini dari apa yang Anda minta, dan memperbaruinya di setiap perubahan. Ada yang salah atau kurang? Sampaikan kepada agen.',
    correct: 'Sampaikan kepada agen',
    correctPrompt: 'Perbaiki deskripsi aplikasi ini: ',
    placeholder: 'Menjelaskan mengapa entri disimpan selama setahun',
  },
  tests: {
    title: 'Pengujian',
    hint: 'Bagaimana aplikasi ini diuji secara otomatis, serta script dan data yang dipakai pengujiannya. Agen menjaganya tetap mutakhir dan menjalankannya sebelum menyatakan sebuah perubahan selesai. Disimpan bersama setiap versi.',
    inDraft: 'Pengujian draf ini. Pengujian ini menjadi milik aplikasi Anda saat draf dijadikan online - jalankan dulu terhadap pratinjaunya.',
    pages: { testing: 'Cara pengujiannya' },
    emptyTitle: 'Belum ada pengujian',
    emptyText: (code) => (
      <>
        Bagaimana aplikasi diuji (apa yang harus tetap berfungsi, cara memeriksanya, dan cara menjalankan script-nya)
        ditulis agen di {code('tests/README.md')}, dengan script dan data uji di sampingnya.
      </>
    ),
    ask: 'Minta agen menulis pengujian',
    writePrompt: 'Tulis pengujian untuk aplikasi ini: apa yang harus tetap berfungsi dan cara memeriksanya secara otomatis, dengan script yang dijalankan terhadap pratinjaunya.',
    missing: 'Belum dijelaskan cara pengujiannya',
    missingText: 'Apa yang harus tetap berfungsi, bagaimana masing-masing diperiksa, dan cara menjalankan script di sampingnya.',
    placeholder: 'Memeriksa bahwa daftar yang penuh menolak entri baru',
  },
  files: 'File',
  noFiles: 'Tidak ada file di samping halaman-halaman ini.',
  none: 'belum ada',
  missingPill: 'Belum ditulis',
  changedIn: (version) => `Diubah di versi ${version}`,
  changedInDraft: 'Diubah di draf ini',
  showChanges: 'Tampilkan perubahannya',
  hideChanges: 'Sembunyikan perubahannya',
  noChanges: 'Tidak ada yang berubah.',
  edit: 'Edit',
  olderVersion: 'Versi tidak pernah berubah: halaman diedit di versi terbaru, atau di draf.',
  writeIt: 'Tulis sendiri',
  askPage: 'Minta agen menulisnya',
  editInCode: 'Buka di kode',
  cancel: 'Batal',
  save: 'Simpan',
  write: 'Tulis',
  preview: 'Pratinjau',
  writeOrPreview: 'Tulis atau pratinjau',
  discard: 'Perubahan Anda pada halaman ini akan hilang. Buang perubahannya?',
  reading: 'Membaca…',
  readFailed: 'Gagal dibaca.',
  saveFailed: 'Gagal disimpan.',
  savedDraft: 'Disimpan ke draf.',
  savedVersion: (version) => `Disimpan sebagai versi ${version}.`,
  savedOnline: (version) => `Disimpan sebagai versi ${version}, dan sudah online.`,
  savedNotOnline: (version) => `Disimpan sebagai versi ${version}, tetapi gagal online.`,
  saveTitle: 'Simpan sebagai versi baru',
  saveText: (newest) =>
    `Versi tidak pernah berubah, jadi halaman ini disimpan sebagai versi berikutnya - di atas versi ${newest}, dengan semua hal lain tetap seperti semula.`,
  clash: (version) => `Versi ${version} disimpan sejak Anda mulai, dan versi itu juga mengubah halaman ini. Menyimpan akan menggantikan perubahan itu.`,
  alsoOnline: 'Sekaligus jadikan online',
  alsoOnlineNote: 'Hanya dokumentasi yang berubah, jadi pengunjung tidak melihat hal baru - tetapi yang online tetap versi terbaru.',
  skeleton: {
    product: '# Nama aplikasi\n\nApa aplikasinya, dalam satu atau dua kalimat.\n\n## Untuk siapa\n\n## Apa yang dilakukan orang dengannya\n\n## Fitur, dan mengapa ada\n\n## Apa yang tidak dilakukannya\n',
    decisions: '# Keputusan\n\n## Sebuah keputusan\n\nApa yang diputuskan, mengapa, dan apa yang harus diperhatikan saat mengubahnya.\n',
    testing: '# Cara pengujiannya\n\nCara menjalankan pengujian, dan terhadap alamat mana.\n\n## Apa yang harus tetap berfungsi\n\n| Perilaku | Request | Hasil yang diharapkan |\n|---|---|---|\n| | | |\n',
  },
};
