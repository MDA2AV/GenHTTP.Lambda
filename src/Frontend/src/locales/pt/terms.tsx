import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Termos de uso',
  binding: (english) => (
    <>Esta tradução é apenas informativa. O texto que vale é a {english('versão em inglês')}.</>
  ),
  intro:
    'Este é um serviço grátis para experimentar. Ele roda código escrito por desconhecidos em uma infraestrutura compartilhada, e isso só funciona se todo mundo seguir algumas regras.',
  sections: {
    forbiddenTitle: 'O que você não pode colocar aqui',
    forbidden: [
      'Nada de malware, phishing ou mineradores de criptomoeda. Nada que ataque, escaneie, sobrecarregue ou interfira de qualquer outro jeito em outros sistemas, aqui ou em qualquer outro lugar. Nada que assedie alguém. Nada que você não tenha direito de publicar, e isso inclui código, textos, imagens e marcas de outras pessoas.',
      'Não use uma lambda para guardar ou repassar dados pessoais de outras pessoas. Um endereço público não tem nada de privado, e esta plataforma não oferece nenhum jeito de manter esses dados seguros.',
    ],
    actionTitle: 'O que podemos fazer a respeito',
    action:
      'Tudo o que for colocado no ar aqui pode ser tirado do ar ou removido a qualquer momento, sem aviso e sem obrigação de explicar. Na prática, isso acontece quando algo viola as regras acima, quando ameaça a máquina que todo mundo compartilha, ou quando alguém denuncia e tem razão.',
    lastingTitle: 'Quanto tempo as coisas duram',
    lasting: (hours, days) =>
      `Um deploy fica acessível por cerca de ${hours} horas. Uma lambda que você não abre é removida, com todas as versões do código, cerca de ${days} dias depois da última vez que você mexeu nela. Salvar ou fazer deploy conta como mexer, então tudo em que você está trabalhando continua aqui. Nada aqui é backup: guarde sua própria cópia do código que importa para você.`,
    keyTitle: 'Seu link de edição é a sua senha',
    key: 'Qualquer pessoa com o link de edição pode ler e mudar essa lambda, e não existe conta nem senha por trás dele. Se você publicar o link, publicou junto o poder de mudar a lambda. Não há como recuperar um link perdido.',
    warrantyTitle: 'Sem garantia',
    warranty:
      'O serviço é oferecido como está, sem garantia de que funcione, continue funcionando ou guarde o que você colocar nele. Ele pode ser reiniciado, alterado ou desligado a qualquer momento. Não crie nele nada que seja importante para você ou para outras pessoas.',
    reportTitle: 'Como denunciar',
    report: (mailbox, front) => (
      <>
        Se uma lambda hospedada aqui estiver fazendo algo que não deveria, escreva para {mailbox} com o endereço dela.
        Veja na {front('página inicial')} o que incluir.
      </>
    ),
  },
  change: 'Estes termos podem mudar. Vale a versão que está nesta página.',

  short:
    'As lambdas rodam em uma infraestrutura compartilhada. Ao criar uma, você concorda em não colocar no ar malware, páginas de phishing, mineradores de criptomoeda ou qualquer coisa que ataque, escaneie ou sobrecarregue outros sistemas, e em não publicar conteúdo que você não tem direito de publicar. Qualquer pessoa que conheça o link de edição pode mudar sua lambda, então trate esse link como uma senha. No plano grátis, as lambdas ficam no ar enquanto são usadas: uma lambda sem visitas e sem edições por um mês sai do ar, e é removida se nada acontecer nos dois meses seguintes. Qualquer coisa que você colocar no ar pode ser removida a qualquer momento.',
};
