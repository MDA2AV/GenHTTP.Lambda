import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Una piattaforma di coding agentico',
  headline: 'Descriva un’applicazione.',
  headlineAccent: 'Il Suo agente la mette online.',
  intro:
    'Sondaggi, guestbook, classifiche, piccoli negozi: descriva ciò che Le serve al nostro agente o a quello che utilizza già, e riceverà un’applicazione funzionante con un link da condividere. L’applicazione resta modificabile e può essere perfezionata anche molto tempo dopo la prima versione.',
  build: 'Crea un’applicazione',
  ownAgent: 'Utilizza il Suo agente',
  free: 'Gratuito. Senza account, senza installazioni.',
  seeIt: 'Guarda una dimostrazione',

  videoTitle: 'Da una frase a un’applicazione online',
  videoText:
    'Una finestra di navigazione privata, nessun account e un’unica richiesta nella pagina Crea – quindi l’applicazione completata, aperta dal suo link come la vedrà qualsiasi visitatore.',
  videoNote: 'La creazione è mostrata a velocità accelerata. Tutto il resto in tempo reale.',
  tryIt: 'Provi ora',

  oneShotTitle: 'Non un risultato isolato',
  oneShotText:
    'La maggior parte dei generatori consegna un risultato e si ferma lì. Qui l’applicazione continua a funzionare dove è stata creata, ed è possibile continuare a svilupparla insieme al proprio agente.',
  steps: [
    {
      title: 'Descriva ciò che Le serve',
      body: 'Lo descriva con parole Sue, all’agente di questo sito o a quello che utilizza già. Senza codice, senza configurazione e senza account.',
      alt: 'La pagina Crea con una richiesta per un sondaggio sul pranzo',
    },
    {
      title: 'Un’applicazione funzionante e un link',
      body: 'L’applicazione viene creata, distribuita e restituita come indirizzo pubblico da condividere. Conserva i propri dati – voti, punteggi, messaggi – così che tutti vedano lo stesso stato.',
      alt: 'Il sondaggio completato, aperto in un browser',
    },
    {
      title: 'Miglioramento continuo',
      body: 'Ogni applicazione dispone di un link di modifica privato. Lo consegni al Suo agente insieme alla modifica successiva, oppure lo apra direttamente. Ogni modifica diventa una nuova versione e l’indirizzo resta invariato.',
      alt: 'Il centro di controllo del sondaggio: le sue versioni, ciascuna con la richiesta, la modifica e la differenza rispetto alla precedente',
    },
  ],
  weekLater: 'Una settimana dopo',
  weekAsk:
    'Ecco il link di modifica del mio sondaggio. Chiudi la votazione alle 11 il venerdì e mostra il risultato in alto.',
  weekAnswer:
    'Fatto. La versione 4 è online allo stesso indirizzo e la versione 3 resta disponibile in caso di ripristino.',

  agentsTitle: 'Utilizzi l’agente che preferisce',
  agentsText:
    'Lavora già con Claude o con un altro assistente? Lo colleghi a questo indirizzo: potrà creare, distribuire e aggiornare applicazioni qui, direttamente dalla conversazione già aperta.',
  agents: [
    {
      name: 'Claude sul web o su desktop',
      how: 'Apra le Impostazioni, quindi «Connectors», e selezioni «Add custom connector». Incolli l’indirizzo qui sopra: non sono richiesti chiavi API né accesso.',
    },
    {
      name: 'Claude Code',
      how: 'Esegua una volta in un terminale:',
    },
    {
      name: 'Altri client MCP',
      how: 'Cursor, VS Code, Codex e altri client MCP supportano i server remoti. Li configuri con lo stesso indirizzo.',
    },
  ],
  thenAsk: (em) => (
    <>A quel punto basta una richiesta come: {em('crea un foglio di iscrizione per il nostro evento aziendale e pubblicalo')}.</>
  ),

  contactTitle: 'Contatti',
  contactText:
    'Ha bisogno di assistenza, sta pianificando un progetto più ampio o cerca una soluzione su misura? Saremo lieti di ricevere la Sua richiesta.',
  mailTitle: 'Via e-mail',
  mailText: 'Per progetti, richieste e qualsiasi argomento che preferisca trattare in modo riservato.',
  discordTitle: 'Il nostro Discord',
  discordText: 'Presenti i Suoi progetti, riceva supporto e si confronti direttamente con il team.',
  discordLink: 'Il Discord di GenHTTP',

  terms: 'Termini di servizio',
  writeCode: 'Scrivere il codice autonomamente',
  contact: 'Contatti',
};
