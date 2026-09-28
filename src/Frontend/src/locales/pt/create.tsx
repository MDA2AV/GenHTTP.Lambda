import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Criar uma lambda',
  whatTitle: 'O que você quer criar?',
  whatText:
    'Escolha o mais parecido e comece com uma cópia de algo que já funciona, pronta para você mudar. Ou comece do zero.',
  seeIt: 'Ver funcionando',
  startFrom: 'Começar com este',
  starters: {
    'demo-crud': {
      title: 'Organizar coisas',
      description: 'Uma lista em que as pessoas adicionam, editam e marcam itens como feitos: tarefas, notas, favoritos ou um pequeno estoque.',
    },
    'demo-registration': {
      title: 'Cadastro e login',
      description: 'Contas para as pessoas se cadastrarem e entrarem, e páginas que só elas veem.',
    },
    'demo-game': {
      title: 'Um jogo para jogar junto',
      description: 'Algo que várias pessoas jogam ao mesmo tempo, ao vivo no navegador.',
    },
    'demo-files': {
      title: 'Compartilhar arquivos e fotos',
      description: 'As pessoas enviam fotos ou documentos, e todo mundo pode ver.',
    },
    'demo-live': {
      title: 'Mostrar tudo ao vivo',
      description: 'Uma página que se atualiza sozinha assim que algo muda: votos, pontuações, um dashboard.',
    },
    empty: {
      title: 'Outra coisa',
      description: 'Comece com uma lambda vazia e crie o que você tiver em mente.',
    },
  },

  addressTitle: 'Escolha um endereço',
  fromDemo: (title) => <>{title} – sua lambda começa como uma cópia da demo, e você pode mudar tudo nela.</>,
  fromNothing: 'Sua lambda começa vazia, pronta para o que você imaginar.',
  pickAgain: 'Escolher outro',
  publicKey: 'Chave pública',
  free: (key) => `“${key}” está disponível.`,
  keyHint: 'Letras minúsculas, números e hífens. No mínimo três caracteres. Deixe vazio para gerar uma aleatória.',
  accept: 'Aceito os termos de uso',
  fullTerms: 'Ler os termos de uso completos',
  back: 'Voltar',
  creating: 'Criando…',
  submit: 'Criar minha lambda',
  keepLink: 'A próxima tela mostra seu link de edição. É o único jeito de voltar, então guarde bem.',
  failed: 'Não foi possível criar a lambda.',
};
