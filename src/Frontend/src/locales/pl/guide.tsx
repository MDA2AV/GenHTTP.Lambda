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
    features: 'Bezpieczne zmiany',
    files: 'Więcej niż jeden plik',
    page: 'Serwowanie strony',
    spa: 'Frontend krok po kroku',
    storage: 'Dwa miejsca na pliki',
    keeping: 'Przechowywanie danych',
    secrets: 'Klucze i hasła',
    sockets: 'Websockety',
    limits: 'Czego nie da się zrobić',
    away: 'Zabierz kod ze sobą',
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
    ['Przegląd', () => <>Czy jest online, ile dziś było żądań i ile z nich się nie udało, ostatnia zmiana i ile zostało miejsca.</>],
    [
      'Zmień',
      (k) => (
        <>
          Napisz, co ma być inaczej, a agent na tym serwerze zrobi to na twoich oczach. Pracuje w szkicu, tam sprawdza
          zmianę, a gdy działa, scala szkic w kolejną wersję. Wyłącz {k.b('Wdróż po zakończeniu')}, jeśli chcesz
          najpierw samodzielnie wypróbować szkic.
        </>
      ),
    ],
    ['Szkice', () => <>Zmiany przygotowywane obok lambdy: każdą wypróbowuje się pod osobnym adresem i scala w kolejną wersję, gdy jest gotowa. Otwarty szkic ma własny kod, dane i logi.</>],
    ['Pliki', () => <>Pliki danej wersji: jej kod i zasoby, czyli sam program. Kłódka albo globus pokazuje, czy są publicznie dostępne.</>],
    ['Dane', () => <>To, co lambda przechowuje w trakcie działania, wspólne dla wszystkich wersji: obszar roboczy i sekrety, każde na własnej karcie. Przeglądaj je, przesyłaj pliki, ustawiaj sekrety albo włączaj i wyłączaj dany rodzaj. Widok uproszczony pokazuje tę sekcję, gdy tylko aplikacja coś przechowuje.</>],
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
  ],
  sections: (k) => (
    <>
      Każda sekcja działa tak samo: tytuł, {k.b('ⓘ')} z wyjaśnieniem, akcje po prawej i – jeśli sekcja ma kilka widoków
      – rząd zakładek pod spodem. W sekcji {k.b('Kod')} zakładki to pliki.
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
    change: 'Trzyma wpisy w obszarze roboczym, żeby przetrwały restart',
  },
  why2: (k) => (
    <>
      Agenci przekazują te same dwa pola do {k.code('write_code')}. W sekcji {k.b('Kod')} o zmianę pyta okno
      zapisywania. Oba pola są opcjonalne. Za długi tekst nie jest odrzucany, tylko przycinany: specyfikacja do 4000
      znaków, zmiana do 500. Szkic ma własne dwa pola, a wersja, w którą zostanie scalony, je przejmuje.
    </>
  ),

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
        Utwórz go w sekcji {k.b('Szkice')} albo na bazie dowolnej wersji. To kopia kodu i zasobów tej wersji oraz
        danych lambdy.
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
    ['co zawiera', 'kod i zasoby: program, łącznie z frontendem', 'wszystko, co zapisze lambda albo ktoś prześle'],
    ['kiedy się zmienia', 'nigdy – zmiana to nowa wersja', 'w chwili, gdy coś zostanie zapisane'],
    ['wdrożenie', 'wrzuca online dokładnie te pliki', 'nigdy ich nie rusza'],
    ['powrót do starszej wersji', 'przywraca stare pliki', 'bez wpływu: wszystkie wersje je współdzielą'],
    ['szkic', 'zaczyna jako ich kopia', 'działa na ich kopii'],
    ['kiedy znika', 'razem ze starymi wersjami, po przekroczeniu limitu', 'razem z lambdą albo gdy je wyłączysz'],
  ],
  reachedAs: 'dostęp z kodu przez',
  storageAside:
    'Nie mogą być jednym miejscem. Gdyby były, każde wdrożenie albo kasowałoby wszystko, co lambda zapisała od poprzedniego, albo z tego, co wdrażasz, nie dałoby się nigdy niczego usunąć. Gra z rankingiem potrzebuje tego drugiego, a strona, którą serwuje – pierwszego. Dlatego strona trafia do wersji, a ranking do danych.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} to prywatny katalog, w którym lambda może czytać i zapisywać. To miejsce na wszystko, co ma
      przetrwać dłużej niż jedno żądanie albo jedno wdrożenie.
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
    'Twój kod działa na wspólnym serwerze, więc część C# jest odrzucana jeszcze przed kompilacją: uruchamianie procesów, otwieranie własnych socketów, ładowanie assembly, sięganie do systemu plików poza obszarem roboczym i refleksja użyta, żeby to wszystko obejść.',
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
    </>
  ),
  awayAside:
    'Warto to wiedzieć, zanim cokolwiek tu zbudujesz: to, co piszesz, należy do ciebie i możesz to zabrać w całości. To, że kod działa na tej maszynie, w niczym go do niej nie przywiązuje.',

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
      agent dowiaduje się, że jego kod działa, zamiast to zakładać. Ty widzisz to samo w centrum sterowania.
    </>
  ),
  more: 'Więcej o tym →',
  make: 'Utwórz lambdę',
};
