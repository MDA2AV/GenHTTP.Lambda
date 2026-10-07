import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: '状態を確認中…',
  readDocs: 'ドキュメントを読む',
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
  inProgress: '作業中',
  allFeatures: 'すべての下書き',
  previewOnline: 'プレビューはオンラインです',
  previewOffline: 'プレビューはオフラインです',
  behind: '古くなっている',
  storage: 'ストレージ',
  versionAllowance: 'コードとリソース',
  data: 'データ',
};
