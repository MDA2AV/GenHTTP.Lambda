import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Şöyle bir uygulama istiyorum…',
  password: 'şifre',
  unlimited:
    'Bu model şifre gerektirir ve ardından süre sınırı olmadan çalışır: süre dolana kadar değil, uygulamanız bitene kadar devam eder.',
  ideas: [
    'üyelerin etkinliklere kaydolabildiği bir kulüp uygulaması',
    'kimse aynı yemeği getirmesin diye bir piknik listesi',
    'düğünümüz için bir anı defteri',
    'insanların oy verip sonuçları gördüğü bir anket',
    'haftalık bilgi yarışması gecemiz için bir skor tablosu',
    'arkadaşların iyi dileklerini yazdığı bir doğum günü sayfası',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Sizinkinden önce bir uygulama var, sonra sıra sizde.' : `Sizinkinden önce ${waiting} uygulama var.`,
  starting: 'Başlıyor…',
  online: 'Uygulamanız yayında',
  notOnline: 'Uygulamanız oluşturuldu, ancak yayına alınmadı.',
  open: 'Uygulamayı aç',
  steps: {
    guide: 'Hazırlanıyor',
    examples: 'Örneklere bakılıyor',
    create: 'Uygulamanız için bir adres seçiliyor',
    write: 'Uygulamanız yazılıyor',
    improve: 'Uygulamanız iyileştiriliyor',
    check: 'Hatalar kontrol ediliyor',
    online: 'Yayına alınıyor',
    trying: 'Deneniyor',
    looking: 'Uygulamanıza bakılıyor',
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
  further: 'Editör linkiniz',
  keep:
    'Bu linki güvenli bir yerde saklayın. Uygulamanıza başka bir cihazdan girmenin tek yolu bu ve kaybolursa biz de kurtaramayız.',
  copyLink: 'Editör linkini kopyala',
  lifetime: (offline, removed) =>
    `Kullanıldığı sürece yayında tutarız: ${offline} gün boyunca ziyaret veya değişiklik olmazsa yayından kaldırılır, ${removed} gün sonra da silinir. Yeniden yayına buradan ya da editöründen alabilirsiniz.`,

  failedToStart: 'İstek gönderilemedi.',
  noAnswer: 'Ajan işini bitirdi ama ne olduğunu söylemedi.',
  failed: 'Bir şeyler ters gitti.',
  profiles: {
    enterprise: {
      placeholder: 'Şöyle bir uygulamaya ihtiyacımız var…',
      ideas: [
        'izin talep etmek için, ekip lideri tarafından onaylanan bir form',
        'toplantı odalarımız için bir rezervasyon takvimi',
        'yeni çalışma arkadaşlarının işe alıştırılması için bir kontrol listesi',
        'ekipmanlarımızın ve kimde olduklarının envanteri',
        'açık destek taleplerinin ve her birinden sorumlu kişinin listesi',
        'eğitimlerimiz için bir geri bildirim formu',
      ],
    },
  },
};
