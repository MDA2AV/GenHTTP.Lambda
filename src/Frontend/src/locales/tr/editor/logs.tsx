import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Log okunamadı.',
  hint: (capturing) =>
    'İstekler, lambdanın yazdırdıkları ve ters giden şeyler, anında.' +
    (capturing ? '' : ' Bu kurulum lambdaların yazdırdıklarını saklamaz, bu yüzden yalnızca istekler ve hatalar görünür.') +
    ' Log bellekte tutulur ve buradaki tüm lambdalarla paylaşılır. Bu yüzden dakikalar ya da saatler öncesine kadar gider ve sunucu yeniden başlayınca boşalır. Ziyaretçilerin adresleri gösterilmez.',
  featureHint: (capturing) =>
    'Bu taslağın önizlemesinin verdiği yanıtlar, yazdırdıkları ve fırlattığı hatalar, anında.' +
    (capturing ? '' : ' Bu kurulum lambdaların yazdırdıklarını saklamaz, bu yüzden yalnızca istekler ve hatalar görünür.') +
    ' Lambdanın kendi logundan ayrı tutulur; o log önizlemeyi asla göstermez. Bellekte tutulur, bu yüzden dakikalar ya da saatler öncesine kadar gider.',
  nothingPreview: 'Henüz bir şey yok. Taslağın önizlemesini açın, istekler burada görünsün.',
  search: 'Ara',
  searchLabel: 'Logda ara',
  resume: 'Yeni satırları geldikçe göster',
  pause: 'Okurken yeni satır eklemeyi durdur',
  paused: 'Duraklatıldı',
  live: 'Canlı',
  show: 'Göster',
  all: 'Hepsi',
  requests: 'İstekler',
  output: 'Çıktılar',
  problems: 'Sorunlar',
  reading: 'Log okunuyor…',
  noProblems: 'Logun hâlâ hatırladığı bir sorun yok.',
  nothing: 'Henüz bir şey yok. Lambdanın adresini açın, istekler burada görünsün.',
  noMatch: 'Eşleşen bir şey yok.',
  identical: (count) => `${count} aynı satır`,
  at: (domain) => `, ${domain} üzerinden`,
  from: (country) => `, ülke: ${country}`,
};
