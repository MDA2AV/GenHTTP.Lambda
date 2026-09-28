import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Aviso legal',
  intro:
    'Quem gere este site. A lei alemã exige que qualquer site gerido a partir da Alemanha apresente estes dados num só local, fácil de encontrar (§ 5 DDG). É aqui.',
  sections: {
    providerTitle: 'Prestador do serviço',
    contactTitle: 'Contacto',
    contact: (mail, abuse) => (
      <>
        E-mail: {mail}. Para denunciar uma lambda que esteja a causar danos, escreve para {abuse}.
      </>
    ),
    editorialTitle: 'Responsável pelo conteúdo',
    editorial:
      'Responsável pelas páginas deste site, nos termos do § 18(2) da lei alemã MStV, mas não pelas lambdas aqui alojadas, que são escritas pelos próprios donos:',
    dsaTitle: 'Ponto de contacto ao abrigo do Regulamento dos Serviços Digitais',
    dsa: (mail) => (
      <>
        As autoridades, a Comissão Europeia e qualquer pessoa que use este serviço podem contactar-nos através de {mail}, em alemão ou inglês (artigos 11.º e 12.º do DSA).
      </>
    ),
  },
};
