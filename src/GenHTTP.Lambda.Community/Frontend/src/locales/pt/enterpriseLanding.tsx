import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Apps para o seu trabalho',
  headline: 'Descreva o app de que o seu trabalho precisa.',
  headlineAccent: 'Um agente de IA o cria e o coloca no ar aqui.',
  intro:
    'Um formulário, uma agenda de reservas, uma lista para a sua equipe: diga com as suas próprias palavras o que ele deve fazer. Em poucos minutos você tem um link para compartilhar com os colegas, e depois muda o app do mesmo jeito, sem escrever código.',
  describe: 'Descrever um app',
  own: 'Usar o meu próprio agente de IA',

  examplesTitle: 'O que as pessoas criam aqui',
  examples: [
    { title: 'Formulários e solicitações', text: 'Folgas, compras, feedback: as pessoas preenchem, e quem decide aprova.' },
    { title: 'Reservas', text: 'Salas de reunião, mesas, equipamentos: quem está com o quê, e quando.' },
    { title: 'Listas e acompanhamentos', text: 'Chamados de suporte, estoque, integração de novos colegas: o que está em aberto e quem cuida disso.' },
    { title: 'Páginas para uma equipe', text: 'Uma escala, um manual, novidades: um único link que todos abrem.' },
  ],

  stepsTitle: 'Como funciona',
  steps: [
    {
      title: 'Descreva',
      text: 'Diga o que o app deve fazer, como você explicaria a um colega. As ideias abaixo do campo são um ponto de partida.',
    },
    {
      title: 'Compartilhe o link',
      text: 'Alguns minutos depois, ele está no ar aqui. Envie o link às pessoas que vão usá-lo e guarde o seu link de edição: é por ele que você muda o app.',
    },
    {
      title: 'Peça mais',
      text: 'No mesmo campo: algo novo, algo diferente, ou como ele está indo, quem o usou e o que foi preenchido. As versões anteriores são mantidas, então a anterior pode voltar a ficar no ar.',
    },
  ],

  questionsTitle: 'Bom saber',
  questions: (showcase) => [
    ['Preciso saber programar?', 'Não. Você descreve o que o app deve fazer, e o agente o escreve. Você nunca precisa olhar o código.'],
    ['Quem pode abrir o meu app?', 'Quem tiver o link dele e conseguir acessar este site. Envie-o às pessoas que devem usá-lo.'],
    ['Onde ficam guardados os dados que as pessoas preenchem?', 'Cada app guarda os seus próprios registros nesta instalação, que a sua organização mantém.'],
    [
      'Como volto ao meu app?',
      'Este navegador lembra os apps que você criou aqui, ao lado do campo em que os criou. Em outro dispositivo, abra o link de edição que você recebeu com o app. Guarde-o em um lugar seguro, pois ninguém consegue recuperá-lo.',
    ],
    [
      'Algo não funciona. E agora?',
      'Diga ao agente o que está errado, do mesmo jeito que você pediu o app. Se uma mudança piorou as coisas, coloque a versão anterior de volta no ar no editor do app.',
    ],
    ['O que os colegas criaram?', <>Os apps que eles escolheram mostrar estão na {showcase('vitrine')}.</>],
  ],
};
