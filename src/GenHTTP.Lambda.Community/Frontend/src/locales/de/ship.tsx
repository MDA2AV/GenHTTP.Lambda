import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Agenten verbinden',
  title: 'Veröffentlichen Sie mit dem Agenten, den Sie schon nutzen.',
  intro:
    'Claude Code, Codex, Cursor oder jeder Agent, der MCP spricht, bekommt diese Installation als Werkzeuge: Er baut eine App, probiert eine Änderung in einem Entwurf aus, stellt sie online und liest ihre Logs. Jede App antwortet unter einer eigenen Adresse, mit einer eigenen Datenbank.',
  stepsTitle: 'Drei Schritte zur App online',
  step: (n) => `Schritt ${n}`,
  steps: [
    {
      title: 'Einmal verbinden',
      body: 'Tragen Sie die Adresse des MCP-Servers dieser Installation in Ihrem Agenten ein. Einen Schlüssel braucht es nicht: Wer den Editor-Schlüssel einer App hat, kann diese App ändern und sonst nichts.',
    },
    {
      title: 'Bauen oder veröffentlichen lassen',
      body: 'Beschreiben Sie eine neue App oder bitten Sie ihn, die App aus Ihrem Projekt zu veröffentlichen. Er schreibt den Code, prüft, ob er kompiliert, und stellt ihn online.',
    },
    {
      title: 'Editor-Link aufheben',
      body: 'Sie erhalten die Adresse der App und ihren Editor-Link. Die Adresse ist für alle; mit dem Editor-Link ändern Sie – oder Ihr Agent – die App später.',
    },
  ],
  sayLike: 'Sagen Sie dann etwa',
  asks: ['Bau ein Gästebuch mit einer Seite, auf der ich die Einträge freigebe.', 'Veröffentliche diese App hier und schick mir den Link.'],
  noAgent: 'Kein Agent? Nutzen Sie den Build-Agenten',

  questionsTitle: 'Gut zu wissen',
  questions: (create) => [
    [
      'Welche Agenten lassen sich verbinden?',
      'Jeder Agent, der einen entfernten MCP-Server über HTTP hinzufügen kann: Claude Code, Claude, Codex, Cursor, VS Code und andere, unter der Adresse oben.',
    ],
    [
      'Wie ändere ich eine App später?',
      'Geben Sie Ihrem Agenten bei der nächsten Änderung den Editor-Link der App oder öffnen Sie den Link selbst: Der Editor zeigt ihre Versionen, ihren Code, ihre Einträge und Logs. Eine Änderung wird mit einer Kopie der Daten in einem Entwurf ausprobiert, bevor sie online geht.',
    ],
    [
      'Kann ich mit git an einer App arbeiten?',
      'Ja. Jede App ist ein git-Repository: Klonen Sie sie aus dem Editor, pushen Sie einen Commit auf main, um eine Version zu speichern, oder pushen Sie einen Branch, um einen Entwurf mit eigener Vorschau zu starten.',
    ],
    [
      'Wohin gehören API-Schlüssel und Passwörter?',
      'Nicht in den Code. Der Agent liest sie über ihren Namen, und Sie tragen die Werte im Editor ein, wo sie niemand zurücklesen kann – auch der Agent nicht.',
    ],
    [
      'Kann ich eine App ohne Agenten schreiben?',
      <>Ja: {create('Schreiben Sie den Code selbst')}, ausgehend von einer der Demos. Eine App ist ein C#-GenHTTP-Handler, der auf diesem Server kompiliert wird.</>,
    ],
  ],
};
