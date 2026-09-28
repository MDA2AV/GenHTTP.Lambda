import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Criar uma lambda',
  whatTitle: 'O que queres criar?',
  whatText:
    'Escolhe o mais parecido e começas com uma cópia de algo que já funciona, pronta para alterares. Ou começa do zero.',
  seeIt: 'Ver a funcionar',
  startFrom: 'Começar com este',
  starters: {
    'demo-crud': {
      title: 'Organizar coisas',
      description: 'Uma lista onde as pessoas podem adicionar, alterar e riscar coisas: tarefas, notas, marcadores ou um pequeno inventário.',
    },
    'demo-registration': {
      title: 'Registo de utilizadores',
      description: 'Contas com que as pessoas se registam e iniciam sessão, e páginas que só elas podem ver.',
    },
    'demo-game': {
      title: 'Um jogo em grupo',
      description: 'Algo que várias pessoas jogam ao mesmo tempo, em tempo real, no browser.',
    },
    'demo-files': {
      title: 'Partilhar ficheiros e fotos',
      description: 'As pessoas carregam fotos ou documentos, e toda a gente pode vê-los.',
    },
    'demo-live': {
      title: 'Mostrar tudo em tempo real',
      description: 'Uma página que se atualiza sozinha assim que algo muda: votos, pontuações, um dashboard.',
    },
    empty: {
      title: 'Outra coisa',
      description: 'Começa com uma lambda vazia e cria o que tiveres em mente.',
    },
  },

  addressTitle: 'Dá-lhe um endereço',
  fromDemo: (title) => <>{title} – a tua lambda começa como uma cópia da demo, e podes alterar tudo o que lá está.</>,
  fromNothing: 'A tua lambda começa vazia, pronta para o que tiveres em mente.',
  pickAgain: 'Escolher outra coisa',
  publicKey: 'Chave pública',
  free: (key) => `«${key}» está disponível.`,
  keyHint: 'Letras minúsculas, algarismos e hífenes. Pelo menos três caracteres. Deixa em branco para gerar uma ao acaso.',
  accept: 'Aceito os termos de utilização',
  fullTerms: 'Ler os termos de utilização completos',
  back: 'Voltar',
  creating: 'A criar…',
  submit: 'Criar a minha lambda',
  keepLink: 'O próximo ecrã mostra o teu link de edição. É a única forma de voltares a entrar, por isso guarda-o.',
  failed: 'Não foi possível criar a lambda.',
};
