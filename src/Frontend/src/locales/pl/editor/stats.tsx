import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const stats: EditorMessages['stats'] = {
  readFailed: 'Nie udało się odczytać statystyk.',
  range: 'Zakres czasu',
  lastHour: 'Ostatnia godzina',
  lastDay: 'Ostatnie 24 godziny',
  hint: (since) =>
    `Liczone w pamięci od ostatniego startu serwera (${since}). Po restarcie liczenie zaczyna się od nowa.`,
  reading: 'Odczytywanie statystyk…',
  requests: 'żądania',
  websockets: (count) => `oraz ${counted(count, 'połączenie', 'połączenia', 'połączeń')} websocket`,
  failed: 'nieudane',
  serverErrors: (count) => counted(count, 'błąd serwera', 'błędy serwera', 'błędów serwera'),
  rejected: 'nieznalezione lub odrzucone',
  average: 'średni czas odpowiedzi',
  sent: (amount) => `wysłano ${amount}`,
  nobody: (hour) => (hour ? 'Brak wywołań w ostatniej godzinie.' : 'Brak wywołań w ostatnich 24 godzinach.'),
  requestsTitle: 'Żądania',
  per: (hour) => (hour ? 'Na minutę.' : 'Na 15 minut.'),
  answered: 'Obsłużone',
  rejectedSeries: 'Nieznalezione lub odrzucone',
  failedSeries: 'Nieudane',
  timeTitle: 'Czas odpowiedzi',
  averagePer: (hour) => (hour ? 'Średnia na minutę.' : 'Średnia na 15 minut.'),
  averageSeries: 'Średnia',
  mostAsked: 'Najpopularniejsze ścieżki',
  path: 'Ścieżka',
  requestsColumn: 'Żądania',
  failedColumn: 'Nieudane',
  averageColumn: 'Średnia',
  since: 'Od startu serwera.',
};
