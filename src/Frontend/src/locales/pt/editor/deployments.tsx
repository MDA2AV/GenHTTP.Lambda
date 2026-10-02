import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Um deploy fica no ar enquanto está em uso${until ? ` (se ninguém usar, até ${until})` : ''}. Um novo deploy ou qualquer visita reinicia essa contagem.`,
  takeOffline: 'Tirar do ar',
  readFailed: 'Não foi possível ler o histórico.',
  reading: 'Lendo o histórico…',
  none: 'Nenhum deploy ainda.',
  noDescription: 'Sem descrição',
  deployed: (when, by) => `Deploy em ${when} por ${by}`,
  duration: 'Tempo no ar',
  online: 'no ar',
  short: {
    replaced: 'substituído',
    stopped: 'tirado do ar',
    expired: 'expirou',
    admin: 'pelo operador',
    ended: 'encerrado',
  },
  putBack: (version) => `Colocar a versão ${version} de volta no ar`,
  timeline: 'O que esteve no ar nos últimos sete dias',
  block: (version, from, to) => `Versão ${version}, de ${from} até ${to ?? 'agora'}`,
  weekAgo: 'há uma semana',
  now: 'agora',
};
