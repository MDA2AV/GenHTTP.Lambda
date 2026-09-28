import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Od pomysłu do strony internetowej.',
  intro:
    'Opisz stronę internetową lub aplikację, o której myślisz. AI ją dla ciebie stworzy, my hostujemy ją na naszych serwerach, a ona od razu jest online – z linkiem, który możesz wysłać każdemu. Bez programowania, bez konfigurowania hostingu, bez konta.',
  placeholder: 'Chcę stronę internetową, która…',
  working: 'agent pracuje…',
  shortcut: 'Ctrl + Enter',
  building: 'Tworzenie',
  buildIt: 'Stwórz moją stronę',
  builtBy: 'Tworzy',
  password: 'hasło',
  fable:
    'Fable jest na razie za hasłem, bo go testujemy. Nie ma limitu czasu, więc pracuje, aż skończy – a nie aż skończy się czas.',
  onlyNew:
    'Tutaj powstają nowe strony. Aby zmienić istniejącą, otwórz jej link do edytora i opisz w sekcji „Zmień”, co ma być inaczej.',
  ideas: [
    'stronę dla naszego klubu, na której członkowie zapisują się na wydarzenia',
    'księgę gości na nasze wesele',
    'ankietę, w której ludzie głosują i widzą wyniki',
    'tablicę wyników na nasz cotygodniowy quiz',
    'odliczanie do naszego otwarcia, które każdy może zobaczyć',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Przed twoją jest jeszcze jedna strona – potem twoja kolej.' : `Stron przed twoją: ${waiting}.`,
  starting: 'Zaczynamy…',

  points: [
    {
      title: 'Opisana, nie zaprogramowana',
      text: 'Powiedz własnymi słowami, co ma robić twoja strona. Nie potrzebujesz programowania ani wiedzy technicznej.',
    },
    {
      title: 'Hosting wliczony',
      text: 'Twoja strona działa na naszych serwerach. Hostingiem, bezpieczeństwem i aktualizacjami zajmujemy się my – niczego nie musisz konfigurować ani pilnować.',
    },
    {
      title: 'Online w kilka minut',
      text: 'Od razu dostajesz link do udostępnienia. Strona może też zapamiętywać dane – zgłoszenia, głosy, wyniki – więc wszyscy widzą to samo.',
    },
  ],

  yourApp: 'Twoja strona',
  further: 'Aby zmienić ją później',
  keep:
    'Zachowaj ten link. To jedyna droga powrotu i nikt go nie odzyska – my też nie. Dodaj go do zakładek, zanim zamkniesz tę kartę.',
  change:
    'Aby zmienić swoją stronę, otwórz link do edytora i opisz w sekcji „Zmień”, co ma być inaczej – tak samo jak tutaj. Może to zrobić także twój własny asystent AI, jak opisano poniżej.',
  copyLink: 'Kopiuj link do edytora',
  lifetime: (offline, removed) =>
    `Utrzymujemy ją online, dopóki jest używana: po ${offline} dniach bez odwiedzin i zmian zostaje wyłączona, a po ${removed} dniach usunięta. Otwórz edytor, aby znów ją uruchomić.`,
  openEditor: 'Otwórz edytor',
  another: 'Stwórz kolejną stronę',

  keepGoing: 'Działaj dalej z własnym asystentem AI',
  orOwn: 'Albo użyj własnego asystenta AI',
  ownText:
    'Korzystasz już z Claude albo innego asystenta AI? Podłącz go tutaj, a będzie tworzył i zmieniał strony dla ciebie w ten sam sposób. My je hostujemy, więc nadal nie musisz niczego konfigurować. Nie ma dziennego limitu.',
  ownTitle: 'Stwórz stronę ze swoim asystentem AI',
  ownOnly:
    'Podłącz Claude albo innego asystenta AI pod adres poniżej i opisz stronę, jakiej chcesz. Asystent ją tworzy, my hostujemy ją na naszych serwerach, a ona od razu jest online – z linkiem do udostępnienia.',
  thenAsk: 'Potem powiedz mu, czego chcesz, na przykład: „Stwórz stronę dla naszego chóru z kalendarzem koncertów”.',
  howToChange: 'Tak samo zmienisz stronę później: daj asystentowi link do edytora i powiedz, co ma być inaczej.',

  failedToStart: 'Nie udało się wysłać prośby.',
  noAnswer: 'Agent skończył, ale nie napisał, co się stało.',
  failed: 'Nie udało się.',
};
