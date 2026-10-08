import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'ドメインを読み込めませんでした。',
  reaching: (domain) => `${domain}へのリクエストが、このlambdaに届くようになりました。`,
  saveFailed: 'ドメインを保存できませんでした。',
  removed: 'ドメインを削除しました。lambdaは再び通常のURLで応答します。',
  removeFailed: 'ドメインを削除できませんでした。',
  hint:
    'プレミアムのlambdaは、独自ドメインでも応答できます（ルート以下すべて）。まずドメインをこのサーバーに向けてから、ここに入力してください。以降はそのドメインへのリクエストがlambdaに届き、通常のURLにアクセスした訪問者はそのドメインに転送されます。',
  loading: '読み込み中…',
  example: 'your-domain.com',
  open: (domain) => `${domain}を開く`,
  label: '応答するドメイン',
  serving: (domain) => <>{domain}で配信中です。通常のURLにアクセスした訪問者はこちらに転送されます。</>,
  none: 'まだありません。shop.example.comのようなサブドメインも、example.comのようなドメイン全体も使えます。',
  change: '変更',
  use: 'このドメインを使う',
  remove: '削除',
  confirm: 'ドメインを削除しますか？',
  keep: '残す',
  confirmText: (domain) => (
    <>
      {domain}
      へのリクエストは、すぐにこのlambdaに届かなくなり、通常のURLは訪問者を転送せず、再び直接応答します。ドメインのDNS設定は変わりません。
    </>
  ),
  point: 'ドメインをこのサーバーに向ける',
  check: '再確認',
  records:
    'ドメインのDNSを管理しているサービスで、次の2つのレコードを追加してください。IPv6で接続できなくてもよければ、AAAAレコードは省いてかまいません。',
  type: 'タイプ',
  name: '名前',
  value: '値',
  pointsHere: (domain) => <>{domain}はこのサーバーを向いています。</>,
  alsoElsewhere: (addresses) =>
    `ただし、このサーバー以外の${addresses}にも解決されます。そちらに送られた訪問者は、lambdaにたどり着けません。`,
  elsewhere: (addresses) => `現在は${addresses}に解決されます。まだこのサーバーを向いていません。`,
  wait: '変更が行き渡るまで、しばらくかかることがあります（最長で古いレコードのTTL分）。',
  cname: '代わりにCNAMEレコードを使う',
  cnameText: (target) => (
    <>
      サブドメインなら、代わりにCNAMEレコードで{target}
      を指すこともできます。そうすれば、このサーバーのIPアドレスが変わっても自動で追従します。ただし、欠点もあります：
    </>
  ),
  cnameRoot: (example) => (
    <>
      ドメイン全体（{example}
      そのもの）には使えません。どのドメインもルートに持っているレコードと同じ名前にCNAMEを置くことは、規格で認められていないためです。代わりにALIAS、ANAME、「フラット化」レコードが使えるプロバイダーもあります。
    </>
  ),
  cnameAlone: '同じ名前にほかのレコードを置けません。メール用のMXレコードも、認証用のTXTレコードも使えなくなります。',
  cnameLookup: '訪問者のリゾルバーが、到着までに名前解決を1回多く行います。',
  copy: 'コピー',
  copyValue: (value) => `${value}をコピー`,
};
