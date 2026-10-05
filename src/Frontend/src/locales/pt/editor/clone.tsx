import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Clonar',
  title: 'Clonar com git',
  intro:
    'Trabalhe nele com suas próprias ferramentas e seu agente de código: o repositório é o projeto como ele roda, cada versão é um commit da main e cada rascunho é um branch.',
  keyWarning: 'O endereço contém a chave de edição: quem a tiver pode alterar o app. Não a inclua no que você compartilha.',
  draft: (branch) => <>Este rascunho é o branch {branch}.</>,
  pushing: 'Fazendo push',
  toMain: (deploy) => <>Um commit enviado por push para a main é a próxima versão, ainda fora do ar - {deploy} a coloca no ar junto com o push.</>,
  toBranch: 'Um branch enviado por push é um rascunho, com a prévia no ar em um endereço próprio.',
  agents: (file) => <>{file} no repositório explica o resto para um agente de código.</>,
  readOnly: 'Uma demo é somente leitura: clone-a para ler e crie uma lambda sua a partir dela para fazer mudanças.',
  copy: 'Copiar',
  copied: 'Copiado',
};
