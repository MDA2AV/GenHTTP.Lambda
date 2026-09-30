import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Ana menü',
  build: 'Web sitesi oluştur',
  ship: 'Yayınla',
  showcase: 'Vitrin',
  enterprise: 'Kurumsal',
  docs: 'Belgeler',
  admin: 'Yönetim',
  lightMode: 'Açık temaya geç',
  darkMode: 'Koyu temaya geç',
  openMenu: 'Menüyü aç',
  closeMenu: 'Menüyü kapat',
  language: 'Dil',
  terms: 'Kullanım koşulları',
  privacy: 'Gizlilik politikası',
  imprint: 'Künye',
  writeCode: 'Kodu kendiniz yazın',
  contact: 'İletişim',
};

export const common: Messages['common'] = {
  loading: 'Yükleniyor…',
  loadingEditor: 'Editör yükleniyor…',
  editorFailed: 'Editör yüklenemedi',
  pageFailed: 'Sayfa yüklenemedi',
  editorFailedWhy: 'Bu genellikle sekme açıkken sitenin güncellendiği anlamına gelir.',
  reload: 'Sayfayı yenile',
  backToStart: 'Ana sayfaya dön',
  tryAgain: 'Tekrar dene',
  copy: 'Kopyala',
  copied: 'Kopyalandı',
  copyToClipboard: 'Panoya kopyala',
  openInNewTab: 'Yeni sekmede aç',
  close: 'Kapat',
  operatorCountry: 'Almanya',
};

export const notFound: Messages['notFound'] = {
  title: 'Sayfa bulunamadı',
  heading: 'Böyle bir sayfa yok',
  text: 'Link eskimiş ya da gösterdiği lambda silinmiş olabilir.',
};

export const missing: Messages['missing'] = {
  title: 'Burada çalışan bir şey yok',
  heading: 'Burada çalışan bir şey yok',
  notDeployed: (key) => (
    <>
      {key} adresinde bir lambda var ama şu anda yayında değil. Ücretsiz plandaki lambdalar kullanıldıkları sürece
      yayında kalır; bir ay boyunca ziyaret ya da değişiklik olmazsa yayından kaldırılır. Editör linki elinde olan
      herkes onu yeniden yayına alabilir.
    </>
  ),
  unknown: (key) => (
    <>
      {key} adresinde barındırılan bir lambda yok. Böyle bir anahtar hiç olmamış olabilir ya da arkasındaki lambda
      silinmiş olabilir.
    </>
  ),
  create: 'Bu adreste lambda oluştur',
};

export const abuse: Messages['abuse'] = {
  report: 'Kötüye kullanımı bildir',
  title: 'Bir lambdayı bildirin',
  write: 'Bize yazın',
  subject: 'Kötüye kullanım bildirimi',
  intro:
    'Burada herkes kod yayınlayabilir. Bu da bazen birinin yayınlamaması gereken bir şeyi yayınlaması demek. Burada barındırılan bir sayfa insanları kandırmaya çalışıyorsa, bir yere saldırıyorsa ya da hakkı olmayan içerik kullanıyorsa bize haber verin, kaldıralım.',
  how: (mailbox, strong, path) => (
    <>
      {mailbox} adresine yazın. {strong('Sayfanın adresini')} ({path} biçiminde) ve sorunu anlatan bir cümle ekleyin.
      Ekran görüntüsü de çok işe yarar. Hesabınızın olması ya da bu siteyi kullanıyor olmanız gerekmez.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Sonra ne olur?')} Bildiriminizi bir insan okur. Lambda {terms('kullanım koşullarını')} ihlal ediyorsa,
      genellikle bir gün içinde yayından kaldırılır. Kimin yayınladığını size söylemeyiz ve her bildirime yanıt
      vereceğimize söz veremeyiz. Ama hepsi okunur.
    </>
  ),
  danger:
    'Biri acil bir tehlike altındaysa ya da bir suç işleniyorsa lütfen yerel yetkililere de başvurun. Biz bir sayfayı kaldırabiliriz ama fazlasını yapamayız.',
};
