import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Die Domain konnte nicht gelesen werden.',
  reaching: (domain) => `Requests an ${domain} erreichen jetzt dieses Lambda.`,
  saveFailed: 'Die Domain konnte nicht gespeichert werden.',
  removed: 'Die Domain ist entfernt. Das Lambda antwortet wieder unter seiner Adresse hier.',
  removeFailed: 'Die Domain konnte nicht entfernt werden.',
  hint:
    'Ein Premium-Lambda kann unter einer eigenen Domain antworten – unter der ganzen Domain, ab der Wurzel. Lassen Sie die Domain zuerst auf diesen Server zeigen und tragen Sie sie dann hier ein: Ab dann erreichen Requests an die Domain das Lambda, und seine Adresse hier leitet Besucher dorthin weiter.',
  loading: 'Lädt …',
  example: 'ihre-domain.de',
  open: (domain) => `${domain} öffnen`,
  label: 'Domain, unter der es antwortet',
  serving: (domain) => <>Antwortet jetzt unter {domain}. Die Adresse hier leitet Besucher dorthin weiter.</>,
  none: 'Noch keine. Eine Subdomain wie shop.example.com oder eine ganze Domain wie example.com.',
  change: 'Ändern',
  use: 'Diese Domain verwenden',
  remove: 'Entfernen',
  confirm: 'Domain entfernen?',
  keep: 'Behalten',
  confirmText: (domain) => (
    <>
      Requests an {domain} erreichen dieses Lambda sofort nicht mehr, und seine Adresse hier antwortet wieder, statt
      Besucher weiterzuleiten. Die DNS-Einträge der Domain bleiben, wie sie sind.
    </>
  ),
  point: 'Domain auf diesen Server zeigen lassen',
  check: 'Nochmal prüfen',
  records:
    'Legen Sie beim DNS-Anbieter der Domain diese zwei Einträge an. Den AAAA-Eintrag können Sie weglassen, wenn die Domain nicht über IPv6 erreichbar sein soll.',
  type: 'Typ',
  name: 'Name',
  value: 'Wert',
  pointsHere: (domain) => <>{domain} zeigt hierher.</>,
  alsoElsewhere: (addresses) =>
    ` Die Domain löst aber auch zu ${addresses} auf, und das ist nicht dieser Server – Besucher, die dort landen, erreichen das Lambda nicht.`,
  elsewhere: (addresses) => `Die Domain löst zu ${addresses} auf – das ist noch nicht dieser Server.`,
  wait: 'Bis eine Änderung überall ankommt, kann es dauern – höchstens so lange wie die TTL des alten Eintrags.',
  cname: 'Stattdessen einen CNAME-Eintrag verwenden',
  cnameText: (target) => (
    <>
      Eine Subdomain kann stattdessen per CNAME auf {target} zeigen. Dann folgt sie diesem Server, falls sich seine
      Adressen einmal ändern. Das hat Nachteile:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Für eine ganze Domain ({example} selbst) geht das nicht: Der Standard erlaubt keinen CNAME neben den Einträgen, die
      jede Domain an ihrer Wurzel hat. Manche Anbieter haben dafür einen ALIAS-, ANAME- oder „Flattening“-Eintrag.
    </>
  ),
  cnameAlone: 'Unter demselben Namen darf nichts anderes stehen – kein MX-Eintrag für E-Mail, kein TXT-Eintrag für Verifizierungen.',
  cnameLookup: 'Die Resolver der Besucher brauchen eine Abfrage mehr, bis sie ankommen.',
  copy: 'Kopieren',
  copyValue: (value) => `${value} kopieren`,
};
