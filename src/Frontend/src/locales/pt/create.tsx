import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Criar um lambda',
  whatTitle: 'O que você deseja criar?',
  whatText:
    'Escolha a opção mais próxima e comece com uma cópia de algo que já funciona, totalmente editável. Ou comece do zero.',
  seeIt: 'Ver em execução',
  startFrom: 'Usar este modelo',
  starters: {
    'demo-crud': {
      title: 'Organizar informações',
      description: 'Uma lista que pode ser ampliada, alterada e marcada – tarefas, anotações, favoritos ou um pequeno inventário.',
    },
    'demo-registration': {
      title: 'Cadastro e login',
      description: 'Contas com cadastro e login, e páginas visíveis apenas para usuários autenticados.',
    },
    'demo-game': {
      title: 'Um jogo multijogador',
      description: 'Algo que várias pessoas usam ao mesmo tempo, em tempo real no navegador.',
    },
    'demo-files': {
      title: 'Compartilhar arquivos e imagens',
      description: 'As pessoas enviam imagens ou documentos, e os demais podem visualizá-los.',
    },
    'demo-live': {
      title: 'Atualizações em tempo real',
      description: 'Uma página que se atualiza sozinha assim que algo muda – votos, pontuações, um painel.',
    },
    empty: {
      title: 'Lambda vazio',
      description: 'Comece com um lambda vazio e desenvolva sua própria ideia.',
    },
  },

  addressTitle: 'Defina um endereço',
  fromDemo: (title) => <>{title} – seu lambda começa como uma cópia da demo, totalmente editável.</>,
  fromNothing: 'Seu lambda começa vazio, pronto para o que você planejar.',
  pickAgain: 'Escolher outra opção',
  publicKey: 'Chave pública',
  free: (key) => `“${key}” está disponível.`,
  keyHint: 'Letras minúsculas, dígitos e hífens, com no mínimo três caracteres. Deixe em branco para uma chave aleatória.',
  accept: 'Aceito os termos de serviço',
  fullTerms: 'Ler os termos de serviço completos',
  back: 'Voltar',
  creating: 'Criando…',
  submit: 'Criar lambda',
  keepLink: 'A próxima tela exibe seu link de edição. Ele é o único acesso, portanto guarde-o.',
  failed: 'Não foi possível criar o lambda.',
};
