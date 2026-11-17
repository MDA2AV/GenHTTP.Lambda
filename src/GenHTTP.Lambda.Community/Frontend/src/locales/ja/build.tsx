import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'アプリを説明して作る',
  intro:
    'この環境のビルドエージェントに、必要なものを伝えてください。エージェントがアプリを書き、ここで公開し、一覧に残します。あとから同じ方法で、質問したり変更したりできます。',
  other: (ship, create) => (
    <>
      または、{ship('お使いのエージェントを接続する')}か、{create('自分でコードを書く')}こともできます。
    </>
  ),
};
