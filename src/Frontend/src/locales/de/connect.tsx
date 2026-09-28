import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Adresse für Ihren Agenten',
  editorLink: 'Editor-Link – für Ihren Agenten, für niemanden sonst',
  yourAgent: 'Ihr Agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Einmal im Terminal ausführen. Danach kann Claude Code hier aus jedem Projekt heraus Apps erstellen und ändern.',
    claude: (strong) => (
      <>
        Öffnen Sie in Claude (Web oder Desktop) die {strong('Einstellungen')}, dann {strong('Connectors')}, und wählen Sie{' '}
        {strong('Add custom connector')}. Adresse oben einfügen, speichern, fertig.
      </>
    ),
    cursor: 'Fügen Sie das in die MCP-Einstellungen von Cursor oder in die Datei unten ein und laden Sie neu.',
    vscode: 'Speichern Sie das in Ihrem Projekt und starten Sie den Server in der MCP-Ansicht von Copilot Chat.',
  },
  elsewhere:
    'Sie nutzen etwas anderes? Windsurf, Codex, Zed und die meisten anderen Agenten können in ihren Einstellungen einen Remote-MCP-Server hinzufügen. Nutzen Sie dafür die Adresse oben.',
};
