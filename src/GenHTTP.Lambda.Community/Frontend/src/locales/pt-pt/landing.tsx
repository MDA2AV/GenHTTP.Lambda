import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community edition',
  headline: 'A plataforma onde os teus agentes criam apps.',
  headlineAccent: 'A funcionar no teu próprio servidor.',
  intro:
    'Descreve uma app e o agente de criação escreve-a - ou deixa o Claude Code, o Codex ou o Cursor fazê-lo por MCP. Cada app é um handler GenHTTP em C#, compilado neste servidor e online em segundos, com base de dados, chaves e palavras-passe, versões e git.',
  describe: 'Descrever uma app',
  connect: 'Ligar o teu agente',
  write: 'Escrever o código tu mesmo',

  buildTitle: 'Descreve-a',
  buildText: 'Uma frase chega. O agente de criação escreve a app, põe-na online e dá-te o link para a alterares mais tarde.',
  noAgent: 'Nenhum agente de criação está ligado a esta instalação. Liga abaixo o teu próprio agente - ele faz o mesmo.',

  connectTitle: 'Liga o teu agente',
  connectText:
    'O teu agente de programação recebe a plataforma como ferramentas: criar uma app, escrever e verificar o seu código, experimentar uma alteração num rascunho com uma cópia dos dados, pô-la online e ler os seus registos.',
  connectThen: 'Depois pede-lhe algo, por exemplo: «Cria um livro de visitas, com uma página onde eu aprovo as entradas.»',

  featuresTitle: 'O que cada app recebe',
  features: [
    { title: 'Um endereço', text: 'Online sob /lambda/ neste servidor assim que compila - ou num domínio próprio.' },
    { title: 'Registos', text: 'Uma base de dados SQLite própria, com as tabelas criadas por migrações, lida e escrita com o Entity Framework Core.' },
    { title: 'Ficheiros', text: 'Uma pasta para o que a app guarda: uploads, exportações, tudo o que não é um registo.' },
    { title: 'Chaves e palavras-passe', text: 'Guardadas cifradas no servidor. O agente diz quais precisa, tu introduzi-las, e ninguém as volta a ler.' },
    { title: 'Rascunhos e versões', text: 'Uma alteração é experimentada num rascunho com uma cópia dos dados e depois posta online. Cada versão fica guardada, a um clique de voltar atrás.' },
    { title: 'Git', text: 'Cada app é um repositório: clona-o, faz push de um commit para a pôr online, faz push de um ramo para começar um rascunho.' },
    { title: 'Visitantes', text: 'Visualizações de páginas, de onde vieram os visitantes e os seus países - contados no servidor, sem cookies, depois de o ativares.' },
    { title: 'O teu código', text: 'Descarrega qualquer app como um projeto .NET simples e executa-a onde quiseres, ou publica o seu código-fonte para outros lerem.' },
  ],

  runsTitle: 'Como funciona',
  runs: [
    { title: 'Um só contentor', text: 'O servidor, o editor, a API e todas as apps num só processo, e tudo o que guardam numa só pasta.' },
    {
      title: 'Por caminho ou por host',
      text: 'As apps respondem sob /lambda/ neste endereço. Dá um domínio à instalação e cada uma passa a ter, em vez disso, um subdomínio próprio.',
    },
    {
      title: 'Código em que confias',
      text: 'As apps são compiladas dentro do servidor e o seu código é verificado quanto ao que não podem fazer - mas partilham o seu processo. Executa o que executarias tu mesmo.',
    },
    { title: 'Código aberto', text: 'Com licença MIT: lê-o, altera-o, executa-o onde quiseres.' },
  ],
  repository: 'O código-fonte no GitHub',

  aroundTitle: 'Dá uma vista de olhos',
  around: {
    demos: { title: 'Começar por uma demonstração', text: 'Uma API sobre registos, um início de sessão, atualizações em direto, um jogo: cada uma é uma app pronta para copiar e alterar.' },
    showcase: { title: 'Montra', text: 'As apps que os seus donos escolheram mostrar aqui.' },
    guide: { title: 'Guia', text: 'O que uma app pode fazer e como, pela ordem de que precisas.' },
    api: { title: 'API REST', text: 'O mesmo que as ferramentas, por HTTP, descrito em OpenAPI.' },
    sources: { title: 'Códigos publicados', text: 'Apps cujos donos publicaram o código, para ler e descarregar.' },
    admin: { title: 'Administração', text: 'As apps, os limites, os bloqueios e o registo, protegidos pelo token de administração.' },
  },
};
