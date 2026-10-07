import type { EditorMessages } from '../../en/editor';
import { many } from './language';

export const summary: EditorMessages['summary'] = {
  reading: 'Status laden…',
  readDocs: 'Documentatie lezen',
  hint: (since, kept, retention, tier) =>
    `Het verkeer wordt geteld sinds de laatste start van de server (${since}). ` +
    (kept
      ? `Een lambda blijft online zolang mensen hem gebruiken. Na ${retention} dagen zonder bezoek en zonder wijzigingen wordt hij verwijderd.`
      : `Deze lambda heeft het pakket ${tier}. Daardoor blijft hij online en bewaard, hoe stil het ook wordt.`),
  onlineFor: (duration, version) => (
    <>
      Al {duration('een tijdje')} online, met versie {version}.
    </>
  ),
  offline: 'Offline. Er wordt niets geserveerd tot je een versie deployt.',
  nothing: 'Er is nog niets geschreven.',
  requestsToday: 'requests vandaag',
  lastHour: (count) => `${count} in het afgelopen uur`,
  hourly: 'Requests per uur, afgelopen dag',
  failed: 'mislukt',
  failedTitle: (failed, rejected) =>
    `${many(failed, 'serverfout', 'serverfouten')}, ${rejected} niet gevonden of geweigerd, afgelopen dag`,
  average: 'gemiddelde responstijd',
  noneYet: 'nog geen',
  lastVisit: 'laatste bezoek',
  problems: 'Er ging onlangs iets mis',
  openLog: 'Logs openen',
  latest: 'Laatste wijziging',
  allVersions: 'Alle versies',
  noDescription: 'Geen beschrijving',
  version: (version) => `Versie ${version}`,
  notOnline: 'nog niet online',
  wanted: 'Wat er gevraagd werd',
  noVersions: 'Nog geen versies.',
  inProgress: 'In de maak',
  allFeatures: 'Alle concepten',
  previewOnline: 'De voorvertoning staat online',
  previewOffline: 'De voorvertoning staat offline',
  behind: 'verouderd',
  storage: 'Opslag',
  versionAllowance: 'Code en resources',
  data: 'Data',
};
