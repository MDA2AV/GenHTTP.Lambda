import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Não foi possível ler o log.',
  hint: (capturing) =>
    'Requisições, o que a lambda imprimiu e o que deu errado, em tempo real.' +
    (capturing ? '' : ' Esta instalação não guarda o que as lambdas imprimem, então só aparecem requisições e erros.') +
    ' O log fica em memória e é compartilhado com todas as lambdas daqui. Por isso, guarda de minutos a horas e fica vazio depois de um reinício. Os endereços dos visitantes não aparecem.',
  featureHint: (capturing) =>
    'O que a prévia deste rascunho respondeu, o que imprimiu e o que deu errado, em tempo real.' +
    (capturing ? '' : ' Esta instalação não guarda o que as lambdas imprimem, então só aparecem requisições e erros.') +
    ' Fica separado do log da própria lambda, que nunca mostra a prévia. O log fica em memória, por isso guarda de minutos a horas.',
  nothingPreview: 'Nada ainda. Abra a prévia do rascunho e as requisições aparecem aqui.',
  search: 'Buscar',
  searchLabel: 'Buscar no log',
  resume: 'Mostrar novas linhas conforme chegam',
  pause: 'Pausar novas linhas enquanto você lê',
  paused: 'Pausado',
  live: 'Ao vivo',
  show: 'Mostrar',
  all: 'Tudo',
  requests: 'Requisições',
  output: 'Saída',
  problems: 'Problemas',
  reading: 'Lendo o log…',
  noProblems: 'Nada deu errado, pelo menos no que o log ainda lembra.',
  nothing: 'Nada ainda. Abra o endereço da lambda e as requisições aparecem aqui.',
  noMatch: 'Nenhum resultado.',
  identical: (count) => `${count} linhas idênticas`,
  at: (domain) => `, em ${domain}`,
  from: (country) => `, de ${country}`,
};
