import type { Messages } from '../en';

export const platform: Messages['platform'] = {
  menu: 'Özellikler',

  eyebrow: 'Özellikler',
  title: 'Yapmak bir dakika sürer.',
  titleAccent: 'Gerisi hazır geliyor.',
  intro:
    'Burada yapılan bir uygulama, bir yapay zekânın yazdığı koddan fazlasıdır. Onu işe yarar kılan şey sonrasında gelir: kayıtları, sürümleri ve ne işe yaradığı, onu ziyaret edenler, kendi alan adı. Her uygulamada bunların hepsi vardır; hepsi, uygulama yapıldığında aldığınız linkle açılan tek bir yerdedir.',
  build: 'Bir şey yapın',
  ownAgent: 'Kendi ajanınızı kullanın',
  free: 'Ücretsiz. Üyelik yok, kredi kartı yok.',
  heroAlt: 'Bir kort rezervasyon uygulamasının editörü: kulübün kendi alan adında yayında, bugün kaç kişi kullandı ve son değişikliği',
  example: 'Bu sayfadaki her görsel aynı örnek uygulamayı gösterir: bir tenis kulübünün kort rezervasyonu.',

  contents: 'Bu sayfada',

  makeTitle: 'Yapın, değiştirin',
  makeText: 'İhtiyacınızı söyleyin ve söylemeye devam edin. İkisi için de kimsenin kod yazması ya da okuması gerekmez.',

  behindTitle: 'Her uygulamanın arkasında',
  behindText:
    'Bir uygulamayı işe yarar kılan, kodunun çevresindeki her şeydir. Her uygulamada bunların hepsi ilk dakikadan itibaren vardır - yalnızca kod okuyabilenler için değil, uygulamayı yapan herkes için.',

  yoursTitle: 'Daha ileri taşımak size kalmış',
  yoursText: 'Geliştiriciler için ve uygulamasının bu siteden uzun yaşamasını isteyen herkes için.',

  features: {
    build: {
      name: 'Yapın',
      title: 'Anlatın, yayında olsun',
      text: 'İhtiyacınızı ajanımıza kendi sözlerinizle anlatın - ya da Claude Code, Codex veya Cursor’dan isteyin. Birkaç dakika sonra uygulama kendi adresinde çalışır, paylaşmaya hazırdır. Kurulacak sunucu, veritabanı ya da hesap yoktur.',
      points: ['Yapı sayfasındaki ajanımız ya da MCP üzerinden kendi ajanınız', 'Dakikalar içinde paylaşılabilir bir link', 'Ücretsiz, üyeliksiz'],
      alt: 'Kort rezervasyon sayfası: dört kort, saat saat, rezerve edilen saatlerin adlarıyla',
    },
    change: {
      name: 'Değiştir',
      title: 'İsteyerek değiştirin',
      text: 'Her uygulamanın, yapıldığında aldığınız özel linkle açılan bir editörü vardır. Neyin farklı olması gerektiğini söyleyin; ajan uygulamayı değiştirir ve ne zaman bakacağınızı bildirir - ya da linki bir sonraki değişiklikle birlikte kendi ajanınıza verin.',
      points: ['Bir cümle yeterli', 'Çalışınca kendiliğinden yayına girer ya da önce denemeniz için saklanır', 'İstemediğiniz sürece kod görünmez'],
      alt: 'Değiştir bölümü: kulüp antrenörün derslerini istedi ve ajan değişikliğin denemeye hazır olduğunu söylüyor',
    },
    drafts: {
      name: 'Taslaklar',
      title: 'Bir değişikliği kimse görmeden deneyin',
      text: 'Daha büyük bir değişiklik bir taslakta yapılır: uygulamanın olacağı hali, kendi adresinde ve denemek için kayıtların bir kopyasıyla. Orada yapılan hiçbir şey gerçek kayıtlara dokunmaz. Doğru olduğunda tek tıkla yayına alınır.',
      points: ['Denemek ve başkalarına göstermek için kendi adresi', 'Test verileri: kayıtların kendisi değil, bir kopyası', 'Doğru olunca yayına alın ya da vazgeçin'],
      alt: 'Rezervasyon sayfasının bir taslağı: antrenörün dersleri, rezervasyonların bir kopyasında mor renkle gösteriliyor',
    },
    history: {
      name: 'Geçmiş',
      title: 'Her değişiklik bir sürüm, geri dönüş yolu var',
      text: 'Her değişiklik, neyi değiştirdiğiyle anlatılan yeni bir sürüm olur. Biri ters giderse öncesindeki sürüm tek tık uzakta - aynı adreste, aynı kayıtlarla.',
      points: ['Ne istendi ve neyi değiştirdi', 'Tek tıkla önceki bir sürüme dönüş', 'Kayıtlar tüm sürümlerde ortaktır; geri dönmek hiçbirini kaybettirmez'],
      alt: 'Uygulamanın geçmişi: değişiklikleri alt alta, en yenisi yayında',
    },
    about: {
      name: 'Hakkında',
      title: 'Ne olduğunu bilir',
      text: 'Ajan her değişiklikle birlikte uygulamanın ne işe yaradığını, kimlerin kullandığını ve neden böyle çalıştığını yazar. Siz bunu sade sözlerle okursunuz; bir sonraki ajan da bir şeyi değiştirmeden önce okur - böylece değişiklik tahmine değil, var olana dayanır.',
      points: ['Ne yaptığı ve kimin için, sade sözlerle', 'Arkasındaki kararlar, onu bir sonraki değiştirecek kişi için', 'Nasıl test edildiği, her sürümle birlikte saklanır'],
      alt: 'Uygulamanız hakkında: kort rezervasyonu sade sözlerle anlatılmış, kimlerin kullandığı ve ne yaptığı',
    },
    records: {
      name: 'Kayıtlar',
      title: 'Kendi veritabanında kayıtlar',
      text: 'Bir şeyleri tutan uygulama - rezervasyonlar, kayıtlar, skorlar - bunları kendi veritabanında tutar ve uygulama değişse de kalırlar. Onları editörde tablo tablo okuyun; yanında uygulamanın kaydettiği dosyalar, örneğin ilan panosu için haftalık liste.',
      points: ['İhtiyacı olan her uygulama için bir veritabanı', 'Her tablo editörde okunabilir', 'Uygulamanın kaydettiği dosyalar yanlarında listelenir'],
      alt: 'Uygulamanın kayıtları: rezervasyonları ve üyeleri, tablo tablo okunuyor',
    },
    keys: {
      name: 'Anahtarlar ve parolalar',
      title: 'Anahtarlar ve parolalar gizli kalır',
      text: 'Bir uygulamanın bir parolaya ya da başka bir hizmetin anahtarına ihtiyacı olduğunda ajan onu yalnızca adıyla ister. Değeri siz editöre yazarsınız ve o andan sonra kimse göremez - ne siz ne ajan. Yalnızca uygulama okur.',
      points: ['Ajan değeri hiç görmez', 'Şifreli tutulur, bir daha gösterilmez', 'İstediğiniz zaman değiştirilir'],
      alt: 'Anahtarlar ve parolalar: resepsiyonun parolası ayarlanmış, değeri gizli',
    },
    analytics: {
      name: 'Analitik',
      title: 'Kimlerin ziyaret ettiğini çerezsiz görün',
      text: 'Analitiği açın; uygulamayı kaç kişinin açtığını, nereden geldiklerini ve hangi cihazlardan baktıklarını görün - sunucumuzda sayılır, çerez olmadan ve sayfalarınıza bir betik eklenmeden.',
      points: ['Ziyaretçiler, sayfa görüntülemeleri ve nereden geldikleri', 'Çerez yok, ziyaretçiyi belirten hiçbir şey yok', 'Siz açana kadar kapalı'],
      alt: 'Analitik: son otuz günün ziyaretçileri, gün gün',
    },
    domain: {
      name: 'Alan adı',
      title: 'Kendi alan adı',
      text: 'Her uygulama bizim bir adresimizde başlar. Tuttuğunda, sahibi olduğunuz bir alan adına taşınabilir: aynı uygulama ve aynı editör, insanların hatırlayacağı bir adreste. Eski adrese giden linkler çalışmaya devam eder - yenisine yönlendirir.',
      points: ['Premium uygulamalar için: bize yazın', 'Aynı uygulama, editör ve kayıtlar', 'Eski adres ziyaretçileri yönlendirir'],
      alt: 'Uygulamanın adresi genhttp.run üzerindeki adresinden kulübün kendi alan adına taşınıyor',
    },
    code: {
      name: 'Kod ve git',
      title: 'Kod, ne zaman isterseniz',
      text: 'Editörü tam görünüme geçirin; her sürümün kodu, günlükleri ve rakamlarıyla orada. Uygulamayı git ile klonlayın ve bir değişiklik push edin ya da her yerde çalışan bir .NET projesi olarak indirin. Kod sizin.',
      points: ['Her dosya, sürümden sürüme karşılaştırılır', 'Git: main’e commit bir sürümdür, bir dal bir taslaktır', 'Kayıtlar dahil, kendi başına çalışan bir indirme'],
      alt: 'Editörün tam görünümü: uygulamanın dosyaları ve kodu',
    },
    publish: {
      name: 'Vitrin ve kaynak',
      title: 'Gösterin ya da kodunu yayımlayın',
      text: 'Uygulamayı başkalarının bulması için vitrine koyun ya da kodunu, herkesin okuyup yıldızlayıp indirebilmesi için açık kaynak lisansıyla yayımlayın. İkisi de sizin seçiminiz; hiçbiri kayıtlarını paylaşmaz.',
      points: ['Vitrinde, bir görsel ve bir açıklamayla', 'Kodu MIT ya da başka bir lisansla', 'Kayıtları, anahtarları ya da ziyaretçileri asla'],
      alt: 'Kort rezervasyonunun yayımlanmış kodu: dosyaları, belgeleri ve sürümleri, MIT lisansıyla',
    },
  },

  draftBadge: 'Taslak',
  domainOwn: 'Premium',
  domainSubject: 'Uygulamam için bir alan adı',
  domainAsk: 'Alan adını sorun',
  zoom: 'Büyüt',
  close: 'Kapat',

  closeTitle: 'Denemesi ücretsiz.',
  closeAccent: 'Üyelik yok.',
  closeBuild: 'Bir şey yapın',
  closeShip: 'Yaptığınızı yayınlayın',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda açık kaynaktır. Burada kullanın ya da {selfHosting('kendi sunucunuzda çalıştırın')} - kod{' '}
      {github('GitHub')}’da.
    </>
  ),
};
