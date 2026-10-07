import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const summary: EditorMessages['summary'] = {
  reading: 'Sprawdzanie stanu…',
  readDocs: 'Przeczytaj dokumentację',
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
  versionAllowance: 'Kod i zasoby',
  data: 'Dane',
};
