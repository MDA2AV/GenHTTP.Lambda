import type { SourceMessages } from '../en/source';
import { counted } from './plural';

/** Teksty stron z opublikowanym kodem – /source i każdego projektu pod nim – po polsku. */
export const source: SourceMessages = {
  shell: {
    section: 'Open source',
    home: 'GenHTTP Lambda – strona główna',
  },

  lambda: {
    label: 'Czym jest lambda?',
    text: 'Aplikacja webowa na GenHTTP Lambda: ktoś opisuje, czego chce, agent AI pisze ją w C#, a po kilku minutach działa pod własnym adresem – każda wersja jest zachowana razem z tym, co zmieniła.',
    build: 'Zbuduj własną',
  },

  catalog: {
    eyebrow: 'Open source',
    title: 'Zobacz, jak powstały tutejsze aplikacje',
    intro:
      'Lambdy, których właściciele opublikowali kod: każda wersja, to, co zmieniła, dokumentacja i testy. Czytaj go tutaj albo pobierz projekt, który uruchomisz wszędzie tam, gdzie działa .NET.',
    searchLabel: 'Szukaj w projektach',
    searchPlaceholder: 'Szukaj po nazwie albo po tym, co robi',
    orderLabel: 'Kolejność',
    orders: {
      stars: 'Najwięcej gwiazdek',
      updated: 'Ostatnio zmienione',
      published: 'Ostatnio opublikowane',
    },
    counted: (total) => counted(total, 'projekt', 'projekty', 'projektów'),
    failed: 'Nie udało się załadować projektów.',
    loadingMore: 'Ładowanie kolejnych…',
    showMore: 'Pokaż więcej',
    nothingTitle: 'Nic jeszcze nie opublikowano',
    nothing: (tab) => (
      <>
        Masz coś, z czego inni mogą się czegoś nauczyć? Otwórz centrum sterowania, wybierz {tab('Open source')} i
        licencję, a kod pojawi się tutaj.
      </>
    ),
    noMatchTitle: 'Nic nie pasuje',
    noMatch: (query) => `Żaden opublikowany projekt nie zawiera frazy „${query}”.`,
    clear: 'Pokaż wszystkie projekty',
    yoursTitle: 'Opublikuj swój kod',
    yours: (tab) => (
      <>
        Otwórz centrum sterowania swojej lambdy i wybierz {tab('Open source')} albo poproś agenta, który ją zbudował,
        żeby ją opublikował. Może to zrobić tylko osoba z kluczem edytora, na wybranej przez siebie licencji – a to, co
        aplikacja przechowuje, czyli jej rekordy, pliki i klucze, nigdy nie jest publikowane.
      </>
    ),
    build: 'Zbuduj coś',
    online: 'Online',
    offline: 'Offline',
    changed: (ago) => `zmieniono ${ago}`,
    stars: (count) => counted(count, 'gwiazdka', 'gwiazdki', 'gwiazdek'),
  },

  project: {
    loading: 'Ładowanie kodu…',
    failed: 'Nie udało się załadować kodu.',
    missingTitle: 'Nie ma tu opublikowanego kodu',
    missing: 'Być może właściciel wycofał publikację albo pod tym adresem nigdy nie było lambdy.',
    all: 'Wszystkie projekty',
    by: (name) => `autor: ${name}`,
    versions: (count) => counted(count, 'wersja', 'wersje', 'wersji'),
    onlineAt: (address) => <>Online pod adresem {address}</>,
    offline: 'Teraz offline',
    openApp: 'Otwórz aplikację',
    opens: (address) => `Otwiera ${address} w nowej karcie`,
    published: (ago) => `Opublikowano ${ago}`,
    changed: (ago) => `Zmieniono ${ago}`,
    picture: (name) => `Tak wygląda ${name}`,
    tabsLabel: 'Co czytać',
    tabs: {
      code: 'Kod',
      docs: 'Dokumentacja',
      tests: 'Testy',
      changes: 'Zmiany',
    },
  },

  versions: {
    label: 'Wersja',
    choose: 'Czytaj inną wersję',
    newest: 'najnowsza',
    online: 'online',
    older: (version, ago, newest) => `Czytasz wersję ${version}, zapisaną ${ago}. Najnowsza to wersja ${newest}.`,
    toNewest: 'Czytaj najnowszą',
    noChange: 'Bez notatki o tym, co zmieniła',
  },

  star: {
    star: 'Gwiazdka',
    add: 'Daj temu projektowi gwiazdkę',
    remove: 'Cofnij swoją gwiazdkę',
    count: (count) => counted(count, 'gwiazdka', 'gwiazdki', 'gwiazdek'),
    failed: 'Nie udało się zapisać gwiazdki.',
  },
  clone: {
    button: 'Kod',
    title: 'Sklonuj przez git',
    what: (oldest, newest) =>
      oldest === newest
        ? `Jej wersja to commit na main, oznaczony v${newest}.`
        : `Każda wersja trafia do klonu jako commit na main, oznaczony od v${oldest} do v${newest} - main jest najnowszy.`,
    readOnly:
      'Tylko do odczytu. Żeby na tym budować, zacznij własną lambdę i przenieś tam te pliki - AGENTS.md w klonie mówi jak, a licencja, co wolno.',
  },

  download: {
    title: (version) => `Wersja ${version} jako projekt`,
    what:
      'Projekt .NET 10 z plikiem Dockerfile, dokumentacją, testami i licencją. To, co aplikacja przechowuje – jej rekordy, zapisane pliki, klucze – nie wchodzi w jego skład.',
    zip: 'Pobierz ZIP',
    preparing: 'Przygotowywanie projektu…',
    slow: 'Wersja pobierana po raz pierwszy jest pakowana, gdy czekasz.',
    failed: 'Nie udało się przygotować projektu. Spróbuj ponownie za chwilę.',
    run: 'Uruchom go',
    local: 'Z zainstalowanym .NET 10 SDK:',
    container: 'Albo w kontenerze:',
    agent: 'Albo przekaż folder swojemu agentowi kodującemu i rozwijaj projekt dalej – zgodnie z jego licencją.',
    copy: 'Kopiuj',
    copied: 'Skopiowano',
  },

  tree: {
    label: 'Pliki',
    files: (count) => counted(count, 'plik', 'pliki', 'plików'),
    packing: 'Pakowanie tej wersji…',
    packingSlow: 'Wersja jest pakowana, gdy ktoś czyta ją po raz pierwszy – przy dużej trwa to chwilę.',
    failed: 'Nie udało się załadować plików tej wersji.',
    legend: 'Co jest czym',
    kinds: {
      code: 'Własny kod lambdy',
      asset: 'To, co serwuje: strony, skrypty, style, obrazki – i migracje bazy danych',
      docs: 'Czym jest i dlaczego jest zbudowana właśnie tak',
      tests: 'Jak jest testowana',
      dev: 'Z czego zbudowane są jego zasoby: projekt jego frontendu',
      platform: 'To, co zastępuje platformę',
      project: 'Host, build, kontener i licencja',
    },
    short: {
      code: 'Kod',
      asset: 'Serwowane',
      docs: 'Dokumentacja',
      tests: 'Testy',
      dev: 'Dev',
      platform: 'Platforma',
      project: 'Projekt',
    },
  },

  file: {
    loading: 'Ładowanie…',
    failed: 'Nie udało się załadować tego pliku.',
    missing: (path) => `W tej wersji nie ma ${path}.`,
    binary: 'Ten plik nie jest tekstem.',
    tooLarge: 'Ten plik jest za długi, żeby go tu pokazać.',
    download: 'Pobierz',
    raw: 'Surowy',
    rawTitle: 'Otwórz plik w oryginalnej postaci',
    copy: 'Kopiuj',
    copied: 'Skopiowano',
    lines: (count) => counted(count, 'linia', 'linie', 'linii'),
    plain: 'Pokazany bez kolorów, bo jest długi.',
    line: (line) => `Linia ${line}`,
  },

  docs: {
    pages: 'Strony',
    product: 'Czym jest',
    decisions: 'Decyzje',
    loading: 'Ładowanie…',
    failed: 'Nie udało się załadować tej strony.',
    noneTitle: 'O tej wersji nic nie napisano',
    none: 'Jej dokumentacja byłaby w docs/: czym jest aplikacja, dla kogo i dlaczego jest zbudowana właśnie tak.',
  },

  tests: {
    files: 'Skrypty i dane',
    noneTitle: 'Ta wersja nic nie mówi o swoich testach',
    none: 'To, jak aplikacja jest testowana, byłoby opisane w tests/README.md, a obok leżałyby skrypty, które uruchamiają testy.',
  },

  changes: {
    title: 'Wszystkie wersje, od najnowszej',
    intro: 'Zapisana wersja nigdy się nie zmienia. Każda mówi w jednej linijce, co zmieniła.',
    agent: 'Napisane przez agenta',
    online: 'online',
    browse: 'Czytaj kod',
    noChange: 'Bez notatki',
  },

  licenses: {
    MIT: 'Każdy może używać kodu, zmieniać go i przekazywać dalej, w czymkolwiek, o ile licencja i informacja o prawach autorskich pozostaną przy nim.',
    'Apache-2.0': 'Jak MIT, z licencją patentową od każdego, kto coś wniósł, i ze zmianami oznaczonymi jako zmiany.',
    'BSD-3-Clause': 'Jak MIT, a do tego nikt nie może używać nazwisk autorów, żeby promować to, co z niego zrobił.',
    'MPL-2.0': 'Zmiany w tych plikach pozostają na tej samej licencji; można je łączyć z kodem na dowolnej innej.',
    'GPL-3.0-or-later': 'Kto przekazuje kod dalej, zmieniony czy nie, przekazuje też jego źródła na tej samej licencji.',
    'AGPL-3.0-or-later': 'Jak GPL, a udostępnianie zmienionej kopii ludziom przez sieć też liczy się jako przekazanie dalej.',
    Unlicense: 'Przekazany do domeny publicznej: każdy może z nim zrobić wszystko, bez żadnych warunków.',
  },

  kinds: {
    Permissive: 'Liberalna',
    Copyleft: 'Copyleft',
    PublicDomain: 'Domena publiczna',
  },
};
