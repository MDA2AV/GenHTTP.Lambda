import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community edition',
  headline: 'A plataforma onde seus agentes criam apps.',
  headlineAccent: 'Código aberto, em um servidor próprio.',
  intro:
    'Descreva um app e o agente de criação o escreve - ou deixe o Claude Code, o Codex ou o Cursor fazer isso via MCP. Cada app é um handler GenHTTP em C#, compilado neste servidor e no ar em segundos, com banco de dados, chaves e senhas, versões e git.',
  describe: 'Descrever um app',
  connect: 'Conectar seu agente',
  write: 'Escrever o código você mesmo',

  buildTitle: 'Descreva',
  buildText: 'Uma frase basta. O agente de criação escreve o app, o coloca no ar e entrega o link para você mudá-lo depois.',
  noAgent: 'Nenhum agente de criação está conectado a esta instalação. Conecte seu próprio agente abaixo - ele faz o mesmo.',

  connectTitle: 'Conecte seu agente',
  connectText:
    'Seu agente de programação recebe a plataforma como ferramentas: criar um app, escrever e verificar o código, testar uma mudança em um rascunho com uma cópia dos dados, colocá-la no ar e ler os logs.',
  connectThen: 'Depois peça algo a ele, por exemplo: “Crie um livro de visitas, com uma página onde eu aprovo as entradas.”',

  featuresTitle: 'O que todo app recebe',
  features: [
    { title: 'Um endereço', text: 'Um endereço próprio neste servidor, com o nome da chave - e um domínio próprio quando virar premium.' },
    { title: 'Registros', text: 'Um banco de dados SQLite próprio, com tabelas criadas por migrações, lido e gravado com o Entity Framework Core.' },
    { title: 'Arquivos', text: 'Uma pasta para o que o app salva: uploads, exportações, tudo o que não é um registro.' },
    { title: 'Chaves e senhas', text: 'Guardadas com segurança no servidor. O agente diz quais precisa, você as informa, e ninguém as lê de volta.' },
    { title: 'Rascunhos e versões', text: 'Uma mudança é testada em um rascunho com uma cópia dos dados e depois colocada no ar. As versões anteriores são guardadas, a um clique de voltar.' },
    { title: 'Git', text: 'Cada app é um repositório: clone-o, faça push de um commit para salvar uma versão - com -o deploy para colocá-la no ar - ou de um branch para começar um rascunho.' },
    { title: 'Visitantes', text: 'Visualizações de página, de onde os visitantes vieram e seus países - contados no servidor, sem cookies, assim que você ativar.' },
    { title: 'Seu código', text: 'Baixe qualquer app como um projeto .NET simples e execute onde quiser, ou publique o código-fonte para outros lerem.' },
  ],

  runsTitle: 'Como funciona',
  runs: [
    { title: 'Um processo', text: 'O servidor, o editor, a API e todos os apps rodam em um único processo e guardam tudo em uma única pasta. O agente de criação, quando há um, roda ao lado.' },
    {
      title: 'Por caminho ou por host',
      text: 'Os apps respondem abaixo de /lambda/ no endereço da instalação - ou, se ela tiver um domínio para eles, cada um em um subdomínio próprio.',
    },
    {
      title: 'Código em que você confia',
      text: 'Os apps são compilados no servidor, e seu código é verificado quanto ao que não podem fazer - mas compartilham o processo dele. Execute o que você executaria por conta própria.',
    },
    { title: 'Código aberto', text: 'Licença MIT: leia, mude e execute onde quiser.' },
  ],
  repository: 'O código-fonte no GitHub',

  aroundTitle: 'Dê uma olhada',
  around: {
    demos: { title: 'Comece por uma demo', text: 'Uma API sobre registros, um login, atualizações ao vivo, um jogo: cada um um app pronto para copiar e mudar.' },
    showcase: { title: 'Vitrine', text: 'Os apps que seus donos escolheram mostrar aqui.' },
    guide: { title: 'Guia', text: 'O que um app pode fazer e como, na ordem em que você precisa.' },
    api: { title: 'API REST', text: 'O mesmo que as ferramentas, via HTTP, descrito em OpenAPI.' },
    sources: { title: 'Códigos-fonte publicados', text: 'Apps cujos donos publicaram o código, para ler e baixar.' },
    admin: { title: 'Administração', text: 'Os apps, os limites, os bloqueios e o log, protegidos pelo token de administrador.' },
  },
};
