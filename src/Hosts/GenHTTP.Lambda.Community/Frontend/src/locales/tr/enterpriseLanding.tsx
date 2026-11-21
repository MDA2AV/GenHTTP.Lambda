import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'İşiniz için uygulamalar',
  headline: 'İşinizin ihtiyaç duyduğu uygulamayı anlatın.',
  headlineAccent: 'Bir yapay zekâ ajanı onu oluşturur ve burada yayına alır.',
  intro:
    'Bir form, bir rezervasyon takvimi, ekibiniz için bir liste: ne yapması gerektiğini kendi sözlerinizle söyleyin. Dakikalar içinde iş arkadaşlarınızla paylaşabileceğiniz bir linkiniz olur; uygulamayı daha sonra da aynı şekilde, tek satır kod yazmadan değiştirirsiniz.',
  describe: 'Bir uygulama anlatın',
  own: 'Kendi yapay zekâ ajanınızı kullanın',

  examplesTitle: 'Burada insanlar neler oluşturuyor',
  examples: [
    { title: 'Formlar ve talepler', text: 'İzin, satın alma, geri bildirim: kişiler doldurur, karar verecek kişi onaylar.' },
    { title: 'Rezervasyonlar', text: 'Toplantı odaları, masalar, ekipmanlar: kimde ne var ve ne zaman.' },
    { title: 'Listeler ve takip araçları', text: 'Destek talepleri, envanter, işe alıştırma: nelerin açık olduğu ve kimin ilgilendiği.' },
    { title: 'Ekipler için sayfalar', text: 'Bir program, bir el kitabı, haberler: herkesin açtığı tek bir link.' },
  ],

  stepsTitle: 'Nasıl çalışır',
  steps: [
    {
      title: 'Anlatın',
      text: 'Uygulamanın ne yapması gerektiğini bir iş arkadaşınıza anlatır gibi söyleyin. Kutunun altındaki fikirler bir başlangıç noktasıdır.',
    },
    {
      title: 'Linki paylaşın',
      text: 'Birkaç dakika sonra uygulama burada yayında olur. Linkini kullanacak kişilere gönderin ve editör linkinizi saklayın: uygulamayı onunla değiştirirsiniz.',
    },
    {
      title: 'Daha fazlasını isteyin',
      text: 'Aynı kutuda: yeni bir şey, farklı bir şey ya da uygulamanın durumu; kimlerin kullandığı ve nelerin girildiği. Önceki sürümler saklanır, bu yüzden bir önceki yeniden yayına alınabilir.',
    },
  ],

  questionsTitle: 'Bilmekte fayda var',
  questions: (showcase) => [
    ['Kod yazmayı bilmem gerekir mi?', 'Hayır. Uygulamanın ne yapması gerektiğini anlatırsınız, ajan onu yazar. Koda hiç bakmanız gerekmez.'],
    ['Uygulamamı kimler açabilir?', 'Linkine sahip olan ve bu siteye ulaşabilen herkes. Linki, uygulamayı kullanacak kişilere gönderin.'],
    ['İnsanların girdiği bilgiler nerede tutulur?', 'Her uygulama kendi kayıtlarını, kuruluşunuzun işlettiği bu kurulumda tutar.'],
    [
      'Uygulamama nasıl geri dönerim?',
      'Bu tarayıcı, burada oluşturduğunuz uygulamaları oluşturduğunuz kutunun yanında hatırlar. Başka bir cihazda, uygulamayla birlikte aldığınız editör linkini açın; onu güvenli bir yerde saklayın, çünkü kimse onu geri getiremez.',
    ],
    [
      'Bir şey çalışmıyor. Şimdi ne yapmalıyım?',
      'Uygulamayı nasıl istediyseniz aynı şekilde ajana neyin yanlış olduğunu söyleyin. Bir değişiklik işleri kötüleştirdiyse, uygulamanın editöründen bir önceki sürümü yeniden yayına alın.',
    ],
    ['İş arkadaşlarım neler oluşturdu?', <>Göstermeyi seçtikleri uygulamalar {showcase('vitrinde')} yer alıyor.</>],
  ],
};
