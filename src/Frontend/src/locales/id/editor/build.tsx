import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Build',
  hint: 'Bahan pembuat aset atau kode sebuah versi: file tempat siapa pun yang mengubah aplikasi - agen Anda, di sebuah kloning - menjalankan alat build, disimpan bersama setiap versi dan tidak pernah dikompilasi atau disajikan. Platform ini tidak membangun apa pun, jadi file ini hanya dibaca di sini, tidak diedit.',
  overview: 'Ringkasan',
  files: 'File',
  scope: (version) =>
    `Bahan pembuat versi ${version} - disimpan bersamanya, tidak pernah dikompilasi atau disajikan, dan dibangun oleh siapa pun yang mengubahnya, tidak pernah di sini.`,
  scopeDraft: 'Bahan pembuat draf ini - disimpan bersamanya, tidak pernah dikompilasi atau disajikan, dan dibangun oleh siapa pun yang mengubahnya, tidak pernah di sini.',
  reading: 'Membaca bahan pembuatnya…',
  readFailed: 'Bahan pembuatnya tidak dapat dibaca.',

  emptyTitle: (version) => `Versi ${version} tidak menyimpan bahan pembuat apa pun`,
  emptyTitleDraft: 'Draf ini tidak menyimpan bahan pembuat apa pun',
  emptyText: (code) => (
    <>
      Jika aset atau kode sebuah versi dibuat oleh alat build - dikompilasi, digabungkan, atau dihasilkan - file
      bahan pembuatnya disimpan di sini, bersama setiap versi: folder {code('build/')} di sebuah kloning. Siapa pun yang
      mengubah aplikasi menjalankan build di tempatnya bekerja dan menyimpan keduanya sekaligus; platform ini tidak
      membangun apa pun. Yang ditulis apa adanya untuk disajikan atau dikompilasi tidak membutuhkannya.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} di sebuah kloning memberi tahu agen pemrograman cara menggunakannya.</>
  ),

  inVersion: (version) => `Di versi ${version}`,
  inDraft: 'Di draf ini',
  comparedWith: (version) => `dibandingkan dengan versi ${version}`,
  first: 'Versi pertama yang menyimpannya.',
  both: (here, program) =>
    `${here} file berubah di sini, dan ${program} file berubah pada kode dan aset.`,
  hereOnly: (here) =>
    `${here} file berubah di sini, dan tidak ada yang berubah pada kode atau aset: jika yang berubah dibangun ke dalamnya, berarti belum dibangun.`,
  programOnly: 'Tidak ada yang berubah di sini.',
  unchanged: 'Tidak ada yang berubah di sini, maupun pada kode atau aset.',
  showChanges: 'Tampilkan perubahan',
  hideChanges: 'Sembunyikan perubahan',
  noChanges: 'Tidak ada yang berubah di sini.',

  readme: 'Cara membangunnya',
  noReadme: (code) => (
    <>
      Belum ada yang menjelaskan cara membangunnya. Sebuah {code('README.md')} di bagian atas - perintahnya, dan ke
      mana hasil build disimpan - menjadi pegangan agen berikutnya untuk membangun.
    </>
  ),
  readOnly: 'Hanya baca: diubah di tempat ia dibangun.',
  noFiles: 'Tidak ada file.',
};
