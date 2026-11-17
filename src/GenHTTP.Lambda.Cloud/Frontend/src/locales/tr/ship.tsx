import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Vibe coding için ücretsiz hosting',
  title: 'Localhost’tan herkesin ekranına.',
  intro:
    'Claude Code, Codex ya da Cursor ile bir uygulama yaptınız ama yalnızca sizin bilgisayarınızda çalışıyor. Ajanınızdan onu burada yayınlamasını isteyin. Birkaç dakika sonra herkesin açabileceği bir linki, kendi veritabanı ve onu açmış herkesle canlı bir bağlantısı olur. Böylece insanlar orada birlikte oynayabilir, sohbet edebilir, paylaşım yapabilir.',
  facts: ['Ücretsiz', 'Üyelik yok', 'Kredi kartı yok', 'Kurulum yok'],

  stepsTitle: 'Localhost’taki uygulamanızı üç adımda yayınlayın',
  step: (n) => `Adım ${n}`,
  steps: [
    {
      title: 'Bir kez bağlayın',
      body: 'Claude Code, Codex, Cursor ya da hangi ajanla çalışıyorsanız, ona tek bir adres ekleyin: bir uzak MCP sunucusu. Bir dakikadan kısa sürer ve yalnızca bir kez yaparsınız.',
    },
    {
      title: 'Yayınlamasını isteyin',
      body: 'Ajanınıza uygulamayı burada yayınlamasını söyleyin. Ajan uygulamanızı paketler, yayınlar ve yanıt verip vermediğini kontrol eder. GitHub reposu, deploy pipeline’ı ya da Docker gerekmez.',
    },
    {
      title: 'Linki paylaşın',
      body: 'Herkese açık bir adres ve özel bir editör linki alırsınız. İlkini herkese gönderin. İkincisini saklayın, uygulamayı sonra onunla değiştirirsiniz.',
    },
  ],

  togetherTitle: 'Sadece hosting değil. Veritabanı ve çok oyunculu mod dahil.',
  together:
    'Çoğu hosting hizmeti her ziyaretçiye uygulamanın ayrı bir kopyasını verir ve herkes tek başına oynar: bir tarayıcının localStorage’da tuttuğunu bir sonraki hiç görmez. Burada her uygulamanın kendi veritabanı ve onu açmış herkesle canlı bir bağlantısı var. Birinin yaptığı hamle diğerlerinin ekranında anında görünür. Paylaşılanlar da ertesi gün hâlâ orada olur.',
  sayLike: 'Sonra projenizde şuna benzer bir şey söyleyin',
  asks: [
    'Bu uygulamayı GenHTTP Lambda’da yayınla ve linki bana gönder.',
    'Yüksek skorları ortak yap, herkes aynı skor tablosunu görsün.',
  ],

  questionsTitle: 'Yayınlamadan önce',
  questions: (offline, removed, showcase, terms, selfHosting) => [
    [
      'Gerçekten ücretsiz mi?',
      <>
        Evet. Üyelik yok, kredi kartı yok, deneme süresi yok. Uygulamanız insanlar kullandığı sürece yayında kalır. Tek bir
        ziyaret ya da değişiklik olmadan {offline} gün geçerse yayından kalkar, {removed} gün geçerse silinir.
      </>,
    ],
    [
      'Claude Code, Codex ya da Cursor uygulamamı burada yayınlayabilir mi?',
      'Evet; uzak MCP sunucusu ekleyebilen her ajan yayınlayabilir. Yukarıdaki adresle bir kez bağlayın, sonra yayınlamasını isteyin: ajan uygulamayı yayınlar, yanıt verip vermediğini kontrol eder ve linki size gönderir.',
    ],
    [
      'Arkadaşlarım localhost linkimi neden açamıyor?',
      'Çünkü localhost sizin kendi bilgisayarınızdır: adres yalnızca orada ve yalnızca uygulama çalışırken işe yarar. Bir tünel, laptopunuz açık kaldığı sürece ona herkese açık bir adres ödünç verir. Burada yayınlanan uygulama ise bizim sunucularımızda çalışır; linki, laptopunuz kapalıyken de açılır.',
    ],
    [
      'Sunucu, backend ya da Supabase gerekir mi?',
      'Hayır. Her uygulamanın kendi veritabanı, dosya depolama alanı ve onu açmış herkesle canlı bir bağlantısı vardır. Kiralanacak bir sunucu ya da kurulacak ikinci bir servis yoktur; sizin tarafınızda çalışır halde tutmanız gereken bir şey de yoktur.',
    ],
    [
      'Oyunumu sunucu çalıştırmadan çok oyunculu yapabilir miyim?',
      'Evet. Bir tarayıcının localStorage’da tuttuğunu bir sonraki hiç görmez; bu yüzden ortak kısmın bir sunucuda durması gerekir. Burada o sunucu bizim. Ajanınızdan oyunu çok oyunculu yapmasını isteyin; her hamle, oyunu açmış herkese ulaşır.',
    ],
    [
      'Uygulamamın belli bir şekilde yapılmış olması gerekiyor mu?',
      'Hayır, bununla ajanınız ilgilenir. Sayfalar, görseller ve stiller olduğu gibi yüklenir. Sunucuda çalışması gereken kısmı ise ajan bu platforma uyarlar. Siz uygulamanın ne yapacağını anlatırsınız, teknik kısmı ajan halleder.',
    ],
    [
      'Sonradan nasıl değiştiririm?',
      'Yayınlandığında aldığınız editör linkiyle. Bir sonraki değişiklikle birlikte ajanınıza verin ya da tarayıcınızda açın. Her değişiklik aynı adreste yeni bir sürüm olur. İstediğiniz zaman eski bir sürüme dönebilirsiniz.',
    ],
    [
      'git ile push edebilir miyim?',
      'Evet. Her uygulama aynı zamanda bir git deposudur: editördeki Klonla altındaki adresten klonlayın, kendi araçlarınız ya da ajanınızla değiştirin ve push edin. main’e push edilen her commit sonraki sürüm olur, push ettiğiniz bir dal ise kendi önizlemesi olan bir taslak olur.',
    ],
    [
      'API anahtarlarım nereye gider?',
      'Koda değil. Ajanınız anahtarı adıyla ister, değerini siz editöre yazarsınız. Kimse onu geri okuyamaz; ne editör ne de ajan.',
    ],
    [
      'Kodumu alıp götürebilir miyim?',
      'Evet, kod sizin. İstediğiniz zaman editörden, kendi başına çalışan bir proje olarak indirin; veritabanı da dahil. Ya da git ile klonlayın; her sürüm de dahil.',
    ],
    [
      'Bunu bunun yerine kendi sunucumda barındırabilir miyim?',
      <>
        Evet. GenHTTP Lambda açık kaynaktır; Community sürümü aynı platformdur, tek bir Docker konteyneri olarak, kendi
        sunucunuz ve kendi ajanınız için. {selfHosting('Bulut mu, kendi sunucunuz mu')} neyin farklı olduğunu anlatır.
      </>,
    ],
    [
      'Uygulamamı kimler görebilir?',
      <>Linki verdiğiniz herkes. Siz {showcase('vitrine')} eklemedikçe hiçbir yerde listelenmez.</>,
    ],
    [
      'Yayınlayamayacağım bir şey var mı?',
      <>
        Birkaç şey var; örneğin insanlara zarar veren ya da onları kandıran her şey. {terms('Kullanım koşulları')} kısa
        ve sade bir dille yazıldı.
      </>,
    ],
  ],
  noAgent: 'Ajanınız yok mu? Burada oluşturun',

};
