import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Das Log konnte nicht gelesen werden.',
  hint: (capturing) =>
    'Requests, Ausgaben des Lambdas und Fehler – live.' +
    (capturing ? '' : ' Diese Installation speichert keine Ausgaben von Lambdas, daher erscheinen nur Requests und Fehler.') +
    ' Das Log liegt im Arbeitsspeicher und wird mit allen Lambdas hier geteilt. Es reicht Minuten bis Stunden zurück und ist nach einem Neustart leer. Adressen von Besuchern werden nicht angezeigt.',
  featureHint: (capturing) =>
    'Requests, Ausgaben und Fehler der Vorschau dieses Entwurfs – live.' +
    (capturing ? '' : ' Diese Installation speichert keine Ausgaben von Lambdas, daher erscheinen nur Requests und Fehler.') +
    ' Getrennt vom Log des Lambdas, das die Vorschau nie zeigt. Es liegt im Arbeitsspeicher und reicht Minuten bis Stunden zurück.',
  nothingPreview: 'Noch nichts. Öffnen Sie die Vorschau des Entwurfs, dann erscheinen hier ihre Requests.',
  search: 'Suchen',
  searchLabel: 'Log durchsuchen',
  resume: 'Neue Zeilen live anzeigen',
  pause: 'Keine neuen Zeilen, während Sie lesen',
  paused: 'Pausiert',
  live: 'Live',
  show: 'Zeigen',
  all: 'Alles',
  requests: 'Requests',
  output: 'Ausgaben',
  problems: 'Fehler',
  reading: 'Log wird gelesen …',
  noProblems: 'Nichts ist schiefgegangen, soweit das Log zurückreicht.',
  nothing: 'Noch nichts. Rufen Sie die Adresse des Lambdas auf, dann erscheinen hier seine Requests.',
  noMatch: 'Keine Treffer.',
  identical: (count) => `${count} identische Zeilen`,
  at: (domain) => `, über ${domain}`,
  from: (country) => `, aus ${country}`,
};
