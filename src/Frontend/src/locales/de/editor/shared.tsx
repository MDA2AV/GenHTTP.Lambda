import type { EditorMessages } from '../../en/editor';

export const shared: EditorMessages['shared'] = {
  units: { s: 's', min: 'min', h: 'h', d: 'd' },
  amount: (value, unit) => `${value} ${unit}`,
  pair: (larger, smaller) => `${larger} ${smaller}`,
  never: 'nie',
  justNow: 'gerade eben',
  ago: (span) => `vor ${span}`,
  in: (span) => `in ${span}`,
  origins: {
    agent: 'Agent',
    template: 'Vorlage',
    admin: 'Betreiber',
    system: 'Plattform',
    api: 'API / Editor',
    unknown: 'unbekannt',
  },
  endings: {
    replaced: 'durch ein neueres Deployment ersetzt',
    stopped: 'offline genommen',
    expired: 'abgelaufen, weil ungenutzt',
    admin: 'vom Betreiber offline genommen',
    ended: 'beendet',
  },
  whatThisIs: 'Was ist das?',
  byAgent: 'von einem Agenten',
  writtenByAgent: 'Von einem Agenten geschrieben',
  more: 'Mehr',
  of: (used, total) => `${used} von ${total}`,
  online: (version) => `Online · v${version}`,
  onlineTitle: (version) => `Online, liefert Version ${version} aus`,
  offline: 'Offline',
  offlineTitle: 'Offline: Es wird nichts ausgeliefert',
  premium:
    'Premium: kann unter einer eigenen Domain antworten, hat mehr Platz für Code, Assets und Daten und bleibt online, egal wie wenig los ist',
  demo: 'Demo: von dieser Installation online gehalten, schreibgeschützt',
  tier: (tier) => `Tarif ${tier}`,
  entrances: {
    title: 'Aufgerufen über',
    note: 'Seit dem Start des Servers, Websocket-Verbindungen eingeschlossen.',
  },
  chart: {
    showChart: 'Diagramm zeigen',
    showValues: 'Werte zeigen',
    none: 'Noch keine Messwerte.',
    time: 'Zeit',
  },
  diagnostics: {
    compiles: 'Der Code kompiliert.',
    none: 'Noch keine Meldungen. Prüfen oder deployen Sie, um Ihren Code zu kompilieren.',
    line: (line) => `Zeile ${line}`,
  },
};
