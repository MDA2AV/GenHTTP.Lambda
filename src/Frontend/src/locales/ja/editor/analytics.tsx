import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  /** Next to the section's title, on hover. */
  hint: 'プラットフォームが代わりに数えています。Cookieも、アプリへのコードの追加も、訪問者の同意も必要ありません。',
  loading: '訪問数を読み込んでいます…',
  loadFailed: '訪問数を読み込めませんでした。',

  /** The page shown while analytics are off. */
  offTitle: 'アプリを訪れた人を確認しましょう',
  offText:
    '毎日どれくらいの人が訪れるか、どのページを開くか、どのサイトから来るか、どの国から、どのデバイスで来るか。オンにした時点から、日ごとに集計します。',
  offPrivate: 'Cookieも、訪問者が同意する必要のあるバナーもありません',
  offAddresses: 'IPアドレスは保存せず、日ごとの訪問数だけを保存します',
  offKept: (days) =>
    days >= 365
      ? '1年間保存され、そのすべてを表示します'
      : `1年間保存されます。表示するのは直近${days}日間で、すべてを見るにはプレミアムが必要です`,
  switchOn: 'アクセス解析をオンにする',
  switchedOn: 'アクセス解析をオンにしました。最初の訪問は数分以内に表示されます。',
  switchOnFailed: 'アクセス解析をオンにできませんでした。',
  demo: 'デモの訪問者は数えません。ご自身のコピーなら訪問者を数えられます。',

  range: '期間',
  days: (days) => (days >= 365 ? '12か月' : `${days}日間`),
  /** On a range the tier does not show, on hover. */
  premiumRange: 'プレミアムなら最大1年分を表示します',

  /** The figures at the top; each one picks what the chart below shows. */
  visitors: '訪問者',
  visitorsHint: 'ユニークな訪問者数です。同じ人は1日に1回だけ数えます。',
  views: 'ページビュー',
  viewsHint: 'ページが開かれた回数です。',
  hits: 'リクエスト',
  hitsHint: 'アプリに寄せられたすべてのリクエストです。ページに加え、ページが読み込む画像やスクリプトも含みます。',
  /** Under a figure: how it changed against the days before the ones shown. */
  versus: (days) => `その前の${days}日間と比較`,
  noChange: '変化なし',
  /** Under a figure when there is nothing to compare with. */
  inDays: (days) => (days >= 365 ? '直近12か月' : `直近${days}日間`),

  /** Above the chart, for the figure picked. */
  perDay: {
    visitors: '1日あたりの訪問者',
    views: '1日あたりのページビュー',
    hits: '1日あたりのリクエスト',
  },
  perWeek: {
    visitors: '1週間あたりの訪問者',
    views: '1週間あたりのページビュー',
    hits: '1週間あたりのリクエスト',
  },
  showValues: '数値を表示',
  showChart: 'グラフを表示',
  day: '日',
  week: '週の開始日',

  nobody: 'この期間の訪問者はいません。',
  nobodyYet: 'アクセス解析をオンにしてから、まだ訪問者はいません。最初の訪問は数分以内に表示されます。',

  pages: 'ページ',
  pagesHint: '開かれたページを、多い順に表示します。',
  requested: 'リクエストの多いもの',
  requestedHint: 'アプリに寄せられたすべてのリクエストを、パスごとに表示します。人が開くページはありません。',
  sources: '訪問元',
  sourcesHint: '訪問者を送ってきたサイトを、元のページごとに表示します。検索エンジンについては、検索エンジンから来たことだけがわかり、検索語はわかりません。',
  direct: '直接入力、ブックマーク、またはアプリ',
  countries: '国',
  countriesHint: '訪問者のIPアドレスが登録されている国です。多くの場合、実際にいる国と一致しますが、そうとは限りません。',
  unknownCountry: '不明',
  devices: 'デバイス',
  device: {
    desktop: 'パソコン',
    mobile: 'スマートフォン',
    tablet: 'タブレット',
    bot: 'ボット',
    other: 'その他',
  },
  browsers: 'ブラウザ',
  systems: 'OS',
  crawlers: '検索エンジンとAI',
  crawlersHint: '訪れたプログラムです。ページを登録する検索エンジン、ページを読むAIアシスタント、リンクのプレビューを表示するチャットなどです。',
  noCrawlers: 'この期間に検索エンジンやAIは訪れていません。',
  /** What did not fit in a day's list, gathered in one row. */
  other: 'その他',
  otherBots: 'その他のプログラム',
  showAll: (count) => `${count}件すべてを表示`,
  showFewer: '表示を減らす',
  /** Next to a share in a list, on hover. */
  share: (part, whole) => `${whole}のうち${part}`,
  none: 'まだありません。',

  /** For a free app that counted more than it shows. */
  moreKept: (since) => `${since}から集計しています。プレミアムなら最大1年分をすべて表示します。`,
  premiumAsk: 'プレミアムについて問い合わせる',
  premiumSubject: '私のアプリのプレミアムについて',
  premiumBody: (address) => `お世話になっております。\n\n私のアプリのアクセス解析をすべて見たいと考えています：${address}\n\n`,

  /** At the foot of the page. */
  footer: (minutes) =>
    minutes === 1 ? '日ごとにUTCで集計しています。1分ごとに更新されます。' : `日ごとにUTCで集計しています。${minutes}分ごとに更新されます。`,
  updated: (time) => `最終更新：${time}`,
  since: (date) => `${date}から集計しています。`,

  switchOff: 'アクセス解析をオフにする',
  switchOffTitle: 'アクセス解析をオフにしますか？',
  switchOffText: '集計を停止し、これまでに集計したものはすべて削除されます。この操作は元に戻せません。',
  keep: '集計を続ける',
  switchOffConfirm: 'オフにして削除する',
  switchedOff: 'アクセス解析をオフにし、集計したものを削除しました。',
  switchOffFailed: 'アクセス解析をオフにできませんでした。',
};
