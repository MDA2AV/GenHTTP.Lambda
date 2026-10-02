import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Documentazione',
    titleSimple: 'Informazioni sulla tua app',
    hint: 'Cos’è questa app, per chi è e perché, e perché è costruita così. Gli agenti la scrivono a ogni modifica e resta con ogni versione, così una versione precedente torna insieme alla documentazione che valeva per lei.',
    hintSimple: 'A cosa serve la tua app e perché, per come l’ha capito l’agente da quello che hai chiesto. L’agente tiene questa descrizione aggiornata a ogni modifica.',
    inDraft: 'La documentazione di questa bozza. Diventa quella della tua app quando la bozza va online.',
    pages: { product: 'Prodotto', decisions: 'Decisioni' },
    emptyTitle: 'Non c’è ancora niente di scritto',
    emptyText: (code) => (
      <>
        Gli agenti scrivono la documentazione insieme alle loro modifiche: cos’è l’app, per chi è e perché in{' '}
        {code('.lambda/docs/product.md')}, e perché è costruita così in {code('decisions.md')}. Fa parte della
        versione, accanto al codice.
      </>
    ),
    emptySimpleTitle: 'Non c’è ancora niente di scritto sulla tua app',
    emptySimple: 'L’agente può descrivere a cosa serve la tua app e perché, partendo da quello che hai chiesto, e da lì in poi tiene aggiornata la descrizione.',
    ask: 'Chiedi all’agente di scriverla',
    describe: 'Chiedi all’agente di descriverla',
    writePrompt: 'Scrivi la documentazione di questa app: cos’è, per chi è e perché, e le decisioni tecniche che ci stanno dietro.',
    describePrompt: 'Descrivi a cosa serve questa app e perché, così posso leggerlo in «Informazioni».',
    decisionsPrompt: 'Metti per iscritto le decisioni tecniche dietro questa app, e perché sono state prese.',
    missingProduct: 'Non c’è ancora una pagina sul prodotto',
    missingProductText: 'Cos’è l’app, per chi è, cosa ci fa la gente e perché, con le parole di chi l’ha chiesta.',
    missingDecisions: 'Nessuna decisione ancora messa per iscritto',
    missingDecisionsText: 'Come è costruita l’app e perché: come conserva i dati, da cosa dipende, cosa è stato lasciato fuori. Quello che deve sapere chi la modificherà dopo.',
    correctText: 'L’agente scrive questo testo partendo da quello che hai chiesto, e lo tiene aggiornato a ogni modifica. Qualcosa non va o manca? Diglielo.',
    correct: 'Dillo all’agente',
    correctPrompt: 'Correggi la descrizione dell’app: ',
    placeholder: 'Spiega perché le voci vengono conservate per un anno',
  },
  tests: {
    title: 'Test',
    hint: 'Come viene testata automaticamente questa app, e gli script e i dati che usano i test. Gli agenti li tengono aggiornati e li eseguono prima di dare per conclusa una modifica. Restano con ogni versione.',
    inDraft: 'I test di questa bozza. Diventano quelli della tua app quando la bozza va online: prima eseguili sulla sua anteprima.',
    pages: { testing: 'Come viene testata' },
    emptyTitle: 'Ancora nessun test',
    emptyText: (code) => (
      <>
        Come viene testata l’app (cosa deve continuare a funzionare, come verificarlo e come eseguire gli script per
        farlo) lo scrivono gli agenti in {code('.lambda/tests/README.md')}, con gli script e i dati di test accanto.
      </>
    ),
    ask: 'Chiedi all’agente di scrivere i test',
    writePrompt: 'Scrivi i test di questa app: cosa deve continuare a funzionare e come verificarlo automaticamente, con uno script da eseguire sulla sua anteprima.',
    missing: 'Non è ancora detto come viene testata',
    missingText: 'Cosa deve continuare a funzionare, come si verifica ciascuna cosa e come eseguire gli script che ci sono accanto.',
    placeholder: 'Verifica che una lista piena rifiuti nuove voci',
  },
  files: 'File',
  noFiles: 'Nessun file oltre alle pagine.',
  none: 'assente',
  missingPill: 'Non ancora scritta',
  changedIn: (version) => `Modificata nella versione ${version}`,
  changedInDraft: 'Modificata in questa bozza',
  showChanges: 'Mostra cosa è cambiato',
  hideChanges: 'Nascondi cosa è cambiato',
  noChanges: 'Non è cambiato niente.',
  edit: 'Modifica',
  olderVersion: 'Una versione non cambia mai: una pagina si modifica sulla versione più recente, o in una bozza.',
  writeIt: 'Scrivila tu',
  askPage: 'Chiedi all’agente di scriverla',
  editInCode: 'Apri nel codice',
  cancel: 'Annulla',
  save: 'Salva',
  write: 'Scrivi',
  preview: 'Anteprima',
  writeOrPreview: 'Scrittura o anteprima',
  discard: 'Le modifiche a questa pagina andranno perse. Vuoi scartarle?',
  reading: 'Lettura…',
  readFailed: 'Impossibile leggere questo contenuto.',
  saveFailed: 'Impossibile salvare.',
  savedDraft: 'Salvato nella bozza.',
  savedVersion: (version) => `Salvata come versione ${version}.`,
  savedOnline: (version) => `Salvata come versione ${version}, e online.`,
  savedNotOnline: (version) => `Salvata come versione ${version}, ma non è andata online.`,
  saveTitle: 'Salva come nuova versione',
  saveText: (newest) =>
    `Una versione non cambia mai, quindi questa pagina viene salvata come la prossima: sopra la versione ${newest}, con tutto il resto com’è.`,
  clash: (version) => `La versione ${version} è stata salvata da quando hai iniziato, e ha modificato anche questa pagina. Salvando la sostituisci.`,
  alsoOnline: 'Mettila anche online',
  alsoOnlineNote: 'Cambia solo la documentazione, quindi i visitatori non vedono niente di nuovo, ma quello che è online resta la versione più recente.',
  skeleton: {
    product: '# Nome dell’app\n\nCos’è, in una o due frasi.\n\n## Per chi è\n\n## Cosa ci fa la gente\n\n## Funzionalità, e perché ci sono\n\n## Cosa non fa\n',
    decisions: '# Decisioni\n\n## Una decisione\n\nCosa è stato deciso, perché, e cosa deve tenere presente una modifica.\n',
    testing: '# Come viene testata\n\nCome eseguire i test, e su quale indirizzo.\n\n## Cosa deve continuare a funzionare\n\n| Comportamento | Richiesta | Risultato atteso |\n|---|---|---|\n| | | |\n',
  },
};
