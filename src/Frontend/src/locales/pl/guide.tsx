import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Jak to działa',
  intro:
    'Piszesz snippet w C#. To, co zwraca, w kilka sekund trafia pod publiczny adres HTTPS. Poniżej wszystko po kolei – w takiej kolejności, w jakiej na to trafisz.',
  contents: 'Spis treści',

  parts: {
    what: 'Czym jest lambda',
    first: 'Twoja pierwsza lambda',
    editor: 'Centrum sterowania',
    why: 'Opisz, dlaczego',
    written: 'Dokumentacja i testy',
    features: 'Bezpieczne zmiany',
    files: 'Więcej niż jeden plik',
    page: 'Serwowanie strony',
    spa: 'Frontend krok po kroku',
    storage: 'Dwa miejsca na pliki',
    database: 'Przechowywanie rekordów',
    keeping: 'Przechowywanie plików',
    secrets: 'Klucze i hasła',
    sockets: 'Websockety',
    limits: 'Czego nie da się zrobić',
    away: 'Zabierz kod ze sobą',
    open: 'Publikowanie kodu',
    agents: 'Niech zrobi to agent',
  },

  what: [
    (k) => (
      <>
        Lambda to snippet, który zwraca handler GenHTTP. Platforma go kompiluje, ładuje i montuje to, co zwrócił, pod
        twoim własnym adresem. Nie ma projektu, pliku builda ani instrukcji {k.code('using')}. Wszystkie moduły GenHTTP
        są już zaimportowane.
      </>
    ),
    (k) => (
      <>
        To kompletna lambda. Wdrożona pod {k.code('/lambda/your-key/')} odpowiada na każde żądanie słowem hello.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Snippet to {k.em('instrukcje')}, a nie klasa. Na końcu zwraca coś, co potrafi obsługiwać żądania: handler albo
      builder, który go tworzy.
    </>
  ),

  first: [
    (k) => (
      <>
        Kliknij {k.b('Utwórz lambdę')}. Dostaniesz publiczny adres i klucz edytora. Klucz to jedyna droga powrotu, więc
        go zachowaj. Nikt go za ciebie nie odzyska.
      </>
    ),
    () => (
      <>
        Trafiasz do centrum sterowania, a pierwsza wersja to już gotowa mała usługa REST. To tylko punkt wyjścia.
      </>
    ),
    (k) => (
      <>
        Daj klucz edytora agentowi i powiedz, co ma zbudować – zapisuje nowe wersje przez {k.link('/#agents', 'MCP')}.
        Albo otwórz {k.b('Kod')} i napisz wszystko samodzielnie: {k.b('Sprawdź')} kompiluje bez zapisywania i pokazuje,
        co mówi kompilator – z nazwą pliku i numerem linii.
      </>
    ),
    (k) => (
      <>
        Kliknij {k.b('Wdróż')}. Teraz lambda jest online. Wcześniej nic nie jest dostępne, a każde kolejne wdrożenie
        wydłuża czas, przez który pozostaje online.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Link do edytora otwiera centrum sterowania, a nie pole tekstowe: większość kodu piszą tu agenci, więc najpierw
      widzisz, jak radzi sobie twoja lambda. Na pasku bocznym jest sama lambda – czy jest online, jej adres i przycisk,
      gdy nowsza wersja czeka na wdrożenie – oraz jej sekcje. Rzadsze akcje, jak zmiana adresu czy usunięcie, są tam w
      menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Przegląd', () => <>Czym jest aplikacja, czy jest online, ile dziś było żądań i ile z nich się nie udało, ostatnia zmiana i ile zostało miejsca.</>],
    ['Dokumentacja', () => <>Czym jest aplikacja, dla kogo i po co, i dlaczego jest zbudowana właśnie tak – pisana przez agentów, zachowywana z każdą wersją.</>],
    [
      'Zmień',
      (k) => (
        <>
          Napisz, co ma być inaczej, a agent na tym serwerze zrobi to na twoich oczach. Pracuje w szkicu, tam sprawdza
          zmianę, a gdy działa, scala szkic w kolejną wersję. Wyłącz {k.b('Wdróż po zakończeniu')}, jeśli chcesz
          najpierw samodzielnie wypróbować szkic.
          Zajmuje się tylko twoją aplikacją: prośbę, która jej nie dotyczy albo ma komuś zaszkodzić, odrzuca i mówi dlaczego.
        </>
      ),
    ],
    ['Szkice', () => <>Zmiany przygotowywane obok lambdy: każdą wypróbowuje się pod osobnym adresem i scala w kolejną wersję, gdy jest gotowa. Otwarty szkic ma własny kod, dane i logi.</>],
    ['Pliki', () => <>Pliki danej wersji: jej kod i zasoby, czyli sam program. Kłódka albo globus pokazuje, czy są publicznie dostępne.</>],
    ['Dane', () => <>To, co lambda przechowuje w trakcie działania, wspólne dla wszystkich wersji: baza danych, obszar roboczy i sekrety, każde na własnej karcie. Przeglądaj tabele i pliki, przesyłaj pliki, ustawiaj sekrety albo włączaj i wyłączaj dany rodzaj. Widok uproszczony pokazuje tę sekcję, gdy tylko aplikacja coś przechowuje.</>],
    ['Wersje', () => <>Co zmieniła każda wersja, o co proszono i czym różni się od poprzedniej. Stąd wdrażasz wersję albo wracasz do starszej – albo tworzysz szkic na bazie dowolnej z nich.</>],
    ['Wdrożenia', () => <>Co i kiedy było online – i co to wyłączyło.</>],
    ['Statystyki', () => <>Żądania, błędy, czasy odpowiedzi i najczęściej odwiedzane ścieżki z ostatniej godziny albo ostatnich 24 godzin.</>],
    ['Logi', () => <>Żądania, to, co lambda wypisała, i stack trace każdego błędu – na bieżąco.</>],
    [
      'Kod',
      (k) => (
        <>
          Tu piszesz kod ręcznie. {k.b('Sprawdź')} kompiluje, {k.b('Zapisz')} tworzy wersję, {k.b('Wdróż')} wrzuca kod
          online. W szkicu {k.b('Zapisz')} zostawia kod w szkicu, a {k.b('Wdróż podgląd')} wrzuca go online pod adresem
          szkicu. {k.code('Ctrl-S')} zapisuje; {k.code('F12')} przechodzi do deklaracji.
        </>
      ),
    ],
    ['Testy', () => <>Jak aplikacja jest testowana automatycznie, razem ze skryptami i danymi testowymi. Tylko w widoku pełnym.</>],
  ],
  sections: (k) => (
    <>
      Każda sekcja działa tak samo: tytuł, {k.b('ⓘ')} z wyjaśnieniem, akcje po prawej i – jeśli sekcja ma kilka widoków
      – rząd zakładek pod spodem. W sekcji {k.b('Kod')} zakładki to pliki. Widok pełny zbiera sekcje w grupy: jak
      ludzie trafiają do lambdy, gdzie powstają zmiany, program i jego dane oraz jak lambda działa.
    </>
  ),
  editorAside:
    'Ruch i log są trzymane w pamięci – do podglądu, nie do archiwizacji: po restarcie serwera zaczynają się od zera. Wersje i historia wdrożeń są zapisywane na stałe.',

  why: (k) => (
    <>
      Wersja to kod i opcjonalnie dwie notatki o nim: {k.b('specyfikacja')}, czyli czego chce użytkownik i dlaczego –
      najlepiej jego słowami, oraz {k.b('zmiana')}, czyli jedna linijka o tym, co robi ta wersja. Obie widać obok diffu w
      historii wersji, więc {k.em('dlaczego')} zostaje obok {k.em('co')} – dla ciebie i dla następnego agenta, który
      przeczyta historię, zanim cokolwiek zmieni.
    </>
  ),
  whySample: {
    specification: 'Księga gości, którą ludzie mogą podpisać; wpisy muszą przetrwać restart',
    change: 'Trzyma wpisy w bazie danych, żeby przetrwały restart',
  },
  why2: (k) => (
    <>
      Agenci przekazują te same dwa pola do {k.code('write_code')}. W sekcji {k.b('Kod')} o zmianę pyta okno
      zapisywania. Oba pola są opcjonalne. Za długi tekst nie jest odrzucany, tylko przycinany: specyfikacja do 4000
      znaków, zmiana do 500. Szkic ma własne dwa pola, a wersja, w którą zostanie scalony, je przejmuje.
    </>
  ),

  written: (k) => (
    <>
      Każda wersja przechowuje obok programu to, co o niej napisano: {k.b('dokumentację')} – czym jest aplikacja, dla
      kogo i po co, i dlaczego jest zbudowana właśnie tak – oraz {k.b('testy')}: jak automatycznie sprawdzić, że
      działa, razem ze skryptami i danymi testowymi. Agenci piszą je przy tworzeniu nowej lambdy i aktualizują przy
      każdej zmianie. Następny agent, który zmienia lambdę, najpierw je czyta, więc wie, do czego służy aplikacja i co
      musi działać dalej – czego sam kod nie mówi.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'czym jest aplikacja, dla kogo, co ludzie z nią robią i po co'],
    ['.lambda/docs/decisions.md', 'decyzje techniczne i dlaczego je podjęto'],
    ['.lambda/tests/README.md', 'jak aplikacja jest testowana automatycznie i jak uruchomić testy'],
    ['.lambda/tests/…', 'skrypty i dane testowe, których używają testy'],
  ],
  written2: (k) => (
    <>
      To pliki wersji jak wszystkie inne, w folderze {k.code('.lambda')}: historia pokazuje, co wersja w nich
      zmieniła, powrót do starszej wersji przywraca dokumentację, która była dla niej aktualna, a szkic ma własną
      kopię, która trafia online razem z nim. Nigdy nie są kompilowane ani serwowane i wliczają się do limitu zasobów
      wersji.
    </>
  ),
  written3: (k) => (
    <>
      W centrum sterowania sekcja {k.b('Dokumentacja')} pokazuje strony do przeczytania, a {k.b('Testy')} – jak
      aplikacja jest testowana i pliki, które leżą obok; wersję wybiera się tak samo jak w plikach. Stronę można tam
      też edytować – zapisanie tworzy kolejną wersję. Widok prosty nazywa dokumentację {k.b('O aplikacji')} i pokazuje
      tylko, do czego służy aplikacja – żeby to poprawić, powiedz agentowi.
    </>
  ),
  writtenAside:
    'Są pisane w języku, którego używasz w rozmowie z agentem, dla tego, kto jako następny zmieni aplikację – człowieka albo agenta. To nie kopia kodu, tylko to, do czego aplikacja służy i dlaczego.',

  features: (k) => (
    <>
      Zapisana wersja nigdy się nie zmienia – i właśnie dlatego każdą warto zachować: każdą można porównać i przywrócić
      online dokładnie taką, jaka była. Żeby zmienić lambdę, z której ludzie korzystają, utwórz zamiast tego{' '}
      {k.b('szkic')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Utwórz go w sekcji {k.b('Szkice')} albo na bazie dowolnej wersji. To kopia kodu, zasobów, dokumentacji i
        testów tej wersji oraz danych lambdy.
      </>
    ),
    (k) => (
      <>
        Zmieniaj go tyle razy, ile trzeba – w sekcji {k.b('Kod')} albo prosząc agenta. {k.b('Wdróż podgląd')} wrzuca
        go online pod osobnym adresem, {k.code('/features/…/')}, na jego własnej kopii danych. Odwiedzający lambdę nic
        z tego nie widzą, a nic, co zapisze szkic, nie trafia do danych lambdy.
      </>
    ),
    (k) => (
      <>
        {k.b('Scal')} go, gdy wszystko będzie gotowe: stanie się kolejną wersją razem ze swoimi notatkami i od razu
        trafi online, jeśli zechcesz. Szkic znika – razem z podglądem i kopią danych.
      </>
    ),
  ],
  featureSample: 'Ranking',
  featuresAside: () => (
    <>
      Nad kilkoma szkicami można pracować jednocześnie. Scalić można tylko szkic oparty na najnowszej wersji, żeby
      scalenie nigdy nie cofnęło wersji zapisanej po utworzeniu szkicu. Jeśli wcześniej scalono inny, przenieś jego
      zmiany – albo poproś o to agenta – a potem oprzyj szkic na najnowszej wersji. Nic nie scala się samo; tak ma być.
    </>
  ),

  files: (k) => (
    <>
      Typów nie trzeba dopisywać pod kodem, który ich używa. W sekcji {k.b('Kod')} kliknij {k.b('+')} obok plików, a nowy plik
      zostanie skompilowany obok snippetu, w tej samej przestrzeni nazw – nic nie trzeba importować. Nazwa bez
      rozszerzenia oznacza plik C#.
    </>
  ),

  page: 'Stronę można serwować na dwa sposoby, a do tego jest jeszcze trzeci – na to, co ludzie przesyłają obok niej.',
  inlineTitle: 'Jedna strona, wpisana w kod',
  inline: 'Wystarczy do czegoś małego. Strona jest częścią snippetu.',
  folderTitle: 'Folder z prawdziwymi plikami',
  folder:
    'Najlepszy wybór, gdy masz arkusz stylów i skrypt. Pliki dodajesz tak samo jak plik C#, a serwowane są dokładnie tak, jak je napiszesz. Nic ich nie kompiluje.',
  workspaceTitle: 'Przesłane pliki, z danych',
  workspace:
    'Na to, co przesyłają ludzie albo tworzy lambda – zdjęcia, dokumenty – serwowane obok aplikacji. Nie na strony samej aplikacji: ich miejsce jest w folderze z plikami, gdzie trafiają do wersji razem z kodem, który ich potrzebuje.',

  spa: (k) => (
    <>
      Drugi sposób, krok po kroku. Każde demo serwuje swoją stronę właśnie tak, z folderu {k.code('web')} – otwórz{' '}
      {k.link('/editor/demo-crud', 'demo-crud')}, żeby zobaczyć przykład. Dema są tylko do odczytu; ich klucz edytora to
      ich nazwa.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        W sekcji {k.b('Kod')} kliknij {k.b('+')} obok plików i wpisz {k.code('site/index.html')}. Ukośnik w nazwie
        umieszcza plik w folderze, a rozszerzenie mówi, jakim plikiem jest.
      </>
    ),
    (k) => (
      <>
        Tak samo dodaj {k.code('site/app.css')} i {k.code('site/app.js')}. Strona odwołuje się do nich po nazwie, np.{' '}
        {k.code('href="app.css"')}, bo folder to katalog główny serwowanych plików, a nie część adresu.
      </>
    ),
    (k) => (
      <>
        Pliki, które nie są tekstem, np. obrazek czy font, dodasz tak: otwórz dowolny plik w {k.code('site')} i kliknij
        przycisk przesyłania obok plików – plik trafi do tego samego folderu. PNG nie da się wpisać w edytor tekstu, więc
        to jedyna droga.
      </>
    ),
    (k) => <>W {k.code('lambda.cs')} serwuj ten folder:</>,
    (k) => (
      <>
        Kliknij {k.b('Wdróż')}. {k.code('site/index.html')} odpowiada pod {k.code('/')}, {k.code('site/app.css')} pod{' '}
        {k.code('/app.css')}, a na każdy adres, który nie pasuje do żadnego pliku, odpowiada strona. Dzięki temu frontend
        z własnym routingiem działa, nawet gdy ktoś odświeży głęboki link.
      </>
    ),
    () => <>Dodaj obok API, a strona będzie miała z czym rozmawiać:</>,
  ],

  storage: (k) => (
    <>
      Lambda trzyma pliki w dwóch miejscach, a edytor pokazuje je osobno: {k.b('Pliki')} to pliki wersji – program –
      a {k.b('Dane')} to obszar roboczy – to, co program przechowuje. Cała różnica polega na tym,{' '}
      {k.em('do kogo należą')}. Pliki wersji należą do tej wersji; dane należą do lambdy i wszystkie wersje je
      współdzielą.
    </>
  ),
  savedWithCode: 'W wersji',
  workspaceColumn: 'W danych',
  table: [
    ['co zawiera', 'kod i zasoby: program, łącznie z frontendem – oraz jego dokumentacja i testy', 'wszystko, co zapisze lambda albo ktoś prześle'],
    ['kiedy się zmienia', 'nigdy – zmiana to nowa wersja', 'w chwili, gdy coś zostanie zapisane'],
    ['wdrożenie', 'wrzuca online dokładnie te pliki', 'nigdy ich nie rusza'],
    ['powrót do starszej wersji', 'przywraca stare pliki', 'bez wpływu: wszystkie wersje je współdzielą'],
    ['szkic', 'zaczyna jako ich kopia', 'działa na ich kopii'],
    ['kiedy znika', 'razem ze starymi wersjami, po przekroczeniu limitu', 'razem z lambdą albo gdy je wyłączysz'],
  ],
  reachedAs: 'dostęp z kodu przez',
  storageAside:
    'Nie mogą być jednym miejscem. Gdyby były, każde wdrożenie albo kasowałoby wszystko, co lambda zapisała od poprzedniego, albo z tego, co wdrażasz, nie dałoby się nigdy niczego usunąć. Gra z rankingiem potrzebuje tego drugiego, a strona, którą serwuje – pierwszego. Dlatego strona trafia do wersji, a ranking do danych.',

  database: (k) => (
    <>
      Rekordy – wpisy, konta, zamówienia, głosy – należą do {k.b('bazy danych')}: własnej bazy SQLite lambdy, którą
      włączasz w sekcji {k.b('Dane')}. Kod otwiera połączenie przez {k.code('Database.GetConnection()')} i czyta oraz
      zapisuje dane za pomocą {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, z własnym
      kontekstem, który mapuje tabele:
    </>
  ),
  database2: (k) => (
    <>
      Jej tabele tworzą {k.b('migracje')}: pliki SQL dostarczane z wersją w {k.code('migrations/')}, które{' '}
      {k.link('https://evolve-db.netlify.app/', 'Evolve')} stosuje po kolei przy starcie lambdy – każdą tylko raz, więc
      nowa wersja uruchamia zawsze tylko to, co nowe. Nigdy nie zmieniaj migracji, która została już zastosowana;
      zmiana tabeli to kolejny plik.
    </>
  ),
  database3: (k) => (
    <>
      Jak wszystkie dane, baza danych jest wspólna dla wszystkich wersji, wdrożenia i powroty do starszej wersji jej nie
      ruszają, a szkic pracuje na jej kopii. W sekcji {k.b('Dane')} widzisz jej tabele i to, co w nich jest – widok
      uproszczony nazywa to wpisami. {k.b('Pobierz jako projekt .NET')} dołącza ją jako zwykły plik SQLite.
    </>
  ),
  databaseAside: (k) => (
    <>
      Twórz kontekst tam, gdzie go potrzebujesz, i zwalniaj go po użyciu, a korzystaj z niego synchronicznie –{' '}
      {k.code('ToList')} i {k.code('SaveChanges')}, a nie {k.code('ToListAsync')} i {k.code('SaveChangesAsync')}.
      Tabele tworzą migracje, nigdy Entity Framework. Demo {k.link('/editor/demo-crud', 'demo-crud')} robi to wszystko.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} to prywatny katalog, w którym lambda może czytać i zapisywać: miejsce na pliki – zdjęcia,
      które ktoś przesyła, dokument, który lambda tworzy, model, który wczytuje. Rekordy należą do bazy danych, a to, co
      wiadomo o pliku – kto go przesłał i kiedy – też jest rekordem.
    </>
  ),
  keeping2: (k) => (
    <>
      Są też {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} oraz {k.code('Tree')}/{k.code('Files')}/{k.code('App')} do serwowania. Nic więcej w
      systemie plików nie jest dostępne.
    </>
  ),

  secrets: (k) => (
    <>
      Klucz API, hasło czy token należy do {k.b('sekretów')}, a nie do kodu – tam miałaby go każda wersja, każde
      pobranie i każdy, kto czyta historię. Kod odczytuje sekret po nazwie:
    </>
  ),
  secrets2: (k) => (
    <>
      Włącz sekrety w sekcji {k.b('Dane')} i ustaw tam wartość. Po zapisaniu nigdy nie jest już pokazywana – ani tobie,
      ani agentowi; możesz ją tylko zastąpić. Lista pokazuje, które nazwy kod odczytuje, choć nie mają jeszcze wartości,
      a przegląd o nie prosi. {k.code('Secret.Exists')} mówi, czy sekret jest ustawiony – dla kodu, który działa i bez
      niego. Jak wszystkie dane, sekrety są wspólne dla wszystkich wersji, a szkic pracuje na kopii.
    </>
  ),
  secretsAside: (k) => (
    <>
      Są przechowywane w postaci zaszyfrowanej, kluczem, którego nie ma w bazie danych. W pobranym projekcie{' '}
      {k.code('Secret.Read("NAME")')} odczytuje zmienną środowiskową {k.code('NAME')} – same wartości zostają tutaj.
    </>
  ),

  sockets: (k) => (
    <>
      Obsługiwane – i to nie na doczepkę. Demo {k.link('/editor/demo-game', 'demo-game')} łączy graczy w pary i prowadzi
      każdą grę na serwerze. Najprostsza wersja to trzy callbacki:
    </>
  ),
  socketsAside: (k) => (
    <>
      Jedna rzecz zaskakuje każdego: przeglądarka nie może ustawić nagłówków przy nawiązywaniu połączenia websocket.
      Przekaż to, czego potrzebuje handler, w query stringu, skąd odczyta to przez{' '}
      {k.code('connection.Request.Header.Query')}, albo wyślij sekrety w pierwszej wiadomości.
    </>
  ),

  limits:
    'Twój kod działa na wspólnym serwerze, więc część C# jest odrzucana jeszcze przed kompilacją: uruchamianie procesów, otwieranie własnych socketów, ładowanie assembly, sięganie do systemu plików poza obszarem roboczym i refleksja użyta, żeby to wszystko obejść. Tak samo czekanie na zadanie przez .Result lub .Wait() zamiast await: żądania działają na jednym wątku na rdzeń, a zadanie musiałoby się zakończyć na tym samym wątku, który na nie czeka.',
  limits2:
    'Cała reszta jest dostępna, łącznie z pełnym API modułów GenHTTP. Jeśli coś zostanie odrzucone, dowiesz się, w której linii i dlaczego – a nie tylko, że się nie udało.',

  away: (k) => (
    <>
      {k.b('Pobierz jako projekt .NET')} w menu edytora daje ci całość: solution, które możesz otworzyć, uruchomić przez{' '}
      {k.code('dotnet run')} i zachować na zawsze. Potrzebuje tylko pakietu GenHTTP, a w zestawie jest{' '}
      {k.code('Dockerfile')}, żeby zbudować i uruchomić je jako kontener.
    </>
  ),
  away2: (k) => (
    <>
      Twój snippet staje się plikiem {k.code('Project.cs')}, a {k.code('Program.cs')} serwuje to, co snippet zwraca.
      Pozostałe pliki trafiają do projektu bez zmian. {k.code('Workspace')} i {k.code('Assets')} stają się
      dwoma folderami obok programu, z tymi samymi metodami, osobno w folderze {k.code('Platform')}, więc w kodzie nie
      trzeba nic zmieniać.
      {' '}{k.code('Secret')} odczytuje tam zmienne środowiskowe o tej samej nazwie; wartości zostają tutaj.
      Dokumentacja i testy trafiają do folderów {k.code('docs')} i {k.code('tests')}.
      {' '}{k.code('Database')} otwiera {k.code('database/database.db')} – pobrany projekt zawiera ten plik razem z
      rekordami, które zapisała twoja aplikacja.
    </>
  ),
  awayAside:
    'Warto to wiedzieć, zanim cokolwiek tu zbudujesz: to, co piszesz, należy do ciebie i możesz to zabrać w całości. To, że kod działa na tej maszynie, w niczym go do niej nie przywiązuje.',

  open: (k) => (
    <>
      Jeśli to, co zbudujesz, może pomóc komuś innemu, opublikuj kod: otwórz sekcję {k.b('Open source')} w centrum
      sterowania, wybierz licencję – MIT, chyba że wolisz inną – i włącz publikację. Kod dostanie własną stronę wśród{' '}
      {k.link('/source', 'aplikacji open source')}, gdzie każdy może go przeczytać, dać mu gwiazdkę i pobrać dowolną
      wersję jako ten sam projekt, który daje {k.b('Pobierz jako projekt .NET')}, razem z licencją.
    </>
  ),
  open2: () => (
    <>
      Publikowana jest każda wersja, także wcześniejsze, razem z dokumentacją, testami i zmianą, którą wprowadziła.
      To, co aplikacja przechowuje – jej rekordy, zapisane pliki, wartości kluczy i haseł – nigdy nie jest publikowane,
      podobnie jak twoje prośby, sformułowane twoimi słowami, i to, kto korzysta z aplikacji. Po wyłączeniu strona
      znika; gwiazdki zostają zachowane na wypadek ponownej publikacji.
    </>
  ),
  openAside:
    'Wszystko, co jest w kodzie, staje się publiczne, łącznie z wcześniejszymi wersjami. Miejsce klucza czy hasła jest wśród kluczy i haseł w sekcji Dane, nigdy w kodzie – opublikowanym czy nie.',

  agents: (k) => (
    <>
      Pod {k.code('/mcp')} działa endpoint MCP. Podłącz do niego agenta, a zrobi wszystko to, co edytor: przeczyta
      dokumentację, przeczyta całe demo, zapisze pliki, skompiluje je i wdroży. Pod spodem to samo API.
    </>
  ),
  agents2: (k) => (
    <>
      Przy okazji mówi, dlaczego coś robi – {k.code('write_code')} przyjmuje specyfikację i zmianę – i może sprawdzić, co
      wdrożył: {k.code('read_logs')} zwraca ostatnie żądania lambdy, to, co wypisała, i stack trace każdego wyjątku. Tak
      agent dowiaduje się, że jego kod działa, zamiast to zakładać. Ty widzisz to samo w centrum sterowania. W trakcie
      pracy pisze dokumentację i testy, czyta je, zanim cokolwiek zmieni, i uruchamia testy pod adresem szkicu, zanim
      wrzuci szkic online. Strona, którą ludzie mają znaleźć, dostaje tytuł, opis, ikonę i podgląd, który widać, gdy
      ktoś udostępnia link do niej.
    </>
  ),
  more: 'Więcej o tym →',
  make: 'Utwórz lambdę',
};
