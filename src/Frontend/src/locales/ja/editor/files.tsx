import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'バージョン',
  shown: (version, online, newest) => `バージョン${version}${online ? '（オンライン）' : newest ? '（最新）' : ''}`,
  optionOnline: '（オンライン）',
  count: (files) => `ファイル${files}個`,
  usage: (files, used, of) => `${files}、${used}／${of}`,
  dataPublic: '公開：オンラインのコードがWorkspaceで配信しています。',
  dataPrivate: 'lambdaだけが使える非公開のデータです。どのバージョンにも含まれません。',
  uploadFailed: (path) => `${path}をアップロードできませんでした。`,
  deleteFolder: (path, held) =>
    held > 0 ? `${path}と、中にあるファイル${held}個を削除しますか？` : `フォルダー${path}を削除しますか？`,
  deleteFile: (path) => `${path}を削除しますか？　lambdaからは参照できなくなります。`,
  deleteFailed: '削除できませんでした。',
  full: 'データの容量がいっぱいです',
  uploadInto: (folder) => `${folder}にアップロード`,
  upload: 'アップロード',
  reading: '読み込み中…',
  noData: 'まだ何もありません。lambdaが実行中に保存したものが、ここに表示されます。',
  delete: (path) => `${path}を削除`,
  deleteShort: '削除',
  fileFailed: 'ファイルを読み込めませんでした。',
  pick: 'ファイルを選ぶと、中身が表示されます。',
  tooLarge: (name, size) => (
    <>
      {name}は{size}あり、大きすぎてここには表示できません。
    </>
  ),
  download: 'ダウンロード',
  readingFile: (name) => `${name}を読み込み中…`,
  missing: (name) => `このバージョンには、${name}というファイルはありません。`,
  saved: '最終保存',
  notText: 'テキストではありません。中身を見るには、ダウンロードしてください。',
};
