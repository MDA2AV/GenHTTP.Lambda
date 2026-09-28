import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Termini di servizio',
  binding: (english) => (
    <>Questa traduzione è solo informativa. Fa fede la {english('versione inglese')}.</>
  ),
  intro:
    'Questo è un servizio gratuito per sperimentare. Esegue codice scritto da sconosciuti su un’infrastruttura condivisa, e funziona solo se tutti rispettano poche regole.',
  sections: {
    forbiddenTitle: 'Cosa non puoi pubblicare qui',
    forbidden: [
      'Niente malware, niente phishing, niente miner di criptovalute. Niente che attacchi, scansioni, sommerga di richieste o disturbi in altro modo altri sistemi, qui o altrove. Niente che molesti qualcuno. Niente che tu non abbia il diritto di pubblicare, compresi codice, testi, immagini e marchi di altri.',
      'Non usare una lambda per salvare o inoltrare dati personali di altre persone. Un indirizzo pubblico non ha niente di privato, e questa piattaforma non ti offre alcun modo per proteggere questi dati.',
    ],
    actionTitle: 'Cosa possiamo fare',
    action:
      'Tutto ciò che viene messo online qui può essere messo offline o eliminato in qualsiasi momento, senza preavviso e senza obbligo di spiegazioni. In pratica succede quando qualcosa viola le regole qui sopra, quando mette a rischio il server che tutti condividono, o quando qualcuno lo segnala e ha ragione.',
    lastingTitle: 'Quanto dura',
    lasting: (hours, days) =>
      `Un deployment resta raggiungibile per circa ${hours} ore. Una lambda che non apri viene eliminata, con tutte le versioni del suo codice, circa ${days} giorni dopo l’ultima volta che ci hai messo mano. Salvare o fare il deploy conta come metterci mano, quindi quello su cui stai lavorando resta. Niente qui è un backup: tieni una tua copia del codice che ti sta a cuore.`,
    keyTitle: 'Il link di modifica è la tua password',
    key: 'Chiunque abbia il link di modifica può leggere e modificare quella lambda, e dietro non c’è né un account né una password. Se pubblichi il link, dai a chiunque la possibilità di modificarla. Un link perso non si può recuperare.',
    warrantyTitle: 'Nessuna garanzia',
    warranty:
      'Il servizio è fornito così com’è, senza garanzia che funzioni, che continui a funzionare o che conservi quello che ci metti. Può essere riavviato, cambiato o spento in qualsiasi momento. Non costruirci sopra niente che sia importante per te o per altri.',
    reportTitle: 'Segnalazioni',
    report: (mailbox, front) => (
      <>
        Se una lambda ospitata qui fa qualcosa che non dovrebbe, scrivi a {mailbox} indicando il suo indirizzo. Cosa
        includere lo trovi nella {front('home page')}.
      </>
    ),
  },
  change: 'Questi termini possono cambiare. Vale la versione pubblicata in questa pagina.',

  short:
    'Le lambda girano su un’infrastruttura condivisa. Creandone una, accetti di non mettere online malware, pagine di phishing, miner di criptovalute o qualsiasi cosa che attacchi, scansioni o sommerga di richieste altri sistemi, e di non pubblicare contenuti che non hai il diritto di pubblicare. Chiunque conosca il link di modifica può modificare la tua lambda: trattalo come una password. Nel piano gratuito le lambda restano online finché vengono usate: una lambda che nessuno visita e nessuno modifica per un mese va offline, e viene eliminata se nei due mesi successivi non succede niente. Tutto ciò che pubblichi può essere eliminato in qualsiasi momento.',
};
