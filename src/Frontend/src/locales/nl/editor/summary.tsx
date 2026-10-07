import type { EditorMessages } from '../../en/editor';
import { many } from './language';

export const summary: EditorMessages['summary'] = {
  reading: 'Status laden…',
  readDocs: 'Documentatie lezen',
  written: 'Documentatie en tests',
  writtenWhy: 'In docs/ en tests/ van de code: bewaard bij elke versie, nooit gecompileerd en nooit geserveerd.',
  writtenMissing: 'Nog niet geschreven',
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
  inVersion: (version) => `In versie ${version}`,
  noVersion: 'In de versie',
  inData: 'In de data',
  sharedByAll: 'Gedeeld door elke versie',
  browse: 'Bekijken',
  versionAllowance: 'Code en resources',
  dataAllowance: 'Database en workspace',
  files: (files, size) => (files === 1 ? `1 bestand, ${size}` : `${files} bestanden, ${size}`),
  code: 'Code',
  codeWhy: 'Nooit geserveerd. De .cs-bestanden bovenaan worden gecompileerd; de rest – documentatie, tests, waaruit het is gebouwd – wordt bij de versie bewaard.',
  resources: 'Resources',
  resourcesPublic: 'Openbaar: de code serveert ze.',
  resourcesPrivate: 'Niet geserveerd door de code.',
  data: 'Data',
  workspace: 'Workspace',
  workspaceOff: 'uitgezet',
  dataPublic: 'Openbaar: de code serveert de workspace.',
  dataPrivate: 'Alleen voor de lambda zelf.',
  secrets: 'Secrets',
  secretsOff: 'uitgeschakeld',
  secretsCount: (count) => (count === 1 ? '1 secret' : `${count} secrets`),
  secretsMissing: (count) => `${count} ontbreken`,
  secretsMissingTitle: 'De code leest secrets die niet zijn ingesteld, en mislukt waar hij dat doet.',
  database: 'Database',
  databaseOff: 'uitgezet',
  databaseHolds: (tables, size) => (tables === 1 ? `1 tabel, ${size}` : `${tables} tabellen, ${size}`),
  databaseOffUsed: 'De code maakt verbinding met de database, maar die staat uit.',
};
