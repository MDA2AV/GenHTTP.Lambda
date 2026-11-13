import type { EditorMessages } from '../en';

export const connectivity: EditorMessages['connectivity'] = {
  hint:
    'Czy kod może łączyć się z innymi serwerami: wywoływać API, otwierać gniazdo, wysyłać pocztę. Lambdy współdzielą jeden serwer, dlatego zgoda dotyczy każdej lambdy osobno – lambda premium ją ma, każda inna dopiero po zgodzie operatora tej strony.',

  included: 'W cenie premium',
  includedText: 'Kod może łączyć się z innymi serwerami: premium to obejmuje.',
  allowed: 'Dozwolone',
  allowedText: (ago) => <>Operator tej strony zezwolił na to {ago}. Kod może łączyć się z innymi serwerami.</>,
  requested: 'Czeka na operatora',
  requestedText: (ago) => (
    <>Prośba wysłana {ago}. Gdy operator tej strony się zgodzi, kod, który się łączy, będzie się kompilował – do tego czasu jest odrzucany.</>
  ),
  declined: 'Odrzucone',
  declinedText: (ago) => <>Operator tej strony odrzucił prośbę {ago}. Możesz poprosić ponownie, opisując dokładniej, do czego to służy.</>,
  none: 'Niedozwolone',
  noneText:
    'Kod nie może łączyć się z innymi serwerami: kod, który wywołuje API, otwiera gniazdo lub wysyła pocztę, się nie kompiluje. Jeśli lambda tego potrzebuje, poproś o zgodę poniżej.',
  operatorSaid: 'Operator napisał',
  purposeSaid: 'Do czego to służy',

  connects: 'Kod łączy się z innymi serwerami, a ta lambda nie ma na to zgody: nie skompiluje się, dopóki zgoda nie zostanie udzielona.',
  connectsWaiting: 'Kod łączy się z innymi serwerami: skompiluje się, gdy operator się zgodzi.',

  askTitle: 'Poproś o zgodę',
  askAgainTitle: 'Poproś ponownie',
  editTitle: 'Do czego to służy',
  purpose: 'Z czym się łączy i po co?',
  purposeHint: 'Podaj serwery lub usługi oraz to, co lambda z nimi robi. Operator tej strony czyta to, by zdecydować.',
  purposeExample: 'api.open-meteo.com, do prognozy na stronie głównej',
  send: 'Wyślij prośbę',
  save: 'Zapisz',
  cancel: 'Anuluj',
  edit: 'Zmień opis, do czego to służy',
  asked: 'Prośba wysłana. Operator tej strony zdecyduje.',
  askFailed: 'Nie udało się wysłać prośby.',
  withdraw: 'Wycofaj prośbę',
  withdrawn: 'Prośba została wycofana.',
  giveUp: 'Zrezygnuj',
  givenUp: 'Kod nie może już łączyć się z innymi serwerami.',
  giveUpTitle: 'Zrezygnować z łączenia z innymi serwerami?',
  giveUpText:
    'Od następnego żądania kod nie będzie mógł łączyć się z innymi serwerami, a wersja online przestanie działać tam, gdzie to robi. Aby to przywrócić, poproś ponownie.',
  keep: 'Zachowaj',
  failed: 'To się nie udało.',

  coversTitle: 'Co się liczy',
  covers: ['HttpClient i WebRequest', 'Gniazda i klienty WebSocket', 'Poczta (SmtpClient)', 'DNS i ping', 'XML wczytywany z adresu'],
  notCovered:
    'Nie dotyczy tego, co dzieje się w przeglądarce: strony lambdy mogą same pobierać dane z innych witryn, a odwiedzający, webhooki i websockety docierają do lambdy jak dotąd. Publiczne API, które mogą wywoływać przeglądarki, najlepiej wywołać ze strony – to nie wymaga zgody.',

  refused: 'Kod łączy się z innymi serwerami',
  refusedText: 'Ta lambda jeszcze nie ma na to zgody. Poproś o nią, a operator tej strony zdecyduje.',
  openConnectivity: 'Poproś o zgodę',
};
