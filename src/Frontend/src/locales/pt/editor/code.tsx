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
  demo: 'É uma demo, então tudo aqui é somente leitura. Para mudar, crie uma lambda sua a partir dela. ',
  edit: 'Edite o código à mão. Salvar cria uma nova versão e não mexe no que está no ar; fazer deploy coloca no ar. Para testar uma mudança antes, comece um rascunho. ',
  editFeature:
    'O código deste rascunho. Salvar mantém a mudança no rascunho: nada do que os visitantes da lambda recebem muda. Fazer deploy coloca no ar no endereço próprio do rascunho, para testar; mesclar o rascunho faz dele a próxima versão. ',
  inFeature: (name) => `em “${name}”`,
  changedElsewhere: 'O rascunho foi salvo em outro lugar depois que você abriu (talvez pelo agente). Carregue o que está salvo antes de salvar aqui; suas alterações não seriam salvas por cima.',
  readAgain: 'Carregar o que está salvo',
  files: (entry, cs, context) => (
    <>
      {entry} retorna o que é servido, outros arquivos {cs} guardam tipos, e qualquer outro arquivo é servido como
      está, exceto o que fica em {context}: a documentação e os testes, que nunca são compilados nem servidos. Ctrl-S
      salva, F12 vai para uma declaração.
    </>
  ),
  newer: (version) => ` A versão ${version} é mais nova do que a que está aberta aqui.`,
  check: 'Verificar',
  save: 'Salvar',
  deploy: 'Fazer deploy',
  deployPreviewTitle: 'Salvar e colocar o rascunho no ar no endereço próprio dele, para testar',
  binary: (size) => `Não é texto, então não há o que editar. É servido como está e tem ${size} kB.`,
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
};
