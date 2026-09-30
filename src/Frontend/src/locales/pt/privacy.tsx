import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Política de privacidade',
  binding: (english) => (
    <>Esta tradução é apenas informativa. A política que vale é a {english('versão em inglês')}.</>
  ),
  intro:
    'O que este site fica sabendo sobre você, o que faz com isso, por quanto tempo guarda e quem mais pode ver. Resumindo: não existem contas, anúncios nem rastreamento. O servidor anota quem pediu o quê, para que ele continue funcionando e para que abusos possam ser investigados. E o que você pede ao agente de criação é enviado à Anthropic, cujo modelo escreve o app.',
  sections: {
    whoTitle: 'Quem é o responsável',
    who: 'Este site é mantido pela pessoa abaixo. Nos termos do Regulamento Geral sobre a Proteção de Dados da UE (RGPD), ela é a responsável pelo tratamento dos dados pessoais feito aqui. Para qualquer assunto desta página, escreva para este endereço:',

    requestsTitle: 'O que o servidor registra a cada requisição',
    requests: [
      'Toda requisição feita a este site, e a cada lambda hospedada nele, é gravada no log do servidor: o endereço IP de onde ela veio, o endereço que ela informa como origem quando passa por um intermediário, o navegador ou programa que a enviou, o endereço pedido, quando ela chegou e como foi respondida. O servidor também procura o país, a cidade e a rede a que o endereço IP pertence, em um banco de dados que ele mesmo mantém. Ninguém de fora é consultado.',
      'É assim que encontramos falhas, descobrimos o que está sobrecarregando o servidor e investigamos os abusos denunciados a nós. O endereço IP também é usado, só em memória, para limitar quantas requisições e quantos pedidos de criação um mesmo visitante pode fazer. Sem esses dados, não dá para responder a uma requisição. A base legal é o nosso legítimo interesse em manter o serviço funcionando e seguro (art. 6º, 1, f, do RGPD).',
      'Os administradores podem ver tudo isso. O dono de uma lambda vê o país e o navegador de cada requisição feita à lambda dele, mas não o endereço IP.',
    ],

    logsTitle: 'Por quanto tempo o log fica guardado',
    logs: 'O log fica em dois lugares: na memória do servidor, que é apagada toda vez que o servidor reinicia, e na saída de console do servidor, que é apagada toda vez que o servidor é atualizado. Os dois têm tamanho fixo, então cada linha nova empurra a mais antiga para fora. Por isso, quanto tempo uma linha dura depende do movimento do site. Nada do log é arquivado em outro lugar.',

    contentTitle: 'O que você coloca aqui',
    content: (days) =>
      `Uma lambda é formada pelo código, pelos arquivos, pelas configurações e pelas notas salvas com cada versão, que contam o que foi pedido e o que mudou. Tudo isso fica guardado no servidor para que ela possa rodar e ser editada. Uma lambda do plano grátis é removida, com todas as versões, cerca de ${days} dias depois da última mudança ou visita, e imediatamente se quem tem o link de edição excluir a lambda. Qualquer pessoa com o link de edição pode ler tudo isso. O que você coloca na vitrine fica visível para todo mundo. E os administradores olham uma lambda quando precisam, para tratar uma denúncia ou manter o servidor seguro. A base legal é a prestação do serviço que você pediu (art. 6º, 1, b, do RGPD).`,

    agentTitle: 'O que você pede ao agente de criação',
    agent: (policy) => (
      <>
        O que você digita na caixa de criação, ou na seção Mudar do editor de uma lambda, é enviado à Anthropic PBC, nos
        Estados Unidos, que opera o Claude, o modelo que escreve o app. Para fazer uma mudança, o agente também lê a
        lambda (o código, as notas das versões e o log, que contém as requisições e o que ela imprimiu, mas não os
        endereços IP dos visitantes), e o que ele lê também é enviado para lá. O que a Anthropic faz com isso está descrito
        na {policy('política de privacidade da própria empresa')}. Os Estados Unidos não protegem dados pessoais do
        mesmo jeito que a UE. Seu pedido é enviado para lá porque isso é necessário para criar ou mudar o que você pediu
        (art. 6º, 1, b, e art. 49, 1, b, do RGPD). Então não coloque nele nada que você não queira compartilhar.
      </>
    ),
    agentKept:
      'O agente salva o seu pedido, muitas vezes com outras palavras, como nota da versão que ele escreve. O pedido em si é gravado por inteiro no log do servidor, e as primeiras centenas de caracteres dele no log do serviço de criação; os dois também têm tamanho fixo. Se você usar o seu próprio agente, como o Claude ou o Claude Code, o que você diz a ele vai para o fornecedor desse agente, não para nós: só recebemos o código e as notas que ele envia para cá.',

    lambdasTitle: 'O que uma lambda faz depende do dono dela',
    lambdas:
      'Uma lambda é escrita por quem tem o link de edição dela, não por nós. O que ela pede aos visitantes e o que faz com isso é decisão dessa pessoa, e esta página não cobre isso, a não ser pelo log de requisições descrito acima, que o servidor mantém para todas as lambdas. Os termos de uso não permitem usar uma lambda para coletar dados pessoais de outras pessoas. Se você encontrar uma que faça isso, denuncie.',

    mailTitle: 'Quando você escreve para a gente',
    mail: 'Se você escrever para a gente, para denunciar um abuso ou sobre qualquer outro assunto, usamos o seu e-mail e a sua mensagem para responder e para resolver o que você escreveu. Depois apagamos tudo, quando não for mais necessário para isso (art. 6º, 1, f, do RGPD).',

    storageTitle: 'Cookies e o seu navegador',
    storage:
      'Existe um único cookie, chamado lang. Ele guarda o idioma que você escolheu, para que os endereços que não indicam idioma abram nele, e dura um ano. O armazenamento do próprio navegador guarda o tema claro ou escuro, algumas configurações das páginas que você usa e, no caso dos administradores, o token deles. Nada disso é usado para rastrear você, e nada vai para mais ninguém: não há ferramentas de análise, nem anúncios, e nada é carregado de outros sites, nem mesmo fontes. Como tudo isso só serve para fazer o que você pediu, não é preciso consentimento (§ 25(2) nº 2 da lei alemã TDDDG).',

    hostingTitle: 'Onde tudo fica guardado',
    hosting:
      'O servidor onde tudo isso roda é alugado de um provedor de hospedagem na União Europeia, e é lá que fica guardado o que esta página descreve.',

    rightsTitle: 'Seus direitos',
    rights: (mailbox) => (
      <>
        Você pode perguntar o que guardamos sobre você e pedir uma cópia, pedir que os dados sejam corrigidos ou
        apagados, ou que o uso deles seja limitado, e se opor a qualquer coisa que fazemos com base no nosso legítimo
        interesse (arts. 15 a 21 do RGPD). Escreva para {mailbox}. Como não existem contas, só conseguimos encontrar o
        que é seu se você disser como: o endereço IP que você usou e mais ou menos quando, ou o endereço da sua lambda.
        Nenhuma decisão que produza efeitos jurídicos para você, ou que afete você de forma igualmente importante, é
        tomada automaticamente (art. 22 do RGPD).
      </>
    ),
    complaint:
      'Você também pode fazer uma reclamação a uma autoridade de proteção de dados, onde você mora ou onde nós estamos. No nosso caso, é o Comissário para a Proteção de Dados e a Liberdade de Informação do estado alemão de Baden-Württemberg (LfDI Baden-Württemberg).',
  },
  change: 'Esta política muda quando o site muda. Vale a versão que está nesta página.',
  updated: 'Última alteração em 30 de setembro de 2026.',
};
