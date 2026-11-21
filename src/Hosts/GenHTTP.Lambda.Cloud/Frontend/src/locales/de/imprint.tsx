import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Impressum',
  intro:
    'Wer diese Website betreibt. Das deutsche Recht verlangt von jeder Website, die aus Deutschland betrieben wird, diese Angaben an einer leicht auffindbaren Stelle (§ 5 DDG) – hier sind sie.',
  sections: {
    providerTitle: 'Angaben gemäß § 5 DDG',
    contactTitle: 'Kontakt',
    contact: (mail, abuse) => (
      <>
        E-Mail: {mail}. Um ein Lambda zu melden, das Schaden anrichtet, schreiben Sie an {abuse}.
      </>
    ),
    editorialTitle: 'Verantwortlich für den Inhalt',
    editorial:
      'Verantwortlich nach § 18 Abs. 2 MStV für die Seiten dieser Website – nicht für die hier gehosteten Lambdas, die ihre Besitzer selbst schreiben:',
    dsaTitle: 'Kontaktstelle nach dem Gesetz über digitale Dienste',
    dsa: (mail) => (
      <>
        Behörden, die EU-Kommission und alle, die diesen Dienst nutzen, erreichen uns unter {mail}, auf Deutsch oder Englisch (Art. 11 und 12 DSA).
      </>
    ),
  },
};
