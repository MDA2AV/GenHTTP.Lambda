import type { EditorMessages } from '../en/editor';

/** Editörün Türkçe metinleri. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 'sn', min: 'dk', h: 'sa', d: 'gün' },
    amount: (value, unit) => `${value} ${unit}`,
    pair: (larger, smaller) => `${larger} ${smaller}`,
    never: 'hiçbir zaman',
    justNow: 'az önce',
    ago: (span) => `${span} önce`,
    in: (span) => `${span} sonra`,
    origins: {
      agent: 'ajan',
      template: 'şablon',
      admin: 'operatör',
      system: 'platform',
      api: 'API / editör',
      unknown: 'bilinmiyor',
    },
    endings: {
      replaced: 'daha yeni bir sürüm yayına alındı',
      stopped: 'yayından kaldırıldı',
      expired: 'kullanılmadığı için süresi doldu',
      admin: 'operatör yayından kaldırdı',
      ended: 'sona erdi',
    },
    whatThisIs: 'Bu nedir?',
    byAgent: 'bir ajan tarafından',
    writtenByAgent: 'Bir ajan yazdı',
    more: 'Daha fazla',
    of: (used, total) => `${used} / ${total}`,
    online: (version) => `Yayında · v${version}`,
    onlineTitle: (version) => `Yayında, sürüm ${version} sunuluyor`,
    offline: 'Yayında değil',
    offlineTitle: 'Yayında değil: hiçbir şey sunulmuyor',
    premium:
      'Premium: kendi alan adında yanıt verebilir, kod, statik dosyalar ve veriler için daha fazla yeri vardır ve ne kadar az kullanılırsa kullanılsın yayında kalır',
    demo: 'Demo: bu kurulum tarafından yayında tutulur ve salt okunurdur',
    tier: (tier) => `Plan: ${tier}`,
    entrances: {
      title: 'Erişim yolu',
      note: 'Sunucu başladığından beri, websocket bağlantıları dahil.',
    },
    chart: {
      showChart: 'Grafiği göster',
      showValues: 'Değerleri göster',
      none: 'Henüz ölçüm yok.',
      time: 'Zaman',
    },
    diagnostics: {
      compiles: 'Kod sorunsuz derleniyor.',
      none: 'Henüz mesaj yok. Kodunuzu derlemek için kontrol edin ya da yayına alın.',
      line: (line) => `satır ${line}`,
    },
  },

  frame: {
    title: 'Editör',
    sections: {
      overview: 'Genel bakış',
      showcase: 'Vitrin',
      domain: 'Alan adı',
      files: 'Dosyalar',
      versions: 'Sürümler',
      deployments: 'Yayın geçmişi',
      stats: 'İstatistikler',
      logs: 'Loglar',
      code: 'Kod',
    },
    sectionsLabel: 'Bölümler',
    loadFailed: 'Bu lambda yüklenemedi.',
    online: (version) => `Sürüm ${version} yayında.`,
    deployFailed: 'Lambda yayına alınamadı.',
    offline: 'Yayından kaldırıldı. Kod hâlâ burada.',
    offlineFailed: 'Lambda yayından kaldırılamadı.',
    leave: 'Koddaki kaydedilmemiş değişiklikleriniz kaybolacak. Yine de çıkılsın mı?',
    nothingTitle: 'Bu link hiçbir şey açmıyor',
    createNew: 'Yeni bir lambda oluştur',
    loading: 'Lambdanız yükleniyor…',
    moreActions: 'Diğer işlemler',
    redeploy: (version) => `${version}. sürümü yeniden yayına al`,
    takeOffline: 'Yayından kaldır',
    copyLink: 'Linki kopyala',
    copyPrivate: 'Özel linki kopyala',
    privateLink: 'Bu linke sahip olan herkes lambdayı değiştirebilir. Kimseyle paylaşmayın.',
    rename: 'Adresi değiştir',
    download: '.NET projesi olarak indir',
    delete: 'Bu lambdayı sil',
    deploy: (version) => `${version}. sürümü yayına al`,
    problems: 'Son zamanlarda bir şeyler ters gitti',
    demoTitle: 'Bir demo: bu kurulum tarafından yayında tutulur ve salt okunurdur.',
    demo: (start) => (
      <>
        Kodunu, geçmişini, sakladıklarını ve loglarını okuyun; demo zaten bunun için var. Değiştirmek isterseniz{' '}
        {start('bundan kendi lambdanızı oluşturun')}.
      </>
    ),
    keep: 'Bu linki saklayın. Bu lambdaya geri dönmenin tek yolu bu.',
    gotIt: 'Anladım',
    rejected: (version) => `Sürüm ${version} yayına alınamadı`,
    refused: 'Yayına alma reddedildi',
    openCode: 'Kodu aç',
    close: 'Kapat',
    notCompiling: 'Kod derlenmiyor. Daha önce yayında olan ne varsa hâlâ yayında.',
    moved: (path) => `Yeni adres: ${path}.`,
    deleteTitle: 'Bu lambda silinsin mi?',
    cancel: 'İptal',
    deleteForGood: 'Kalıcı olarak sil',
    deleteFailed: 'Lambda silinemedi.',
    deleteText: (key) => (
      <>Tüm sürümleri, dosyaları, geçmişi ve {key} adresi onunla birlikte silinir. Bu işlem geri alınamaz.</>
    ),
    openInTab: 'Yeni sekmede aç',
    open: (address) => `${address} adresini yeni sekmede aç`,
    copyAddress: 'Adresi kopyala',
    renameFailed: 'Adres değiştirilemedi.',
    moveIt: 'Taşı',
    renameText: 'Eski adres hemen çalışmayı bırakır. Ona link veren her yeri güncelleyin.',
  },

  summary: {
    reading: 'Durum yükleniyor…',
    hint: (since, kept, retention, tier) =>
      `Trafik, sunucu son başladığından beri sayılıyor (${since}). ` +
      (kept
        ? `Lambda, kullanıldığı sürece yayında kalır. ${retention} gün boyunca ziyaret ya da değişiklik olmazsa silinir.`
        : `Bu lambdanın planı: ${tier}. Bu planda lambda, ne kadar az kullanılırsa kullanılsın yayında ve kayıtlı kalır.`),
    onlineFor: (duration, version) => (
      <>
        Sürüm {version}, {duration('bir')} süredir yayında.
      </>
    ),
    offline: 'Yayında değil. Bir sürüm yayına alınana kadar hiçbir şey sunulmuyor.',
    nothing: 'Henüz hiçbir şey yazılmadı.',
    requestsToday: 'bugün gelen istek',
    lastHour: (count) => `Son bir saatte ${count} istek`,
    hourly: 'Son 24 saatte saatlik istekler',
    failed: 'başarısız',
    failedTitle: (failed, rejected) =>
      `Son 24 saatte ${failed} sunucu hatası, ${rejected} bulunamayan ya da reddedilen istek`,
    average: 'ortalama yanıt süresi',
    noneYet: 'henüz yok',
    lastVisit: 'son ziyaret',
    problems: 'Son zamanlarda bir şeyler ters gitti',
    openLog: 'Logu aç',
    latest: 'Son değişiklik',
    allVersions: 'Tüm sürümler',
    noDescription: 'Açıklama yok',
    version: (version) => `Sürüm ${version}`,
    notOnline: 'henüz yayında değil',
    wanted: 'Ne istendi',
    noVersions: 'Henüz sürüm yok.',
    storage: 'Depolama',
    browse: 'Göz at',
    code: 'Kod',
    codeWhy: 'C# derlenir, asla sunulmaz.',
    characters: 'karakter',
    assets: 'Statik dosyalar',
    assetsPublic: 'Herkese açık: kod bunları sunuyor.',
    assetsPrivate: 'Kod bunları sunmuyor.',
    data: 'Veriler',
    dataPublic: 'Herkese açık: kod çalışma alanını sunuyor.',
    dataPrivate: 'Yalnızca lambdaya özel.',
  },

  files: {
    hint: (b) => (
      <>
        {b('Kod')} derlenir ve asla sunulmaz. {b('Statik dosyalar')} (sayfalar, stiller, görseller) her sürümle birlikte
        kaydedilir ve kod onları sunuyorsa herkese açıktır. {b('Veriler')}, lambdanın çalışırken yazdıklarıdır. Hiçbir
        sürümün parçası değildir ve yalnızca kod onları sunuyorsa herkese açıktır.
      </>
    ),
    edit: 'Bu sürümü düzenle',
    version: 'Sürüm',
    shown: (version, online, newest) => `Sürüm ${version}${online ? ', yayında' : newest ? ', en yeni' : ''}`,
    optionOnline: ' (yayında)',
    readFailed: 'Bu sürüm okunamadı.',
    dataFailed: 'Veriler okunamadı.',
    noVersion: 'Henüz gösterilecek bir sürüm yok.',
    label: 'Dosyalar',
    code: 'Kod',
    codeWhy: 'Lambdanın içine derlenir, asla sunulmaz.',
    count: (files) => `${files} dosya`,
    codeUsage: (files, used, of) => `${files}, ${used} / ${of} karakter`,
    usage: (files, used, of) => `${files}, ${used} / ${of}`,
    noCode: 'Bu sürümde kod yok.',
    assets: 'Statik dosyalar',
    assetsPublic: 'Herkese açık: bu sürüm onları Assets ile sunuyor.',
    assetsPrivate: 'Kodla birlikte kaydedildi ama bu sürüm onları sunmuyor.',
    noAssets: 'Bu sürümde yok.',
    data: 'Veriler',
    dataPublic: 'Herkese açık: bu sürüm onları Workspace ile sunuyor.',
    dataPrivate: 'Yalnızca lambdaya özel. Hiçbir sürümün parçası değil.',
    uploadFailed: (path) => `${path} yüklenemedi.`,
    deleteFolder: (path, held) =>
      held > 0 ? `${path} klasörü ve içindeki ${held} dosya silinsin mi?` : `${path} klasörü silinsin mi?`,
    deleteFile: (path) => `${path} silinsin mi? Lambda onu artık bulamayacak.`,
    deleteFailed: 'Silinemedi.',
    full: 'Veri alanı dolu',
    uploadInto: (folder) => `${folder} klasörüne yükle`,
    upload: 'Yükle',
    reading: 'Okunuyor…',
    noData: 'Henüz bir şey yok. Lambdanın çalışırken kaydettikleri burada görünür.',
    delete: (path) => `Sil: ${path}`,
    deleteShort: 'Sil',
    fileFailed: 'Dosya okunamadı.',
    pick: 'İçinde ne olduğunu görmek için bir dosya seçin.',
    tooLarge: (name, size) => (
      <>
        {name} ({size}) burada gösterilemeyecek kadar büyük.
      </>
    ),
    download: 'İndir',
    readingFile: (name) => `${name} okunuyor…`,
    missing: (name) => `Bu sürümde ${name} adlı bir dosya yok.`,
    saved: 'kaydedildi',
    notText: 'Metin değil. İçine bakmak için indirin.',
  },

  versions: {
    hint: (limit) =>
      `Her sürüm, yazan belirttiyse, ne istendiğini ve neyi değiştirdiğini saklar. En fazla ${limit} sürüm tutulur, fazlası olunca en eskiler silinir. Yayındaki sürüm asla silinmez.`,
    none: 'Henüz sürüm yok.',
    noDescription: 'Açıklama yok',
    online: 'yayında',
    putOnline: 'Bu sürümü yayına al',
    rollBackTitle: 'Bu eski sürümü yeniden yayına al',
    deploy: 'Yayına al',
    rollBack: 'Geri dön',
    readFailed: 'Bu sürüm okunamadı.',
    comparing: 'Karşılaştırılıyor…',
    unchanged: 'Bir önceki sürüme göre değişiklik yok.',
    first: 'İlk sürüm.',
    status: { added: 'eklendi', removed: 'silindi', changed: 'değişti', same: 'aynı' },
    browse: 'Dosyalarına göz at',
    edit: 'Buradan düzenle',
    binary: 'Metin değil, karşılaştırılacak satır yok.',
    tooLarge: 'Satır satır karşılaştırmak için çok büyük.',
  },

  deployments: {
    hint: (until) =>
      `Yayına alınan sürüm, insanlar kullandığı sürece yayında kalır${until ? ` (kimse kullanmazsa ${until} tarihine kadar)` : ''}. Yeniden yayına almak ya da herhangi bir ziyaret bu süreyi sıfırlar.`,
    takeOffline: 'Yayından kaldır',
    readFailed: 'Geçmiş okunamadı.',
    reading: 'Geçmiş okunuyor…',
    none: 'Henüz hiçbir şey yayına alınmadı.',
    noDescription: 'Açıklama yok',
    deployed: (when, by) => `${when} tarihinde yayına alındı (${by})`,
    duration: 'Ne kadar yayında kaldı',
    online: 'yayında',
    short: {
      replaced: 'yenisiyle değişti',
      stopped: 'kaldırıldı',
      expired: 'süresi doldu',
      admin: 'operatör kaldırdı',
      ended: 'sona erdi',
    },
    putBack: (version) => `${version}. sürümü yeniden yayına al`,
    timeline: 'Son yedi günde yayında olanlar',
    block: (version, from, to) => `Sürüm ${version}, ${from} – ${to ?? 'şimdi'}`,
    weekAgo: 'bir hafta önce',
    now: 'şimdi',
  },

  stats: {
    readFailed: 'İstatistikler yüklenemedi.',
    range: 'Zaman aralığı',
    lastHour: 'Son bir saat',
    lastDay: 'Son 24 saat',
    hint: (since) =>
      `Sunucu son başladığından beri (${since}) bellekte sayılır. Sunucu yeniden başlarsa sayım sıfırlanır.`,
    reading: 'İstatistikler yükleniyor…',
    requests: 'istek',
    websockets: (count) => `ve ${count} websocket bağlantısı`,
    failed: 'başarısız',
    serverErrors: (count) => `${count} sunucu hatası`,
    rejected: 'bulunamadı ya da reddedildi',
    average: 'ortalama yanıt süresi',
    sent: (amount) => `Gönderilen: ${amount}`,
    nobody: (hour) => (hour ? 'Son bir saatte hiç istek gelmedi.' : 'Son 24 saatte hiç istek gelmedi.'),
    requestsTitle: 'İstekler',
    per: (hour) => (hour ? 'Dakika başına.' : '15 dakika başına.'),
    answered: 'Yanıtlanan',
    rejectedSeries: 'Bulunamayan ya da reddedilen',
    failedSeries: 'Başarısız',
    timeTitle: 'Yanıt süresi',
    averagePer: (hour) => (hour ? 'Dakika başına ortalama.' : '15 dakika başına ortalama.'),
    averageSeries: 'Ortalama',
    mostAsked: 'En çok istenenler',
    path: 'Yol',
    requestsColumn: 'İstek',
    failedColumn: 'Başarısız',
    averageColumn: 'Ortalama',
    since: 'Sunucu başladığından beri.',
  },

  logs: {
    readFailed: 'Log okunamadı.',
    hint: (capturing) =>
      'İstekler, lambdanın yazdırdıkları ve ters giden şeyler, anında.' +
      (capturing ? '' : ' Bu kurulum lambdaların yazdırdıklarını saklamaz, bu yüzden yalnızca istekler ve hatalar görünür.') +
      ' Log bellekte tutulur ve buradaki tüm lambdalarla paylaşılır. Bu yüzden dakikalar ya da saatler öncesine kadar gider ve sunucu yeniden başlayınca boşalır. Ziyaretçilerin adresleri gösterilmez.',
    search: 'Ara',
    searchLabel: 'Logda ara',
    resume: 'Yeni satırları geldikçe göster',
    pause: 'Okurken yeni satır eklemeyi durdur',
    paused: 'Duraklatıldı',
    live: 'Canlı',
    show: 'Göster',
    all: 'Hepsi',
    requests: 'İstekler',
    output: 'Çıktılar',
    problems: 'Sorunlar',
    reading: 'Log okunuyor…',
    noProblems: 'Logun hâlâ hatırladığı bir sorun yok.',
    nothing: 'Henüz bir şey yok. Lambdanın adresini açın, istekler burada görünsün.',
    noMatch: 'Eşleşen bir şey yok.',
    identical: (count) => `${count} aynı satır`,
    at: (domain) => `, ${domain} üzerinden`,
    from: (country) => `, ülke: ${country}`,
  },

  showcase: {
    loadFailed: 'Vitrin yüklenemedi.',
    loading: 'Yükleniyor…',
    title: 'başlık',
    description: 'açıklama',
    picture: 'görsel',
    updated: 'Vitrin kartı güncellendi.',
    listed: 'Artık vitrin sayfasında.',
    waiting: 'Kaydedildi. Lambda yayına girince vitrin sayfasında görünecek.',
    saveFailed: 'Vitrin kartı kaydedilemedi.',
    removed: 'Vitrin sayfasından kaldırıldı.',
    removeFailed: 'Vitrin kartı kaldırılamadı.',
    wrongType: 'Bu bir PNG, JPEG, GIF ya da WebP görseli değil.',
    tooLarge: (size, limit) => `Bu dosya ${size}. Bir görsel en fazla ${limit} olabilir.`,
    unreadable: 'Bu dosya okunamadı.',
    hint: (tool) => (
      <>
        Vitrin sayfası, sahiplerinin göstermeyi seçtiği lambdaları listeler; son zamanlarda kullanılanlar önce gelir. Bir
        lambdayı oraya yalnızca editör anahtarına sahip olan ekleyebilir ya da oradan kaldırabilir. Lambda yalnızca
        yayındayken listelenir. Bir ajan da aynısını {tool} aracıyla yapabilir.
      </>
    ),
    open: 'Vitrini aç',
    switch: 'Bu lambdayı vitrin sayfasında göster',
    listedNow: 'Şu anda listede. Vitrine göz atan herkes açabilir.',
    notListed: 'Kaydedildi ama listede değil, çünkü lambda yayında değil. Yeniden yayına alınınca tekrar görünür.',
    off: 'Kapalı. Bunu açıp kaydedene kadar bu lambdayla ilgili hiçbir şey hiçbir yerde gösterilmez.',
    offline: 'Lambda yayında değil, bu yüzden kart lambda yayına alınana kadar bekleyecek. Yalnızca yanıt veren lambdalar listelenir.',
    titleLabel: 'Başlık',
    titlePlaceholder: 'Quiz gecesi skor tablosu',
    descriptionLabel: 'Açıklama',
    descriptionPlaceholder:
      'Takımlar cevaplarını telefondan girer, quiz’i yöneten kişi puan verir ve skor tablosu salondaki herkes için güncellenir.',
    save: 'Değişiklikleri kaydet',
    add: 'Vitrine ekle',
    takeOff: 'Kaldır',
    needs: (missing) =>
      `Eksik: ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} ve ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Bazı alanlar çok uzun.',
    allSaved: 'Her şey kaydedildi.',
    preview: 'Önizleme',
    card: (address) => <>Ziyaretçilerin gördüğü kart bu. Tıklanınca {address} açılır.</>,
    confirm: 'Vitrinden kaldırılsın mı?',
    keep: 'Kalsın',
    confirmText: 'Başlık, açıklama ve görsel silinir. Lambdanın kendisi olduğu gibi kalır.',
    pictureLabel: 'Görsel',
    formats: (limit) => `PNG, JPEG, GIF ya da WebP, en fazla ${limit}`,
    notSaved: 'henüz kaydedilmedi',
    replace: 'Değiştirmek için yenisini buraya bırakın.',
    drop: 'Bir görseli buraya bırakın.',
    advice: 'En iyi sonucu 16:10 oranında bir ekran görüntüsü ya da kullanımını gösteren kısa bir GIF verir.',
    another: 'Başkasını seç',
    choose: 'Dosya seç',
    keepSaved: 'Kaydedileni koru',
    clear: 'Temizle',
  },

  domain: {
    readFailed: 'Alan adı okunamadı.',
    reaching: (domain) => `${domain} alan adına gelen istekler artık bu lambdaya ulaşıyor.`,
    saveFailed: 'Alan adı kaydedilemedi.',
    removed: 'Alan adı kaldırıldı. Lambda buradaki adresinde yanıt vermeye devam ediyor.',
    removeFailed: 'Alan adı kaldırılamadı.',
    hint:
      'Premium bir lambda, buradaki adresinin yanı sıra kendi alan adında da yanıt verebilir, hem de kökten itibaren alan adının tamamında. Alan adını bu sunucuya yönlendirin ve buraya girin. O alan adına gelen istekler lambdaya ulaşır.',
    loading: 'Yükleniyor…',
    example: 'alan-adiniz.com',
    open: (domain) => `${domain} adresini aç`,
    label: 'Yanıt verdiği alan adı',
    serving: (domain) => <>Buradaki adresinin yanı sıra artık {domain} alan adında da yayında.</>,
    none: 'Henüz yok. shop.example.com gibi bir alt alan adı ya da example.com gibi bütün bir alan adı olabilir.',
    change: 'Değiştir',
    use: 'Bu alan adını kullan',
    remove: 'Kaldır',
    confirm: 'Alan adı kaldırılsın mı?',
    keep: 'Kalsın',
    confirmText: (domain) => (
      <>
        {domain} alan adına gelen istekler hemen bu lambdaya ulaşmayı bırakır. Lambdanın buradaki adresi olduğu gibi
        kalır, alan adının DNS kayıtları da.
      </>
    ),
    point: 'Alan adını bu sunucuya yönlendirin',
    check: 'Tekrar kontrol et',
    records:
      'Alan adınızın DNS kayıtlarını yönettiğiniz yerde şu iki kaydı ekleyin. IPv6 üzerinden erişilebilir olmak istemiyorsanız AAAA kaydını eklemeyin.',
    type: 'Tür',
    name: 'Ad',
    value: 'Değer',
    pointsHere: (domain) => <>{domain} alan adı bu sunucuya yönleniyor.</>,
    alsoElsewhere: (addresses) =>
      ` Ayrıca şu adreslere de çözümleniyor: ${addresses}. Bunlar bu sunucu değil, oraya giden ziyaretçiler lambdaya ulaşamaz.`,
    elsewhere: (addresses) => `Şu adreslere çözümleniyor: ${addresses}. Bunlar henüz bu sunucu değil.`,
    wait: 'Bir değişikliğin her yerde görünmesi biraz sürebilir, en fazla eski kaydın TTL süresi kadar.',
    cname: 'Bunun yerine CNAME kaydı kullanmak',
    cnameText: (target) => (
      <>
        Bir alt alan adı, bunun yerine CNAME kaydıyla {target} adresine yönlendirilebilir. O zaman bu sunucunun adresleri
        değişse bile onu takip eder. Ama bazı dezavantajları var:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Bütün bir alan adı için, yani {example} alan adının kendisi için kullanılamaz. Standart, her alan adının kökünde
        bulunan kayıtların yanında CNAME olmasına izin vermez. Bazı sağlayıcılar bunun yerine orada çalışan ALIAS, ANAME
        ya da “düzleştirilmiş” (flattened) bir kayıt sunar.
      </>
    ),
    cnameAlone: 'Aynı ada başka hiçbir kayıt eklenemez: ne e-posta için MX kaydı ne de doğrulamalar için TXT kaydı.',
    cnameLookup: 'Ziyaretçilerin çözümleyicileri, siteye ulaşmadan önce bir sorgu daha yapar.',
    copy: 'Kopyala',
    copyValue: (value) => `Kopyala: ${value}`,
  },

  code: {
    title: 'Kod',
    version: (version) => `sürüm ${version}`,
    edited: ', düzenlendi',
    online: ', yayında',
    loadFailed: 'Bu sürüm yüklenemedi.',
    compiles: 'Sorunsuz derleniyor.',
    notYet: 'Henüz derlenmiyor.',
    checkFailed: 'Kod kontrol edilemedi.',
    saved: (version) => `Sürüm ${version} olarak kaydedildi.`,
    isOnline: (version) => `Sürüm ${version} yayında.`,
    notOnline: 'Yayına alınamadı. Derleyicinin ne dediğine aşağıdan bakın.',
    failed: 'Bir şeyler ters gitti.',
    unchanged: 'Son kayıttan beri hiçbir şey değişmedi.',
    demo: 'Bu bir demo, buradaki her şey salt okunur. Değiştirmek için bundan kendi lambdanızı oluşturun. ',
    edit: 'Kodu elle düzenleyin. Kaydettiğinizde yeni bir sürüm oluşur, yayındaki sürüm değişmez. Yayına aldığınızda ise yeni sürüm yayına girer. ',
    files: (entry, cs) => (
      <>
        {entry} sunulacak şeyi döndürür, diğer {cs} dosyaları türleri barındırır, geri kalan her dosya olduğu gibi
        sunulur. Ctrl-S kaydeder, F12 bir tanıma gider.
      </>
    ),
    newer: (version) => ` Sürüm ${version}, burada açık olandan daha yeni.`,
    check: 'Kontrol et',
    save: 'Kaydet',
    deploy: 'Yayına al',
    binary: (size) => `Metin değil, düzenlenecek bir şey yok. Olduğu gibi sunulur. Boyutu ${size} kB.`,
    saveAndDeploy: 'Kaydet ve yayına al',
    saveVersion: 'Yeni sürüm kaydet',
    cancel: 'İptal',
    what: 'Ne değişiyor? İsteğe bağlı, geçmişte gösterilir.',
    placeholder: 'İletişim formu ekler',
    goToDefinition: 'Tanıma git',
  },

  tabs: {
    codeName: 'Harf, rakam, tire ve alt çizgi; sonu .cs olmalı',
    slashes: 'Başında ya da sonunda eğik çizgi olmamalı, 120 karakterden kısa olmalı.',
    deep: 'En fazla altı klasör derinliğinde olabilir.',
    characters: 'Eğik çizgiyle ayrılmış harf, rakam, tire, alt çizgi ve nokta.',
    extension: 'Doğru türde sunulabilmesi için bir uzantısı olmalı.',
    exists: 'Bu adda bir dosya zaten var.',
    remove: (name) => `${name} kaldırılsın mı? İçeriği de silinir.`,
    there: (name) => `${name} zaten var.`,
    entry: 'Kod parçası: döndürdüğü şey sunulur',
    errors: 'hata içeriyor',
    removeFile: (name) => `Kaldır: ${name}`,
    removeTitle: 'Bu dosyayı kaldır',
    placeholder: 'Types.cs ya da site/index.html',
    newFile: 'Yeni dosya',
    uploadTitle: 'Dosya yükle: görsel, font, sayfa',
    upload: 'Dosya yükle',
  },
};
