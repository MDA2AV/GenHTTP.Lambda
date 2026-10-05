import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      Pliki jednej wersji – czyli program. {b('Kod')} jest kompilowany i nigdy nie jest serwowany. {b('Zasoby')} –
      strony, skrypty, style, obrazki – są zapisywane razem z kodem, razem z nim wdrażane i przywracane, i są publiczne,
      jeśli kod je serwuje. Tego, co lambda przechowuje w trakcie działania, tu nie ma: to jej {b('Dane')}.
    </>
  ),
  scope: (version, data) => (
    <>
      Te pliki należą do wersji {version} i zmieniają się razem z nią. To, co lambda przechowuje w trakcie działania,
      jest wspólne dla wszystkich wersji i znajdziesz to w sekcji {data('Dane')}.
    </>
  ),
  edit: 'Edytuj tę wersję',
  version: 'Wersja',
  shown: (version, online, newest) => `Wersja ${version}${online ? ', online' : newest ? ', najnowsza' : ''}`,
  optionOnline: ' (online)',
  readFailed: 'Nie udało się odczytać tej wersji.',
  noVersion: 'Nie ma jeszcze żadnej wersji.',
  label: 'Pliki',
  code: 'Kod',
  codeWhy: 'Kompilowany do lambdy, nigdy serwowany.',
  count: (files) => counted(files, 'plik', 'pliki', 'plików'),
  codeUsage: (files, used, of) => `${files}, ${used} z ${of} znaków`,
  usage: (files, used, of) => `${files}, ${used} z ${of}`,
  noCode: 'W tej wersji nie ma kodu.',
  assets: 'Zasoby',
  assetsPublic: 'Publiczne: ta wersja serwuje je przez Assets.',
  assetsPrivate: 'Zapisane z kodem, ale ta wersja ich nie serwuje.',
  noAssets: 'Brak w tej wersji.',
  context: 'Dokumentacja i testy',
  contextWhy: 'Nigdy nie są kompilowane ani serwowane: to, co napisano o tej wersji, dla każdego, kto ją czyta lub zmienia.',
  contextUsage: (files, size) => `${files}, ${size} – wliczane do zasobów`,
  noContext: 'O tej wersji nic jeszcze nie napisano.',
  build: 'Budowanie',
  buildWhy: 'Nigdy nie kompilowane i nigdy nie serwowane: to, z czego kod lub zasoby są budowane przez tego, kto je zmienia.',
  noBuild: 'Nic – jeśli kod lub zasoby powstają w narzędziu do budowania, to, z czego powstają, jest przechowywane tutaj.',
  data: 'Dane',
  dataPublic: 'Publiczne: kod, który jest online, serwuje je przez Workspace.',
  dataPrivate: 'Dostępne tylko dla lambdy. Nie należą do żadnej wersji.',
  uploadFailed: (path) => `Nie udało się przesłać pliku ${path}.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `Usunąć folder ${path} i ${counted(held, 'plik', 'pliki', 'plików')} w środku?`
      : `Usunąć folder ${path}?`,
  deleteFile: (path) => `Usunąć plik ${path}? Lambda już go nie znajdzie.`,
  deleteFailed: 'Nie udało się tego usunąć.',
  full: 'Brak miejsca na dane',
  uploadInto: (folder) => `Prześlij do folderu ${folder}`,
  upload: 'Prześlij',
  reading: 'Odczytywanie…',
  noData: 'Na razie pusto. Tu pojawi się to, co lambda zapisze w trakcie działania.',
  delete: (path) => `Usuń ${path}`,
  deleteShort: 'Usuń',
  fileFailed: 'Nie udało się odczytać pliku.',
  pick: 'Wybierz plik, żeby zobaczyć, co w nim jest.',
  tooLarge: (name, size) => (
    <>
      Plik {name} ma {size} – to za dużo, żeby go tu pokazać.
    </>
  ),
  download: 'Pobierz',
  readingFile: (name) => `Odczytywanie pliku ${name}…`,
  missing: (name) => `Ta wersja nie ma pliku ${name}.`,
  saved: 'zapisano',
  notText: 'To nie jest tekst. Pobierz plik, żeby zajrzeć do środka.',
};
