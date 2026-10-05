import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Clonar',
  title: 'Clonar com git',
  intro:
    'Trabalha nela com as tuas próprias ferramentas e o teu agente de programação: o repositório é o projeto com que corre, cada versão é um commit de main e cada rascunho é um ramo.',
  keyWarning: 'O endereço contém a chave de edição: quem a tiver pode alterar a app. Não a partilhes com ninguém.',
  draft: (branch) => <>Este rascunho é o ramo {branch}.</>,
  pushing: 'Fazer push',
  toMain: (deploy) => <>Um commit enviado para main é a versão seguinte, ainda não online - {deploy} põe-na online com o push.</>,
  toBranch: 'Um ramo enviado é um rascunho, com a pré-visualização online num endereço próprio.',
  agents: (file) => <>{file} no repositório explica o resto a um agente de programação.</>,
  readOnly: 'Uma demo é só de leitura: clona-a para a ler e começa uma lambda tua a partir dela para a alterar.',
  copy: 'Copiar',
  copied: 'Copiado',
};
