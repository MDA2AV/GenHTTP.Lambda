import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Aviso legal',
  intro:
    'Quem administra este site. A lei alemã exige que todo site administrado a partir da Alemanha traga estes dados em um só lugar, fácil de achar (§ 5 DDG). É aqui.',
  sections: {
    providerTitle: 'Provedor do serviço',
    contactTitle: 'Contato',
    contact: (mail, abuse) => (
      <>
        E-mail: {mail}. Para denunciar uma lambda que esteja causando danos, escreva para {abuse}.
      </>
    ),
    editorialTitle: 'Responsável pelo conteúdo',
    editorial:
      'Responsável pelas páginas deste site, segundo o § 18(2) da lei alemã MStV, mas não pelas lambdas hospedadas aqui, que são escritas pelos próprios donos:',
    dsaTitle: 'Ponto de contato segundo o Regulamento dos Serviços Digitais',
    dsa: (mail) => (
      <>
        Autoridades, a Comissão Europeia e qualquer pessoa que use este serviço podem nos contatar em {mail}, em alemão ou inglês (arts. 11 e 12 do DSA).
      </>
    ),
  },
};
