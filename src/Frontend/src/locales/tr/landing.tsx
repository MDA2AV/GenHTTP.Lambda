import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Yapay zekâ ajanlarıyla kodlama',
  headline: 'Uygulamanızı anlatın.',
  headlineAccent: 'Ajanınız yayına alsın.',
  intro:
    'Anketler, ziyaretçi defterleri, skor tabloları, küçük mağazalar. Ne istediğinizi bizim ajanımıza ya da kendi ajanınıza anlatın. Çalışan bir uygulama ve paylaşabileceğiniz bir link alın. Uygulama hep düzenlenebilir kalır, yani ilk sürümden çok sonra da geliştirmeye devam edebilirsiniz.',
  build: 'Uygulama oluşturun',
  ownAgent: 'Kendi ajanınızı kullanın',
  free: 'Ücretsiz. Hesap yok, kurulum yok.',
  seeIt: 'Nasıl çalıştığını görün',

  videoTitle: 'Tek cümleden çalışan uygulamaya',
  videoText:
    'Gizli bir tarayıcı penceresi, hesap yok, oluşturma sayfasında tek bir istek. Sonra bitmiş uygulama, herhangi bir ziyaretçi gibi kendi linkinden açılıyor.',
  videoNote: 'Oluşturma kısmı hızlandırıldı, gerisi gerçek zamanlı.',
  tryIt: 'Kendiniz deneyin',

  oneShotTitle: 'Tek seferlik değil',
  oneShotText:
    'Çoğu araç bir sonuç üretir ve sizi onunla baş başa bırakır. Burada uygulama, yapıldığı yerde çalışmaya devam eder. Siz de ajanınızla onu geliştirmeyi sürdürürsünüz.',
  steps: [
    {
      title: 'Ne istediğinizi söyleyin',
      body: 'Günlük dille anlatın: bu sitedeki ajana ya da kendi ajanınıza. Kod yok, kurulum yok, hesap yok.',
      alt: 'Oluşturma sayfası, kutuya öğle yemeği anketi isteği yazılmış',
    },
    {
      title: 'Çalışan bir uygulama ve link alın',
      body: 'Uygulama yapılır, yayına alınır ve size paylaşabileceğiniz herkese açık bir adres verilir. Oylar, skorlar, mesajlar saklanır, böylece açan herkes aynı şeyi görür.',
      alt: 'Bitmiş öğle yemeği anketi, tarayıcıda açık',
    },
    {
      title: 'Geliştirmeye devam edin',
      body: 'Her uygulamanın özel bir editör linki var. Linki, istediğiniz değişiklikle birlikte ajanınıza verin ya da kendiniz açın. İstediğiniz her değişiklik ayrı bir sürüm olur, adres aynı kalır.',
      alt: 'Anketin kontrol paneli: sürümleri ve her birinde ne istendiği, neyin değiştiği, bir öncekinden farkı',
    },
  ],
  weekLater: 'Bir hafta sonra',
  weekAsk:
    'Öğle yemeği anketimin editör linki bu. Cuma günleri oylamayı saat 11’de kapat, kazananı da en üstte göster lütfen.',
  weekAnswer:
    'Hallettim. Sürüm 4 aynı adreste yayında. Geri dönmek isterseniz sürüm 3 hâlâ duruyor.',

  agentsTitle: 'Sevdiğiniz ajanla çalışın',
  agentsText:
    'Zaten Claude ya da başka bir asistanla mı çalışıyorsunuz? Onu bu adrese bağlayın. Artık burada uygulama oluşturabilir, yayına alabilir ve güncelleyebilir. Hem de açık olan sohbetten hiç çıkmadan.',
  agents: [
    {
      name: 'Claude (web veya masaüstü)',
      how: 'Ayarlar’ı açın, “Connectors” bölümüne gidin ve “Add custom connector” seçeneğine tıklayın. Yukarıdaki adresi yapıştırın. API anahtarı ya da giriş gerekmez.',
    },
    {
      name: 'Claude Code',
      how: 'Terminalde bunu bir kez çalıştırın:',
    },
    {
      name: 'Diğer MCP istemcileri',
      how: 'Cursor, VS Code, Codex ve diğer MCP istemcileri uzak sunucuları destekler. Aynı adresi girmeniz yeterli.',
    },
  ],
  thenAsk: (em) => (
    <>Sonra şunu istemeniz yeterli: {em('ekip etkinliğimiz için bir kayıt listesi yap ve yayına al')}.</>
  ),

  contactTitle: 'Konuşalım',
  contactText:
    'Yardım mı lazım? Daha büyük bir şey mi planlıyorsunuz, yoksa size özel bir çözüm mü arıyorsunuz? Sizden haber bekliyoruz.',
  mailTitle: 'E-posta gönderin',
  mailText: 'Projeler, sorular ve özel olarak konuşmak istediğiniz her şey için.',
  discordTitle: 'Discord’a katılın',
  discordText: 'Yaptıklarınızı paylaşın, sonraki adım için yardım alın, ekiple doğrudan konuşun.',
  discordLink: 'GenHTTP Discord sunucusu',
};
