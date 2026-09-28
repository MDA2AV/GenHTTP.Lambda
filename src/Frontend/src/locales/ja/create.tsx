import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'lambdaを作成',
  whatTitle: '何を作りますか？',
  whatText:
    'いちばん近いものを選ぶと、すでに動いているアプリのコピーから始められます。中身は自由に変えてかまいません。ゼロから始めることもできます。',
  seeIt: 'デモを見る',
  startFrom: 'これで始める',
  starters: {
    'demo-crud': {
      title: 'リストで管理する',
      description: 'みんなで追加・編集・チェックできるリスト。タスク、メモ、ブックマーク、ちょっとした在庫管理に。',
    },
    'demo-registration': {
      title: 'ユーザー登録を受け付ける',
      description: 'ユーザーが登録してサインインできるアカウントと、本人だけが見られるページ。',
    },
    'demo-game': {
      title: 'みんなで遊べるゲーム',
      description: '何人もが同時に、それぞれのブラウザーでリアルタイムに遊べるもの。',
    },
    'demo-files': {
      title: 'ファイルや画像を共有する',
      description: '画像や資料をアップロードすると、ほかのみんなが見られます。',
    },
    'demo-live': {
      title: 'リアルタイムに表示する',
      description: '何か変わった瞬間に、自動で更新されるページ。投票、スコア、ダッシュボードなどに。',
    },
    empty: {
      title: 'それ以外',
      description: '空のlambdaから始めて、思いどおりのものを作れます。',
    },
  },

  addressTitle: 'URLを決める',
  fromDemo: (title) => <>{title}：デモのコピーからlambdaを始めます。中身はすべて自由に変えられます。</>,
  fromNothing: '空のlambdaから始めます。何でも好きなものを作れます。',
  pickAgain: '選び直す',
  publicKey: '公開キー',
  free: (key) => `「${key}」は使えます。`,
  keyHint: '英小文字、数字、ハイフンで3文字以上。空欄にするとランダムに決まります。',
  accept: '利用規約に同意します',
  fullTerms: '利用規約の全文を読む',
  back: '戻る',
  creating: '作成中…',
  submit: 'lambdaを作成',
  keepLink: '次の画面に編集用リンクが表示されます。lambdaに戻る唯一の方法なので、必ず保管してください。',
  failed: 'lambdaを作成できませんでした。',
};
