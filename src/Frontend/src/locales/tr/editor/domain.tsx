import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Alan adı okunamadı.',
  reaching: (domain) => `${domain} alan adına gelen istekler artık bu lambdaya ulaşıyor.`,
  saveFailed: 'Alan adı kaydedilemedi.',
  removed: 'Alan adı kaldırıldı. Lambda buradaki adresinde yeniden yanıt veriyor.',
  removeFailed: 'Alan adı kaldırılamadı.',
  hint:
    'Premium bir lambda, kendi alan adında da yanıt verebilir, hem de kökten itibaren alan adının tamamında. Önce alan adını bu sunucuya yönlendirin, ardından buraya girin: bundan sonra o alan adına gelen istekler lambdaya ulaşır, buradaki adresi ise ziyaretçileri alan adına yönlendirir.',
  loading: 'Yükleniyor…',
  example: 'alan-adiniz.com',
  open: (domain) => `${domain} adresini aç`,
  label: 'Yanıt verdiği alan adı',
  serving: (domain) => <>Artık {domain} alan adında yayında. Buradaki adresi ziyaretçileri oraya yönlendiriyor.</>,
  none: 'Henüz yok. shop.example.com gibi bir alt alan adı ya da example.com gibi bütün bir alan adı olabilir.',
  change: 'Değiştir',
  use: 'Bu alan adını kullan',
  remove: 'Kaldır',
  confirm: 'Alan adı kaldırılsın mı?',
  keep: 'Kalsın',
  confirmText: (domain) => (
    <>
      {domain} alan adına gelen istekler hemen bu lambdaya ulaşmayı bırakır ve lambdanın buradaki adresi ziyaretçileri
      yönlendirmek yerine yeniden yanıt verir. Alan adının DNS kayıtları olduğu gibi kalır.
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
