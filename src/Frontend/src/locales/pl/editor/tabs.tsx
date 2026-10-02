import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Litery, cyfry, myślniki i podkreślenia, na końcu .cs',
  slashes: 'Bez ukośnika na początku i na końcu, poniżej 120 znaków.',
  deep: 'Najwyżej sześć poziomów folderów.',
  characters: 'Litery, cyfry, myślniki, podkreślenia i kropki, rozdzielone ukośnikami.',
  extension: 'Nazwa musi mieć rozszerzenie, żeby plik był serwowany jako właściwy typ.',
  context: 'W .lambda/ tylko docs/ i tests/ – litery, cyfry, myślniki, podkreślenia i kropki, rozdzielone ukośnikami.',
  contextFiles: 'Dokumentacja i testy: część wersji, nigdy nie są kompilowane ani serwowane',
  exists: 'Plik o tej nazwie już istnieje.',
  remove: (name) => `Usunąć plik ${name}? Jego zawartość też zniknie.`,
  there: (name) => `Plik ${name} już istnieje.`,
  entry: 'Snippet: to, co zwraca, jest serwowane',
  errors: 'zawiera błędy',
  removeFile: (name) => `Usuń plik ${name}`,
  removeTitle: 'Usuń ten plik',
  placeholder: 'Types.cs, site/index.html lub .lambda/docs/api.md',
  newFile: 'Nowy plik',
  uploadTitle: 'Prześlij plik – obrazek, font albo stronę',
  upload: 'Prześlij plik',
};
