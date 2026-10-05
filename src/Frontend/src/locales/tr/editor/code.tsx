import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Kod',
  version: (version) => `sürüm ${version}`,
  edited: ', düzenlendi',
  online: ', yayında',
  loadFailed: 'Bu sürüm yüklenemedi.',
  compiles: 'Sorunsuz derleniyor.',
  notYet: 'Henüz derlenmiyor.',
  checkFailed: 'Kod kontrol edilemedi.',
  saved: (version) => `Sürüm ${version} olarak kaydedildi.`,
  featureSaved: 'Taslağa kaydedildi. Denemek için önizlemesini yayına alın.',
  featureLoadFailed: 'Taslak yüklenemedi.',
  previewOnline: 'Önizleme yayında.',
  previewRefused: 'Önizleme değişmedi. Derleyicinin ne dediğine aşağıdan bakın.',
  isOnline: (version) => `Sürüm ${version} yayında.`,
  notOnline: 'Yayına alınamadı. Derleyicinin ne dediğine aşağıdan bakın.',
  failed: 'Bir şeyler ters gitti.',
  unchanged: 'Son kayıttan beri hiçbir şey değişmedi.',
  demo: 'Bu bir demo, buradaki her şey salt okunur. Değiştirmek için bundan kendi lambdanızı oluşturun. ',
  edit: 'Kodu elle düzenleyin. Kaydetmek yeni bir sürüm oluşturur ve yayındakine dokunmaz; o sürüm, siz yayına aldığınızda yayına girer. Bir değişikliği önce denemek için bir taslak başlatın. ',
  editFeature:
    'Bu taslağın kodu. Kaydetmek onu taslakta tutar, lambdanın ziyaretçilerinin gördüğü hiçbir şey değişmez. Yayına aldığınızda, denemeniz için taslağın kendi adresinde yayına girer; taslağı birleştirmek onu bir sonraki sürüm yapar. ',
  inFeature: (name) => `“${name}” taslağında`,
  changedElsewhere: 'Siz açtıktan sonra taslak başka bir yerden kaydedildi, belki ajan tarafından. Burada kaydetmeden önce kaydedileni yükleyin; değişiklikleriniz onun üzerine kaydedilmez.',
  readAgain: 'Kaydedileni yükle',
  files: (entry, cs, context) => (
    <>
      {entry} sunulacak şeyi döndürür, diğer {cs} dosyaları türleri barındırır, geri kalan her dosya olduğu gibi
      sunulur - {context} içindekiler hariç: onlar dokümantasyon, testler ve neyden derlendiğidir, asla derlenmez ve
      sunulmaz. Ctrl-S kaydeder, F12 bir tanıma gider.
    </>
  ),
  newer: (version) => ` Sürüm ${version}, burada açık olandan daha yeni.`,
  check: 'Kontrol et',
  save: 'Kaydet',
  deploy: 'Yayına al',
  deployPreviewTitle: 'Kaydet ve denemek için taslağı kendi adresinde yayına al',
  binary: (size) => `Metin değil, düzenlenecek bir şey yok. Olduğu gibi sunulur. Boyutu ${size} kB.`,
  saveAndDeploy: 'Kaydet ve yayına al',
  saveVersion: 'Yeni sürüm kaydet',
  fromOlder: (version, newest) =>
    `Bu kod sürüm ${version} üzerine kurulu, ama sürüm ${newest} daha yeni. Kaydederseniz, sürüm ${version} sonrasında gelenler olmadan en yeni sürüm olur.`,
  featureInstead: (start) => (
    <>
      Bir şey mi deniyorsunuz? {start('Bunun yerine yeni bir taslağa koyun')}: kendi adresini alır ve hazır olana kadar
      hiçbir sürüm kaydedilmez.
    </>
  ),
  cancel: 'İptal',
  what: 'Ne değişiyor? İsteğe bağlı, geçmişte gösterilir.',
  placeholder: 'İletişim formu ekler',
  goToDefinition: 'Tanıma git',
};
