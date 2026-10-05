import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Compilação',
  hint: 'Aquilo a partir do qual os assets ou o código de uma versão são compilados: ficheiros sobre os quais quem altera a app - o teu agente, num clone - executa uma ferramenta de compilação, guardados com cada versão e nunca compilados nem servidos. Esta plataforma não compila nada, por isso aqui só se leem, não se editam.',
  overview: 'Resumo',
  files: 'Ficheiros',
  scope: (version) =>
    `Aquilo a partir do qual a versão ${version} é compilada - guardado com ela, nunca compilado nem servido, e compilado por quem a altera, nunca aqui.`,
  scopeDraft: 'Aquilo a partir do qual este rascunho é compilado - guardado com ele, nunca compilado nem servido, e compilado por quem o altera, nunca aqui.',
  reading: 'A ler aquilo a partir do qual é compilado…',
  readFailed: 'Não foi possível ler aquilo a partir do qual é compilado.',

  emptyTitle: (version) => `A versão ${version} não guarda nada a partir do qual seja compilada`,
  emptyTitleDraft: 'Este rascunho não guarda nada a partir do qual seja compilado',
  emptyText: (code) => (
    <>
      Quando os assets ou o código de uma versão são feitos por uma ferramenta de compilação - compilados, empacotados
      ou gerados - os ficheiros a partir dos quais são feitos ficam guardados aqui, com cada versão: a pasta{' '}
      {code('dev/')} num clone. Quem altera a app executa a compilação onde trabalha e guarda ambos em conjunto; esta
      plataforma não compila nada. O que é escrito tal como é servido ou compilado não precisa de nada disto.
    </>
  ),
  emptyHow: (code) => (
    <>O {code('AGENTS.md')} num clone diz a um agente de programação como se usa.</>
  ),

  inVersion: (version) => `Na versão ${version}`,
  inDraft: 'Neste rascunho',
  comparedWith: (version) => `em relação à versão ${version}`,
  first: 'A primeira versão que o guarda.',
  both: (here, program) =>
    `${here === 1 ? '1 ficheiro alterado' : `${here} ficheiros alterados`} aqui, e ${program === 1 ? '1 ficheiro' : `${program} ficheiros`} do código e dos assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 ficheiro alterado' : `${here} ficheiros alterados`} aqui, e nada do código nem dos assets: se o que mudou é compilado para dentro deles, não foi compilado.`,
  programOnly: 'Nada mudou aqui.',
  unchanged: 'Nada mudou aqui, nem no código ou nos assets.',
  showChanges: 'Mostrar as alterações',
  hideChanges: 'Ocultar as alterações',
  noChanges: 'Nada mudou aqui.',

  readme: 'Como é compilado',
  noReadme: (code) => (
    <>
      Nada diz como é compilado. Um {code('README.md')} no topo - os comandos e para onde vai o resultado - é aquilo a
      partir do qual o próximo agente compila.
    </>
  ),
  readOnly: 'Só de leitura: altera-se onde é compilado.',
  noFiles: 'Sem ficheiros.',
};
