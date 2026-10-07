import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Kode',
  version: (version) => `versi ${version}`,
  edited: ', diedit',
  loadFailed: 'Versi itu gagal dimuat.',
  compiles: 'Berhasil dikompilasi.',
  notYet: 'Belum bisa dikompilasi.',
  checkFailed: 'Kode gagal diperiksa.',
  saved: (version) => `Disimpan sebagai versi ${version}.`,
  featureSaved: 'Disimpan ke draf. Deploy pratinjaunya untuk mencobanya.',
  featureLoadFailed: 'Draf gagal dimuat.',
  previewOnline: 'Pratinjau sudah online.',
  previewRefused: 'Pratinjau tidak berubah. Lihat pesan compiler di bawah.',
  isOnline: (version) => `Versi ${version} sudah online.`,
  notOnline: 'Gagal online. Lihat pesan compiler di bawah.',
  failed: 'Tidak berhasil.',
  unchanged: 'Tidak ada perubahan sejak terakhir disimpan.',
  demo: 'Ini demo, jadi semuanya hanya bisa dibaca. Untuk mengubahnya, buat lambda Anda sendiri dari demo ini.',
  hint: (b) => (
    <>
      File dari satu versi. {b('Kode')}-nya adalah program beserta semua yang disimpan bersamanya: file .cs di bagian atas
      dikompilasi, dan setiap file lain - dokumentasi, pengujian, bahan pembuat front end - disimpan bersama versi dan
      tidak pernah dikompilasi atau disajikan. {b('Sumber daya')}-nya - halaman, script, stylesheet, gambar, migrasi
      database - dibaca dan disajikan selama versi berjalan, dan bersifat publik di tempat kode menyajikannya. Menyimpan
      membuat versi baru dan tidak mengubah apa yang sedang online; untuk mencoba perubahan lebih dulu, mulai draf.
      Ctrl-S menyimpan, F12 membuka deklarasi.
    </>
  ),
  hintFeature: (b) => (
    <>
      File draf ini: {b('kode')}-nya - file .cs di bagian atas dikompilasi, sisanya disimpan bersamanya - dan{' '}
      {b('sumber daya')}-nya, yang dibaca dan disajikan selama draf berjalan. Menyimpan menaruhnya di draf dan
      menampilkannya di alamat draf itu sendiri; pengunjung Anda tidak melihat apa pun sampai Anda menjadikan draf
      online.
    </>
  ),
  inFeature: (name) => `di draf “${name}”`,
  changedElsewhere: 'Draf ini disimpan dari tempat lain sejak Anda membukanya, mungkin oleh agen. Muat yang tersimpan sebelum menyimpan di sini; perubahan Anda tidak akan disimpan di atasnya.',
  readAgain: 'Muat yang tersimpan',
  newer: (version) => `Versi ${version} lebih baru dari yang terbuka di sini.`,
  check: 'Periksa',
  save: 'Simpan',
  deploy: 'Deploy',
  deployPreviewTitle: 'Simpan, lalu buat draf online di alamatnya sendiri untuk dicoba',
  binary: (size) => `Bukan teks, jadi tidak ada yang bisa diedit di sini. Ukurannya ${size}.`,
  saveAndDeploy: 'Simpan dan deploy',
  saveVersion: 'Simpan versi baru',
  fromOlder: (version, newest) =>
    `Ini berawal dari versi ${version}, dan versi ${newest} lebih baru. Menyimpan menjadikannya versi terbaru, tanpa apa yang datang setelah versi ${version}.`,
  featureInstead: (start) => (
    <>
      Mau mencoba sesuatu? {start('Masukkan ke draf baru sebagai gantinya')}: draf punya alamat sendiri, dan tidak ada versi yang
      disimpan sampai hasilnya pas.
    </>
  ),
  cancel: 'Batal',
  what: 'Apa yang diubah? Opsional, akan ditampilkan di riwayat.',
  placeholder: 'Menambahkan formulir kontak',
  goToDefinition: 'Buka definisi',
  versionLabel: 'Versi',
  shown: (version, online, newest) =>
    `Versi ${version}${online ? ', online' : newest ? ', terbaru' : ''}`,
  optionOnline: ' (online)',
  switchUnsaved: 'Perubahan Anda di sini belum disimpan. Tetap buka versi yang lain?',
  noVersion: 'Belum ada versi yang bisa ditampilkan.',
  label: 'File',
  codeGroup: 'Kode',
  codeWhy: 'Tidak pernah disajikan. File .cs di bagian atas dikompilasi; sisanya disimpan bersama versi.',
  resources: 'Sumber daya',
  resourcesPublic: 'Publik: versi ini menyajikannya dengan Resources.',
  resourcesPrivate: 'Ikut dalam versi, tetapi versi ini tidak menyajikannya.',
  noResources: 'Tidak ada di versi ini.',
  count: (files) => (files === 1 ? '1 file' : `${files} file`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Versi ini memakai ${used} dari ${of} yang boleh dimiliki sebuah versi, untuk kode dan sumber daya bersama-sama.`,
  scope: (data) => (
    <>Apa yang disimpan lambda selama berjalan sama untuk setiap versi, dan ada di bawah {data('Data')}.</>
  ),
  download: 'Unduh',
  newIn: (group) => `File baru di ${group}`,
  uploadIn: (group) => `Unggah ke ${group}`,
  pick: 'Pilih file untuk melihat isinya.',
};
