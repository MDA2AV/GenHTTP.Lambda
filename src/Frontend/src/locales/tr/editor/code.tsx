import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Kod',
  version: (version) => `sürüm ${version}`,
  edited: ', düzenlendi',
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
  demo: 'Bu bir demo, buradaki her şey salt okunur. Değiştirmek için bundan kendi lambdanızı oluşturun.',
  hint: (b) => (
    <>
      Bir sürümün dosyaları. {b('Kodu')}, programdır ve onunla birlikte saklanan her şeydir: .cs dosyaları, hangi
      klasörde olursa olsun derlenir; diğer her dosya - dokümantasyonu, testleri, bir ön yüzün neyden derlendiği -
      sürümle birlikte saklanır ve asla derlenmez ya da sunulmaz. {b('Kaynakları')} - sayfalar, scriptler, stiller,
      görseller, veritabanının migration’ları - sürüm çalışırken okunur ve sunulur; kod onları sunduğu yerde herkese
      açıktır. Kaydetmek yeni bir sürüm oluşturur ve yayındakine dokunmaz; önce bir değişikliği denemek için bir taslak
      başlatın. Ctrl-S kaydeder, F12 bir tanıma gider.
    </>
  ),
  hintFeature: (b) => (
    <>
      Bu taslağın dosyaları: {b('kodu')} - .cs dosyaları, hangi klasörde olursa olsun derlenir, gerisi onunla birlikte
      saklanır - ve sürüm çalışırken okunup sunulan {b('kaynakları')}. Kaydetmek onları taslakta tutar ve taslağın
      kendi adresinde gösterir; siz taslağı yayına alana kadar ziyaretçileriniz bunların hiçbirini görmez.
    </>
  ),
  inFeature: (name) => `“${name}” taslağında`,
  changedElsewhere: 'Siz açtıktan sonra taslak başka bir yerden kaydedildi, belki ajan tarafından. Burada kaydetmeden önce kaydedileni yükleyin; değişiklikleriniz onun üzerine kaydedilmez.',
  readAgain: 'Kaydedileni yükle',
  newer: (version) => `Sürüm ${version}, burada açık olandan daha yeni.`,
  check: 'Kontrol et',
  save: 'Kaydet',
  deploy: 'Yayına al',
  deployPreviewTitle: 'Kaydet ve denemek için taslağı kendi adresinde yayına al',
  binary: (size) => `Metin değil, burada düzenlenecek bir şey yok. Boyutu ${size}.`,
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
  versionLabel: 'Sürüm',
  shown: (version, online, newest) =>
    `Sürüm ${version}${online ? ', yayında' : newest ? ', en yeni' : ''}`,
  optionOnline: ' (yayında)',
  switchUnsaved: 'Burada yaptığınız değişiklik kaydedilmedi. Yine de diğer sürümü açmak istiyor musunuz?',
  noVersion: 'Gösterilecek sürüm henüz yok.',
  label: 'Dosyalar',
  codeGroup: 'Kod',
  codeWhy: 'Asla sunulmaz. .cs dosyaları, hangi klasörde olursa olsun derlenir; geri kalanı sürümle birlikte saklanır.',
  resources: 'Kaynaklar',
  resourcesPublic: 'Herkese açık: bu sürüm bunları Resources ile sunar.',
  resourcesPrivate: 'Sürümle birlikte gelir, ancak bu sürüm bunları sunmaz.',
  noResources: 'Bu sürümde yok.',
  count: (files) => (files === 1 ? '1 dosya' : `${files} dosya`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Bu sürüm, kodu ve kaynaklarıyla birlikte, bir sürümün sahip olabileceği ${of} içinden ${used} tutuyor.`,
  scope: (data) => (
    <>Lambdanın çalışırken sakladıkları her sürüm için aynıdır ve {data('Veriler')} altındadır.</>
  ),
  download: 'İndir',
  newIn: (group) => `${group} içinde yeni dosya`,
  uploadIn: (group) => `${group} içine yükle`,
  pick: 'İçindekileri görmek için bir dosya seçin.',
};
