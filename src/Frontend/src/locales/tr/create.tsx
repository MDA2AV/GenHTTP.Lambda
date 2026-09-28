import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Lambda oluşturun',
  whatTitle: 'Ne yapmak istersiniz?',
  whatText:
    'Size en yakın olanı seçin. Zaten çalışan bir şeyin kopyasıyla başlarsınız ve dilediğiniz gibi değiştirirsiniz. Ya da sıfırdan başlayın.',
  seeIt: 'Çalışırken görün',
  startFrom: 'Bununla başla',
  starters: {
    'demo-crud': {
      title: 'Bir şeyleri takip edin',
      description: 'İnsanların ekleyip değiştirebildiği ve tamamlandı diye işaretleyebildiği bir liste: görevler, notlar, yer imleri ya da küçük bir envanter.',
    },
    'demo-registration': {
      title: 'İnsanlar kaydolsun',
      description: 'İnsanların kaydolup giriş yaptığı hesaplar ve yalnızca onların görebildiği sayfalar.',
    },
    'demo-game': {
      title: 'Birlikte oynanan bir oyun',
      description: 'Birkaç kişinin aynı anda, tarayıcılarında canlı oynadığı bir şey.',
    },
    'demo-files': {
      title: 'Dosya ve fotoğraf paylaşın',
      description: 'İnsanlar fotoğraf ya da belge yükler, diğer herkes görebilir.',
    },
    'demo-live': {
      title: 'Olanları anında gösterin',
      description: 'Bir şey değiştiği anda kendiliğinden güncellenen bir sayfa: oylar, skorlar, bir gösterge paneli.',
    },
    empty: {
      title: 'Başka bir şey',
      description: 'Boş bir lambdayla başlayın, aklınızdakini yapın.',
    },
  },

  addressTitle: 'Bir adres verin',
  fromDemo: (title) => (
    <>{title}: lambdanız demonun bir kopyası olarak başlar ve içindeki her şeyi değiştirebilirsiniz.</>
  ),
  fromNothing: 'Lambdanız boş başlar, aklınızdaki her şeye hazır.',
  pickAgain: 'Başka bir şey seç',
  publicKey: 'Herkese açık anahtar',
  free: (key) => `“${key}” kullanılabilir.`,
  keyHint: 'Küçük harf, rakam ve tire. En az üç karakter. Rastgele bir anahtar için boş bırakın.',
  accept: 'Kullanım koşullarını kabul ediyorum',
  fullTerms: 'Kullanım koşullarının tamamını okuyun',
  back: 'Geri',
  creating: 'Oluşturuluyor…',
  submit: 'Lambda oluştur',
  keepLink: 'Sonraki ekranda editör linkinizi göreceksiniz. Geri dönmenin tek yolu bu, o yüzden saklayın.',
  failed: 'Lambda oluşturulamadı.',
};
