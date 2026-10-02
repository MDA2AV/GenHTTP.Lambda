import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Domain gagal dibaca.',
  reaching: (domain) => `Request ke ${domain} sekarang sampai ke lambda ini.`,
  saveFailed: 'Domain gagal disimpan.',
  removed: 'Domain sudah dihapus. Lambda tetap bisa diakses di alamatnya di sini.',
  removeFailed: 'Domain gagal dihapus.',
  hint: 'Lambda premium bisa diakses di domain sendiri (seluruhnya, mulai dari root), selain di alamatnya di sini. Arahkan domain ke server ini, masukkan di sini, dan request ke domain itu akan sampai ke lambda.',
  loading: 'Memuat…',
  example: 'domain-anda.com',
  open: (domain) => `Buka ${domain}`,
  label: 'Domain yang dipakai',
  serving: (domain) => <>Sekarang juga melayani {domain}, selain alamatnya di sini.</>,
  none: 'Belum ada. Bisa subdomain seperti shop.example.com, atau domain penuh seperti example.com.',
  change: 'Ganti',
  use: 'Pakai domain ini',
  remove: 'Hapus',
  confirm: 'Hapus domain?',
  keep: 'Tetap pakai',
  confirmText: (domain) => (
    <>
      Request ke {domain} langsung berhenti sampai ke lambda ini. Alamatnya di sini tetap sama, begitu juga pengaturan
      DNS domain itu.
    </>
  ),
  point: 'Arahkan domain ke server ini',
  check: 'Cek lagi',
  records:
    'Di penyedia DNS domain Anda, tambahkan dua record ini. Lewati record AAAA kalau Anda tidak ingin bisa diakses lewat IPv6.',
  type: 'Tipe',
  name: 'Nama',
  value: 'Nilai',
  pointsHere: (domain) => <>{domain} sudah mengarah ke sini.</>,
  alsoElsewhere: (addresses) =>
    ` Domain ini juga mengarah ke ${addresses}, yang bukan server ini. Pengunjung yang diarahkan ke sana tidak akan sampai ke lambda.`,
  elsewhere: (addresses) => `Domain ini masih mengarah ke ${addresses}, bukan ke server ini.`,
  wait: 'Perubahan bisa butuh waktu sampai terlihat di mana-mana, paling lama sesuai TTL record yang lama.',
  cname: 'Pakai record CNAME sebagai gantinya',
  cnameText: (target) => (
    <>
      Subdomain juga bisa diarahkan ke {target} dengan record CNAME. Dengan begitu, subdomain ikut menyesuaikan kalau
      alamat server ini berubah. Tapi ada kekurangannya:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Tidak bisa dipakai untuk domain penuh ({example} itu sendiri): standarnya tidak mengizinkan CNAME berdampingan
      dengan record yang dimiliki setiap domain di root-nya. Beberapa penyedia menawarkan record ALIAS, ANAME, atau
      “flattened” yang bisa dipakai di sana.
    </>
  ),
  cnameAlone: 'Tidak boleh ada record lain di nama yang sama: tidak ada record MX untuk email, tidak ada record TXT untuk verifikasi.',
  cnameLookup: 'Resolver pengunjung perlu satu lookup tambahan sebelum sampai.',
  copy: 'Salin',
  copyValue: (value) => `Salin ${value}`,
};
