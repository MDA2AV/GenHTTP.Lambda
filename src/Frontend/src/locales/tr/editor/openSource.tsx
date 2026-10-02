import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Yükleniyor…',
  loadFailed: 'Kodun yayımlanıp yayımlanmadığı okunamadı.',
  hint: (tool) => (
    <>
      Yayımlanan bir lambdayı herkes, Açık kaynak altındaki kendi sayfasında okuyabilir, ona yıldız verebilir ve onu
      indirebilir: seçtiğiniz lisansla, her sürümüyle birlikte - ama sakladığı veriler asla dahil değildir. Onu
      yalnızca editör anahtarına sahip olan yayımlayabilir ya da yayımlamayı durdurabilir. Bir ajan da aynısını{' '}
      {tool} aracıyla yapabilir.
    </>
  ),
  hintSimple:
    'Uygulamanızın nasıl yapıldığını herkes kendi sayfasında okuyabilir ve seçtiğiniz lisansla onun üzerine bir şeyler geliştirebilir - uygulamanın sakladıkları asla buna dahil değildir. Onu yalnızca siz yayımlayabilir ya da yayımlamayı durdurabilirsiniz.',
  open: 'Kaynak kodu sayfasını aç',
  switch: 'Bu uygulamanın kodunu yayımla',
  publishedNow: (license) => `${license} lisansıyla yayımlandı. Herkes okuyabilir ve indirebilir.`,
  off: 'Kapalı. Siz yayımlayana kadar kimse kodu göremez.',
  keptStars: (stars) => `Aldığı ${stars} yıldız saklanıyor; yeniden yayımladığınızda geri gelir.`,
  published: 'Yayımlandı. Artık herkes kodu okuyabilir.',
  saved: 'Kaydedildi.',
  saveFailed: 'Kod yayımlanamadı.',
  withdrawn: 'Yayımlama durduruldu. Sayfası artık yok.',
  withdrawFailed: 'Yayımlama durdurulamadı.',
  whatTitle: 'Neler yayımlanır',
  what: [
    'Kodu: şu anki hâli ve önceki tüm hâlleri',
    'Gösterdiği her şey: sayfaları, stilleri ve görselleri',
    'Hakkında yazılanlar: ne için olduğu ve nasıl test edildiği',
    'Geçirdiği her değişiklik, her biri tek satırla',
  ],
  neverTitle: 'Neler asla yayımlanmaz',
  never: [
    'Sakladıkları: kayıtları, kaydettikleri, anahtarları ve parolaları',
    'Kendi sözlerinizle ne istediğiniz',
    'Onu kimlerin kullandığı: ziyaretçileri ve yaptıkları',
    'Editör linki',
  ],
  careful:
    'Koddaki her şey herkese açık hâle gelir, önceki hâlleri de. Bir parolanın ya da anahtarın yeri asla kod değildir: onların yeri, Veriler altındaki anahtarlar ve parolalardır ve bunlar asla yayımlanmaz.',
  licenseLabel: 'Lisans',
  licenseHint:
    'Başkalarının kodla neler yapabileceği. En yaygını olan MIT, adınız üzerinde kaldığı sürece herkesin onunla neredeyse her şeyi yapmasına izin verir.',
  readLicense: 'Lisansı oku',
  authorLabel: 'Lisanstaki ad',
  optional: 'isteğe bağlı',
  authorPlaceholder: (key) => `${key} yazarları`,
  authorHint:
    'Adınız ya da kuruluşunuzun adı; kaynak kodu sayfasında ve lisansta gösterilir. Boş bırakılırsa lisansta bu uygulamanın yazarları anılır.',
  publish: 'Yayımla',
  save: 'Değişiklikleri kaydet',
  allSaved: 'Her şey kaydedildi.',
  takeDown: 'Yayımlamayı durdur',
  confirm: 'Kodun yayımlanması durdurulsun mu?',
  confirmText:
    'Sayfası ve indirmeleri hemen kaldırılır. Onu daha önce indirmiş olanlar, birlikte geldiği lisansla saklamaya devam eder. Yıldızları ise yeniden yayımladığınızda geri gelmek üzere saklanır.',
  keep: 'Yayımlanmaya devam etsin',
  stars: (count) => `${count} yıldız`,
  sidebar: 'Kaynak kodu',
};
