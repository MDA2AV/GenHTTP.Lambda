import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Uma versão é o programa (o código e os assets) e nunca muda depois de guardada, por isso qualquer uma pode ser comparada e voltar a ficar online exatamente como era. Cada uma guarda o que foi pedido e o que mudou. Para alterar a lambda, começa um rascunho: passa a ser a próxima versão quando estiver bem. Quando há mais de ${limit}, as mais antigas são removidas; a que está online, nunca.`,
  none: 'Ainda não há versões.',
  noDescription: 'Sem descrição',
  online: 'online',
  putOnline: 'Pôr esta versão online',
  rollBackTitle: 'Voltar a pôr online esta versão anterior',
  deploy: 'Fazer deploy',
  rollBack: 'Reverter',
  readFailed: 'Não foi possível ler esta versão.',
  comparing: 'A comparar…',
  unchanged: 'Nada mudou em relação à versão anterior.',
  first: 'A primeira versão.',
  status: { added: 'adicionado', removed: 'removido', changed: 'alterado', same: 'igual' },
  groups: {
    code: 'Código',
    assets: 'Assets',
    build: 'Compilação',
    context: 'Documentação e testes',
  },
  browse: 'Ver os ficheiros',
  docs: 'Ler a documentação',
  build: 'Ver aquilo a partir do qual é compilada',
  edit: 'Editar a partir daqui',
  feature: 'Começar um rascunho a partir daqui',
  featureTitle: 'Trabalhar numa alteração desta versão ao lado da lambda, e integrá-la na próxima versão quando estiver bem',
  binary: 'Não é texto, por isso não há linhas para comparar.',
  tooLarge: 'Demasiado grande para comparar linha a linha.',
};
