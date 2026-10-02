import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Un deployment resta online finché qualcuno lo usa${until ? `. Scadenza se nessuno lo usa: ${until}` : ''}. Un nuovo deploy, o qualsiasi visita, fa ripartire il conto alla rovescia.`,
  takeOffline: 'Metti offline',
  readFailed: 'Impossibile leggere la cronologia.',
  reading: 'Lettura della cronologia…',
  none: 'Non è ancora stato fatto nessun deploy.',
  noDescription: 'Nessuna descrizione',
  deployed: (when, by) => `Deploy: ${when} (${by})`,
  duration: 'Per quanto è stato online',
  online: 'online',
  short: {
    replaced: 'sostituito',
    stopped: 'messo offline',
    expired: 'scaduto',
    admin: 'fermato dall’operatore',
    ended: 'terminato',
  },
  putBack: (version) => `Rimetti online la versione ${version}`,
  timeline: 'Cosa è stato online negli ultimi sette giorni',
  block: (version, from, to) => `Versione ${version}, ${from} – ${to ?? 'ora'}`,
  weekAgo: 'una settimana fa',
  now: 'ora',
};
