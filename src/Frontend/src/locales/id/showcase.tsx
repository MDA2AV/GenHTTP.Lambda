import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Showcase',
  title: 'Dibuat di sini, online sekarang',
  intro:
    'Lambda yang dipamerkan oleh pemiliknya. Semuanya online, jadi setiap kartu membuka aplikasi aslinya. Yang baru-baru ini dipakai muncul lebih dulu.',
  counted: (total) => `${total} lambda`,
  failed: 'Showcase gagal dimuat.',
  loadingMore: 'Memuat lebih banyak…',
  showMore: 'Tampilkan lebih banyak',
  nothingTitle: 'Belum ada yang dipamerkan',
  nothing: (tab) => (
    <>
      Sudah membuat sesuatu yang jalan? Buka pusat kontrolnya, pilih {tab('Showcase')}, lalu tambahkan judul, sedikit
      deskripsi, dan gambar. Aplikasinya akan muncul di sini selama online.
    </>
  ),
  buildOne: 'Buat aplikasi',
  yoursTitle: 'Mau aplikasi Anda tampil di sini?',
  yours: (tab) => (
    <>
      Buka pusat kontrol lambda Anda dan pilih {tab('Showcase')}, atau minta agen yang membuatnya untuk memamerkannya.
      Hanya pemegang kunci editor yang bisa melakukannya, dan aplikasinya bisa ditarik lagi kapan saja.
    </>
  ),
  buildSomething: 'Buat aplikasi',
};

export const card: Messages['card'] = {
  noPicture: 'Belum ada gambar',
  title: 'Judul',
  description: 'Apa yang bisa dilakukan pengunjung di sini.',
  opens: (title, address) => `${title}, membuka ${address} di tab baru`,
};
