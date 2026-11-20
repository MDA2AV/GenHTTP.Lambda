import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Kendi sunucunuzda',

  eyebrow: 'Açık kaynak · bulut ya da kendi sunucunuz',
  title: 'Burada kullanın,',
  titleAccent: 'ya da kendiniz barındırın.',
  intro:
    'GenHTTP Lambda, MIT lisansıyla açık kaynaktır. genhttp.dev onu herkesin kullanması için çalıştırır; Community sürümü ise aynı platformun Docker imajıdır, kendisi çalıştırmak isteyenler için. İkisi de aynı uygulamaları yapar ve barındırır; farkları, nerede durdukları ve kimin baktığıdır.',

  cloud: {
    kicker: 'Bulut',
    name: 'genhttp.dev',
    summary: 'Bu siteyi açın ve başlayın. Sunucuya, güncellemelerine ve sınırlarına biz bakarız.',
    points: ['Kurulacak ya da çalıştırılacak bir şey yok', 'Uygulamalar genhttp.run üzerindeki bir adreste yanıt verir', 'Yapı ajanı dahil'],
    action: 'Bir şey yapın',
  },
  community: {
    kicker: 'Kendi sunucunuzda',
    name: 'Community sürümü',
    summary: 'Sizin makinenizde tek bir Docker konteyneri. Sunucuya, güncellemelerine ve sınırlarına siz bakarsınız.',
    points: ['Bir konteyner, bir disk, veritabanı sunucusu yok', 'Uygulamalar sizin adresinizde yanıt verir', 'Yapı ajanı ek bir konteyner olarak'],
    action: 'Kendiniz çalıştırın',
  },

  sameTitle: 'İkisinde de aynı',
  sameText: 'Community sürümü daha küçük bir sürüm değildir. Her uygulama, nerede çalışırsa çalışsın aynılarını alır:',
  same: [
    'Yapı ajanı',
    'MCP üzerinden kendi ajanınız',
    'REST API',
    'Editör, basit ve tam görünüm',
    'Test verili taslaklar',
    'Sürümler ve geri dönüş yolu',
    'Kayıtlar, dosyalar, anahtarlar ve parolalar',
    'Çerezsiz analitik',
    'Kendi alan adları',
    'Git ve indirmeler',
    'Vitrin ve yayımlanmış kod',
  ],

  differTitle: 'Neler farklı',
  aspect: 'Soru',
  rows: (offline) => [
    ['Uygulamalar nerede çalışır', 'Bizim sunucumuzda', 'Sizinkinde, bir konteynerde ve bir diskle'],
    ['Başlamak', 'Yapı sayfasını açın - hesap yok', 'İmajı başlatın, sonra adresini açın'],
    ['Sunucuya kim bakar', 'Biz', 'Siz'],
    ['Maliyeti', 'Yok; Premium uygulamalar istek üzerine', 'Yok; sunucunun masrafı size ait'],
    ['Uygulama nerede yanıt verir', 'genhttp.run altındaki bir alt alan adında', 'Adresinizin altında ya da verdiğiniz bir alan adının alt alan adlarında'],
    ['Kendi alan adı', 'Premium uygulamalar için', 'Production’a taşıdığınız her uygulama için'],
    ['Kimsenin kullanmadığı uygulamalar', `Ziyaret ya da değişiklik olmadan ${offline} gün geçince yayından kalkar, Premium değilse`, 'Sizin ayarladığınız gibi'],
    ['Sınırlar ve kurallar', 'Bizimkiler ve kullanım koşullarımız', 'Sizinkiler, yönetim panelinde ayarlanır'],
    ['Yapı ajanı', 'Dahil, günde belirli sayıda yapı', 'İsteğe bağlı, kendi Claude aboneliğiniz ya da API anahtarınızla'],
    ['Kullanım raporları', 'Yok', 'Günde bir kez: sürüm, makine, seçenekler ve uygulamaların sayıları - asla bir uygulama ya da kişi değil. Kapatabilirsiniz.'],
  ],

  moveTitle: 'Aralarında geçiş',
  move:
    'Aynı uygulamalar ikisinde de çalışır ve ajanınız bir uygulamayı git ile taşır: uygulamayı diğer tarafta oluşturur, eskisinin dosyalarını kopyalar ve push eder. İndirme, her yerde çalışan bir .NET projesidir; kayıtları bir SQLite dosyasındadır.',

  pickTitle: 'Hangisi size uyar',
  pickCloud: {
    title: 'genhttp.dev, eğer',
    points: [
      'hiçbir şey çalıştırmadan bir uygulamayı yayına almak istiyorsanız',
      'onu kullananlar her yerde olabiliyorsa',
      'bir sunucuya bakmak istemiyorsanız',
    ],
  },
  pickCommunity: {
    title: 'Community sürümü, eğer',
    points: [
      'uygulamalarınız ve kayıtları sizin bir makinenizde kalmalıysa',
      'ekibiniz için, kendi ağınızın içinde araçlarsa',
      'sınırları ve kuralları kendiniz koymak istiyorsanız',
    ],
  },

  startTitle: 'Başlarken',
  startCloud: {
    title: 'genhttp.dev’de',
    text: 'İhtiyacınızı yapı sayfasında anlatın ya da zaten kullandığınız ajanı bağlayın. Açılacak bir hesap yok.',
  },
  startCommunity: {
    title: 'Sunucunuzda',
    text: 'Tek bir komut başlatır. Adresini açın, ajanınızı bağlayın ve yapın - gerisini README anlatır.',
  },
  connect: 'Ajanınızı bağlayın',
  readme: 'README’yi okuyun',
  dockerHub: 'Docker Hub’daki imaj',

  sourceTitle: 'Açık kaynak',
  source: (github) => (
    <>
      GenHTTP Lambda, MIT lisansıyla yayımlanır. Kodu, sorunları ve sürümleri {github('GitHub')}’da.
    </>
  ),
};
