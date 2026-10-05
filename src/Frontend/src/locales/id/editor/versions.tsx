import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Versi adalah programnya, yaitu kode dan asetnya, dan tidak pernah berubah setelah disimpan. Jadi versi mana pun bisa dibandingkan dan dijadikan online lagi persis seperti semula. Setiap versi menyimpan apa yang diminta dan apa yang diubah. Untuk mengubah lambda, mulai draf: draf menjadi versi berikutnya setelah hasilnya pas. Versi terlama dihapus begitu jumlahnya lebih dari ${limit}; versi yang sedang online tidak pernah dihapus.`,
  none: 'Belum ada versi.',
  noDescription: 'Tanpa deskripsi',
  online: 'online',
  putOnline: 'Deploy versi ini',
  rollBackTitle: 'Jadikan versi lama ini online lagi',
  deploy: 'Deploy',
  rollBack: 'Rollback',
  readFailed: 'Versi ini gagal dibaca.',
  comparing: 'Membandingkan…',
  unchanged: 'Tidak ada yang berubah dari versi sebelumnya.',
  first: 'Versi pertama.',
  status: { added: 'ditambahkan', removed: 'dihapus', changed: 'diubah', same: 'sama' },
  groups: {
    code: 'Kode',
    assets: 'Aset',
    development: 'Ruang pengembangan',
    context: 'Dokumentasi dan pengujian',
  },
  browse: 'Lihat file-nya',
  docs: 'Baca dokumentasinya',
  development: 'Lihat bahan pembuatnya',
  edit: 'Edit dari sini',
  feature: 'Mulai draf dari sini',
  featureTitle: 'Kerjakan perubahan dari versi ini di samping lambda, lalu gabungkan menjadi versi berikutnya setelah hasilnya pas',
  binary: 'Bukan teks, jadi tidak ada baris untuk dibandingkan.',
  tooLarge: 'Terlalu besar untuk dibandingkan baris per baris.',
};
