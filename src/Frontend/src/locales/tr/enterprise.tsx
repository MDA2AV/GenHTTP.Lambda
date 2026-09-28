import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Kurumsal',
  title: 'Ücretsiz deneyin, kendiniz çalıştırın',
  intro:
    'Buradaki her şey ücretsiz ve hesap gerektirmez. Ekibiniz kalıcı olarak yayında kalan ve kendi girişinizle korunan uygulamalar istiyorsa, size özel bir kurulum edinin: bulutta ya da kendi sunucularınızda.',

  free: 'Ücretsiz',
  freeTagline: 'Denemek için',
  forever: 'sonsuza dek',
  buildOne: 'Uygulama oluşturun',
  freeFeatures: (offline, removed) => [
    'Sınırsız lambda, hesap yok',
    'Yerleşik ajan ya da MCP üzerinden kendi ajanınız',
    'Kullanıldığı sürece yayında',
    `${offline} gün ziyaret olmazsa yayından kalkar, ${removed} günün sonunda silinir`,
    'Ortak sunucuda bir alt yolda sunulur',
  ],
  freeNote: 'Kart yok, kayıt yok. Bir lambda oluşturun, artık sizindir.',

  name: 'Kurumsal',
  tagline: 'Kendine ait bir kurulum isteyen ekipler için',
  perUser: 'kullanıcı başına / ay',
  contact: 'Bize ulaşın',
  features: [
    'Size özel kurulum, bulutta ya da kendi sunucularınızda',
    'Tüm uygulamalar tek bir serviste çalışır',
    'Kendi SSO’nuzla giriş',
    'Yönetişim ve uyumluluk kurallarınız yerleşik',
    'Uygulamalar kalıcı olarak yayında kalır, hiçbir şey silinmez',
    'MCP üzerinden kendi ajanınızı bağlayın',
    'Öncelikli destek',
  ],
  users: (count) => <>{count} kullanıcı</>,
  perMonth: ' / ay',
  price: (amount) => `$${amount.toLocaleString('tr-TR')}`,
  perUserPrice: (amount) => `$${amount.toLocaleString('tr-TR')} / kullanıcı / ay`,

  compareTitle: 'Planları karşılaştırın',
  compareText: 'İkisi de aynı platformda çalışır. Değişen tek şey, uygulamanızın ne kadar süre ve nerede tutulduğu.',
  included: 'Dahil',
  notIncluded: 'Dahil değil',
  groups: (offline, removed) => [
    {
      title: 'Oluşturma',
      rows: [
        ['Lambdalar', 'Sınırsız', 'Sınırsız'],
        ['Yerleşik ajan', true, false],
        ['MCP üzerinden kendi ajanınız', true, true],
        ['Editör, sürümler ve loglar', true, true],
        ['Vitrin', true, 'Size özel'],
      ],
    },
    {
      title: 'Barındırma',
      rows: [
        ['Kullanılmayınca yayından kalkar', `${offline} gün sonra`, 'Asla'],
        ['Kullanılmayınca silinir', `${removed} gün sonra`, 'Asla'],
        ['Kurulum', 'Ortak', 'Size özel'],
        ['Nerede çalışır', 'Bizim bulutumuzda', 'Bulutta ya da kendi sunucularınızda'],
        ['Sizin yönettiğiniz', 'Hiçbir şey', 'Tek bir servis'],
        ['Özel alan adları', false, true],
      ],
    },
    {
      title: 'Kontrol',
      rows: [
        ['Giriş', 'Gerekmez', 'Kendi SSO’nuz'],
        ['Ajanlar için yönetişim ve uyumluluk kurallarınız', false, true],
        ['Yönetim konsolu', false, true],
        ['Veriler diğer müşterilerden ayrı', false, true],
        ['Destek', 'Topluluk', 'Öncelikli'],
      ],
    },
  ],

  questionsTitle: 'Sorular',
  questions: [
    [
      'Başlamak için hesap gerekiyor mu?',
      'Hayır. Ücretsiz bir lambda için, oluştururken aldığınız editör linkinden başka hiçbir şey gerekmez.',
    ],
    [
      'Kurumsal planda kimler kullanıcı sayılır?',
      'SSO’nuz üzerinden giriş yapan herkes. İster editörde bir şey oluşturmak, ister kurulumunuzda yayındaki bir uygulamayı kullanmak için giriş yapsın. Giriş yapmadan bir uygulamaya erişenler sayılmaz.',
    ],
    [
      'Yerleşik ajan Kurumsal plana dahil mi?',
      'Hayır. Ekibiniz kendi ajanını (Claude, Claude Code ya da MCP destekleyen başka bir araç) kurulumunuza bağlar. Bunun için ajanın sağlayıcısında zaten kullandığınız plan yeterli.',
    ],
    [
      'Ajanlar uyumluluk kurallarımızı nasıl öğrenir?',
      'Yönetişim ve uyumluluk kurallarınızı, platformun MCP üzerinden ajanlara verdiği bilgilere ekliyoruz. Ekibinizin bağladığı her ajan, kod yazarken bu kuralları alır. Böylece kimsenin onları ezbere bilmesi gerekmez, uygulamalar zaten kurallarınıza uygun çıkar.',
    ],
    [
      'Kubernetes ya da bir cluster gerekiyor mu?',
      'Hayır. Her uygulama tek bir servisin içinde çalışır. Dağıtılacak pod yok, uygulama başına yönetilecek bir şey yok. Kurulumu işletmek, o tek servisi çalıştırmak demek.',
    ],
    [
      'Kurumsal kurulum nerede çalışır?',
      'Nerede isterseniz. Sizin için kendi bulutumuzda barındırabiliriz. Ya da sizin bulut hesabınızda veya kendi sunucularınızda, konteyner çalıştırabilen her yerde çalışır. Her iki durumda da kurulumda yardım eder, sistemi güncel tutarız.',
    ],
  ],
  anythingElse: (mail) => <>Başka bir sorunuz mu var? {mail} adresine yazın.</>,
};
