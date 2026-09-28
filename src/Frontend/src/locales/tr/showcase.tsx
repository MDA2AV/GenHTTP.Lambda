import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Vitrin',
  title: 'Burada yapıldı, şu an çalışıyor',
  intro:
    'Sahiplerinin göstermeyi seçtiği lambdalar. Hepsi yayında, yani her kart gerçek uygulamayı açar. Son zamanlarda kullanılanlar önce gelir.',
  counted: (total) => `${total} lambda`,
  failed: 'Vitrin yüklenemedi.',
  loadingMore: 'Daha fazlası yükleniyor…',
  showMore: 'Daha fazla göster',
  nothingTitle: 'Vitrinde henüz bir şey yok',
  nothing: (tab) => (
    <>
      Çalışan bir şey mi yaptınız? Kontrol panelini açın, {tab('Vitrin')} bölümünü seçin ve bir başlık, birkaç kelime
      ve bir görsel ekleyin. Yayında olduğu sürece burada görünür.
    </>
  ),
  buildOne: 'Uygulama oluşturun',
  yoursTitle: 'Sizinki de burada olsun mu?',
  yours: (tab) => (
    <>
      Lambdanızın kontrol panelini açıp {tab('Vitrin')} bölümünü seçin ya da onu yapan ajandan vitrine eklemesini
      isteyin. Bunu yalnızca editör anahtarına sahip olan yapabilir ve istediğiniz zaman geri kaldırabilirsiniz.
    </>
  ),
  buildSomething: 'Uygulama oluşturun',
};

export const card: Messages['card'] = {
  noPicture: 'Henüz görsel yok',
  title: 'Başlık',
  description: 'Ziyaretçiler bununla neler yapabilir?',
  opens: (title, address) => `${title}: ${address} yeni sekmede açılır`,
};
