import type { EditorMessages } from '../../en/editor';
import { many } from './language';

export const stats: EditorMessages['stats'] = {
  readFailed: 'De cijfers konden niet worden gelezen.',
  range: 'Periode',
  lastHour: 'Afgelopen uur',
  lastDay: 'Afgelopen dag',
  hint: (since) =>
    `In het geheugen geteld sinds de laatste start van de server, ${since}. Na een herstart beginnen deze cijfers opnieuw.`,
  reading: 'Cijfers laden…',
  requests: 'requests',
  websockets: (count) => `en ${many(count, 'websocketverbinding', 'websocketverbindingen')}`,
  failed: 'mislukt',
  serverErrors: (count) => many(count, 'serverfout', 'serverfouten'),
  rejected: 'niet gevonden of geweigerd',
  average: 'gemiddelde responstijd',
  sent: (amount) => `${amount} verstuurd`,
  nobody: (hour) =>
    hour ? 'Niemand heeft hem het afgelopen uur aangeroepen.' : 'Niemand heeft hem de afgelopen dag aangeroepen.',
  requestsTitle: 'Requests',
  per: (hour) => (hour ? 'Per minuut.' : 'Per 15 minuten.'),
  answered: 'Beantwoord',
  rejectedSeries: 'Niet gevonden of geweigerd',
  failedSeries: 'Mislukt',
  timeTitle: 'Responstijd',
  averagePer: (hour) => (hour ? 'Het gemiddelde per minuut.' : 'Het gemiddelde per 15 minuten.'),
  averageSeries: 'Gemiddeld',
  mostAsked: 'Meest opgevraagd',
  path: 'Pad',
  requestsColumn: 'Requests',
  failedColumn: 'Mislukt',
  averageColumn: 'Gemiddeld',
  since: 'Sinds de start van de server.',
};
