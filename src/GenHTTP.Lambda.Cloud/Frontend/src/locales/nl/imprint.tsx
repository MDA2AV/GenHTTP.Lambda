import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Colofon',
  intro:
    'Wie deze site beheert. De Duitse wet vraagt elke site die vanuit Duitsland wordt beheerd om deze gegevens op één makkelijk te vinden plek te zetten (§ 5 DDG). Dit is die plek.',
  sections: {
    providerTitle: 'Aanbieder',
    contactTitle: 'Contact',
    contact: (mail, abuse) => (
      <>
        E-mail: {mail}. Wil je een lambda melden die schade aanricht? Schrijf dan naar {abuse}.
      </>
    ),
    editorialTitle: 'Verantwoordelijk voor de inhoud',
    editorial:
      "Volgens § 18 lid 2 van de Duitse MStV verantwoordelijk voor de pagina's van deze site, niet voor de lambda's die hier gehost worden en die hun eigenaars zelf schrijven:",
    dsaTitle: 'Contactpunt volgens de digitaledienstenverordening',
    dsa: (mail) => (
      <>
        Autoriteiten, de Europese Commissie en iedereen die deze dienst gebruikt, kunnen ons bereiken via {mail}, in het Duits of Engels (art. 11 en 12 DSA).
      </>
    ),
  },
};
