import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Buat lambda',
  whatTitle: 'Mau membuat apa?',
  whatText:
    'Pilih yang paling mendekati. Anda akan mulai dari salinan aplikasi yang sudah jalan, dan bebas mengubahnya. Atau mulai dari nol.',
  seeIt: 'Lihat langsung',
  startFrom: 'Mulai dari ini',
  starters: {
    'demo-crud': {
      title: 'Catat apa saja',
      description: 'Daftar yang bisa ditambah, diubah, dan dicentang bersama: tugas, catatan, bookmark, atau inventaris kecil.',
    },
    'demo-registration': {
      title: 'Daftar dan login',
      description: 'Akun yang bisa didaftarkan dan dipakai untuk login, plus halaman yang hanya bisa dilihat pemiliknya.',
    },
    'demo-game': {
      title: 'Game untuk main bareng',
      description: 'Permainan untuk beberapa orang sekaligus, langsung di browser masing-masing.',
    },
    'demo-files': {
      title: 'Berbagi file dan gambar',
      description: 'Orang mengunggah gambar atau dokumen, dan semua orang lain bisa melihatnya.',
    },
    'demo-live': {
      title: 'Tampilkan secara real-time',
      description: 'Halaman yang otomatis diperbarui begitu ada perubahan: vote, skor, atau dashboard.',
    },
    empty: {
      title: 'Yang lain',
      description: 'Mulai dari lambda kosong, dan buat apa pun yang Anda bayangkan.',
    },
  },

  addressTitle: 'Beri alamat',
  fromDemo: (title) => (
    <>{title} – lambda Anda dimulai sebagai salinan demo ini, dan semua isinya bebas Anda ubah.</>
  ),
  fromNothing: 'Lambda Anda mulai dari kosong, siap untuk ide apa pun.',
  pickAgain: 'Pilih yang lain',
  publicKey: 'Kunci publik',
  free: (key) => `“${key}” masih tersedia.`,
  keyHint: 'Huruf kecil, angka, dan tanda hubung. Minimal tiga karakter. Kosongkan untuk kunci acak.',
  accept: 'Saya menyetujui ketentuan layanan',
  fullTerms: 'Baca ketentuan layanan selengkapnya',
  back: 'Kembali',
  creating: 'Membuat…',
  submit: 'Buat lambda',
  keepLink: 'Layar berikutnya menampilkan link editor Anda. Hanya lewat link itu Anda bisa masuk lagi, jadi simpan baik-baik.',
  failed: 'Lambda gagal dibuat.',
};
