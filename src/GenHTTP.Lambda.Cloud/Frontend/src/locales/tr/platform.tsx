import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'Uygulama yapan yapay zekâ, hosting dahil',
  title: 'Yapmak bir dakika sürer.',
  titleAccent: 'Gerisi hazır geliyor.',
  intro:
    'Uygulamanızı yapay zekâ ajanımıza - ya da Claude Code, Codex veya Cursor’a - anlatın; dakikalar içinde yayında olsun, sizin için barındırılsın, paylaşabileceğiniz bir linki olsun. Onu işe yarar kılan her şey hazır: kayıtları, sürümleri ve ne işe yaradığı, onu kimlerin ziyaret ettiği ve tutunduğunda kendi alan adı.',
  build: 'Bir şey yapın',
  ownAgent: 'Kendi ajanınızı kullanın',
  free: 'Ücretsiz. Üyelik yok, kredi kartı yok.',
  heroAlt: 'Bir kort rezervasyon uygulamasının editörü: kulübün kendi alan adında yayında, bugün kaç kişi kullandı ve son değişikliği',
  example: 'Bu sayfadaki her görsel aynı örnek uygulamayı gösterir: bir tenis kulübünün kort rezervasyonu.',

  contents: 'Bu sayfada',

  makeTitle: 'Yapın, değiştirin',
  makeText: 'İhtiyacınızı söyleyin ve söylemeye devam edin. İkisi için de kimsenin kod yazması ya da okuması gerekmez.',

  behindTitle: 'Her uygulama için veritabanı, sürüm geçmişi ve analitik',
  behindText:
    'Bir uygulamayı işe yarar kılan, kodunun çevresindeki her şeydir - yalnızca kod okuyabilenler için değil, uygulamayı yapan herkes için orada.',

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
      title: 'Sorun, değiştirin',
      text: 'Uygulamalarınız oluşturma sayfasında listelenir, her birinin bir sohbeti vardır. Nasıl gittiğini sorun - kimler ziyaret etti, insanlar ne girdi, bir şey ters mi gidiyor - ya da neyin farklı olması gerektiğini söyleyin; ajan yanıtlar ya da değiştirir. Ya da editör linkini kendi ajanınıza verin.',
      points: ['Ziyaretçiler, kayıtlar ve hatalar hakkında yanıtlar', 'Çalışınca kendiliğinden yayına girer ya da önce denemeniz için taslak olarak saklanır', 'İstemediğiniz sürece kod görünmez'],
      alt: 'Oluşturma sayfası: kulüp bu hafta kaç kortun ayrıldığını sordu, ajan yanıtladı ve antrenörün dersleri bir taslakta denemeye hazır',
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
