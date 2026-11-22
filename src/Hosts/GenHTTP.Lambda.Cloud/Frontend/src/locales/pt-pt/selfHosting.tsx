import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Alojamento próprio',

  eyebrow: 'Código aberto · cloud ou alojamento próprio',
  title: 'Usa-o aqui,',
  titleAccent: 'ou aloja-o tu.',
  intro:
    'O GenHTTP Lambda é código aberto, sob a licença MIT. O genhttp.dev corre-o para qualquer pessoa usar, e a edição Community é a mesma plataforma como imagem Docker, para quem prefere geri-la por si. Ambas constroem e alojam as mesmas apps; o que muda é onde vivem e quem cuida delas.',

  cloud: {
    kicker: 'Cloud',
    name: 'genhttp.dev',
    summary: 'Abre este site e começa. O servidor, as suas atualizações e os seus limites ficam por nossa conta.',
    points: ['Nada para instalar nem para correr', 'As apps respondem num endereço em genhttp.run', 'O agente de construção incluído'],
    action: 'Construir algo',
  },
  community: {
    kicker: 'Alojamento próprio',
    name: 'Edição Community',
    summary: 'Um contentor Docker numa máquina tua. O servidor, as suas atualizações e os seus limites ficam por tua conta.',
    points: ['Um contentor, um volume, sem servidor de base de dados', 'As apps respondem no teu endereço', 'O agente de construção como contentor extra'],
    action: 'Corre-o tu',
  },

  sameTitle: 'Igual nas duas',
  sameText: 'A edição Community não é uma versão mais pequena. Cada app recebe as mesmas coisas, onde quer que corra:',
  same: [
    'O agente de construção',
    'O teu próprio agente por MCP',
    'A API REST',
    'O editor, simples e completo',
    'Rascunhos com dados de teste',
    'Versões e caminho de volta',
    'Registos, ficheiros, chaves e palavras-passe',
    'Análise de visitas sem cookies',
    'Domínios próprios',
    'Git e downloads',
    'A montra e o código publicado',
  ],

  differTitle: 'O que muda',
  aspect: 'Pergunta',
  rows: (offline) => [
    ['Onde correm as apps', 'No nosso servidor', 'No teu, num contentor com um volume'],
    ['Como começar', 'Abre a página de construção - sem conta', 'Inicia a imagem e abre o seu endereço'],
    ['Quem cuida do servidor', 'Nós', 'Tu'],
    ['Quanto custa', 'Nada para experimentar; produção para organizações sem fins lucrativos, a pedido, e para empresas, por acordo', 'Nada; o servidor és tu que o pagas'],
    ['Onde responde uma app', 'Num subdomínio de genhttp.run', 'Por baixo do teu endereço, ou em subdomínios de um domínio que lhe indiques'],
    ['Um domínio próprio', 'Para apps em produção, registado por ti', 'Para qualquer app que passes a produção'],
    ['Apps que ninguém usa', `Offline após ${offline} dias sem uma visita ou alteração, exceto em produção`, 'Como o definires'],
    ['Limites e regras', 'Nossos, e os nossos termos de utilização', 'Teus, definidos no painel de administração'],
    ['O agente de construção', 'Incluído, com um número de construções por dia', 'Opcional, com a tua própria subscrição do Claude ou chave de API'],
    ['Relatórios de utilização', 'Nenhum', 'Uma vez por dia: a versão, a máquina, as opções e contagens sobre as apps - nunca uma app nem uma pessoa. Podes desativá-lo.'],
  ],

  moveTitle: 'Passar de uma para a outra',
  move:
    'As mesmas apps correm nas duas, e o teu agente passa uma com git: cria a app no outro lado, copia para lá os ficheiros da antiga e faz push deles. Um download é um projeto .NET que corre em qualquer lado, com os registos num ficheiro SQLite.',

  pickTitle: 'Qual te convém',
  pickCloud: {
    title: 'genhttp.dev, se',
    points: [
      'queres uma app online sem gerir nada',
      'as pessoas que a usam estão em qualquer lugar',
      'preferes não cuidar de um servidor',
    ],
  },
  pickCommunity: {
    title: 'A edição Community, se',
    points: [
      'as tuas apps e os seus registos devem ficar numa máquina tua',
      'são ferramentas para a tua equipa, dentro da tua própria rede',
      'queres definir tu os limites e as regras',
    ],
  },

  startTitle: 'Como começar',
  startCloud: {
    title: 'No genhttp.dev',
    text: 'Descreve o que precisas na página de construção, ou liga o agente que já usas. Não há conta para criar.',
  },
  startCommunity: {
    title: 'No teu servidor',
    text: 'Um comando inicia-o. Abre o seu endereço, liga o teu agente e constrói - o README diz o resto.',
  },
  connect: 'Liga o teu agente',
  readme: 'Ler o README',
  dockerHub: 'A imagem no Docker Hub',

  sourceTitle: 'Código aberto',
  source: (github) => (
    <>
      O GenHTTP Lambda é publicado sob a licença MIT. O seu código, os seus issues e as suas versões estão no {github('GitHub')}.
    </>
  ),
};
