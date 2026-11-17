import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hosting gratuito per il vibe coding',
  title: 'Da localhost allo schermo di tutti.',
  intro:
    'Hai creato un’app con Claude Code, Codex o Cursor, ma gira solo sul tuo computer. Chiedi al tuo agente di pubblicarla qui. In pochi minuti avrà un link pubblico che chiunque può aprire, un suo database e un collegamento in tempo reale con tutti quelli che la tengono aperta, così le persone possono giocare, chattare e postare insieme.',
  facts: ['Gratis', 'Senza registrazione', 'Senza carta di credito', 'Niente da installare'],

  stepsTitle: 'Metti online la tua web app in tre passi',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Collegalo una volta',
      body: 'Aggiungi un indirizzo, quello di un server MCP remoto, a Claude Code, Codex, Cursor o all’agente che usi. Ci vuole meno di un minuto e lo fai una volta sola.',
    },
    {
      title: 'Chiedigli di pubblicare',
      body: 'Digli di mettere online l’app qui. L’agente la impacchetta, la pubblica e controlla che risponda: niente repo GitHub, niente pipeline di deploy, niente Docker.',
    },
    {
      title: 'Condividi il link',
      body: 'Ricevi un indirizzo pubblico e un link di modifica privato. Il primo mandalo a chi vuoi. Il secondo tienilo per te: ti serve per modificare l’app in futuro.',
    },
  ],

  togetherTitle: 'Non solo hosting. Database e multiplayer inclusi.',
  together:
    'Quasi tutti gli hosting danno a ogni visitatore una copia della tua app, e ognuno gioca da solo: quello che un browser tiene nel localStorage, il successivo non lo vede mai. Qui ogni app ha il suo database e un collegamento in tempo reale con tutti quelli che la tengono aperta. La mossa di uno appare subito a tutti gli altri, e quello che postano resta lì anche il giorno dopo.',
  sayLike: 'Poi, nel tuo progetto, chiedi per esempio',
  asks: [
    'Pubblica questa app su GenHTTP Lambda e mandami il link.',
    'Rendi condivisi i punteggi, così tutti vedono la stessa classifica.',
  ],

  questionsTitle: 'Prima di pubblicare',
  questions: (offline, removed, showcase, terms, selfHosting) => [
    [
      'È davvero gratis?',
      <>
        Sì. Niente registrazione, niente carta di credito, niente periodo di prova. La tua app resta online finché la
        gente la usa. Dopo {offline} giorni senza nemmeno una visita o una modifica va offline, e dopo {removed} giorni
        viene eliminata.
      </>,
    ],
    [
      'Claude Code, Codex o Cursor possono pubblicare la mia app qui?',
      'Sì, e anche qualsiasi altro agente che sappia aggiungere un server MCP remoto. Collegalo una volta con l’indirizzo qui sopra, poi chiedigli di pubblicare: fa il deploy dell’app, controlla che risponda e ti manda il link.',
    ],
    [
      'Perché i miei amici non riescono ad aprire il mio link localhost?',
      'Perché localhost è il tuo computer: l’indirizzo funziona solo lì, e solo finché l’app è in esecuzione. Un tunnel le presta un indirizzo pubblico finché il tuo portatile resta acceso. Pubblicata qui, l’app gira sui nostri server, con un link che funziona anche a portatile chiuso.',
    ],
    [
      'Mi serve un server, un backend o Supabase?',
      'No. Ogni app ha il suo database, uno spazio per i file e un collegamento in tempo reale con tutti quelli che la tengono aperta. Nessun server da affittare, nessun secondo servizio da configurare, e niente da tenere acceso dalla tua parte.',
    ],
    [
      'Posso rendere multiplayer il mio gioco senza gestire un server?',
      'Sì. Quello che un browser tiene nel localStorage, il successivo non lo vede mai, quindi la parte condivisa deve stare su un server: qui è il nostro. Chiedi al tuo agente di rendere il gioco multiplayer, e ogni mossa arriva a tutti quelli che lo tengono aperto.',
    ],
    [
      'La mia app deve essere fatta in un certo modo?',
      'No, ci pensa il tuo agente. Pagine, immagini e stili vanno online così come sono, e quello che deve girare sul server l’agente lo adatta a questa piattaforma. Tu descrivi cosa deve fare l’app, al resto pensa lui.',
    ],
    [
      'Come la modifico dopo?',
      'Con il link di modifica che hai ricevuto quando l’hai pubblicata. Passalo al tuo agente con la prossima richiesta, oppure aprilo nel browser. Ogni modifica diventa una nuova versione allo stesso indirizzo, e puoi tornare a una versione precedente quando vuoi.',
    ],
    [
      'Posso fare push con git?',
      'Sì. Ogni app è anche un repository git: clonalo dall’indirizzo sotto Clona nell’editor, modificalo con i tuoi strumenti o il tuo agente e fai push. Ogni commit inviato a main diventa la versione successiva, e un branch che invii diventa una bozza con la sua anteprima.',
    ],
    [
      'Dove vanno le mie chiavi API?',
      'Non nel codice. Il tuo agente chiede una chiave per nome, e tu ne scrivi il valore nell’editor. Nessuno può rileggerlo: né l’editor, né l’agente.',
    ],
    [
      'Posso portarmi via il codice?',
      'Sì, è tuo. Scaricalo dall’editor quando vuoi, come progetto che funziona da solo, database incluso, oppure clonalo con git, con tutte le versioni.',
    ],
    [
      'Posso ospitarla invece sul mio server?',
      <>
        Sì. GenHTTP Lambda è open source, e la sua edizione Community è la stessa piattaforma in un solo container Docker,
        per il tuo server e il tuo agente. {selfHosting('Cloud o self-hosted')} spiega cosa cambia.
      </>,
    ],
    [
      'Chi può vedere la mia app?',
      <>
        Chiunque abbia il link. Non compare da nessuna parte, a meno che tu non decida di aggiungerla alla{' '}
        {showcase('vetrina')}.
      </>,
    ],
    [
      'C’è qualcosa che non posso pubblicare?',
      <>
        Poche cose, per esempio tutto ciò che danneggia o inganna le persone. I {terms('termini')} sono brevi e scritti in
        modo semplice.
      </>,
    ],
  ],
  noAgent: 'Niente agente? Creala qui',

};
