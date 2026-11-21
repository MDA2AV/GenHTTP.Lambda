import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Apps para o teu trabalho',
  headline: 'Descreve a app de que o teu trabalho precisa.',
  headlineAccent: 'Um agente de IA cria-a e põe-na online aqui.',
  intro:
    'Um formulário, um calendário de reservas, uma lista para a tua equipa: diz por palavras tuas o que deve fazer. Em poucos minutos tens um link para partilhar com os teus colegas - e mais tarde alteras a app da mesma forma, sem escrever código.',
  describe: 'Descrever uma app',
  own: 'Usar o teu próprio agente de IA',

  examplesTitle: 'O que as pessoas criam aqui',
  examples: [
    { title: 'Formulários e pedidos', text: 'Férias, compras, comentários: as pessoas preenchem e quem decide aprova.' },
    { title: 'Reservas', text: 'Salas de reunião, secretárias, equipamento: quem tem o quê, e quando.' },
    { title: 'Listas e acompanhamento', text: 'Pedidos de apoio, inventário, integração de novos colaboradores: o que está em aberto e quem trata disso.' },
    { title: 'Páginas para uma equipa', text: 'Um horário, um manual, novidades: um só link que todos abrem.' },
  ],

  stepsTitle: 'Como funciona',
  steps: [
    {
      title: 'Descreve-a',
      text: 'Diz o que a app deve fazer, como o explicarias a um colega. As ideias por baixo da caixa são um ponto de partida.',
    },
    {
      title: 'Partilha o link',
      text: 'Poucos minutos depois está online aqui. Envia o link às pessoas que a vão usar e guarda o teu link de edição: é com ele que alteras a app.',
    },
    {
      title: 'Pede mais',
      text: 'Na mesma caixa: algo novo, algo diferente, ou como está a correr - quem a usou e o que foi introduzido. As versões anteriores ficam guardadas, por isso a anterior pode voltar a estar online.',
    },
  ],

  questionsTitle: 'Bom saber',
  questions: (showcase) => [
    ['Preciso de saber programar?', 'Não. Descreves o que a app deve fazer e o agente escreve-a. Nunca tens de ver o código.'],
    ['Quem pode abrir a minha app?', 'Quem tiver o link e conseguir aceder a este site. Envia-o às pessoas que a devem usar.'],
    ['Onde fica guardado o que as pessoas introduzem?', 'Cada app guarda os seus próprios registos nesta instalação, que a tua organização gere.'],
    [
      'Como volto à minha app?',
      'Este navegador lembra-se das apps que criaste aqui, junto à caixa onde as criaste. Noutro dispositivo, abre o link de edição que recebeste com a app - guarda-o num local seguro, porque ninguém o consegue recuperar.',
    ],
    [
      'Algo não funciona. E agora?',
      'Diz ao agente o que está mal, da mesma forma que pediste a app. Se uma alteração piorou as coisas, volta a pôr online a versão anterior no editor da app.',
    ],
    ['O que construíram os colegas?', <>As apps que escolheram mostrar estão na {showcase('Montra')}.</>],
  ],
};
