import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Kode',
  version: (version) => `versi ${version}`,
  edited: ', diedit',
  online: ', online',
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
  demo: 'Ini demo, jadi semuanya hanya bisa dibaca. Untuk mengubahnya, buat lambda Anda sendiri dari demo ini. ',
  edit: 'Edit kode secara manual. Menyimpan membuat versi baru dan tidak mengubah yang sedang online; deploy membuatnya online. Untuk mencoba perubahan dulu, mulai draf. ',
  editFeature:
    'Kode draf ini. Menyimpan membuatnya tetap di draf, jadi tidak ada yang berubah bagi pengunjung lambda. Deploy membuatnya online di alamat draf itu sendiri, untuk dicoba; menggabungkan draf menjadikannya versi berikutnya. ',
  inFeature: (name) => `di draf “${name}”`,
  changedElsewhere: 'Draf ini disimpan dari tempat lain sejak Anda membukanya, mungkin oleh agen. Muat yang tersimpan sebelum menyimpan di sini; perubahan Anda tidak akan disimpan di atasnya.',
  readAgain: 'Muat yang tersimpan',
  files: (entry, cs, context) => (
    <>
      {entry} mengembalikan apa yang disajikan, file {cs} lainnya berisi tipe, dan file lain disajikan apa adanya -
      kecuali yang ada di {context}: dokumentasi dan pengujian, yang tidak pernah dikompilasi atau disajikan. Ctrl-S
      untuk menyimpan, F12 untuk membuka deklarasi.
    </>
  ),
  newer: (version) => ` Versi ${version} lebih baru dari yang terbuka di sini.`,
  check: 'Periksa',
  save: 'Simpan',
  deploy: 'Deploy',
  deployPreviewTitle: 'Simpan, lalu buat draf online di alamatnya sendiri untuk dicoba',
  binary: (size) => `Bukan teks, jadi tidak ada yang bisa diedit. File ini disajikan apa adanya, ukurannya ${size} kB.`,
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
};
