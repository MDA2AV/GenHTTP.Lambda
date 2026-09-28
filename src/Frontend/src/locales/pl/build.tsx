import type { Messages } from '../en';
import { counted } from './plural';

export const build: Messages['build'] = {
  offTitle: 'Ta funkcja jest tu wyłączona',
  off: (write, mcp) => (
    <>
      Ta instalacja nie ma agenta do budowania. Nadal możesz {write('napisać kod samodzielnie')} albo podłączyć własnego
      agenta Claude pod {mcp}.
    </>
  ),

  title: 'Powiedz, czego chcesz.',
  intro:
    'Agent to zbuduje, wrzuci do sieci i da ci link, który możesz wysłać, komu chcesz. Bez konta, bez instalacji. Aplikacja zapamiętuje dane – wyniki, wiadomości, wpisy – więc każdy, kto ją otworzy, widzi to samo.',
  placeholder: 'zbuduj…',
  working: 'agent pracuje…',
  shortcut: 'Ctrl + Enter',
  building: 'Budowanie',
  buildIt: 'Zbuduj',
  builtBy: 'Model',
  password: 'hasło',
  fable:
    'Fable jest na razie za hasłem, bo go testujemy. Nie ma limitu czasu, więc pracuje, aż skończy – a nie aż skończy się czas.',
  onlyNew:
    'Tu powstają tylko nowe aplikacje. Chcesz rozwinąć coś, co już masz? Daj link do edytora własnemu agentowi – szczegóły niżej.',
  ideas: [
    'zrób tablicę, na której każdy może zostawić krótki wpis',
    'potrzebuję rankingu wyników do gry w kości',
    'zbuduj ankietę, w której ludzie głosują i widzą wyniki',
    'przygotuj księgę gości na moje wesele',
    'zrób odliczanie do ważnej daty, widoczne dla wszystkich',
  ],
  ahead: (waiting) =>
    waiting === 1
      ? 'Przed tobą jeszcze jedno zadanie – zaraz twoja kolej.'
      : `Przed tobą w kolejce ${counted(waiting, 'zadanie', 'zadania', 'zadań')}.`,
  starting: 'Zaczynamy…',

  yourApp: 'Twoja aplikacja',
  further: 'Do dalszej pracy',
  keep: 'Zachowaj ten link. To jedyna droga powrotu i nikt go nie odzyska – my też nie. Dodaj go do zakładek, zanim zamkniesz tę kartę.',
  change:
    'Ta strona tylko tworzy nowe aplikacje. Żeby zmienić tę, podłącz własnego agenta (instrukcja niżej), daj mu link do edytora i powiedz, co ma być inaczej.',
  copyLink: 'Kopiuj link do edytora',
  lifetime: (offline, removed) =>
    `Aplikacja działa, dopóki ktoś z niej korzysta. Po ${offline} dniach bez odwiedzin i zmian zostaje wyłączona, a po ${removed} dniach usunięta. Żeby ją przywrócić, otwórz edytor i kliknij „Wdróż”.`,
  openEditor: 'Otwórz edytor',
  another: 'Zbuduj coś innego',

  keepGoing: 'Pracuj dalej z własnym agentem',
  orOwn: 'Albo użyj własnego agenta',
  ownText:
    'Pole powyżej obsługuje Claude działający na naszym serwerze. Jeśli masz własnego agenta, możesz go podłączyć tutaj. Zrobi to samo – utworzy lambdę, napisze kod i wrzuci ją do sieci – bez dziennego limitu i bez tej strony.',
  thenAsk: 'Potem poproś go o to, czego chcesz – tak samo jak tutaj.',
  claudeWeb: 'Claude w przeglądarce',
  claudeWebHow:
    'Ustawienia, potem „Connectors”, potem „Add custom connector”. Wklej adres podany wyżej jako URL zdalnego serwera MCP. Nie potrzeba klucza ani logowania.',
  howToChange: 'Tak samo zmienisz coś, co już działa: daj agentowi link do edytora i powiedz, co ma zrobić.',
  more: 'Więcej o pracy z agentem',

  failedToStart: 'Nie udało się wysłać prośby.',
  noAnswer: 'Agent skończył, ale nie napisał, co się stało.',
  failed: 'Nie udało się.',
};
