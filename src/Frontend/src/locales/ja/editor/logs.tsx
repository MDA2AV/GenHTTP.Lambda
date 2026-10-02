import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'ログを読み込めませんでした。',
  hint: (capturing) =>
    'リクエスト、lambdaの出力、エラーをリアルタイムで表示します。' +
    (capturing ? '' : 'この環境ではlambdaの出力を保存しないため、表示されるのはリクエストとエラーだけです。') +
    'ログはメモリ上にあり、ここにあるすべてのlambdaで共有しているため、さかのぼれるのは数分から数時間分です。再起動すると空になります。訪問者のIPアドレスは表示されません。',
  featureHint: (capturing) =>
    'この下書きのプレビューの応答、出力、エラーをリアルタイムで表示します。' +
    (capturing ? '' : 'この環境ではlambdaの出力を保存しないため、表示されるのはリクエストとエラーだけです。') +
    'lambda自体のログとは別で、そちらにプレビューが表示されることはありません。ログはメモリ上にあるため、さかのぼれるのは数分から数時間分です。',
  nothingPreview: 'まだ何もありません。下書きのプレビューを開くと、リクエストがここに表示されます。',
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
};
