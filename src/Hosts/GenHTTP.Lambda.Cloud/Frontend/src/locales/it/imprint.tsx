import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Note legali',
  intro:
    'Chi gestisce questo sito. La legge tedesca chiede a ogni sito gestito dalla Germania di riportare questi dati in un unico punto facile da trovare (§ 5 DDG): eccolo.',
  sections: {
    providerTitle: 'Fornitore del servizio',
    contactTitle: 'Contatti',
    contact: (mail, abuse) => (
      <>
        E-mail: {mail}. Per segnalare una lambda che sta causando danni, scrivi a {abuse}.
      </>
    ),
    editorialTitle: 'Responsabile dei contenuti',
    editorial:
      'Responsabile delle pagine di questo sito ai sensi del § 18, comma 2 della legge tedesca MStV, ma non delle lambda ospitate qui, che scrivono i loro proprietari:',
    dsaTitle: 'Punto di contatto ai sensi del Regolamento sui servizi digitali',
    dsa: (mail) => (
      <>
        Le autorità, la Commissione europea e chiunque usi questo servizio possono scriverci a {mail}, in tedesco o in inglese (artt. 11 e 12 DSA).
      </>
    ),
  },
};
