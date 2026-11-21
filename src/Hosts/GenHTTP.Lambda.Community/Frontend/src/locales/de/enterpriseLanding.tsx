import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Apps für Ihre Arbeit',
  headline: 'Beschreiben Sie die App, die Ihre Arbeit braucht.',
  headlineAccent: 'Ein KI-Agent baut sie und stellt sie hier online.',
  intro:
    'Ein Formular, ein Buchungskalender, eine Liste für Ihr Team: Sagen Sie in Ihren eigenen Worten, was die App tun soll. Nach wenigen Minuten haben Sie einen Link, den Sie mit Ihren Kolleginnen und Kollegen teilen können – und später ändern Sie die App auf dieselbe Weise, ohne Code zu schreiben.',
  describe: 'App beschreiben',
  own: 'Eigenen KI-Agenten verwenden',

  examplesTitle: 'Was hier gebaut wird',
  examples: [
    { title: 'Formulare und Anträge', text: 'Urlaub, Einkäufe, Feedback: Die Mitarbeitenden füllen es aus, und wer entscheidet, genehmigt es.' },
    { title: 'Buchungen', text: 'Besprechungsräume, Arbeitsplätze, Geräte: wer was hat und wann.' },
    { title: 'Listen und Übersichten', text: 'Support-Anfragen, Bestand, Onboarding: was offen ist und wer daran arbeitet.' },
    { title: 'Seiten für ein Team', text: 'Ein Dienstplan, ein Handbuch, Neuigkeiten: ein Link, den alle öffnen.' },
  ],

  stepsTitle: 'So funktioniert es',
  steps: [
    {
      title: 'Beschreiben',
      text: 'Sagen Sie, was die App tun soll, so wie Sie es einer Kollegin oder einem Kollegen erklären würden. Die Ideen unter dem Eingabefeld sind ein Anfang.',
    },
    {
      title: 'Link teilen',
      text: 'Wenige Minuten später ist sie hier online. Senden Sie den Link an die Personen, die sie nutzen, und bewahren Sie Ihren Editor-Link auf: Damit ändern Sie die App.',
    },
    {
      title: 'Mehr anfordern',
      text: 'Im selben Feld: etwas Neues, etwas anderes oder wie es läuft – wer sie genutzt hat und was eingegeben wurde. Frühere Versionen bleiben erhalten, sodass die vorherige wieder online gestellt werden kann.',
    },
  ],

  questionsTitle: 'Gut zu wissen',
  questions: (showcase) => [
    ['Muss ich programmieren können?', 'Nein. Sie beschreiben, was die App tun soll, und der Agent schreibt sie. Den Code müssen Sie sich nie ansehen.'],
    ['Wer kann meine App öffnen?', 'Wer ihren Link hat und diese Website erreichen kann. Senden Sie ihn an die Personen, die sie nutzen sollen.'],
    ['Wo werden die Eingaben gespeichert?', 'Jede App speichert ihre eigenen Einträge auf dieser Installation, die Ihre Organisation betreibt.'],
    [
      'Wie komme ich zu meiner App zurück?',
      'Dieser Browser merkt sich die Apps, die Sie hier gebaut haben, neben dem Feld, in dem Sie sie gebaut haben. Öffnen Sie auf einem anderen Gerät den Editor-Link, den Sie mit der App erhalten haben – bewahren Sie ihn sicher auf, denn niemand kann ihn wiederherstellen.',
    ],
    [
      'Etwas funktioniert nicht. Was nun?',
      'Sagen Sie dem Agenten, was nicht stimmt, so wie Sie die App angefordert haben. Wenn eine Änderung alles verschlechtert hat, stellen Sie im Editor der App die vorherige Version wieder online.',
    ],
    ['Was haben Kolleginnen und Kollegen gebaut?', <>Die Apps, die sie zeigen möchten, finden Sie im {showcase('Showcase')}.</>],
  ],
};
