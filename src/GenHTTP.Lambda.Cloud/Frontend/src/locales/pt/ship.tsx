import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hospedagem grátis de vibe coding',
  title: 'Do localhost para a tela de todo mundo.',
  intro:
    'Você criou um app com o Claude Code, o Codex ou o Cursor, mas ele só roda na sua máquina. Peça para o seu agente publicar aqui. Em poucos minutos, o app ganha um link público para qualquer pessoa, um banco de dados próprio e uma conexão ao vivo com todo mundo que está com ele aberto: as pessoas podem jogar, conversar e postar juntas.',
  facts: ['Grátis', 'Sem cadastro', 'Sem cartão de crédito', 'Nada para instalar'],

  stepsTitle: 'Do localhost a um link público em três passos',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Conecte uma vez',
      body: 'Adicione um endereço, um servidor MCP remoto, ao Claude Code, ao Codex, ao Cursor ou ao agente que você usa. Leva menos de um minuto, e é só uma vez.',
    },
    {
      title: 'Peça para publicar',
      body: 'Diga para ele colocar o app no ar aqui. Ele empacota o app, faz o deploy e confere se está respondendo. Sem repositório no GitHub, sem pipeline de deploy, sem Docker.',
    },
    {
      title: 'Compartilhe o link',
      body: 'Você recebe um endereço público e um link de edição privado. Mande o primeiro para quem quiser. Guarde o segundo: é com ele que você muda o app depois.',
    },
  ],

  togetherTitle: 'Mais que hospedagem: banco de dados e multiplayer incluídos.',
  together:
    'A maioria das hospedagens entrega uma cópia do app para cada visitante, e cada um joga sozinho: o que um navegador guarda no localStorage, o próximo nunca vê. Aqui, todo app tem banco de dados próprio e uma conexão ao vivo com todo mundo que está com ele aberto. O que uma pessoa faz aparece na hora para as outras, e o que elas postam continua lá amanhã.',
  sayLike: 'Depois, no seu projeto, diga algo como',
  asks: [
    'Publica este app no GenHTTP Lambda e me manda o link.',
    'Deixa os recordes compartilhados, para todo mundo ver o mesmo ranking.',
  ],

  questionsTitle: 'Antes de publicar',
  questions: (offline, removed, showcase, terms, selfHosting) => [
    [
      'É grátis mesmo?',
      <>
        Sim. Sem cadastro, sem cartão de crédito e sem período de teste. Seu app fica no ar enquanto as pessoas usarem.
        Depois de {offline} dias sem nenhuma visita ou mudança, ele sai do ar, e depois de {removed} dias é removido.
      </>,
    ],
    [
      'O Claude Code, o Codex ou o Cursor conseguem publicar meu app aqui?',
      'Sim, e qualquer outro agente que aceite um servidor MCP remoto. Conecte uma vez com o endereço acima e peça para publicar: ele faz o deploy, confere se o app responde e manda o link para você.',
    ],
    [
      'Por que meus amigos não conseguem abrir meu link do localhost?',
      'Porque localhost é o seu próprio computador: o endereço só funciona nele, e só enquanto o app está rodando. Um túnel empresta um endereço público enquanto o notebook estiver ligado. Publicado aqui, o app roda em nossos servidores, com um link que continua funcionando com o notebook fechado.',
    ],
    [
      'Preciso de servidor, backend ou Supabase?',
      'Não. Todo app tem banco de dados próprio, armazenamento de arquivos e uma conexão ao vivo com todo mundo que está com ele aberto. Não há servidor para alugar nem outro serviço para configurar, e nada para manter rodando do seu lado.',
    ],
    [
      'Posso deixar meu jogo multiplayer sem manter um servidor?',
      'Sim. O que um navegador guarda no localStorage o próximo nunca vê, então a parte compartilhada precisa ficar em um servidor, e aqui o servidor é nosso. Peça para o seu agente deixar o jogo multiplayer, e cada jogada chega a todo mundo que está com ele aberto.',
    ],
    [
      'Meu app precisa ser feito de um jeito específico?',
      'Não, seu agente cuida disso. Páginas, imagens e estilos sobem do jeito que estão, e o que precisa rodar no servidor o agente adapta para esta plataforma. Você descreve o que o app deve fazer, e ele faz a tradução.',
    ],
    [
      'Como eu mudo o app depois?',
      'Com o link de edição que você recebeu ao publicar. Passe para o seu agente junto com a próxima mudança, ou abra no navegador. Cada mudança vira uma nova versão no mesmo endereço, e você pode voltar para uma versão anterior quando quiser.',
    ],
    [
      'Posso fazer push nele com git?',
      'Pode. Todo app também é um repositório git: clone-o pelo endereço em Clonar no editor, mude com suas próprias ferramentas ou seu agente e faça o push. Cada commit enviado para a main vira a próxima versão, e um branch enviado vira um rascunho com uma prévia própria.',
    ],
    [
      'Onde ficam minhas chaves de API?',
      'Fora do código. Seu agente pede uma chave pelo nome, e você digita o valor no editor. Ninguém consegue ler de volta: nem o editor, nem o agente.',
    ],
    [
      'Posso levar meu código?',
      'Pode, ele é seu. Baixe pelo editor quando quiser, como um projeto que roda sozinho, com o banco de dados junto - ou clone com git, com todas as versões.',
    ],
    [
      'Posso hospedar no meu próprio servidor?',
      <>
        Pode. O GenHTTP Lambda é código aberto, e a edição Community é a mesma plataforma em um único container Docker,
        para o seu servidor e o seu agente. {selfHosting('Nuvem ou auto-hospedagem')} explica o que muda.
      </>,
    ],
    [
      'Quem pode ver meu app?',
      <>
        Qualquer pessoa que tiver o link. O app não fica listado em lugar nenhum, a não ser que você decida mostrar
        na {showcase('vitrine')}.
      </>,
    ],
    [
      'Tem algo que eu não posso publicar?',
      <>Poucas coisas, como algo que prejudique ou engane pessoas. Os {terms('termos')} são curtos e em linguagem simples.</>,
    ],
  ],
  noAgent: 'Sem agente? Crie aqui',

};
