import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Het domein kon niet worden gelezen.',
  reaching: (domain) => `Requests naar ${domain} komen nu bij deze lambda uit.`,
  saveFailed: 'Het domein kon niet worden opgeslagen.',
  removed: 'Het domein is verwijderd. De lambda reageert hier weer op zijn adres.',
  removeFailed: 'Het domein kon niet worden verwijderd.',
  hint:
    'Een premium lambda kan op een eigen domein draaien, helemaal vanaf de root. Laat het domein eerst naar deze server wijzen en vul het daarna hier in: vanaf dan komen requests naar dat domein bij de lambda uit, en stuurt het adres hier bezoekers door naar dat domein.',
  loading: 'Laden…',
  example: 'jouw-domein.nl',
  open: (domain) => `${domain} openen`,
  label: 'Het domein waarop hij reageert',
  serving: (domain) => <>Draait nu op {domain}. Het adres hier stuurt bezoekers door.</>,
  none: 'Nog geen. Een subdomein zoals shop.example.com, of een heel domein zoals example.com.',
  change: 'Wijzigen',
  use: 'Dit domein gebruiken',
  remove: 'Verwijderen',
  confirm: 'Domein verwijderen?',
  keep: 'Laten staan',
  confirmText: (domain) => (
    <>
      Requests naar {domain} komen meteen niet meer bij deze lambda uit, en het adres hier antwoordt weer zelf in plaats
      van bezoekers door te sturen. De DNS-instellingen van het domein blijven zoals ze zijn.
    </>
  ),
  point: 'Laat het domein naar deze server wijzen',
  check: 'Opnieuw controleren',
  records:
    'Voeg deze twee records toe bij de partij die de DNS van het domein beheert. Laat het AAAA-record weg als je liever niet via IPv6 bereikbaar bent.',
  type: 'Type',
  name: 'Naam',
  value: 'Waarde',
  pointsHere: (domain) => <>{domain} wijst hierheen.</>,
  alsoElsewhere: (addresses) =>
    ` Het verwijst ook naar ${addresses}, en dat is niet deze server. Bezoekers die daar terechtkomen, bereiken de lambda niet.`,
  elsewhere: (addresses) => `Het verwijst naar ${addresses}, en dat is nog niet deze server.`,
  wait: 'Het kan even duren voordat een wijziging overal zichtbaar is, maximaal de TTL van het oude record.',
  cname: 'Een CNAME-record gebruiken',
  cnameText: (target) => (
    <>
      Een subdomein kan ook met een CNAME-record naar {target} wijzen. Dan volgt het deze server als zijn adressen
      ooit veranderen. Er zitten wel nadelen aan:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Het werkt niet voor een heel domein ({example} zelf): de standaard staat geen CNAME toe naast de records die
      elk domein op zijn root heeft. Sommige providers bieden daar een ALIAS-, ANAME- of ‘flattened’ record voor.
    </>
  ),
  cnameAlone: 'Er kan niets anders op dezelfde naam staan: geen MX-record voor mail, geen TXT-record voor verificaties.',
  cnameLookup: 'De resolvers van bezoekers doen één lookup extra voordat ze aankomen.',
  copy: 'Kopiëren',
  copyValue: (value) => `${value} kopiëren`,
};
