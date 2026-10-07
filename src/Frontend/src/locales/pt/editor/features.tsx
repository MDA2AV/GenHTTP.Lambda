import type { EditorMessages } from '../../en/editor';

export const features: EditorMessages['features'] = {
  hint:
    'Um rascunho é uma cópia do seu app para testar uma mudança antes que alguém a veja, com endereço e dados de teste próprios. Coloque no ar quando estiver certo; até lá, seus visitantes continuam recebendo o que está no ar agora.',
  newFeature: 'Novo rascunho',
  full: (limit) => `Já existem ${limit} rascunhos, o máximo permitido. Coloque um no ar ou descarte-o antes.`,
  emptyTitle: 'Nenhum rascunho',
  emptyText:
    'Um rascunho é uma cópia do seu app para testar uma mudança antes de ela ir ao ar. Quando o agente deixar uma mudança para você testar, ela aparece aqui.',
  start: 'Novo rascunho',
  askAgentNew: 'Pedir uma mudança ao agente',
  noChange: 'Ainda não diz o que muda',
  behindTitle: 'Seu app mudou desde que este rascunho começou',
  behind: () => 'desatualizado',
  branchTitle: 'O branch em que este rascunho está no repositório git do app',
  previewOnline: 'prévia no ar',
  previewOutdated: 'a prévia mostra um salvamento anterior',
  previewOffline: 'prévia fora do ar',
  changed: 'alterado',
  openPreview: 'Testar',
  openPreviewTitle: 'Abrir a prévia em nova aba',
  count: (open, limit) => `${open} de ${limit} rascunhos`,
  loading: 'Carregando o rascunho…',
  readFailed: 'Não foi possível ler o rascunho.',

  newTitle: 'Novo rascunho',
  newText:
    'Uma cópia do seu app e dos dados dele, com endereço próprio. Mude e teste lá: seus visitantes não veem nada disso até você colocar no ar.',
  newTextFiles:
    'O que você digitou vai para o rascunho em vez de virar uma versão, para você testar no endereço próprio dele antes de ir ao ar.',
  name: 'Nome',
  namePlaceholder: 'Ranking',
  wanted: 'O que ele deve fazer?',
  wantedPlaceholder: 'Opcional. Guarde as dez melhores pontuações e mostre depois de cada partida.',
  olderBase: (newest) =>
    `Este começa de uma versão mais antiga, então já nasce desatualizado: antes de ir ao ar, é preciso incorporar o que mudou até a versão ${newest}.`,
  create: 'Começar o rascunho',
  createFailed: 'Não foi possível criar o rascunho.',
  retry: 'Tentar de novo',
  madeNotSaved: (name) =>
    `O rascunho “${name}” foi criado, mas o que você digitou ainda não pôde ser salvo nele. Tente de novo, ou feche isto e encontre o rascunho em Rascunhos.`,
  created: (name) => `O rascunho “${name}” foi criado.`,
  cancel: 'Cancelar',

  featureHint:
    'Uma cópia do seu app para testar esta mudança. A prévia tem endereço e dados de teste próprios, então seus visitantes não veem nada disso até você colocar no ar.',
  askAgent: 'Pedir ao agente',
  askCatchUp: 'Pedir ao agente para atualizá-lo',
  catchUp: 'Atualize este rascunho com a versão mais nova do app, mantendo o que ele muda.',
  editCode: 'Editar o código',
  deployPreview: 'Iniciar a prévia',
  updatePreview: 'Atualizar a prévia',
  previewDeployed: 'A prévia está no ar.',
  previewFailed: 'Não foi possível iniciar a prévia.',
  previewStopped: 'A prévia está fora do ar.',
  previewRejected: 'A prévia não mudou',
  previewNotCompiling: 'Não compila, então a prévia continua mostrando a última versão que compilou.',
  started: 'Começou',
  changes: () => 'Arquivos alterados',
  noChanges: () => 'Nada foi alterado ainda.',
  editNotes: 'Nome e notas',
  what: 'O que ele muda?',
  whatPlaceholder: 'Adiciona um ranking que guarda as dez melhores pontuações',
  missed: () => 'O que mudou no seu app desde que ele começou',
  missedNothing: 'Nada nos arquivos.',

  behindText: (_base, newest) =>
    `A versão ${newest} do seu app foi salva depois que este rascunho começou. Colocar o rascunho no ar agora desfaria o que ela mudou, então ele precisa ser atualizado primeiro - o agente pode fazer isso por você.`,
  moveBase: 'Marcar como atualizado',
  close: 'Fechar',
  mergeTitle: (name) => `Colocar “${name}” no ar`,
  mergeTitleShort: 'Fazer dele a nova versão do seu app e colocar no ar',
  leaks: (path, files) =>
    `Em ${files}, há links para ${path}, que é o seu app no ar. A partir da prévia, esses links leem e alteram os dados reais dele, em vez dos dados de teste. Peça ao agente que use links sem essa parte (“api/items”).`,
  mergeButton: 'Colocar no ar',
  saveFirst: 'Salve suas mudanças primeiro: a prévia e colocar no ar usam o que está salvo.',
  mergeAndDeploy: () => 'Colocar no ar',
  mergeText: (version) =>
    `Ele vira a versão ${version} do seu app e vai ao ar. Os dados do seu app ficam como estão.`,
  deployTooNote: (active) => `A versão ${active} continua a um clique, nas versões.`,
  deployTooOffline: 'A lambda está fora do ar agora; isto a coloca no ar.',
  notCompiling: 'Não compila, então não foi colocado no ar. Corrija no rascunho primeiro.',
  mergeFailed: 'Não foi possível colocar o rascunho no ar.',
  merged: (version) => `Salvo como versão ${version}.`,
  mergedOnline: (version) => `A versão ${version} está no ar.`,

  notesTitle: 'Nome e notas',
  save: 'Salvar',
  saveFailed: 'Não foi possível salvar.',

  baseTitle: 'Marcar como atualizado?',
  baseText: () =>
    'Só um rascunho que contém o que a versão mais recente mudou pode ir ao ar sem desfazer essas mudanças. Se elas já estão neste rascunho - trazidas por você ou pelo agente - marque-o como atualizado.',
  moveTo: () => 'Marcar como atualizado',
  baseWarning: 'Nada confere isso. Se as mudanças não estão no rascunho, colocá-lo no ar as desfaz.',

  deleteTitle: (name) => `Descartar “${name}”?`,
  deleteText:
    'O código, a prévia e os dados de teste dele são excluídos de vez. Seu app e as versões dele não são afetados.',
  keep: 'Manter',
  deleteForGood: 'Descartar',
  deleteFailed: 'Não foi possível descartar o rascunho.',
  deleted: (name) => `O rascunho “${name}” foi descartado.`,

  all: 'Todos os rascunhos',
  actions: 'Mais opções deste rascunho',
  download: 'Baixar como zip',
  stopPreview: 'Tirar a prévia do ar',
  delete: 'Descartar este rascunho',
  viewsLabel: 'O rascunho',
  views: {
    overview: 'Rascunho',
    docs: 'Documentação',
    code: 'Código',
    tests: 'Testes',
    data: 'Dados de teste',
    logs: 'Logs',
  },
  missingTitle: 'Este rascunho não existe mais',
  missingText: 'Ele foi colocado no ar ou descartado. As versões mostram o que aconteceu com ele.',
};
