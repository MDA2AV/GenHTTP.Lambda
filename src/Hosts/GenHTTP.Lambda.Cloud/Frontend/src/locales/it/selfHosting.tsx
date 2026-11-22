import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Self-hosting',

  eyebrow: 'Open source · cloud o self-hosted',
  title: 'Usala qui,',
  titleAccent: 'oppure in self-hosting.',
  intro:
    'GenHTTP Lambda è open source con licenza MIT. genhttp.dev la fa girare per chiunque voglia usarla, e l’edizione Community è la stessa piattaforma come immagine Docker, per chi preferisce gestirla da sé. Entrambe creano e ospitano le stesse app; cambia dove vivono e chi se ne occupa.',

  cloud: {
    kicker: 'Cloud',
    name: 'genhttp.dev',
    summary: 'Apri questo sito e inizia. Del server, dei suoi aggiornamenti e dei suoi limiti ci occupiamo noi.',
    points: ['Niente da installare o far girare', 'Le app rispondono a un indirizzo su genhttp.run', 'Agente di creazione incluso'],
    action: 'Crea qualcosa',
  },
  community: {
    kicker: 'Self-hosted',
    name: 'Edizione Community',
    summary: 'Un container Docker su una macchina tua. Del server, dei suoi aggiornamenti e dei suoi limiti ti occupi tu.',
    points: ['Un container, un volume, nessun server di database', 'Le app rispondono al tuo indirizzo', 'L’agente di creazione come container aggiuntivo'],
    action: 'Installala tu',
  },

  sameTitle: 'Uguali in entrambe',
  sameText: 'L’edizione Community non è una versione ridotta. Ogni app ha le stesse cose ovunque giri:',
  same: [
    'L’agente di creazione',
    'Il tuo agente tramite MCP',
    'L’API REST',
    'L’editor, semplice e completo',
    'Bozze con dati di prova',
    'Versioni e la strada per tornare indietro',
    'Voci, file, chiavi e password',
    'Analytics senza cookie',
    'Domini propri',
    'Git e download',
    'La vetrina e il codice pubblicato',
  ],

  differTitle: 'Cosa cambia',
  aspect: 'Domanda',
  rows: (offline) => [
    ['Dove girano le app', 'Sul nostro server', 'Sul tuo, in un container con un volume'],
    ['Come iniziare', 'Apri la pagina di creazione, senza account', 'Avvia l’immagine, poi apri il suo indirizzo'],
    ['Chi gestisce il server', 'Noi', 'Tu'],
    ['Quanto costa', 'Niente per provare; la produzione per le non profit su richiesta, per le aziende previo accordo', 'Niente; il server è a tue spese'],
    ['Dove risponde un’app', 'A un sottodominio di genhttp.run', 'Sotto il tuo indirizzo, o su sottodomini di un dominio che indichi tu'],
    ['Un dominio proprio', 'Per le app in produzione, registrato da te', 'Per qualsiasi app che sposti in produzione'],
    ['App che nessuno usa', `Offline dopo ${offline} giorni senza una visita o una modifica, tranne quelle in produzione`, 'Come lo imposti tu'],
    ['Limiti e regole', 'Nostri, e le nostre condizioni di servizio', 'Tuoi, impostati nel pannello di amministrazione'],
    ['L’agente di creazione', 'Incluso, con un certo numero di creazioni al giorno', 'Facoltativo, con il tuo abbonamento Claude o la tua chiave API'],
    ['Report di utilizzo', 'Nessuno', 'Una volta al giorno: la versione, la macchina, le opzioni e i conteggi sulle app, mai un’app o una persona. Puoi disattivarlo.'],
  ],

  moveTitle: 'Passare dall’una all’altra',
  move:
    'Le stesse app girano su entrambe, e il tuo agente ne sposta una con git: crea l’app sull’altra e ci copia i file di quella vecchia e fa push. Un download è un progetto .NET che gira ovunque, con le sue voci in un file SQLite.',

  pickTitle: 'Quale fa per te',
  pickCloud: {
    title: 'genhttp.dev, se',
    points: [
      'vuoi un’app online senza far girare nulla',
      'le persone che la usano sono ovunque',
      'preferisci non occuparti di un server',
    ],
  },
  pickCommunity: {
    title: 'L’edizione Community, se',
    points: [
      'le tue app e le loro voci devono restare su una macchina tua',
      'sono strumenti per il tuo team, dentro la tua rete',
      'vuoi impostare tu i limiti e le regole',
    ],
  },

  startTitle: 'Per iniziare',
  startCloud: {
    title: 'Su genhttp.dev',
    text: 'Descrivi cosa ti serve nella pagina di creazione, oppure collega l’agente che usi già. Non c’è nessun account da creare.',
  },
  startCommunity: {
    title: 'Sul tuo server',
    text: 'Un comando lo avvia. Apri il suo indirizzo, collega il tuo agente e crea: il resto lo dice il README.',
  },
  connect: 'Collega il tuo agente',
  readme: 'Leggi il README',
  dockerHub: 'L’immagine su Docker Hub',

  sourceTitle: 'Open source',
  source: (github) => (
    <>
      GenHTTP Lambda è rilasciata con licenza MIT. Il suo codice, le sue issue e le sue release sono su {github('GitHub')}.
    </>
  ),
};
