import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Informasi hukum',
  intro:
    'Siapa yang mengelola situs ini. Hukum Jerman mewajibkan setiap situs yang dikelola dari Jerman mencantumkan data ini di satu tempat yang mudah ditemukan (§ 5 DDG), dan inilah tempatnya.',
  sections: {
    providerTitle: 'Penyedia layanan',
    contactTitle: 'Kontak',
    contact: (mail, abuse) => (
      <>
        Email: {mail}. Untuk melaporkan lambda yang merugikan orang lain, kirim email ke {abuse}.
      </>
    ),
    editorialTitle: 'Penanggung jawab konten',
    editorial:
      'Bertanggung jawab atas halaman-halaman situs ini menurut § 18 ayat (2) MStV (undang-undang Jerman), tetapi tidak atas lambda yang di-host di sini, yang ditulis sendiri oleh pemiliknya:',
    dsaTitle: 'Titik kontak menurut Digital Services Act',
    dsa: (mail) => (
      <>
        Otoritas, Komisi Eropa, dan siapa pun yang memakai layanan ini dapat menghubungi kami di {mail}, dalam bahasa Jerman atau Inggris (Pasal 11 dan 12 DSA).
      </>
    ),
  },
};
