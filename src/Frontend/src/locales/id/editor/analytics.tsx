import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  hint: 'Dihitung untuk Anda oleh platform: tanpa cookie, tanpa kode di aplikasi Anda, dan tanpa sesuatu yang harus disetujui pengunjung.',
  loading: 'Membaca kunjungan…',
  loadFailed: 'Kunjungan tidak dapat dibaca.',

  offTitle: 'Lihat siapa yang mengunjungi aplikasi Anda',
  offText:
    'Berapa banyak orang yang datang setiap hari, halaman apa yang mereka buka, situs mana yang mengarahkan mereka, dari negara mana dan dengan perangkat apa - dihitung per hari, sejak Anda mengaktifkannya.',
  offPrivate: 'Tanpa cookie, dan tanpa banner yang harus disetujui pengunjung Anda',
  offAddresses: 'Tidak ada alamat yang disimpan - hanya jumlah kunjungan setiap hari',
  offKept: (days) =>
    days >= 365
      ? 'Disimpan selama setahun, dan semuanya ditampilkan kepada Anda'
      : `Disimpan selama setahun - ${days} hari terakhir ditampilkan, semuanya dengan Premium`,
  switchOn: 'Aktifkan analitik',
  switchedOn: 'Analitik aktif. Kunjungan pertama muncul dalam beberapa menit.',
  switchOnFailed: 'Analitik tidak dapat diaktifkan.',
  demo: 'Pengunjung sebuah demo tidak dihitung. Salinan milik Anda sendiri dapat menghitung pengunjungnya.',

  range: 'Rentang waktu',
  days: (days) => (days >= 365 ? '12 bulan' : `${days} hari`),
  premiumRange: 'Premium menampilkan hingga setahun',

  visitors: 'Pengunjung',
  visitorsHint: 'Orang yang berbeda, masing-masing dihitung sekali sehari.',
  views: 'Tampilan halaman',
  viewsHint: 'Setiap kali seseorang membuka sebuah halaman.',
  hits: 'Request',
  hitsHint: 'Semua yang diminta dari aplikasi Anda: halamannya, serta gambar dan skrip yang dimuat halaman itu.',
  versus: (days) => `dibandingkan ${days} hari sebelumnya`,
  noChange: 'tidak berubah',
  inDays: (days) => (days >= 365 ? 'dalam 12 bulan terakhir' : `dalam ${days} hari terakhir`),

  perDay: {
    visitors: 'Pengunjung per hari',
    views: 'Tampilan halaman per hari',
    hits: 'Request per hari',
  },
  perWeek: {
    visitors: 'Pengunjung per minggu',
    views: 'Tampilan halaman per minggu',
    hits: 'Request per minggu',
  },
  showValues: 'Tampilkan nilai',
  showChart: 'Tampilkan grafik',
  day: 'Hari',
  week: 'Minggu',

  nobody: 'Tidak ada yang berkunjung pada hari-hari ini.',
  nobodyYet: 'Belum ada yang berkunjung sejak analitik diaktifkan. Kunjungan pertama muncul dalam beberapa menit.',

  pages: 'Halaman',
  pagesHint: 'Halaman yang dibuka orang, yang paling sering dibuka di atas.',
  requested: 'Paling sering diminta',
  requestedHint: 'Semua yang diminta dari aplikasi Anda, menurut alamat - aplikasi ini tidak punya halaman yang dibuka orang.',
  sources: 'Asal pengunjung',
  sourcesHint: 'Situs yang mengarahkan orang ke sini, menurut halaman tempat mereka berada. Mesin pencari hanya menyebutkan bahwa mereka asalnya, bukan apa yang dicari.',
  direct: 'Diketik langsung, bookmark, atau aplikasi',
  countries: 'Negara',
  countriesHint: 'Tempat alamat internet pengunjung terdaftar - biasanya, tetapi tidak selalu, tempat mereka berada.',
  unknownCountry: 'Tidak diketahui',
  devices: 'Perangkat',
  device: {
    desktop: 'Komputer',
    mobile: 'Ponsel',
    tablet: 'Tablet',
    bot: 'Bot',
    other: 'Lainnya',
  },
  browsers: 'Browser',
  systems: 'Sistem',
  crawlers: 'Mesin pencari dan AI',
  crawlersHint: 'Program yang datang berkunjung: mesin pencari yang mengindeks halaman Anda, asisten AI yang membacanya, chat yang menampilkan pratinjau link.',
  noCrawlers: 'Tidak ada mesin pencari atau AI yang datang pada hari-hari ini.',
  other: 'Lainnya',
  otherBots: 'Program lain',
  showAll: (count) => `Tampilkan semua ${count}`,
  showFewer: 'Tampilkan lebih sedikit',
  share: (part, whole) => `${part} dari ${whole}`,
  none: 'Belum ada.',

  moreKept: (since) => `Dihitung sejak ${since} - Premium menampilkan semuanya, hingga setahun.`,
  premiumAsk: 'Tanya soal Premium',
  premiumSubject: 'Premium untuk aplikasi saya',
  premiumBody: (address) => `Halo,\n\nSaya ingin melihat semua analitik aplikasi saya: ${address}\n\n`,

  footer: (minutes) =>
    minutes === 1 ? 'Dihitung per hari, dalam UTC. Diperbarui setiap menit.' : `Dihitung per hari, dalam UTC. Diperbarui setiap ${minutes} menit.`,
  updated: (time) => `Pembaruan terakhir pukul ${time}.`,
  since: (date) => `Dihitung sejak ${date}.`,

  switchOff: 'Nonaktifkan analitik',
  switchOffTitle: 'Nonaktifkan analitik?',
  switchOffText: 'Penghitungan berhenti, dan semua yang telah dihitung sejauh ini dihapus. Tindakan ini tidak dapat dibatalkan.',
  keep: 'Tetap hitung',
  switchOffConfirm: 'Nonaktifkan dan hapus',
  switchedOff: 'Analitik nonaktif, dan yang telah dihitung dihapus.',
  switchOffFailed: 'Analitik tidak dapat dinonaktifkan.',
};
