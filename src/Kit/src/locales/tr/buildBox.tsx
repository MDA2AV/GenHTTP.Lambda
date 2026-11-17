import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Şöyle bir web sitesi istiyorum…',
  password: 'şifre',
  fable:
    'Fable deneme sürecinde şifreyle korunuyor. Süre sınırı olmadan çalışır: süre dolunca değil, web siteniz bitince durur.',
  ideas: [
    'üyelerin etkinliklere kaydolabildiği bir kulüp sitesi',
    'kimse aynı yemeği getirmesin diye bir piknik listesi',
    'düğünümüz için bir anı defteri',
    'insanların oy verip sonuçları gördüğü bir anket',
    'haftalık bilgi yarışması gecemiz için bir skor tablosu',
    'arkadaşların iyi dileklerini yazdığı bir doğum günü sayfası',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Sizinkinden önce bir web sitesi var, sonra sıra sizde.' : `Sizinkinden önce ${waiting} web sitesi var.`,
  starting: 'Başlıyor…',
  online: 'Web siteniz yayında',
  notOnline: 'Web siteniz oluşturuldu, ancak yayına alınmadı.',
  open: 'Web sitesini aç',
  steps: {
    guide: 'Hazırlanıyor',
    examples: 'Örneklere bakılıyor',
    create: 'Web siteniz için bir adres seçiliyor',
    write: 'Web siteniz yazılıyor',
    improve: 'Web siteniz iyileştiriliyor',
    check: 'Hatalar kontrol ediliyor',
    online: 'Yayına alınıyor',
    trying: 'Deneniyor',
    looking: 'Web sitenize bakılıyor',
    forRecords: 'Kayıtlar için yer hazırlanıyor',
    forKeys: 'Anahtarlar ve parolalar için yer hazırlanıyor',
    forFiles: 'Kaydedecekleri için yer hazırlanıyor',
    records: 'Kayıtlara bakılıyor',
    keys: 'Hangi anahtar ve parolaların gerektiği kontrol ediliyor',
    addFile: 'Dosya ekleniyor',
    removeFile: 'Dosya kaldırılıyor',
    files: 'Kaydedilenlere bakılıyor',
    services: 'Başka hizmetleri kullanma izni isteniyor',
  },
  further: 'Daha sonra değiştirmek için',
  keep:
    'Bu linki saklayın. Uygulamanıza geri dönmenin tek yolu bu ve kaybolursa biz de kurtaramayız. Sekmeyi kapatmadan önce yer imlerine ekleyin.',
  copyLink: 'Editör linkini kopyala',
  lifetime: (offline, removed) =>
    `Kullanıldığı sürece yayında tutarız: ${offline} gün boyunca ziyaret veya değişiklik olmazsa yayından kaldırılır, ${removed} gün sonra da silinir. Yeniden yayına almak için editörü açın.`,
  openEditor: 'Editörü aç',

  failedToStart: 'İstek gönderilemedi.',
  noAnswer: 'Ajan işini bitirdi ama ne olduğunu söylemedi.',
  failed: 'Bir şeyler ters gitti.',
};
