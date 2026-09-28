import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Piattaforma di coding agentico',
  headline: 'Descrivi la tua app.',
  headlineAccent: 'Il tuo agente la mette online.',
  intro:
    'Sondaggi, guestbook, classifiche, piccoli negozi online. Spiega cosa ti serve al nostro agente o a quello che usi già. Ricevi un’app funzionante, con un link da condividere. E puoi continuare a migliorarla anche molto dopo la prima versione.',
  build: 'Crea qualcosa',
  ownAgent: 'Usa il tuo agente',
  free: 'Gratis. Niente account, niente da installare.',
  seeIt: 'Guarda come funziona',

  videoTitle: 'Da una frase a un’app online',
  videoText:
    'Una finestra in incognito, nessun account e una sola richiesta al nostro agente. Poi l’app finita, aperta dal suo link come la vedrebbe chiunque.',
  videoNote: 'Nel video la creazione è accelerata. Tutto il resto è in tempo reale.',
  tryIt: 'Prova anche tu',

  oneShotTitle: 'Non è usa e getta',
  oneShotText:
    'Quasi tutti i generatori ti danno un risultato e finisce lì. Qui l’app resta online dove è nata, così tu e il tuo agente potete continuare a lavorarci.',
  steps: [
    {
      title: 'Chiedi quello che vuoi',
      body: 'Descrivilo con parole tue, all’agente di questo sito o a quello che usi già. Niente codice, niente configurazione, niente account.',
      alt: 'La pagina di creazione con la richiesta di un sondaggio per il pranzo',
    },
    {
      title: 'Ricevi un’app e un link',
      body: 'L’app viene creata e messa online, con un indirizzo pubblico da condividere. Ricorda i suoi dati (voti, punteggi, messaggi), così chiunque la apra vede la stessa cosa.',
      alt: 'Il sondaggio per il pranzo, finito e aperto nel browser',
    },
    {
      title: 'Continua a migliorarla',
      body: 'Ogni app ha un link di modifica privato. Passalo al tuo agente con la prossima richiesta, oppure aprilo tu. Ogni modifica diventa una nuova versione e l’indirizzo non cambia.',
      alt: 'Il pannello di controllo del sondaggio: le versioni, ognuna con la richiesta, cosa ha cambiato e le differenze rispetto alla precedente',
    },
  ],
  weekLater: 'Una settimana dopo',
  weekAsk:
    'Ecco il link di modifica del mio sondaggio per il pranzo. Il venerdì chiudi le votazioni alle 11 e metti il vincitore in alto.',
  weekAnswer:
    'Fatto. La versione 4 è online allo stesso indirizzo, e la 3 è ancora lì se vuoi tornare indietro.',

  agentsTitle: 'Collega il tuo agente preferito',
  agentsText:
    'Lavori già con Claude o con un altro assistente? Collegalo a questo indirizzo: potrà creare, pubblicare e aggiornare app qui, direttamente dalla chat che hai già aperta.',
  agents: [
    {
      name: 'Claude sul web o su desktop',
      how: 'Apri Impostazioni, poi «Connectors» e scegli «Add custom connector». Incolla l’indirizzo qui sopra: niente chiave API, niente login.',
    },
    {
      name: 'Claude Code',
      how: 'Esegui una volta questo comando nel terminale:',
    },
    {
      name: 'Altri client MCP',
      how: 'Cursor, VS Code, Codex e gli altri client MCP supportano i server remoti. Configurali con lo stesso indirizzo.',
    },
  ],
  thenAsk: (em) => (
    <>Poi basta chiedere: {em('crea un modulo di iscrizione per l’evento del nostro team e mettilo online')}.</>
  ),

  contactTitle: 'Parliamone',
  contactText:
    'Ti serve aiuto, hai in mente un progetto più grande o cerchi una soluzione su misura? Ci farebbe piacere sentirti.',
  mailTitle: 'Scrivici un’email',
  mailText: 'Per progetti, richieste e tutto ciò di cui preferisci parlare in privato.',
  discordTitle: 'Entra nel nostro Discord',
  discordText: 'Mostra cosa hai creato, fatti aiutare con il prossimo passo e parla direttamente con il team.',
  discordLink: 'Il Discord di GenHTTP',
};
