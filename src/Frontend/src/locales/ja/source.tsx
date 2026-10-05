import type { SourceMessages } from '../en/source';

/**
 * 公開されたソースコードのページ（/source とその下の各プロジェクト）の日本語テキスト。
 *
 * 日本語の文は単語の間にスペースを入れないため、JSX のテキストは途中で改行せず、
 * 改行するのは {…} の直前か直後だけにしています。
 */
export const source: SourceMessages = {
  shell: {
    section: 'オープンソース',
    home: 'GenHTTP Lambdaのトップページ',
  },

  lambda: {
    label: 'lambdaとは？',
    text: 'GenHTTP Lambdaで動くWebアプリです。作りたいものを伝えるとAIエージェントがC#で書き、数分で専用のURLで公開されます。すべてのバージョンが、何を変えたかと一緒に残ります。',
    build: '自分でも作ってみる',
  },

  catalog: {
    eyebrow: 'オープンソース',
    title: 'ここで生まれたアプリが、どう作られているか',
    intro:
      '作った人がコードを公開したlambdaです。すべてのバージョンと、それぞれの変更内容、ドキュメント、テストを読めます。ここで読むことも、.NETが動く場所ならどこでも動くプロジェクトとしてダウンロードすることもできます。',
    searchLabel: 'プロジェクトを検索',
    searchPlaceholder: '名前や、できることで検索',
    orderLabel: '並び順',
    orders: {
      stars: 'スターが多い順',
      updated: '最近変更された順',
      published: '新しく公開された順',
    },
    counted: (total) => `${total}件のプロジェクト`,
    failed: 'プロジェクトを読み込めませんでした。',
    loadingMore: 'さらに読み込み中…',
    showMore: 'もっと見る',
    nothingTitle: 'まだ何も公開されていません',
    nothing: (tab) => (
      <>
        ほかの人の参考になりそうなものを作りましたか？　管理画面を開いて{tab('オープンソース')}
        を選び、ライセンスを決めると、コードがここに表示されます。
      </>
    ),
    noMatchTitle: '一致するものはありません',
    noMatch: (query) => `「${query}」に一致する公開プロジェクトはありません。`,
    clear: 'すべてのプロジェクトを表示',
    yoursTitle: '自分のアプリも公開したい？',
    yours: (tab) => (
      <>
        lambdaの管理画面を開いて{tab('オープンソース')}
        を選ぶか、作ったエージェントに公開を頼んでください。公開できるのは編集用キーを持っている人だけで、ライセンスもその人が選びます。アプリが残しているもの（記録、ファイル、キー）は決して含まれません。
      </>
    ),
    build: '作ってみる',
    online: 'オンライン',
    offline: 'オフライン',
    changed: (ago) => `${ago}に変更`,
    stars: (count) => `スター${count}個`,
  },

  project: {
    loading: 'ソースコードを読み込み中…',
    failed: 'ソースコードを読み込めませんでした。',
    missingTitle: 'ここに公開されているソースコードはありません',
    missing: '持ち主が公開をやめたか、このURLにはもともとlambdaがなかった可能性があります。',
    all: 'すべてのプロジェクト',
    by: (name) => `作者：${name}`,
    versions: (count) => `${count}個のバージョン`,
    onlineAt: (address) => <>オンライン：{address}</>,
    offline: '現在はオフライン',
    openApp: 'アプリを開く',
    opens: (address) => `${address}を新しいタブで開きます`,
    published: (ago) => `${ago}に公開`,
    changed: (ago) => `${ago}に変更`,
    picture: (name) => `${name}の画面`,
    tabsLabel: '読む内容',
    tabs: {
      code: 'コード',
      docs: 'ドキュメント',
      tests: 'テスト',
      changes: '変更履歴',
    },
  },

  versions: {
    label: 'バージョン',
    choose: '別のバージョンを読む',
    newest: '最新',
    online: 'オンライン',
    older: (version, ago, newest) =>
      `読んでいるのは、${ago}に保存されたバージョン${version}です。最新はバージョン${newest}です。`,
    toNewest: '最新を読む',
    noChange: '変更内容のメモなし',
  },

  star: {
    star: 'スター',
    add: 'このプロジェクトにスターを付ける',
    remove: 'スターを取り消す',
    count: (count) => `スター${count}個`,
    failed: 'スターを保存できませんでした。',
  },
  clone: {
    button: 'コード',
    title: 'gitでクローンする',
    what: (oldest, newest) =>
      oldest === newest
        ? `バージョンはmainのコミットで、v${newest}のタグが付いています。`
        : `すべてのバージョンがmainのコミットとして入っていて、v${oldest}からv${newest}までタグが付いています。mainが最新です。`,
    readOnly:
      '読み取り専用です。これをもとに作るには、自分のlambdaを作成し、ここにあるファイルを移してください。方法はクローン内のAGENTS.mdに、できることはライセンスに書いてあります。',
  },

  download: {
    title: (version) => `バージョン${version}のプロジェクト`,
    what:
      'Dockerfile、ドキュメント、テスト、ライセンスの入った.NET 10のプロジェクトです。アプリが残しているもの（記録、保存したファイル、キー）は含まれません。',
    zip: 'ZIPをダウンロード',
    preparing: 'プロジェクトを準備中…',
    slow: 'バージョンを初めてダウンロードするときは、その場で準備するので少しお待ちください。',
    failed: 'プロジェクトを準備できませんでした。少し待ってから、もう一度お試しください。',
    run: '実行方法',
    local: '.NET 10 SDKで：',
    container: 'またはコンテナーで：',
    agent: 'または、フォルダーをお使いのコーディングエージェントに渡して、その上に作り足すこともできます。ライセンスは守ってください。',
    copy: 'コピー',
    copied: 'コピーしました',
  },

  tree: {
    label: 'ファイル',
    files: (count) => `ファイル${count}個`,
    packing: 'このバージョンを準備中…',
    packingSlow: 'バージョンは、誰かが初めて読むときに準備されます。大きなものは少し時間がかかります。',
    failed: 'このバージョンのファイルを読み込めませんでした。',
    legend: '凡例',
    kinds: {
      code: 'lambda自体のコード',
      asset: '配信するもの：ページ、スクリプト、スタイル、画像。データベースのマイグレーションもここにあります',
      docs: 'アプリが何で、なぜこのように作られているのか',
      tests: 'テストの方法',
      build: 'ビルドツールでコードやアセットを作る際の作成元',
      platform: 'プラットフォームの代わりになるもの',
      project: 'ホスト、ビルド、コンテナー、ライセンス',
    },
    short: {
      code: 'コード',
      asset: '配信',
      docs: 'ドキュメント',
      tests: 'テスト',
      build: 'ビルド',
      platform: 'プラットフォーム',
      project: 'プロジェクト',
    },
  },

  file: {
    loading: '読み込み中…',
    failed: 'このファイルを読み込めませんでした。',
    missing: (path) => `このバージョンには${path}はありません。`,
    binary: 'このファイルはテキストではありません。',
    tooLarge: 'このファイルは長すぎて、ここには表示できません。',
    download: 'ダウンロード',
    raw: '元のファイル',
    rawTitle: 'ファイルをそのまま開く',
    copy: 'コピー',
    copied: 'コピーしました',
    lines: (count) => `${count}行`,
    plain: '長いため、色分けせずに表示しています。',
    line: (line) => `${line}行目`,
  },

  docs: {
    pages: 'ページ',
    product: 'アプリについて',
    decisions: '設計判断',
    loading: '読み込み中…',
    failed: 'このページを読み込めませんでした。',
    noneTitle: 'このバージョンについては、何も書かれていません',
    none: 'ドキュメントはdocs/に入ります。アプリが何で、誰のためのもので、なぜこのように作られているのかを書く場所です。',
  },

  tests: {
    files: 'スクリプトとデータ',
    noneTitle: 'このバージョンには、テストについての記述がありません',
    none: 'テストの方法はtests/README.mdに書かれ、実行するスクリプトはその隣に置かれます。',
  },

  changes: {
    title: 'すべてのバージョン（新しい順）',
    intro: 'バージョンは、一度保存すると変わりません。どのバージョンにも、何を変えたかが1行で書かれています。',
    agent: 'エージェントが作成',
    online: 'オンライン',
    browse: 'コードを読む',
    noChange: 'メモなし',
  },

  licenses: {
    MIT: 'ライセンスと著作権表示を残す限り、誰でも、どんな用途にでも、使う・変える・配布することができます。',
    'Apache-2.0': 'MITと同様で、さらに貢献したすべての人から特許の利用許諾が得られます。変更した箇所には、変更したことを明記します。',
    'BSD-3-Clause': 'MITと同様で、さらにこれをもとに作ったものの宣伝に、作者の名前を使うことはできません。',
    'MPL-2.0': 'これらのファイルへの変更は、同じライセンスのままにします。ほかのどんなライセンスのコードとも組み合わせられます。',
    'GPL-3.0-or-later': '変更したかどうかにかかわらず、配布する人は、ソースコードも同じライセンスで一緒に提供します。',
    'AGPL-3.0-or-later': 'GPLと同様で、さらに変更したものをネットワーク越しに人に提供することも、配布とみなされます。',
    Unlicense: 'パブリックドメインとして提供されています。誰でも、条件なしに何でもできます。',
  },

  kinds: {
    Permissive: 'パーミッシブ',
    Copyleft: 'コピーレフト',
    PublicDomain: 'パブリックドメイン',
  },
};
