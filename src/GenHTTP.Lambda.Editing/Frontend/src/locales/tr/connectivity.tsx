import type { EditorMessages } from '../en';

export const connectivity: EditorMessages['connectivity'] = {
  hint:
    'Kodun başka sunuculara bağlanıp bağlanamayacağı: bir API çağırmak, soket açmak, e-posta göndermek. Lambdalar tek bir sunucuyu paylaşır, bu yüzden izin lambda lambda verilir - premium bir lambda bağlanabilir, diğerleri ise bu sitenin operatörü izin verdikten sonra.',

  included: 'Premium ile dahil',
  includedText: 'Kod başka sunuculara bağlanabilir: premium bunu içerir.',
  allowed: 'İzin verildi',
  allowedText: (ago) => <>Bu sitenin operatörü {ago} izin verdi. Kod başka sunuculara bağlanabilir.</>,
  requested: 'Operatör bekleniyor',
  requestedText: (ago) => (
    <>{ago} istendi. Bu sitenin operatörü izin verdiğinde, bağlanan kod derlenir - o zamana kadar reddedilir.</>
  ),
  declined: 'Reddedildi',
  declinedText: (ago) => <>Bu sitenin operatörü {ago} reddetti. Ne için gerektiğini daha ayrıntılı anlatarak yeniden isteyebilirsiniz.</>,
  none: 'İzin verilmedi',
  noneText:
    'Kod başka sunuculara bağlanamaz: bir API çağıran, soket açan veya e-posta gönderen kod derlenmez. Lambda buna ihtiyaç duyuyorsa aşağıdan isteyin.',
  operatorSaid: 'Operatör şunu yazdı',
  purposeSaid: 'Ne için gerekli',

  connects: 'Kod başka sunuculara bağlanıyor, ancak bu lambdanın buna izni yok: izin verilene kadar derlenmez.',
  connectsWaiting: 'Kod başka sunuculara bağlanıyor: operatör izin verdiğinde derlenir.',

  askTitle: 'İzin isteyin',
  askAgainTitle: 'Yeniden isteyin',
  editTitle: 'Ne için gerekli',
  purpose: 'Neye bağlanıyor ve neden?',
  purposeHint: 'Sunucuları veya hizmetleri ve lambdanın bunlarla ne yaptığını yazın. Bu sitenin operatörü karar vermek için bunu okur.',
  purposeExample: 'api.open-meteo.com, ana sayfadaki hava durumu için',
  send: 'İsteği gönder',
  save: 'Kaydet',
  cancel: 'Vazgeç',
  edit: 'Amacı değiştir',
  asked: 'İstendi. Bu sitenin operatörü karar verir.',
  askFailed: 'İstek gönderilemedi.',
  withdraw: 'İsteği geri çek',
  withdrawn: 'İstek geri çekildi.',
  giveUp: 'Vazgeç',
  givenUp: 'Kod artık başka sunuculara bağlanamaz.',
  giveUpTitle: 'Başka sunuculara bağlanmaktan vazgeçilsin mi?',
  giveUpText:
    'Sonraki istekten itibaren kod başka sunuculara bağlanamaz ve yayındaki sürüm bağlandığı yerlerde çalışmayı bırakır. Geri almak için yeniden isteyin.',
  keep: 'Kalsın',
  failed: 'Bu işlem başarısız oldu.',

  coversTitle: 'Neler sayılır',
  covers: ['HttpClient ve WebRequest', 'Soketler ve WebSocket istemcileri', 'E-posta (SmtpClient)', 'DNS ve ping', 'Bir adresten yüklenen XML'],
  notCovered:
    'Tarayıcıda olanlar buna dahil değildir: lambdanın sayfaları başka sitelerden kendileri veri alabilir; ziyaretçiler, webhook’lar ve websocket’ler lambdaya her zamanki gibi ulaşır. Tarayıcıların çağırabileceği herkese açık bir API en iyi sayfadan çağrılır - bunun için izin gerekmez.',

  refused: 'Kod başka sunuculara bağlanıyor',
  refusedText: 'Bu lambdanın buna henüz izni yok. İzin isteyin, bu sitenin operatörü karar verir.',
  openConnectivity: 'İzin isteyin',
};
