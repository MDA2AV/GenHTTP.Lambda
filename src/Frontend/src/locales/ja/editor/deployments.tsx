import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
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
};
