import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Üstte bir C# dosyası: harf, rakam, tire ve alt çizgi; harfle başlamalı, .cs ile bitmeli, en fazla 40 karakter.',
  name: 'Harf, rakam ve - _ . + @ ( ) [ ] { } $ ~; klasörler eğik çizgiyle ayrılır, boşluk olmaz ve ad nokta ile bitmez.',
  taken: 'Dışa aktarılan ya da klonlanan bir lambdanın en üstünde bu adda bir dosya ya da klasör bulunur. Bir klasörün içine koyun ya da başka bir ad verin.',
  lambda: 'Bir lambda artık .lambda/ klasörü tutmaz: dokümantasyonu docs/ içinde, testleri tests/ içindedir.',
  assets: 'Bir lambdanın sunduğu şeyler artık kaynaklarındadır - onları oraya ekleyin.',
  resourceName: 'Harf, rakam, tire, alt çizgi ve nokta; eğik çizgiyle ayrılır, en fazla altı klasör derinliğinde - ve bir uzantı, böylece doğru türde sunulur.',
  exists: 'Bu adda bir dosya zaten var.',
  remove: (name) => `${name} kaldırılsın mı? İçeriği de silinir.`,
  removeFolder: (name, files) => `${name} ve içindeki ${files === 1 ? '1 dosya' : `${files} dosya`} kaldırılsın mı?`,
  there: (name) => `${name} zaten var.`,
  entry: 'Kod parçası: döndürdüğü şey sunulur',
  errors: 'hata içeriyor',
  removeFile: (name) => `Kaldır: ${name}`,
  removeTitle: 'Kaldır',
  codePlaceholder: 'Store.cs, docs/notes.md veya frontend/app.ts',
  resourcePlaceholder: 'web/index.html',
  upload: 'Dosya yükle',
};
