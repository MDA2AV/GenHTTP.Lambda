import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Programação com agentes de IA',
  headline: 'Descreva um app.',
  headlineAccent: 'Seu agente coloca no ar.',
  intro:
    'Enquetes, livros de visitas, rankings, lojinhas. Descreva o que você precisa para o nosso agente ou para o que você já usa, e receba um app funcionando, com link para compartilhar. E o app continua editável: dá para seguir melhorando muito depois da primeira versão.',
  build: 'Criar um app',
  ownAgent: 'Usar seu agente',
  free: 'Grátis. Sem conta, sem instalar nada.',
  seeIt: 'Veja na prática',

  videoTitle: 'De uma frase a um app no ar',
  videoText:
    'Uma janela anônima, nenhuma conta e um único pedido na página de criação. Depois, o app pronto, aberto pelo link, do jeito que qualquer visitante abriria.',
  videoNote: 'A criação aparece acelerada. O resto é em tempo real.',
  tryIt: 'Experimente você também',

  oneShotTitle: 'Não para na primeira versão',
  oneShotText:
    'A maioria dos geradores entrega um resultado e deixa você se virar com ele. Aqui, o app continua rodando onde foi criado, então você e seu agente podem seguir trabalhando nele.',
  steps: [
    {
      title: 'Diga o que você quer',
      body: 'Explique com suas palavras, para o agente deste site ou para o que você já usa. Sem código, sem configuração, sem conta.',
      alt: 'A página de criação com um pedido de enquete para o almoço',
    },
    {
      title: 'Receba um app pronto e um link',
      body: 'O app é criado, vai para o ar e você recebe um endereço público para compartilhar. Ele guarda os dados (votos, pontuações, mensagens), então todo mundo que abre vê a mesma coisa.',
      alt: 'A enquete do almoço pronta, aberta no navegador',
    },
    {
      title: 'Continue melhorando',
      body: 'Todo app vem com um link de edição privado. Passe o link para o seu agente junto com a próxima mudança, ou abra você mesmo. Cada mudança que você pede vira uma versão própria, e o endereço continua o mesmo.',
      alt: 'O painel de controle da enquete: as versões, cada uma com o pedido, o que mudou e a diferença para a anterior',
    },
  ],
  weekLater: 'Uma semana depois',
  weekAsk:
    'Segue o link de edição da minha enquete do almoço. Fecha a votação às 11h nas sextas e mostra o vencedor no topo, por favor.',
  weekAnswer:
    'Pronto! A versão 4 já está no ar, no mesmo endereço. A versão 3 continua disponível, caso você queira voltar.',

  agentsTitle: 'Traga seu agente favorito',
  agentsText:
    'Já usa o Claude ou outro assistente? É só conectar a este endereço. Aí ele cria, coloca no ar e atualiza apps aqui, direto da conversa que você já tem aberta.',
  agents: [
    {
      name: 'Claude na web ou no desktop',
      how: 'Abra Configurações, depois “Connectors”, e escolha “Add custom connector”. Cole o endereço acima. Não precisa de chave de API nem de login.',
    },
    {
      name: 'Claude Code',
      how: 'Rode isto uma vez no terminal:',
    },
    {
      name: 'Outros clientes MCP',
      how: 'Cursor, VS Code, Codex e outros clientes MCP aceitam servidores remotos. Configure com o mesmo endereço.',
    },
  ],
  thenAsk: (em) => (
    <>Depois é só pedir: {em('cria uma lista de inscrições para o evento da equipe e coloca no ar')}.</>
  ),

  contactTitle: 'Fale com a gente',
  contactText:
    'Precisa de ajuda, está planejando algo maior ou quer uma solução sob medida? Adoraríamos conversar com você.',
  mailTitle: 'Mande um e-mail',
  mailText: 'Para projetos, dúvidas e tudo o que você preferir tratar em particular.',
  discordTitle: 'Entre no Discord',
  discordText: 'Mostre o que você criou, peça ajuda para o próximo passo e converse direto com a equipe.',
  discordLink: 'O Discord do GenHTTP',
};
