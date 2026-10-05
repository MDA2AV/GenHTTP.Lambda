import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Kod',
  version: (version) => `wersja ${version}`,
  edited: ', edytowana',
  online: ', online',
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
  demo: 'To demo, więc wszystko tu jest tylko do odczytu. Żeby coś zmienić, utwórz na jego podstawie własną lambdę. ',
  edit: 'Edytuj kod ręcznie. Zapisanie tworzy nową wersję i nie rusza tego, co jest online; wdrożenie wrzuca ją online. Jeśli chcesz najpierw wypróbować zmianę, utwórz szkic. ',
  editFeature:
    'Kod tego szkicu. Zapisanie zostawia go w szkicu – dla odwiedzających lambdę nic się nie zmienia. Wdrożenie wrzuca go online pod własnym adresem szkicu, żeby go wypróbować; scalenie szkicu zamienia go w kolejną wersję. ',
  inFeature: (name) => `szkic „${name}”`,
  changedElsewhere: 'Od chwili otwarcia szkic został zapisany gdzie indziej – może przez agenta. Zanim zapiszesz tutaj, wczytaj zapisany stan; twoje zmiany nie zostałyby zapisane na nim.',
  readAgain: 'Wczytaj zapisany stan',
  files: (entry, cs, context) => (
    <>
      {entry} zwraca to, co jest serwowane, pozostałe pliki {cs} zawierają typy, a każdy inny plik jest serwowany bez
      zmian – poza tym, co jest w folderze {context}: dokumentacją, testami i przestrzenią deweloperską, które nigdy nie
      są kompilowane ani serwowane. Ctrl+S zapisuje, F12 przechodzi do deklaracji.
    </>
  ),
  newer: (version) => ` Wersja ${version} jest nowsza niż ta otwarta tutaj.`,
  built: (folder) =>
    `Build przestrzeni deweloperskiej zapisuje ${folder}: następny build zastąpi to, co zmienisz tutaj. Zmień raczej źródła, z których jest budowany.`,
  check: 'Sprawdź',
  save: 'Zapisz',
  deploy: 'Wdróż',
  deployPreviewTitle: 'Zapisz i wrzuć szkic online pod jego własnym adresem, żeby go wypróbować',
  binary: (size) => `Ten plik nie jest tekstem, więc nie da się go edytować. Jest serwowany bez zmian i waży ${size} kB.`,
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
};
