import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Auto-hospedagem',

  eyebrow: 'Código aberto · nuvem ou auto-hospedado',
  title: 'Use aqui,',
  titleAccent: 'ou hospede você mesmo.',
  intro:
    'O GenHTTP Lambda é código aberto sob a licença MIT. O genhttp.dev o executa para qualquer pessoa usar, e a edição Community é a mesma plataforma como imagem Docker, para quem prefere rodá-la por conta própria. Os dois criam e hospedam os mesmos apps; muda onde eles ficam e quem cuida deles.',

  cloud: {
    kicker: 'Nuvem',
    name: 'genhttp.dev',
    summary: 'Abra este site e comece. O servidor, as atualizações e os limites ficam por nossa conta.',
    points: ['Nada para instalar nem para rodar', 'Os apps respondem em um endereço em genhttp.run', 'Agente de criação incluído'],
    action: 'Criar algo',
  },
  community: {
    kicker: 'Auto-hospedado',
    name: 'Edição Community',
    summary: 'Um container Docker em uma máquina sua. O servidor, as atualizações e os limites ficam por sua conta.',
    points: ['Um container, um volume, nenhum servidor de banco de dados', 'Os apps respondem no seu endereço', 'O agente de criação como um container à parte'],
    action: 'Rodar você mesmo',
  },

  sameTitle: 'Igual nos dois',
  sameText: 'A edição Community não é uma versão menor. Todo app recebe as mesmas coisas, onde quer que rode:',
  same: [
    'O agente de criação',
    'Seu próprio agente via MCP',
    'A API REST',
    'O editor, simples e completo',
    'Rascunhos com dados de teste',
    'Versões e um caminho de volta',
    'Registros, arquivos, chaves e senhas',
    'Analytics sem cookies',
    'Domínios próprios',
    'Git e downloads',
    'A vitrine e o código publicado',
  ],

  differTitle: 'O que muda',
  aspect: 'Pergunta',
  rows: (offline) => [
    ['Onde os apps rodam', 'No nosso servidor', 'No seu, em um container com um volume'],
    ['Como começar', 'Abra a página de criação - sem conta', 'Inicie a imagem e abra o endereço dela'],
    ['Quem cuida do servidor', 'Nós', 'Você'],
    ['Quanto custa', 'Nada; apps Premium sob consulta', 'Nada; o servidor é por sua conta'],
    ['Onde um app responde', 'Em um subdomínio de genhttp.run', 'Abaixo do seu endereço, ou em subdomínios de um domínio que você informar'],
    ['Um domínio próprio', 'Para apps Premium', 'Para qualquer app que você levar para produção'],
    ['Apps que ninguém usa', `Saem do ar após ${offline} dias sem visita nem mudança, a menos que sejam Premium`, 'Como você definir'],
    ['Limites e regras', 'Nossos, e nossos termos de uso', 'Seus, definidos no painel de administração'],
    ['O agente de criação', 'Incluído, com um número de criações por dia', 'Opcional, com a sua própria assinatura do Claude ou chave de API'],
    ['Relatórios de uso', 'Nenhum', 'Uma vez por dia: a versão, a máquina, as opções e contagens dos apps - nunca um app nem uma pessoa. Você pode desligar.'],
  ],

  moveTitle: 'Passar de um para o outro',
  move:
    'Os mesmos apps rodam nos dois, e seu agente leva um deles com git: ele cria o app do outro lado, copia os arquivos do antigo e faz push. Um download é um projeto .NET que roda em qualquer lugar, com os registros em um arquivo SQLite.',

  pickTitle: 'Qual combina com você',
  pickCloud: {
    title: 'genhttp.dev, se',
    points: [
      'você quer um app no ar sem rodar nada',
      'as pessoas que o usam estão em qualquer lugar',
      'você prefere não cuidar de um servidor',
    ],
  },
  pickCommunity: {
    title: 'A edição Community, se',
    points: [
      'seus apps e seus registros devem ficar em uma máquina sua',
      'são ferramentas para sua equipe, dentro da sua própria rede',
      'você quer definir os limites e as regras',
    ],
  },

  startTitle: 'Como começar',
  startCloud: {
    title: 'No genhttp.dev',
    text: 'Descreva o que você precisa na página de criação, ou conecte o agente que você já usa. Não é preciso criar conta.',
  },
  startCommunity: {
    title: 'No seu servidor',
    text: 'Um comando inicia tudo. Abra o endereço, conecte seu agente e crie - o README explica o resto.',
  },
  connect: 'Conectar seu agente',
  readme: 'Ler o README',
  dockerHub: 'A imagem no Docker Hub',

  sourceTitle: 'Código aberto',
  source: (github) => (
    <>
      O GenHTTP Lambda é distribuído sob a licença MIT. O código, as issues e os releases estão no {github('GitHub')}.
    </>
  ),
};
