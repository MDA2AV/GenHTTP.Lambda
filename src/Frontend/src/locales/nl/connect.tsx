import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Adres voor je agent',
  editorLink: 'Editorlink – voor je agent, en voor niemand anders',
  yourAgent: 'Je agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Voer dit één keer uit in een terminal. Daarna kan Claude Code hier vanuit elk project apps maken en aanpassen.',
    claude: (strong) => (
      <>
        Ga in Claude op het web of op je desktop naar {strong('Instellingen')}, dan naar {strong('Connectors')}, en
        kies {strong('Add custom connector')}. Plak het adres hierboven en sla op. Meer hoeft niet.
      </>
    ),
    cursor: 'Voeg dit toe aan de MCP-instellingen van Cursor, of aan het bestand hieronder, en herlaad.',
    vscode: 'Sla dit op in je project en start de server vanuit de MCP-weergave van Copilot Chat.',
  },
  elsewhere:
    'Gebruik je iets anders? Windsurf, Codex, Zed en de meeste andere agents kunnen in hun instellingen een remote MCP-server toevoegen. Geef ze het adres hierboven.',
};
