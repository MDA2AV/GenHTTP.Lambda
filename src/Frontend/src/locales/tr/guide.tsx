import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Nasıl çalışır',
  intro:
    'Bir parça C# kodu yazarsınız. Döndürdüğü şey birkaç saniye içinde HTTPS üzerinden, herkese açık bir adreste yayına girer. Aşağıda her şey, karşılaşacağınız sırayla anlatılıyor.',
  contents: 'İçindekiler',

  parts: {
    what: 'Lambda nedir?',
    first: 'İlk lambdanız',
    editor: 'Kontrol paneli',
    why: 'Nedenini yazmak',
    features: 'Güvenle değiştirmek',
    files: 'Birden fazla dosya',
    page: 'Sayfa sunmak',
    spa: 'Adım adım bir frontend',
    storage: 'Dosyaların durduğu iki yer',
    keeping: 'Veri saklamak',
    sockets: 'Websocket’ler',
    limits: 'İzin verilmeyenler',
    away: 'Kodunuzu alıp gitmek',
    agents: 'İşi bir ajana bırakmak',
  },

  what: [
    (k) => (
      <>
        Lambda, bir GenHTTP handler döndüren bir kod parçasıdır. Platform onu derler, yükler ve döndürdüğü şeyi kendi
        adresinizin altına bağlar. Proje yok, build dosyası yok, {k.code('using')} satırı yok. Tüm GenHTTP modülleri
        zaten içe aktarılmış durumda.
      </>
    ),
    (k) => (
      <>
        Bu, eksiksiz bir lambda. {k.code('/lambda/your-key/')} adresinde yayına alındığında her isteğe “hello”
        kelimesiyle yanıt verir.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Kod parçası bir sınıf değildir, {k.em('deyimlerden')} oluşur. Yaptığı son iş, istekleri karşılayabilen bir şey
      döndürmektir: bir handler ya da onu oluşturan bir builder.
    </>
  ),

  first: [
    (k) => (
      <>
        {k.b('Lambda oluştur')} düğmesine basın. Herkese açık bir adres ve bir editör anahtarı alırsınız. Anahtar, geri
        dönmenin tek yolu. Saklayın, çünkü kimse onu sizin için kurtaramaz.
      </>
    ),
    () => (
      <>
        Ardından lambdanın kontrol paneline gelirsiniz. İlk sürüm olarak küçük bir REST servisi hazır bekler. Bu sadece
        bir başlangıç.
      </>
    ),
    (k) => (
      <>
        Editör anahtarını bir ajana verin ve ne yapacağını söyleyin. Ajan {k.link('/#agents', 'MCP')} üzerinden
        yeni sürümler yazar. Ya da {k.b('Kod')} bölümünü açıp kendiniz yazın:{' '}
        {k.b('Kontrol et')} hiçbir şey kaydetmeden derler ve derleyicinin ne dediğini dosya ve satırıyla gösterir.
      </>
    ),
    (k) => (
      <>
        {k.b('Yayına al')} düğmesine basın. Artık yayında. Bundan önce hiçbir şeye erişilemez. Yeniden yayına almak da
        yayında kalma süresini uzatır.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Editör linki bir metin kutusu değil, bir kontrol paneli açar. Buradaki kodun çoğunu ajanlar yazar, bu yüzden
      ekranda ilk gördüğünüz şey lambdanızın durumudur. Kenar çubuğunda lambdanın kendisi (yayında olup olmadığı,
      adresi ve daha yeni bir sürüm yayına alınmayı bekliyorsa bir düğme) ve bölümleri yer alır. Adresi değiştirmek
      ya da lambdayı silmek gibi nadiren yapılan işler, oradaki {k.b('⋯')} menüsündedir.
    </>
  ),
  bits: [
    ['Genel bakış', () => <>Yayında olup olmadığı, bugün kaç istek aldığı ve kaçının başarısız olduğu, son değişiklik ve ne kadar yer kaldığı.</>],
    [
      'Değiştir',
      (k) => (
        <>
          Neyin farklı olması gerektiğini yazın, gerisini bu sunucudaki ajan siz izlerken halleder. Bir taslakta çalışır,
          değişikliği orada dener ve çalışınca birleştirip bir sonraki sürüm yapar. Taslağı önce kendiniz denemek
          isterseniz {k.b('Bitince yayına al')} seçeneğini kapatın.
        </>
      ),
    ],
    ['Taslaklar', () => <>Lambdanın yanında üzerinde çalışılan değişiklikler: her biri kendi adresinde denenir ve hazır olunca birleştirilip bir sonraki sürüm olur. Açıldığında bir taslağın kendi kodu, verileri ve logları vardır.</>],
    ['Dosyalar', () => <>Bir sürümün dosyaları: kodu ve statik dosyaları, yani programın kendisi. Kilit ya da dünya simgesi, herkesin onlara erişip erişemeyeceğini gösterir.</>],
    ['Veriler', () => <>Lambdanın çalışırken sakladıkları, tüm sürümler için ortak: çalışma alanı. İçine bakın, dosya yükleyip silin ya da onu kapatın. Gizli anahtarlar – API belirteçleri ve kimlik bilgileri – açıldıktan sonra burada yer alır: değiştirilebilir veya silinebilir, ama bir daha gösterilmez.</>],
    ['Sürümler', () => <>Her sürümün neyi değiştirdiği, ne istendiği ve bir öncekinden farkı. Buradan yayına alabilir, eski bir sürüme dönebilir ya da herhangi bir sürümden bir taslak başlatabilirsiniz.</>],
    ['Yayın geçmişi', () => <>Ne zaman neyin yayında olduğu ve neden yayından kalktığı.</>],
    ['İstatistikler', () => <>Son bir saatin ya da günün istekleri, hataları, yanıt süreleri ve en çok istenen yolları.</>],
    ['Loglar', () => <>İstekler, lambdanın yazdırdıkları ve ters giden her şeyin stack trace’i, anında.</>],
    [
      'Kod',
      (k) => (
        <>
          Elle yazmak için. {k.b('Kontrol et')} derler, {k.b('Kaydet')} bir sürüm oluşturur, {k.b('Yayına al')} yayına
          alır. Bir taslakta ise {k.b('Kaydet')} onu taslakta tutar, {k.b('Önizlemeyi yayına al')} da taslağın adresinde
          yayına alır. {k.code('Ctrl-S')} kaydeder, {k.code('F12')} bir tanıma gider.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Her bölüm aynı şekilde çalışır: başlığı, onu açıklayan bir {k.b('ⓘ')} simgesi, sağda eylemleri ve birden fazla
      görünümü varsa altında bir sıra sekme. Kod bölümünde bu sekmeler dosyalardır.
    </>
  ),
  editorAside:
    'Trafik ve log bellekte tutulur. Saklamak için değil, izlemek içindir: sunucu yeniden başlarsa sıfırdan başlarlar. Sürümler ve yayın geçmişi ise kalıcı olarak saklanır.',

  why: (k) => (
    <>
      Bir sürüm, kod ve isteğe bağlı iki nottan oluşur: {k.b('spesifikasyon')}, yani kullanıcının ne istediği ve
      nedeni, mümkünse kendi sözleriyle; ve {k.b('değişiklik')}, yani sürümün ne yaptığını anlatan tek bir satır.
      İkisi de sürüm geçmişinde diff’in yanında görünür. Böylece {k.em('neden')}, {k.em('ne')} ile yan yana kalır.
      Hem sizin için, hem de bir şeyi değiştirmeden önce geçmişi okuyan bir sonraki ajan için.
    </>
  ),
  whySample: {
    specification: 'İnsanların imza atabileceği bir ziyaretçi defteri; kayıtlar yeniden başlatmada kaybolmamalı',
    change: 'Kayıtları çalışma alanında tutar, böylece yeniden başlatmada kaybolmazlar',
  },
  why2: (k) => (
    <>
      Ajanlar da aynı iki alanı {k.code('write_code')} aracına verir. {k.b('Kod')} bölümünde kaydederken değişiklik
      sorulur. İkisi de isteğe bağlıdır. Uzun bir spesifikasyon reddedilmez, 4.000 karakterde kesilir; değişiklik ise
      500 karakterde. Bir taslağın da kendi iki notu vardır; birleştirildiği sürüm bunları devralır.
    </>
  ),

  features: (k) => (
    <>
      Bir sürüm, kaydedildikten sonra bir daha değişmez. Her birini saklamaya değer kılan da bu: herhangi biriyle
      karşılaştırma yapılabilir, herhangi biri tam olduğu gibi yeniden yayına alınabilir. İnsanların kullandığı bir
      lambdayı değiştirmek için bunun yerine bir {k.b('taslak')} başlatın.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        {k.b('Taslaklar')} bölümünden ya da herhangi bir sürümden başlatın. Taslak, o sürümün kodunun ve statik
        dosyalarının, bir de lambdanın verilerinin kopyasıdır.
      </>
    ),
    (k) => (
      <>
        Gerektiği kadar değiştirin: {k.b('Kod')} bölümünde ya da ajandan isteyerek. {k.b('Önizlemeyi yayına al')} onu
        kendi adresinde, {k.code('/features/…/')} altında, verilerin kendine ait kopyasıyla yayına alır. Lambdanın
        ziyaretçileri bunların hiçbirini görmez; taslağın yazdığı hiçbir şey lambdanın verilerine ulaşmaz.
      </>
    ),
    (k) => (
      <>
        Hazır olunca {k.b('Birleştir')} düğmesine basın: notlarıyla birlikte bir sonraki sürüm olur ve isterseniz hemen
        yayına girer. Taslak ise önizlemesi ve verilerin kopyasıyla birlikte kaldırılır.
      </>
    ),
  ],
  featureSample: 'Skor tablosu',
  featuresAside: () => (
    <>
      Aynı anda birden fazla taslak üzerinde çalışılabilir. Yalnızca en yeni sürümü temel alan bir taslak
      birleştirilebilir; böylece bir birleştirme, taslak başladıktan sonra kaydedilen bir sürümü asla geri almaz. Önce
      başka bir taslak birleştirildiyse onun değişikliklerini taslağa taşıyın (ya da ajandan isteyin), sonra taslağın
      temelini en yeni sürüm yapın. Hiçbir şey kendiliğinden birleşmez; bu bilinçli bir tercih.
    </>
  ),

  files: (k) => (
    <>
      Türlerin, onları kullanan kodun altında durması gerekmez. {k.b('Kod')} bölümünde dosyaların yanındaki{' '}
      {k.b('+')} düğmesine basın. Yeni dosya, kod parçasıyla aynı namespace içinde, onunla birlikte derlenir. Böylece
      erişmek için hiçbir şeyi içe aktarmanız gerekmez. Uzantısı olmayan bir ad C# dosyası sayılır.
    </>
  ),

  page: 'Sayfa sunmanın iki yolu var. İnsanların yanına yüklediği dosyalar için de bir üçüncüsü.',
  inlineTitle: 'Tek sayfa, kodun içinde',
  inline: 'Küçük şeyler için yeterli. Sayfa doğrudan kodun içinde yer alır.',
  folderTitle: 'Gerçek dosyalarla bir klasör',
  folder:
    'Stil dosyası ve script içeren her şey için doğru seçim. Dosyalar tıpkı bir C# dosyası gibi eklenir ve tam yazıldığı gibi sunulur. Derlenmezler.',
  workspaceTitle: 'Yüklenen dosyalar, verilerden',
  workspace:
    'İnsanların yüklediği ya da lambdanın oluşturduğu şeyler (görseller, belgeler) için; uygulamanın yanında sunulurlar. Uygulamanın kendi sayfaları için değil: onların yeri bir dosya klasörüdür, orada onlara ihtiyaç duyan kodla birlikte sürümlenirler.',

  spa: (k) => (
    <>
      Bunların ikincisi, baştan sona. Her demo, sayfasını bu şekilde {k.code('web')} adlı bir klasörden sunar. Bir
      örnek görmek için {k.link('/editor/demo-crud', 'demo-crud')} demosunu açın. Demolar salt okunurdur; editör
      anahtarları adlarıyla aynıdır.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        {k.b('Kod')} bölümünde dosyaların yanındaki {k.b('+')} düğmesine basın ve {k.code('site/index.html')} yazın.
        Adında eğik çizgi olan bir dosya bir klasöre girer. Uzantısı olan bir ad da uzantısının söylediği türde dosya
        sayılır.
      </>
    ),
    (k) => (
      <>
        {k.code('site/app.css')} ve {k.code('site/app.js')} dosyalarını da aynı şekilde ekleyin. Sayfanız onlara{' '}
        {k.code('href="app.css"')} örneğindeki gibi adlarıyla başvurur. Çünkü klasör adresin bir parçası değil, sunulan
        içeriğin köküdür.
      </>
    ),
    (k) => (
      <>
        Görsel ya da font gibi metin olmayan dosyalar için {k.code('site')} klasöründeki bir dosyayı açın ve dosyaların
        yanındaki yükleme düğmesine basın. Dosya aynı klasöre gider. PNG bir metin editöründe yazılamaz, o yüzden yolu bu.
      </>
    ),
    (k) => <>{k.code('lambda.cs')} dosyasında klasörü sunun:</>,
    (k) => (
      <>
        {k.b('Yayına al')} düğmesine basın. {k.code('site/index.html')} dosyası {k.code('/')} adresinde,{' '}
        {k.code('site/app.css')} dosyası {k.code('/app.css')} adresinde yanıt verir. Hiçbir dosyayla eşleşmeyen her
        adreste sayfanın kendisi döner. Böylece kendi yönlendirmesini yapan bir frontend, biri bir deep link üzerinde sayfayı
        yenilediğinde de çalışır.
      </>
    ),
    () => <>Yanına bir API ekleyin, sayfanın konuşacağı bir şey olsun:</>,
  ],

  storage: (k) => (
    <>
      Bir lambda dosyaları iki yerde tutar ve editör onları ayrı gösterir: {k.b('Dosyalar')} bir sürümün dosyalarını
      (programı), {k.b('Veriler')} ise çalışma alanını (programın sakladıklarını) tutar. Fark,{' '}
      {k.em('kime ait olduklarında')} yatar. Bir sürümün dosyaları o sürüme aittir; veriler ise lambdaya aittir ve her
      sürüm onları paylaşır.
    </>
  ),
  savedWithCode: 'Bir sürümde',
  workspaceColumn: 'Verilerde',
  table: [
    ['ne tutar', 'kod ve statik dosyalar: frontend dahil programın kendisi', 'lambdanın yazdığı ya da birinin yüklediği her şey'],
    ['ne zaman değişir', 'hiçbir zaman: her değişiklik yeni bir sürümdür', 'içine bir şey yazıldığı anda'],
    ['yayına alma', 'tam olarak bu dosyaları yayına alır', 'ona hiç dokunmaz'],
    ['eski bir sürüme dönmek', 'eski dosyaları geri getirir', 'etkisi yok: her sürüm onu paylaşır'],
    ['bir taslak', 'onların bir kopyasıyla başlar', 'onun bir kopyası üzerinde çalışır'],
    ['ne zaman silinir', 'sınır aşılınca, eski sürümlerle birlikte', 'lambdayla birlikte ya da siz kapattığınızda'],
  ],
  reachedAs: 'koddan erişim',
  storageAside:
    'İkisi tek bir yer olamaz. Olsaydı, her yayına alma ya lambdanızın o zamandan beri yazdığı her şeyi silerdi ya da yayına aldığınız dosyalardan hiçbir şey kaldırılamazdı. Skor tablosu tutan bir oyun ikincisini ister, sunduğu sayfa ise birincisini. Bu yüzden sayfa sürüme, skor tablosu da verilere girer.',

  keeping: (k) => (
    <>
      {k.code('Workspace')}, lambdanızın okuyup yazabildiği özel bir klasördür. Bir istekten ya da bir yayından daha
      uzun yaşaması gereken her şeyin yeri burasıdır.
    </>
  ),
  keeping2: (k) => (
    <>
      Ayrıca {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} ve içeriği sunmak için {k.code('Tree')}/{k.code('Files')}/{k.code('App')} da var.
      Dosya sisteminin geri kalanına erişilemez.
    </>
  ),

  keeping3: (k) => (
    <>
      Gizli anahtarlar, koda asla konmaması gereken API belirteçleri ve kimlik bilgileri içindir. {k.b('Veriler')} bölümünden açın, bir tane ekleyin ve {k.code('Secret.Read')} ile adıyla okuyun. Değer şifrelenmiş olarak saklanır ve bir daha gösterilemez – size de, API üzerinden de, bir ajana da – yalnızca değiştirilebilir ya da silinebilir. Yeni bir değer, dağıtım gerekmeden bir sonraki istekten itibaren okunur; her sürüm aynı gizli anahtarları okur ve bir taslak onların kopyası üzerinde çalışır. İndirilen bir proje bunun yerine aynı adlı bir ortam değişkenini okur.
    </>
  ),

  sockets: (k) => (
    <>
      Destekleniyor, hem de sonradan akla gelmiş bir özellik olarak değil. {k.link('/editor/demo-game', 'demo-game')}{' '}
      demosu oyuncuları eşleştirir ve her oyunu sunucuda yürütür. En basit hâli üç callback’ten oluşur:
    </>
  ),
  socketsAside: (k) => (
    <>
      Herkesin takıldığı bir nokta var: tarayıcı, websocket el sıkışmasında header ekleyemez. Handler’ın ihtiyaç
      duyduğu bilgiyi sorgu parametrelerinde gönderin; handler onu {k.code('connection.Request.Header.Query')}{' '}
      üzerinden okur. Ya da gizli bilgileri ilk mesaj olarak gönderin.
    </>
  ),

  limits:
    'Kodunuz ortak bir sunucuda çalışır. Bu yüzden C# dilinin bazı kısımları derlenmeden önce reddedilir: süreç başlatmak, kendi soketlerinizi açmak, assembly yüklemek, çalışma alanınızın dışında dosya sistemine erişmek ve bunları aşmak için reflection kullanmak.',
  limits2:
    'Geri kalan her şey mevcut, GenHTTP modül API’sinin tamamı dahil. Bir şey reddedilirse sadece başarısız olduğu değil, hangi satırda ve neden reddedildiği de söylenir.',

  away: (k) => (
    <>
      Editördeki {k.b('.NET projesi olarak indir')} ile lambdanın tamamını alırsınız: açabileceğiniz,{' '}
      {k.code('dotnet run')} ile çalıştırabileceğiniz ve saklayabileceğiniz bir solution. İçinde tek bir paket referansı
      var, bu platformdan ise hiçbir iz yok.
    </>
  ),
  away2: (k) => (
    <>
      Kod parçanız {k.code('Program.cs')} dosyasının gövdesi olur ve döndürdüğü şeyi sunan bir host içine yerleşir.
      Diğer dosyalarınız tam yazdığınız gibi gelir. {k.code('Workspace')} ve {k.code('Assets')} kodun yanında iki klasör
      olur ve aynı metotlarla çalışır. Yani kodunuzda hiçbir şeyi değiştirmeniz gerekmez.
    </>
  ),
  awayAside:
    'Burada bir şey yapmadan önce bilmekte fayda var: yazdığınız kod sizindir ve eksiksiz olarak sizinle gelir. Onu bu makinede çalıştırmak, onu bu makineye bağlamaz.',

  agents: (k) => (
    <>
      {k.code('/mcp')} adresinde bir MCP endpoint’i var. Bir ajanı buraya bağlayın, editörün yaptığı her şeyi
      yapabilir: kılavuzu okur, bir demoyu baştan sona inceler, dosya yazar, derler ve yayına alır. Altta aynı API
      çalışır.
    </>
  ),
  agents2: (k) => (
    <>
      Ajan çalışırken nedenini de söyler ({k.code('write_code')} aracı spesifikasyonu ve değişikliği alır) ve yayına
      aldığı şeye bakabilir: {k.code('read_logs')} aracı lambdanın son isteklerini, yazdırdıklarını ve fırlattığı her
      hatanın stack trace’ini döndürür. Ajan, kodunun çalıştığını varsaymak yerine böyle öğrenir. Siz de aynı şeyi
      kontrol panelinde izlersiniz.
    </>
  ),
  more: 'Daha fazlası →',
  make: 'Lambda oluştur',
};
