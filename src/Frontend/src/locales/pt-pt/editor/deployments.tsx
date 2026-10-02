import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Um deploy fica online enquanto é usado${until ? ` (se ninguém o usar, até ${until})` : ''}. Um novo deploy, ou qualquer visita, reinicia essa contagem.`,
  takeOffline: 'Pôr offline',
  readFailed: 'Não foi possível ler o histórico.',
  reading: 'A ler o histórico…',
  none: 'Ainda não houve nenhum deploy.',
  noDescription: 'Sem descrição',
  deployed: (when, by) => `Deploy feito a ${when} (${by})`,
  duration: 'Tempo online',
  online: 'online',
  short: {
    replaced: 'substituído',
    stopped: 'posto offline',
    expired: 'expirou',
    admin: 'pelo operador',
    ended: 'terminado',
  },
  putBack: (version) => `Voltar a pôr a versão ${version} online`,
  timeline: 'O que esteve online nos últimos sete dias',
  block: (version, from, to) => `Versão ${version}, de ${from} a ${to ?? 'agora'}`,
  weekAgo: 'há uma semana',
  now: 'agora',
};
