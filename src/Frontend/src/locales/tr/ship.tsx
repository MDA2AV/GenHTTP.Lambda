import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Laptopunuzdan herkesin ekranına.',
  intro:
    'Kodlama ajanınızla bir şey yaptınız ama yalnızca sizin bilgisayarınızda çalışıyor. Ajanınızdan onu burada yayınlamasını isteyin. Birkaç dakika sonra herkesin açabileceği bir linki olur. Üstelik bir şeyleri hatırlayabilir, yani insanlar orada birlikte oynayabilir, sohbet edebilir, paylaşım yapabilir.',
  facts: ['Ücretsiz', 'Hesap yok', 'Kurulum yok'],
  connect: 'Ajanınızı bağlayın',
  seeOthers: 'Başkalarının yayınladıklarına bakın',

  stepsTitle: 'Üç adım, biri de tek bir cümle',
  step: (n) => `Adım ${n}`,
  steps: [
    {
      title: 'Bir kez bağlayın',
      body: 'Claude, Cursor ya da hangi ajanla çalışıyorsanız, ona tek bir adres ekleyin. Bir dakikadan kısa sürer ve yalnızca bir kez yaparsınız.',
    },
    {
      title: 'Yayınlamasını isteyin',
      body: 'Ajanınıza uygulamayı burada yayınlamasını söyleyin. Ajan uygulamanızı paketler, yayınlar ve yanıt verip vermediğini kontrol eder.',
    },
    {
      title: 'Linki paylaşın',
      body: 'Herkese açık bir adres ve özel bir editör linki alırsınız. İlkini herkese gönderin. İkincisini saklayın, uygulamayı sonra onunla değiştirirsiniz.',
    },
  ],

  togetherTitle: 'Sadece bir sayfa değil. İnsanların buluştuğu bir yer.',
  together:
    'Çoğu barındırma hizmeti her ziyaretçiye uygulamanın ayrı bir kopyasını verir ve herkes tek başına oynar. Burada her uygulamanın kendi hafızası ve onu açmış herkesle canlı bir bağlantısı var. Birinin yaptığı hamle diğerlerinin ekranında anında görünür. Paylaşılanlar da ertesi gün hâlâ orada olur.',
  together2:
    'Üye olmanız gereken bir veritabanı yok, bağlamanız gereken ikinci bir servis yok. Bir arkadaşınıza anlatır gibi isteyin.',
  kinds: [
    { name: 'Çok oyunculu oyunlar', ask: 'En fazla sekiz arkadaş aynı tura katılabilsin ve birbirinin hamlelerini canlı görsün.' },
    { name: 'Sohbet odaları', ask: 'Linki olan herkesin konuşabileceği bir oda ekle, son yüz mesajı da sakla.' },
    { name: 'Ortak listeler', ask: 'Eşya listesini tüm ekip aynı anda düzenleyebilsin.' },
    { name: 'Skorlar ve rekorlar', ask: 'Herkesin en iyi süresini tutan bir skor tablosu yap, ilk onu da başlangıç ekranında göster.' },
    { name: 'Küçük sosyal ağlar', ask: 'Düğün misafirleri aynı duvara fotoğraf yükleyip birbirininkileri beğenebilsin.' },
  ],
  quote: (text) => `“${text}”`,

  connectTitle: 'Ajanınızı bir kez bağlayın',
  connectText:
    'Ajanınıza bu adresi verin. Bundan sonra burada nasıl yayınlayacağını bilir. Anahtar ya da giriş gerekmez.',
  sayLike: 'Sonra projenizde şuna benzer bir şey söyleyin',
  asks: [
    'Bu uygulamayı GenHTTP Lambda’da yayınla ve linki bana gönder.',
    'Yüksek skorları ortak yap, herkes aynı skor tablosunu görsün.',
  ],

  domainChip: 'Uygulamanız tutunca',
  domainTitle: 'Kendine ait bir adı olsun',
  domainText:
    'Aynı uygulama, aynı editör linki. Ama bu kez size ait bir adreste. Söylemesi de akılda tutması da daha kolay. İnsanlar paylaşmaya başladığında da daha profesyonel durur.',
  domainSubject: 'Uygulamam için alan adı',
  domainAsk: 'Alan adı için bize yazın',

  questionsTitle: 'Sormadan önce',
  questions: (offline, removed, showcase, terms) => [
    [
      'Gerçekten ücretsiz mi?',
      <>
        Evet. Kart yok, deneme süresi yok, hesap yok. Uygulamanız insanlar kullandığı sürece yayında kalır. Tek bir ziyaret ya
        da değişiklik olmadan {offline} gün geçerse yayından kalkar, {removed} gün geçerse silinir.
      </>,
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

  closeTitle: 'Sizin bilgisayarınızda çalışıyor.',
  closeAccent: 'Şimdi herkesinkinde çalışsın.',
  noAgent: 'Ajanınız yok mu? Burada oluşturun',
  closeFacts: 'Ücretsiz. Hesap yok. Kurulum yok.',

  scene: {
    label:
      'Bir ajandan uygulamayı yayınlaması isteniyor. Adres localhost yerine herkese açık bir linke dönüşüyor ve insanlar katılıyor.',
    ask: 'Quiz oyunumu yayına al da arkadaşlarım katılabilsin.',
    live: 'Yayında. İşte linkiniz.',
    publishing: 'Yayınlanıyor…',
    public: 'Herkese açık',
    onlyYou: 'Sadece siz',
    app: 'Cuma quiz gecesi',
    playing: (count) => <>{count} kişi oynuyor</>,
    you: 'Siz',
  },

  compareTitle: '“Çalışıyor” ile “Bir dene” arasındaki en kısa yol',
  compareText:
    'Vercel, Cloudflare ve Lovable bir şeyleri çalıştırmak için harika yerler. Ama hepsi bir kayıt formuyla başlar. Uygulamanız ziyaretçiler arasında bir şey paylaşacaksa, bir de kurulması gereken ikinci bir servis çıkar karşınıza. Sıfırdan başladığınızda tablo şöyle.',
  rows: [
    'Hesap açmadan başlamak',
    'Zaten kullandığınız ajandan yayınlamak',
    'Canlı ortak veri: sohbet, çok oyunculu, rekorlar',
    'İlk linkin maliyeti',
  ],
  us: ['Evet', 'Bir kez bağlayın, sonra isteyin', 'Her uygulamada hazır', 'Ücretsiz'],
  rivals: [
    ['Kayıt gerekli', 'Kendi araçlarına giriş yapınca', 'Veritabanı servisi eklenmeli', 'Ücretsiz plan'],
    ['Kayıt gerekli', 'Kendi araçlarına giriş yapınca', 'Mümkün, kurulumla', 'Ücretsiz plan'],
    ['Kayıt gerekli', 'Kendi editöründe oluşturulur', 'Bağlı bir backend ile', 'Ücretsiz plan, sınırlı kredi'],
  ],
  compareNote:
    'Eylül 2026 itibarıyla, hiçbir yerde hesabı olmayan biri için. Diğer servislerin planları ve özellikleri değişebilir. Ayrıntılar için kendi sitelerine bakın.',

  yourAgent: 'Ajanınız',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Bunu terminalde bir kez çalıştırın. Sonra açtığınız her proje buraya yayınlayabilir.',
    claude: (strong) => (
      <>
        Claude’un web ya da masaüstü uygulamasında {strong('Ayarlar')} bölümünü açın, ardından {strong('Connectors')}{' '}
        bölümüne gidin ve {strong('Add custom connector')} seçeneğine tıklayın. Yukarıdaki adresi yapıştırıp kaydedin.
        Hepsi bu.
      </>
    ),
    cursor: 'Bunu Cursor’ın MCP ayarlarına ya da aşağıdaki dosyaya ekleyin, sonra yeniden yükleyin.',
    vscode: 'Bunu projenize kaydedin, sonra sunucuyu Copilot Chat’in MCP görünümünden başlatın.',
  },
  elsewhere:
    'Başka bir şey mi kullanıyorsunuz? Windsurf, Codex, Zed ve diğer ajanların çoğu, ayarlarından uzak bir MCP sunucusu ekleyebilir. Onlara yukarıdaki adresi verin.',
};
