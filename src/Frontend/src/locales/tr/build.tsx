import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Yapay zekâ ile web sitesi yapın.',
  intro:
    'Aklınızdaki web sitesini veya uygulamayı kendi cümlelerinizle anlatın. Yapay zekâ sizin için yapar, biz barındırırız ve siteniz dakikalar içinde, herkese gönderebileceğiniz bir linkle yayında olur. Ücretsiz; kod bilmenize ya da üye olmanıza gerek yok.',
  placeholder: 'Şöyle bir web sitesi istiyorum…',
  shortcut: 'ctrl + enter',
  buildIt: 'Web sitemi oluştur',
  builtBy: 'Oluşturan',
  password: 'şifre',
  fable:
    'Fable deneme sürecinde şifreyle korunuyor. Süre sınırı olmadan çalışır: süre dolunca değil, web siteniz bitince durur.',
  onlyNew:
    'Burada yeni web siteleri oluşturulur. Mevcut bir siteyi değiştirmek için editör linkini açın ve “Değiştir” bölümünde neyin farklı olması gerektiğini anlatın.',
  ideas: [
    'üyelerin etkinliklere kaydolabildiği bir kulüp sitesi',
    'kimse aynı yemeği getirmesin diye bir piknik listesi',
    'düğünümüz için bir anı defteri',
    'insanların oy verip sonuçları gördüğü bir anket',
    'haftalık bilgi yarışması gecemiz için bir skor tablosu',
    'arkadaşların iyi dileklerini yazdığı bir doğum günü sayfası',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Sizinkinden önce bir web sitesi var, sonra sıra sizde.' : `Sizinkinden önce ${waiting} web sitesi var.`,
  starting: 'Başlıyor…',
  asked: 'İsteğiniz',
  leaveOpen: 'Bu sayfayı açık bırakın: web sitenizi daha sonra değiştirmek için gereken link yalnızca burada, siteniz hazır olduğunda gösterilir.',
  log: 'Yapay zekânın yaptıkları',
  online: 'Web siteniz yayında',
  notOnline: 'Web siteniz oluşturuldu, ancak yayına alınmadı.',
  open: 'Web sitesini aç',
  steps: {
    guide: 'Hazırlanıyor',
    examples: 'Örneklere bakılıyor',
    create: 'Web siteniz için bir adres seçiliyor',
    write: 'Web siteniz yazılıyor',
    improve: 'Web siteniz iyileştiriliyor',
    check: 'Hatalar kontrol ediliyor',
    online: 'Yayına alınıyor',
    trying: 'Deneniyor',
    looking: 'Web sitenize bakılıyor',
    forRecords: 'Kayıtlar için yer hazırlanıyor',
    forKeys: 'Anahtarlar ve parolalar için yer hazırlanıyor',
    forFiles: 'Kaydedecekleri için yer hazırlanıyor',
    records: 'Kayıtlara bakılıyor',
    keys: 'Hangi anahtar ve parolaların gerektiği kontrol ediliyor',
    addFile: 'Dosya ekleniyor',
    removeFile: 'Dosya kaldırılıyor',
    files: 'Kaydedilenlere bakılıyor',
  },

  points: [
    {
      title: 'Kod bilmenize gerek yok',
      text: 'Web sitenizin ne yapması gerektiğini, bir arkadaşınıza anlatır gibi kendi cümlelerinizle söyleyin. Yapay zekâ sizin için yapar; teknik bilgi gerekmez.',
    },
    {
      title: 'Ücretsiz hosting dahil',
      text: 'Web siteniz bizim sunucularımızda çalışır. Hosting paketi, sunucu ya da domain satın almanız, bir şey kurmanız gerekmez; güvenlik ve güncellemelerle biz ilgileniriz.',
    },
    {
      title: 'Dakikalar içinde yayında',
      text: 'Paylaşabileceğiniz linki hemen alırsınız. Site, insanların girdiklerini hatırlar: kayıtlar, oylar, mesajlar, skorlar. Böylece herkes aynı şeyi görür.',
    },
  ],

  questionsTitle: 'Başlamadan önce',
  questions: (offline, removed) => [
    [
      'Yapay zekâ ile gerçekten ücretsiz web sitesi yapılabilir mi?',
      `Evet. Kendi cümlelerinizle anlatın; yapay zekâ siteyi yapar, yayına alır ve linkini size verir. Üyelik yok, kredi kartı yok, deneme süresi yok. Site, insanlar kullandığı sürece yayında kalır: ${offline} gün boyunca ziyaret veya değişiklik olmazsa yayından kaldırılır, ${removed} gün sonra da silinir.`,
    ],
    [
      'Hosting, sunucu ya da domain gerekir mi?',
      'Hayır. Web siteniz bizim sunucularımızda çalışır; hosting, güvenlik ve güncellemeler dahildir. Linkinizi hemen alırsınız, yani domain satın almanız da gerekmez.',
    ],
    [
      'Kod bilmeden uygulama yapabilir miyim?',
      'Evet. Hiç kod görmezsiniz. Ne yapması gerektiğini bir arkadaşınıza anlatır gibi söyleyin, gerisini yapay zekâ halleder: bir web sitesi, küçük bir uygulama ya da bir oyun.',
    ],
    [
      'İnsanlar bir şeyler girebilir mi? Kayıt, oy, mesaj?',
      'Evet. Web siteniz insanların girdiklerini hatırlar; linki açan herkes aynı kayıtları, oyları ve skorları görür.',
    ],
    [
      'Başkaları siteyi nasıl açar?',
      'Linkle; herhangi bir tarayıcıda, telefonda ya da bilgisayarda. Kurulacak bir şey yoktur, arada bir uygulama mağazası da yoktur.',
    ],
    [
      'Sonradan nasıl değiştiririm?',
      'Web sitenizle birlikte aldığınız editör linkini açın ve burada olduğu gibi neyin farklı olması gerektiğini anlatın. Bir değişikliği beğenmezseniz sitenin önceki hâline dönebilirsiniz.',
    ],
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
