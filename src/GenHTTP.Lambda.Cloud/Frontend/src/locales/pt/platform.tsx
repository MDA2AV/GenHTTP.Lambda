import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'Apps com IA e hospedagem grátis',
  title: 'Criar leva um minuto.',
  titleAccent: 'O resto já vem incluído.',
  intro:
    'Descreva um app para o nosso agente de IA - ou para o Claude Code, o Codex ou o Cursor - e ele fica no ar em minutos, hospedado para você, com um link para compartilhar. Tudo o que o mantém útil já está lá: seus registros, suas versões e para que ele serve, quem o visita e um domínio próprio quando ele fizer sucesso.',
  build: 'Criar algo',
  ownAgent: 'Usar seu próprio agente',
  free: 'Grátis. Sem cadastro, sem cartão de crédito.',
  heroAlt: 'O editor de um app de reserva de quadras: no ar no domínio do próprio clube, quantas pessoas o usaram hoje e sua última mudança',
  example: 'Todas as imagens desta página mostram o mesmo app de exemplo: a reserva de quadras de um clube de tênis.',

  contents: 'Nesta página',

  makeTitle: 'Criar e mudar',
  makeText: 'Diga o que você precisa e continue dizendo. Ninguém precisa escrever nem ler código para isso.',

  behindTitle: 'Banco de dados, histórico de versões e analytics para todo app',
  behindText:
    'O que mantém um app útil é tudo o que existe em volta do código - disponível para quem o criou, e não só para quem sabe ler código.',

  yoursTitle: 'Seu para levar adiante',
  yoursText: 'Para desenvolvedores e para quem quer que o app continue existindo além deste site.',

  features: {
    build: {
      name: 'Criar',
      title: 'Descreva, e ele está no ar',
      text: 'Conte ao nosso agente o que você precisa, com suas próprias palavras, ou peça ao Claude Code, ao Codex ou ao Cursor. Poucos minutos depois, o app roda em um endereço próprio, pronto para compartilhar. Não há servidor, banco de dados nem conta para configurar.',
      points: ['Nosso agente na página de criação, ou o seu via MCP', 'Um link para compartilhar em minutos', 'Grátis, sem cadastro'],
      alt: 'A página de reserva de quadras: quatro quadras, hora a hora, com os horários reservados identificados',
    },
    change: {
      name: 'Mudar',
      title: 'Pergunte, mude',
      text: 'Seus apps ficam na lista da página de criação, cada um com uma conversa. Pergunte como um deles está - quem visitou, o que as pessoas digitaram, se algo está errado - ou diga o que deve ser diferente, e o agente responde ou muda o app. Ou entregue o link de edição dele ao seu próprio agente.',
      points: ['Respostas sobre visitantes, registros e erros', 'No ar sozinho quando funciona, ou guardado como rascunho para você testar antes', 'O código fica fora de vista, a menos que você queira'],
      alt: 'A página de criação: o clube perguntou quantas quadras foram reservadas esta semana, o agente respondeu, e as aulas do treinador estão prontas para testar em um rascunho',
    },
    drafts: {
      name: 'Rascunhos',
      title: 'Teste uma mudança antes que alguém a veja',
      text: 'Uma mudança maior é feita em um rascunho: o app como ele vai ficar, em um endereço próprio, com uma cópia dos registros para testar. Nada do que é feito ali mexe nos registros de verdade. Quando estiver certo, um clique o coloca no ar.',
      points: ['Um endereço próprio, para testar e para mostrar a outras pessoas', 'Dados de teste: uma cópia dos registros, não os registros', 'Coloque no ar quando estiver certo, ou descarte'],
      alt: 'Um rascunho da página de reservas, em que as aulas do treinador aparecem em roxo - em uma cópia das reservas',
    },
    history: {
      name: 'Histórico',
      title: 'Cada mudança é uma versão, com um caminho de volta',
      text: 'Cada mudança vira uma nova versão, descrita pelo que mudou. Se alguma der errado, a versão anterior está a um clique: no mesmo endereço, com os mesmos registros.',
      points: ['O que foi pedido e o que mudou', 'Volte a uma versão anterior com um clique', 'Os registros são compartilhados por todas as versões, então voltar não perde nenhum'],
      alt: 'O histórico do app: suas mudanças, uma sob a outra, a mais recente no ar',
    },
    about: {
      name: 'Sobre',
      title: 'Ele sabe o que é',
      text: 'A cada mudança, o agente anota para que o app serve, quem o usa e por que ele funciona do jeito que funciona. Você lê em linguagem simples, e o próximo agente lê antes de mudar qualquer coisa, então cada mudança parte do que já existe, em vez de adivinhar.',
      points: ['O que ele faz e para quem, em linguagem simples', 'As decisões por trás dele, para quem for mudá-lo depois', 'Como ele é testado, guardado em cada versão'],
      alt: 'Sobre seu app: a reserva de quadras descrita em linguagem simples, quem a usa e o que ela faz',
    },
    records: {
      name: 'Registros',
      title: 'Registros em um banco de dados próprio',
      text: 'Um app que guarda coisas - reservas, inscrições, pontuações - as guarda em um banco de dados próprio, e elas permanecem quando o app muda. Leia tudo tabela por tabela no editor, junto com os arquivos que ele salvou, como a lista semanal do mural.',
      points: ['Um banco de dados para cada app que precisa de um', 'Todas as tabelas, legíveis no editor', 'Os arquivos que o app salva, listados ao lado'],
      alt: 'Os registros do app: suas reservas e seus sócios, lidos tabela por tabela',
    },
    keys: {
      name: 'Chaves e senhas',
      title: 'Chaves e senhas continuam secretas',
      text: 'Quando um app precisa de uma senha ou da chave de outro serviço, o agente apenas a pede pelo nome. Você digita o valor no editor e, daí em diante, ninguém o vê: nem você, nem o agente. Só o app o lê.',
      points: ['O agente nunca vê o valor', 'Guardado com criptografia, nunca mostrado de novo', 'Troque quando quiser'],
      alt: 'Chaves e senhas: a senha da recepção está definida, e seu valor fica oculto',
    },
    analytics: {
      name: 'Analytics',
      title: 'Veja quem visita, sem cookies',
      text: 'Ative o Analytics e veja quantas pessoas abriram o app, de onde vieram e em quais dispositivos - contado no nosso servidor, sem cookies e sem nenhum script nas suas páginas.',
      points: ['Visitantes, visualizações de página e de onde vieram', 'Sem cookies e sem nada que identifique um visitante', 'Desligado até você ativar'],
      alt: 'Analytics: os visitantes dos últimos trinta dias, dia a dia',
    },
    domain: {
      name: 'Domínio',
      title: 'Um domínio próprio',
      text: 'Todo app começa em um endereço nosso. Quando ele pega, pode ir para um domínio seu: o mesmo app e o mesmo editor, em um endereço que as pessoas lembram. Os links do endereço antigo continuam funcionando e levam ao novo.',
      points: ['Para apps Premium: fale com a gente', 'O mesmo app, editor e registros', 'O endereço antigo encaminha os visitantes'],
      alt: 'O endereço do app saindo do endereço em genhttp.run para o domínio do próprio clube',
    },
    code: {
      name: 'Código e git',
      title: 'O código, quando você quiser',
      text: 'Mude o editor para a visão completa, e o código de cada versão está lá, com seus logs e seus números. Clone o app com git e faça push de uma mudança, ou baixe-o como um projeto .NET que roda em qualquer lugar. O código é seu.',
      points: ['Todos os arquivos, comparados de versão para versão', 'Git: um commit na main é uma versão, um branch é um rascunho', 'Um download que roda sozinho, com os registros incluídos'],
      alt: 'A visão completa do editor: os arquivos do app e o código dele',
    },
    publish: {
      name: 'Vitrine e código-fonte',
      title: 'Mostre o app ou publique o código',
      text: 'Coloque o app na vitrine para outras pessoas o encontrarem, ou publique o código dele sob uma licença de código aberto para qualquer um ler, favoritar e baixar. As duas coisas são escolha sua, e nenhuma compartilha seus registros.',
      points: ['Na vitrine, com uma imagem e uma descrição', 'Seu código sob MIT ou outra licença', 'Nunca seus registros, chaves ou visitantes'],
      alt: 'O código publicado da reserva de quadras: seus arquivos, sua documentação e suas versões, sob a licença MIT',
    },
  },

  draftBadge: 'Rascunho',
  domainOwn: 'Premium',
  domainSubject: 'Um domínio para o meu app',
  domainAsk: 'Perguntar sobre domínio',
  zoom: 'Ampliar',
  close: 'Fechar',

  closeTitle: 'Grátis para testar.',
  closeAccent: 'Sem cadastro.',
  closeBuild: 'Criar algo',
  closeShip: 'Publicar o que você criou',
  openSource: (selfHosting, github) => (
    <>
      O GenHTTP Lambda é código aberto. Use aqui ou {selfHosting('rode no seu próprio servidor')} - o código está no{' '}
      {github('GitHub')}.
    </>
  ),
};
