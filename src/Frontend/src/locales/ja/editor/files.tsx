import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      1つのバージョンのファイル、つまりプログラムそのものです。{b('コード')}はコンパイルされるだけで、配信はされません。
      {b('アセット')}
      （ページ、スクリプト、スタイル、画像）はコードと一緒に保存され、一緒にデプロイ・ロールバックされます。コードが配信すれば公開されます。lambdaが実行中に保存するものはここにはありません。それは
      {b('データ')}にあります。
    </>
  ),
  scope: (version, data) => (
    <>
      これらはバージョン{version}
      のもので、バージョンと一緒に変わります。lambdaが実行中に保存するものはどのバージョンでも同じで、{data('データ')}
      にあります。
    </>
  ),
  edit: 'このバージョンを編集',
  version: 'バージョン',
  shown: (version, online, newest) => `バージョン${version}${online ? '（オンライン）' : newest ? '（最新）' : ''}`,
  optionOnline: '（オンライン）',
  readFailed: 'そのバージョンを読み込めませんでした。',
  noVersion: '表示できるバージョンはまだありません。',
  label: 'ファイル',
  code: 'コード',
  codeWhy: 'lambdaにコンパイルされるだけで、配信はされません。',
  count: (files) => `ファイル${files}個`,
  codeUsage: (files, used, of) => `${files}、${used}／${of}文字`,
  usage: (files, used, of) => `${files}、${used}／${of}`,
  noCode: 'このバージョンにはコードがありません。',
  assets: 'アセット',
  assetsPublic: '公開：このバージョンがAssetsで配信しています。',
  assetsPrivate: 'コードと一緒に保存されていますが、このバージョンでは配信されていません。',
  noAssets: 'このバージョンにはありません。',
  context: 'ドキュメントとテスト',
  contextWhy: 'コンパイルも配信もされません。このバージョンについて書かれたもので、読む人や変更する人のためのものです。',
  contextUsage: (files, size) => `${files}、${size}（アセットの容量に含む）`,
  noContext: 'このバージョンについては、まだ何も書かれていません。',
  development: '開発スペース',
  developmentWhy: 'コンパイルも配信もされません。アセットのビルド元で、変更した人がビルドします。',
  noDevelopment: 'なし。ツールチェーンでビルドするフロントエンドは、そのプロジェクトをここに保存します。',
  data: 'データ',
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
