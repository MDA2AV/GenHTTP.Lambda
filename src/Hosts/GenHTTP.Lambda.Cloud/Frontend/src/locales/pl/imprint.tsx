import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Nota prawna',
  intro:
    'Kto prowadzi ten serwis. Niemieckie prawo wymaga, żeby każdy serwis prowadzony z Niemiec podawał te dane w jednym, łatwym do znalezienia miejscu (§ 5 DDG). To właśnie to miejsce.',
  sections: {
    providerTitle: 'Usługodawca',
    contactTitle: 'Kontakt',
    contact: (mail, abuse) => (
      <>
        E-mail: {mail}. Jeśli chcesz zgłosić lambdę, która wyrządza szkody, napisz na {abuse}.
      </>
    ),
    editorialTitle: 'Odpowiedzialność za treść',
    editorial:
      'Osoba odpowiedzialna za strony tego serwisu zgodnie z § 18 ust. 2 niemieckiej ustawy MStV (ale nie za hostowane tu lambdy, które piszą ich właściciele):',
    dsaTitle: 'Punkt kontaktowy zgodnie z aktem o usługach cyfrowych',
    dsa: (mail) => (
      <>
        Organy państwowe, Komisja Europejska i każdy, kto korzysta z tej usługi, mogą się z nami skontaktować pod adresem {mail}, po niemiecku lub po angielsku (art. 11 i 12 DSA).
      </>
    ),
  },
};
