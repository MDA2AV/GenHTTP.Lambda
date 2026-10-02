import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Een deployment blijft online zolang mensen hem gebruiken${until ? `. Gebruikt niemand hem, dan blijft hij online tot ${until}` : ''}. Opnieuw deployen, of elk bezoek, zet die klok weer op nul.`,
  takeOffline: 'Offline halen',
  readFailed: 'De geschiedenis kon niet worden gelezen.',
  reading: 'Geschiedenis laden…',
  none: 'Er is nog niets gedeployd.',
  noDescription: 'Geen beschrijving',
  deployed: (when, by) => `Gedeployd op ${when} (${by})`,
  duration: 'Hoe lang hij online stond',
  online: 'online',
  short: {
    replaced: 'vervangen',
    stopped: 'offline gehaald',
    expired: 'verlopen',
    admin: 'door beheerder',
    ended: 'beëindigd',
  },
  putBack: (version) => `Versie ${version} weer online zetten`,
  timeline: 'Wat er de afgelopen zeven dagen online stond',
  block: (version, from, to) => `Versie ${version}, ${from} tot ${to ?? 'nu'}`,
  weekAgo: 'een week geleden',
  now: 'nu',
};
