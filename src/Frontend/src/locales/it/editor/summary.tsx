import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: 'Lettura dello stato…',
  readDocs: 'Leggi la documentazione',
  hint: (since, kept, retention, tier) =>
    `Il traffico è contato dall’ultimo avvio del server (${since}). ` +
    (kept
      ? `Una lambda resta online finché qualcuno la usa, e viene eliminata dopo ${retention} giorni senza visite né modifiche.`
      : `Questa lambda è nel piano ${tier}, che la tiene online e salvata anche quando nessuno la usa.`),
  onlineFor: (duration, version) => (
    <>
      Online da {duration('un po’')}, con la versione {version}.
    </>
  ),
  offline: 'Offline. Non viene servito niente finché non fai il deploy di una versione.',
  nothing: 'Non è ancora stato scritto niente.',
  requestsToday: 'richieste oggi',
  lastHour: (count) => `${count} nell’ultima ora`,
  hourly: 'Richieste all’ora nelle ultime 24 ore',
  failed: 'fallite',
  failedTitle: (failed, rejected) =>
    `${failed} errori del server, ${rejected} non trovate o rifiutate, nelle ultime 24 ore`,
  average: 'tempo medio di risposta',
  noneYet: 'ancora nessuna',
  lastVisit: 'ultima visita',
  problems: 'Qualcosa è andato storto di recente',
  openLog: 'Apri il log',
  latest: 'Ultima modifica',
  allVersions: 'Tutte le versioni',
  noDescription: 'Nessuna descrizione',
  version: (version) => `Versione ${version}`,
  notOnline: 'non ancora online',
  wanted: 'Cosa è stato chiesto',
  noVersions: 'Ancora nessuna versione.',
  inProgress: 'In lavorazione',
  allFeatures: 'Tutte le bozze',
  previewOnline: 'La sua anteprima è online',
  previewOffline: 'La sua anteprima è offline',
  behind: 'non aggiornata',
  storage: 'Spazio',
  versionAllowance: 'Codice e risorse',
  data: 'Dati',
};
