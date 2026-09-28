import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Bu kurulumda kapalı',
  off: (write, mcp) => (
    <>
      Bu kurulumda oluşturma ajanı yok. Yine de kodu {write('kendiniz yazabilir')} ya da kendi Claude ajanınızı {mcp}{' '}
      adresine bağlayabilirsiniz.
    </>
  ),

  title: 'Ne istediğinizi söyleyin.',
  intro:
    'Uygulamanız yapılır, yayına alınır ve herkese gönderebileceğiniz bir link alırsınız. Hesap yok, kurulum yok. Üstelik uygulamanız bir şeyleri hatırlayabilir: skorları, mesajları, kayıtları. Böylece açan herkes aynı şeyi görür.',
  placeholder: 'bana bir … yap',
  working: 'çalışıyor…',
  shortcut: 'ctrl + enter',
  building: 'Oluşturuluyor',
  buildIt: 'Oluştur',
  builtBy: 'Oluşturan',
  password: 'şifre',
  fable:
    'Fable deneme sürecinde şifreyle korunuyor. Süre sınırı olmadan çalışır: süre dolunca değil, iş bitince durur.',
  onlyNew:
    'Burada yalnızca yeni uygulamalar oluşturulur. Daha önce yaptığınız bir şeyi geliştirmek için editör linkini kendi kodlama ajanınıza verin. Nasıl yapılacağı aşağıda.',
  ideas: [
    'herkesin tek satırlık mesaj bırakabildiği bir duvar yap',
    'zar oyunu için bir skor tablosu hazırla',
    'insanların oy verip sonuçları gördüğü bir anket yap',
    'düğünüm için bir ziyaretçi defteri yap',
    'herkesin görebileceği bir geri sayım sayacı yap',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Sizden önce bir istek var, sıradaki sizsiniz.' : `Sizden önce ${waiting} istek var.`,
  starting: 'Başlıyor…',

  yourApp: 'Uygulamanız',
  further: 'Geliştirmeye devam etmek için',
  keep: 'Bu linki saklayın. Uygulamanıza geri dönmenin tek yolu bu ve kaybolursa biz de kurtaramayız. Sekmeyi kapatmadan önce yer imlerine ekleyin.',
  change:
    'Bu sayfa yalnızca yeni uygulamalar oluşturur. Bunu değiştirmek için kendi kodlama ajanınızı aşağıda anlatıldığı gibi bağlayın, editör linkini ona verin ve neyin değişmesini istediğinizi söyleyin.',
  copyLink: 'Editör linkini kopyala',
  lifetime: (offline, removed) =>
    `Kullanıldığı sürece yayında kalır. ${offline} gün boyunca ziyaret ya da değişiklik olmazsa yayından kalkar, ${removed} günün sonunda da silinir. Geri getirmek için editörü açıp Yayına al düğmesine basın.`,
  openEditor: 'Editörü aç',
  another: 'Başka bir şey oluştur',

  keepGoing: 'Kendi ajanınızla devam edin',
  orOwn: 'Ya da kendi ajanınızı kullanın',
  ownText:
    'Yukarıdaki kutu, bu sunucuda çalışan bir Claude. Kendi ajanınız varsa onu buraya bağlayın. Aynı şeyleri yapabilir: lambda oluşturur, kodu yazar, yayına alır. Hem de günlük sınır olmadan ve bu sayfaya uğramadan.',
  thenAsk: 'Sonra ne istediğinizi ona, burada yazdığınız gibi anlatın.',
  claudeWeb: 'Web’de Claude',
  claudeWebHow:
    'Ayarlar, sonra “Connectors”, sonra “Add custom connector”. Yukarıdaki adresi uzak MCP sunucusunun URL’si olarak yapıştırın. Anahtar yok, giriş adımı yok.',
  howToChange:
    'Oluşturduğunuz bir şeyi değiştirmenin yolu da bu: editör linkini ajanınıza verin ve ne yapması gerektiğini söyleyin.',
  more: 'Ajanla çalışmak hakkında daha fazlası',

  failedToStart: 'İstek gönderilemedi.',
  noAnswer: 'Ajan işini bitirdi ama ne olduğunu söylemedi.',
  failed: 'Bir şeyler ters gitti.',
};
