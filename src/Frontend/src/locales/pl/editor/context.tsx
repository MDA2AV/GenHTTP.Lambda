import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Dokumentacja',
    titleSimple: 'O twojej aplikacji',
    hint: 'Czym jest ta aplikacja, dla kogo i po co – i dlaczego jest zbudowana właśnie tak. Agenci piszą ją przy każdej zmianie i jest zachowywana z każdą wersją, więc starsza wersja wraca z dokumentacją, która była wtedy aktualna.',
    hintSimple: 'Do czego służy twoja aplikacja i dlaczego – tak, jak agent zrozumiał to z twoich próśb. Aktualizuje to przy każdej zmianie.',
    inDraft: 'Dokumentacja tego szkicu. Stanie się dokumentacją twojej aplikacji, gdy szkic trafi online.',
    pages: { product: 'Produkt', decisions: 'Decyzje' },
    emptyTitle: 'Nic jeszcze nie napisano',
    emptyText: (code) => (
      <>
        Agenci piszą dokumentację razem ze swoimi zmianami: czym jest aplikacja, dla kogo i po co – w{' '}
        {code('.lambda/docs/product.md')}, a dlaczego jest zbudowana właśnie tak – w {code('decisions.md')}. Jest
        częścią wersji, obok kodu.
      </>
    ),
    emptySimpleTitle: 'O twojej aplikacji nic jeszcze nie napisano',
    emptySimple: 'Agent może opisać, do czego służy twoja aplikacja i dlaczego, na podstawie twoich próśb – a potem będzie ten opis aktualizować.',
    ask: 'Poproś agenta, żeby ją napisał',
    describe: 'Poproś agenta, żeby ją opisał',
    writePrompt: 'Napisz dokumentację tej aplikacji: czym jest, dla kogo i po co, oraz jakie decyzje techniczne za nią stoją.',
    describePrompt: 'Opisz, do czego służy ta aplikacja i dlaczego – do przeczytania w sekcji „O aplikacji”.',
    decisionsPrompt: 'Zapisz decyzje techniczne, które stoją za tą aplikacją, i dlaczego je podjęto.',
    missingProduct: 'Nie ma jeszcze strony o produkcie',
    missingProductText: 'Czym jest aplikacja, dla kogo, co ludzie z nią robią i po co – słowami osoby, która o nią poprosiła.',
    missingDecisions: 'Nie zapisano jeszcze żadnych decyzji',
    missingDecisionsText: 'Jak aplikacja jest zbudowana i dlaczego: jak przechowuje dane, od czego zależy, co pominięto. To, co musi wiedzieć ten, kto zmieni ją jako następny.',
    correctText: 'Agent pisze to na podstawie twoich próśb i aktualizuje przy każdej zmianie. Coś się nie zgadza albo czegoś brakuje? Powiedz mu.',
    correct: 'Powiedz agentowi',
    correctPrompt: 'Popraw opis aplikacji: ',
    placeholder: 'Wyjaśnia, dlaczego wpisy są przechowywane przez rok',
  },
  tests: {
    title: 'Testy',
    hint: 'Jak ta aplikacja jest testowana automatycznie oraz skrypty i dane, których używają testy. Agenci je aktualizują i uruchamiają, zanim uznają zmianę za gotową. Są zachowywane z każdą wersją.',
    inDraft: 'Testy tego szkicu. Staną się testami twojej aplikacji, gdy szkic trafi online – najpierw uruchom je na jego podglądzie.',
    pages: { testing: 'Jak jest testowana' },
    emptyTitle: 'Nie ma jeszcze testów',
    emptyText: (code) => (
      <>
        To, jak aplikacja jest testowana – co musi działać dalej, jak to sprawdzić i jak uruchomić skrypty – agenci
        opisują w {code('.lambda/tests/README.md')}, a obok leżą skrypty i dane testowe.
      </>
    ),
    ask: 'Poproś agenta o napisanie testów',
    writePrompt: 'Napisz testy tej aplikacji: co musi działać dalej i jak to sprawdzić automatycznie, ze skryptem do uruchomienia na jej podglądzie.',
    missing: 'Nie opisano jeszcze, jak jest testowana',
    missingText: 'Co musi działać dalej, jak sprawdzić każdą z tych rzeczy i jak uruchomić skrypty, które leżą obok.',
    placeholder: 'Sprawdza, czy pełna lista odrzuca nowe wpisy',
  },
  files: 'Pliki',
  noFiles: 'Obok stron nie ma żadnych plików.',
  none: 'brak',
  missingPill: 'Jeszcze nie napisano',
  changedIn: (version) => `Zmieniono w wersji ${version}`,
  changedInDraft: 'Zmieniono w tym szkicu',
  showChanges: 'Pokaż, co się zmieniło',
  hideChanges: 'Ukryj, co się zmieniło',
  noChanges: 'Nic się nie zmieniło.',
  edit: 'Edytuj',
  olderVersion: 'Wersja nigdy się nie zmienia: stronę edytuje się w najnowszej wersji albo w szkicu.',
  writeIt: 'Napisz samodzielnie',
  askPage: 'Poproś agenta, żeby ją napisał',
  editInCode: 'Otwórz w kodzie',
  cancel: 'Anuluj',
  save: 'Zapisz',
  write: 'Pisz',
  preview: 'Podgląd',
  writeOrPreview: 'Pisanie albo podgląd',
  discard: 'Zmiany na tej stronie przepadną. Odrzucić je?',
  reading: 'Odczytywanie…',
  readFailed: 'Nie udało się tego odczytać.',
  saveFailed: 'Nie udało się tego zapisać.',
  savedDraft: 'Zapisano w szkicu.',
  savedVersion: (version) => `Zapisano jako wersję ${version}.`,
  savedOnline: (version) => `Zapisano jako wersję ${version} i wdrożono.`,
  savedNotOnline: (version) => `Zapisano jako wersję ${version}, ale nie trafiła online.`,
  saveTitle: 'Zapisz jako nową wersję',
  saveText: (newest) =>
    `Wersja nigdy się nie zmienia, więc ta strona zostanie zapisana jako kolejna – na bazie wersji ${newest}, a wszystko inne zostanie bez zmian.`,
  clash: (version) => `Od rozpoczęcia edycji zapisano wersję ${version}, która też zmieniła tę stronę. Zapisanie zastąpi tamtą zmianę.`,
  alsoOnline: 'Od razu wdróż',
  alsoOnlineNote: 'Zmienia się tylko dokumentacja, więc odwiedzający nie zobaczą nic nowego – ale to, co jest online, pozostanie najnowszą wersją.',
  skeleton: {
    product: '# Nazwa aplikacji\n\nCzym jest, w jednym lub dwóch zdaniach.\n\n## Dla kogo jest\n\n## Co ludzie z nią robią\n\n## Funkcje i po co są\n\n## Czego nie robi\n',
    decisions: '# Decyzje\n\n## Decyzja\n\nCo postanowiono, dlaczego i o czym musi pamiętać każda zmiana.\n',
    testing: '# Jak jest testowana\n\nJak uruchomić testy i pod jakim adresem.\n\n## Co musi działać dalej\n\n| Zachowanie | Żądanie | Oczekiwany wynik |\n|---|---|---|\n| | | |\n',
  },
};
