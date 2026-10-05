import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Eine Version ist das Programm – Code und Assets – und ändert sich nie mehr, sobald sie gespeichert ist. So lässt sich jede vergleichen und genau so wieder online stellen, wie sie war. Jede hält fest, worum gebeten wurde und was sie geändert hat. Um das Lambda zu ändern, beginnen Sie einen Entwurf: Er wird zur nächsten Version, sobald alles passt. Bei mehr als ${limit} Versionen fallen die ältesten weg; die Version, die online ist, nie.`,
  none: 'Noch keine Versionen.',
  noDescription: 'Keine Beschreibung',
  online: 'online',
  putOnline: 'Diese Version online stellen',
  rollBackTitle: 'Diese ältere Version wieder online stellen',
  deploy: 'Deployen',
  rollBack: 'Zurückrollen',
  readFailed: 'Diese Version konnte nicht gelesen werden.',
  comparing: 'Wird verglichen …',
  unchanged: 'Keine Änderung zur vorherigen Version.',
  first: 'Die erste Version.',
  status: { added: 'neu', removed: 'entfernt', changed: 'geändert', same: 'gleich' },
  groups: {
    code: 'Code',
    assets: 'Assets',
    development: 'Entwicklungsbereich',
    context: 'Dokumentation und Tests',
  },
  browse: 'Dateien ansehen',
  docs: 'Dokumentation lesen',
  development: 'Ansehen, woraus sie gebaut wird',
  edit: 'Von hier aus bearbeiten',
  feature: 'Von hier aus einen Entwurf beginnen',
  featureTitle:
    'Neben dem Lambda an einer Änderung dieser Version arbeiten und sie als nächste Version übernehmen, sobald alles passt',
  binary: 'Kein Text, also keine Zeilen zum Vergleichen.',
  tooLarge: 'Zu groß für einen zeilenweisen Vergleich.',
};
