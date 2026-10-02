import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Stwórz stronę internetową z AI.',
  intro:
    'Opisz własnymi słowami stronę internetową lub aplikację, o której myślisz. AI zbuduje ją dla ciebie, my ją hostujemy, a po kilku minutach jest online – z linkiem, który możesz wysłać każdemu. Za darmo, bez programowania, bez rejestracji.',
  placeholder: 'Chcę stronę internetową, która…',
  shortcut: 'Ctrl + Enter',
  buildIt: 'Stwórz moją stronę',
  builtBy: 'Tworzy',
  password: 'hasło',
  fable:
    'Fable jest na razie za hasłem, bo go testujemy. Nie ma limitu czasu, więc pracuje, aż twoja strona będzie gotowa – a nie aż skończy się czas.',
  onlyNew:
    'Tutaj powstają nowe strony. Aby zmienić istniejącą, otwórz jej link do edytora i opisz w sekcji „Zmień”, co ma być inaczej.',
  ideas: [
    'stronę dla naszego klubu, na której członkowie zapisują się na wydarzenia',
    'listę zapisów na naszą imprezę składkową, żeby nikt nie przyniósł tego samego dania',
    'księgę gości na nasze wesele',
    'ankietę, w której ludzie głosują i widzą wyniki',
    'ranking na nasz cotygodniowy quiz',
    'stronę urodzinową, na której znajomi zostawiają życzenia',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Przed twoją jest jeszcze jedna strona – potem twoja kolej.' : `Stron przed twoją: ${waiting}.`,
  starting: 'Zaczynamy…',
  asked: 'Twoja prośba',
  leaveOpen: 'Nie zamykaj tej strony – link do późniejszej zmiany strony pojawi się tylko tutaj, gdy będzie gotowa.',
  log: 'Co zrobiła AI',
  online: 'Twoja strona jest online',
  notOnline: 'Twoja strona powstała, ale nie trafiła do sieci.',
  open: 'Otwórz swoją stronę',
  steps: {
    guide: 'Przygotowania',
    examples: 'Przeglądanie przykładów',
    create: 'Wybieranie adresu twojej strony',
    write: 'Pisanie twojej strony',
    improve: 'Ulepszanie twojej strony',
    check: 'Szukanie błędów',
    online: 'Publikowanie online',
    trying: 'Wypróbowywanie',
    looking: 'Przeglądanie twojej strony',
    forRecords: 'Przygotowywanie miejsca na wpisy',
    forKeys: 'Przygotowywanie miejsca na klucze i hasła',
    forFiles: 'Przygotowywanie miejsca na to, co strona zapisze',
    records: 'Przeglądanie wpisów',
    keys: 'Sprawdzanie, jakich kluczy i haseł potrzebuje',
    addFile: 'Dodawanie pliku',
    removeFile: 'Usuwanie pliku',
    files: 'Przeglądanie tego, co zostało zapisane',
  },

  points: [
    {
      title: 'Bez programowania',
      text: 'Powiedz własnymi słowami, co ma robić twoja strona, jak w rozmowie ze znajomym. AI zbuduje ją dla ciebie – wiedza techniczna nie jest potrzebna.',
    },
    {
      title: 'Darmowy hosting w zestawie',
      text: 'Twoja strona działa na naszych serwerach. Nie kupujesz hostingu, serwera ani domeny i niczego nie instalujesz. Bezpieczeństwem i aktualizacjami zajmujemy się my.',
    },
    {
      title: 'Online w kilka minut',
      text: 'Od razu dostajesz link do udostępnienia. Strona zapamiętuje to, co wpisują ludzie – zapisy, głosy, wiadomości, wyniki – więc wszyscy widzą to samo.',
    },
  ],

  questionsTitle: 'Zanim zaczniesz',
  questions: (offline, removed) => [
    [
      'Czy AI naprawdę stworzy mi stronę internetową za darmo?',
      `Tak. Opisz ją własnymi słowami, a AI ją zbuduje, opublikuje i da ci link. Bez rejestracji, bez karty kredytowej, bez okresu próbnego. Strona działa, dopóki ludzie z niej korzystają: po ${offline} dniach bez odwiedzin i zmian zostaje wyłączona, a po ${removed} dniach usunięta.`,
    ],
    [
      'Czy potrzebuję hostingu, serwera albo domeny?',
      'Nie. Twoja strona działa na naszych serwerach, a hosting, bezpieczeństwo i aktualizacje są w zestawie. Link dostajesz od razu, więc nie musisz też kupować domeny.',
    ],
    [
      'Czy mogę stworzyć aplikację bez programowania?',
      'Tak. Nigdy nie zobaczysz kodu. Powiedz, co ma robić, jak w rozmowie ze znajomym, a AI zrobi resztę – stronę, małą aplikację albo grę.',
    ],
    [
      'Czy ludzie mogą coś wpisywać – zapisy, głosy, wiadomości?',
      'Tak. Twoja strona zapamiętuje to, co wpisują ludzie, więc każdy, kto otworzy link, widzi te same wpisy, głosy i wyniki.',
    ],
    [
      'Jak inni otworzą moją stronę?',
      'Przez link, w dowolnej przeglądarce, na telefonie albo komputerze. Niczego nie trzeba instalować i nie ma po drodze żadnego sklepu z aplikacjami.',
    ],
    [
      'Jak zmienić stronę później?',
      'Otwórz link do edytora, który dostajesz razem ze stroną, i opisz, co ma być inaczej – tak samo jak tutaj. Jeśli zmiana ci się nie spodoba, możesz wrócić do tego, jak było wcześniej.',
    ],
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
