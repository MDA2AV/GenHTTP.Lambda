import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'メインメニュー',
  build: 'サイトを作る',
  ship: '公開する',
  showcase: 'ショーケース',
  enterprise: 'エンタープライズ',
  docs: 'ドキュメント',
  admin: '管理',
  lightMode: 'ライトモードに切り替え',
  darkMode: 'ダークモードに切り替え',
  openMenu: 'メニューを開く',
  closeMenu: 'メニューを閉じる',
  language: '言語',
  terms: '利用規約',
  privacy: 'プライバシーポリシー',
  imprint: '運営者情報',
  writeCode: 'コードを自分で書く',
  contact: 'お問い合わせ',
};

export const common: Messages['common'] = {
  loading: '読み込み中…',
  loadingEditor: 'エディターを読み込み中…',
  editorFailed: 'エディターを読み込めませんでした',
  pageFailed: 'ページを読み込めませんでした',
  editorFailedWhy: 'このタブを開いている間にサイトが更新された可能性があります。',
  reload: 'ページを再読み込み',
  backToStart: 'トップに戻る',
  tryAgain: 'もう一度試す',
  copy: 'コピー',
  copied: 'コピーしました',
  copyToClipboard: 'クリップボードにコピー',
  openInNewTab: '新しいタブで開く',
  close: '閉じる',
  operatorCountry: 'ドイツ',
};

export const notFound: Messages['notFound'] = {
  title: 'ページが見つかりません',
  heading: 'このページは存在しません',
  text: 'リンクが古いか、リンク先のlambdaが削除された可能性があります。',
};

export const missing: Messages['missing'] = {
  title: 'ここでは何も動いていません',
  heading: 'ここでは何も動いていません',
  notDeployed: (key) => (
    <>
      {key}
      にlambdaはありますが、今はデプロイされていません。無料プランのデプロイは、使われている間はオンラインのままです。1か月間アクセスも編集もなければオフラインになります。編集用リンクを持っている人なら、もう一度オンラインにできます。
    </>
  ),
  unknown: (key) => (
    <>
      {key}
      にlambdaはありません。そのキーが最初から存在しないか、lambdaが削除された可能性があります。
    </>
  ),
  create: 'lambdaを作成',
};

export const abuse: Messages['abuse'] = {
  report: '不正を報告',
  title: 'lambdaを報告する',
  write: 'メールで報告',
  subject: '不正利用の報告',
  intro:
    'ここでは誰でもコードを公開できます。そのため、公開すべきでないものが置かれることもあります。ここにあるページが人をだまそうとしている、何かを攻撃している、権利のない素材を使っている。そんなときはお知らせください。削除します。',
  how: (mailbox, strong, path) => (
    <>
      {mailbox}
      宛てに、{strong('ページのURL')}（{path}
      のような形式です）と、何が問題かを一文添えて送ってください。スクリーンショットがあると助かります。アカウントは不要で、このサイトの利用者でなくてもかまいません。
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('その後の流れ：')}報告は人が直接確認します。{terms('利用規約')}
      に違反していれば、通常1日以内にそのlambdaをオフラインにします。公開した人の情報はお伝えできません。また、すべての報告に返信できるとは限りませんが、報告にはすべて目を通しています。
    </>
  ),
  danger:
    '誰かに差し迫った危険がある場合や、犯罪が起きている場合は、警察など、地域の関係機関にも連絡してください。私たちにできるのはページの削除までです。',
};
