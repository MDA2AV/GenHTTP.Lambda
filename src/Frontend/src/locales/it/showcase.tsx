import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Vetrina',
  title: 'Nate qui, online adesso',
  intro:
    'Lambda che i proprietari hanno deciso di mostrare. Sono tutte online, quindi ogni scheda apre l’app vera. Prima quelle usate di recente.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambda`),
  failed: 'Impossibile caricare la vetrina.',
  loadingMore: 'Caricamento…',
  showMore: 'Mostra altre',
  nothingTitle: 'Ancora niente in vetrina',
  nothing: (tab) => (
    <>
      Hai creato qualcosa che funziona? Apri il suo pannello di controllo, scegli {tab('Vetrina')} e aggiungi un titolo,
      due righe di descrizione e un’immagine. Comparirà qui finché è online.
    </>
  ),
  buildOne: 'Crea un’app',
  yoursTitle: 'Vuoi qui anche la tua?',
  yours: (tab) => (
    <>
      Apri il pannello di controllo della tua lambda e scegli {tab('Vetrina')}, oppure chiedi all’agente che l’ha creata
      di metterla in vetrina. Può farlo solo chi ha la chiave di modifica, e puoi toglierla dalla vetrina quando vuoi.
    </>
  ),
  buildSomething: 'Crea qualcosa',
};

export const card: Messages['card'] = {
  noPicture: 'Ancora nessuna immagine',
  title: 'Titolo',
  description: 'Cosa ci può fare chi la visita.',
  opens: (title, address) => `${title}, apre ${address} in una nuova scheda`,
};
