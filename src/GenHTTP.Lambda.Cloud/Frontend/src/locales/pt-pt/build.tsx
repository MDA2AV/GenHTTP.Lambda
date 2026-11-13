import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Cria um site com IA.',
  intro:
    'Descreve por palavras tuas o site ou a app que tens em mente. A IA cria-o por ti, nós alojamo-lo e fica online em minutos, com um link para enviares a quem quiseres. Grátis, sem programar, sem registo.',
  points: [
    {
      title: 'Sem saber programar',
      text: 'Diz por palavras tuas o que o teu site deve fazer, como explicarias a um amigo. A IA cria-o por ti, sem precisares de conhecimentos técnicos.',
    },
    {
      title: 'Alojamento grátis incluído',
      text: 'O teu site corre nos nossos servidores. Sem plano de alojamento, sem servidor nem domínio para comprar, nada para instalar. Segurança e atualizações ficam a nosso cargo.',
    },
    {
      title: 'Online em minutos',
      text: 'Recebes logo um link para partilhar. O site guarda o que as pessoas escrevem (inscrições, votos, mensagens, pontuações), para que todos vejam o mesmo.',
    },
  ],

  questionsTitle: 'Antes de começares',
  questions: (offline, removed) => [
    [
      'A IA cria mesmo um site grátis para mim?',
      `Sim. Descreve-o por palavras tuas e a IA cria-o, põe-no online e dá-te o link. Sem registo, sem cartão de crédito, sem período experimental. Fica online enquanto houver quem o use: após ${offline} dias sem visitas nem alterações fica offline, e após ${removed} dias é removido.`,
    ],
    [
      'Preciso de alojamento, de um servidor ou de um domínio?',
      'Não. O teu site corre nos nossos servidores, com alojamento, segurança e atualizações incluídos. Recebes logo um link, por isso também não tens de comprar um domínio.',
    ],
    [
      'Posso criar uma app sem saber programar?',
      'Sim. Nunca vês código nenhum. Diz o que deve fazer, como explicarias a um amigo, e a IA trata do resto: um site, uma pequena app ou um jogo.',
    ],
    [
      'As pessoas podem deixar inscrições, votos e mensagens?',
      'Sim. O teu site guarda o que as pessoas escrevem, por isso quem abre o link vê as mesmas inscrições, votos e pontuações.',
    ],
    [
      'Como é que as outras pessoas o abrem?',
      'Com o link, em qualquer browser, no telemóvel ou no computador. Não há nada para instalar nem loja de apps pelo meio.',
    ],
    [
      'Como é que o altero mais tarde?',
      'Abre o link de edição que recebes com o teu site e descreve o que deve ser diferente, tal como aqui. Se não gostares de uma alteração, podes voltar ao que estava antes.',
    ],
  ],

};
