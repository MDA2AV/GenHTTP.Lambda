import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: '開発スペース',
  hint: 'バージョンのアセットのビルド元です。ツールチェーンでビルドするフロントエンドのプロジェクトで、ソース、設定、ロックファイルを含みます。バージョンごとに保存され、コンパイルも配信もされません。変更するのは、エージェントやクローンで作業する人です。作業する場所でビルドし、ビルドしたものと一緒に保存します。このプラットフォームは何もビルドしません。そのため、ここでは読むだけで、編集はできません。',
  overview: '概要',
  files: 'ファイル',
  scope: (version) =>
    `バージョン${version}のアセットのビルド元です。バージョンと一緒に保存され、コンパイルも配信もされません。ビルドするのは変更した人で、ここではビルドしません。`,
  scopeDraft: 'この下書きのアセットのビルド元です。下書きと一緒に保存され、コンパイルも配信もされません。ビルドするのは変更した人で、ここではビルドしません。',
  reading: '開発スペースを読み込み中…',
  readFailed: '開発スペースを読み込めませんでした。',

  emptyTitle: (version) => `バージョン${version}には開発スペースがありません`,
  emptyTitleDraft: 'この下書きには開発スペースがありません',
  emptyText: (code) => (
    <>
      フロントエンドをツールチェーン（ViteでビルドするReact、Vue、Svelte、TypeScript、Tailwindなど）でビルドする場合、そのプロジェクトがバージョンごとにここに保存されます。アセットのビルド元です。エージェントが作業する場所でビルドし、ソースをビルドしたものと一緒に保存します。クローンでは{code('dev/')}フォルダーです。普通のHTML、CSS、JavaScriptのフロントエンドには必要ありません。
    </>
  ),
  emptyHow: (code) => (
    <>クローンの{code('AGENTS.md')}に、コーディングエージェント向けの設定方法が書かれています。</>
  ),

  projects: 'プロジェクト',
  atTheTop: '開発スペースそのもの',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: 'ビルドに使うもの',
  build: 'ビルド',
  noBuild: 'package.jsonにビルドスクリプトがありません。',
  into: 'ビルドの出力先',
  intoAssets: (folder, files, size) => (
    <>
      アセットの{folder}：このバージョンでは{files}個のファイル、{size}
    </>
  ),
  intoNothing: (folder) => <>アセットの{folder}：このバージョンでは空です</>,
  packages: 'パッケージ',
  packagesCount: (runtime, tooling) => `実行用${runtime}個、ビルド用${tooling}個`,
  showPackages: '表示する',
  hidePackages: '隠す',
  runtime: '実行用',
  tooling: 'ビルド用',
  missing: (page, files) => (
    <>
      {page}は、アセットにない{files.length}個のファイル（{files.slice(0, 3).join('、')}{files.length > 3 ? '、…' : ''}）を参照しています。ビルドが書き出したものが完全には保存されなかったため、ページは読み込まれません。
    </>
  ),
  noLock: 'ロックファイルがありません。次のビルドでは、前回とは別のバージョンのパッケージがインストールされる可能性があります。',
  noIgnore: '.gitignoreがありません。ツールチェーンがインストールしたものやビルドしたものが、バージョンに入ってしまうことがあります。',

  inVersion: (version) => `バージョン${version}で`,
  inDraft: 'この下書きで',
  comparedWith: (version) => `バージョン${version}との比較`,
  first: 'これを持つ最初のバージョンです。',
  both: (here, assets) => `ここで${here}個、アセットで${assets}個のファイルが変わりました。`,
  hereOnly: (here) =>
    `ここで${here}個のファイルが変わりましたが、アセットは変わっていません。ビルドが不要な変更でなければ、訪問者には以前と同じものが表示されます。`,
  builtOnly: (folder) => (
    <>{folder}にビルドされるものは変わりましたが、ここは変わっていません。ビルドが書き出したものに加えた変更は、次のビルドで取り消されます。</>
  ),
  assetsOnly: 'ここでは何も変わっていません。',
  unchanged: 'ここでもアセットでも、何も変わっていません。',
  showChanges: '変更を表示',
  hideChanges: '変更を隠す',
  noChanges: 'ここでは何も変わっていません。',

  readme: 'ビルド方法',
  noReadme: (code) => (
    <>
      ビルド方法が書かれていません。開発スペースの先頭に{code('README.md')}を置き、コマンドと出力先を書いておくと、次のエージェントがそれをもとにビルドします。
    </>
  ),
  readOnly: '読み取り専用です。変更はビルドする場所で行います。',
  noFiles: 'ファイルがありません。',
};
