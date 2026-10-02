import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Impossibile caricare la vetrina.',
  loading: 'Caricamento…',
  title: 'un titolo',
  description: 'una descrizione',
  picture: 'un’immagine',
  updated: 'Scheda in vetrina aggiornata.',
  listed: 'Ora è in vetrina.',
  waiting: 'Salvato. Comparirà in vetrina appena la lambda sarà online.',
  saveFailed: 'Impossibile salvare la scheda in vetrina.',
  removed: 'Tolta dalla vetrina.',
  removeFailed: 'Impossibile togliere la scheda dalla vetrina.',
  wrongType: 'Non è un’immagine PNG, JPEG, GIF o WebP.',
  tooLarge: (size, limit) => `Pesa ${size}; un’immagine può pesare al massimo ${limit}.`,
  unreadable: 'Impossibile leggere il file.',
  hint: (tool) => (
    <>
      La vetrina mostra le lambda che i proprietari hanno deciso di far vedere, prima quelle usate di recente. Solo chi
      ha la chiave di modifica può metterci una lambda o toglierla, e la lambda compare solo finché è online. Un agente
      può fare lo stesso con lo strumento {tool}.
    </>
  ),
  open: 'Apri la vetrina',
  switch: 'Mostra questa lambda in vetrina',
  listedNow: 'In vetrina. Chiunque la sfogli può aprirla.',
  notListed: 'Salvata, ma non in vetrina: la lambda è offline. Ricomparirà dopo il prossimo deploy.',
  off: 'Disattivata. Questa lambda non compare da nessuna parte finché non attivi l’opzione e salvi.',
  offline: 'La lambda è offline, quindi la scheda aspetterà il prossimo deploy. In vetrina finiscono solo le lambda che rispondono.',
  titleLabel: 'Titolo',
  titlePlaceholder: 'Classifica del quiz al pub',
  descriptionLabel: 'Descrizione',
  descriptionPlaceholder:
    'Le squadre inseriscono le risposte dal telefono, il presentatore le corregge e la classifica si aggiorna per tutti in sala.',
  save: 'Salva le modifiche',
  add: 'Aggiungi alla vetrina',
  takeOff: 'Togli dalla vetrina',
  needs: (missing) =>
    `${missing.length > 1 ? 'Mancano' : 'Manca'} ancora ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} e ${missing[missing.length - 1]}` : missing[0]}.`,
  tooLong: 'Qualche campo è troppo lungo.',
  allSaved: 'Tutto salvato.',
  preview: 'Anteprima',
  card: (address) => <>Questa è la scheda che vedono i visitatori. Apre {address}.</>,
  confirm: 'Togliere dalla vetrina?',
  keep: 'Lasciala',
  confirmText: 'Titolo, descrizione e immagine vengono eliminati. La lambda resta esattamente com’è.',
  pictureLabel: 'Immagine',
  formats: (limit) => `PNG, JPEG, GIF o WebP, fino a ${limit}`,
  notSaved: 'non ancora salvata',
  replace: 'Trascina qui un’altra immagine per sostituirla.',
  drop: 'Trascina qui un’immagine.',
  advice: 'Funziona meglio uno screenshot, o una breve GIF dell’app in uso, in 16:10.',
  another: 'Scegline un’altra',
  choose: 'Scegli un file',
  keepSaved: 'Tieni quella salvata',
  clear: 'Rimuovi',
};
