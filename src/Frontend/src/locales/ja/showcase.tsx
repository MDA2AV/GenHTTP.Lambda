import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'ショーケース',
  title: 'ここで生まれて、いま動いている',
  intro:
    '作った人が公開を選んだlambdaです。どれもオンラインなので、カードを開けば実物がそのまま動きます。最近よく使われているものから並んでいます。',
  counted: (total) => `${total}件のlambda`,
  failed: 'ショーケースを読み込めませんでした。',
  loadingMore: 'さらに読み込み中…',
  showMore: 'もっと見る',
  nothingTitle: 'まだ何も掲載されていません',
  nothing: (tab) => (
    <>
      ちゃんと動くものができましたか？　管理画面を開いて{tab('ショーケース')}
      を選び、タイトルと短い説明、画像を追加してください。オンラインの間、ここに表示されます。
    </>
  ),
  buildOne: '作ってみる',
  yoursTitle: '自分の作品も載せたい？',
  yours: (tab) => (
    <>
      lambdaの管理画面を開いて{tab('ショーケース')}
      を選ぶか、作ったエージェントに掲載を頼んでください。掲載できるのは編集用キーを持っている人だけで、いつでも取り下げられます。
    </>
  ),
  buildSomething: '作ってみる',
};

export const card: Messages['card'] = {
  noPicture: '画像はまだありません',
  title: 'タイトル',
  description: 'このアプリでできること',
  opens: (title, address) => `${title}：${address}を新しいタブで開きます`,
};
