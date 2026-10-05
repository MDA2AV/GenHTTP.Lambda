import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Build',
  hint: 'Ciò da cui sono costruiti gli asset o il codice di una versione: file su cui chi modifica l’app (il tuo agente, in un clone) esegue uno strumento di build, conservati con ogni versione e mai compilati né serviti. Questa piattaforma non costruisce niente, quindi qui si leggono e non si modificano.',
  overview: 'Panoramica',
  files: 'File',
  scope: (version) =>
    `Ciò da cui è costruita la versione ${version}: conservato con lei, mai compilato né servito, e costruito da chi la modifica, mai qui.`,
  scopeDraft: 'Ciò da cui è costruita questa bozza: conservato con lei, mai compilato né servito, e costruito da chi la modifica, mai qui.',
  reading: 'Lettura di ciò da cui è costruita…',
  readFailed: 'Non è stato possibile leggere ciò da cui è costruita.',

  emptyTitle: (version) => `La versione ${version} non conserva nulla da cui sia costruita`,
  emptyTitleDraft: 'Questa bozza non conserva nulla da cui sia costruita',
  emptyText: (code) => (
    <>
      Quando gli asset o il codice di una versione sono prodotti da uno strumento di build (compilati, assemblati o
      generati), i file da cui sono prodotti si conservano qui, con ogni versione: la cartella {code('dev/')} in un
      clone. Chi modifica l’app esegue la build dove lavora e salva entrambi insieme; questa piattaforma non
      costruisce niente. Ciò che è scritto così com’è servito o compilato non ne ha bisogno.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} in un clone spiega a un agente di programmazione come si usa.</>
  ),

  inVersion: (version) => `Nella versione ${version}`,
  inDraft: 'In questa bozza',
  comparedWith: (version) => `rispetto alla versione ${version}`,
  first: 'La prima versione che lo conserva.',
  both: (here, program) =>
    `${here === 1 ? '1 file modificato' : `${here} file modificati`} qui, e ${program === 1 ? '1 file' : `${program} file`} del codice e degli asset.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 file modificato' : `${here} file modificati`} qui, e niente del codice o degli asset: se ciò che è cambiato vi è incorporato, non è stato costruito.`,
  programOnly: 'Qui non è cambiato niente.',
  unchanged: 'Qui non è cambiato niente, né nel codice o negli asset.',
  showChanges: 'Mostra le modifiche',
  hideChanges: 'Nascondi le modifiche',
  noChanges: 'Qui non è cambiato niente.',

  readme: 'Come viene costruito',
  noReadme: (code) => (
    <>
      Nulla dice come viene costruito. Un {code('README.md')} in cima, con i comandi e la destinazione della build, è
      ciò da cui costruirà il prossimo agente.
    </>
  ),
  readOnly: 'Sola lettura: si modifica dove viene costruito.',
  noFiles: 'Nessun file.',
};
