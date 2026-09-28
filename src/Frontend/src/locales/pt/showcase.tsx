import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Vitrine',
  title: 'Feito aqui, rodando agora',
  intro:
    'Lambdas que os donos escolheram mostrar. Todas estão no ar, então cada card abre o app de verdade. As mais usadas ultimamente vêm primeiro.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'Não foi possível carregar a vitrine.',
  loadingMore: 'Carregando mais…',
  showMore: 'Mostrar mais',
  nothingTitle: 'Nada na vitrine ainda',
  nothing: (tab) => (
    <>
      Criou algo que funciona? Abra o painel de controle, escolha {tab('Vitrine')} e adicione um título, uma descrição
      curta e uma imagem. O app aparece aqui enquanto estiver no ar.
    </>
  ),
  buildOne: 'Criar um app',
  yoursTitle: 'Quer o seu aqui?',
  yours: (tab) => (
    <>
      Abra o painel de controle da sua lambda e escolha {tab('Vitrine')}, ou peça ao agente que criou o app para
      colocar na vitrine. Só quem tem a chave de edição pode fazer isso, e dá para tirar a qualquer momento.
    </>
  ),
  buildSomething: 'Criar algo',
};

export const card: Messages['card'] = {
  noPicture: 'Sem imagem ainda',
  title: 'Título',
  description: 'O que um visitante pode fazer com o app.',
  opens: (title, address) => `${title}, abre ${address} em uma nova aba`,
};
