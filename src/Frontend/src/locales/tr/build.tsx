import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Fikirden web sitesine.',
  intro:
    'Aklınızdaki web sitesini veya uygulamayı anlatın. Yapay zekâ sizin için oluşturur, biz kendi sunucularımızda barındırırız ve siteniz, herkese gönderebileceğiniz bir linkle hemen yayına girer. Kodlama yok, hosting kurulumu yok, hesap yok.',
  placeholder: 'Şöyle bir web sitesi istiyorum…',
  working: 'çalışıyor…',
  shortcut: 'ctrl + enter',
  building: 'Oluşturuluyor',
  buildIt: 'Web sitemi oluştur',
  builtBy: 'Oluşturan',
  password: 'şifre',
  fable:
    'Fable deneme sürecinde şifreyle korunuyor. Süre sınırı olmadan çalışır: süre dolunca değil, iş bitince durur.',
  onlyNew:
    'Burada yeni web siteleri oluşturulur. Mevcut bir siteyi değiştirmek için editör linkini açın ve “Değiştir” bölümünde neyin farklı olması gerektiğini anlatın.',
  ideas: [
    'üyelerin etkinliklere kaydolabildiği bir kulüp sitesi',
    'düğünümüz için bir anı defteri',
    'insanların oy verip sonuçları gördüğü bir anket',
    'haftalık bilgi yarışması gecemiz için bir skor tablosu',
    'herkesin görebileceği, açılışımıza geri sayım',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Sizinkinden önce bir web sitesi var, sonra sıra sizde.' : `Sizinkinden önce ${waiting} web sitesi var.`,
  starting: 'Başlıyor…',

  points: [
    {
      title: 'Anlatılır, kodlanmaz',
      text: 'Web sitenizin ne yapması gerektiğini kendi cümlelerinizle söyleyin. Kodlama ya da teknik bilgi gerekmez.',
    },
    {
      title: 'Hosting dahil',
      text: 'Web siteniz bizim sunucularımızda çalışır. Hosting, güvenlik ve güncellemelerle biz ilgileniriz; kurmanız ya da takip etmeniz gereken bir şey yoktur.',
    },
    {
      title: 'Dakikalar içinde yayında',
      text: 'Paylaşabileceğiniz linki hemen alırsınız. Site kayıtları, oyları ve skorları da hatırlayabilir; böylece herkes aynı şeyi görür.',
    },
  ],

  yourApp: 'Web siteniz',
  further: 'Daha sonra değiştirmek için',
  keep:
    'Bu linki saklayın. Uygulamanıza geri dönmenin tek yolu bu ve kaybolursa biz de kurtaramayız. Sekmeyi kapatmadan önce yer imlerine ekleyin.',
  change:
    'Web sitenizi değiştirmek için editör linkini açın ve “Değiştir” bölümünde, burada olduğu gibi neyin farklı olması gerektiğini anlatın. Aşağıda anlatıldığı gibi kendi yapay zekâ asistanınız da bunu yapabilir.',
  copyLink: 'Editör linkini kopyala',
  lifetime: (offline, removed) =>
    `Kullanıldığı sürece yayında tutarız: ${offline} gün boyunca ziyaret veya değişiklik olmazsa yayından kaldırılır, ${removed} gün sonra da silinir. Yeniden yayına almak için editörü açın.`,
  openEditor: 'Editörü aç',
  another: 'Başka bir web sitesi oluştur',

  keepGoing: 'Kendi yapay zekâ asistanınızla devam edin',
  orOwn: 'Ya da kendi yapay zekâ asistanınızı kullanın',
  ownText:
    'Claude veya başka bir yapay zekâ asistanı mı kullanıyorsunuz? Onu buraya bağlayın; sizin için aynı şekilde web siteleri oluşturur ve değiştirir. Barındırmayı biz yaparız, yani yine kurmanız gereken bir şey yoktur. Günlük sınır yoktur.',
  ownTitle: 'Web sitenizi kendi yapay zekâ asistanınızla oluşturun',
  ownOnly:
    'Claude veya başka bir yapay zekâ asistanını aşağıdaki adrese bağlayın, ardından istediğiniz web sitesini anlatın. Asistan siteyi oluşturur, biz sunucularımızda barındırırız ve site paylaşılabilir bir linkle hemen yayına girer.',
  thenAsk: 'Sonra ona ne istediğinizi söyleyin, örneğin: “Korumuz için konser takvimi olan bir web sitesi oluştur.”',
  howToChange:
    'Bir siteyi daha sonra da böyle değiştirirsiniz: asistanınıza editör linkini verin ve neyin farklı olması gerektiğini söyleyin.',

  failedToStart: 'İstek gönderilemedi.',
  noAnswer: 'Ajan işini bitirdi ama ne olduğunu söylemedi.',
  failed: 'Bir şeyler ters gitti.',
};
