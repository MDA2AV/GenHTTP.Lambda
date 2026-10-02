import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Alan adı okunamadı.',
  reaching: (domain) => `${domain} alan adına gelen istekler artık bu lambdaya ulaşıyor.`,
  saveFailed: 'Alan adı kaydedilemedi.',
  removed: 'Alan adı kaldırıldı. Lambda buradaki adresinde yanıt vermeye devam ediyor.',
  removeFailed: 'Alan adı kaldırılamadı.',
  hint:
    'Premium bir lambda, buradaki adresinin yanı sıra kendi alan adında da yanıt verebilir, hem de kökten itibaren alan adının tamamında. Alan adını bu sunucuya yönlendirin ve buraya girin. O alan adına gelen istekler lambdaya ulaşır.',
  loading: 'Yükleniyor…',
  example: 'alan-adiniz.com',
  open: (domain) => `${domain} adresini aç`,
  label: 'Yanıt verdiği alan adı',
  serving: (domain) => <>Buradaki adresinin yanı sıra artık {domain} alan adında da yayında.</>,
  none: 'Henüz yok. shop.example.com gibi bir alt alan adı ya da example.com gibi bütün bir alan adı olabilir.',
  change: 'Değiştir',
  use: 'Bu alan adını kullan',
  remove: 'Kaldır',
  confirm: 'Alan adı kaldırılsın mı?',
  keep: 'Kalsın',
  confirmText: (domain) => (
    <>
      {domain} alan adına gelen istekler hemen bu lambdaya ulaşmayı bırakır. Lambdanın buradaki adresi olduğu gibi
      kalır, alan adının DNS kayıtları da.
    </>
  ),
  point: 'Alan adını bu sunucuya yönlendirin',
  check: 'Tekrar kontrol et',
  records:
    'Alan adınızın DNS kayıtlarını yönettiğiniz yerde şu iki kaydı ekleyin. IPv6 üzerinden erişilebilir olmak istemiyorsanız AAAA kaydını eklemeyin.',
  type: 'Tür',
  name: 'Ad',
  value: 'Değer',
  pointsHere: (domain) => <>{domain} alan adı bu sunucuya yönleniyor.</>,
  alsoElsewhere: (addresses) =>
    ` Ayrıca şu adreslere de çözümleniyor: ${addresses}. Bunlar bu sunucu değil, oraya giden ziyaretçiler lambdaya ulaşamaz.`,
  elsewhere: (addresses) => `Şu adreslere çözümleniyor: ${addresses}. Bunlar henüz bu sunucu değil.`,
  wait: 'Bir değişikliğin her yerde görünmesi biraz sürebilir, en fazla eski kaydın TTL süresi kadar.',
  cname: 'Bunun yerine CNAME kaydı kullanmak',
  cnameText: (target) => (
    <>
      Bir alt alan adı, bunun yerine CNAME kaydıyla {target} adresine yönlendirilebilir. O zaman bu sunucunun adresleri
      değişse bile onu takip eder. Ama bazı dezavantajları var:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Bütün bir alan adı için, yani {example} alan adının kendisi için kullanılamaz. Standart, her alan adının kökünde
      bulunan kayıtların yanında CNAME olmasına izin vermez. Bazı sağlayıcılar bunun yerine orada çalışan ALIAS, ANAME
      ya da “düzleştirilmiş” (flattened) bir kayıt sunar.
    </>
  ),
  cnameAlone: 'Aynı ada başka hiçbir kayıt eklenemez: ne e-posta için MX kaydı ne de doğrulamalar için TXT kaydı.',
  cnameLookup: 'Ziyaretçilerin çözümleyicileri, siteye ulaşmadan önce bir sorgu daha yapar.',
  copy: 'Kopyala',
  copyValue: (value) => `Kopyala: ${value}`,
};
