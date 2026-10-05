import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Código',
  version: (version) => `versão ${version}`,
  edited: ', editada',
  online: ', online',
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
  demo: 'É uma demo, por isso aqui tudo é só de leitura. Para a alterar, cria uma lambda tua a partir dela. ',
  edit: 'Edita o código à mão. Guardar cria uma nova versão e não mexe no que está online; fazer deploy põe-na online. Para experimentares uma alteração primeiro, começa um rascunho. ',
  editFeature:
    'O código deste rascunho. Guardar mantém-no no rascunho: nada do que os visitantes da lambda recebem muda. Fazer deploy põe-no online no endereço próprio do rascunho, para o experimentares; integrar o rascunho faz dele a próxima versão. ',
  inFeature: (name) => `em «${name}»`,
  changedElsewhere: 'O rascunho foi guardado noutro sítio desde que o abriste (talvez pelo agente). Carrega o que está guardado antes de guardares aqui; as tuas alterações não seriam guardadas por cima.',
  readAgain: 'Carregar o que está guardado',
  files: (entry, cs, context) => (
    <>
      {entry} devolve o que é servido, os outros ficheiros {cs} têm tipos, e qualquer outro ficheiro é servido tal como
      está, exceto o que está em {context}: a documentação, os testes e aquilo a partir do qual é compilado, que nunca
      são compilados nem servidos. Ctrl-S guarda, F12 vai para a declaração.
    </>
  ),
  newer: (version) => ` A versão ${version} é mais recente do que a que está aberta aqui.`,
  check: 'Verificar',
  save: 'Guardar',
  deploy: 'Fazer deploy',
  deployPreviewTitle: 'Guardar e pôr o rascunho online no endereço próprio dele, para o experimentares',
  binary: (size) => `Não é texto, por isso não há nada para editar. É servido tal como está e tem ${size} kB.`,
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
};
