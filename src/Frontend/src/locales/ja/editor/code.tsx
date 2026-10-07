import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'コード',
  version: (version) => `バージョン${version}`,
  edited: '（編集中）',
  loadFailed: 'そのバージョンを読み込めませんでした。',
  compiles: 'コンパイルできます。',
  notYet: 'まだコンパイルできません。',
  checkFailed: 'コードをチェックできませんでした。',
  saved: (version) => `バージョン${version}として保存しました。`,
  featureSaved: '下書きに保存しました。試すには、プレビューをデプロイしてください。',
  featureLoadFailed: '下書きを読み込めませんでした。',
  previewOnline: 'プレビューがオンラインになりました。',
  previewRefused: 'プレビューは変わりませんでした。下のコンパイラーのメッセージを確認してください。',
  isOnline: (version) => `バージョン${version}がオンラインになりました。`,
  notOnline: 'オンラインになりませんでした。下のコンパイラーのメッセージを確認してください。',
  failed: 'うまくいきませんでした。',
  unchanged: '前回の保存から変更はありません。',
  demo: 'デモなので、すべて読み取り専用です。変更するには、これをもとに自分のlambdaを作成してください。',
  hint: (b) => (
    <>
      1つのバージョンのファイルです。{b('コード')}はプログラムと、それに添えて保存されるすべてのものです。.csファイルはどのフォルダーにあってもコンパイルされ、それ以外のファイル（ドキュメント、テスト、フロントエンドのビルド元）はバージョンと一緒に保存されるだけで、コンパイルも配信もされません。{b('リソース')}（ページ、スクリプト、スタイル、画像、データベースのマイグレーション）は実行中に読み込まれて配信され、コードが配信するものは一般公開されます。保存すると新しいバージョンができ、オンラインのものは変わりません。先に変更を試したいときは、下書きを作ってください。Ctrl-Sで保存、F12で宣言へ移動します。
    </>
  ),
  hintFeature: (b) => (
    <>
      この下書きのファイルです。{b('コード')}（.csファイルはどのフォルダーにあってもコンパイルされ、残りはコードと一緒に保存されます）と、実行中に読み込まれて配信される{b('リソース')}があります。保存すると下書きに保存され、下書き専用のURLに表示されます。下書きを公開するまで、訪問者には何も見えません。
    </>
  ),
  inFeature: (name) => `下書き「${name}」`,
  changedElsewhere:
    'この下書きは、開いたあとにほかの場所で保存されています（エージェントかもしれません）。ここで保存する前に、保存されている内容を読み込んでください。ここでの変更で上書きすることはできません。',
  readAgain: '保存されている内容を読み込む',
  newer: (version) => `ここで開いているものより新しい、バージョン${version}があります。`,
  check: 'チェック',
  save: '保存',
  deploy: 'デプロイ',
  deployPreviewTitle: '保存して、試せるように下書き専用のURLでオンラインにする',
  binary: (size) => `テキストではないため、ここでは編集できません。サイズは${size}です。`,
  saveAndDeploy: '保存してデプロイ',
  saveVersion: '新しいバージョンとして保存',
  fromOlder: (version, newest) =>
    `これはバージョン${version}をもとにしていますが、バージョン${newest}のほうが新しいです。保存すると、バージョン${version}よりあとの変更を含まないまま、最新のバージョンになります。`,
  featureInstead: (start) => (
    <>
      試してみるだけなら、{start('代わりに新しい下書きに入れましょう')}
      。専用のURLで試せて、うまくいくまでバージョンは保存されません。
    </>
  ),
  cancel: 'キャンセル',
  what: '何を変更しましたか？（任意。履歴に表示されます）',
  placeholder: 'お問い合わせフォームを追加',
  goToDefinition: '定義へ移動',
  versionLabel: 'バージョン',
  shown: (version, online, newest) =>
    `バージョン${version}${online ? '、オンライン' : newest ? '、最新' : ''}`,
  optionOnline: '（オンライン）',
  switchUnsaved: '変更内容が保存されていません。それでも別のバージョンを開きますか？',
  noVersion: '表示できるバージョンはまだありません。',
  label: 'ファイル',
  codeGroup: 'コード',
  codeWhy: '配信されません。.csファイルはどのフォルダーにあってもコンパイルされ、それ以外はバージョンと一緒に保存されます。',
  resources: 'リソース',
  resourcesPublic: '公開：このバージョンがResourcesで配信します。',
  resourcesPrivate: 'バージョンには含まれますが、このバージョンは配信しません。',
  noResources: 'このバージョンにはありません。',
  count: (files) => (files === 1 ? '1個のファイル' : `${files}個のファイル`),
  groupUsage: (files, size) => `${files}、${size}`,
  usage: (used, of) => `このバージョンは、1バージョンあたりの上限${of}のうち${used}を使っています（コードとリソースの合計）。`,
  scope: (data) => (
    <>lambdaが実行中に保存するものはどのバージョンでも共通で、{data('データ')}にあります。</>
  ),
  download: 'ダウンロード',
  newIn: (group) => `${group}に新しいファイル`,
  uploadIn: (group) => `${group}にアップロード`,
  pick: 'ファイルを選ぶと、中身が表示されます。',
};
