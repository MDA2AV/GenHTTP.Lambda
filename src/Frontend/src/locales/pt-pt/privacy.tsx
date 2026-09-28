import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Política de privacidade',
  binding: (english) => (
    <>Esta tradução é apenas informativa. Só a {english('versão em inglês')} desta política é vinculativa.</>
  ),
  intro:
    'O que este site fica a saber sobre ti, o que faz com isso, durante quanto tempo o guarda e quem mais o pode ver. Em resumo: não há contas, nem publicidade, nem rastreamento. O servidor regista quem lhe pediu o quê, para poder continuar a funcionar e para que os abusos possam ser investigados. E o que pedes ao agente de criação é enviado à Anthropic, cujo modelo escreve a app.',
  sections: {
    whoTitle: 'Quem é o responsável',
    who: 'Este site é gerido pela pessoa indicada abaixo. Nos termos do Regulamento Geral sobre a Proteção de Dados da UE (RGPD), é ela a responsável pelo tratamento de dados pessoais que aqui se faz. Escreve para este endereço sobre qualquer assunto desta página:',

    requestsTitle: 'O que o servidor regista em cada pedido',
    requests: [
      'Cada pedido feito a este site, e a cada lambda alojada nele, fica escrito no log do servidor: o endereço IP de onde veio, o endereço que indica como origem quando passa por um intermediário, o browser ou programa que o enviou, o endereço pedido, a data e a hora, e a resposta que recebeu. O servidor também procura o país, a cidade e a rede a que pertence o endereço IP, numa base de dados que ele próprio mantém. Não se pergunta nada a mais ninguém.',
      'É assim que se encontram falhas, que se descobre o que está a sobrecarregar o servidor e que se investigam os abusos que nos são denunciados. O endereço IP também é usado, só em memória, para limitar quantos pedidos um mesmo visitante pode fazer e quantas apps pode mandar criar. Sem estes dados, não é possível responder a um pedido. O fundamento jurídico é o nosso interesse legítimo em manter o serviço a funcionar e em segurança (art. 6.º, n.º 1, alínea f), do RGPD).',
      'Os administradores podem ver tudo isto. Quem é dono de uma lambda vê o país e o browser de cada pedido feito a essa lambda, mas não o endereço IP.',
    ],

    logsTitle: 'Durante quanto tempo o log é guardado',
    logs: 'O log fica em dois sítios: na memória do servidor, que é esvaziada sempre que o servidor reinicia, e na saída da consola do servidor, que é apagada sempre que o servidor é atualizado. Ambos têm um tamanho fixo, por isso cada linha nova empurra para fora a mais antiga, e o tempo que uma linha lá fica depende do movimento que o site tiver. Nada do log é copiado para um arquivo.',

    contentTitle: 'O que pões aqui',
    content: (days) =>
      `Uma lambda é o seu código, os seus ficheiros, as suas definições e as notas guardadas com cada versão sobre o que foi pedido e o que mudou. Tudo isto fica guardado no servidor para que a lambda possa correr e ser editada. Uma lambda do plano gratuito é removida, com todas as versões, cerca de ${days} dias depois da última alteração ou visita, e de imediato se quem tiver o link de edição a eliminar. Quem tiver o link de edição pode ler tudo isto, o que puseres na montra fica à vista de todos, e os administradores consultam uma lambda quando é preciso, para tratar de uma denúncia ou manter o servidor seguro. O fundamento jurídico é a prestação do serviço que pediste (art. 6.º, n.º 1, alínea b), do RGPD).`,

    agentTitle: 'O que pedes ao agente de criação',
    agent: (policy) => (
      <>
        O que escreves na caixa de criação é enviado à Anthropic PBC, nos Estados Unidos, a empresa por trás do Claude,
        o modelo que escreve a app. O que a Anthropic faz com isso rege-se pela{' '}
        {policy('sua própria política de privacidade')}. Os Estados Unidos não protegem os dados pessoais da mesma
        forma que a UE. O teu pedido é enviado para lá porque é necessário para criar o que pediste (art. 6.º, n.º 1,
        alínea b), e art. 49.º, n.º 1, alínea b), do RGPD), por isso não ponhas nele nada que não queiras partilhar.
      </>
    ),
    agentKept:
      'O agente guarda o teu pedido, muitas vezes por outras palavras, como nota da versão que escreve, e as primeiras centenas de caracteres vão para o log do serviço de criação, que também tem um tamanho fixo. Se usares o teu próprio agente, como o Claude ou o Claude Code, o que lhe disseres vai para o fornecedor desse agente, e não para nós: só recebemos o código e as notas que ele envia para cá.',

    lambdasTitle: 'Quem decide o que uma lambda faz é o dono dela',
    lambdas:
      'Uma lambda é escrita por quem tem o link de edição, não por nós. O que pede aos visitantes e o que faz com isso é decisão dessa pessoa, e esta página não trata disso, à exceção do log de pedidos descrito acima, que o servidor guarda para todas as lambdas. Os termos de utilização não permitem usar uma lambda para recolher dados pessoais de outras pessoas. Se encontrares uma que o faça, denuncia-a.',

    mailTitle: 'Quando nos escreves',
    mail: 'Se nos escreveres, para denunciar um abuso ou por qualquer outro motivo, usamos o teu endereço de email e a tua mensagem para te responder e tratar do assunto, e apagamo-los quando deixarem de ser necessários para isso (art. 6.º, n.º 1, alínea f), do RGPD).',

    storageTitle: 'Cookies e o teu browser',
    storage:
      'Há um único cookie, chamado lang. Guarda o idioma que escolheste, para que os endereços que não indicam idioma abram nesse idioma, e dura um ano. O armazenamento do próprio browser guarda o modo claro ou escuro, algumas definições das páginas que usas e, no caso dos administradores, o respetivo token. Nada disto é usado para te seguir, e nada vai para mais ninguém: não há ferramentas de análise, nem publicidade, e nada é carregado de outros sites, nem sequer os tipos de letra. Como tudo isto só serve para fazer o que pediste, não é necessário consentimento (§ 25(2), n.º 2, da lei alemã TDDDG).',

    hostingTitle: 'Onde fica guardado',
    hosting:
      'O servidor onde tudo isto corre é alugado a um fornecedor de alojamento na União Europeia, e é lá que fica guardado o que esta página descreve.',

    rightsTitle: 'Os teus direitos',
    rights: (mailbox) => (
      <>
        Podes perguntar o que guardamos sobre ti e pedir uma cópia, pedir que os dados sejam corrigidos ou apagados, ou
        que a sua utilização seja limitada, e opor-te a tudo o que fazemos com base no nosso interesse legítimo (artigos
        15.º a 21.º do RGPD). Escreve para {mailbox}. Como não há contas, só conseguimos encontrar o que é teu se nos
        disseres como: o endereço IP que usaste e mais ou menos quando, ou o endereço da tua lambda. Nenhuma decisão que
        produza efeitos jurídicos para ti, ou que te afete de forma igualmente significativa, é tomada de forma
        automática (artigo 22.º do RGPD).
      </>
    ),
    complaint:
      'Também podes apresentar uma reclamação a uma autoridade de proteção de dados, no sítio onde vives ou onde nós estamos. No nosso caso, é o Comissário para a Proteção de Dados e a Liberdade de Informação do estado federado alemão de Baden-Württemberg (LfDI Baden-Württemberg).',
  },
  change: 'Esta política muda quando o site muda. Aplica-se a versão que está nesta página.',
  updated: 'Última alteração a 28 de setembro de 2026.',
};
