import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Un file C# in cima: lettere, numeri, trattini e underscore, che inizia con una lettera e finisce in .cs, al massimo 40 caratteri.',
  name: 'Lettere, numeri e - _ . + @ ( ) [ ] { } $ ~, cartelle separate da barre, senza spazi e senza nomi che finiscono con un punto.',
  taken: 'Una lambda esportata o clonata ha già un file o una cartella con quel nome in cima. Mettilo in una cartella, oppure scegli un altro nome.',
  lambda: 'Una lambda non ha più una cartella .lambda/: la sua documentazione va in docs/, i suoi test in tests/.',
  assets: 'Quello che una lambda serve ora sta nelle sue risorse: aggiungilo lì.',
  resourceName: 'Lettere, numeri, trattini, underscore e punti, separati da barre, al massimo sei cartelle di profondità, e un’estensione, così viene servito come il tipo giusto.',
  exists: 'Esiste già un file con questo nome.',
  remove: (name) => `Rimuovere ${name}? Anche il suo contenuto andrà perso.`,
  removeFolder: (name, files) => `Rimuovere ${name} e ${files === 1 ? '1 file' : `${files} file`} al suo interno?`,
  there: (name) => `${name} esiste già.`,
  entry: 'Lo snippet: quello che restituisce è ciò che viene servito',
  errors: 'contiene errori',
  removeFile: (name) => `Rimuovi ${name}`,
  removeTitle: 'Rimuovi',
  codePlaceholder: 'Store.cs, docs/notes.md o frontend/app.ts',
  resourcePlaceholder: 'web/index.html',
  upload: 'Carica un file',
};
