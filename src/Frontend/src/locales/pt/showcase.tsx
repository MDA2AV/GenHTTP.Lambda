import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Vitrine',
  title: 'Criadas aqui, em execução agora',
  intro:
    'Lambdas que seus proprietários decidiram exibir. Todos estão no ar, então cada cartão abre a aplicação real. Os mais utilizados recentemente aparecem primeiro.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'Não foi possível carregar a vitrine.',
  loadingMore: 'Carregando mais…',
  showMore: 'Mostrar mais',
  nothingTitle: 'Ainda não há nada em exibição',
  nothing: (tab) => (
    <>
      Criou algo que funciona? Abra o centro de controle, escolha {tab('Vitrine')} e adicione um título, uma breve
      descrição e uma imagem. A aplicação aparecerá aqui enquanto estiver no ar.
    </>
  ),
  buildOne: 'Criar uma aplicação',
  yoursTitle: 'Quer exibir a sua?',
  yours: (tab) => (
    <>
      Abra o centro de controle do seu lambda e escolha {tab('Vitrine')}, ou peça ao agente que o criou que o adicione.
      Somente quem tem a chave de edição pode fazer isso, e a exibição pode ser removida a qualquer momento.
    </>
  ),
  buildSomething: 'Criar uma aplicação',
};

export const card: Messages['card'] = {
  noPicture: 'Ainda sem imagem',
  title: 'Título',
  description: 'O que um visitante pode fazer com ela.',
  opens: (title, address) => `${title}, abre ${address} em uma nova aba`,
};
