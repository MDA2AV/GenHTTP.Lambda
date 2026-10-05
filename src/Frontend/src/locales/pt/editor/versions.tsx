import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Uma versão é o programa (o código e os assets) e nunca muda depois de salva, então qualquer uma delas pode ser comparada e colocada de volta no ar exatamente como era. Cada uma guarda o que foi pedido e o que mudou. Para mudar a lambda, comece um rascunho: ele vira a próxima versão quando estiver tudo certo. Quando passam de ${limit}, as mais antigas são removidas; a que está no ar, nunca.`,
  none: 'Nenhuma versão ainda.',
  noDescription: 'Sem descrição',
  online: 'no ar',
  putOnline: 'Colocar esta versão no ar',
  rollBackTitle: 'Colocar esta versão antiga de volta no ar',
  deploy: 'Fazer deploy',
  rollBack: 'Reverter',
  readFailed: 'Não foi possível ler esta versão.',
  comparing: 'Comparando…',
  unchanged: 'Nada mudou em relação à versão anterior.',
  first: 'A primeira versão.',
  status: { added: 'adicionado', removed: 'removido', changed: 'alterado', same: 'igual' },
  groups: {
    code: 'Código',
    assets: 'Assets',
    build: 'Build',
    context: 'Documentação e testes',
  },
  browse: 'Ver os arquivos',
  docs: 'Ler a documentação',
  build: 'Ver aquilo a partir do qual é construído',
  edit: 'Editar a partir daqui',
  feature: 'Começar um rascunho a partir daqui',
  featureTitle:
    'Trabalhar numa mudança desta versão ao lado da lambda e mesclar na próxima versão quando estiver tudo certo',
  binary: 'Não é texto, então não há linhas para comparar.',
  tooLarge: 'Grande demais para comparar linha a linha.',
};
