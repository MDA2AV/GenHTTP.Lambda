import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Crie um site com IA.',
  intro:
    'Descreva com suas palavras o site ou app que você imagina. A IA cria para você, nós hospedamos e ele fica no ar em minutos, com um link para mandar para quem quiser. Grátis, sem programar, sem cadastro.',
  points: [
    {
      title: 'Sem saber programar',
      text: 'Diga com suas palavras o que o seu site deve fazer, como você explicaria para um amigo. A IA cria para você, sem precisar de conhecimento técnico.',
    },
    {
      title: 'Hospedagem grátis incluída',
      text: 'Seu site roda em nossos servidores. Sem plano de hospedagem, sem servidor, sem domínio para comprar e sem instalar nada. Segurança e atualizações ficam por nossa conta.',
    },
    {
      title: 'No ar em minutos',
      text: 'Você recebe na hora um link para compartilhar. O site guarda o que as pessoas enviam (inscrições, votos, recados, pontuações), para todos verem a mesma coisa.',
    },
  ],

  questionsTitle: 'Antes de começar',
  questions: (offline, removed) => [
    [
      'A IA cria mesmo um site de graça para mim?',
      `Sim. Descreva com suas palavras e a IA cria o site, coloca no ar e passa o link para você. Sem cadastro, sem cartão de crédito, sem período de teste. Ele fica no ar enquanto as pessoas usarem: depois de ${offline} dias sem visitas nem mudanças, sai do ar, e depois de ${removed} dias é removido.`,
    ],
    [
      'Preciso de hospedagem, servidor ou domínio?',
      'Não. Seu site roda em nossos servidores, com hospedagem, segurança e atualizações incluídas. Você recebe um link na hora, então também não precisa comprar domínio.',
    ],
    [
      'Dá para criar um app sem saber programar?',
      'Sim. Você nunca vê código. Diga o que ele deve fazer, como explicaria para um amigo, e a IA faz o resto: um site, um pequeno app ou um jogo.',
    ],
    [
      'As pessoas podem enviar inscrições, votos e recados?',
      'Sim. Seu site guarda o que as pessoas enviam, então todo mundo que abre o link vê as mesmas inscrições, votos e pontuações.',
    ],
    [
      'Como as outras pessoas abrem o site?',
      'Pelo link, em qualquer navegador, no celular ou no computador. Não precisa instalar nada, e não tem loja de apps no meio.',
    ],
    [
      'Como eu mudo o site depois?',
      'Abra o link de edição que você recebe com o seu site e descreva o que deve ser diferente, do mesmo jeito que aqui. Se não gostar de uma mudança, dá para voltar ao que era antes.',
    ],
  ],

};
