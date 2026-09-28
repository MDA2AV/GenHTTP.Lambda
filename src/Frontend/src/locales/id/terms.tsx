import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Ketentuan layanan',
  binding: (english) => (
    <>Terjemahan ini hanya untuk informasi. Yang berlaku secara hukum adalah {english('versi bahasa Inggris')}.</>
  ),
  intro:
    'Ini layanan gratis untuk mencoba berbagai hal. Layanan ini menjalankan kode yang ditulis siapa saja, di infrastruktur yang dipakai bersama. Itu hanya bisa berjalan kalau semua orang mematuhi beberapa aturan.',
  sections: {
    forbiddenTitle: 'Yang tidak boleh ada di sini',
    forbidden: [
      'Tidak boleh ada malware, phishing, atau crypto miner. Tidak boleh ada yang menyerang, memindai, membanjiri, atau mengganggu sistem lain, baik di sini maupun di tempat lain. Tidak boleh ada yang melecehkan orang. Tidak boleh ada yang tidak berhak Anda publikasikan, termasuk kode, teks, gambar, dan merek dagang milik orang lain.',
      'Jangan gunakan lambda untuk menyimpan atau meneruskan data pribadi orang lain. Alamat publik sama sekali tidak privat, dan platform ini tidak menyediakan cara apa pun untuk menjaga data semacam itu tetap aman.',
    ],
    actionTitle: 'Tindakan yang bisa kami ambil',
    action:
      'Apa pun yang di-deploy di sini bisa dimatikan atau dihapus kapan saja, tanpa pemberitahuan dan tanpa kewajiban menjelaskan alasannya. Dalam praktiknya, itu terjadi kalau sesuatu melanggar aturan di atas, mengancam server yang dipakai bersama semua orang, atau dilaporkan seseorang dan laporannya ternyata benar.',
    lastingTitle: 'Berapa lama semuanya bertahan',
    lasting: (hours, days) =>
      `Deployment tetap bisa diakses sekitar ${hours} jam. Lambda yang tidak Anda buka akan dihapus, beserta semua versi kodenya, sekitar ${days} hari setelah terakhir kali Anda menyentuhnya. Menyimpan atau men-deploy juga dihitung, jadi apa pun yang sedang Anda kerjakan tetap ada. Layanan ini bukan backup: simpan salinan sendiri untuk kode yang penting bagi Anda.`,
    keyTitle: 'Link editor Anda adalah kata sandi Anda',
    key: 'Siapa pun yang punya link editor bisa membaca dan mengubah lambda tersebut, dan tidak ada akun maupun kata sandi di baliknya. Kalau Anda memublikasikan link itu, berarti Anda juga memublikasikan akses untuk mengubah lambda tersebut. Link yang hilang tidak bisa dipulihkan.',
    warrantyTitle: 'Tanpa jaminan',
    warranty:
      'Layanan ini disediakan apa adanya. Tidak ada jaminan bahwa layanan ini berfungsi, akan terus berfungsi, atau akan menyimpan apa pun yang Anda masukkan. Layanan bisa dimulai ulang, diubah, atau dihentikan kapan saja. Jangan membuat apa pun di sini yang penting bagi Anda atau bagi orang lain.',
    reportTitle: 'Melaporkan sesuatu',
    report: (mailbox, front) => (
      <>
        Kalau lambda yang di-host di sini melakukan sesuatu yang tidak semestinya, kirim email ke {mailbox} beserta
        alamatnya. Lihat {front('halaman depan')} untuk tahu apa saja yang perlu disertakan.
      </>
    ),
  },
  change: 'Ketentuan ini bisa berubah. Versi yang berlaku adalah versi yang ada di halaman ini.',

  short:
    'Lambda berjalan di infrastruktur yang dipakai bersama. Dengan membuat lambda, Anda setuju untuk tidak men-deploy malware, halaman phishing, crypto miner, atau apa pun yang menyerang, memindai, atau membanjiri sistem lain, dan tidak memublikasikan konten yang tidak berhak Anda publikasikan. Siapa pun yang tahu link editor bisa mengubah lambda Anda, jadi perlakukan link itu seperti kata sandi. Lambda di paket gratis tetap online selama dipakai: lambda yang tidak dikunjungi dan tidak diubah siapa pun selama sebulan akan dimatikan, lalu dihapus jika tetap tidak ada aktivitas selama dua bulan setelahnya. Apa pun yang Anda deploy bisa dihapus kapan saja.',
};
