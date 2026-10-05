import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Ruang pengembangan',
  hint: 'Bahan pembuat aset sebuah versi, tempat toolchain membangunnya: proyek front end-nya, dengan sumber, konfigurasi, dan lock file-nya. Ini disimpan bersama setiap versi dan tidak pernah dikompilasi atau disajikan. Siapa pun yang mengubahnya - agen Anda, di sebuah kloning - membangunnya di tempat mereka bekerja dan menyimpannya bersama hasil build-nya: platform ini tidak membangun apa pun. Karena itu di sini hanya dibaca, tidak diedit.',
  overview: 'Ringkasan',
  files: 'File',
  scope: (version) =>
    `Bahan pembuat aset versi ${version} - disimpan bersamanya, tidak pernah dikompilasi atau disajikan, dan dibangun oleh siapa pun yang mengubahnya, tidak pernah di sini.`,
  scopeDraft: 'Bahan pembuat aset draf ini - disimpan bersamanya, tidak pernah dikompilasi atau disajikan, dan dibangun oleh siapa pun yang mengubahnya, tidak pernah di sini.',
  reading: 'Membaca ruang pengembangan…',
  readFailed: 'Ruang pengembangan tidak dapat dibaca.',

  emptyTitle: (version) => `Tidak ada ruang pengembangan di versi ${version}`,
  emptyTitleDraft: 'Tidak ada ruang pengembangan di draf ini',
  emptyText: (code) => (
    <>
      Jika front end dibangun dengan toolchain - React, Vue, atau Svelte dengan Vite, TypeScript, Tailwind - proyeknya
      disimpan di sini, bersama setiap versi: bahan pembuat asetnya. Agen Anda membangunnya di tempatnya bekerja dan
      menyimpan sumbernya bersama hasil build-nya - di sebuah kloning, ini adalah folder {code('dev/')}. Front end
      berupa HTML, CSS, dan JavaScript biasa tidak membutuhkannya.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} di sebuah kloning memberi tahu agen pemrograman cara menyiapkannya.</>
  ),

  projects: 'Proyek',
  atTheTop: 'ruang pengembangan itu sendiri',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: 'Dibangun dengan',
  build: 'Build',
  noBuild: 'Tidak ada script build di package.json-nya.',
  into: 'Hasil build ke',
  intoAssets: (folder, files, size) => (
    <>
      {folder} dari aset - {files === 1 ? '1 file' : `${files} file`}, {size} di versi ini
    </>
  ),
  intoNothing: (folder) => <>{folder} dari aset - yang kosong di versi ini</>,
  packages: 'Paket',
  packagesCount: (runtime, tooling) =>
    `${runtime} untuk dijalankan, ${tooling} untuk membangun`,
  showPackages: 'Tampilkan',
  hidePackages: 'Sembunyikan',
  runtime: 'Untuk dijalankan',
  tooling: 'Untuk membangun',
  missing: (page, files) => (
    <>
      {page} merujuk ke {files.length === 1 ? 'sebuah file' : `${files.length} file`} yang tidak ada di antara aset
      ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): hasil build tidak tersimpan utuh, dan halaman
      tidak dapat dimuat.
    </>
  ),
  noLock: 'Tidak ada lock file: build berikutnya mungkin memasang paket dengan versi yang berbeda dari build terakhir.',
  noIgnore: 'Tidak ada .gitignore: apa yang dipasang dan dibangun toolchain-nya bisa ikut masuk ke sebuah versi.',

  inVersion: (version) => `Di versi ${version}`,
  inDraft: 'Di draf ini',
  comparedWith: (version) => `dibandingkan dengan versi ${version}`,
  first: 'Versi pertama yang memilikinya.',
  both: (here, assets) =>
    `${here} file berubah di sini, dan ${assets} file dari aset.`,
  hereOnly: (here) =>
    `${here} file berubah di sini, dan tidak ada aset yang berubah: kecuali perubahannya tidak memerlukan build, pengunjung mendapat apa yang mereka dapat sebelumnya.`,
  builtOnly: (folder) => (
    <>Yang dibangun ke {folder} berubah, dan tidak ada yang berubah di sini: perubahan pada hasil build akan dibatalkan oleh build berikutnya.</>
  ),
  assetsOnly: 'Tidak ada yang berubah di sini.',
  unchanged: 'Tidak ada yang berubah di sini maupun di aset.',
  showChanges: 'Tampilkan perubahan',
  hideChanges: 'Sembunyikan perubahan',
  noChanges: 'Tidak ada yang berubah di sini.',

  readme: 'Cara membangunnya',
  noReadme: (code) => (
    <>
      Belum ada yang menjelaskan cara membangunnya. {code('README.md')} di bagian atas ruang pengembangan - berisi
      perintah dan tujuan hasil build - adalah acuan agen berikutnya.
    </>
  ),
  readOnly: 'Hanya baca: diubah di tempat ia dibangun.',
  noFiles: 'Tidak ada file.',
};
