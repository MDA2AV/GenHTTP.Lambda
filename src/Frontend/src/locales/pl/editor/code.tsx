import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Kod',
  version: (version) => `wersja ${version}`,
  edited: ', edytowana',
  loadFailed: 'Nie udało się załadować tej wersji.',
  compiles: 'Kompiluje się.',
  notYet: 'Jeszcze się nie kompiluje.',
  checkFailed: 'Nie udało się sprawdzić kodu.',
  saved: (version) => `Zapisano jako wersję ${version}.`,
  featureSaved: 'Zapisano w szkicu. Wdróż podgląd, żeby go wypróbować.',
  featureLoadFailed: 'Nie udało się załadować szkicu.',
  previewOnline: 'Podgląd jest online.',
  previewRefused: 'Podgląd się nie zmienił. Zobacz niżej, co mówi kompilator.',
  isOnline: (version) => `Wersja ${version} jest online.`,
  notOnline: 'Nie trafiła online. Zobacz niżej, co mówi kompilator.',
  failed: 'Nie udało się.',
  unchanged: 'Nic się nie zmieniło od ostatniego zapisu.',
  demo: 'To demo, więc wszystko tu jest tylko do odczytu. Żeby coś zmienić, utwórz na jego podstawie własną lambdę.',
  hint: (b) => (
    <>
      Pliki jednej wersji. Jej {b('kod')} to program i wszystko, co jest z nim przechowywane: pliki .cs na górze są
      kompilowane, a każdy inny plik – dokumentacja, testy, to, z czego budowany jest frontend – jest przechowywany z
      wersją i nigdy nie jest kompilowany ani serwowany. Jej {b('zasoby')} – strony, skrypty, style, obrazy, migracje
      bazy danych – są czytane i serwowane w trakcie działania, a publiczne są tam, gdzie kod je serwuje. Zapisanie tworzy
      nową wersję i nie rusza tego, co jest online; żeby najpierw wypróbować zmianę, utwórz szkic. Ctrl-S zapisuje, F12
      przechodzi do deklaracji.
    </>
  ),
  hintFeature: (b) => (
    <>
      Pliki tego szkicu: jego {b('kod')} – pliki .cs na górze są kompilowane, reszta jest z nim przechowywana – oraz jego{' '}
      {b('zasoby')}, czytane i serwowane w trakcie działania. Zapisanie zostawia je w szkicu i pokazuje pod własnym
      adresem szkicu; twoi odwiedzający nic z tego nie zobaczą, dopóki nie udostępnisz szkicu.
    </>
  ),
  inFeature: (name) => `szkic „${name}”`,
  changedElsewhere: 'Od chwili otwarcia szkic został zapisany gdzie indziej – może przez agenta. Zanim zapiszesz tutaj, wczytaj zapisany stan; twoje zmiany nie zostałyby zapisane na nim.',
  readAgain: 'Wczytaj zapisany stan',
  newer: (version) => `Wersja ${version} jest nowsza niż ta otwarta tutaj.`,
  check: 'Sprawdź',
  save: 'Zapisz',
  deploy: 'Wdróż',
  deployPreviewTitle: 'Zapisz i wrzuć szkic online pod jego własnym adresem, żeby go wypróbować',
  binary: (size) => `Ten plik nie jest tekstem, więc nie da się go tu edytować. Ma rozmiar ${size}.`,
  saveAndDeploy: 'Zapisz i wdróż',
  saveVersion: 'Zapisz nową wersję',
  fromOlder: (version, newest) =>
    `To zaczyna się od wersji ${version}, a wersja ${newest} jest nowsza. Zapisanie zrobi z tego najnowszą wersję – bez tego, co przyszło po wersji ${version}.`,
  featureInstead: (start) => (
    <>
      Chcesz coś wypróbować? {start('Przenieś to do nowego szkicu')}: dostanie własny adres, a żadna wersja nie
      zostanie zapisana, dopóki wszystko nie będzie gotowe.
    </>
  ),
  cancel: 'Anuluj',
  what: 'Co zmienia ta wersja? Opcjonalnie – pojawi się w historii.',
  placeholder: 'Dodaje formularz kontaktowy',
  goToDefinition: 'Przejdź do definicji',
  versionLabel: 'Wersja',
  shown: (version, online, newest) =>
    `Wersja ${version}${online ? ', online' : newest ? ', najnowsza' : ''}`,
  optionOnline: ' (online)',
  switchUnsaved: 'To, co tu zmieniono, nie jest zapisane. Otworzyć mimo to inną wersję?',
  noVersion: 'Nie ma jeszcze wersji do pokazania.',
  label: 'Pliki',
  codeGroup: 'Kod',
  codeWhy: 'Nigdy nie jest serwowany. Pliki .cs na górze są kompilowane, reszta jest przechowywana razem z wersją.',
  resources: 'Zasoby',
  resourcesPublic: 'Publiczne: ta wersja serwuje je przez Resources.',
  resourcesPrivate: 'Dołączone do wersji, ale ta wersja ich nie serwuje.',
  noResources: 'Brak w tej wersji.',
  count: (files) => (files === 1 ? '1 plik' : files % 10 >= 2 && files % 10 <= 4 && (files % 100 < 12 || files % 100 > 14) ? `${files} pliki` : `${files} plików`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Ta wersja zajmuje ${used} z ${of}, które może mieć wersja – razem jej kod i zasoby.`,
  scope: (data) => (
    <>To, co lambda przechowuje w trakcie działania, jest wspólne dla każdej wersji i znajduje się w sekcji {data('Dane')}.</>
  ),
  download: 'Pobierz',
  newIn: (group) => `Nowy plik w: ${group}`,
  uploadIn: (group) => `Prześlij do: ${group}`,
  pick: 'Wybierz plik, aby zobaczyć, co jest w środku.',
};
