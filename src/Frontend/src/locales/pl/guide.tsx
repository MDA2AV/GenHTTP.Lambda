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
    files: 'Więcej niż jeden plik',
    page: 'Serwowanie strony',
    spa: 'Frontend krok po kroku',
    storage: 'Dwa miejsca na pliki',
    keeping: 'Przechowywanie danych',
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
        Daj klucz edytora agentowi i powiedz, co ma zbudować – nowe wersje zapisuje przez {k.link('/#agents', 'MCP')}.
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
          Napisz, co ma być inaczej, a agent na tym serwerze zrobi to na twoich oczach: przeczyta kod, zmieni go,
          sprawdzi, czy się kompiluje, i wrzuci online jako nową wersję. Wyłącz {k.b('Wdróż po zakończeniu')}, jeśli
          chcesz najpierw przejrzeć zmianę.
        </>
      ),
    ],
    ['Pliki', () => <>Pliki danej wersji i dane lambdy – to, co zapisuje w trakcie działania. Kłódka albo globus pokazuje, czy są publicznie dostępne.</>],
    ['Wersje', () => <>Co zmieniła każda wersja, o co proszono i czym różni się od poprzedniej. Stąd wdrażasz wersję albo wracasz do starszej.</>],
    ['Wdrożenia', () => <>Co i kiedy było online – i co to wyłączyło.</>],
    ['Statystyki', () => <>Żądania, błędy, czasy odpowiedzi i najczęściej odwiedzane ścieżki z ostatniej godziny albo ostatnich 24 godzin.</>],
    ['Logi', () => <>Żądania, to, co lambda wypisała, i stack trace każdego błędu – na bieżąco.</>],
    [
      'Kod',
      (k) => (
        <>
          Tu piszesz kod ręcznie. {k.b('Sprawdź')} kompiluje, {k.b('Zapisz')} tworzy wersję, {k.b('Wdróż')} wrzuca ją online.{' '}
          {k.code('Ctrl-S')} zapisuje; {k.code('F12')} przechodzi do deklaracji.
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
      Agenci przekazują te same dwa pola do {k.code('write_code')}. W sekcji {k.b('Kod')} przy zapisie pojawia się
      pytanie o zmianę. Oba pola są opcjonalne. Za długi tekst nie jest odrzucany, tylko przycinany: specyfikacja do 4000
      znaków, zmiana do 500.
    </>
  ),

  files: (k) => (
    <>
      Typów nie trzeba dopisywać pod kodem, który ich używa. W sekcji {k.b('Kod')} kliknij {k.b('+')} obok plików, a nowy plik
      zostanie skompilowany obok snippetu, w tej samej przestrzeni nazw – nic nie trzeba importować. Nazwa bez
      rozszerzenia oznacza plik C#.
    </>
  ),

  page: 'Są trzy sposoby, a wybór zależy od tego, gdzie leży strona.',
  inlineTitle: 'Jedna strona, wpisana w kod',
  inline: 'Wystarczy do czegoś małego. Strona jest częścią snippetu.',
  folderTitle: 'Folder z prawdziwymi plikami',
  folder:
    'Najlepszy wybór, gdy masz arkusz stylów i skrypt. Pliki dodajesz tak samo jak plik C#, a serwowane są dokładnie tak, jak je napiszesz. Nic ich nie kompiluje.',
  workspaceTitle: 'Z obszaru roboczego',
  workspace: 'Gdy stronę przesyłasz, zamiast ją pisać, i chcesz ją zmieniać bez ponownego wdrażania.',

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
      Sekcja {k.b('Pliki')} pokazuje oba miejsca – pliki wersji i obszar roboczy jako {k.b('Dane')} – i mówi, które z
      nich są publicznie dostępne. Pliki kodu zmieniasz w sekcji {k.b('Kod')}; dane możesz przesyłać i usuwać w sekcji{' '}
      {k.b('Pliki')}. To jednak nie to samo, a cała różnica polega na tym, {k.em('kiedy każde z nich się zmienia')}.
    </>
  ),
  savedWithCode: 'Zapisywane z kodem',
  workspaceColumn: 'Obszar roboczy',
  table: [
    ['co zawiera', 'każdy plik lambdy, łącznie z C#', 'wszystko, co zostało zapisane lub przesłane'],
    ['kiedy się zmienia', 'gdy klikniesz Zapisz albo Wdróż', 'w chwili, gdy coś zostanie w nim zapisane'],
    ['wdrożenie', 'zastępuje całość', 'nigdy go nie rusza'],
    ['powrót do starszej wersji', 'przywraca stare pliki', 'bez wpływu'],
    ['klonowanie lambdy', 'są kopiowane', 'nie jest kopiowany'],
  ],
  reachedAs: 'dostęp z kodu przez',
  storageAside:
    'Nie mogą być jednym katalogiem. Gdyby były, każde wdrożenie albo kasowałoby wszystko, co lambda zapisała od poprzedniego, albo z tego, co wdrażasz, nie dałoby się nigdy niczego usunąć. Gra z rankingiem potrzebuje tego drugiego, a strona, którą serwuje – pierwszego.',

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
      {k.code('dotnet run')} i zachować na zawsze. Ma jedną referencję do pakietu i ani śladu tej platformy.
    </>
  ),
  away2: (k) => (
    <>
      Twój snippet staje się treścią {k.code('Program.cs')}, opakowaną w hosta, który serwuje to, co snippet zwraca.
      Pozostałe pliki trafiają do projektu bez zmian. {k.code('Workspace')} i {k.code('Assets')} stają się
      dwoma folderami obok kodu, z tymi samymi metodami, więc w kodzie nie trzeba nic zmieniać.
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
