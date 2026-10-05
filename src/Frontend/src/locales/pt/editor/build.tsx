import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Build',
  hint: 'Aquilo a partir do qual os assets ou o código de uma versão são gerados: arquivos nos quais quem altera o app (o seu agente, num clone) executa uma ferramenta de build, guardados com cada versão e nunca compilados nem servidos. Esta plataforma não constrói nada, então eles são lidos aqui, não editados.',
  overview: 'Visão geral',
  files: 'Arquivos',
  scope: (version) =>
    `Aquilo a partir do qual a versão ${version} é construída: guardado com ela, nunca compilado nem servido, e construído por quem a altera, nunca aqui.`,
  scopeDraft: 'Aquilo a partir do qual este rascunho é construído: guardado com ele, nunca compilado nem servido, e construído por quem o altera, nunca aqui.',
  reading: 'Lendo aquilo a partir do qual é construído…',
  readFailed: 'Não foi possível ler aquilo a partir do qual é construído.',

  emptyTitle: (version) => `A versão ${version} não guarda nada a partir do qual seja construída`,
  emptyTitleDraft: 'Este rascunho não guarda nada a partir do qual seja construído',
  emptyText: (code) => (
    <>
      Quando os assets ou o código de uma versão são feitos por uma ferramenta de build (compilados, empacotados ou
      gerados), os arquivos a partir dos quais são feitos ficam guardados aqui, com cada versão: a pasta {code('dev/')}{' '}
      num clone. Quem altera o app executa o build onde trabalha e salva os dois juntos; esta plataforma não constrói
      nada. O que é escrito do jeito que é servido ou compilado não precisa de nada disso.
    </>
  ),
  emptyHow: (code) => (
    <>O {code('AGENTS.md')} num clone explica a um agente de programação como ele é usado.</>
  ),

  inVersion: (version) => `Na versão ${version}`,
  inDraft: 'Neste rascunho',
  comparedWith: (version) => `em relação à versão ${version}`,
  first: 'A primeira versão que o guarda.',
  both: (here, program) =>
    `${here === 1 ? '1 arquivo alterado' : `${here} arquivos alterados`} aqui, e ${program === 1 ? '1 arquivo' : `${program} arquivos`} do código e dos assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 arquivo alterado' : `${here} arquivos alterados`} aqui, e nada do código nem dos assets: se o que mudou é incorporado a eles, o build não foi feito.`,
  programOnly: 'Nada mudou aqui.',
  unchanged: 'Nada mudou aqui, nem no código nem nos assets.',
  showChanges: 'Mostrar as mudanças',
  hideChanges: 'Ocultar as mudanças',
  noChanges: 'Nada mudou aqui.',

  readme: 'Como é construído',
  noReadme: (code) => (
    <>
      Nada explica como é construído. Um {code('README.md')} no topo, com os comandos e o destino do build, é o que o
      próximo agente usa para construir.
    </>
  ),
  readOnly: 'Somente leitura: é alterado onde é construído.',
  noFiles: 'Nenhum arquivo.',
};
