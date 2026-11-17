import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'L’AI app builder con hosting incluso',
  title: 'Crearla richiede un minuto.',
  titleAccent: 'Il resto è già incluso.',
  intro:
    'Descrivi un’app al nostro agente IA - o a Claude Code, Codex o Cursor - e in pochi minuti è online, ospitata per te, con un link da condividere. Tutto quello che la rende utile c’è già: le sue voci, le sue versioni e a cosa serve, chi la visita e un dominio tutto suo quando inizia a funzionare.',
  build: 'Crea qualcosa',
  ownAgent: 'Usa il tuo agente',
  free: 'Gratis. Senza registrazione, senza carta di credito.',
  heroAlt: 'L’editor di un’app per prenotare i campi: online sul dominio del circolo, quante persone l’hanno usata oggi e la sua ultima modifica',
  example: 'Tutte le immagini di questa pagina mostrano la stessa app di esempio: la prenotazione dei campi di un circolo di tennis.',

  contents: 'In questa pagina',

  makeTitle: 'Creala, modificala',
  makeText: 'Di’ cosa ti serve, e continua a dirlo. Per nessuna delle due cose serve scrivere o leggere codice.',

  behindTitle: 'Database, cronologia delle versioni e analytics per ogni app',
  behindText:
    'Quello che rende utile un’app è tutto ciò che sta intorno al suo codice: c’è per chi l’ha creata, non solo per chi sa leggere il codice.',

  yoursTitle: 'Tua, per andare oltre',
  yoursText: 'Per gli sviluppatori e per chiunque voglia che la propria app sopravviva a questo sito.',

  features: {
    build: {
      name: 'Crea',
      title: 'Descrivila, ed è online',
      text: 'Racconta al nostro agente cosa ti serve, con parole tue, oppure chiedilo a Claude Code, Codex o Cursor. Pochi minuti dopo l’app gira a un indirizzo tutto suo, pronta da condividere. Non c’è nessun server, database o account da configurare.',
      points: ['Il nostro agente nella pagina di creazione, o il tuo tramite MCP', 'Un link da condividere in pochi minuti', 'Gratis, senza registrazione'],
      alt: 'La pagina di prenotazione dei campi: quattro campi, ora per ora, con le ore prenotate indicate per nome',
    },
    change: {
      name: 'Modifica',
      title: 'Fai domande, modifica',
      text: 'Le tue app restano elencate nella pagina di creazione, ciascuna con una conversazione. Chiedi come sta andando - chi l’ha visitata, cosa hanno inserito le persone, se qualcosa non va - oppure di’ cosa deve cambiare, e l’agente risponde o la modifica. Oppure passa il suo link di modifica al tuo agente.',
      points: ['Risposte su visitatori, voci ed errori', 'Online da sola quando funziona, oppure tenuta come bozza perché tu la provi prima', 'Il codice resta nascosto, a meno che tu non voglia vederlo'],
      alt: 'La pagina di creazione: il circolo ha chiesto quanti campi sono stati prenotati questa settimana, l’agente ha risposto e le lezioni dell’allenatore sono pronte da provare in una bozza',
    },
    drafts: {
      name: 'Bozze',
      title: 'Prova una modifica prima che la veda qualcuno',
      text: 'Una modifica più grande si fa in una bozza: l’app come sarà, a un indirizzo tutto suo, con una copia delle voci su cui provarla. Niente di quello che fai lì tocca quelle vere. Quando è tutto a posto, un clic la mette online.',
      points: ['Un indirizzo tutto suo, per provarla e mostrarla ad altri', 'Dati di prova: una copia delle voci, non le voci vere', 'Mettila online quando è giusta, oppure scartala'],
      alt: 'Una bozza della pagina di prenotazione, in cui le lezioni dell’allenatore sono mostrate in viola, in una copia delle prenotazioni',
    },
    history: {
      name: 'Cronologia',
      title: 'Ogni modifica è una versione, con la strada per tornare indietro',
      text: 'Ogni modifica diventa una nuova versione, descritta da ciò che ha cambiato. Se una va storta, la versione precedente è a un clic: allo stesso indirizzo, con le stesse voci.',
      points: ['Cosa è stato chiesto e cosa è cambiato', 'Si torna a una versione precedente con un clic', 'Le voci sono condivise da tutte le versioni, quindi tornare indietro non ne fa perdere nessuna'],
      alt: 'La cronologia dell’app: le sue modifiche, una sotto l’altra, con la più recente online',
    },
    about: {
      name: 'Informazioni',
      title: 'Sa cos’è',
      text: 'A ogni modifica l’agente annota a cosa serve l’app, chi la usa e perché funziona così. Tu lo leggi in parole semplici, e l’agente successivo lo legge prima di cambiare qualsiasi cosa: così una modifica si appoggia su ciò che c’è, invece di tirare a indovinare.',
      points: ['Cosa fa e per chi, in parole semplici', 'Le decisioni che ci sono dietro, per chi la modificherà dopo', 'Come viene provata, conservato con ogni versione'],
      alt: 'Info sulla tua app: la prenotazione dei campi descritta in parole semplici, chi la usa e cosa fa',
    },
    records: {
      name: 'Voci',
      title: 'Le voci in un database tutto suo',
      text: 'Un’app che conserva dei dati - prenotazioni, iscrizioni, punteggi - li tiene in un database tutto suo, e restano anche quando l’app cambia. Le leggi tabella per tabella nell’editor, accanto ai file che ha salvato, come l’elenco settimanale per la bacheca.',
      points: ['Un database per ogni app che ne ha bisogno', 'Ogni tabella, leggibile nell’editor', 'I file salvati dall’app, elencati accanto'],
      alt: 'Le voci dell’app: le sue prenotazioni e i suoi soci, lette tabella per tabella',
    },
    keys: {
      name: 'Chiavi e password',
      title: 'Chiavi e password restano segrete',
      text: 'Quando un’app ha bisogno di una password o della chiave di un altro servizio, l’agente la chiede solo per nome. Il valore lo scrivi tu nell’editor, e da quel momento nessuno lo vede: né tu, né l’agente. Solo l’app lo legge.',
      points: ['L’agente non vede mai il valore', 'Conservato cifrato, non viene mai più mostrato', 'Lo sostituisci quando vuoi'],
      alt: 'Chiavi e password: la password della reception è impostata e il suo valore è nascosto',
    },
    analytics: {
      name: 'Analytics',
      title: 'Scopri chi la visita, senza cookie',
      text: 'Attiva Analytics e scopri quante persone hanno aperto l’app, da dove sono arrivate e con quali dispositivi: tutto contato sul nostro server, senza cookie e senza script nelle tue pagine.',
      points: ['Visitatori, visualizzazioni e provenienza', 'Niente cookie e niente che identifichi un visitatore', 'Spento finché non lo attivi'],
      alt: 'Analytics: i visitatori degli ultimi trenta giorni, giorno per giorno',
    },
    domain: {
      name: 'Dominio',
      title: 'Un dominio tutto suo',
      text: 'Ogni app parte da un nostro indirizzo. Quando prende piede, può passare a un dominio tuo: la stessa app e lo stesso editor, a un indirizzo che la gente ricorda. I link al vecchio indirizzo continuano a funzionare e portano a quello nuovo.',
      points: ['Per le app Premium: scrivici', 'Stessa app, stesso editor, stesse voci', 'Il vecchio indirizzo manda avanti i visitatori'],
      alt: 'L’indirizzo dell’app che passa da quello su genhttp.run al dominio del circolo',
    },
    code: {
      name: 'Codice e git',
      title: 'Il codice, quando lo vuoi',
      text: 'Passa l’editor alla vista completa e il codice di ogni versione è lì, con i suoi log e i suoi numeri. Clona l’app con git e fai push di una modifica, oppure scaricala come progetto .NET che gira ovunque. È il tuo codice.',
      points: ['Ogni file, confrontato da una versione all’altra', 'Git: un commit su main è una versione, un branch è una bozza', 'Un download che gira da solo, voci incluse'],
      alt: 'La vista completa dell’editor: i file dell’app e il suo codice',
    },
    publish: {
      name: 'Vetrina e codice sorgente',
      title: 'Mostrala, o pubblica il suo codice',
      text: 'Metti l’app in vetrina perché altri la trovino, oppure pubblica il suo codice con una licenza open source, perché chiunque possa leggerlo, mettere una stella e scaricarlo. Sono entrambe scelte tue, e nessuna delle due condivide le tue voci.',
      points: ['In vetrina, con un’immagine e una descrizione', 'Il suo codice con licenza MIT o un’altra', 'Mai le sue voci, le sue chiavi o i suoi visitatori'],
      alt: 'Il codice pubblicato della prenotazione dei campi: i suoi file, la sua documentazione e le sue versioni, con licenza MIT',
    },
  },

  draftBadge: 'Bozza',
  domainOwn: 'Premium',
  domainSubject: 'Un dominio per la mia app',
  domainAsk: 'Chiedi informazioni su un dominio',
  zoom: 'Ingrandisci',
  close: 'Chiudi',

  closeTitle: 'Provare è gratis.',
  closeAccent: 'Senza registrazione.',
  closeBuild: 'Crea qualcosa',
  closeShip: 'Pubblica quello che hai creato',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda è open source. Usala qui, oppure {selfHosting('installala sul tuo server')}: il codice è su{' '}
      {github('GitHub')}.
    </>
  ),
};
