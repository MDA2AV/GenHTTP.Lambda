import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Deployment tetap online selama dipakai${until ? ` (kalau tidak ada yang memakai, sampai ${until})` : ''}. Deploy ulang, atau kunjungan apa pun, memulai ulang hitungan itu.`,
  takeOffline: 'Matikan',
  readFailed: 'Riwayat gagal dibaca.',
  reading: 'Membaca riwayat…',
  none: 'Belum ada yang di-deploy.',
  noDescription: 'Tanpa deskripsi',
  deployed: (when, by) => `Di-deploy pada ${when} oleh ${by}`,
  duration: 'Berapa lama online',
  online: 'online',
  short: {
    replaced: 'diganti',
    stopped: 'dimatikan',
    expired: 'kedaluwarsa',
    admin: 'oleh operator',
    ended: 'berakhir',
  },
  putBack: (version) => `Jadikan versi ${version} online lagi`,
  timeline: 'Yang online selama tujuh hari terakhir',
  block: (version, from, to) => `Versi ${version}, ${from} sampai ${to ?? 'sekarang'}`,
  weekAgo: 'seminggu lalu',
  now: 'sekarang',
};
