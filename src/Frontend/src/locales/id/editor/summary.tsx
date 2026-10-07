import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: 'Membaca kondisinya…',
  readDocs: 'Baca dokumentasi',
  hint: (since, kept, retention, tier) =>
    `Trafik dihitung sejak server terakhir dimulai (${since}). ` +
    (kept
      ? `Lambda tetap online selama dipakai, dan dihapus setelah ${retention} hari tanpa kunjungan dan tanpa perubahan.`
      : `Lambda ini ada di paket ${tier}, yang membuatnya tetap online dan tersimpan sesepi apa pun.`),
  onlineFor: (duration, version) => (
    <>
      Online selama {duration('beberapa waktu')}, menyajikan versi {version}.
    </>
  ),
  offline: 'Offline. Tidak ada yang disajikan sampai sebuah versi di-deploy.',
  nothing: 'Belum ada kode yang ditulis.',
  requestsToday: 'request hari ini',
  lastHour: (count) => `${count} dalam satu jam terakhir`,
  hourly: 'Request per jam selama sehari terakhir',
  failed: 'gagal',
  failedTitle: (failed, rejected) =>
    `${failed} error server, ${rejected} tidak ditemukan atau ditolak, selama sehari terakhir`,
  average: 'rata-rata waktu respons',
  noneYet: 'belum ada',
  lastVisit: 'kunjungan terakhir',
  problems: 'Ada masalah baru-baru ini',
  openLog: 'Buka log',
  latest: 'Perubahan terakhir',
  allVersions: 'Semua versi',
  noDescription: 'Tanpa deskripsi',
  version: (version) => `Versi ${version}`,
  notOnline: 'belum online',
  wanted: 'Yang diminta',
  noVersions: 'Belum ada versi.',
  inProgress: 'Sedang dikerjakan',
  allFeatures: 'Semua draf',
  previewOnline: 'Pratinjaunya online',
  previewOffline: 'Pratinjaunya offline',
  behind: 'tertinggal',
  storage: 'Penyimpanan',
  versionAllowance: 'Kode dan sumber daya',
  data: 'Data',
};
