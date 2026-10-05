import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Kloning',
  title: 'Kloning dengan git',
  intro:
    'Kerjakan dengan alat dan agen coding Anda sendiri: repositorinya adalah proyek yang dijalankan aplikasi ini, setiap versi adalah commit di main, dan setiap draf adalah branch.',
  keyWarning: 'Alamat ini memuat kunci editor: siapa pun yang memilikinya bisa mengubah aplikasi. Jangan sertakan dalam apa yang Anda bagikan.',
  draft: (branch) => <>Draf ini adalah branch {branch}.</>,
  pushing: 'Push',
  toMain: (deploy) => <>Commit yang di-push ke main menjadi versi berikutnya, belum online - {deploy} menjadikannya online bersama push.</>,
  toBranch: 'Branch yang di-push menjadi draf, dengan pratinjau online di alamatnya sendiri.',
  agents: (file) => <>{file} di dalam repositori memberi tahu agen coding sisanya.</>,
  readOnly: 'Demo hanya bisa dibaca: kloning untuk membacanya, lalu mulai lambda Anda sendiri darinya untuk mengubahnya.',
  copy: 'Salin',
  copied: 'Tersalin',
};
