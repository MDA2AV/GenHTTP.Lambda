import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Impossibile leggere il log.',
  hint: (capturing) =>
    'Richieste, cosa ha stampato la lambda e cosa è andato storto, in tempo reale.' +
    (capturing ? '' : ' Questa installazione non conserva quello che stampano le lambda, quindi compaiono solo richieste ed errori.') +
    ' Il log sta in memoria ed è condiviso con tutte le lambda di questo server: copre da qualche minuto a qualche ora e si svuota dopo un riavvio. Gli indirizzi dei visitatori non vengono mostrati.',
  featureHint: (capturing) =>
    'Le risposte dell’anteprima di questa bozza, cosa ha stampato e cosa è andato storto, in tempo reale.' +
    (capturing ? '' : ' Questa installazione non conserva quello che stampano le lambda, quindi compaiono solo richieste ed errori.') +
    ' È separato dal log della lambda, che non mostra mai l’anteprima. Sta in memoria, quindi copre da qualche minuto a qualche ora.',
  nothingPreview: 'Ancora niente. Apri l’anteprima della bozza e qui compariranno le sue richieste.',
  search: 'Cerca',
  searchLabel: 'Cerca nel log',
  resume: 'Mostra le nuove righe man mano che arrivano',
  pause: 'Blocca le nuove righe mentre leggi',
  paused: 'In pausa',
  live: 'Live',
  show: 'Mostra',
  all: 'Tutto',
  requests: 'Richieste',
  output: 'Output',
  problems: 'Errori',
  reading: 'Lettura del log…',
  noProblems: 'Nessun errore, almeno tra quelli che il log ricorda ancora.',
  nothing: 'Ancora niente. Apri l’indirizzo della lambda e qui compariranno le sue richieste.',
  noMatch: 'Nessun risultato.',
  identical: (count) => `${count} righe identiche`,
  at: (domain) => `, su ${domain}`,
  from: (country) => `, paese: ${country}`,
};
