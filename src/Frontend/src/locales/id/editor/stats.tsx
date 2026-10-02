import type { EditorMessages } from '../../en/editor';

export const stats: EditorMessages['stats'] = {
  readFailed: 'Statistik gagal dibaca.',
  range: 'Rentang waktu',
  lastHour: '1 jam terakhir',
  lastDay: '1 hari terakhir',
  hint: (since) =>
    `Dihitung di memori sejak server terakhir dimulai, ${since}. Setelah restart, angka ini mulai dari nol lagi.`,
  reading: 'Membaca statistik…',
  requests: 'request',
  websockets: (count) => `dan ${count} koneksi websocket`,
  failed: 'gagal',
  serverErrors: (count) => `${count} error server`,
  rejected: 'tidak ditemukan atau ditolak',
  average: 'rata-rata waktu respons',
  sent: (amount) => `${amount} terkirim`,
  nobody: (hour) =>
    hour ? 'Tidak ada yang mengaksesnya dalam satu jam terakhir.' : 'Tidak ada yang mengaksesnya dalam sehari terakhir.',
  requestsTitle: 'Request',
  per: (hour) => (hour ? 'Per menit.' : 'Per 15 menit.'),
  answered: 'Dijawab',
  rejectedSeries: 'Tidak ditemukan atau ditolak',
  failedSeries: 'Gagal',
  timeTitle: 'Waktu respons',
  averagePer: (hour) => (hour ? 'Rata-rata per menit.' : 'Rata-rata per 15 menit.'),
  averageSeries: 'Rata-rata',
  mostAsked: 'Paling sering diminta',
  path: 'Path',
  requestsColumn: 'Request',
  failedColumn: 'Gagal',
  averageColumn: 'Rata-rata',
  since: 'Sejak server dimulai.',
};
