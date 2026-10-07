import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Documentação',
    titleSimple: 'Sobre o seu app',
    hint: 'O que é este app, para quem ele é e por quê, e por que ele é construído desse jeito. Os agentes escrevem a documentação a cada mudança, e ela é guardada com cada versão: uma versão mais antiga volta com a documentação que valia para ela.',
    hintSimple: 'Para que serve o seu app e por quê, do jeito que o agente entendeu a partir do que você pediu. Ele mantém isto atualizado a cada mudança.',
    inDraft: 'A documentação deste rascunho. Ela passa a ser a do seu app quando o rascunho entrar no ar.',
    pages: { product: 'Produto', decisions: 'Decisões' },
    emptyTitle: 'Nada escrito ainda',
    emptyText: (code) => (
      <>
        Os agentes escrevem a documentação junto com as mudanças: o que é o app, para quem ele é e por quê em{' '}
        {code('docs/product.md')}, e por que ele é construído desse jeito em {code('decisions.md')}. Ela faz
        parte da versão, ao lado do código.
      </>
    ),
    emptySimpleTitle: 'Ainda não há nada escrito sobre o seu app',
    emptySimple: 'O agente pode descrever para que serve o seu app e por quê, a partir do que você pediu, e mantém a descrição atualizada daí em diante.',
    ask: 'Pedir ao agente para escrever',
    describe: 'Pedir ao agente para descrever',
    writePrompt: 'Escreva a documentação deste app: o que ele é, para quem é e por quê, e as decisões técnicas por trás dele.',
    describePrompt: 'Descreva para que serve este app e por quê, para eu ler em Sobre.',
    decisionsPrompt: 'Registre as decisões técnicas por trás deste app e por que elas foram tomadas.',
    missingProduct: 'Ainda não há página de produto',
    missingProductText: 'O que é o app, para quem ele é, o que as pessoas fazem com ele e por quê, nas palavras de quem o pediu.',
    missingDecisions: 'Nenhuma decisão registrada ainda',
    missingDecisionsText: 'Como o app é construído e por quê: como ele guarda os dados, do que depende, o que ficou de fora. O que precisa saber quem for mudá-lo depois.',
    correctText: 'O agente escreve isto a partir do que você pediu e mantém tudo atualizado a cada mudança. Algo errado ou faltando? Avise o agente.',
    correct: 'Avisar o agente',
    correctPrompt: 'Corrija a descrição do app: ',
    placeholder: 'Explica por que as entradas são guardadas por um ano',
  },
  tests: {
    title: 'Testes',
    hint: 'Como este app é testado automaticamente, e os scripts e dados que os testes usam. Os agentes mantêm isto atualizado e rodam os testes antes de considerar uma mudança concluída. Fica guardado com cada versão.',
    inDraft: 'Os testes deste rascunho. Eles passam a ser os do seu app quando o rascunho entrar no ar; rode-os antes na prévia dele.',
    pages: { testing: 'Como é testado' },
    emptyTitle: 'Nenhum teste ainda',
    emptyText: (code) => (
      <>
        Como o app é testado (o que precisa continuar funcionando, como verificar isso e como rodar os scripts para
        isso) é escrito pelos agentes em {code('tests/README.md')}, com os scripts e os dados de teste ao lado.
      </>
    ),
    ask: 'Pedir ao agente para escrever testes',
    writePrompt: 'Escreva os testes deste app: o que precisa continuar funcionando e como verificar isso automaticamente, com um script para rodar na prévia dele.',
    missing: 'Ainda não diz como é testado',
    missingText: 'O que precisa continuar funcionando, como cada parte é verificada e como rodar os scripts ao lado.',
    placeholder: 'Verifica se uma lista cheia recusa novas entradas',
  },
  files: 'Arquivos',
  noFiles: 'Nenhum arquivo além das páginas.',
  none: 'faltando',
  missingPill: 'Ainda não foi escrita',
  changedIn: (version) => `Alterada na versão ${version}`,
  changedInDraft: 'Alterada neste rascunho',
  showChanges: 'Ver o que mudou',
  hideChanges: 'Ocultar o que mudou',
  noChanges: 'Nada mudou.',
  edit: 'Editar',
  olderVersion: 'Uma versão nunca muda: uma página é editada na versão mais recente ou num rascunho.',
  writeIt: 'Escrever você mesmo',
  askPage: 'Pedir ao agente para escrever',
  editInCode: 'Abrir no código',
  cancel: 'Cancelar',
  save: 'Salvar',
  write: 'Escrever',
  preview: 'Visualizar',
  writeOrPreview: 'Escrever ou visualizar',
  discard: 'Suas alterações nesta página vão se perder. Descartar mesmo assim?',
  reading: 'Lendo…',
  readFailed: 'Não foi possível ler isto.',
  saveFailed: 'Não foi possível salvar.',
  savedDraft: 'Salvo no rascunho.',
  savedVersion: (version) => `Salvo como versão ${version}.`,
  savedOnline: (version) => `Salvo como versão ${version}, e no ar.`,
  savedNotOnline: (version) => `Salvo como versão ${version}, mas não foi para o ar.`,
  saveTitle: 'Salvar como nova versão',
  saveText: (newest) =>
    `Uma versão nunca muda, então esta página é salva como a próxima: a partir da versão ${newest}, com todo o resto como está.`,
  clash: (version) => `A versão ${version} foi salva depois que você começou, e também mudou esta página. Salvar substitui essa mudança.`,
  alsoOnline: 'Colocar no ar também',
  alsoOnlineNote: 'Só a documentação muda, então os visitantes não veem nada de novo, mas o que está no ar continua sendo a versão mais recente.',
  skeleton: {
    product: '# Nome do app\n\nO que ele é, em uma ou duas frases.\n\n## Para quem é\n\n## O que as pessoas fazem com ele\n\n## Funcionalidades, e por que existem\n\n## O que ele não faz\n',
    decisions: '# Decisões\n\n## Uma decisão\n\nO que foi decidido, por quê, e o que uma mudança precisa levar em conta.\n',
    testing: '# Como é testado\n\nComo rodar os testes, e em qual endereço.\n\n## O que precisa continuar funcionando\n\n| Comportamento | Requisição | Esperado |\n|---|---|---|\n| | | |\n',
  },
};
