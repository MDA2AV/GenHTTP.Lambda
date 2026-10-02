import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Nie udało się odczytać domeny.',
  reaching: (domain) => `Żądania do ${domain} trafiają teraz do tej lambdy.`,
  saveFailed: 'Nie udało się zapisać domeny.',
  removed: 'Domena została usunięta. Lambda nadal odpowiada pod swoim adresem tutaj.',
  removeFailed: 'Nie udało się usunąć domeny.',
  hint:
    'Lambda premium może odpowiadać nie tylko pod swoim adresem tutaj, ale też pod całą własną domeną. Skieruj domenę na ten serwer, wpisz ją tutaj, a żądania do niej trafią do lambdy.',
  loading: 'Ładowanie…',
  example: 'twoja-domena.pl',
  open: (domain) => `Otwórz ${domain}`,
  label: 'Domena, pod którą odpowiada',
  serving: (domain) => <>Działa teraz pod {domain} i nadal pod swoim adresem tutaj.</>,
  none: 'Jeszcze brak. Może to być subdomena, np. shop.example.com, albo cała domena, np. example.com.',
  change: 'Zmień',
  use: 'Użyj tej domeny',
  remove: 'Usuń',
  confirm: 'Usunąć domenę?',
  keep: 'Zostaw',
  confirmText: (domain) => (
    <>
      Żądania do {domain} od razu przestaną trafiać do tej lambdy. Jej adres tutaj zostaje bez zmian, podobnie jak
      ustawienia DNS domeny.
    </>
  ),
  point: 'Skieruj domenę na ten serwer',
  check: 'Sprawdź ponownie',
  records:
    'U dostawcy DNS domeny dodaj te dwa rekordy. Pomiń rekord AAAA, jeśli nie chcesz, żeby domena była dostępna przez IPv6.',
  type: 'Typ',
  name: 'Nazwa',
  value: 'Wartość',
  pointsHere: (domain) => <>{domain} wskazuje na ten serwer.</>,
  alsoElsewhere: (addresses) =>
    ` Domena wskazuje też na ${addresses}, a to nie jest ten serwer – odwiedzający, którzy tam trafią, nie dotrą do lambdy.`,
  elsewhere: (addresses) => `Domena wskazuje na ${addresses}, a to jeszcze nie jest ten serwer.`,
  wait: 'Zmiana może chwilę potrwać, zanim będzie widoczna wszędzie – najdłużej tyle, ile wynosi TTL starego rekordu.',
  cname: 'Rekord CNAME jako alternatywa',
  cnameText: (target) => (
    <>
      Subdomena może zamiast tego wskazywać na {target} rekordem CNAME – wtedy, jeśli adresy tego serwera kiedyś się
      zmienią, domena sama za nimi nadąży. Ma to jednak wady:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Nie da się tego zrobić dla całej domeny (czyli {example} bez subdomeny): standard nie pozwala na CNAME obok
      rekordów, które każda domena ma w korzeniu. Niektórzy dostawcy oferują w zamian rekord ALIAS, ANAME lub
      „spłaszczony”, który tam działa.
    </>
  ),
  cnameAlone: 'Pod tą samą nazwą nie może być nic innego – ani rekordu MX dla poczty, ani rekordu TXT do weryfikacji.',
  cnameLookup: 'Serwery DNS odwiedzających muszą wykonać jedno zapytanie więcej.',
  copy: 'Kopiuj',
  copyValue: (value) => `Kopiuj ${value}`,
};
