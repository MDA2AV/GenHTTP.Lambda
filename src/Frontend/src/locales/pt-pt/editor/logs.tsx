import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Não foi possível ler o log.',
  hint: (capturing) =>
    'Pedidos, o que a lambda escreveu na consola e o que correu mal, em tempo real.' +
    (capturing ? '' : ' Esta instalação não guarda o que as lambdas escrevem na consola, por isso só aparecem pedidos e erros.') +
    ' Fica em memória e é partilhado por todas as lambdas daqui, por isso recua entre alguns minutos e algumas horas, e fica vazio depois de um reinício. Os endereços dos visitantes não são mostrados.',
  featureHint: (capturing) =>
    'O que a pré-visualização deste rascunho respondeu, escreveu na consola e lançou, em tempo real.' +
    (capturing ? '' : ' Esta instalação não guarda o que as lambdas escrevem na consola, por isso só aparecem pedidos e erros.') +
    ' Fica separado do log da própria lambda, que nunca mostra a pré-visualização. Fica em memória, por isso recua entre alguns minutos e algumas horas.',
  nothingPreview: 'Ainda nada. Abre a pré-visualização do rascunho e os pedidos aparecem aqui.',
  search: 'Pesquisar',
  searchLabel: 'Pesquisar no log',
  resume: 'Mostrar as novas linhas à medida que chegam',
  pause: 'Parar de acrescentar linhas enquanto lês',
  paused: 'Em pausa',
  live: 'Em direto',
  show: 'Mostrar',
  all: 'Tudo',
  requests: 'Pedidos',
  output: 'Consola',
  problems: 'Problemas',
  reading: 'A ler o log…',
  noProblems: 'Nada correu mal, pelo menos que o log ainda se lembre.',
  nothing: 'Ainda nada. Abre o endereço da lambda e os pedidos aparecem aqui.',
  noMatch: 'Nenhum resultado.',
  identical: (count) => `${count} linhas idênticas`,
  at: (domain) => `, em ${domain}`,
  from: (country) => `, de ${country}`,
};
