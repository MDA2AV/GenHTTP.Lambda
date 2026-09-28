import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Montra',
  title: 'Feito aqui, a funcionar agora',
  intro:
    'Lambdas que os donos escolheram mostrar. Estão todas online, por isso cada cartão abre a app verdadeira. As usadas mais recentemente aparecem primeiro.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'Não foi possível carregar a montra.',
  loadingMore: 'A carregar mais…',
  showMore: 'Mostrar mais',
  nothingTitle: 'Ainda não há nada na montra',
  nothing: (tab) => (
    <>
      Criaste algo que funciona? Abre o painel de controlo, escolhe {tab('Montra')} e junta um título, umas palavras e
      uma imagem. Aparece aqui enquanto estiver online.
    </>
  ),
  buildOne: 'Criar uma app',
  yoursTitle: 'Queres a tua aqui?',
  yours: (tab) => (
    <>
      Abre o painel de controlo da tua lambda e escolhe {tab('Montra')}, ou pede ao agente que a criou para a pôr na
      montra. Só quem tem a chave de edição pode fazê-lo, e pode retirá-la a qualquer momento.
    </>
  ),
  buildSomething: 'Criar algo',
};

export const card: Messages['card'] = {
  noPicture: 'Ainda sem imagem',
  title: 'Título',
  description: 'O que um visitante pode fazer com ela.',
  opens: (title, address) => `${title}, abre ${address} num novo separador`,
};
