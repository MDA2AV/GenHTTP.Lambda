import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: 'Durum yükleniyor…',
  readDocs: 'Dokümantasyonu oku',
  hint: (since, kept, retention, tier) =>
    `Trafik, sunucu son başladığından beri sayılıyor (${since}). ` +
    (kept
      ? `Lambda, kullanıldığı sürece yayında kalır. ${retention} gün boyunca ziyaret ya da değişiklik olmazsa silinir.`
      : `Bu lambdanın planı: ${tier}. Bu planda lambda, ne kadar az kullanılırsa kullanılsın yayında ve kayıtlı kalır.`),
  onlineFor: (duration, version) => (
    <>
      Sürüm {version}, {duration('bir')} süredir yayında.
    </>
  ),
  offline: 'Yayında değil. Bir sürüm yayına alınana kadar hiçbir şey sunulmuyor.',
  nothing: 'Henüz hiçbir şey yazılmadı.',
  requestsToday: 'bugün gelen istek',
  lastHour: (count) => `Son bir saatte ${count} istek`,
  hourly: 'Son 24 saatte saatlik istekler',
  failed: 'başarısız',
  failedTitle: (failed, rejected) =>
    `Son 24 saatte ${failed} sunucu hatası, ${rejected} bulunamayan ya da reddedilen istek`,
  average: 'ortalama yanıt süresi',
  noneYet: 'henüz yok',
  lastVisit: 'son ziyaret',
  problems: 'Son zamanlarda bir şeyler ters gitti',
  openLog: 'Logu aç',
  latest: 'Son değişiklik',
  allVersions: 'Tüm sürümler',
  noDescription: 'Açıklama yok',
  version: (version) => `Sürüm ${version}`,
  notOnline: 'henüz yayında değil',
  wanted: 'Ne istendi',
  noVersions: 'Henüz sürüm yok.',
  inProgress: 'Üzerinde çalışılanlar',
  allFeatures: 'Tüm taslaklar',
  previewOnline: 'Önizlemesi yayında',
  previewOffline: 'Önizlemesi yayında değil',
  behind: 'güncel değil',
  storage: 'Depolama',
  versionAllowance: 'Kod ve kaynaklar',
  data: 'Veriler',
};
