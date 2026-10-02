import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: 'De logs konden niet worden gelezen.',
  hint: (capturing) =>
    'Requests, output van de lambda en wat er misging, live.' +
    (capturing ? '' : " Deze installatie bewaart de output van lambda's niet, dus je ziet alleen requests en fouten.") +
    " De logs staan in het geheugen, dat alle lambda's hier delen. Ze gaan dus minuten tot uren terug, en zijn leeg na een herstart. Adressen van bezoekers worden niet getoond.",
  featureHint: (capturing) =>
    'Requests, output en fouten van de voorvertoning van dit concept, live.' +
    (capturing ? '' : " Deze installatie bewaart de output van lambda's niet, dus je ziet alleen requests en fouten.") +
    ' Ze staan los van de logs van de lambda zelf, die de voorvertoning nooit tonen. De logs staan in het geheugen, dus ze gaan minuten tot uren terug.',
  nothingPreview: 'Nog niets. Open de voorvertoning van het concept, dan verschijnen de requests hier.',
  search: 'Zoeken',
  searchLabel: 'Zoeken in de logs',
  resume: 'Nieuwe regels tonen zodra ze binnenkomen',
  pause: 'Geen nieuwe regels toevoegen terwijl je leest',
  paused: 'Gepauzeerd',
  live: 'Live',
  show: 'Tonen',
  all: 'Alles',
  requests: 'Requests',
  output: 'Output',
  problems: 'Problemen',
  reading: 'Logs laden…',
  noProblems: 'Er is niets misgegaan, voor zover de logs nog weten.',
  nothing: 'Nog niets. Open het adres van de lambda, dan verschijnen de requests hier.',
  noMatch: 'Geen resultaten.',
  identical: (count) => `${count} identieke regels`,
  at: (domain) => `, op ${domain}`,
  from: (country) => `, uit ${country}`,
};
