import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Ajanınızı bağlayın',
  title: 'Zaten kullandığınız ajandan yayınlayın.',
  intro:
    'Claude Code, Codex, Cursor ya da MCP konuşan herhangi bir ajan bu kurulumu araç olarak alır: uygulama oluşturur, bir değişikliği taslakta dener, yayına alır ve loglarını okur. Her uygulama kendi adresinde ve kendi veritabanıyla yanıt verir.',
  stepsTitle: 'Yayında bir uygulamaya üç adım',
  step: (n) => `Adım ${n}`,
  steps: [
    {
      title: 'Bir kez bağlayın',
      body: 'Bu kurulumun MCP sunucusunun adresini ajanınıza ekleyin. Anahtar gerekmez: bir uygulamanın editör anahtarına sahip olan o uygulamayı değiştirebilir, başka hiçbir şeyi değil.',
    },
    {
      title: 'Oluşturmasını ya da yayınlamasını isteyin',
      body: 'Yeni bir uygulamayı anlatın ya da projenizdeki uygulamayı yayınlamasını isteyin. Ajan kodu yazar, derlendiğini kontrol eder ve yayına alır.',
    },
    {
      title: 'Editör linkini saklayın',
      body: 'Uygulamanın adresini ve editör linkini alırsınız. Adres herkes içindir; editör linki ise uygulamayı daha sonra sizin - ya da ajanınızın - değiştirme yoludur.',
    },
  ],
  sayLike: 'Sonra şuna benzer bir şey söyleyin',
  asks: ['Onayladığım kayıtların göründüğü bir sayfası olan bir ziyaretçi defteri oluştur.', 'Bu uygulamayı burada yayınla ve linkini bana gönder.'],
  noAgent: 'Ajanınız yok mu? Oluşturma ajanını kullanın',

  questionsTitle: 'Bilmekte fayda var',
  questions: (create) => [
    [
      'Hangi ajanlar bağlanabilir?',
      'HTTP üzerinden uzak bir MCP sunucusu ekleyebilen her ajan: Claude Code, Claude, Codex, Cursor, VS Code ve diğerleri, yukarıdaki adresle.',
    ],
    [
      'Bir uygulamayı sonradan nasıl değiştiririm?',
      'Bir sonraki değişiklikle birlikte uygulamanın editör linkini ajanınıza verin ya da linki kendiniz açın: editör uygulamanın sürümlerini, kodunu, kayıtlarını ve loglarını gösterir. Bir değişiklik, yayına alınmadan önce verilerin bir kopyasıyla bir taslakta denenir.',
    ],
    [
      'Bir uygulama üzerinde git ile çalışabilir miyim?',
      'Evet. Her uygulama bir git deposudur: editörden klonlayın, bir sürümü kaydetmek için main’e bir commit push edin ya da kendi önizlemesi olan bir taslak başlatmak için bir dal push edin.',
    ],
    [
      'API anahtarları ve parolalar nereye konur?',
      'Koda değil. Ajan onları adıyla okur, değerleri ise siz editörde girersiniz; orada kimse onları geri okuyamaz - ajan da.',
    ],
    [
      'Ajan olmadan uygulama yazabilir miyim?',
      <>Evet: demolardan birinden başlayarak {create('kodu kendiniz yazın')}. Bir uygulama, bu sunucuda derlenen bir C# GenHTTP handler’ıdır.</>,
    ],
  ],
};
