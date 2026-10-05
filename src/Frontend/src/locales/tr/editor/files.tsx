import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      Bir sürümün dosyaları, yani programın kendisi. {b('Kod')} derlenir ve asla sunulmaz. {b('Statik dosyalar')}{' '}
      (sayfalar, scriptler, stiller, görseller) kodla birlikte kaydedilir, onunla birlikte yayına alınır ve eski bir
      sürüme dönüldüğünde onunla birlikte geri gelir. Kod onları sunuyorsa herkese açıktır. Lambdanın çalışırken
      sakladıkları burada değil, {b('Veriler')} bölümündedir.
    </>
  ),
  scope: (version, data) => (
    <>
      Bunlar {version}. sürüme aittir ve onunla birlikte değişir. Lambdanın çalışırken sakladıkları her sürüm için
      aynıdır ve {data('Veriler')} bölümündedir.
    </>
  ),
  edit: 'Bu sürümü düzenle',
  version: 'Sürüm',
  shown: (version, online, newest) => `Sürüm ${version}${online ? ', yayında' : newest ? ', en yeni' : ''}`,
  optionOnline: ' (yayında)',
  readFailed: 'Bu sürüm okunamadı.',
  noVersion: 'Henüz gösterilecek bir sürüm yok.',
  label: 'Dosyalar',
  code: 'Kod',
  codeWhy: 'Lambdanın içine derlenir, asla sunulmaz.',
  count: (files) => `${files} dosya`,
  codeUsage: (files, used, of) => `${files}, ${used} / ${of} karakter`,
  usage: (files, used, of) => `${files}, ${used} / ${of}`,
  noCode: 'Bu sürümde kod yok.',
  assets: 'Statik dosyalar',
  assetsPublic: 'Herkese açık: bu sürüm onları Assets ile sunuyor.',
  assetsPrivate: 'Kodla birlikte kaydedildi ama bu sürüm onları sunmuyor.',
  noAssets: 'Bu sürümde yok.',
  context: 'Dokümantasyon ve testler',
  contextWhy: 'Asla derlenmez ve asla sunulmaz: bu sürüm hakkında, onu okuyan ya da değiştiren herkes için yazılanlar.',
  contextUsage: (files, size) => `${files}, ${size} - statik dosyalarla birlikte sayılır`,
  noContext: 'Bu sürüm hakkında henüz bir şey yazılmadı.',
  build: 'Derleme',
  buildWhy: 'Asla derlenmez ve asla sunulmaz: kodun veya statik dosyaların, onları değiştiren kişi tarafından neyden derlendiği.',
  data: 'Veriler',
  dataPublic: 'Herkese açık: yayındaki kod onları Workspace ile sunuyor.',
  dataPrivate: 'Yalnızca lambdaya özel. Hiçbir sürümün parçası değil.',
  uploadFailed: (path) => `${path} yüklenemedi.`,
  deleteFolder: (path, held) =>
    held > 0 ? `${path} klasörü ve içindeki ${held} dosya silinsin mi?` : `${path} klasörü silinsin mi?`,
  deleteFile: (path) => `${path} silinsin mi? Lambda onu artık bulamayacak.`,
  deleteFailed: 'Silinemedi.',
  full: 'Veri alanı dolu',
  uploadInto: (folder) => `${folder} klasörüne yükle`,
  upload: 'Yükle',
  reading: 'Okunuyor…',
  noData: 'Henüz bir şey yok. Lambdanın çalışırken kaydettikleri burada görünür.',
  delete: (path) => `Sil: ${path}`,
  deleteShort: 'Sil',
  fileFailed: 'Dosya okunamadı.',
  pick: 'İçinde ne olduğunu görmek için bir dosya seçin.',
  tooLarge: (name, size) => (
    <>
      {name} ({size}) burada gösterilemeyecek kadar büyük.
    </>
  ),
  download: 'İndir',
  readingFile: (name) => `${name} okunuyor…`,
  missing: (name) => `Bu sürümde ${name} adlı bir dosya yok.`,
  saved: 'kaydedildi',
  notText: 'Metin değil. İçine bakmak için indirin.',
};
