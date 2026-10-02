import type { EditorMessages } from '../../en/editor';

export const shared: EditorMessages['shared'] = {
  units: { s: 's', min: 'min', h: 'h', d: 'd' },
  amount: (value, unit) => `${value} ${unit}`,
  pair: (larger, smaller) => `${larger} ${smaller}`,
  never: 'nunca',
  justNow: 'agora mesmo',
  ago: (span) => `há ${span}`,
  in: (span) => `daqui a ${span}`,
  origins: {
    agent: 'agente',
    template: 'modelo',
    admin: 'operador',
    system: 'plataforma',
    api: 'API / editor',
    unknown: 'desconhecido',
  },
  endings: {
    replaced: 'substituído por um deploy mais recente',
    stopped: 'posto offline',
    expired: 'expirou por falta de uso',
    admin: 'posto offline pelo operador',
    ended: 'terminado',
  },
  whatThisIs: 'O que é isto',
  byAgent: 'por um agente',
  writtenByAgent: 'Escrito por um agente',
  more: 'Mais',
  of: (used, total) => `${used} de ${total}`,
  online: (version) => `Online · v${version}`,
  onlineTitle: (version) => `Online, a servir a versão ${version}`,
  offline: 'Offline',
  offlineTitle: 'Offline: não está a ser servido nada',
  premium:
    'Premium: pode responder num domínio próprio, tem mais espaço para código, assets e dados, e fica online mesmo sem movimento',
  demo: 'Demo: mantida online por esta instalação, só de leitura',
  tier: (tier) => `Plano ${tier}`,
  entrances: {
    title: 'Acedido através de',
    note: 'Desde que o servidor arrancou, incluindo ligações WebSocket.',
  },
  chart: {
    showChart: 'Mostrar gráfico',
    showValues: 'Mostrar valores',
    none: 'Ainda sem medições.',
    time: 'Hora',
  },
  diagnostics: {
    compiles: 'O código compila.',
    none: 'Ainda sem mensagens. Verifica ou faz deploy para compilar o teu código.',
    line: (line) => `linha ${line}`,
  },
};
