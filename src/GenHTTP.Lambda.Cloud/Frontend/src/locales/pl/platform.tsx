import type { Messages } from '../en';

export const platform: Messages['platform'] = {
  menu: 'Funkcje',

  eyebrow: 'Funkcje',
  title: 'Zbudowanie zajmuje minutę.',
  titleAccent: 'Cała reszta jest już w środku.',
  intro:
    'Aplikacja zrobiona tutaj to coś więcej niż kod napisany przez AI. O jej przydatności decyduje to, co jest potem: jej wpisy, wersje i opis tego, do czego służy, wiedza o tym, kto ją odwiedza, oraz własna domena, gdy aplikacja się przyjmie – wszystko w jednym miejscu, otwieranym linkiem, który dostajesz po jej zbudowaniu.',
  build: 'Zbuduj coś',
  ownAgent: 'Użyj własnego agenta',
  free: 'Za darmo. Bez rejestracji, bez karty kredytowej.',
  heroAlt: 'Edytor aplikacji do rezerwacji kortów: działa pod własną domeną klubu, pokazuje, ile osób korzystało dziś, i ostatnią zmianę',
  example: 'Każdy obrazek na tej stronie pokazuje tę samą przykładową aplikację: rezerwację kortów w klubie tenisowym.',

  contents: 'Na tej stronie',

  makeTitle: 'Zbuduj ją i zmieniaj',
  makeText: 'Powiedz, czego potrzebujesz, i mów to dalej. Nikt nie musi do tego pisać ani czytać kodu.',

  behindTitle: 'W każdej aplikacji',
  behindText:
    'O przydatności aplikacji decyduje wszystko, co jest wokół jej kodu – to jest dla każdego, kto ją zrobił, nie tylko dla tych, którzy czytają kod.',

  yoursTitle: 'Rozwijaj ją dalej po swojemu',
  yoursText: 'Dla programistów i dla każdego, kto chce, żeby jego aplikacja przetrwała dłużej niż ta strona.',

  features: {
    build: {
      name: 'Budowanie',
      title: 'Opisz ją, a będzie online',
      text: 'Powiedz naszemu agentowi, czego potrzebujesz, własnymi słowami – albo poproś Claude Code, Codex lub Cursor. Kilka minut później aplikacja działa pod własnym adresem, gotowa do udostępnienia. Nie ma serwera, bazy danych ani konta do skonfigurowania.',
      points: ['Nasz agent na stronie budowania albo twój własny przez MCP', 'Link do udostępnienia po kilku minutach', 'Za darmo, bez rejestracji'],
      alt: 'Strona rezerwacji kortów: cztery korty, godzina po godzinie, z podpisanymi zarezerwowanymi godzinami',
    },
    change: {
      name: 'Zmień',
      title: 'Zmieniaj ją, prosząc o to',
      text: 'Każda aplikacja ma edytor, otwierany prywatnym linkiem, który dostajesz po jej zbudowaniu. Powiedz, co ma być inaczej, a agent zmieni aplikację i da znać, kiedy zajrzeć – albo daj link własnemu agentowi razem z kolejną zmianą.',
      points: ['Wystarczy jedno zdanie', 'Udostępniana sama, gdy działa, albo zatrzymana, żebyś najpierw ją wypróbował', 'Kod pozostaje poza zasięgiem wzroku, dopóki go nie zechcesz'],
      alt: 'Sekcja Zmień: klub poprosił o lekcje trenera, a agent mówi, że zmiana jest gotowa do wypróbowania',
    },
    drafts: {
      name: 'Szkice',
      title: 'Wypróbuj zmianę, zanim ktokolwiek ją zobaczy',
      text: 'Większą zmianę robi się w szkicu: to aplikacja taka, jaka będzie, pod własnym adresem, z kopią wpisów do wypróbowania. Nic, co tam zrobisz, nie dotyka prawdziwych wpisów. Gdy wszystko jest dobrze, jedno kliknięcie udostępnia ją.',
      points: ['Własny adres do wypróbowania i pokazania innym', 'Dane testowe: kopia wpisów, nie same wpisy', 'Udostępnij, gdy jest dobrze, albo odrzuć'],
      alt: 'Szkic strony rezerwacji, na którym lekcje trenera są pokazane na fioletowo – w kopii rezerwacji',
    },
    history: {
      name: 'Historia',
      title: 'Każda zmiana to wersja, z drogą powrotną',
      text: 'Każda zmiana staje się nową wersją, opisaną tym, co zmieniła. Jeśli coś pójdzie źle, poprzednia wersja jest o jedno kliknięcie – pod tym samym adresem, z tymi samymi wpisami.',
      points: ['O co poproszono i co się zmieniło', 'Powrót do wcześniejszej wersji jednym kliknięciem', 'Wpisy są wspólne dla wszystkich wersji, więc powrót nic nie gubi'],
      alt: 'Historia aplikacji: jej zmiany jedna pod drugą, najnowsza jest online',
    },
    about: {
      name: 'O aplikacji',
      title: 'Wie, czym jest',
      text: 'Przy każdej zmianie agent zapisuje, do czego służy aplikacja, kto z niej korzysta i dlaczego działa tak, a nie inaczej. Czytasz to prostymi słowami, a następny agent czyta to, zanim cokolwiek zmieni – dzięki temu zmiana opiera się na tym, co jest, a nie na domysłach.',
      points: ['Co robi i dla kogo, prostymi słowami', 'Decyzje, które za tym stoją, dla tego, kto zmieni ją następny', 'Jak jest testowana, przechowywane z każdą wersją'],
      alt: 'O twojej aplikacji: rezerwacja kortów opisana prostymi słowami, kto z niej korzysta i co robi',
    },
    records: {
      name: 'Wpisy',
      title: 'Wpisy we własnej bazie danych',
      text: 'Aplikacja, która coś zapamiętuje – rezerwacje, zapisy, wyniki – trzyma to we własnej bazie danych, a wpisy zostają, gdy aplikacja się zmienia. Czytasz je tabela po tabeli w edytorze, obok plików, które zapisała, takich jak tygodniowa lista na tablicę ogłoszeń.',
      points: ['Baza danych dla każdej aplikacji, która jej potrzebuje', 'Każda tabela do przeczytania w edytorze', 'Pliki zapisane przez aplikację wymienione obok'],
      alt: 'Wpisy aplikacji: jej rezerwacje i członkowie, czytane tabela po tabeli',
    },
    keys: {
      name: 'Klucze i hasła',
      title: 'Klucze i hasła pozostają tajne',
      text: 'Gdy aplikacja potrzebuje hasła albo klucza do innej usługi, agent prosi o niego tylko po nazwie. Wartość wpisujesz w edytorze i od tej chwili nikt jej nie widzi – ani ty, ani agent. Odczytuje ją tylko aplikacja.',
      points: ['Agent nigdy nie widzi wartości', 'Przechowywane w postaci zaszyfrowanej, nigdy więcej nie pokazywane', 'Wymienisz je, kiedy zechcesz'],
      alt: 'Klucze i hasła: hasło recepcji jest ustawione, a jego wartość jest ukryta',
    },
    analytics: {
      name: 'Analityka',
      title: 'Zobacz, kto odwiedza – bez ciasteczek',
      text: 'Włącz analitykę i sprawdź, ile osób otworzyło aplikację, skąd przyszły i na jakich urządzeniach – liczone na naszym serwerze, bez ciasteczek i bez skryptu na twoich stronach.',
      points: ['Odwiedzający, odsłony i skąd przyszli', 'Bez ciasteczek i bez niczego, co identyfikuje odwiedzającego', 'Wyłączona, dopóki jej nie włączysz'],
      alt: 'Analityka: odwiedzający z ostatnich trzydziestu dni, dzień po dniu',
    },
    domain: {
      name: 'Domena',
      title: 'Własna domena',
      text: 'Każda aplikacja zaczyna pod naszym adresem. Gdy się przyjmie, może przenieść się na domenę, którą posiadasz: ta sama aplikacja i ten sam edytor, pod adresem, który ludzie zapamiętają. Linki do starego adresu nadal działają – prowadzą do nowego.',
      points: ['Dla aplikacji Premium: napisz do nas', 'Ta sama aplikacja, edytor i wpisy', 'Stary adres odsyła odwiedzających dalej'],
      alt: 'Adres aplikacji przenosi się z adresu na genhttp.run na własną domenę klubu',
    },
    code: {
      name: 'Kod i git',
      title: 'Kod, kiedy go zechcesz',
      text: 'Przełącz edytor na pełny widok, a znajdziesz tam kod każdej wersji, razem z logami i wykresami. Sklonuj aplikację przez git i wypchnij zmianę albo pobierz ją jako projekt .NET, który działa wszędzie. To twój kod.',
      points: ['Każdy plik, porównany z wersji na wersję', 'Git: commit na main to wersja, gałąź to szkic', 'Pobranie, które działa samo, razem z wpisami'],
      alt: 'Pełny widok edytora: pliki aplikacji i jej kod',
    },
    publish: {
      name: 'Galeria i kod źródłowy',
      title: 'Pokaż ją albo opublikuj jej kod',
      text: 'Dodaj aplikację do galerii, żeby inni mogli ją znaleźć, albo opublikuj jej kod na licencji open source, żeby każdy mógł go czytać, oceniać gwiazdką i pobierać. Jedno i drugie to twój wybór i żadne nie ujawnia wpisów.',
      points: ['W galerii, z obrazkiem i opisem', 'Jej kod na licencji MIT lub innej', 'Nigdy jej wpisy, klucze ani odwiedzający'],
      alt: 'Opublikowany kod rezerwacji kortów: jego pliki, dokumentacja i wersje, na licencji MIT',
    },
  },

  draftBadge: 'Szkic',
  domainOwn: 'Premium',
  domainSubject: 'Domena dla mojej aplikacji',
  domainAsk: 'Zapytaj o domenę',
  zoom: 'Pokaż większe',
  close: 'Zamknij',

  closeTitle: 'Wypróbuj za darmo.',
  closeAccent: 'Bez rejestracji.',
  closeBuild: 'Zbuduj coś',
  closeShip: 'Opublikuj to, co zbudowałeś',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda jest open source. Używaj jej tutaj albo {selfHosting('uruchom ją na własnym serwerze')} – kod jest na{' '}
      {github('GitHubie')}.
    </>
  ),
};
