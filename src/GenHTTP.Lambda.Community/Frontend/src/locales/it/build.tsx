import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Crei un’app descrivendola',
  intro:
    'Dica all’agente di creazione di questa installazione di cosa ha bisogno. Scrive l’app, la mette online qui e la tiene nel suo elenco, per fare domande e modificarla in seguito allo stesso modo.',
  other: (ship, create) => (
    <>
      Oppure {ship('colleghi il suo agente')}, oppure {create('scriva il codice da sé')}.
    </>
  ),
};
