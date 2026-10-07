import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const summary: EditorMessages['summary'] = {
  reading: 'Sprawdzanie stanu…',
  readDocs: 'Przeczytaj dokumentację',
  written: 'Dokumentacja i testy',
  writtenWhy: 'W docs/ i tests/ w kodzie: zachowywane z każdą wersją, nigdy nie są kompilowane ani serwowane.',
  writtenMissing: 'Jeszcze nie napisano',
  hint: (since, kept, retention, tier) =>
    `Ruch jest liczony od ostatniego startu serwera (${since}). ` +
    (kept
      ? `Lambda działa, dopóki ktoś z niej korzysta, i jest usuwana po ${retention === 1 ? '1 dniu' : `${retention} dniach`} bez odwiedzin i zmian.`
      : `Ta lambda jest w planie ${tier}, więc zostaje online i zapisana bez względu na ruch.`),
  onlineFor: (duration, version) => (
    <>
      Online od {duration('jakiegoś czasu')}, serwuje wersję {version}.
    </>
  ),
  offline: 'Offline. Nic nie jest serwowane, dopóki nie wdrożysz jakiejś wersji.',
  nothing: 'Nic jeszcze nie napisano.',
  requestsToday: 'żądania dziś',
  lastHour: (count) => `${count} w ostatniej godzinie`,
  hourly: 'Żądania na godzinę, ostatnie 24 godziny',
  failed: 'nieudane',
  failedTitle: (failed, rejected) =>
    `Ostatnie 24 godziny: ${counted(failed, 'błąd serwera', 'błędy serwera', 'błędów serwera')}, nieznalezione lub odrzucone: ${rejected}`,
  average: 'średni czas odpowiedzi',
  noneYet: 'brak',
  lastVisit: 'ostatnia wizyta',
  problems: 'Ostatnio coś poszło nie tak',
  openLog: 'Otwórz log',
  latest: 'Ostatnia zmiana',
  allVersions: 'Wszystkie wersje',
  noDescription: 'Bez opisu',
  version: (version) => `Wersja ${version}`,
  notOnline: 'jeszcze nie online',
  wanted: 'O co proszono',
  noVersions: 'Jeszcze nie ma wersji.',
  inProgress: 'W toku',
  allFeatures: 'Wszystkie szkice',
  previewOnline: 'Podgląd jest online',
  previewOffline: 'Podgląd jest offline',
  behind: 'nieaktualny',
  storage: 'Miejsce',
  inVersion: (version) => `W wersji ${version}`,
  noVersion: 'W wersji',
  inData: 'W danych',
  sharedByAll: 'Wspólne dla wszystkich wersji',
  browse: 'Przeglądaj',
  versionAllowance: 'Kod i zasoby',
  dataAllowance: 'Baza danych i obszar roboczy',
  files: (files, size) => (files === 1 ? `1 plik, ${size}` : files % 10 >= 2 && files % 10 <= 4 && (files % 100 < 12 || files % 100 > 14) ? `${files} pliki, ${size}` : `${files} plików, ${size}`),
  code: 'Kod',
  codeWhy: 'Nigdy nie jest serwowany. Pliki .cs są kompilowane, w dowolnym folderze; reszta – dokumentacja, testy, to, z czego coś jest budowane – jest przechowywana razem z wersją.',
  resources: 'Zasoby',
  resourcesPublic: 'Publiczne: kod je serwuje.',
  resourcesPrivate: 'Nieserwowane przez kod.',
  data: 'Dane',
  workspace: 'Obszar roboczy',
  workspaceOff: 'wyłączony',
  dataPublic: 'Publiczne: kod serwuje obszar roboczy.',
  dataPrivate: 'Dostępne tylko dla lambdy.',
  secrets: 'Sekrety',
  secretsOff: 'wyłączone',
  secretsCount: (count) => `${count} ${count === 1 ? 'sekret' : [2, 3, 4].includes(count % 10) && ![12, 13, 14].includes(count % 100) ? 'sekrety' : 'sekretów'}`,
  secretsMissing: (count) => `brakuje: ${count}`,
  secretsMissingTitle: 'Kod odczytuje sekrety, które nie są ustawione, i w tych miejscach nie działa.',
  database: 'Baza danych',
  databaseOff: 'wyłączona',
  databaseHolds: (tables, size) => (tables === 1 ? `1 tabela, ${size}` : tables % 10 >= 2 && tables % 10 <= 4 && (tables % 100 < 12 || tables % 100 > 14) ? `${tables} tabele, ${size}` : `${tables} tabel, ${size}`),
  databaseOffUsed: 'Kod łączy się z bazą danych, która jest wyłączona.',
};
