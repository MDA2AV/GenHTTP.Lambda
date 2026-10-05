import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'コード',
  version: (version) => `バージョン${version}`,
  edited: '（編集中）',
  online: '（オンライン）',
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
  edit: 'コードを手で編集します。保存すると新しいバージョンができ、オンラインのものはそのままです。デプロイすると、オンラインになります。先に変更を試したいときは、下書きを作ってください。',
  editFeature:
    'この下書きのコードです。保存しても下書きの中に残るだけで、lambdaの訪問者に配信されるものは何も変わりません。デプロイすると下書き専用のURLでオンラインになり、試せます。下書きを確定すると、次のバージョンになります。',
  inFeature: (name) => `下書き「${name}」`,
  changedElsewhere:
    'この下書きは、開いたあとにほかの場所で保存されています（エージェントかもしれません）。ここで保存する前に、保存されている内容を読み込んでください。ここでの変更で上書きすることはできません。',
  readAgain: '保存されている内容を読み込む',
  files: (entry, cs, context) => (
    <>
      {entry}が返すものが配信され、ほかの{cs}
      ファイルには型を書きます。それ以外のファイルはそのまま配信されます。ただし、{context}
      の中にあるもの（ドキュメント、テスト、開発スペース）は、コンパイルも配信もされません。Ctrl-Sで保存、F12で宣言へ移動します。
    </>
  ),
  newer: (version) => `ここで開いているものより新しい、バージョン${version}があります。`,
  built: (folder) =>
    `開発スペースのビルドが${folder}を書き出します。次のビルドで、ここでの変更は置き換えられます。代わりに、ビルド元のソースを変更してください。`,
  check: 'チェック',
  save: '保存',
  deploy: 'デプロイ',
  deployPreviewTitle: '保存して、試せるように下書き専用のURLでオンラインにする',
  binary: (size) => `テキストではないため、編集できません。このまま配信されます（${size} kB）。`,
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
};
