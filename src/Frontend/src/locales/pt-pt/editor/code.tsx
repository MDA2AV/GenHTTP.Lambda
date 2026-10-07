import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Código',
  version: (version) => `versão ${version}`,
  edited: ', editada',
  loadFailed: 'Não foi possível carregar essa versão.',
  compiles: 'Compila.',
  notYet: 'Ainda não compila.',
  checkFailed: 'Não foi possível verificar o código.',
  saved: (version) => `Guardado como versão ${version}.`,
  featureSaved: 'Guardado no rascunho. Faz deploy da pré-visualização para o experimentares.',
  featureLoadFailed: 'Não foi possível carregar o rascunho.',
  previewOnline: 'A pré-visualização está online.',
  previewRefused: 'A pré-visualização não mudou. Vê abaixo o que o compilador disse.',
  isOnline: (version) => `A versão ${version} está online.`,
  notOnline: 'Não ficou online. Vê abaixo o que o compilador disse.',
  failed: 'Não correu bem.',
  unchanged: 'Nada mudou desde que guardaste pela última vez.',
  demo: 'É uma demo, por isso aqui tudo é só de leitura. Para a alterar, cria uma lambda tua a partir dela.',
  hint: (b) => (
    <>
      Os ficheiros de uma versão. O seu {b('código')} é o programa e tudo o que é guardado com ele: os ficheiros .cs no
      topo são compilados, e todos os outros ficheiros - a documentação, os testes, aquilo a partir do qual um front end
      é compilado - são guardados com a versão e nunca são compilados nem servidos. Os seus {b('recursos')} - páginas,
      scripts, estilos, imagens, as migrações da base de dados - são lidos e servidos enquanto ela corre, e são públicos
      onde o código os serve. Guardar cria uma nova versão e não toca no que está online; para experimentar primeiro uma
      alteração, começa um rascunho. Ctrl-S guarda, F12 vai para uma declaração.
    </>
  ),
  hintFeature: (b) => (
    <>
      Os ficheiros deste rascunho: o seu {b('código')} - os ficheiros .cs no topo são compilados, o resto é guardado com
      ele - e os seus {b('recursos')}, lidos e servidos enquanto corre. Guardar mantém-nos no rascunho e mostra-os no
      endereço do próprio rascunho; os teus visitantes não veem nada disto até pores o rascunho online.
    </>
  ),
  inFeature: (name) => `em «${name}»`,
  changedElsewhere: 'O rascunho foi guardado noutro sítio desde que o abriste (talvez pelo agente). Carrega o que está guardado antes de guardares aqui; as tuas alterações não seriam guardadas por cima.',
  readAgain: 'Carregar o que está guardado',
  newer: (version) => `A versão ${version} é mais recente do que a que está aberta aqui.`,
  check: 'Verificar',
  save: 'Guardar',
  deploy: 'Fazer deploy',
  deployPreviewTitle: 'Guardar e pôr o rascunho online no endereço próprio dele, para o experimentares',
  binary: (size) => `Não é texto, por isso não há nada para editar aqui. Tem ${size}.`,
  saveAndDeploy: 'Guardar e fazer deploy',
  saveVersion: 'Guardar uma nova versão',
  fromOlder: (version, newest) =>
    `Isto parte da versão ${version}, e a versão ${newest} é mais recente. Guardar torna-o a versão mais recente, sem o que veio depois da versão ${version}.`,
  featureInstead: (start) => (
    <>
      Estás a experimentar algo? {start('Põe-no antes num novo rascunho')}: tem um endereço próprio, e nenhuma versão é
      guardada até estar bem.
    </>
  ),
  cancel: 'Cancelar',
  what: 'O que é que muda? Opcional: aparece no histórico.',
  placeholder: 'Adiciona um formulário de contacto',
  goToDefinition: 'Ir para a definição',
  versionLabel: 'Versão',
  shown: (version, online, newest) =>
    `Versão ${version}${online ? ', online' : newest ? ', a mais recente' : ''}`,
  optionOnline: ' (online)',
  switchUnsaved: 'O que alteraste aqui não está guardado. Abrir mesmo assim a outra versão?',
  noVersion: 'Ainda não há nenhuma versão para mostrar.',
  label: 'Ficheiros',
  codeGroup: 'Código',
  codeWhy: 'Nunca são servidos. Os ficheiros .cs no topo são compilados; o resto é guardado com a versão.',
  resources: 'Recursos',
  resourcesPublic: 'Públicos: esta versão serve-os com Resources.',
  resourcesPrivate: 'Seguem com a versão, mas esta versão não os serve.',
  noResources: 'Nenhum nesta versão.',
  count: (files) => (files === 1 ? '1 ficheiro' : `${files} ficheiros`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Esta versão ocupa ${used} dos ${of} que uma versão pode ter, com o código e os recursos juntos.`,
  scope: (data) => (
    <>O que a lambda guarda enquanto corre é igual para todas as versões, e está em {data('Dados')}.</>
  ),
  download: 'Transferir',
  newIn: (group) => `Novo ficheiro em ${group}`,
  uploadIn: (group) => `Carregar para ${group}`,
  pick: 'Escolhe um ficheiro para ver o que tem.',
};
