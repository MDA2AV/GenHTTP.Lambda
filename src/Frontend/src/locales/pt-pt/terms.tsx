import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Termos de utilização',
  binding: (english) => (
    <>Esta tradução é apenas informativa. Só a {english('versão em inglês')} é vinculativa.</>
  ),
  intro:
    'Este é um serviço gratuito para experimentar coisas. Corre código escrito por desconhecidos numa infraestrutura partilhada, e isso só funciona se toda a gente cumprir algumas regras.',
  sections: {
    forbiddenTitle: 'O que não podes pôr aqui',
    forbidden: [
      'Nada de malware, phishing ou mineradores de criptomoedas. Nada que ataque, faça scans, sobrecarregue ou interfira de qualquer outra forma com outros sistemas, aqui ou noutro sítio qualquer. Nada que assedie alguém. Nada que não tenhas o direito de publicar, o que inclui código, textos, imagens e marcas de outras pessoas.',
      'Não uses uma lambda para guardar ou reencaminhar dados pessoais de outras pessoas. Um endereço público não tem nada de privado, e esta plataforma não te dá forma nenhuma de manter esses dados seguros.',
    ],
    actionTitle: 'O que podemos fazer',
    action:
      'Tudo o que for publicado aqui pode ser posto offline ou removido a qualquer momento, sem aviso e sem obrigação de explicar porquê. Na prática, isso acontece quando algo viola as regras acima, quando ameaça a máquina que toda a gente partilha, ou quando alguém o denuncia e tem razão.',
    lastingTitle: 'Quanto tempo as coisas duram',
    lasting: (hours, days) =>
      `Um deploy fica acessível durante cerca de ${hours} horas. Uma lambda que não abriste é removida, com todas as versões do código, cerca de ${days} dias depois da última vez que lhe mexeste. Guardar ou fazer deploy conta como mexer, por isso aquilo em que estás a trabalhar fica. Nada aqui é uma cópia de segurança: guarda a tua própria cópia do código que te importa.`,
    keyTitle: 'O teu link de edição é a tua palavra-passe',
    key: 'Quem tiver o link de edição pode ler e alterar essa lambda, e não há conta nem palavra-passe por trás dele. Se publicares o link, publicaste também a possibilidade de a alterar. Um link perdido não pode ser recuperado.',
    warrantyTitle: 'Sem garantia',
    warranty:
      'O serviço é fornecido tal como está, sem garantia de que funcione, de que continue a funcionar ou de que guarde o que lá puseres. Pode ser reiniciado, alterado ou desligado a qualquer momento. Não construas nele nada que seja importante para ti ou para outra pessoa.',
    reportTitle: 'Denunciar alguma coisa',
    report: (mailbox, front) => (
      <>
        Se uma lambda alojada aqui estiver a fazer algo que não devia, escreve para {mailbox} com o endereço dela. Vê na{' '}
        {front('página inicial')} o que deves incluir.
      </>
    ),
  },
  change: 'Estes termos podem mudar. Aplica-se a versão que está nesta página.',

  short:
    'As lambdas correm numa infraestrutura partilhada. Ao criar uma, aceitas não pôr online malware, páginas de phishing, mineradores de criptomoedas nem nada que ataque, faça scans ou sobrecarregue outros sistemas, e não publicar conteúdos que não tenhas o direito de publicar. Quem souber o link de edição pode alterar a tua lambda, por isso trata-o como uma palavra-passe. No plano gratuito, as lambdas ficam online enquanto forem usadas: uma lambda que ninguém visite nem edite durante um mês fica offline, e é removida se nada acontecer nos dois meses seguintes. Tudo o que puseres online pode ser removido a qualquer momento.',
};
