import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Litery, cyfry, myślniki i podkreślenia, na końcu .cs',
  slashes: 'Bez ukośnika na początku i na końcu, poniżej 120 znaków.',
  deep: 'Najwyżej sześć poziomów folderów.',
  characters: 'Litery, cyfry, myślniki, podkreślenia i kropki, rozdzielone ukośnikami.',
  extension: 'Nazwa musi mieć rozszerzenie, żeby plik był serwowany jako właściwy typ.',
  context: 'W .lambda/ tylko docs/ i tests/ – litery, cyfry, myślniki, podkreślenia i kropki, rozdzielone ukośnikami.',
  contextFiles: 'Dokumentacja i testy: część wersji, nigdy nie są kompilowane ani serwowane',
  build: 'To, z czego jest zbudowane, zmienia się tam, gdzie jest budowane – w sklonowanym repozytorium lub przez twojego agenta – a czyta w sekcji Budowanie.',
  buildFiles: (count) =>
    count === 1
      ? 'Budowanie · 1 plik'
      : `Budowanie · ${count} ${count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 12 || count % 100 > 14) ? 'pliki' : 'plików'}`,
  buildTitle: 'To, z czego budowany jest kod lub zasoby. Czytasz to w sekcji Budowanie; przy zapisie zostaje bez zmian.',
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
