import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Código',
  version: (version) => `versão ${version}`,
  edited: ', editada',
  online: ', no ar',
  loadFailed: 'Não foi possível carregar essa versão.',
  compiles: 'Compila.',
  notYet: 'Ainda não compila.',
  checkFailed: 'Não foi possível verificar o código.',
  saved: (version) => `Salvo como versão ${version}.`,
  featureSaved: 'Salvo no rascunho. Faça deploy da prévia para testar.',
  featureLoadFailed: 'Não foi possível carregar o rascunho.',
  previewOnline: 'A prévia está no ar.',
  previewRefused: 'A prévia não mudou. Veja abaixo o que o compilador disse.',
  isOnline: (version) => `A versão ${version} está no ar.`,
  notOnline: 'Não foi para o ar. Veja abaixo o que o compilador disse.',
  failed: 'Não deu certo.',
  unchanged: 'Nada mudou desde o último salvamento.',
  demo: 'É uma demo, então tudo aqui é somente leitura. Para mudar, crie uma lambda sua a partir dela.',
  hint: (b) => (
    <>
      Os arquivos de uma versão. O {b('código')} dela é o programa e tudo o que fica guardado com ele: os arquivos .cs
      no topo são compilados, e todo outro arquivo (a documentação, os testes, aquilo a partir do qual um front end é
      gerado) fica guardado com a versão e nunca é compilado nem servido. Os {b('recursos')} dela (páginas, scripts,
      estilos, imagens, as migrações do banco de dados) são lidos e servidos enquanto ela roda, e são públicos onde o
      código os serve. Salvar cria uma nova versão e não mexe no que está no ar; para testar uma mudança antes, comece
      um rascunho. Ctrl-S salva, F12 vai para uma declaração.
    </>
  ),
  hintFeature: (b) => (
    <>
      Os arquivos deste rascunho: o {b('código')} (os arquivos .cs no topo são compilados, o resto fica guardado com
      ele) e os {b('recursos')}, lidos e servidos enquanto ele roda. Salvar os mantém no rascunho e os mostra no
      endereço do próprio rascunho; seus visitantes não veem nada disso até você colocar o rascunho no ar.
    </>
  ),
  inFeature: (name) => `em “${name}”`,
  changedElsewhere: 'O rascunho foi salvo em outro lugar depois que você abriu (talvez pelo agente). Carregue o que está salvo antes de salvar aqui; suas alterações não seriam salvas por cima.',
  readAgain: 'Carregar o que está salvo',
  newer: (version) => `A versão ${version} é mais nova do que a que está aberta aqui.`,
  check: 'Verificar',
  save: 'Salvar',
  deploy: 'Fazer deploy',
  deployPreviewTitle: 'Salvar e colocar o rascunho no ar no endereço próprio dele, para testar',
  binary: (size) => `Não é texto, então não há o que editar aqui. Tem ${size}.`,
  saveAndDeploy: 'Salvar e fazer deploy',
  saveVersion: 'Salvar nova versão',
  fromOlder: (version, newest) =>
    `Isto parte da versão ${version}, e a versão ${newest} é mais nova. Salvar faz disto a versão mais nova, sem o que veio depois da versão ${version}.`,
  featureInstead: (start) => (
    <>
      Só testando uma ideia? {start('Coloque num rascunho novo')}: ele ganha um endereço próprio, e nenhuma versão é
      salva até estar tudo certo.
    </>
  ),
  cancel: 'Cancelar',
  what: 'O que muda? Opcional, aparece no histórico.',
  placeholder: 'Adiciona um formulário de contato',
  goToDefinition: 'Ir para a definição',
  versionLabel: 'Versão',
  shown: (version, online, newest) =>
    `Versão ${version}${online ? ', no ar' : newest ? ', mais recente' : ''}`,
  optionOnline: ' (no ar)',
  switchUnsaved: 'O que você mudou aqui não foi salvo. Abrir a outra versão mesmo assim?',
  noVersion: 'Ainda não há nenhuma versão para mostrar.',
  label: 'Arquivos',
  codeGroup: 'Código',
  codeWhy: 'Nunca servido. Os arquivos .cs no topo são compilados; o resto fica guardado com a versão.',
  resources: 'Recursos',
  resourcesPublic: 'Públicos: esta versão os serve com Resources.',
  resourcesPrivate: 'Enviados com a versão, mas esta versão não os serve.',
  noResources: 'Nenhum nesta versão.',
  count: (files) => (files === 1 ? '1 arquivo' : `${files} arquivos`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Esta versão ocupa ${used} dos ${of} que uma versão pode ter, contando o código e os recursos juntos.`,
  scope: (data) => (
    <>O que a lambda guarda enquanto roda é igual para todas as versões e fica em {data('Dados')}.</>
  ),
  download: 'Baixar',
  newIn: (group) => `Novo arquivo em ${group}`,
  uploadIn: (group) => `Enviar para ${group}`,
  pick: 'Escolha um arquivo para ver o que há nele.',
};
