import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Buat aplikasi dengan menjelaskannya',
  intro:
    'Sampaikan kebutuhan Anda kepada agen build di instalasi ini. Agen menulis aplikasinya, menjadikannya online di sini, dan menyimpannya di daftar Anda - untuk ditanyakan dan diubah nanti dengan cara yang sama.',
  other: (ship, create) => (
    <>
      Atau {ship('hubungkan agen Anda sendiri')}, atau {create('tulis kodenya sendiri')}.
    </>
  ),
};
