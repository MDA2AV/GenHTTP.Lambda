import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `バージョンはプログラムそのもの、つまりコードとアセットです。一度保存すると変わらないので、どのバージョンとも比較でき、そのままの形でオンラインに戻せます。各バージョンには、依頼内容と変更点が記録されます。lambdaを変更するときは、下書きを作ってください。うまくいったら、次のバージョンになります。バージョンが${limit}個を超えると古いものから削除されますが、オンラインのものは削除されません。`,
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
  groups: {
    code: 'コード',
    assets: 'アセット',
    build: 'ビルド',
    context: 'ドキュメントとテスト',
  },
  browse: 'ファイルを見る',
  docs: 'ドキュメントを読む',
  build: 'ビルド元を見る',
  edit: 'ここから編集',
  feature: 'ここから下書きを作る',
  featureTitle: 'このバージョンの変更をlambdaとは別に進め、うまくいったら確定して次のバージョンにする',
  binary: 'テキストではないため、行単位では比較できません。',
  tooLarge: '大きすぎるため、行単位では比較できません。',
};
