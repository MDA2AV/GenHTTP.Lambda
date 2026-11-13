import type { EditorMessages } from '../en';

export const connectivity: EditorMessages['connectivity'] = {
  hint:
    'コードが他のサーバーに接続できるかどうかです。APIの呼び出し、ソケットの接続、メール送信などが対象です。lambdaは1台のサーバーを共有しているため、lambdaごとに許可します。プレミアムのlambdaは接続でき、それ以外はこのサイトの運営者が許可した場合に接続できます。',

  included: 'プレミアムに含まれます',
  includedText: 'コードは他のサーバーに接続できます。プレミアムに含まれています。',
  allowed: '許可済み',
  allowedText: (ago) => <>このサイトの運営者が{ago}に許可しました。コードは他のサーバーに接続できます。</>,
  requested: '運営者の確認待ち',
  requestedText: (ago) => (
    <>{ago}に依頼しました。このサイトの運営者が許可すると、接続するコードがコンパイルされます。それまでは拒否されます。</>
  ),
  declined: '却下されました',
  declinedText: (ago) => <>このサイトの運営者が{ago}に却下しました。用途をもっと詳しく書いて、もう一度依頼できます。</>,
  none: '許可されていません',
  noneText:
    'コードは他のサーバーに接続できません。APIの呼び出し、ソケットの接続、メール送信を行うコードはコンパイルされません。lambdaに必要な場合は、下から依頼してください。',
  operatorSaid: '運営者のコメント',
  purposeSaid: '用途',

  connects: 'コードが他のサーバーに接続していますが、このlambdaには許可されていません。許可されるまでコンパイルされません。',
  connectsWaiting: 'コードが他のサーバーに接続しています。運営者が許可すると、コンパイルされます。',

  askTitle: '許可を依頼する',
  askAgainTitle: 'もう一度依頼する',
  editTitle: '用途',
  purpose: '何に接続し、何のために使いますか?',
  purposeHint: 'サーバーやサービスの名前と、lambdaがそれらで何をするかを書いてください。このサイトの運営者が読んで判断します。',
  purposeExample: 'api.open-meteo.com(トップページの天気予報用)',
  send: '依頼を送信',
  save: '保存',
  cancel: 'キャンセル',
  edit: '用途を変更',
  asked: '依頼しました。このサイトの運営者が判断します。',
  askFailed: '依頼を送信できませんでした。',
  withdraw: '依頼を取り下げる',
  withdrawn: '依頼を取り下げました。',
  giveUp: '許可を手放す',
  givenUp: 'コードは他のサーバーに接続できなくなりました。',
  giveUpTitle: '他のサーバーへの接続を手放しますか?',
  giveUpText:
    '次のリクエストから、コードは他のサーバーに接続できなくなり、公開中のバージョンも接続する部分が動かなくなります。再び使うには、もう一度依頼してください。',
  keep: '手放さない',
  failed: '実行できませんでした。',

  coversTitle: '対象となるもの',
  covers: ['HttpClientとWebRequest', 'ソケットとWebSocketクライアント', 'メール(SmtpClient)', 'DNSとping', 'アドレスから読み込むXML'],
  notCovered:
    'ブラウザー上で行われることは対象外です。lambdaのページは他のサイトから自分で取得でき、訪問者、Webhook、WebSocketも従来どおりlambdaに届きます。ブラウザーから呼び出せる公開APIは、ページから呼び出すのが最適で、許可は必要ありません。',

  refused: 'コードが他のサーバーに接続しています',
  refusedText: 'このlambdaはまだ接続できません。許可を依頼すると、このサイトの運営者が判断します。',
  openConnectivity: '許可を依頼する',
};
