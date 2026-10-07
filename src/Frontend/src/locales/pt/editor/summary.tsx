import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: 'Vendo como ela está…',
  readDocs: 'Ler a documentação',
  hint: (since, kept, retention, tier) =>
    `O tráfego é contado desde o último reinício do servidor (${since}). ` +
    (kept
      ? `Uma lambda fica no ar enquanto está em uso, e é removida após ${retention} dias sem visitas nem mudanças.`
      : `Esta lambda está no plano ${tier}, que a mantém no ar e guardada mesmo sem movimento.`),
  onlineFor: (duration, version) => (
    <>
      No ar há {duration('algum tempo')}, servindo a versão {version}.
    </>
  ),
  offline: 'Fora do ar. Nada é servido até você fazer deploy de uma versão.',
  nothing: 'Nada foi escrito ainda.',
  requestsToday: 'requisições hoje',
  lastHour: (count) => `${count} na última hora`,
  hourly: 'Requisições por hora nas últimas 24 horas',
  failed: 'com falha',
  failedTitle: (failed, rejected) =>
    `${failed} erros de servidor, ${rejected} não encontradas ou recusadas, nas últimas 24 horas`,
  average: 'tempo médio de resposta',
  noneYet: 'nenhuma ainda',
  lastVisit: 'última visita',
  problems: 'Algo deu errado recentemente',
  openLog: 'Abrir o log',
  latest: 'Última mudança',
  allVersions: 'Todas as versões',
  noDescription: 'Sem descrição',
  version: (version) => `Versão ${version}`,
  notOnline: 'ainda não está no ar',
  wanted: 'O que foi pedido',
  noVersions: 'Nenhuma versão ainda.',
  inProgress: 'Em andamento',
  allFeatures: 'Todos os rascunhos',
  previewOnline: 'A prévia está no ar',
  previewOffline: 'A prévia está fora do ar',
  behind: 'desatualizado',
  storage: 'Armazenamento',
  versionAllowance: 'Código e recursos',
  data: 'Dados',
};
