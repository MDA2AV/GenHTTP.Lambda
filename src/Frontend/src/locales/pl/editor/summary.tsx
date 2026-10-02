import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const summary: EditorMessages['summary'] = {
  reading: 'Sprawdzanie stanu…',
  readDocs: 'Przeczytaj dokumentację',
  written: 'Dokumentacja i testy',
  writtenWhy: 'Nigdy nie są kompilowane ani serwowane. Zachowywane z każdą wersją i wliczane do zasobów.',
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
  code: 'Kod',
  codeWhy: 'C# jest kompilowany, nigdy serwowany.',
  characters: 'znaków',
  assets: 'Zasoby',
  assetsPublic: 'Publiczne: kod je serwuje.',
  assetsPrivate: 'Kod ich nie serwuje.',
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
  databaseTables: (count) => counted(count, 'tabela', 'tabele', 'tabel'),
  databaseOffUsed: 'Kod łączy się z bazą danych, która jest wyłączona.',
};
