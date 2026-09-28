import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Lambda erstellen',
  whatTitle: 'Was möchten Sie bauen?',
  whatText:
    'Wählen Sie, was am besten passt. Sie starten mit einer Kopie von etwas, das schon funktioniert – und können alles daran ändern. Oder Sie fangen bei null an.',
  seeIt: 'Live ansehen',
  startFrom: 'Damit starten',
  starters: {
    'demo-crud': {
      title: 'Dinge im Blick behalten',
      description: 'Eine Liste zum Ergänzen, Ändern und Abhaken – für Aufgaben, Notizen, Lesezeichen oder ein kleines Inventar.',
    },
    'demo-registration': {
      title: 'Registrierung und Login',
      description: 'Konten, mit denen sich Leute registrieren und anmelden, und Seiten, die nur sie sehen.',
    },
    'demo-game': {
      title: 'Gemeinsam spielen',
      description: 'Mehrere Leute spielen gleichzeitig, live im Browser.',
    },
    'demo-files': {
      title: 'Dateien und Bilder teilen',
      description: 'Leute laden Bilder oder Dokumente hoch, und alle anderen sehen sie.',
    },
    'demo-live': {
      title: 'Live zeigen, was passiert',
      description: 'Eine Seite, die sich sofort aktualisiert, wenn sich etwas ändert – Stimmen, Punkte, ein Dashboard.',
    },
    empty: {
      title: 'Etwas anderes',
      description: 'Starten Sie mit einem leeren Lambda und bauen Sie, was Sie vorhaben.',
    },
  },

  addressTitle: 'Adresse wählen',
  fromDemo: (title) => <>{title} – Ihr Lambda startet als Kopie der Demo, und Sie können alles daran ändern.</>,
  fromNothing: 'Ihr Lambda startet leer – bereit für Ihre Idee.',
  pickAgain: 'Etwas anderes wählen',
  publicKey: 'Öffentlicher Schlüssel',
  free: (key) => `„${key}“ ist frei.`,
  keyHint: 'Kleinbuchstaben, Ziffern und Bindestriche, mindestens drei Zeichen. Leer lassen für einen zufälligen.',
  accept: 'Ich akzeptiere die Nutzungsbedingungen',
  fullTerms: 'Nutzungsbedingungen vollständig lesen',
  back: 'Zurück',
  creating: 'Wird erstellt …',
  submit: 'Lambda erstellen',
  keepLink: 'Auf dem nächsten Bildschirm sehen Sie Ihren Editor-Link. Er ist der einzige Weg zurück – heben Sie ihn gut auf.',
  failed: 'Das Lambda konnte nicht erstellt werden.',
};
