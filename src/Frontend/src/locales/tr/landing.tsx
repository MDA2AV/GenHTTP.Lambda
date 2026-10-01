import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Uygulama yapan yapay zekâ, hosting dahil',
  headline: 'Uygulamanızı anlatın.',
  headlineAccent: 'Ajanınız yayına alsın.',
  intro:
    'Anketler, ziyaretçi defterleri, skor tabloları, çok oyunculu oyunlar. Ne istediğinizi bizim yapay zekâ ajanımıza ya da kendi ajanınıza anlatın. Çalışan bir uygulama ve paylaşabileceğiniz bir link alın; hosting’i biz üstleniriz. Uygulama hep düzenlenebilir kalır, yani ilk sürümden çok sonra da geliştirmeye devam edebilirsiniz.',
  build: 'Uygulama oluşturun',
  ownAgent: 'Kendi ajanınızı kullanın',
  free: 'Ücretsiz. Üyelik yok, kredi kartı yok, kurulum yok.',
  seeIt: 'Nasıl çalıştığını görün',

  videoTitle: 'Tek cümleden çalışan uygulamaya',
  videoText:
    'Gizli bir tarayıcı penceresi, hesap yok, oluşturma sayfasında tek bir istek. Sonra bitmiş uygulama, herhangi bir ziyaretçi gibi kendi linkinden açılıyor.',
  videoNote: 'Oluşturma kısmı hızlandırıldı, gerisi gerçek zamanlı.',
  tryIt: 'Kendiniz deneyin',

  oneShotTitle: 'Tek seferlik bir uygulama üreticisi değil',
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
      body: 'Uygulama yapılır, barındırılır ve size paylaşabileceğiniz herkese açık bir adres verilir. Sunucu, hosting paketi, alan adı ya da veritabanı kurmanız gerekmez; o kısım bizde. Oylar, skorlar, mesajlar saklanır, böylece açan herkes aynı şeyi görür.',
      alt: 'Bitmiş öğle yemeği anketi, tarayıcıda açık',
    },
    {
      title: 'Geliştirmeye devam edin',
      body: 'Her uygulamanın özel bir editör linki var. Linki, istediğiniz değişiklikle birlikte ajanınıza verin ya da kendiniz açın. Her değişiklik yeni bir sürüm olur, adres aynı kalır.',
      alt: 'Anketin kontrol paneli: sürümleri ve her birinde ne istendiği, neyin değiştiği, bir öncekinden farkı',
    },
  ],
  weekLater: 'Bir hafta sonra',
  weekAsk:
    'Öğle yemeği anketimin editör linki bu. Cuma günleri oylamayı saat 11’de kapat, kazananı da en üstte göster lütfen.',
  weekAnswer:
    'Hallettim. Sürüm 4 aynı adreste yayında. Geri dönmek isterseniz sürüm 3 hâlâ duruyor.',

  agentsTitle: 'Kendi ajanınızla çalışın: Claude, Codex, Cursor',
  agentsText:
    'Claude Code, Codex, Cursor ya da başka bir asistanla zaten vibe coding mi yapıyorsunuz? Onu bu adrese bağlayın; bu bir uzak MCP sunucusu, anahtar gerekmez. Artık burada uygulama oluşturabilir, yayına alabilir ve güncelleyebilir. Hem de açık olan sohbetten hiç çıkmadan.',
  thenAsk: (em) => (
    <>Sonra şunu istemeniz yeterli: {em('ekip etkinliğimiz için bir kayıt listesi yap ve yayına al')}.</>
  ),
  hostIt: (link) => (
    <>Yalnızca localhost’ta çalışan bir şey mi yaptınız? {link('Vibe coding projenizi burada yayınlayın')}.</>
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
