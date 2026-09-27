import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Lambda erstellen',
  whatTitle: 'Was möchten Sie erstellen?',
  whatText:
    'Wählen Sie die passendste Vorlage: Sie starten mit einer Kopie einer funktionierenden Anwendung, die Sie beliebig anpassen können. Alternativ beginnen Sie mit einem leeren Lambda.',
  seeIt: 'Live ansehen',
  startFrom: 'Diese Vorlage verwenden',
  starters: {
    'demo-crud': {
      title: 'Einträge verwalten',
      description: 'Eine Liste, die sich ergänzen, bearbeiten und abhaken lässt – für Aufgaben, Notizen, Lesezeichen oder ein kleines Inventar.',
    },
    'demo-registration': {
      title: 'Registrierung und Anmeldung',
      description: 'Benutzerkonten mit Registrierung und Anmeldung sowie Seiten, die nur angemeldeten Personen zugänglich sind.',
    },
    'demo-game': {
      title: 'Ein Mehrspieler-Spiel',
      description: 'Eine Anwendung, die mehrere Personen gleichzeitig live im Browser nutzen.',
    },
    'demo-files': {
      title: 'Dateien und Bilder teilen',
      description: 'Bilder oder Dokumente hochladen, die anschließend für alle sichtbar sind.',
    },
    'demo-live': {
      title: 'Live-Aktualisierungen',
      description: 'Eine Seite, die sich bei jeder Änderung automatisch aktualisiert – für Abstimmungen, Punktestände oder ein Dashboard.',
    },
    empty: {
      title: 'Leeres Lambda',
      description: 'Beginnen Sie mit einem leeren Lambda und setzen Sie Ihr eigenes Vorhaben um.',
    },
  },

  addressTitle: 'Adresse festlegen',
  fromDemo: (title) => <>{title} – Ihr Lambda startet als Kopie der Demo und kann vollständig angepasst werden.</>,
  fromNothing: 'Ihr Lambda startet leer und kann nach Ihren Vorstellungen aufgebaut werden.',
  pickAgain: 'Andere Vorlage wählen',
  publicKey: 'Öffentlicher Schlüssel',
  free: (key) => `„${key}“ ist verfügbar.`,
  keyHint: 'Kleinbuchstaben, Ziffern und Bindestriche, mindestens drei Zeichen. Ohne Eingabe wird ein zufälliger Schlüssel vergeben.',
  accept: 'Ich akzeptiere die Nutzungsbedingungen',
  fullTerms: 'Vollständige Nutzungsbedingungen lesen',
  back: 'Zurück',
  creating: 'Wird erstellt …',
  submit: 'Lambda erstellen',
  keepLink: 'Im nächsten Schritt erhalten Sie Ihren Editor-Link. Er ist der einzige Zugang – bitte bewahren Sie ihn sicher auf.',
  failed: 'Das Lambda konnte nicht erstellt werden.',
};
