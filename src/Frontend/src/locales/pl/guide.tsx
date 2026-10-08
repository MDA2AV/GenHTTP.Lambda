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
    files: 'Kod i zasoby',
    page: 'Serwowanie strony',
    spa: 'Frontend krok po kroku',
    built: 'Z czego jest zbudowane',
    storage: 'Wersja i jej dane',
    database: 'Przechowywanie rekordów',
    keeping: 'Przechowywanie plików',
    secrets: 'Klucze i hasła',
    sockets: 'Websockety',
    limits: 'Czego nie da się zrobić',
    away: 'Zabierz kod ze sobą',
    git: 'Praca z git',
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
        To kompletna lambda. Po wdrożeniu odpowiada pod własnym adresem utworzonym od jej klucza, takim jak{' '}
        {k.code('your-key.genhttp.run')}, a na każde żądanie tam odpowiada słowem hello.
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
          Napisz, co ma być inaczej, a agent na tym serwerze zrobi to na twoich oczach. Wypróbowuje zmianę w szkicu –
          kopii z własnym adresem – i udostępnia ją, gdy działa. Wyłącz {k.b('Udostępnij po zakończeniu')}, jeśli chcesz
          najpierw samodzielnie wypróbować szkic.
          Zajmuje się tylko twoją aplikacją: prośbę, która jej nie dotyczy albo ma komuś zaszkodzić, odrzuca i mówi dlaczego.
        </>
      ),
    ],
    ['Szkice', () => <>Zmiany wypróbowywane, zanim zostaną udostępnione, każda pod własnym adresem i na własnych danych testowych. Otwarty szkic ma własny kod, dane testowe i logi. Sekcja pojawia się, gdy istnieje szkic.</>],
    ['Dane', () => <>To, co lambda przechowuje w trakcie działania, wspólne dla wszystkich wersji: baza danych, obszar roboczy i sekrety, każde na własnej karcie. Przeglądaj tabele i pliki, przesyłaj pliki, ustawiaj sekrety albo włączaj i wyłączaj dany rodzaj. Widok uproszczony pokazuje tę sekcję, gdy tylko aplikacja coś przechowuje.</>],
    ['Wersje', () => <>Co zmieniła każda wersja, o co proszono i czym różni się od poprzedniej. Stąd wdrażasz wersję albo wracasz do starszej – albo tworzysz szkic na bazie dowolnej z nich.</>],
    ['Wdrożenia', () => <>Co i kiedy było online – i co to wyłączyło.</>],
    ['Statystyki', () => <>Żądania, błędy, czasy odpowiedzi i najczęściej odwiedzane ścieżki z ostatniej godziny albo ostatnich 24 godzin.</>],
    ['Logi', () => <>Żądania, to, co lambda wypisała, i stack trace każdego błędu – na bieżąco.</>],
    [
      'Kod',
      (k) => (
        <>
          Każdy plik wersji, jej kod i zasoby, w drzewie obok edytora – razem z informacją, ile miejsca zajmują i czy
          są publicznie dostępne. Wybierz starszą wersję nad nim, aby ją przeczytać. {k.b('Sprawdź')} kompiluje,{' '}
          {k.b('Zapisz')} tworzy wersję, {k.b('Wdróż')} wrzuca kod online. W szkicu {k.b('Zapisz')} zostawia kod w
          szkicu i pokazuje go pod adresem szkicu. {k.code('Ctrl-S')} zapisuje; {k.code('F12')} przechodzi do deklaracji.
        </>
      ),
    ],
    ['Testy', () => <>Jak aplikacja jest testowana automatycznie, razem ze skryptami i danymi testowymi. Tylko w widoku pełnym.</>],
  ],
  sections: (k) => (
    <>
      Każda sekcja działa tak samo: tytuł, {k.b('ⓘ')} z wyjaśnieniem, akcje po prawej i – jeśli sekcja ma kilka widoków
      – rząd zakładek pod spodem. Widok pełny zbiera sekcje w grupy: jak ludzie trafiają do lambdy, gdzie powstają
      zmiany, wersje i dane oraz jak lambda działa.
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
    ['docs/product.md', 'czym jest aplikacja, dla kogo, co ludzie z nią robią i po co'],
    ['docs/decisions.md', 'decyzje techniczne i dlaczego je podjęto'],
    ['tests/README.md', 'jak aplikacja jest testowana automatycznie i jak uruchomić testy'],
    ['tests/…', 'skrypty i dane testowe, których używają testy'],
  ],
  written2: (k) => (
    <>
      To pliki jego kodu jak wszystkie inne, w folderach {k.code('docs')} i {k.code('tests')}: historia pokazuje, co
      wersja w nich zmieniła, powrót do starszej wersji przywraca dokumentację, która była dla niej aktualna, a szkic ma
      własną kopię, która trafia online razem z nim. Nigdy nie są kompilowane ani serwowane i wliczają się do tego, ile
      może zajmować wersja.
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
      online dokładnie taką, jaka była. Żeby zmienić lambdę, z której ludzie korzystają, najpierw wypróbuj zmianę w{' '}
      {k.b('szkicu')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Zacznij od dowolnej wersji w sekcji {k.b('Wersje')} albo poproś agenta, żeby utworzył szkic. To kopia kodu i
        zasobów tej wersji, w tym jej dokumentacji i testów, oraz danych lambdy.
      </>
    ),
    (k) => (
      <>
        Zmieniaj go tyle razy, ile trzeba – w sekcji {k.b('Kod')} albo prosząc agenta. Jego podgląd odpowiada pod osobnym
        adresem, {k.code('/features/…/')}, na własnych danych testowych. Odwiedzający lambdę nic z tego nie widzą, a nic,
        co zapisze szkic, nie trafia do danych lambdy.
      </>
    ),
    (k) => (
      <>
        {k.b('Udostępnij')} szkic, gdy będzie gotowy: stanie się kolejną wersją razem ze swoimi notatkami i trafi online.
        Szkic znika razem z podglądem i danymi testowymi.
      </>
    ),
  ],
  featureSample: 'Ranking',
  featuresAside: () => (
    <>
      Nad kilkoma szkicami można pracować jednocześnie. Online może trafić tylko szkic aktualny względem najnowszej
      wersji, żeby nigdy nie cofnął wersji zapisanej po jego utworzeniu. Gdy wcześniej udostępniono inny, przenieś jego
      zmiany – albo poproś o to agenta – i oznacz szkic jako aktualny. Nic nie trafia online samo; tak ma być. API
      nazywa szkic „feature”, a udostępnienie go „merge”.
    </>
  ),

  files: (k) => (
    <>
      Wersja to dowolna liczba plików, w dwóch częściach. Jej {k.b('kod')} to każdy plik poza zasobami: jej pliki{' '}
      {k.code('.cs')} są kompilowane, w dowolnym folderze, jak w każdym projekcie C#, a każdy inny plik jest
      przechowywany z wersją i nigdy nie jest kompilowany ani serwowany: jej dokumentacja, testy, to, z czego budowany
      jest frontend. Jej {k.b('zasoby')}, w {k.code('resources/')}, to to, co czyta i serwuje w trakcie działania –
      strony, skrypty, style, obrazy, migracje bazy danych – dostępne z kodu jako {k.code('Resources')}.
    </>
  ),
  files2: (k) => (
    <>
      Typów nie trzeba dopisywać pod kodem, który ich używa. W sekcji {k.b('Kod')} kliknij {k.b('+')} obok kodu i wpisz
      nazwę: plik {k.code('.cs')} zostanie skompilowany obok snippetu, w tej samej przestrzeni nazw, w którym folderze
      by się nie znajdował – nic nie trzeba importować. Nazwa bez rozszerzenia i bez folderu oznacza plik C#.
    </>
  ),
  filesAside:
    'To, jak ułożony jest kod, zależy od tego, kto go pisze – folder na typy, folder na źródła frontendu, folder na skrypty. Każdy plik .cs jest kompilowany do aplikacji, więc C#, który nie jest jej częścią – test, własne narzędzie – nie należy do kodu jako plik .cs. Kod i zasoby wersji dzielą jedną pulę miejsca, którą pokazuje przegląd.',

  page: 'Stronę można serwować na dwa sposoby, a do tego jest jeszcze trzeci – na to, co ludzie przesyłają obok niej.',
  inlineTitle: 'Jedna strona, wpisana w kod',
  inline: 'Wystarczy do czegoś małego. Strona jest częścią snippetu.',
  folderTitle: 'Folder z prawdziwymi plikami',
  folder:
    'Najlepszy wybór, gdy masz arkusz stylów i skrypt. Pliki są zasobami wersji i serwowane są dokładnie tak, jak je napiszesz. Nic ich nie kompiluje.',
  workspaceTitle: 'Przesłane pliki, z danych',
  workspace:
    'Na to, co przesyłają ludzie albo tworzy lambda – zdjęcia, dokumenty – serwowane obok aplikacji. Nie na strony samej aplikacji: ich miejsce jest w zasobach, gdzie trafiają do wersji razem z kodem, który ich potrzebuje.',

  spa: (k) => (
    <>
      Drugi sposób, krok po kroku. Każde demo serwuje swoją stronę właśnie tak, z {k.code('resources/web')} – otwórz{' '}
      {k.link('/editor/demo-crud', 'demo-crud')}, żeby zobaczyć przykład. Dema są tylko do odczytu; ich klucz edytora to
      ich nazwa.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        W sekcji {k.b('Kod')} kliknij {k.b('+')} obok zasobów i wpisz {k.code('site/index.html')}: powstanie{' '}
        {k.code('resources/site/index.html')}. Ukośnik w nazwie umieszcza plik w folderze, a rozszerzenie mówi, jakim
        plikiem jest.
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
        przycisk przesyłania obok zasobów – plik trafi do tego samego folderu. PNG nie da się wpisać w edytor tekstu, więc
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
  built: (k) => (
    <>
      Część lambdy może powstawać w narzędziu do budowania, zamiast być pisana tak, jak jest serwowana lub kompilowana:
      kompilowana, pakowana lub generowana. Wersja zawiera to, co narzędzie wytwarza – jako swoje zasoby albo jako swój
      kod – a pliki, z których to powstaje, są częścią jej kodu, w osobnym folderze: na przykład {k.code('frontend/')},
      z README, które mówi, jak to się buduje. Twój agent zmienia te pliki, uruchamia build u siebie i zapisuje jedno i
      drugie w tej samej wersji. Ta platforma niczego nie buduje.
    </>
  ),
  built2: (k) => (
    <>
      Tak jak dokumentacja należą do wersji: są porównywane w historii, przywracane, kopiowane do szkicu, klonowane,
      pobierane i publikowane razem z resztą kodu – i nigdy nie są kompilowane ani serwowane. W centrum sterowania są w
      sekcji {k.b('Kod')}, razem z każdym innym plikiem wersji.
    </>
  ),
  builtAside:
    'To, co jest pisane tak, jak jest serwowane lub kompilowane, żadnego takiego folderu nie potrzebuje. To, co build instaluje lub zachowuje dla siebie – na przykład node_modules – nigdy nie jest częścią wersji: pilnuje tego .gitignore w jego folderze.',

  storage: (k) => (
    <>
      Lambda trzyma pliki w dwóch miejscach, a edytor pokazuje je osobno: {k.b('Kod')} to pliki wersji – program –
      a {k.b('Dane')} to obszar roboczy – to, co program przechowuje. Cała różnica polega na tym,{' '}
      {k.em('do kogo należą')}. Pliki wersji należą do tej wersji; dane należą do lambdy i wszystkie wersje je
      współdzielą. Każde z nich ma jedną pulę miejsca: kod i zasoby wersji dzielą jedną, a baza danych i obszar
      roboczy lambdy – drugą.
    </>
  ),
  savedWithCode: 'W wersji',
  workspaceColumn: 'W danych',
  table: [
    ['co zawiera', 'kod i zasoby: program, łącznie z frontendem – oraz jego dokumentacja, testy i to, z czego jest zbudowany', 'wszystko, co zapisze lambda albo ktoś prześle'],
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
      Jej tabele tworzą {k.b('migracje')}: pliki SQL dostarczane z wersją w {k.code('resources/migrations/')}, które{' '}
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
  sockets2: (k) => (
    <>
      Gdy strona tylko nasłuchuje – licznik, kanał wiadomości, tablica wyników – prostsze są zdarzenia wysyłane przez
      serwer (server-sent events): jedna długa odpowiedź, do której serwer stale dopisuje, a przeglądarka łączy się
      ponownie sama. Demo {k.link('/editor/demo-live', 'demo-live')} wysyła w ten sposób każdy głos wszystkim, którzy
      je oglądają. Tak czy inaczej to serwer przesyła to, co się zmieniło. Strona, która co kilka sekund pyta od nowa,
      wysyła żądanie za każdym razem, niezależnie od tego, czy coś się zmieniło, i i tak jest spóźniona.
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
      Pozostałe pliki trafiają do projektu dokładnie tak, jak je napisano, i tam, gdzie były – zasoby do{' '}
      {k.code('resources')}, dokumentacja do {k.code('docs')}, testy do {k.code('tests')}. {k.code('Workspace')} i{' '}
      {k.code('Resources')} stają się dwoma folderami obok programu, z tymi samymi metodami, osobno w folderze{' '}
      {k.code('Platform')}, więc w kodzie nie trzeba nic zmieniać.
      {' '}{k.code('Secret')} odczytuje tam zmienne środowiskowe o tej samej nazwie; wartości zostają tutaj.
      {' '}{k.code('Database')} otwiera {k.code('database/database.db')} – pobrany projekt zawiera ten plik razem z
      rekordami, które zapisała twoja aplikacja.
    </>
  ),
  awayAside:
    'Warto to wiedzieć, zanim cokolwiek tu zbudujesz: to, co piszesz, należy do ciebie i możesz to zabrać w całości. To, że kod działa na tej maszynie, w niczym go do niej nie przywiązuje.',
  git: (k) => (
    <>
      Każda lambda jest też repozytorium git. {k.b('Sklonuj')}, na przeglądzie centrum sterowania i obok kodu, podaje
      jej adres - adres twojego edytora z nazwą aplikacji na końcu - a {k.code('git clone')} daje ci projekt, który
      daje {k.b('Pobierz')}, z każdą wersją jako commitem na {k.code('main')}, oznaczonym {k.code('v1')},{' '}
      {k.code('v2')} i tak dalej, i z każdym szkicem jako gałęzią. Otwórz go we własnym edytorze, daj go swojemu
      agentowi, uruchom przez {k.code('dotnet run')}.
    </>
  ),
  git2: (k) => (
    <>
      Wypchnij i jest tutaj. Każdy commit wypchnięty na {k.code('main')} staje się kolejną wersją, a jego pierwszy
      wiersz opisem zmiany - najpierw jest kompilowany i odrzucany, jeśli się nie kompiluje - a{' '}
      {k.code('git push -o deploy')} udostępnia go od razu. Wypchnięta gałąź staje się szkicem z podglądem online pod
      własnym adresem; wypchnij ją na {k.code('main')} albo dodaj {k.code('-o merge')} do ostatniego wypchnięcia, a
      stanie się kolejną wersją. To, co platforma dokłada do twojego kodu, żeby był projektem - {k.code('Program.cs')},
      plik projektu, {k.code('Platform')} - nie jest częścią twojej aplikacji, więc wypchnięcie, które to zmienia,
      zostaje odrzucone z wyjaśnieniem. {k.code('AGENTS.md')} w repozytorium podpowiada agentowi resztę.
    </>
  ),
  gitAside:
    'Adres zawiera twój link do edytora, tak jak adres edytora: kto go ma, może wypychać zmiany. To, co aplikacja zapisuje - jej wpisy, pliki, klucze i hasła - nigdy nie trafia do repozytorium.',

  open: (k) => (
    <>
      Jeśli to, co zbudujesz, może pomóc komuś innemu, opublikuj kod: otwórz sekcję {k.b('Open source')} w centrum
      sterowania, wybierz licencję – MIT, chyba że wolisz inną – i włącz publikację. Kod dostanie własną stronę wśród{' '}
      {k.link('/source', 'aplikacji open source')}, gdzie każdy może go przeczytać, dać mu gwiazdkę, pobrać dowolną
      wersję jako ten sam projekt, który daje {k.b('Pobierz')}, razem z licencją, albo sklonować każdą jego wersję
      przez git.
    </>
  ),
  open2: () => (
    <>
      Publikowana jest każda wersja, także wcześniejsze, razem z całym jej kodem – w tym z dokumentacją i testami – oraz
      zmianą, którą wprowadziła.
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
      ktoś udostępnia link do niej. Na dole stron, które buduje, dodaje małą linijkę z informacją, że powstały w GenHTTP
      Lambda – powiedz mu, jeśli wolisz jej nie mieć, a ją usunie.
    </>
  ),
  more: 'Więcej o tym →',
  make: 'Utwórz lambdę',
};
