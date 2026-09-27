import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Uma plataforma de programação agêntica',
  headline: 'Descreva uma aplicação.',
  headlineAccent: 'Seu agente a coloca no ar.',
  intro:
    'Enquetes, livros de visitas, rankings, pequenas lojas: descreva o que você precisa ao nosso agente ou ao que você já utiliza e receba uma aplicação funcional com um link para compartilhar. A aplicação continua editável, para que você possa aprimorá-la muito depois da primeira versão.',
  build: 'Criar uma aplicação',
  ownAgent: 'Usar seu próprio agente',
  free: 'Gratuito. Sem conta e sem instalação.',
  seeIt: 'Ver uma demonstração',

  videoTitle: 'De uma frase a uma aplicação no ar',
  videoText:
    'Uma janela anônima do navegador, nenhuma conta e uma única solicitação na página Criar – em seguida, a aplicação pronta, aberta pelo seu link, exatamente como qualquer visitante a verá.',
  videoNote: 'A criação é exibida em velocidade acelerada. Todo o restante, em tempo real.',
  tryIt: 'Experimentar',

  oneShotTitle: 'Muito além de um resultado pontual',
  oneShotText:
    'A maioria dos geradores entrega um resultado e encerra ali. Aqui, a aplicação continua em execução onde foi criada, para que você e seu agente possam seguir desenvolvendo-a.',
  steps: [
    {
      title: 'Descreva o que você precisa',
      body: 'Explique com suas próprias palavras, ao agente deste site ou ao que você já utiliza. Sem código, sem configuração e sem conta.',
      alt: 'A página Criar com uma solicitação de enquete para o almoço',
    },
    {
      title: 'Uma aplicação funcional e um link',
      body: 'A aplicação é criada, implantada e entregue como um endereço público que você pode compartilhar. Ela mantém seus dados – votos, pontuações, mensagens –, para que todos vejam o mesmo estado.',
      alt: 'A enquete concluída, aberta em um navegador',
    },
    {
      title: 'Aprimore continuamente',
      body: 'Cada aplicação vem com um link de edição privado. Entregue-o ao seu agente com a próxima alteração ou abra-o você mesmo. Cada alteração se torna uma nova versão, e o endereço permanece o mesmo.',
      alt: 'O centro de controle da enquete: suas versões, cada uma com a solicitação, a alteração e a diferença em relação à anterior',
    },
  ],
  weekLater: 'Uma semana depois',
  weekAsk:
    'Este é o link de edição da minha enquete. Por favor, encerre a votação às 11h das sextas-feiras e mostre o resultado no topo.',
  weekAnswer:
    'Pronto. A versão 4 está no ar no mesmo endereço, e a versão 3 continua disponível caso você queira reverter.',

  agentsTitle: 'Use o agente de sua preferência',
  agentsText:
    'Você já trabalha com o Claude ou outro assistente? Conecte-o a este endereço e ele poderá criar, implantar e atualizar aplicações aqui, diretamente da conversa que você já tem aberta.',
  agents: [
    {
      name: 'Claude na web ou no desktop',
      how: 'Abra as Configurações, depois “Connectors”, e escolha “Add custom connector”. Cole o endereço acima – não é necessária chave de API nem login.',
    },
    {
      name: 'Claude Code',
      how: 'Execute uma vez em um terminal:',
    },
    {
      name: 'Outros clientes MCP',
      how: 'Cursor, VS Code, Codex e outros clientes MCP oferecem suporte a servidores remotos. Configure-os com o mesmo endereço.',
    },
  ],
  thenAsk: (em) => (
    <>Em seguida, basta pedir: {em('crie uma lista de inscrição para o evento da nossa equipe e publique-a')}.</>
  ),

  contactTitle: 'Fale conosco',
  contactText:
    'Precisa de ajuda, está planejando algo maior ou procura uma solução desenvolvida sob medida? Teremos prazer em atendê-lo.',
  mailTitle: 'Por e-mail',
  mailText: 'Para projetos, consultas e qualquer assunto que você prefira tratar de forma reservada.',
  discordTitle: 'Participe do Discord',
  discordText: 'Apresente o que você criou, obtenha ajuda e converse diretamente com a equipe.',
  discordLink: 'O Discord do GenHTTP',

  terms: 'Termos de serviço',
  writeCode: 'Escrever o código você mesmo',
  contact: 'Contato',
};
