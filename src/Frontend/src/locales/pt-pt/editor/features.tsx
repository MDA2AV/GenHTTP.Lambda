import type { EditorMessages } from '../../en/editor';

export const features: EditorMessages['features'] = {
  hint:
    'Um rascunho é uma cópia da tua app para experimentares uma alteração antes de alguém a ver, com endereço e dados de teste próprios. Põe-no online quando estiver certo; até lá, os visitantes continuam a receber o que está online agora.',
  newFeature: 'Novo rascunho',
  full: (limit) => `Já existem ${limit} rascunhos, que é o máximo permitido. Põe um online ou elimina um primeiro.`,
  emptyTitle: 'Sem rascunhos',
  emptyText:
    'Um rascunho é uma cópia da tua app para experimentares uma alteração antes de ela ficar online. Quando o agente deixar uma alteração para experimentares, encontras o rascunho aqui.',
  start: 'Novo rascunho',
  askAgentNew: 'Pedir uma alteração ao agente',
  noChange: 'Ainda não diz o que altera',
  behindTitle: 'A tua app mudou desde que este rascunho começou',
  behind: () => 'desatualizado',
  previewOnline: 'pré-visualização a correr',
  previewOutdated: 'a pré-visualização mostra um estado anterior',
  previewOffline: 'pré-visualização parada',
  changed: 'alterado',
  openPreview: 'Experimentar',
  openPreviewTitle: 'Abrir a pré-visualização num novo separador',
  count: (open, limit) => `${open} de ${limit} rascunhos`,
  loading: 'A carregar o rascunho…',
  readFailed: 'Não foi possível ler o rascunho.',

  newTitle: 'Novo rascunho',
  newText:
    'Uma cópia da tua app e dos respetivos dados, com endereço próprio. Altera-a e experimenta-a aí: os visitantes não veem nada disto até a pores online.',
  newTextFiles:
    'O que escreveste vai para o rascunho em vez de passar a ser uma versão, por isso podes experimentá-lo no endereço próprio antes de ficar online.',
  name: 'Nome',
  namePlaceholder: 'Ranking',
  wanted: 'O que deve fazer?',
  wantedPlaceholder: 'Opcional. Guardar as dez melhores pontuações e mostrá-las depois de cada jogo.',
  olderBase: (newest) =>
    `Parte de uma versão mais antiga, por isso está desatualizado desde o início: antes de poder ficar online, é preciso trazer o que mudou até à versão ${newest}.`,
  create: 'Começar o rascunho',
  createFailed: 'Não foi possível começar o rascunho.',
  retry: 'Tentar novamente',
  madeNotSaved: (name) =>
    `O rascunho «${name}» foi iniciado, mas o que escreveste ainda não pôde ser colocado nele. Tenta novamente, ou fecha isto e procura-o em Rascunhos.`,
  created: (name) => `O rascunho «${name}» foi criado.`,
  cancel: 'Cancelar',

  featureHint:
    'Uma cópia da tua app para experimentares esta alteração. A pré-visualização tem endereço e dados de teste próprios, por isso os visitantes não veem nada disto até a pores online.',
  askAgent: 'Pedir ao agente',
  askCatchUp: 'Pedir ao agente para o atualizar',
  catchUp: 'Atualiza este rascunho com a versão mais recente da app, mantendo o que ele altera.',
  editCode: 'Editar o código',
  deployPreview: 'Iniciar a pré-visualização',
  updatePreview: 'Atualizar a pré-visualização',
  previewDeployed: 'A pré-visualização está online.',
  previewFailed: 'Não foi possível iniciar a pré-visualização.',
  previewStopped: 'A pré-visualização está offline.',
  previewRejected: 'A pré-visualização não mudou',
  previewNotCompiling: 'Não compila, por isso a pré-visualização continua a mostrar a última versão que compilou.',
  started: 'Começou',
  changes: () => 'Ficheiros alterados',
  noChanges: () => 'Ainda não há alterações.',
  editNotes: 'Nome e notas',
  what: 'O que é que altera?',
  whatPlaceholder: 'Adiciona um ranking com as dez melhores pontuações',
  missed: () => 'O que mudou na tua app desde que começou',
  missedNothing: 'Nada nos ficheiros.',

  behindText: (_base, newest) =>
    `A versão ${newest} da tua app foi guardada depois de este rascunho começar. Pô-lo online agora desfaria o que ela mudou, por isso tem de ser primeiro atualizado - o agente pode fazê-lo por ti.`,
  moveBase: 'Marcar como atualizado',
  close: 'Fechar',
  mergeTitle: (name) => `Pôr «${name}» online`,
  mergeTitleShort: 'Torná-lo a nova versão da tua app e pô-lo online',
  leaks: (path, files) =>
    `${files} têm ligações para ${path}, que é a tua app online. A partir da pré-visualização, essas ligações leem e alteram os dados reais dela, em vez dos dados de teste. Pede ao agente que as ligue sem essa parte («api/items»).`,
  mergeButton: 'Pôr online',
  saveFirst: 'Guarda primeiro as tuas alterações: a pré-visualização e pôr online usam o que está guardado.',
  mergeAndDeploy: () => 'Pôr online',
  mergeText: (version) =>
    `Passa a ser a versão ${version} da tua app e fica online. Os dados da tua app ficam como estão.`,
  deployTooNote: (active) => `A versão ${active} continua à distância de um clique nas versões.`,
  deployTooOffline: 'A tua app está offline; isto põe-na online.',
  notCompiling: 'Não compila, por isso não ficou online. Corrige-o primeiro no rascunho.',
  mergeFailed: 'Não foi possível pôr o rascunho online.',
  merged: (version) => `Guardado como versão ${version}.`,
  mergedOnline: (version) => `A versão ${version} está online.`,

  notesTitle: 'Nome e notas',
  save: 'Guardar',
  saveFailed: 'Não foi possível guardar.',

  baseTitle: 'Marcar como atualizado?',
  baseText: () =>
    'Só um rascunho que tenha o que a versão mais recente mudou pode ficar online sem o desfazer. Se essas alterações já estão neste rascunho - trazidas por ti ou pelo agente - marca-o como atualizado.',
  moveTo: () => 'Marcar como atualizado',
  baseWarning: 'Nada verifica isto. Se as alterações não estiverem no rascunho, pô-lo online desfá-las.',

  deleteTitle: (name) => `Eliminar «${name}»?`,
  deleteText:
    'O código, a pré-visualização e os dados de teste são eliminados de vez. A tua app e as versões dela não são afetadas.',
  keep: 'Manter',
  deleteForGood: 'Eliminar de vez',
  deleteFailed: 'Não foi possível eliminar o rascunho.',
  deleted: (name) => `O rascunho «${name}» foi eliminado.`,

  all: 'Todos os rascunhos',
  actions: 'Mais ações para este rascunho',
  download: 'Transferir como zip',
  stopPreview: 'Pôr a pré-visualização offline',
  delete: 'Eliminar este rascunho',
  viewsLabel: 'O rascunho',
  views: {
    overview: 'Rascunho',
    docs: 'Documentação',
    code: 'Código',
    tests: 'Testes',
    data: 'Dados de teste',
    logs: 'Logs',
  },
  missingTitle: 'Este rascunho já não existe',
  missingText: 'Foi posto online ou eliminado. As versões mostram o que aconteceu com ele.',
};
