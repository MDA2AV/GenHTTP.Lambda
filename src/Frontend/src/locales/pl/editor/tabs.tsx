import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Plik C# ma nazwę z liter, cyfr, myślników, podkreśleń i kropek, zaczynającą się od litery i kończącą na .cs, najwyżej 40 znaków.',
  name: 'Litery, cyfry i - _ . + @ ( ) [ ] { } $ ~, foldery oddzielone ukośnikami, bez spacji i bez nazwy kończącej się kropką.',
  taken: 'Wyeksportowana lub sklonowana lambda ma na górze plik lub folder o tej nazwie. Umieść go w folderze albo nazwij inaczej.',
  lambda: 'Lambda nie ma już folderu .lambda/: jej dokumentacja trafia do docs/, a testy do tests/.',
  assets: 'To, co lambda serwuje, jest teraz w jej zasobach – dodaj to tam.',
  resourceName: 'Litery, cyfry, myślniki, podkreślenia i kropki, oddzielone ukośnikami, najwyżej sześć folderów w głąb – oraz rozszerzenie, żeby plik był serwowany jako właściwy typ.',
  exists: 'Plik o tej nazwie już istnieje.',
  remove: (name) => `Usunąć plik ${name}? Jego zawartość też zniknie.`,
  removeFolder: (name, files) => `Usunąć ${name} i ${files === 1 ? '1 plik' : files % 10 >= 2 && files % 10 <= 4 && (files % 100 < 12 || files % 100 > 14) ? `${files} pliki` : `${files} plików`} w nim?`,
  there: (name) => `Plik ${name} już istnieje.`,
  entry: 'Snippet: to, co zwraca, jest serwowane',
  errors: 'zawiera błędy',
  removeFile: (name) => `Usuń plik ${name}`,
  removeTitle: 'Usuń',
  codePlaceholder: 'Store.cs, models/Item.cs lub docs/notes.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'Prześlij plik',
};
