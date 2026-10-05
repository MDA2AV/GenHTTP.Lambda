import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Una versione è il programma (il suo codice e i suoi asset) e, una volta salvata, non cambia più: così ognuna si può confrontare e rimettere online esattamente com’era. Ognuna conserva cosa è stato chiesto e cosa ha cambiato. Per cambiare la lambda, avvia una bozza: diventa la prossima versione quando è a posto. Oltre ${limit} versioni, le più vecchie vengono eliminate; quella online mai.`,
  none: 'Ancora nessuna versione.',
  noDescription: 'Nessuna descrizione',
  online: 'online',
  putOnline: 'Metti online questa versione',
  rollBackTitle: 'Rimetti online questa versione precedente',
  deploy: 'Deploy',
  rollBack: 'Ripristina',
  readFailed: 'Impossibile leggere questa versione.',
  comparing: 'Confronto…',
  unchanged: 'Nessuna modifica rispetto alla versione precedente.',
  first: 'La prima versione.',
  status: { added: 'aggiunto', removed: 'rimosso', changed: 'modificato', same: 'invariato' },
  groups: {
    code: 'Codice',
    assets: 'Asset',
    build: 'Build',
    context: 'Documentazione e test',
  },
  browse: 'Sfoglia i file',
  docs: 'Leggi la sua documentazione',
  build: 'Vedi da cosa è costruita',
  edit: 'Modifica da qui',
  feature: 'Avvia una bozza da qui',
  featureTitle: 'Lavora a una modifica di questa versione accanto alla lambda, e integrala nella prossima versione quando è a posto',
  binary: 'Non è testo, quindi non ci sono righe da confrontare.',
  tooLarge: 'Troppo grande per un confronto riga per riga.',
};
