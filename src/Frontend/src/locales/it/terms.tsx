import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Termini di servizio',
  binding: (english) => (
    <>Questa traduzione è fornita a solo scopo informativo. Fa fede esclusivamente la {english('versione inglese')}.</>
  ),
  intro:
    'Questo è un servizio gratuito a scopo di sperimentazione. Esegue codice scritto da terzi su un’infrastruttura condivisa, il che è possibile solo se tutti rispettano alcune regole.',
  sections: {
    forbiddenTitle: 'Contenuti non consentiti',
    forbidden: [
      'Nessun malware, nessun phishing, nessun miner di criptovalute. Nulla che attacchi, scansioni, sovraccarichi o interferisca in altro modo con altri sistemi, qui o altrove. Nulla che molesti altre persone. Nulla che non si abbia il diritto di pubblicare, inclusi codice, testi, immagini e marchi di terzi.',
      'Non utilizzi un lambda per memorizzare o inoltrare dati personali di altre persone. Un indirizzo pubblico non ha nulla di privato, e questa piattaforma non offre alcuno strumento per proteggere tali dati.',
    ],
    actionTitle: 'Provvedimenti',
    action:
      'Qualsiasi contenuto distribuito qui può essere disattivato o rimosso in qualsiasi momento, senza preavviso e senza obbligo di motivazione. In pratica ciò avviene in caso di violazione delle regole di cui sopra, di rischio per il server condiviso o di segnalazione fondata.',
    lastingTitle: 'Durata di conservazione',
    lasting: (hours, days) =>
      `Una distribuzione resta raggiungibile per circa ${hours} ore. Un lambda non aperto viene rimosso, insieme a tutte le versioni del codice, circa ${days} giorni dopo l’ultima modifica. Il salvataggio e la distribuzione valgono come modifica: ciò su cui si sta lavorando viene quindi conservato. Questo servizio non costituisce un backup: conservi una copia personale del codice importante.`,
    keyTitle: 'Il link di modifica equivale a una password',
    key: 'Chiunque disponga del link di modifica può leggere e modificare il relativo lambda; non sono associati account né password. Pubblicare il link significa consentire ad altri di modificarlo. Un link smarrito non può essere recuperato.',
    warrantyTitle: 'Nessuna garanzia',
    warranty:
      'Il servizio è fornito così com’è, senza alcuna garanzia di funzionamento, continuità o conservazione dei contenuti. Può essere riavviato, modificato o interrotto in qualsiasi momento. Non vi si basi nulla di importante per Lei o per terzi.',
    reportTitle: 'Segnalazioni',
    report: (mailbox, front) => (
      <>
        Se un lambda ospitato qui ha un comportamento scorretto, scriva a {mailbox} indicandone l’indirizzo. Le informazioni
        utili da includere sono indicate nella {front('pagina iniziale')}.
      </>
    ),
  },
  change: 'Questi termini possono cambiare. Si applica la versione pubblicata in questa pagina.',

  short:
    'I lambda vengono eseguiti su un’infrastruttura condivisa. Creandone uno, Lei si impegna a non distribuire malware, pagine di phishing, miner di criptovalute o qualsiasi cosa che attacchi, scansioni o sovraccarichi altri sistemi, e a non pubblicare contenuti su cui non detiene i diritti. Chiunque conosca il link di modifica può modificare il Suo lambda: lo tratti come una password. I lambda del piano gratuito restano online finché vengono utilizzati: un lambda senza visite né modifiche per un mese viene disattivato e rimosso se non vi è alcuna attività nei due mesi successivi. Qualsiasi contenuto distribuito può essere rimosso in qualsiasi momento.',
};
