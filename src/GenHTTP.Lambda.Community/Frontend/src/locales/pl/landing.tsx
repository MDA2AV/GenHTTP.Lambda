import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community edition',
  headline: 'Platforma, na której twoje agenty budują aplikacje.',
  headlineAccent: 'Open source, na własnym serwerze.',
  intro:
    'Opisz aplikację, a agent budujący ją napisze - albo zleć to Claude Code, Codex lub Cursor przez MCP. Każda aplikacja to handler GenHTTP w C#, kompilowany na tym serwerze i online w kilka sekund, z bazą danych, kluczami i hasłami, wersjami i gitem.',
  describe: 'Opisz aplikację',
  connect: 'Połącz swojego agenta',
  write: 'Napisz kod samodzielnie',

  buildTitle: 'Opisz ją',
  buildText: 'Wystarczy jedno zdanie. Agent budujący napisze aplikację, udostępni ją i da ci link, by później ją zmienić.',
  noAgent: 'Do tej instalacji nie jest podłączony żaden agent budujący. Połącz poniżej własnego agenta - zrobi to samo.',

  connectTitle: 'Połącz swojego agenta',
  connectText:
    'Twój agent do kodowania dostaje platformę jako narzędzia: tworzy aplikację, pisze i sprawdza jej kod, wypróbowuje zmianę w szkicu z kopią danych, udostępnia ją i czyta jej logi.',
  connectThen: 'Potem poproś go o coś, na przykład: „Zbuduj książkę gości ze stroną, na której zatwierdzam wpisy.”',

  featuresTitle: 'Co dostaje każda aplikacja',
  features: [
    { title: 'Adres', text: 'Własny adres na tym serwerze, nazwany od klucza aplikacji – a po przejściu na premium także własna domena.' },
    { title: 'Wpisy', text: 'Własna baza SQLite, tabele tworzone przez migracje, odczyt i zapis przez Entity Framework Core.' },
    { title: 'Pliki', text: 'Folder na to, co aplikacja zapisuje: przesłane pliki, eksporty, wszystko, co nie jest wpisem.' },
    { title: 'Klucze i hasła', text: 'Przechowywane na serwerze w zaszyfrowanej postaci. Agent mówi, których potrzebuje, ty je wpisujesz i nikt ich nie odczyta z powrotem.' },
    { title: 'Szkice i wersje', text: 'Zmianę wypróbowujesz w szkicu z kopią danych, a potem ją udostępniasz. Wcześniejsze wersje są zachowane, powrót jest o jedno kliknięcie.' },
    { title: 'Git', text: 'Każda aplikacja to repozytorium: sklonuj je, wypchnij commit, by zapisać wersję – z -o deploy od razu ją udostępnisz – albo gałąź, by zacząć szkic.' },
    { title: 'Odwiedzający', text: 'Wyświetlenia stron, skąd przyszli odwiedzający i z jakich krajów - liczone na serwerze, bez ciasteczek, po włączeniu.' },
    { title: 'Twój kod', text: 'Pobierz każdą aplikację jako zwykły projekt .NET i uruchom ją gdziekolwiek albo opublikuj jej źródła do wglądu dla innych.' },
  ],

  runsTitle: 'Jak to działa',
  runs: [
    { title: 'Jeden proces', text: 'Serwer, edytor, API i każda aplikacja działają w jednym procesie i trzymają wszystko w jednym folderze. Agent budujący, jeśli jest, działa obok.' },
    {
      title: 'Po ścieżce lub po hoście',
      text: 'Aplikacje odpowiadają pod /lambda/ na adresie instalacji – albo, gdy nadasz domenę dla nich, każda pod własną subdomeną.',
    },
    {
      title: 'Kod, któremu ufasz',
      text: 'Aplikacje są kompilowane do serwera, a ich kod jest sprawdzany pod kątem tego, czego nie wolno im robić - ale dzielą z nim proces. Uruchamiaj to, co uruchomiłbyś sam.',
    },
    { title: 'Open source', text: 'Licencja MIT: czytaj, zmieniaj i uruchamiaj, gdzie chcesz.' },
  ],
  repository: 'Źródła na GitHubie',

  aroundTitle: 'Rozejrzyj się',
  around: {
    demos: { title: 'Zacznij od dema', text: 'API na wpisach, logowanie, aktualizacje na żywo, gra: każde to gotowa aplikacja do skopiowania i zmiany.' },
    showcase: { title: 'Galeria', text: 'Aplikacje, które ich właściciele zdecydowali się tu pokazać.' },
    guide: { title: 'Przewodnik', text: 'Co aplikacja potrafi i jak, w kolejności, w jakiej tego potrzebujesz.' },
    api: { title: 'REST API', text: 'To samo co narzędzia, przez HTTP, opisane w OpenAPI.' },
    sources: { title: 'Opublikowane źródła', text: 'Aplikacje, których właściciele opublikowali kod, do czytania i pobrania.' },
    admin: { title: 'Administracja', text: 'Aplikacje, limity, blokady i log, za tokenem administratora.' },
  },
};
