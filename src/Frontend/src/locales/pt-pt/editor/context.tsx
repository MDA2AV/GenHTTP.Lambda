import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Documentação',
    titleSimple: 'Sobre a tua aplicação',
    hint: 'O que é esta aplicação, para quem é e porquê, e porque está construída desta forma. Os agentes escrevem-na a cada alteração e fica guardada com cada versão, por isso uma versão anterior volta com a documentação que lhe correspondia.',
    hintSimple: 'Para que serve a tua aplicação e porquê, tal como o agente o percebeu a partir do que pediste. Ele mantém isto atualizado a cada alteração.',
    inDraft: 'A documentação deste rascunho. Passa a ser a da tua aplicação quando o rascunho ficar online.',
    pages: { product: 'Produto', decisions: 'Decisões' },
    emptyTitle: 'Ainda não há nada escrito',
    emptyText: (code) => (
      <>
        Os agentes escrevem a documentação com as suas alterações: o que é a aplicação, para quem é e porquê em{' '}
        {code('docs/product.md')}, e porque está construída desta forma em {code('decisions.md')}. Faz parte
        da versão, ao lado do código.
      </>
    ),
    emptySimpleTitle: 'Ainda não há nada escrito sobre a tua aplicação',
    emptySimple: 'O agente pode descrever para que serve a tua aplicação e porquê, a partir do que pediste, e mantém a descrição atualizada a partir daí.',
    ask: 'Pedir ao agente que a escreva',
    describe: 'Pedir ao agente que a descreva',
    writePrompt: 'Escreve a documentação desta aplicação: o que é, para quem é e porquê, e as decisões técnicas por trás dela.',
    describePrompt: 'Descreve para que serve esta aplicação e porquê, para eu ler em Sobre.',
    decisionsPrompt: 'Regista as decisões técnicas por trás desta aplicação, e porque foram tomadas.',
    missingProduct: 'Ainda não há página de produto',
    missingProductText: 'O que é a aplicação, para quem é, o que as pessoas fazem com ela e porquê, nas palavras de quem a pediu.',
    missingDecisions: 'Ainda não há decisões registadas',
    missingDecisionsText: 'Como a aplicação está construída e porquê: como guarda os dados, de que depende, o que ficou de fora. O que precisa de saber quem a alterar a seguir.',
    correctText: 'O agente escreve isto a partir do que pediste, e mantém-no atualizado a cada alteração. Há algo errado ou em falta? Diz ao agente.',
    correct: 'Dizer ao agente',
    correctPrompt: 'Corrige a descrição da aplicação: ',
    placeholder: 'Explica porque é que as entradas são guardadas durante um ano',
  },
  tests: {
    title: 'Testes',
    hint: 'Como esta aplicação é testada automaticamente, e os scripts e dados que os testes usam. Os agentes mantêm isto atualizado e correm os testes antes de darem uma alteração por concluída. Fica guardado com cada versão.',
    inDraft: 'Os testes deste rascunho. Passam a ser os da tua aplicação quando o rascunho ficar online; corre-os primeiro na pré-visualização dele.',
    pages: { testing: 'Como é testada' },
    emptyTitle: 'Ainda não há testes',
    emptyText: (code) => (
      <>
        Como a aplicação é testada (o que tem de continuar a funcionar, como o verificar e como correr os scripts para
        isso) é escrito pelos agentes em {code('tests/README.md')}, com os scripts e os dados de teste ao lado.
      </>
    ),
    ask: 'Pedir ao agente que escreva testes',
    writePrompt: 'Escreve os testes desta aplicação: o que tem de continuar a funcionar e como o verificar automaticamente, com um script para correr na pré-visualização dela.',
    missing: 'Ainda não diz como é testada',
    missingText: 'O que tem de continuar a funcionar, como cada parte é verificada e como correr os scripts ao lado.',
    placeholder: 'Verifica se uma lista cheia recusa novas entradas',
  },
  files: 'Ficheiros',
  noFiles: 'Não há ficheiros além das páginas.',
  none: 'em falta',
  missingPill: 'Ainda por escrever',
  changedIn: (version) => `Alterada na versão ${version}`,
  changedInDraft: 'Alterada neste rascunho',
  showChanges: 'Mostrar o que mudou',
  hideChanges: 'Ocultar o que mudou',
  noChanges: 'Nada mudou.',
  edit: 'Editar',
  olderVersion: 'Uma versão nunca muda: uma página edita-se na versão mais recente, ou num rascunho.',
  writeIt: 'Escrever tu mesmo',
  askPage: 'Pedir ao agente que a escreva',
  editInCode: 'Abrir no código',
  cancel: 'Cancelar',
  save: 'Guardar',
  write: 'Escrever',
  preview: 'Pré-visualizar',
  writeOrPreview: 'Escrever ou pré-visualizar',
  discard: 'As tuas alterações a esta página vão perder-se. Descartá-las?',
  reading: 'A ler…',
  readFailed: 'Não foi possível ler isto.',
  saveFailed: 'Não foi possível guardar.',
  savedDraft: 'Guardado no rascunho.',
  savedVersion: (version) => `Guardado como versão ${version}.`,
  savedOnline: (version) => `Guardado como versão ${version}, e online.`,
  savedNotOnline: (version) => `Guardado como versão ${version}, mas não ficou online.`,
  saveTitle: 'Guardar como nova versão',
  saveText: (newest) =>
    `Uma versão nunca muda, por isso esta página é guardada como a seguinte: sobre a versão ${newest}, com tudo o resto como está.`,
  clash: (version) => `A versão ${version} foi guardada depois de começares, e também alterou esta página. Guardar substitui essa alteração.`,
  alsoOnline: 'Pôr online também',
  alsoOnlineNote: 'Só a documentação muda, por isso os visitantes não veem nada de novo, mas o que está online continua a ser a versão mais recente.',
  skeleton: {
    product: '# Nome da aplicação\n\nO que é, numa ou duas frases.\n\n## Para quem é\n\n## O que as pessoas fazem com ela\n\n## Funcionalidades, e porque existem\n\n## O que não faz\n',
    decisions: '# Decisões\n\n## Uma decisão\n\nO que foi decidido, porquê, e o que uma alteração tem de ter em conta.\n',
    testing: '# Como é testada\n\nComo correr os testes, e em que endereço.\n\n## O que tem de continuar a funcionar\n\n| Comportamento | Pedido | Esperado |\n|---|---|---|\n| | | |\n',
  },
};
