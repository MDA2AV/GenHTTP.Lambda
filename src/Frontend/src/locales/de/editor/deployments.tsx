import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Ein Deployment bleibt online, solange es genutzt wird${until ? ` – ohne Besuche bis ${until}` : ''}. Jedes neue Deployment und jeder Besuch setzt diese Frist zurück.`,
  takeOffline: 'Offline nehmen',
  readFailed: 'Der Verlauf konnte nicht gelesen werden.',
  reading: 'Verlauf wird gelesen …',
  none: 'Noch nichts deployt.',
  noDescription: 'Keine Beschreibung',
  deployed: (when, by) => `Deployt am ${when} (${by})`,
  duration: 'Wie lange es online war',
  online: 'online',
  short: {
    replaced: 'ersetzt',
    stopped: 'offline genommen',
    expired: 'abgelaufen',
    admin: 'vom Betreiber',
    ended: 'beendet',
  },
  putBack: (version) => `Version ${version} wieder online stellen`,
  timeline: 'Was in den letzten sieben Tagen online war',
  block: (version, from, to) => `Version ${version}, ${from} bis ${to ?? 'jetzt'}`,
  weekAgo: 'vor einer Woche',
  now: 'jetzt',
};
