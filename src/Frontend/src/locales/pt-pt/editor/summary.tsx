import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: 'A ver como está…',
  readDocs: 'Ler a documentação',
  hint: (since, kept, retention, tier) =>
    `O tráfego é contado desde o último arranque do servidor (${since}). ` +
    (kept
      ? `Uma lambda fica online enquanto é usada, e é removida ao fim de ${retention} dias sem visitas nem alterações.`
      : `Esta lambda está no plano ${tier}, que a mantém online e guardada por mais parada que esteja.`),
  onlineFor: (duration, version) => (
    <>
      Online há {duration('algum tempo')}, a servir a versão {version}.
    </>
  ),
  offline: 'Offline. Não é servido nada até fazeres deploy de uma versão.',
  nothing: 'Ainda não foi escrito nada.',
  requestsToday: 'pedidos hoje',
  lastHour: (count) => `${count} na última hora`,
  hourly: 'Pedidos por hora nas últimas 24 horas',
  failed: 'com erro',
  failedTitle: (failed, rejected) =>
    `${failed} erros de servidor, ${rejected} não encontrados ou recusados, nas últimas 24 horas`,
  average: 'tempo médio de resposta',
  noneYet: 'ainda nenhuma',
  lastVisit: 'última visita',
  problems: 'Algo correu mal recentemente',
  openLog: 'Abrir o log',
  latest: 'Última alteração',
  allVersions: 'Todas as versões',
  noDescription: 'Sem descrição',
  version: (version) => `Versão ${version}`,
  notOnline: 'ainda não está online',
  wanted: 'O que foi pedido',
  noVersions: 'Ainda não há versões.',
  inProgress: 'Em curso',
  allFeatures: 'Todos os rascunhos',
  previewOnline: 'A pré-visualização está online',
  previewOffline: 'A pré-visualização está offline',
  behind: 'desatualizado',
  storage: 'Armazenamento',
  versionAllowance: 'Código e recursos',
  data: 'Dados',
};
