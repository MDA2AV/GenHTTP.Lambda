import type { EditorMessages } from '../../en/editor';

export const features: EditorMessages['features'] = {
  hint:
    'Ein Entwurf ist eine Kopie Ihrer App, an der Sie eine Änderung ausprobieren, bevor jemand sie sieht – mit eigener Adresse und eigenen Testdaten. Stellen Sie ihn online, wenn alles passt; bis dahin bekommen Ihre Besucher weiter, was jetzt online ist.',
  newFeature: 'Neuer Entwurf',
  full: (limit) => `Es gibt bereits ${limit} Entwürfe – mehr sind nicht erlaubt. Stellen Sie zuerst einen online oder verwerfen Sie ihn.`,
  emptyTitle: 'Keine Entwürfe',
  emptyText:
    'Ein Entwurf ist eine Kopie Ihrer App, an der Sie eine Änderung ausprobieren, bevor sie online geht. Wenn der Agent eine Änderung zum Ausprobieren für Sie bereithält, finden Sie sie hier.',
  start: 'Neuer Entwurf',
  askAgentNew: 'Den Agenten um eine Änderung bitten',
  noChange: 'Noch keine Angabe, was er ändert',
  behindTitle: 'Ihre App hat sich geändert, seit dieser Entwurf begonnen wurde',
  behind: () => 'nicht aktuell',
  previewOnline: 'Vorschau läuft',
  previewOutdated: 'Vorschau zeigt einen älteren Stand',
  previewOffline: 'Vorschau läuft nicht',
  changed: 'geändert',
  openPreview: 'Ausprobieren',
  openPreviewTitle: 'Die Vorschau in einem neuen Tab öffnen',
  count: (open, limit) => `${open} von ${limit} Entwürfen`,
  loading: 'Der Entwurf lädt …',
  readFailed: 'Der Entwurf konnte nicht gelesen werden.',

  newTitle: 'Neuer Entwurf',
  newText:
    'Eine Kopie Ihrer App und ihrer Daten mit eigener Adresse. Ändern Sie sie dort und probieren Sie sie aus – Ihre Besucher sehen nichts davon, bis Sie sie online stellen.',
  newTextFiles:
    'Was Sie geschrieben haben, kommt in den Entwurf, statt eine Version zu werden. So können Sie es unter seiner eigenen Adresse ausprobieren, bevor es online geht.',
  name: 'Name',
  namePlaceholder: 'Bestenliste',
  wanted: 'Was soll er tun?',
  wantedPlaceholder: 'Optional. Die zehn besten Punktestände behalten und nach jedem Spiel anzeigen.',
  olderBase: (newest) =>
    `Dieser Entwurf geht von einer älteren Version aus und ist deshalb von Anfang an nicht aktuell: Bevor er online gehen kann, muss eingearbeitet werden, was sich bis Version ${newest} geändert hat.`,
  create: 'Entwurf beginnen',
  createFailed: 'Der Entwurf konnte nicht angelegt werden.',
  retry: 'Noch einmal versuchen',
  madeNotSaved: (name) =>
    `Der Entwurf „${name}“ ist angelegt, aber was Sie geschrieben haben, konnte noch nicht darin gespeichert werden. Versuchen Sie es noch einmal, oder schließen Sie dieses Fenster und finden Sie den Entwurf unter Entwürfe.`,
  created: (name) => `Der Entwurf „${name}“ ist angelegt.`,
  cancel: 'Abbrechen',

  featureHint:
    'Eine Kopie Ihrer App, an der Sie diese Änderung ausprobieren. Die Vorschau hat eine eigene Adresse und eigene Testdaten, daher sehen Ihre Besucher nichts davon, bis Sie sie online stellen.',
  askAgent: 'Den Agenten fragen',
  askCatchUp: 'Den Agenten bitten, ihn auf den neuesten Stand zu bringen',
  catchUp: 'Bring diesen Entwurf auf den Stand der neuesten Version der App, und behalte, was er ändert.',
  editCode: 'Code bearbeiten',
  deployPreview: 'Vorschau starten',
  updatePreview: 'Vorschau aktualisieren',
  previewDeployed: 'Die Vorschau läuft.',
  previewFailed: 'Die Vorschau konnte nicht gestartet werden.',
  previewStopped: 'Die Vorschau ist gestoppt.',
  previewRejected: 'Die Vorschau hat sich nicht geändert',
  previewNotCompiling: 'Er lässt sich nicht kompilieren, daher zeigt die Vorschau weiter die letzte Version, die sich kompilieren ließ.',
  started: 'Begonnen',
  changes: () => 'Geänderte Dateien',
  noChanges: () => 'Noch nichts geändert.',
  editNotes: 'Name und Notizen',
  what: 'Was ändert er?',
  whatPlaceholder: 'Fügt eine Bestenliste mit den zehn besten Punkteständen hinzu',
  missed: () => 'Was sich in Ihrer App seit dem Beginn geändert hat',
  missedNothing: 'Nichts an den Dateien.',

  behindText: (_base, newest) =>
    `Version ${newest} Ihrer App wurde gespeichert, nachdem dieser Entwurf begonnen wurde. Wenn Sie den Entwurf jetzt online stellen, würde das rückgängig machen, was diese Version geändert hat. Er muss deshalb zuerst auf den neuesten Stand gebracht werden – das kann der Agent für Sie erledigen.`,
  moveBase: 'Als aktuell markieren',
  close: 'Schließen',
  mergeTitle: (name) => `„${name}“ online stellen`,
  mergeTitleShort: 'Zur neuen Version Ihrer App machen und online stellen',
  leaks: (path, files) =>
    `${files} verlinken auf ${path}, also auf Ihre App, die online ist. Aus der Vorschau heraus lesen und ändern diese Links deren echte Daten statt der Testdaten. Bitten Sie den Agenten, ohne diesen Teil zu verlinken („api/items“).`,
  mergeButton: 'Online stellen',
  saveFirst: 'Speichern Sie zuerst Ihre Änderungen: Vorschau und Online-Stellen verwenden, was gespeichert ist.',
  mergeAndDeploy: () => 'Online stellen',
  mergeText: (version) =>
    `Er wird zu Version ${version} Ihrer App und geht online. Die Daten Ihrer App bleiben, wie sie sind.`,
  deployTooNote: (active) => `Zu Version ${active} kommen Sie unter den Versionen mit einem Klick zurück.`,
  deployTooOffline: 'Ihre App ist gerade offline; damit geht sie online.',
  notCompiling: 'Er lässt sich nicht kompilieren, daher ging er nicht online. Beheben Sie das zuerst im Entwurf.',
  mergeFailed: 'Der Entwurf konnte nicht online gestellt werden.',
  merged: (version) => `Als Version ${version} gespeichert.`,
  mergedOnline: (version) => `Version ${version} ist online.`,

  notesTitle: 'Name und Notizen',
  save: 'Speichern',
  saveFailed: 'Das konnte nicht gespeichert werden.',

  baseTitle: 'Als aktuell markieren?',
  baseText: () =>
    'Nur ein Entwurf, der enthält, was die neueste Version geändert hat, kann online gehen, ohne es rückgängig zu machen. Sind diese Änderungen jetzt im Entwurf – von Ihnen oder vom Agenten eingebracht –, markieren Sie ihn als aktuell.',
  moveTo: () => 'Als aktuell markieren',
  baseWarning: 'Das wird nicht geprüft. Sind die Änderungen nicht im Entwurf, macht das Online-Stellen sie rückgängig.',

  deleteTitle: (name) => `„${name}“ verwerfen?`,
  deleteText:
    'Sein Code, seine Vorschau und seine Testdaten werden endgültig gelöscht. Ihre App und ihre Versionen bleiben unberührt.',
  keep: 'Behalten',
  deleteForGood: 'Verwerfen',
  deleteFailed: 'Der Entwurf konnte nicht verworfen werden.',
  deleted: (name) => `Der Entwurf „${name}“ wurde verworfen.`,

  all: 'Alle Entwürfe',
  actions: 'Weitere Aktionen für diesen Entwurf',
  download: 'Als ZIP herunterladen',
  stopPreview: 'Vorschau stoppen',
  delete: 'Diesen Entwurf verwerfen',
  viewsLabel: 'Der Entwurf',
  views: {
    overview: 'Entwurf',
    docs: 'Dokumentation',
    code: 'Code',
    tests: 'Tests',
    data: 'Testdaten',
    logs: 'Logs',
  },
  missingTitle: 'Diesen Entwurf gibt es nicht mehr',
  missingText: 'Er wurde online gestellt oder verworfen. Die Versionen zeigen, was aus ihm geworden ist.',
};
