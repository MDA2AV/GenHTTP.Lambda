import type { EditorMessages } from '../../en/editor';

export const shared: EditorMessages['shared'] = {
  units: { s: 's', min: 'min', h: 'h', d: 'd' },
  amount: (value, unit) => `${value} ${unit}`,
  pair: (larger, smaller) => `${larger} ${smaller}`,
  never: 'nunca',
  justNow: 'agora mesmo',
  ago: (span) => `há ${span}`,
  in: (span) => `em ${span}`,
  origins: {
    agent: 'agente',
    template: 'modelo',
    admin: 'operador',
    system: 'plataforma',
    api: 'API / editor',
    unknown: 'desconhecido',
  },
  endings: {
    replaced: 'substituído por um deploy mais novo',
    stopped: 'tirado do ar',
    expired: 'expirou sem uso',
    admin: 'tirado do ar pelo operador',
    ended: 'encerrado',
  },
  whatThisIs: 'O que é isto',
  byAgent: 'por um agente',
  writtenByAgent: 'Escrito por um agente',
  more: 'Mais',
  of: (used, total) => `${used} de ${total}`,
  online: (version) => `No ar · v${version}`,
  onlineTitle: (version) => `No ar, servindo a versão ${version}`,
  offline: 'Fora do ar',
  offlineTitle: 'Fora do ar: nada está sendo servido',
  premium:
    'Premium: pode responder em um domínio próprio, tem mais espaço para código, assets e dados, e fica no ar mesmo sem movimento',
  demo: 'Demo: mantida no ar por esta instalação, somente leitura',
  tier: (tier) => `Plano ${tier}`,
  entrances: {
    title: 'Acessado por',
    note: 'Desde que o servidor iniciou, incluindo conexões WebSocket.',
  },
  chart: {
    showChart: 'Mostrar gráfico',
    showValues: 'Mostrar valores',
    none: 'Nenhuma medição ainda.',
    time: 'Hora',
  },
  diagnostics: {
    compiles: 'O código compila.',
    none: 'Nenhuma mensagem ainda. Verifique ou faça deploy para compilar seu código.',
    line: (line) => `linha ${line}`,
  },
};
