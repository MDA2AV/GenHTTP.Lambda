import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: 'Status wird gelesen …',
  readDocs: 'Dokumentation lesen',
  hint: (since, kept, retention, tier) =>
    `Der Traffic wird seit dem letzten Serverstart gezählt (${since}). ` +
    (kept
      ? `Ein Lambda bleibt online, solange es genutzt wird. Nach ${retention} Tagen ohne Besuche und ohne Änderungen wird es gelöscht.`
      : `Dieses Lambda ist im Tarif ${tier}. Es bleibt online und gespeichert, egal wie wenig los ist.`),
  onlineFor: (duration, version) => (
    <>
      Seit {duration('einer Weile')} online, liefert Version {version} aus.
    </>
  ),
  offline: 'Offline. Bis eine Version deployt wird, wird nichts ausgeliefert.',
  nothing: 'Es wurde noch nichts geschrieben.',
  requestsToday: 'Requests heute',
  lastHour: (count) => `${count} in der letzten Stunde`,
  hourly: 'Requests pro Stunde in den letzten 24 Stunden',
  failed: 'fehlgeschlagen',
  failedTitle: (failed, rejected) =>
    `${failed} Serverfehler, ${rejected} nicht gefunden oder abgelehnt, in den letzten 24 Stunden`,
  average: 'Antwortzeit im Schnitt',
  noneYet: 'noch keiner',
  lastVisit: 'letzter Besuch',
  problems: 'Zuletzt ist etwas schiefgegangen',
  openLog: 'Log öffnen',
  latest: 'Letzte Änderung',
  allVersions: 'Alle Versionen',
  noDescription: 'Keine Beschreibung',
  version: (version) => `Version ${version}`,
  notOnline: 'noch nicht online',
  wanted: 'Worum gebeten wurde',
  noVersions: 'Noch keine Versionen.',
  inProgress: 'In Arbeit',
  allFeatures: 'Alle Entwürfe',
  previewOnline: 'Die Vorschau ist online',
  previewOffline: 'Die Vorschau ist offline',
  behind: 'nicht aktuell',
  storage: 'Speicher',
  versionAllowance: 'Code und Ressourcen',
  data: 'Daten',
};
