import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Gizlilik politikası',
  binding: (english) => (
    <>
      Bu çeviri yalnızca bilgilendirme amaçlıdır. Hukuken bağlayıcı olan, bu politikanın {english('İngilizce metnidir')}.
    </>
  ),
  intro:
    'Bu sayfa, sitenin sizin hakkınızda neler öğrendiğini, bunlarla ne yaptığını, ne kadar süre sakladığını ve başka kimlerin görebildiğini anlatır. Kısacası: hesap yok, reklam yok, takip yok. Sunucu, çalışmaya devam edebilmek ve kötüye kullanımın izini sürebilmek için kimin ne istediğini not eder. Oluşturma ajanından istedikleriniz ise uygulamayı yazan modeli geliştiren Anthropic’e gönderilir.',
  sections: {
    whoTitle: 'Sorumlu kim',
    who: 'Bu siteyi aşağıdaki kişi işletir. Sitenin işlediği kişisel veriler için AB Genel Veri Koruma Tüzüğü (GDPR) kapsamında veri sorumlusu odur. Bu sayfadaki her konu için bu adrese yazabilirsiniz:',

    requestsTitle: 'Sunucunun her istekte kaydettikleri',
    requests: [
      'Bu siteye ve burada barındırılan her lambdaya gelen her istek sunucunun loguna yazılır: isteğin geldiği IP adresi, istek bir aracı üzerinden geldiyse adına iletildiğini söylediği IP adresi, isteği gönderen tarayıcı ya da program, istenen adres, isteğin zamanı ve nasıl yanıtlandığı. Sunucu ayrıca IP adresinin hangi ülkeye, şehre ve ağa ait olduğuna kendi tuttuğu bir veritabanından bakar. Başka hiç kimseye sorulmaz.',
      'Hataları bu sayede buluruz. Sunucuyu aşırı yükleyen şeyi bu sayede tespit ederiz. Bize bildirilen kötüye kullanımın izini de bu sayede süreriz. IP adresi ayrıca, yalnızca bellekte, bir ziyaretçinin kaç istek gönderip kaç uygulama oluşturabileceğini sınırlamak için kullanılır. Bu bilgiler olmadan bir isteğe yanıt verilemez. Hukuki dayanak, hizmeti işletme ve güvende tutma konusundaki meşru menfaatimizdir (GDPR md. 6/1-f).',
      'Yöneticiler bunların hepsini görebilir. Bir lambdanın sahibi, kendi lambdasına gelen her isteğin ülkesini ve tarayıcısını görür ama IP adresini göremez.',
    ],

    logsTitle: 'Log ne kadar süre saklanır',
    logs: 'Log iki yerde tutulur: sunucunun belleğinde ve sunucunun konsol çıktısında. Bellek sunucu her yeniden başladığında boşalır, konsol çıktısı da sunucu her güncellendiğinde silinir. İkisinin de boyutu sabittir, yani her yeni satır en eski satırı dışarı iter. Bu yüzden bir satırın ne kadar kalacağı, sitenin ne kadar yoğun olduğuna bağlıdır. Logdaki hiçbir şey bir arşive kopyalanmaz.',

    contentTitle: 'Buraya koyduklarınız',
    content: (days) =>
      `Bir lambda; kodundan, dosyalarından, ayarlarından ve sürümleriyle birlikte kaydedilen notlardan oluşur. Bu notlar neyin istendiğini ve neyin değiştiğini anlatır. Lambda çalıştırılabilsin ve düzenlenebilsin diye bunların hepsi sunucuda saklanır. Ücretsiz plandaki bir lambda, en son değiştirildiği ya da ziyaret edildiği andan yaklaşık ${days} gün sonra tüm sürümleriyle birlikte silinir. Editör linkine sahip biri onu silerse hemen silinir. Editör linkine sahip olan herkes bunların hepsini okuyabilir. Vitrine koyduğunuz şeyleri herkes görebilir. Yöneticiler de gerektiğinde, örneğin bir bildirimi incelemek ya da sunucuyu güvende tutmak için bir lambdaya bakar. Hukuki dayanak, istediğiniz hizmeti size sunmaktır (GDPR md. 6/1-b).`,

    agentTitle: 'Oluşturma ajanından istedikleriniz',
    agent: (policy) => (
      <>
        Oluşturma kutusuna yazdıklarınız, uygulamayı yazan model Claude’u işleten ve ABD’de bulunan Anthropic PBC’ye
        gönderilir. Anthropic’in bunlarla ne yaptığı, şirketin {policy('kendi gizlilik politikasında')} anlatılır. ABD,
        kişisel verileri AB’nin koruduğu gibi korumaz. İsteğiniz oraya gönderilir, çünkü istediğiniz şeyi oluşturmak için
        bu gereklidir (GDPR md. 6/1-b ve md. 49/1-b). Bu yüzden paylaşmak istemeyeceğiniz hiçbir şeyi oraya yazmayın.
      </>
    ),
    agentKept:
      'Ajan isteğinizi, çoğu zaman kendi cümleleriyle, yazdığı sürümün notu olarak kaydeder. İsteğin ilk birkaç yüz karakteri de oluşturma hizmetinin loguna yazılır. Bu logun da boyutu sabittir. Bunun yerine Claude ya da Claude Code gibi kendi ajanınızı kullanırsanız, ona söyledikleriniz bize değil, o ajanın sağlayıcısına gider. Biz yalnızca ajanın buraya gönderdiği kodu ve notları alırız.',

    lambdasTitle: 'Bir lambdanın ne yaptığına sahibi karar verir',
    lambdas:
      'Bir lambdayı biz değil, editör linkine sahip olan kişi yazar. Lambdanın ziyaretçilerinden ne istediği ve bununla ne yaptığı o kişiye bağlıdır. Bu sayfa bunları kapsamaz. Tek istisna, yukarıda anlatılan ve sunucunun her lambda için tuttuğu istek logudur. Kullanım koşulları, bir lambdanın başkaları hakkında kişisel veri toplamak için kullanılmasına izin vermez. Bunu yapan bir lambdaya rastlarsanız lütfen bildirin.',

    mailTitle: 'Bize yazdığınızda',
    mail: 'Kötüye kullanımı bildirmek ya da başka bir konu için bize yazarsanız, e-posta adresinizi ve mesajınızı size yanıt vermek ve yazdığınız konuyla ilgilenmek için kullanırız. Bu iş için artık gerekmediklerinde onları sileriz (GDPR md. 6/1-f).',

    storageTitle: 'Çerezler ve tarayıcınız',
    storage:
      'Tek bir çerez var, adı lang. Seçtiğiniz dili hatırlar, böylece dil belirtmeyen adresler o dilde açılır. Bir yıl geçerlidir. Tarayıcının kendi depolama alanı ise açık ya da koyu temayı, kullandığınız sayfaların birkaç ayarını ve yöneticiler için onların erişim anahtarını hatırlar. Bunların hiçbiri sizi takip etmek için kullanılmaz ve hiçbiri başka birine gitmez: analiz aracı yok, reklam yok, başka sitelerden hiçbir şey yüklenmez, yazı tipleri bile. Bunların hepsi yalnızca sizin istediğiniz şeyi yaptığı için onay gerekmez (Alman TDDDG yasası, § 25(2) no. 2).',

    hostingTitle: 'Nerede saklanır',
    hosting:
      'Bunların hepsinin çalıştığı sunucu, Avrupa Birliği’ndeki bir barındırma sağlayıcısından kiralanmıştır. Bu sayfada anlatılan her şey orada saklanır.',

    rightsTitle: 'Haklarınız',
    rights: (mailbox) => (
      <>
        Hakkınızda neyi sakladığımızı sorabilir ve bir kopyasını isteyebilirsiniz. Bu verilerin düzeltilmesini,
        silinmesini ya da kullanımının kısıtlanmasını talep edebilir, meşru menfaatimize dayanarak yaptığımız her şeye
        itiraz edebilirsiniz (GDPR md. 15-21). Bunun için {mailbox} adresine yazın. Hesap olmadığı için size ait olanı
        ancak nasıl bulacağımızı söylerseniz bulabiliriz: kullandığınız IP adresini ve bunu yaklaşık ne zaman
        kullandığınızı ya da lambdanızın adresini yazın. Hakkınızda hukuki sonuç doğuran ya da sizi benzer şekilde önemli
        ölçüde etkileyen hiçbir karar otomatik olarak verilmez (GDPR md. 22).
      </>
    ),
    complaint:
      'Yaşadığınız yerdeki ya da bizim bulunduğumuz yerdeki bir veri koruma makamına şikâyette de bulunabilirsiniz. Bizim bağlı olduğumuz makam, Almanya’nın Baden-Württemberg eyaletinin veri koruma ve bilgi edinme özgürlüğü komiserliğidir (LfDI Baden-Württemberg).',
  },
  change: 'Bu politika, site değiştikçe değişir. Geçerli olan, bu sayfadaki sürümdür.',
  updated: 'Son güncelleme: 28 Eylül 2026.',
};
