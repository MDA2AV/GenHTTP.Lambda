import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Vetrina',
  title: 'Realizzate qui, attive ora',
  intro:
    'Lambda che i proprietari hanno scelto di mostrare. Sono tutti online, quindi ogni scheda apre l’applicazione reale. I più utilizzati di recente compaiono per primi.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambda`),
  failed: 'Impossibile caricare la vetrina.',
  loadingMore: 'Caricamento…',
  showMore: 'Mostra altri',
  nothingTitle: 'Ancora nessuna applicazione in vetrina',
  nothing: (tab) => (
    <>
      Ha realizzato un’applicazione funzionante? Apra il relativo centro di controllo, selezioni {tab('Vetrina')} e
      aggiunga un titolo, una breve descrizione e un’immagine. Comparirà qui finché resta online.
    </>
  ),
  buildOne: 'Crea un’applicazione',
  yoursTitle: 'Desidera mostrare la Sua?',
  yours: (tab) => (
    <>
      Apra il centro di controllo del Suo lambda e selezioni {tab('Vetrina')}, oppure chieda all’agente che lo ha creato
      di aggiungerlo. Solo chi possiede la chiave di modifica può farlo, e l’inserimento può essere rimosso in qualsiasi
      momento.
    </>
  ),
  buildSomething: 'Crea un’applicazione',
};

export const card: Messages['card'] = {
  noPicture: 'Nessuna immagine',
  title: 'Titolo',
  description: 'Cosa può farne un visitatore.',
  opens: (title, address) => `${title}, apre ${address} in una nuova scheda`,
};
