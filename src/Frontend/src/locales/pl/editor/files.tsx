import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const files: EditorMessages['files'] = {
  version: 'Wersja',
  shown: (version, online, newest) => `Wersja ${version}${online ? ', online' : newest ? ', najnowsza' : ''}`,
  optionOnline: ' (online)',
  count: (files) => counted(files, 'plik', 'pliki', 'plików'),
  usage: (files, used, of) => `${files}, ${used} z ${of}`,
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
