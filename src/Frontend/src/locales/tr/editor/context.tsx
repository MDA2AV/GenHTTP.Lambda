import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Dokümantasyon',
    titleSimple: 'Uygulamanız hakkında',
    hint: 'Bu uygulamanın ne olduğu, kimin için ve neden var olduğu - ve neden bu şekilde yapıldığı. Ajanlar bunu her değişiklikte yazar ve her sürümle birlikte saklanır; böylece eski bir sürüme dönüldüğünde, o sürüm için geçerli olan dokümantasyon da onunla birlikte geri gelir.',
    hintSimple: 'Uygulamanızın ne için olduğu ve nedeni, ajanın isteklerinizden anladığı şekliyle. Ajan bunu her değişiklikte günceller.',
    inDraft: 'Bu taslağın dokümantasyonu. Taslak yayına alındığında uygulamanızın dokümantasyonu olur.',
    pages: { product: 'Ürün', decisions: 'Kararlar' },
    emptyTitle: 'Henüz bir şey yazılmadı',
    emptyText: (code) => (
      <>
        Ajanlar dokümantasyonu değişiklikleriyle birlikte yazar: uygulamanın ne olduğunu, kimin için ve neden var
        olduğunu {code('.lambda/docs/product.md')} dosyasına, neden bu şekilde yapıldığını da {code('decisions.md')}{' '}
        dosyasına. Dokümantasyon, kodun yanında sürümün bir parçasıdır.
      </>
    ),
    emptySimpleTitle: 'Uygulamanız hakkında henüz bir şey yazılmadı',
    emptySimple: 'Ajan, isteklerinizden yola çıkarak uygulamanızın ne için olduğunu ve nedenini anlatabilir. Bu açıklamayı o andan itibaren güncel tutar.',
    ask: 'Ajandan yazmasını iste',
    describe: 'Ajandan anlatmasını isteyin',
    writePrompt: 'Bu uygulamanın dokümantasyonunu yaz: ne olduğunu, kimin için ve neden var olduğunu ve arkasındaki teknik kararları.',
    describePrompt: 'Bu uygulamanın ne için olduğunu ve nedenini, Hakkında bölümünde okumam için anlat.',
    decisionsPrompt: 'Bu uygulamanın arkasındaki teknik kararları ve neden alındıklarını yaz.',
    missingProduct: 'Henüz ürün sayfası yok',
    missingProductText: 'Uygulamanın ne olduğu, kimin için olduğu, insanların onunla ne yaptığı ve nedeni - onu isteyen kişinin kendi sözleriyle.',
    missingDecisions: 'Henüz yazılmış bir karar yok',
    missingDecisionsText: 'Uygulamanın nasıl yapıldığı ve nedeni: verilerini nasıl sakladığı, neye bağlı olduğu, neyin dışarıda bırakıldığı. Onu bir sonraki değiştirecek kişinin bilmesi gerekenler.',
    correctText: 'Ajan bunu isteklerinizden yola çıkarak yazar ve her değişiklikte günceller. Yanlış ya da eksik bir şey mi var? Ajana söyleyin.',
    correct: 'Ajana söyleyin',
    correctPrompt: 'Uygulamanın açıklamasını düzelt: ',
    placeholder: 'Kayıtların neden bir yıl saklandığını açıklar',
  },
  tests: {
    title: 'Testler',
    hint: 'Bu uygulamanın otomatik olarak nasıl test edildiği, testlerin kullandığı scriptler ve veriler. Ajanlar bunu güncel tutar ve bir değişikliği bitmiş saymadan önce çalıştırır. Her sürümle birlikte saklanır.',
    inDraft: 'Bu taslağın testleri. Taslak yayına alındığında uygulamanızın testleri olur - önce onları taslağın önizlemesi üzerinde çalıştırın.',
    pages: { testing: 'Nasıl test edildiği' },
    emptyTitle: 'Henüz test yok',
    emptyText: (code) => (
      <>
        Uygulamanın nasıl test edildiğini (neyin çalışmaya devam etmesi gerektiğini, bunun nasıl kontrol edileceğini ve
        scriptlerin nasıl çalıştırılacağını) ajanlar {code('.lambda/tests/README.md')} dosyasına yazar; scriptler ve
        test verileri de onun yanında durur.
      </>
    ),
    ask: 'Ajandan test yazmasını iste',
    writePrompt: 'Bu uygulamanın testlerini yaz: neyin çalışmaya devam etmesi gerektiğini ve bunun otomatik olarak nasıl kontrol edileceğini, önizlemesi üzerinde çalıştırılacak bir scriptle birlikte.',
    missing: 'Nasıl test edildiği henüz yazılmadı',
    missingText: 'Neyin çalışmaya devam etmesi gerektiği, her birinin nasıl kontrol edildiği ve yanındaki scriptlerin nasıl çalıştırılacağı.',
    placeholder: 'Dolu bir listenin yeni kayıtları reddettiğini kontrol eder',
  },
  files: 'Dosyalar',
  noFiles: 'Sayfaların yanında dosya yok.',
  none: 'yok',
  missingPill: 'Henüz yazılmadı',
  changedIn: (version) => `Sürüm ${version} içinde değişti`,
  changedInDraft: 'Bu taslakta değişti',
  showChanges: 'Neyin değiştiğini göster',
  hideChanges: 'Neyin değiştiğini gizle',
  noChanges: 'Hiçbir şey değişmedi.',
  edit: 'Düzenle',
  olderVersion: 'Bir sürüm asla değişmez: bir sayfa en yeni sürümde ya da bir taslakta düzenlenir.',
  writeIt: 'Kendiniz yazın',
  askPage: 'Ajandan yazmasını iste',
  editInCode: 'Kodda aç',
  cancel: 'İptal',
  save: 'Kaydet',
  write: 'Yaz',
  preview: 'Önizleme',
  writeOrPreview: 'Yaz ya da önizle',
  discard: 'Bu sayfada yaptığınız değişiklikler kaybolacak. Atılsın mı?',
  reading: 'Okunuyor…',
  readFailed: 'Bu okunamadı.',
  saveFailed: 'Kaydedilemedi.',
  savedDraft: 'Taslağa kaydedildi.',
  savedVersion: (version) => `Sürüm ${version} olarak kaydedildi.`,
  savedOnline: (version) => `Sürüm ${version} olarak kaydedildi ve yayında.`,
  savedNotOnline: (version) => `Sürüm ${version} olarak kaydedildi, ancak yayına alınamadı.`,
  saveTitle: 'Yeni sürüm olarak kaydet',
  saveText: (newest) =>
    `Bir sürüm asla değişmez, bu yüzden bu sayfa bir sonraki sürüm olarak kaydedilir: sürüm ${newest} üzerine, diğer her şey olduğu gibi kalarak.`,
  clash: (version) => `Siz başladıktan sonra sürüm ${version} kaydedildi ve o da bu sayfayı değiştirdi. Kaydetmek, o değişikliğin yerine geçer.`,
  alsoOnline: 'Yayına da al',
  alsoOnlineNote: 'Yalnızca dokümantasyon değişir, yani ziyaretçiler yeni bir şey görmez - ama yayındaki sürüm en yeni sürüm olarak kalır.',
  skeleton: {
    product: '# Uygulamanın adı\n\nNe olduğu, bir iki cümleyle.\n\n## Kimin için\n\n## İnsanlar onunla ne yapıyor\n\n## Özellikler ve neden var oldukları\n\n## Neleri yapmıyor\n',
    decisions: '# Kararlar\n\n## Bir karar\n\nNe kararlaştırıldı, neden ve bir değişikliğin neyi göz önünde bulundurması gerekiyor.\n',
    testing: '# Nasıl test edildiği\n\nTestlerin nasıl ve hangi adrese karşı çalıştırılacağı.\n\n## Neyin çalışmaya devam etmesi gerekiyor\n\n| Davranış | İstek | Beklenen |\n|---|---|---|\n| | | |\n',
  },
};
