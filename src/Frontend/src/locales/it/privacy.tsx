import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Informativa sulla privacy',
  binding: (english) => (
    <>Questa traduzione è solo informativa. Fa fede la {english('versione inglese')} di questa pagina.</>
  ),
  intro:
    'Cosa sa di te questo sito, cosa ne fa, per quanto tempo lo conserva e chi altro può vederlo. In breve: niente account, niente pubblicità e nessun tracciamento. Il server annota chi gli ha chiesto cosa, per restare in funzione e poter risalire agli abusi, e quello che chiedi al nostro agente viene inviato ad Anthropic, il cui modello scrive l’app.',
  sections: {
    whoTitle: 'Chi è il titolare del trattamento',
    who: 'Questo sito è gestito dalla persona indicata qui sotto, che è titolare del trattamento dei dati personali ai sensi del Regolamento generale sulla protezione dei dati dell’UE (GDPR). Per qualsiasi cosa riguardi questa pagina, scrivi a questo indirizzo:',

    requestsTitle: 'Cosa annota il server a ogni richiesta',
    requests: [
      'Ogni richiesta a questo sito, e a ogni lambda ospitata qui, viene scritta nel log del server: l’indirizzo IP da cui arriva e, se è stata inoltrata, l’indirizzo originale che dichiara, il browser o il programma che l’ha inviata, l’indirizzo richiesto, quando e con quale risposta. Il server cerca anche a quale paese, città e rete appartiene l’indirizzo IP, in un database che gestisce da sé, senza chiedere a nessun altro.',
      'Serve a trovare i problemi, a capire cosa sta sovraccaricando il server e a rintracciare gli abusi che ci vengono segnalati. L’indirizzo IP viene usato anche, solo in memoria, per limitare quante richieste può fare e quante app può creare un singolo visitatore. Senza questi dati non si può rispondere a una richiesta. La base giuridica è il nostro legittimo interesse a gestire il servizio e a mantenerlo sicuro (art. 6, par. 1, lett. f) GDPR).',
      'Gli amministratori possono vedere tutto. Il proprietario di una lambda vede il paese e il browser di ogni richiesta alla sua lambda, ma non l’indirizzo IP.',
    ],

    logsTitle: 'Per quanto tempo si conserva il log',
    logs: 'Il log si trova in due posti: nella memoria del server, che si svuota a ogni riavvio, e nell’output della console del server, che viene cancellato a ogni aggiornamento. Entrambi hanno una dimensione fissa, quindi ogni nuova riga spinge fuori la più vecchia, e quanto dura una riga dipende da quanto è trafficato il sito. Nessuna parte del log viene archiviata.',

    contentTitle: 'Quello che metti qui',
    content: (days) =>
      `Una lambda è il suo codice, i suoi file, le sue impostazioni e le note salvate con le sue versioni su cosa è stato chiesto e cosa è cambiato. Tutto questo è conservato sul server per poterla eseguire e modificare. Una lambda gratuita viene eliminata con tutte le sue versioni circa ${days} giorni dopo l’ultima modifica o visita, e subito se chi ha il suo link di modifica la elimina. Chiunque abbia il link di modifica può leggere tutto, quello che metti in vetrina lo vedono tutti, e gli amministratori guardano una lambda quando serve, per gestire una segnalazione o proteggere il server. La base giuridica è la fornitura del servizio che hai richiesto (art. 6, par. 1, lett. b) GDPR).`,

    agentTitle: 'Cosa chiedi al nostro agente',
    agent: (policy) => (
      <>
        Quello che scrivi nel campo in cui chiedi l’app, o nella sezione Modifica dell’editor di una lambda, viene
        inviato ad Anthropic PBC, negli Stati Uniti, che gestisce Claude, il modello che scrive l’app. Per fare una
        modifica, l’agente legge anche la lambda (il codice, le note delle sue versioni e il suo log, che contiene le
        richieste e quello che ha stampato, ma non gli indirizzi IP dei visitatori), e anche quello che legge viene inviato
        lì. Cosa ne fa Anthropic lo stabilisce {policy('la sua informativa sulla privacy')}. Gli Stati Uniti non
        proteggono i dati personali come l’UE. La tua richiesta viene inviata lì perché serve a creare o modificare
        quello che hai chiesto (artt. 6, par. 1, lett. b) e 49, par. 1, lett. b) GDPR), quindi non scriverci niente che
        non vorresti condividere.
      </>
    ),
    agentKept:
      'L’agente salva la tua richiesta, spesso riformulata con parole sue, come nota della versione che scrive, e le prime centinaia di caratteri finiscono nel log del servizio di creazione, che ha anch’esso una dimensione fissa. Se invece usi un tuo agente, come Claude o Claude Code, quello che gli dici va al fornitore di quell’agente, non a noi: noi riceviamo solo il codice e le note che manda qui.',

    lambdasTitle: 'Cosa fa una lambda lo decide il suo proprietario',
    lambdas:
      'Una lambda la scrive chi ha il suo link di modifica, non noi. Cosa chiede ai suoi visitatori e cosa ne fa lo decide quella persona, e questa pagina non lo copre, a parte il log delle richieste descritto sopra, che il server tiene per ogni lambda. I termini di servizio non consentono di usare una lambda per raccogliere dati personali di altre persone: se ne trovi una che lo fa, segnalacela.',

    mailTitle: 'Quando ci scrivi',
    mail: 'Se ci scrivi, per segnalare un abuso o per qualsiasi altro motivo, usiamo il tuo indirizzo email e il tuo messaggio per risponderti e occuparci di quello che ci hai scritto, e li cancelliamo quando non servono più a questo (art. 6, par. 1, lett. f) GDPR).',

    storageTitle: 'I cookie e il tuo browser',
    storage:
      'C’è un solo cookie, che si chiama lang. Ricorda la lingua che hai scelto, così gli indirizzi che non ne indicano una si aprono in quella lingua, e dura un anno. La memoria locale del browser ricorda il tema chiaro o scuro, alcune impostazioni delle pagine che usi e, per gli amministratori, il loro token. Niente di tutto questo serve a seguirti e niente va a qualcun altro: niente strumenti di analisi, niente pubblicità, e niente viene caricato da altri siti, nemmeno i font. Visto che tutto serve solo a fare quello che hai chiesto tu, non serve il consenso (§ 25, comma 2, n. 2 della legge tedesca TDDDG).',

    hostingTitle: 'Dove sono conservati i dati',
    hosting:
      'Il server su cui gira tutto questo è preso in affitto da un fornitore di hosting nell’Unione europea, e quello che descrive questa pagina è conservato lì.',

    rightsTitle: 'I tuoi diritti',
    rights: (mailbox) => (
      <>
        Puoi chiederci quali dati abbiamo su di te e averne una copia, farli correggere, cancellare o limitarne l’uso, e
        opporti a qualsiasi cosa facciamo in base al nostro legittimo interesse (artt. da 15 a 21 GDPR). Scrivi a{' '}
        {mailbox}. Non ci sono account, quindi possiamo trovare quello che ti riguarda solo se ci dici come: l’indirizzo
        IP che hai usato e più o meno quando, oppure l’indirizzo della tua lambda. Nessuna decisione che produca effetti
        giuridici su di te o che ti riguardi in modo altrettanto significativo viene presa in modo automatizzato (art. 22
        GDPR).
      </>
    ),
    complaint:
      'Puoi anche presentare un reclamo a un’autorità di controllo per la protezione dei dati, dove vivi o dove siamo noi. Per noi è competente il Commissario per la protezione dei dati e la libertà d’informazione del Land tedesco Baden-Württemberg (LfDI Baden-Württemberg).',
  },
  change: 'Questa informativa cambia quando cambia il sito. Vale la versione pubblicata in questa pagina.',
  updated: 'Ultima modifica: 28 settembre 2026.',
};
