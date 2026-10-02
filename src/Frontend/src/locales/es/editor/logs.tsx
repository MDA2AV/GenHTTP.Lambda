import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'No se pudieron leer los logs.',
  hint: (capturing) =>
    'Peticiones, lo que imprime la lambda y lo que sale mal, en tiempo real.' +
    (capturing ? '' : ' Esta instalación no guarda lo que imprimen las lambdas, así que solo aparecen peticiones y errores.') +
    ' Se guardan en memoria y se comparten con todas las lambdas de aquí, así que abarcan de minutos a horas y se vacían tras un reinicio. No se muestran las direcciones de los visitantes.',
  featureHint: (capturing) =>
    'Lo que respondió la vista previa de este borrador, lo que imprimió y lo que falló, en tiempo real.' +
    (capturing ? '' : ' Esta instalación no guarda lo que imprimen las lambdas, así que solo aparecen peticiones y errores.') +
    ' Se guardan aparte de los logs de la propia lambda, que nunca muestran la vista previa. Se guardan en memoria, así que abarcan de minutos a horas.',
  nothingPreview: 'Nada todavía. Abre la vista previa del borrador y sus peticiones aparecerán aquí.',
  search: 'Buscar',
  searchLabel: 'Buscar en los logs',
  resume: 'Mostrar las líneas nuevas según llegan',
  pause: 'Dejar de añadir líneas mientras lees',
  paused: 'En pausa',
  live: 'En vivo',
  show: 'Mostrar',
  all: 'Todo',
  requests: 'Peticiones',
  output: 'Salida',
  problems: 'Problemas',
  reading: 'Leyendo los logs…',
  noProblems: 'No hay ningún fallo que los logs recuerden.',
  nothing: 'Nada todavía. Abre la dirección de la lambda y sus peticiones aparecerán aquí.',
  noMatch: 'No hay coincidencias.',
  identical: (count) => `${count} líneas idénticas`,
  at: (domain) => `, en ${domain}`,
  from: (country) => `, desde ${country}`,
};
