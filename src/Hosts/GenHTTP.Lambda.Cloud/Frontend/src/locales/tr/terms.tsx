import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Kullanım koşulları',
  binding: (english) => (
    <>Bu çeviri yalnızca bilgilendirme amaçlıdır. Hukuken bağlayıcı olan {english('İngilizce metindir')}.</>
  ),
  intro:
    'Bu, bir şeyleri denemek için sunulan ücretsiz bir hizmettir. Tanımadığımız kişilerin yazdığı kodu ortak altyapıda çalıştırır. Bu da ancak herkes birkaç kurala uyarsa yürür.',
  sections: {
    forbiddenTitle: 'Buraya koyamayacaklarınız',
    forbidden: [
      'Kötü amaçlı yazılım, oltalama (phishing) ve kripto madencisi yasaktır. Burada ya da herhangi bir yerde diğer sistemlere saldıran, onları tarayan, trafiğe boğan ya da işleyişlerini başka bir şekilde bozan hiçbir şey koyamazsınız. Kimseyi taciz eden bir şey koyamazsınız. Yayınlama hakkınız olmayan hiçbir şeyi de koyamazsınız; başkalarının kodu, metinleri, görselleri ve markaları da buna dahildir.',
      'Bir lambdayı başkalarına ait kişisel verileri saklamak ya da iletmek için kullanmayın. Herkese açık bir adreste gizli hiçbir şey yoktur ve bu platform bu tür verileri güvende tutmanız için size hiçbir yol sunmaz.',
    ],
    actionTitle: 'Neler yapabiliriz',
    action:
      'Burada yayına alınan her şey, herhangi bir zamanda, haber verilmeden ve açıklama yapma zorunluluğu olmadan yayından kaldırılabilir ya da silinebilir. Pratikte bu şu durumlarda olur: bir şey yukarıdaki kuralları ihlal ettiğinde, herkesin paylaştığı makineyi tehdit ettiğinde ya da biri onu bildirdiğinde ve bildirim haklı çıktığında. Bir lambda ayrıca engellenebilir: yayından kalkmış olarak kalır, artık değiştirilemez ve değiştirildiği adresler tüm site tarafından iki hafta boyunca reddedilebilir.',
    lastingTitle: 'Ne kadar süre kalır',
    lasting: (hours, days) =>
      `Yayına alınan bir sürüm yaklaşık ${hours} saat erişilebilir kalır. Açmadığınız bir lambda, ona en son dokunduğunuz andan yaklaşık ${days} gün sonra kodunun tüm sürümleriyle birlikte silinir. Kaydetmek ya da yayına almak da dokunmak sayılır; yani üzerinde çalıştığınız her şey kalır. Buradaki hiçbir şey yedek değildir: önem verdiğiniz kodun bir kopyasını kendiniz saklayın.`,
    keyTitle: 'Editör linkiniz şifrenizdir',
    key: 'Editör linkine sahip olan herkes o lambdayı okuyabilir ve değiştirebilir. Arkasında bir hesap ya da şifre yoktur. Linki yayınlarsanız, lambdayı değiştirme imkânını da yayınlamış olursunuz. Kaybolan bir linki kurtarmanın hiçbir yolu yoktur.',
    warrantyTitle: 'Garanti yok',
    warranty:
      'Hizmet olduğu gibi sunulur. Çalışacağına, çalışmaya devam edeceğine ya da içine koyduğunuz herhangi bir şeyi saklayacağına dair hiçbir garanti yoktur. Her an yeniden başlatılabilir, değiştirilebilir ya da kapatılabilir. Sizin ya da başkaları için önemli olan hiçbir şeyi bunun üzerine kurmayın.',
    reportTitle: 'Sorun bildirmek',
    report: (mailbox, front) => (
      <>
        Burada barındırılan bir lambda yapmaması gereken bir şey yapıyorsa, adresini belirterek {mailbox} adresine
        yazın. Neleri eklemeniz gerektiğini {front('ana sayfada')} bulabilirsiniz.
      </>
    ),
  },
  change: 'Bu koşullar değişebilir. Geçerli olan, bu sayfadaki sürümdür.',
};
