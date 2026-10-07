import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'Sürüm',
  shown: (version, online, newest) => `Sürüm ${version}${online ? ', yayında' : newest ? ', en yeni' : ''}`,
  optionOnline: ' (yayında)',
  count: (files) => `${files} dosya`,
  usage: (files, used, of) => `${files}, ${used} / ${of}`,
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
