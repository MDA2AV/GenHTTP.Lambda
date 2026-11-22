import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'Apps com IA e alojamento grátis',
  title: 'Construir demora um minuto.',
  titleAccent: 'O resto já vem incluído.',
  intro:
    'Descreve uma app ao nosso agente de IA - ou ao Claude Code, Codex ou Cursor - e ela fica online em minutos, alojada por nós, com um link para partilhar. Tudo o que a torna útil já lá está: os seus registos, as suas versões e para que serve, quem a visita, e um domínio próprio quando ganha adeptos.',
  build: 'Construir algo',
  ownAgent: 'Usar o teu próprio agente',
  free: 'Grátis. Sem registo, sem cartão de crédito.',
  heroAlt: 'Um clube de ténis pede um site de reserva de campos e o agente responde que está online - com um botão para o abrir e o link de edição a guardar',
  example: 'Todas as imagens desta página mostram a mesma app de exemplo: a reserva de campos de um clube de ténis.',

  contents: 'Nesta página',

  makeTitle: 'Constrói, altera',
  makeText: 'Diz o que precisas, e continua a dizê-lo. Ninguém tem de escrever nem de ler código para isso.',

  behindTitle: 'Base de dados, histórico de versões e análise de visitas para cada app',
  behindText:
    'O que torna uma app útil é tudo o que existe à volta do seu código - ao dispor de quem a fez, e não só de quem sabe ler código.',

  yoursTitle: 'Teu para ires mais longe',
  yoursText: 'Para programadores, e para quem quer que a sua app sobreviva a este site.',

  features: {
    build: {
      name: 'Construir',
      title: 'Descreve-a e ela fica online',
      text: 'Diz ao nosso agente o que precisas, pelas tuas palavras - ou pede ao Claude Code, ao Codex ou ao Cursor. Passados alguns minutos, a app corre num endereço próprio, pronta a partilhar. Não há servidor, base de dados nem conta para configurar.',
      points: ['O nosso agente na página de construção, ou o teu por MCP', 'Um link para partilhar em minutos', 'Grátis, sem registo'],
      alt: 'A página de reserva de campos: quatro campos, hora a hora, com as horas reservadas identificadas',
    },
    change: {
      name: 'Alterar',
      title: 'Pergunta sobre ela, altera-a',
      text: 'As tuas apps ficam na lista da página de criação, cada uma com uma conversa. Pergunta como vai uma - quem a visitou, o que as pessoas introduziram, se algo está errado - ou diz o que deve ser diferente, e o agente responde ou altera-a. Ou entrega o link de edição ao teu próprio agente.',
      points: ['Respostas sobre visitantes, registos e erros', 'Fica online sozinha quando funciona, ou guardada como rascunho para a experimentares primeiro', 'O código fica fora da vista, a menos que o queiras ver'],
      alt: 'A página de criação: o clube perguntou quantos campos foram reservados esta semana, o agente respondeu, e as aulas do treinador estão prontas a experimentar num rascunho',
    },
    drafts: {
      name: 'Rascunhos',
      title: 'Experimenta uma alteração antes de alguém a ver',
      text: 'Uma alteração maior faz-se num rascunho: a app como vai ficar, num endereço próprio, com uma cópia dos registos para a experimentares. Nada do que se faz ali toca nos registos verdadeiros. Quando estiver certo, um clique pô-lo online.',
      points: ['Um endereço próprio, para experimentar e mostrar a outros', 'Dados de teste: uma cópia dos registos, não os registos', 'Pôr online quando estiver certo, ou descartar'],
      alt: 'Um rascunho da página de reservas, onde as aulas do treinador aparecem a roxo - numa cópia das reservas',
    },
    history: {
      name: 'Histórico',
      title: 'Cada alteração é uma versão, com caminho de volta',
      text: 'Cada alteração passa a ser uma nova versão, descrita pelo que mudou. Se uma correr mal, a versão anterior está a um clique - no mesmo endereço, com os mesmos registos.',
      points: ['O que foi pedido e o que mudou', 'De volta a uma versão anterior com um clique', 'Os registos são partilhados por todas as versões, por isso voltar atrás não perde nenhum'],
      alt: 'O histórico da app: as suas alterações, umas por baixo das outras, a mais recente online',
    },
    about: {
      name: 'Sobre',
      title: 'Sabe o que é',
      text: 'Com cada alteração, o agente escreve para que serve a app, quem a usa e porque funciona como funciona. Lês isso em palavras simples, e o agente seguinte lê-o antes de mudar o que quer que seja - assim, cada alteração assenta no que existe em vez de adivinhar.',
      points: ['O que faz e para quem, em palavras simples', 'As decisões por trás dela, para quem a alterar a seguir', 'Como é testada, guardado com cada versão'],
      alt: 'Sobre a tua app: a reserva de campos descrita em palavras simples, quem a usa e o que faz',
    },
    records: {
      name: 'Registos',
      title: 'Registos numa base de dados própria',
      text: 'Uma app que guarda coisas - reservas, inscrições, resultados - guarda-as numa base de dados própria, e elas ficam quando a app muda. Lê-as tabela a tabela no editor, ao lado dos ficheiros que ela guardou, como a lista semanal para o painel de avisos.',
      points: ['Uma base de dados para cada app que precisar', 'Todas as tabelas, legíveis no editor', 'Os ficheiros que a app guarda, listados ao lado'],
      alt: 'Os registos da app: as suas reservas e os seus sócios, lidos tabela a tabela',
    },
    keys: {
      name: 'Chaves e palavras-passe',
      title: 'Chaves e palavras-passe ficam secretas',
      text: 'Quando uma app precisa de uma palavra-passe ou da chave de outro serviço, o agente só a pede pelo nome. Tu escreves o valor no editor e, a partir daí, ninguém o vê - nem tu, nem o agente. Só a app o lê.',
      points: ['O agente nunca vê o valor', 'Guardado cifrado, nunca mais mostrado', 'Substituído quando quiseres'],
      alt: 'Chaves e palavras-passe: a palavra-passe da receção está definida, e o seu valor está escondido',
    },
    analytics: {
      name: 'Análise de visitas',
      title: 'Vê quem visita, sem cookies',
      text: 'Ativa a análise de visitas e vê quantas pessoas abriram a app, de onde vieram e em que dispositivos - contado no nosso servidor, sem cookies e sem nenhum script nas tuas páginas.',
      points: ['Visitantes, visualizações de páginas e de onde vieram', 'Sem cookies e sem nada que identifique um visitante', 'Desativada até a ativares'],
      alt: 'Análise de visitas: os visitantes dos últimos trinta dias, dia a dia',
    },
    domain: {
      name: 'Domínio',
      title: 'Um domínio próprio',
      text: 'Cada app começa num endereço nosso. Quando pega, pode passar para um domínio teu: a mesma app e o mesmo editor, num endereço que as pessoas fixam. Os links para o endereço antigo continuam a funcionar - levam ao novo.',
      points: ['Para organizações sem fins lucrativos, a pedido; para empresas, por acordo', 'A mesma app, editor e registos', 'O endereço antigo reencaminha os visitantes'],
      alt: 'O endereço da app a passar do seu endereço em genhttp.run para o domínio do clube',
    },
    code: {
      name: 'Código e git',
      title: 'O código, quando o quiseres',
      text: 'Passa o editor para a vista completa, e o código de cada versão está lá, com os seus registos de atividade e os seus números. Clona a app com git e faz push de uma alteração, ou descarrega-a como um projeto .NET que corre em qualquer lado. O código é teu.',
      points: ['Todos os ficheiros, comparados de versão para versão', 'Git: um commit para main é uma versão, um ramo é um rascunho', 'Um download que corre sozinho, com os registos incluídos'],
      alt: 'A vista completa do editor: os ficheiros da app e o seu código',
    },
    publish: {
      name: 'Montra e código',
      title: 'Mostra-a, ou publica o seu código',
      text: 'Põe a app na montra para que outros a encontrem, ou publica o seu código sob uma licença de código aberto para toda a gente ler, marcar com estrela e descarregar. Ambas as coisas são escolha tua, e nenhuma partilha os registos.',
      points: ['Na montra, com uma imagem e uma descrição', 'O seu código sob MIT ou outra licença', 'Nunca os registos, as chaves nem os visitantes'],
      alt: 'O código publicado da reserva de campos: os seus ficheiros, a sua documentação e as suas versões, sob a licença MIT',
    },
  },

  draftBadge: 'Rascunho',
  domainOwn: 'Produção',
  domainSubject: 'Um domínio para a minha app',
  domainAsk: 'Perguntar sobre um domínio',
  zoom: 'Ampliar',
  close: 'Fechar',

  closeTitle: 'Experimenta grátis.',
  closeAccent: 'Sem registo.',
  closeBuild: 'Construir algo',
  closeShip: 'Publicar o que construíste',
  openSource: (selfHosting, github) => (
    <>
      O GenHTTP Lambda é código aberto. Usa-o aqui, ou {selfHosting('corre-o num servidor teu')} - o código está no{' '}
      {github('GitHub')}.
    </>
  ),
};
