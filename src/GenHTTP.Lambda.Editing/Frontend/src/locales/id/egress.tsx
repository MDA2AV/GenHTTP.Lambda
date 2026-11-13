import type { EditorMessages } from '../en';

export const egress: EditorMessages['egress'] = {
  hint:
    'Apakah kode boleh terhubung ke server lain: memanggil API, membuka socket, mengirim email. Lambda berbagi satu server, jadi izin ini diberikan per lambda - lambda premium boleh, yang lain setelah operator situs ini mengizinkannya.',

  included: 'Termasuk dalam premium',
  includedText: 'Kode boleh terhubung ke server lain: premium sudah mencakupnya.',
  allowed: 'Diizinkan',
  allowedText: (ago) => <>Operator situs ini mengizinkannya {ago}. Kode boleh terhubung ke server lain.</>,
  requested: 'Menunggu operator',
  requestedText: (ago) => (
    <>Diminta {ago}. Setelah operator situs ini mengizinkannya, kode yang terhubung dapat dikompilasi - sampai saat itu kode tersebut ditolak.</>
  ),
  declined: 'Ditolak',
  declinedText: (ago) => <>Operator situs ini menolaknya {ago}. Anda dapat meminta lagi dengan menjelaskan lebih lanjut untuk apa izin ini diperlukan.</>,
  none: 'Tidak diizinkan',
  noneText:
    'Kode tidak boleh terhubung ke server lain: kode yang memanggil API, membuka socket, atau mengirim email tidak dapat dikompilasi. Minta izin di bawah jika lambda memerlukannya.',
  operatorSaid: 'Kata operator',
  purposeSaid: 'Untuk apa',

  connects: 'Kode terhubung ke server lain, padahal lambda ini belum boleh: kode tidak dapat dikompilasi sampai hal itu diizinkan.',
  connectsWaiting: 'Kode terhubung ke server lain: kode dapat dikompilasi setelah operator mengizinkannya.',

  askTitle: 'Minta izin',
  askAgainTitle: 'Minta lagi',
  editTitle: 'Untuk apa',
  purpose: 'Ke mana lambda terhubung, dan untuk apa?',
  purposeHint: 'Sebutkan server atau layanannya, dan apa yang dilakukan lambda dengannya. Operator situs ini membacanya untuk memutuskan.',
  purposeExample: 'api.open-meteo.com, untuk prakiraan cuaca di halaman depan',
  send: 'Kirim permintaan',
  save: 'Simpan',
  cancel: 'Batal',
  edit: 'Ubah keperluannya',
  asked: 'Sudah diminta. Operator situs ini yang memutuskan.',
  askFailed: 'Permintaan tidak dapat dikirim.',
  withdraw: 'Tarik permintaan',
  withdrawn: 'Permintaan ditarik.',
  giveUp: 'Lepaskan izin',
  givenUp: 'Kode tidak lagi boleh terhubung ke server lain.',
  giveUpTitle: 'Lepaskan izin terhubung ke server lain?',
  giveUpText:
    'Mulai permintaan berikutnya, kode tidak boleh terhubung ke server lain, dan versi yang online berhenti berfungsi di bagian yang melakukannya. Untuk mendapatkannya kembali, minta lagi.',
  keep: 'Pertahankan',
  failed: 'Gagal.',

  coversTitle: 'Yang termasuk',
  covers: ['HttpClient dan WebRequest', 'Socket dan klien WebSocket', 'Email (SmtpClient)', 'DNS dan ping', 'XML yang dimuat dari sebuah alamat'],
  notCovered:
    'Bukan yang terjadi di browser: halaman lambda boleh mengambil data dari situs lain sendiri, dan pengunjung, webhook, serta websocket tetap menjangkau lambda seperti biasa. API publik yang dapat dipanggil browser paling baik dipanggil dari halaman - itu tidak memerlukan izin.',

  refused: 'Kode terhubung ke server lain',
  refusedText: 'Lambda ini belum boleh melakukannya. Minta izin, dan operator situs ini yang memutuskan.',
  openEgress: 'Minta izin',
};
