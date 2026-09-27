import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Termos de serviço',
  binding: (english) => (
    <>Esta tradução é fornecida apenas para fins informativos. A versão vinculante é a {english('versão em inglês')}.</>
  ),
  intro:
    'Este é um serviço gratuito para experimentação. Ele executa código escrito por terceiros em uma infraestrutura compartilhada, o que só é viável se todos respeitarem algumas regras.',
  sections: {
    forbiddenTitle: 'O que não é permitido',
    forbidden: [
      'Nada de malware, phishing ou mineradores de criptomoedas. Nada que ataque, escaneie, sobrecarregue ou interfira de outra forma em outros sistemas, aqui ou em qualquer outro lugar. Nada que assedie alguém. Nada que você não tenha o direito de publicar – o que inclui código, textos, imagens e marcas de terceiros.',
      'Não utilize um lambda para armazenar ou encaminhar dados pessoais de outras pessoas. Um endereço público não tem nada de privado, e esta plataforma não oferece meios de proteger esses dados.',
    ],
    actionTitle: 'Medidas que podemos adotar',
    action:
      'Qualquer conteúdo implantado aqui pode ser retirado do ar ou removido a qualquer momento, sem aviso prévio e sem obrigação de justificativa. Na prática, isso ocorre quando algo viola as regras acima, ameaça o servidor compartilhado ou é denunciado com fundamento.',
    lastingTitle: 'Por quanto tempo o conteúdo permanece',
    lasting: (hours, days) =>
      `Uma implantação permanece acessível por cerca de ${hours} horas. Um lambda que você não abriu é removido, com todas as versões do seu código, cerca de ${days} dias após sua última alteração. Salvar ou implantar conta como alteração, portanto tudo em que você estiver trabalhando é mantido. Este serviço não é um backup: mantenha sua própria cópia de todo código importante.`,
    keyTitle: 'Seu link de edição é sua senha',
    key: 'Qualquer pessoa que tenha o link de edição pode ler e alterar o lambda correspondente, e não há conta nem senha associada. Publicar o link equivale a permitir que outros o alterem. Um link perdido não pode ser recuperado.',
    warrantyTitle: 'Sem garantia',
    warranty:
      'O serviço é fornecido no estado em que se encontra, sem garantia de funcionamento, de continuidade ou de preservação do que você armazenar nele. Ele pode ser reiniciado, alterado ou desativado a qualquer momento. Não utilize o serviço para nada que seja importante para você ou para terceiros.',
    reportTitle: 'Denúncias',
    report: (mailbox, front) => (
      <>
        Se um lambda hospedado aqui estiver fazendo algo indevido, escreva para {mailbox} informando o endereço dele. Veja
        na {front('página inicial')} quais informações incluir.
      </>
    ),
  },
  change: 'Estes termos podem mudar. A versão aplicável é a publicada nesta página.',

  short:
    'Os lambdas são executados em uma infraestrutura compartilhada. Ao criar um, você concorda em não implantar malware, páginas de phishing, mineradores de criptomoedas ou qualquer coisa que ataque, escaneie ou sobrecarregue outros sistemas, e em não publicar conteúdo sobre o qual não tenha direitos. Qualquer pessoa que conheça o link de edição pode alterar seu lambda, portanto trate-o como uma senha. Os lambdas do plano gratuito permanecem no ar enquanto são utilizados: um lambda sem acessos nem alterações por um mês é retirado do ar e removido se não houver atividade nos dois meses seguintes. Qualquer conteúdo implantado pode ser removido a qualquer momento.',
};
