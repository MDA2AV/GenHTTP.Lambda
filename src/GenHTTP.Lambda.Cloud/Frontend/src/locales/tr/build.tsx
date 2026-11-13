import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Yapay zekâ ile web sitesi yapın.',
  intro:
    'Aklınızdaki web sitesini veya uygulamayı kendi cümlelerinizle anlatın. Yapay zekâ sizin için yapar, biz barındırırız ve siteniz dakikalar içinde, herkese gönderebileceğiniz bir linkle yayında olur. Ücretsiz; kod bilmenize ya da üye olmanıza gerek yok.',
  points: [
    {
      title: 'Kod bilmenize gerek yok',
      text: 'Web sitenizin ne yapması gerektiğini, bir arkadaşınıza anlatır gibi kendi cümlelerinizle söyleyin. Yapay zekâ sizin için yapar; teknik bilgi gerekmez.',
    },
    {
      title: 'Ücretsiz hosting dahil',
      text: 'Web siteniz bizim sunucularımızda çalışır. Hosting paketi, sunucu ya da domain satın almanız, bir şey kurmanız gerekmez; güvenlik ve güncellemelerle biz ilgileniriz.',
    },
    {
      title: 'Dakikalar içinde yayında',
      text: 'Paylaşabileceğiniz linki hemen alırsınız. Site, insanların girdiklerini hatırlar: kayıtlar, oylar, mesajlar, skorlar. Böylece herkes aynı şeyi görür.',
    },
  ],

  questionsTitle: 'Başlamadan önce',
  questions: (offline, removed) => [
    [
      'Yapay zekâ ile gerçekten ücretsiz web sitesi yapılabilir mi?',
      `Evet. Kendi cümlelerinizle anlatın; yapay zekâ siteyi yapar, yayına alır ve linkini size verir. Üyelik yok, kredi kartı yok, deneme süresi yok. Site, insanlar kullandığı sürece yayında kalır: ${offline} gün boyunca ziyaret veya değişiklik olmazsa yayından kaldırılır, ${removed} gün sonra da silinir.`,
    ],
    [
      'Hosting, sunucu ya da domain gerekir mi?',
      'Hayır. Web siteniz bizim sunucularımızda çalışır; hosting, güvenlik ve güncellemeler dahildir. Linkinizi hemen alırsınız, yani domain satın almanız da gerekmez.',
    ],
    [
      'Kod bilmeden uygulama yapabilir miyim?',
      'Evet. Hiç kod görmezsiniz. Ne yapması gerektiğini bir arkadaşınıza anlatır gibi söyleyin, gerisini yapay zekâ halleder: bir web sitesi, küçük bir uygulama ya da bir oyun.',
    ],
    [
      'İnsanlar bir şeyler girebilir mi? Kayıt, oy, mesaj?',
      'Evet. Web siteniz insanların girdiklerini hatırlar; linki açan herkes aynı kayıtları, oyları ve skorları görür.',
    ],
    [
      'Başkaları siteyi nasıl açar?',
      'Linkle; herhangi bir tarayıcıda, telefonda ya da bilgisayarda. Kurulacak bir şey yoktur, arada bir uygulama mağazası da yoktur.',
    ],
    [
      'Sonradan nasıl değiştiririm?',
      'Web sitenizle birlikte aldığınız editör linkini açın ve burada olduğu gibi neyin farklı olması gerektiğini anlatın. Bir değişikliği beğenmezseniz sitenin önceki hâline dönebilirsiniz.',
    ],
  ],

};
