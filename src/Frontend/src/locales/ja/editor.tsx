import type { EditorMessages } from '../en/editor';

/**
 * エディターの日本語テキスト。
 *
 * 日本語の文は単語の間にスペースを入れないため、JSX のテキストは途中で改行せず、
 * 改行するのは {…} の直前か直後だけにしています。文をつなぐ部分（先頭や末尾の
 * スペース）も、日本語では付けません。
 */
export const editor: EditorMessages = {
  shared: {
    units: { s: '秒', min: '分', h: '時間', d: '日' },
    amount: (value, unit) => `${value}${unit}`,
    pair: (larger, smaller) => `${larger}${smaller}`,
    never: 'なし',
    justNow: 'たった今',
    ago: (span) => `${span}前`,
    in: (span) => `${span}後`,
    origins: {
      agent: 'エージェント',
      template: 'テンプレート',
      admin: '運営者',
      system: 'プラットフォーム',
      api: 'API／エディター',
      unknown: '不明',
    },
    endings: {
      replaced: '新しいデプロイで置き換え',
      stopped: 'オフラインに変更',
      expired: '使われないため期限切れ',
      admin: '運営者がオフラインに変更',
      ended: '終了',
    },
    whatThisIs: '説明',
    byAgent: 'エージェントが作成',
    writtenByAgent: 'エージェントが書いたもの',
    more: 'その他',
    of: (used, total) => `${used}／${total}`,
    online: (version) => `オンライン · v${version}`,
    onlineTitle: (version) => `オンライン：バージョン${version}を配信中`,
    offline: 'オフライン',
    offlineTitle: 'オフライン：何も配信していません',
    premium:
      'プレミアム：独自ドメインで応答でき、コード・アセット・データの容量が大きく、アクセスが少なくてもオンラインのまま保持されます',
    demo: 'デモ：この環境がオンラインに保っている、読み取り専用のlambda',
    tier: (tier) => `${tier}プラン`,
    entrances: {
      title: 'アクセス経路',
      note: 'サーバーの起動以降。WebSocket接続を含みます。',
    },
    chart: {
      showChart: 'グラフを表示',
      showValues: '数値を表示',
      none: 'まだデータがありません。',
      time: '時刻',
    },
    diagnostics: {
      compiles: 'コードはコンパイルできます。',
      none: 'まだメッセージはありません。「チェック」か「デプロイ」を押すと、コードをコンパイルします。',
      line: (line) => `${line}行目`,
    },
  },

  frame: {
    title: 'エディター',
    sections: {
      overview: '概要',
      showcase: 'ショーケース',
      domain: 'ドメイン',
      files: 'ファイル',
      versions: 'バージョン',
      deployments: 'デプロイ履歴',
      stats: '統計',
      logs: 'ログ',
      code: 'コード',
    },
    sectionsLabel: 'セクション',
    loadFailed: 'このlambdaを読み込めませんでした。',
    online: (version) => `バージョン${version}がオンラインになりました。`,
    deployFailed: 'lambdaをデプロイできませんでした。',
    offline: 'オフラインにしました。コードはそのまま残っています。',
    offlineFailed: 'lambdaをオフラインにできませんでした。',
    leave: '保存していないコードの変更は失われます。このまま移動しますか？',
    nothingTitle: 'このリンクの先には何もありません',
    createNew: '新しいlambdaを作成',
    loading: 'lambdaを読み込み中…',
    moreActions: 'その他の操作',
    redeploy: (version) => `バージョン${version}を再デプロイ`,
    takeOffline: 'オフラインにする',
    copyLink: 'リンクをコピー',
    copyPrivate: '編集用リンクをコピー',
    privateLink: 'このリンクを知っている人は誰でも、lambdaを変更できます。他人に知られないようにしてください。',
    rename: 'URLを変更',
    download: '.NETプロジェクトとしてダウンロード',
    delete: 'このlambdaを削除',
    deploy: (version) => `バージョン${version}をデプロイ`,
    problems: '最近エラーが発生しています',
    demoTitle: 'この環境がオンラインに保っている、読み取り専用のデモです。',
    demo: (start) => (
      <>
        コード、履歴、保存されているデータ、ログを読むためのデモです。変更したいときは、{start('これをもとに自分のlambdaを作りましょう')}
        。
      </>
    ),
    keep: 'このリンクは必ず保管してください。このlambdaに戻る唯一の方法です。',
    gotIt: 'わかりました',
    rejected: (version) => `バージョン${version}はオンラインになりませんでした`,
    refused: 'デプロイが拒否されました',
    openCode: 'コードを開く',
    close: '閉じる',
    notCompiling: 'コンパイルできません。オンラインのバージョンは、そのまま動いています。',
    moved: (path) => `新しいURLは${path}です。`,
    deleteTitle: 'このlambdaを削除しますか？',
    cancel: 'キャンセル',
    deleteForGood: '完全に削除',
    deleteFailed: 'lambdaを削除できませんでした。',
    deleteText: (key) => (
      <>すべてのバージョン、ファイル、履歴、そしてURL（{key}）が削除されます。元に戻すことはできません。</>
    ),
    openInTab: '新しいタブで開く',
    open: (address) => `${address}を新しいタブで開く`,
    copyAddress: 'URLをコピー',
    renameFailed: 'URLを変更できませんでした。',
    moveIt: '変更する',
    renameText: '古いURLはすぐに使えなくなります。リンクしている場所があれば、更新してください。',
  },

  summary: {
    reading: '状態を確認中…',
    hint: (since, kept, retention, tier) =>
      `アクセス数は、サーバーの前回起動時（${since}）から集計しています。` +
      (kept
        ? `lambdaは使われている間はオンラインのままで、アクセスも変更もないまま${retention}日たつと削除されます。`
        : `このlambdaは${tier}プランなので、アクセスが少なくてもオンラインのまま保存されます。`),
    onlineFor: (duration, version) => (
      <>
        {duration('しばらく')}前からオンラインで、バージョン{version}を配信しています。
      </>
    ),
    offline: 'オフラインです。バージョンをデプロイするまで、何も配信されません。',
    nothing: 'まだ何も書かれていません。',
    requestsToday: '今日のリクエスト',
    lastHour: (count) => `直近1時間で${count}件`,
    hourly: '直近24時間の、1時間ごとのリクエスト数',
    failed: '失敗',
    failedTitle: (failed, rejected) =>
      `直近24時間で、サーバーエラー${failed}件、見つからないか拒否されたもの${rejected}件`,
    average: '平均応答時間',
    noneYet: 'まだなし',
    lastVisit: '最終アクセス',
    problems: '最近エラーが発生しています',
    openLog: 'ログを開く',
    latest: '最新の変更',
    allVersions: 'すべてのバージョン',
    noDescription: '説明なし',
    version: (version) => `バージョン${version}`,
    notOnline: 'まだオンラインではありません',
    wanted: '依頼内容',
    noVersions: 'まだバージョンはありません。',
    storage: 'ストレージ',
    browse: '一覧を見る',
    code: 'コード',
    codeWhy: 'C#はコンパイルされるだけで、配信はされません。',
    characters: '文字',
    assets: 'アセット',
    assetsPublic: '公開：コードが配信しています。',
    assetsPrivate: 'コードからは配信されていません。',
    data: 'データ',
    dataPublic: '公開：コードがワークスペースを配信しています。',
    dataPrivate: 'lambdaだけが使える非公開のデータです。',
  },

  files: {
    hint: (b) => (
      <>
        {b('コード')}はコンパイルされるだけで、配信はされません。{b('アセット')}
        （ページ、スタイル、画像）はバージョンごとに保存され、コードが配信すれば公開されます。{b('データ')}
        はlambdaが実行中に書き込むものです。どのバージョンにも含まれず、コードが配信する場合にだけ公開されます。
      </>
    ),
    edit: 'このバージョンを編集',
    version: 'バージョン',
    shown: (version, online, newest) => `バージョン${version}${online ? '（オンライン）' : newest ? '（最新）' : ''}`,
    optionOnline: '（オンライン）',
    readFailed: 'そのバージョンを読み込めませんでした。',
    dataFailed: 'データを読み込めませんでした。',
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
    data: 'データ',
    dataPublic: '公開：このバージョンがWorkspaceで配信しています。',
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
  },

  versions: {
    hint: (limit) =>
      `各バージョンには、書いた人が残していれば、依頼内容と変更点が記録されます。バージョンが${limit}個を超えると古いものから削除されますが、オンラインのものは削除されません。`,
    none: 'まだバージョンはありません。',
    noDescription: '説明なし',
    online: 'オンライン',
    putOnline: 'このバージョンをオンラインにする',
    rollBackTitle: 'この古いバージョンをオンラインに戻す',
    deploy: 'デプロイ',
    rollBack: 'ロールバック',
    readFailed: 'このバージョンを読み込めませんでした。',
    comparing: '比較中…',
    unchanged: '前のバージョンから変更はありません。',
    first: '最初のバージョンです。',
    status: { added: '追加', removed: '削除', changed: '変更', same: '変更なし' },
    browse: 'ファイルを見る',
    edit: 'ここから編集',
    binary: 'テキストではないため、行単位では比較できません。',
    tooLarge: '大きすぎるため、行単位では比較できません。',
  },

  deployments: {
    hint: (until) =>
      `デプロイは、使われている間はオンラインのままです${until ? `（誰も使わなければ${until}まで）` : ''}。再デプロイやアクセスがあるたびに、期限はリセットされます。`,
    takeOffline: 'オフラインにする',
    readFailed: '履歴を読み込めませんでした。',
    reading: '履歴を読み込み中…',
    none: 'まだ何もデプロイされていません。',
    noDescription: '説明なし',
    deployed: (when, by) => `${when}にデプロイ（${by}）`,
    duration: 'オンラインだった期間',
    online: 'オンライン',
    short: {
      replaced: '置き換え',
      stopped: 'オフライン化',
      expired: '期限切れ',
      admin: '運営者が停止',
      ended: '終了',
    },
    putBack: (version) => `バージョン${version}をオンラインに戻す`,
    timeline: '直近7日間にオンラインだったもの',
    block: (version, from, to) => `バージョン${version}、${from}〜${to ?? '現在'}`,
    weekAgo: '1週間前',
    now: '現在',
  },

  stats: {
    readFailed: '数値を読み込めませんでした。',
    range: '期間',
    lastHour: '直近1時間',
    lastDay: '直近24時間',
    hint: (since) =>
      `サーバーの前回起動時（${since}）から、メモリ上で集計しています。再起動すると、ゼロから数え直します。`,
    reading: '数値を読み込み中…',
    requests: 'リクエスト',
    websockets: (count) => `ほかにWebSocket接続${count}件`,
    failed: '失敗',
    serverErrors: (count) => `サーバーエラー${count}件`,
    rejected: '見つからないか拒否',
    average: '平均応答時間',
    sent: (amount) => `送信量${amount}`,
    nobody: (hour) => (hour ? '直近1時間はアクセスがありません。' : '直近24時間はアクセスがありません。'),
    requestsTitle: 'リクエスト',
    per: (hour) => (hour ? '1分ごと。' : '15分ごと。'),
    answered: '応答済み',
    rejectedSeries: '見つからないか拒否',
    failedSeries: '失敗',
    timeTitle: '応答時間',
    averagePer: (hour) => (hour ? '1分ごとの平均。' : '15分ごとの平均。'),
    averageSeries: '平均',
    mostAsked: 'よくアクセスされるパス',
    path: 'パス',
    requestsColumn: 'リクエスト',
    failedColumn: '失敗',
    averageColumn: '平均',
    since: 'サーバーの起動以降。',
  },

  logs: {
    readFailed: 'ログを読み込めませんでした。',
    hint: (capturing) =>
      'リクエスト、lambdaの出力、エラーをリアルタイムで表示します。' +
      (capturing ? '' : 'この環境ではlambdaの出力を保存しないため、表示されるのはリクエストとエラーだけです。') +
      'ログはメモリ上にあり、ここにあるすべてのlambdaで共有しているため、さかのぼれるのは数分から数時間分です。再起動すると空になります。訪問者のIPアドレスは表示されません。',
    search: '検索',
    searchLabel: 'ログを検索',
    resume: '新しい行をリアルタイムで表示',
    pause: '読んでいる間は、新しい行の追加を止める',
    paused: '一時停止中',
    live: 'ライブ',
    show: '表示',
    all: 'すべて',
    requests: 'リクエスト',
    output: '出力',
    problems: '問題',
    reading: 'ログを読み込み中…',
    noProblems: 'ログに残っている範囲では、問題は起きていません。',
    nothing: 'まだ何もありません。lambdaのURLを開くと、リクエストがここに表示されます。',
    noMatch: '一致するものはありません。',
    identical: (count) => `同じ行が${count}件`,
    at: (domain) => `、ドメイン：${domain}`,
    from: (country) => `、国：${country}`,
  },

  showcase: {
    loadFailed: 'ショーケースの情報を読み込めませんでした。',
    loading: '読み込み中…',
    title: 'タイトル',
    description: '説明',
    picture: '画像',
    updated: 'ショーケースの掲載内容を更新しました。',
    listed: 'ショーケースに掲載されました。',
    waiting: '保存しました。lambdaがオンラインになると、ショーケースに表示されます。',
    saveFailed: 'ショーケースの掲載内容を保存できませんでした。',
    removed: 'ショーケースから取り下げました。',
    removeFailed: 'ショーケースから取り下げられませんでした。',
    wrongType: 'PNG、JPEG、GIF、WebPのいずれの画像でもありません。',
    tooLarge: (size, limit) => `このファイルは${size}あります。画像は${limit}までです。`,
    unreadable: 'ファイルを読み込めませんでした。',
    hint: (tool) => (
      <>
        ショーケースには、作った人が公開を選んだlambdaが、最近よく使われているものから並びます。掲載や取り下げができるのは編集用キーを持っている人だけで、掲載されるのはオンラインの間だけです。エージェントも
        {tool}
        ツールで同じことができます。
      </>
    ),
    open: 'ショーケースを開く',
    switch: 'このlambdaをショーケースに掲載する',
    listedNow: '掲載中です。ショーケースを見た人は誰でも開けます。',
    notListed: '保存済みですが、lambdaがオフラインのため掲載されていません。もう一度デプロイすると、また表示されます。',
    off: 'オフです。これをオンにして保存するまで、このlambdaの情報はどこにも表示されません。',
    offline: 'lambdaがオフラインなので、デプロイされるまで掲載は保留になります。掲載されるのは、応答するlambdaだけです。',
    titleLabel: 'タイトル',
    titlePlaceholder: 'クイズ大会のスコアボード',
    descriptionLabel: '説明',
    descriptionPlaceholder: '各チームがスマホで回答を入力し、司会者が採点すると、会場全員のスコアボードが更新されます。',
    save: '変更を保存',
    add: 'ショーケースに掲載',
    takeOff: '取り下げる',
    needs: (missing) => `${missing.join('、')}がまだありません。`,
    tooLong: '長すぎる項目があります。',
    allSaved: 'すべて保存済みです。',
    preview: 'プレビュー',
    card: (address) => <>訪問者に表示されるカードです。クリックすると{address}が開きます。</>,
    confirm: 'ショーケースから取り下げますか？',
    keep: '掲載を続ける',
    confirmText: 'タイトル、説明、画像は削除されます。lambda自体はそのまま残ります。',
    pictureLabel: '画像',
    formats: (limit) => `PNG、JPEG、GIF、WebP（${limit}まで）`,
    notSaved: '未保存',
    replace: '新しい画像をここにドロップすると、置き換えられます。',
    drop: 'ここに画像をドロップ',
    advice: 'スクリーンショットか、使っている様子の短いGIFがおすすめです。比率は16:10がきれいに見えます。',
    another: '別の画像を選ぶ',
    choose: 'ファイルを選ぶ',
    keepSaved: '保存済みの画像のままにする',
    clear: 'クリア',
  },

  domain: {
    readFailed: 'ドメインを読み込めませんでした。',
    reaching: (domain) => `${domain}へのリクエストが、このlambdaに届くようになりました。`,
    saveFailed: 'ドメインを保存できませんでした。',
    removed: 'ドメインを削除しました。lambdaは通常のURLで引き続き応答します。',
    removeFailed: 'ドメインを削除できませんでした。',
    hint:
      'プレミアムのlambdaは、通常のURLに加えて、独自ドメインでも応答できます（ルート以下すべて）。ドメインをこのサーバーに向けてここに入力すれば、そのドメインへのリクエストがlambdaに届きます。',
    loading: '読み込み中…',
    example: 'your-domain.com',
    open: (domain) => `${domain}を開く`,
    label: '応答するドメイン',
    serving: (domain) => <>通常のURLに加えて、{domain}でも配信中です。</>,
    none: 'まだありません。shop.example.comのようなサブドメインも、example.comのようなドメイン全体も使えます。',
    change: '変更',
    use: 'このドメインを使う',
    remove: '削除',
    confirm: 'ドメインを削除しますか？',
    keep: '残す',
    confirmText: (domain) => (
      <>
        {domain}
        へのリクエストは、すぐにこのlambdaに届かなくなります。通常のURLはそのまま使え、ドメインのDNS設定も変わりません。
      </>
    ),
    point: 'ドメインをこのサーバーに向ける',
    check: '再確認',
    records:
      'ドメインのDNSを管理しているサービスで、次の2つのレコードを追加してください。IPv6で接続できなくてもよければ、AAAAレコードは省いてかまいません。',
    type: 'タイプ',
    name: '名前',
    value: '値',
    pointsHere: (domain) => <>{domain}はこのサーバーを向いています。</>,
    alsoElsewhere: (addresses) =>
      `ただし、このサーバー以外の${addresses}にも解決されます。そちらに送られた訪問者は、lambdaにたどり着けません。`,
    elsewhere: (addresses) => `現在は${addresses}に解決されます。まだこのサーバーを向いていません。`,
    wait: '変更が行き渡るまで、しばらくかかることがあります（最長で古いレコードのTTL分）。',
    cname: '代わりにCNAMEレコードを使う',
    cnameText: (target) => (
      <>
        サブドメインなら、代わりにCNAMEレコードで{target}
        を指すこともできます。そうすれば、このサーバーのIPアドレスが変わっても自動で追従します。ただし、欠点もあります：
      </>
    ),
    cnameRoot: (example) => (
      <>
        ドメイン全体（{example}
        そのもの）には使えません。どのドメインもルートに持っているレコードと同じ名前にCNAMEを置くことは、規格で認められていないためです。代わりにALIAS、ANAME、「フラット化」レコードが使えるプロバイダーもあります。
      </>
    ),
    cnameAlone: '同じ名前にほかのレコードを置けません。メール用のMXレコードも、認証用のTXTレコードも使えなくなります。',
    cnameLookup: '訪問者のリゾルバーが、到着までに名前解決を1回多く行います。',
    copy: 'コピー',
    copyValue: (value) => `${value}をコピー`,
  },

  code: {
    title: 'コード',
    version: (version) => `バージョン${version}`,
    edited: '（編集中）',
    online: '（オンライン）',
    loadFailed: 'そのバージョンを読み込めませんでした。',
    compiles: 'コンパイルできます。',
    notYet: 'まだコンパイルできません。',
    checkFailed: 'コードをチェックできませんでした。',
    saved: (version) => `バージョン${version}として保存しました。`,
    isOnline: (version) => `バージョン${version}がオンラインになりました。`,
    notOnline: 'オンラインになりませんでした。下のコンパイラーのメッセージを確認してください。',
    failed: 'うまくいきませんでした。',
    unchanged: '前回の保存から変更はありません。',
    demo: 'デモなので、すべて読み取り専用です。変更するには、これをもとに自分のlambdaを作成してください。',
    edit: 'コードを手で編集します。保存すると新しいバージョンができ、オンラインのものはそのままです。デプロイするとオンラインになります。',
    files: (entry, cs) => (
      <>
        {entry}が返すものが配信され、ほかの{cs}
        ファイルには型を書きます。それ以外のファイルはそのまま配信されます。Ctrl-Sで保存、F12で宣言へ移動します。
      </>
    ),
    newer: (version) => `ここで開いているものより新しい、バージョン${version}があります。`,
    check: 'チェック',
    save: '保存',
    deploy: 'デプロイ',
    binary: (size) => `テキストではないため、編集できません。このまま配信されます（${size} kB）。`,
    saveAndDeploy: '保存してデプロイ',
    saveVersion: '新しいバージョンとして保存',
    cancel: 'キャンセル',
    what: '何を変更しましたか？（任意。履歴に表示されます）',
    placeholder: 'お問い合わせフォームを追加',
    goToDefinition: '定義へ移動',
  },

  tabs: {
    codeName: '英数字、ハイフン、アンダースコアを使い、末尾は.cs',
    slashes: '先頭と末尾にはスラッシュを使えません。120文字未満にしてください。',
    deep: 'フォルダーは6階層までです。',
    characters: '英数字、ハイフン、アンダースコア、ドットを使い、スラッシュで区切ります。',
    extension: '正しい形式で配信できるよう、拡張子が必要です。',
    exists: '同じ名前のファイルがすでにあります。',
    remove: (name) => `${name}を削除しますか？　中身も削除されます。`,
    there: (name) => `${name}はすでにあります。`,
    entry: 'スニペット：これが返すものが配信されます',
    errors: 'エラーあり',
    removeFile: (name) => `${name}を削除`,
    removeTitle: 'このファイルを削除',
    placeholder: '例：Types.cs、site/index.html',
    newFile: '新しいファイル',
    uploadTitle: 'ファイルをアップロード（画像、フォント、ページなど）',
    upload: 'ファイルをアップロード',
  },
};
