import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Koppel je agent',
  title: 'Publiceer vanuit de agent die je al gebruikt.',
  intro:
    'Claude Code, Codex, Cursor of elke agent die MCP spreekt krijgt deze installatie als tools: hij bouwt een app, probeert een wijziging in een concept, zet hem online en leest de logs. Elke app antwoordt op een eigen adres, met een eigen database.',
  stepsTitle: 'Drie stappen naar een app online',
  step: (n) => `Stap ${n}`,
  steps: [
    {
      title: 'Eén keer koppelen',
      body: 'Voeg het adres van de MCP-server van deze installatie toe aan je agent. Een sleutel is niet nodig: wie de editorsleutel van een app heeft, kan die app aanpassen, en niets anders.',
    },
    {
      title: 'Vraag hem te bouwen of te publiceren',
      body: 'Beschrijf een nieuwe app, of vraag hem die in je project te publiceren. Hij schrijft de code, controleert of die compileert en zet hem online.',
    },
    {
      title: 'Bewaar de editorlink',
      body: 'Je krijgt het adres van de app en zijn editorlink. Het adres is voor iedereen; met de editorlink pas jij - of je agent - de app later aan.',
    },
  ],
  sayLike: 'Zeg dan bijvoorbeeld',
  asks: ['Bouw een gastenboek, met een pagina waar ik de berichten goedkeur.', 'Publiceer deze app hier en stuur me de link.'],
  noAgent: 'Geen agent? Gebruik de bouwagent',

  questionsTitle: 'Goed om te weten',
  questions: (create) => [
    [
      'Welke agents kunnen koppelen?',
      'Elke agent die een externe MCP-server via HTTP kan toevoegen: Claude Code, Claude, Codex, Cursor, VS Code en andere, op het adres hierboven.',
    ],
    [
      'Hoe pas ik een app later aan?',
      'Geef je agent bij de volgende wijziging de editorlink van de app, of open de link zelf: de editor toont de versies, de code, de items en de logs. Een wijziging wordt eerst in een concept geprobeerd, met een kopie van de data, voordat hij online gaat.',
    ],
    [
      'Kan ik met git aan een app werken?',
      'Ja. Elke app is een git-repository: kloon hem vanuit de editor, push een commit naar main om een versie op te slaan, of push een branch om een concept met een eigen preview te starten.',
    ],
    [
      'Waar komen API-sleutels en wachtwoorden?',
      'Niet in de code. De agent leest ze op naam, en jij voert de waarden in de editor in, waar niemand ze terug kan lezen - de agent ook niet.',
    ],
    [
      'Kan ik een app schrijven zonder agent?',
      <>Ja: {create('schrijf de code zelf')}, te beginnen met een van de demo’s. Een app is een C#-handler van GenHTTP, op deze server gecompileerd.</>,
    ],
  ],
};
