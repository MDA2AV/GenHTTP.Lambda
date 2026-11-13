import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community sürümü',
  headline: 'Ajanlarınızın uygulama geliştirdiği platform.',
  headlineAccent: 'Kendi sunucunuzda çalışır.',
  intro:
    'Bir uygulamayı tarif edin, geliştirme ajanı yazsın - ya da Claude Code, Codex veya Cursor bunu MCP üzerinden yapsın. Her uygulama bir C# GenHTTP handler’ıdır; bu sunucuda derlenir, saniyeler içinde yayına girer ve veritabanı, anahtarlar ve parolalar, sürümler ve git ile gelir.',
  describe: 'Uygulamayı tarif edin',
  connect: 'Ajanınızı bağlayın',
  write: 'Kodu kendiniz yazın',

  buildTitle: 'Tarif edin',
  buildText: 'Bir cümle yeterli. Geliştirme ajanı uygulamayı yazar, yayına alır ve sonradan değiştirebilmeniz için size linkini verir.',
  noAgent: 'Bu kuruluma bağlı bir geliştirme ajanı yok. Aşağıdan kendi ajanınızı bağlayın - aynı işi o yapar.',

  connectTitle: 'Ajanınızı bağlayın',
  connectText:
    'Kodlama ajanınız platformu araç olarak kullanır: uygulama oluşturur, kodunu yazıp kontrol eder, bir değişikliği verilerin kopyasıyla bir taslakta dener, yayına alır ve günlüklerini okur.',
  connectThen: 'Sonra ondan bir şey isteyin, örneğin: “Bir ziyaretçi defteri yap, girişleri onaylayabileceğim bir sayfası olsun.”',

  featuresTitle: 'Her uygulamanın sahip olduğu her şey',
  features: [
    { title: 'Bir adres', text: 'Derlenir derlenmez bu sunucuda /lambda/ altında yayında - ya da kendi alan adında.' },
    { title: 'Kayıtlar', text: 'Kendine ait bir SQLite veritabanı; tabloları migrasyonlarla oluşturulur, Entity Framework Core ile okunur ve yazılır.' },
    { title: 'Dosyalar', text: 'Uygulamanın kaydettikleri için bir klasör: yüklemeler, dışa aktarımlar, kayıt olmayan her şey.' },
    { title: 'Anahtarlar ve parolalar', text: 'Sunucuda mühürlü saklanır. Ajan hangilerine ihtiyaç duyduğunu söyler, siz girersiniz ve kimse bunları geri okuyamaz.' },
    { title: 'Taslaklar ve sürümler', text: 'Bir değişiklik, verilerin kopyasıyla bir taslakta denenir, sonra yayına alınır. Her sürüm saklanır; geri dönmek bir tık uzakta.' },
    { title: 'Git', text: 'Her uygulama bir depodur: klonlayın, yayına almak için bir commit push edin, taslak başlatmak için bir dal push edin.' },
    { title: 'Ziyaretçiler', text: 'Sayfa görüntülemeleri, ziyaretçilerin nereden geldiği ve ülkeleri - siz açtıktan sonra sunucuda, çerez olmadan sayılır.' },
    { title: 'Kodunuz', text: 'Herhangi bir uygulamayı sade bir .NET projesi olarak indirip her yerde çalıştırın ya da kaynağını başkaları okusun diye yayımlayın.' },
  ],

  runsTitle: 'Nasıl çalışır',
  runs: [
    { title: 'Tek bir container', text: 'Sunucu, editör, API ve tüm uygulamalar tek bir süreçte; hepsinin sakladığı her şey tek bir klasörde.' },
    {
      title: 'Yola ya da ana makineye göre',
      text: 'Uygulamalar bu adreste /lambda/ altında yanıt verir. Kuruluma onlar için bir alan adı verirseniz, her biri bunun yerine kendi alt alan adını alır.',
    },
    {
      title: 'Güvendiğiniz kod',
      text: 'Uygulamalar sunucuya derlenir ve kodları yapmamaları gereken şeyler için kontrol edilir - ama sunucunun sürecini paylaşırlar. Kendiniz çalıştıracağınız şeyi çalıştırın.',
    },
    { title: 'Açık kaynak', text: 'MIT lisanslı: okuyun, değiştirin, istediğiniz yerde çalıştırın.' },
  ],
  repository: 'GitHub’daki kaynak',

  aroundTitle: 'Göz atın',
  around: {
    demos: { title: 'Bir demodan başlayın', text: 'Kayıtlar üzerinde bir API, bir giriş, canlı güncellemeler, bir oyun: her biri kopyalanıp değiştirilebilecek bitmiş bir uygulama.' },
    showcase: { title: 'Vitrin', text: 'Sahiplerinin burada göstermeyi seçtiği uygulamalar.' },
    guide: { title: 'Kılavuz', text: 'Bir uygulamanın neler yapabildiği ve nasıl, ihtiyaç duyacağınız sırayla.' },
    api: { title: 'REST API', text: 'Araçlarla aynısı, HTTP üzerinden, OpenAPI ile tanımlanmış.' },
    sources: { title: 'Yayımlanan kaynaklar', text: 'Sahiplerinin kodunu okunup indirilsin diye yayımladığı uygulamalar.' },
    admin: { title: 'Yönetim', text: 'Uygulamalar, sınırlar, engeller ve günlük; yönetici belirtecinin arkasında.' },
  },
};
