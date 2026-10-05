import type { SourceMessages } from '../en/source';

/** Tarayıcının küçük harfle yazdığı bir zaman ("dün", "geçen hafta") satırı açtığında ilk harfi büyük olsun diye. */
const capital = (text: string) => text.charAt(0).toLocaleUpperCase('tr') + text.slice(1);

/** Yayımlanan kaynak kodu sayfalarının (/source ve altındaki her proje) Türkçe metinleri. */
export const source: SourceMessages = {
  shell: {
    section: 'Açık kaynak',
    home: 'GenHTTP Lambda, ana sayfa',
  },

  lambda: {
    label: 'Lambda nedir?',
    text: 'GenHTTP Lambda’da çalışan bir web uygulaması: biri ne istediğini söyler, bir yapay zekâ ajanı onu C# ile yazar ve uygulama birkaç dakika içinde kendi adresinde yayına girer. Her sürüm, neyi değiştirdiğiyle birlikte saklanır.',
    build: 'Kendi uygulamanızı oluşturun',
  },

  catalog: {
    eyebrow: 'Açık kaynak',
    title: 'Buradaki uygulamaların nasıl yapıldığını görün',
    intro:
      'Sahiplerinin kodunu yayımladığı lambdalar: her sürüm, neyi değiştirdiği, dokümantasyonu ve testleri. Burada okuyun ya da .NET’in çalıştığı her yerde çalışan bir proje olarak indirin.',
    searchLabel: 'Projelerde ara',
    searchPlaceholder: 'Ada ya da ne yaptığına göre arayın',
    orderLabel: 'Sıralama',
    orders: {
      stars: 'En çok yıldız alanlar',
      updated: 'Son değiştirilenler',
      published: 'Yeni yayımlananlar',
    },
    counted: (total) => `${total} proje`,
    failed: 'Projeler yüklenemedi.',
    loadingMore: 'Daha fazlası yükleniyor…',
    showMore: 'Daha fazla göster',
    nothingTitle: 'Henüz yayımlanan bir şey yok',
    nothing: (tab) => (
      <>
        Başkalarının bir şeyler öğrenebileceği bir şey mi yaptınız? Kontrol panelini açın, {tab('Açık kaynak')}{' '}
        bölümünü seçin ve bir lisans belirleyin; kodu burada görünür.
      </>
    ),
    noMatchTitle: 'Eşleşen bir şey yok',
    noMatch: (query) => `Yayımlanan hiçbir projede “${query}” geçmiyor.`,
    clear: 'Tüm projeleri göster',
    yoursTitle: 'Sizinkini de yayımlayın',
    yours: (tab) => (
      <>
        Lambdanızın kontrol panelini açıp {tab('Açık kaynak')} bölümünü seçin ya da onu yapan ajandan yayımlamasını
        isteyin. Bunu yalnızca editör anahtarına sahip olan, seçtiği lisansla yapabilir. Uygulamanın sakladıkları
        (kayıtları, dosyaları ve anahtarları) asla buna dahil değildir.
      </>
    ),
    build: 'Uygulama oluşturun',
    online: 'Yayında',
    offline: 'Yayında değil',
    changed: (ago) => `${ago} değiştirildi`,
    stars: (count) => `${count} yıldız`,
  },

  project: {
    loading: 'Kaynak kodu yükleniyor…',
    failed: 'Kaynak kodu yüklenemedi.',
    missingTitle: 'Burada yayımlanmış bir kaynak kodu yok',
    missing: 'Sahibi onu kaldırmış olabilir ya da bu adreste hiç lambda olmamış olabilir.',
    all: 'Tüm projeler',
    by: (name) => `${name} tarafından`,
    versions: (count) => `${count} sürüm`,
    onlineAt: (address) => <>{address} adresinde yayında</>,
    offline: 'Şu anda yayında değil',
    openApp: 'Uygulamayı aç',
    opens: (address) => `${address} adresini yeni sekmede açar`,
    published: (ago) => `${capital(ago)} yayımlandı`,
    changed: (ago) => `${capital(ago)} değiştirildi`,
    picture: (name) => `${name}, göründüğü hâliyle`,
    tabsLabel: 'Okunacaklar',
    tabs: {
      code: 'Kod',
      docs: 'Dokümantasyon',
      tests: 'Testler',
      changes: 'Değişiklikler',
    },
  },

  versions: {
    label: 'Sürüm',
    choose: 'Başka bir sürümü oku',
    newest: 'en yeni',
    online: 'yayında',
    older: (version, ago, newest) =>
      `Şu anda ${ago} kaydedilen ${version}. sürümü okuyorsunuz. En yenisi ${newest}. sürüm.`,
    toNewest: 'En yenisini oku',
    noChange: 'Neyi değiştirdiğine dair bir not yok',
  },

  star: {
    star: 'Yıldız ver',
    add: 'Bu projeye yıldız ver',
    remove: 'Yıldızınızı geri alın',
    count: (count) => `${count} yıldız`,
    failed: 'Yıldız kaydedilemedi.',
  },
  clone: {
    button: 'Kod',
    title: 'git ile klonla',
    what: (oldest, newest) =>
      oldest === newest
        ? `Sürümü, main’in v${newest} etiketli commit’idir.`
        : `Her sürüm main’in bir commit’i olarak gelir, v${oldest} ile v${newest} arası etiketlenir - main en yenisidir.`,
    readOnly:
      'Salt okunur. Üzerine bir şey kurmak için kendi lambdanızı başlatın ve bu dosyaları oraya taşıyın - klondaki AGENTS.md nasıl yapılacağını, lisansı ise neler yapabileceğinizi söyler.',
  },

  download: {
    title: (version) => `${version}. sürüm, proje olarak`,
    what:
      'Dockerfile, dokümantasyonu, testleri ve lisansıyla birlikte bir .NET 10 projesi. Uygulamanın sakladıkları (kayıtları, kaydettiği dosyalar ve anahtarları) buna dahil değildir.',
    zip: 'ZIP olarak indir',
    preparing: 'Proje hazırlanıyor…',
    slow: 'Bir sürüm ilk kez indirildiğinde, siz beklerken paketlenir.',
    failed: 'Proje hazırlanamadı. Birazdan tekrar deneyin.',
    run: 'Çalıştırın',
    local: '.NET 10 SDK ile:',
    container: 'Ya da bir container içinde:',
    agent: 'Ya da klasörü kendi kodlama ajanınıza verip üzerine geliştirmeye devam edin - lisansına uyarak.',
    copy: 'Kopyala',
    copied: 'Kopyalandı',
  },

  tree: {
    label: 'Dosyalar',
    files: (count) => `${count} dosya`,
    packing: 'Bu sürüm paketleniyor…',
    packingSlow: 'Bir sürüm, biri onu ilk kez okuduğunda paketlenir; büyük bir sürümde bu biraz zaman alır.',
    failed: 'Bu sürümün dosyaları yüklenemedi.',
    legend: 'Hangisi ne',
    kinds: {
      code: 'Lambdanın kendi kodu',
      asset: 'Sunduğu şeyler: sayfalar, scriptler, stiller, görseller - ve veritabanı migration’ları',
      docs: 'Ne olduğu ve neden bu şekilde yapıldığı',
      tests: 'Nasıl test edildiği',
      dev: 'Kodunun veya statik dosyalarının bir derleme aracıyla neyden derlendiği',
      platform: 'Platformun yerini tutanlar',
      project: 'Host, build, container ve lisans',
    },
    short: {
      code: 'Kod',
      asset: 'Sunulan',
      docs: 'Dokümanlar',
      tests: 'Testler',
      dev: 'Derleme',
      platform: 'Platform',
      project: 'Proje',
    },
  },

  file: {
    loading: 'Yükleniyor…',
    failed: 'Bu dosya yüklenemedi.',
    missing: (path) => `Bu sürümde ${path} yok.`,
    binary: 'Bu dosya metin değil.',
    tooLarge: 'Bu dosya burada gösterilemeyecek kadar uzun.',
    download: 'İndir',
    raw: 'Ham',
    rawTitle: 'Dosyayı olduğu gibi aç',
    copy: 'Kopyala',
    copied: 'Kopyalandı',
    lines: (count) => `${count} satır`,
    plain: 'Uzun olduğu için renklendirilmeden gösteriliyor.',
    line: (line) => `Satır ${line}`,
  },

  docs: {
    pages: 'Sayfalar',
    product: 'Ne olduğu',
    decisions: 'Kararlar',
    loading: 'Yükleniyor…',
    failed: 'Bu sayfa yüklenemedi.',
    noneTitle: 'Bu sürüm hakkında bir şey yazılmamış',
    none: 'Dokümantasyonu docs/ içinde olurdu: uygulamanın ne olduğu, kimin için olduğu ve neden bu şekilde yapıldığı.',
  },

  tests: {
    files: 'Scriptler ve veriler',
    noneTitle: 'Bu sürüm testleri hakkında bir şey söylemiyor',
    none: 'Nasıl test edildiği, çalıştırdığı scriptlerle birlikte tests/README.md içinde olurdu.',
  },

  changes: {
    title: 'Tüm sürümler, en yenisi başta',
    intro: 'Bir sürüm, kaydedildikten sonra bir daha değişmez. Her biri neyi değiştirdiğini tek satırda söyler.',
    agent: 'Bir ajan yazdı',
    online: 'yayında',
    browse: 'Kodu oku',
    noChange: 'Not yok',
  },

  licenses: {
    MIT: 'Lisans ve telif hakkı bildirimi yanında kaldığı sürece herkes onu her türlü işte kullanabilir, değiştirebilir ve başkalarına verebilir.',
    'Apache-2.0': 'MIT gibi; ayrıca katkıda bulunan herkesten bir patent lisansı içerir ve değişikliklerin değişiklik olarak belirtilmesini ister.',
    'BSD-3-Clause': 'MIT gibi; ayrıca kimse, ondan yaptığı şeyin tanıtımında yazarların adını kullanamaz.',
    'MPL-2.0': 'Bu dosyalardaki değişiklikler aynı lisansla kalır; dosyalar başka herhangi bir lisanstaki kodla birleştirilebilir.',
    'GPL-3.0-or-later': 'Onu değiştirerek ya da olduğu gibi başkalarına veren herkes, kaynak kodunu da aynı lisansla vermek zorundadır.',
    'AGPL-3.0-or-later': 'GPL gibi; ayrıca değiştirilmiş bir kopyayı insanlara ağ üzerinden sunmak da onu başkalarına vermek sayılır.',
    Unlicense: 'Kamu malına bırakılmıştır: herkes onunla koşulsuz olarak her şeyi yapabilir.',
  },

  kinds: {
    Permissive: 'İzin verici',
    Copyleft: 'Copyleft',
    PublicDomain: 'Kamu malı',
  },
};
