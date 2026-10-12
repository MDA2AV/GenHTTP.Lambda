import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  hint: 'Platform sizin için sayar: çerez yok, uygulamanızda kod yok ve ziyaretçilerinizin kabul etmesi gereken hiçbir şey yok.',
  loading: 'Ziyaretler okunuyor…',
  loadFailed: 'Ziyaretler okunamadı.',

  offTitle: 'Uygulamanızı kimlerin ziyaret ettiğini görün',
  offText:
    'Her gün kaç kişinin geldiği, hangi sayfaları açtıkları, onları hangi sitelerin yönlendirdiği, hangi ülkelerden ve hangi cihazlarla geldikleri - açtığınız andan itibaren gün gün sayılır.',
  offPrivate: 'Çerez yok ve ziyaretçilerinizin kabul etmesi gereken bir banner yok',
  offAddresses: 'Hiçbir adres saklanmaz - yalnızca her gün kaç ziyaret olduğu saklanır',
  offKept: (days) =>
    days >= 365
      ? 'Bir yıl saklanır ve tamamı size gösterilir'
      : `Bir yıl saklanır - son ${days} gün gösterilir, tamamı Premium ile`,
  switchOn: 'Analitiği aç',
  switchedOn: 'Analitik açık. İlk ziyaretler birkaç dakika içinde görünür.',
  switchOnFailed: 'Analitik açılamadı.',
  demo: 'Bir demonun ziyaretçileri sayılmaz. Kendi kopyanız ziyaretçilerini sayabilir.',

  range: 'Zaman aralığı',
  days: (days) => (days >= 365 ? '12 ay' : `${days} gün`),
  premiumRange: 'Premium bir yıla kadar gösterir',

  visitors: 'Ziyaretçiler',
  visitorsHint: 'Farklı kişiler, her biri günde bir kez sayılır.',
  views: 'Sayfa görüntülemeleri',
  viewsHint: 'Birinin bir sayfayı her açışı.',
  hits: 'İstekler',
  hitsHint: 'Uygulamanızdan istenen her şey: sayfaları ve onların yüklediği resimler ile scriptler.',
  versus: (days) => `önceki ${days} güne göre`,
  noChange: 'değişmedi',
  inDays: (days) => (days >= 365 ? 'son 12 ayda' : `son ${days} günde`),

  perDay: {
    visitors: 'Günlük ziyaretçiler',
    views: 'Günlük sayfa görüntülemeleri',
    hits: 'Günlük istekler',
  },
  perWeek: {
    visitors: 'Haftalık ziyaretçiler',
    views: 'Haftalık sayfa görüntülemeleri',
    hits: 'Haftalık istekler',
  },
  showValues: 'Değerleri göster',
  showChart: 'Grafiği göster',
  day: 'Gün',
  week: 'Hafta başlangıcı',

  nobody: 'Bu günlerde kimse ziyaret etmedi.',
  nobodyYet: 'Analitik açıldığından beri kimse ziyaret etmedi. İlk ziyaretler birkaç dakika içinde görünür.',

  pages: 'Sayfalar',
  pagesHint: 'İnsanların açtığı sayfalar, en çok açılan başta.',
  requested: 'En çok istenenler',
  requestedHint: 'Uygulamanızdan istenen her şey, adrese göre - insanların açtığı sayfaları yok.',
  sources: 'Ziyaretçiler nereden geliyor',
  sourcesHint: 'İnsanları buraya yönlendiren siteler, bulundukları sayfaya göre. Arama motorları yalnızca kendilerinin yönlendirdiğini söyler, neyin arandığını söylemez.',
  direct: 'Adres çubuğu, yer imi ya da bir uygulama',
  countries: 'Ülkeler',
  countriesHint: 'Ziyaretçilerin internet adreslerinin kayıtlı olduğu yer - çoğunlukla, ama her zaman değil, bulundukları yer.',
  unknownCountry: 'Bilinmiyor',
  devices: 'Cihazlar',
  device: {
    desktop: 'Bilgisayar',
    mobile: 'Telefon',
    tablet: 'Tablet',
    bot: 'Botlar',
    other: 'Diğer',
  },
  browsers: 'Tarayıcılar',
  systems: 'Sistemler',
  crawlers: 'Arama motorları ve yapay zekâ',
  crawlersHint: 'Uğrayan programlar: sayfalarınızı dizinleyen arama motorları, onları okuyan yapay zekâ asistanları, bir linkin önizlemesini gösteren sohbetler.',
  noCrawlers: 'Bu günlerde hiçbir arama motoru ya da yapay zekâ uğramadı.',
  other: 'Diğerleri',
  otherBots: 'Diğer programlar',
  showAll: (count) => `Tümünü göster (${count})`,
  showFewer: 'Daha az göster',
  share: (part, whole) => `${whole} içinden ${part}`,
  none: 'Henüz hiçbir şey yok.',

  moreKept: (since) => `${since} tarihinden beri sayılıyor - Premium, bir yıla kadar tamamını gösterir.`,
  premiumAsk: 'Premium hakkında sorun',
  premiumSubject: 'Uygulamam için Premium',
  premiumBody: (address) => `Merhaba,\n\nUygulamamın tüm analitik verilerini görmek istiyorum: ${address}\n\n`,

  footer: (minutes) =>
    minutes === 1 ? 'Günlük sayılır, UTC’ye göre. Her dakika güncellenir.' : `Günlük sayılır, UTC’ye göre. Her ${minutes} dakikada bir güncellenir.`,
  updated: (time) => `Son güncelleme: ${time}.`,
  since: (date) => `Sayım başlangıcı: ${date}.`,

  switchOff: 'Analitiği kapat',
  switchOffTitle: 'Analitik kapatılsın mı?',
  switchOffText: 'Sayım durur ve şimdiye kadar sayılan her şey silinir. Bu geri alınamaz.',
  keep: 'Saymaya devam et',
  switchOffConfirm: 'Kapat ve sil',
  switchedOff: 'Analitik kapalı ve sayılanlar silindi.',
  switchOffFailed: 'Analitik kapatılamadı.',
};
