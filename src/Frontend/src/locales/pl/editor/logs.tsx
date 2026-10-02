import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Nie udało się odczytać logu.',
  hint: (capturing) =>
    'Żądania, to, co lambda wypisała, i to, co poszło nie tak – na bieżąco.' +
    (capturing ? '' : ' Ta instalacja nie zapisuje tego, co wypisują lambdy, więc widać tylko żądania i błędy.') +
    ' Log jest trzymany w pamięci i wspólny dla wszystkich lambd na tym serwerze, więc sięga od kilku minut do kilku godzin wstecz, a po restarcie jest pusty. Adresy odwiedzających nie są pokazywane.',
  featureHint: (capturing) =>
    'Żądania do podglądu tego szkicu, to, co wypisał, i to, co poszło nie tak – na bieżąco.' +
    (capturing ? '' : ' Ta instalacja nie zapisuje tego, co wypisują lambdy, więc widać tylko żądania i błędy.') +
    ' Ten log jest oddzielony od logu samej lambdy, który nigdy nie pokazuje podglądu. Jest trzymany w pamięci, więc sięga od kilku minut do kilku godzin wstecz.',
  nothingPreview: 'Na razie pusto. Otwórz podgląd szkicu, a jego żądania pojawią się tutaj.',
  search: 'Szukaj',
  searchLabel: 'Szukaj w logu',
  resume: 'Pokazuj nowe wpisy na bieżąco',
  pause: 'Wstrzymaj nowe wpisy na czas czytania',
  paused: 'Wstrzymane',
  live: 'Na żywo',
  show: 'Pokaż',
  all: 'Wszystko',
  requests: 'Żądania',
  output: 'Konsola',
  problems: 'Problemy',
  reading: 'Odczytywanie logu…',
  noProblems: 'Nic nie poszło nie tak – przynajmniej nic, co log jeszcze pamięta.',
  nothing: 'Na razie pusto. Otwórz adres lambdy, a jej żądania pojawią się tutaj.',
  noMatch: 'Brak wyników.',
  identical: (count) => counted(count, 'identyczny wpis', 'identyczne wpisy', 'identycznych wpisów'),
  at: (domain) => `, pod adresem ${domain}`,
  from: (country) => `, kraj: ${country}`,
};
