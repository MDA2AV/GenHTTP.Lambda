import type { ReactNode } from 'react';

/**
 * How to connect an agent of one's own over MCP. The same words wherever
 * the site explains it - the front page, /ship, /build and the Change
 * section of the editor - so they are kept apart from any one page.
 */
export const connect = {
  address: 'Address for your agent',
  editorLink: 'Editor link - give it to your agent, and to nobody else',
  yourAgent: 'Your agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Run this once in a terminal. From then on, Claude Code can create and change apps here from any project.',
    claude: (strong: (text: string) => ReactNode) => (
      <>
        In Claude on the web or on your desktop, open {strong('Settings')}, then {strong('Connectors')}, and choose{' '}
        {strong('Add custom connector')}. Paste the address above and save. No key, no sign-in.
      </>
    ),
    cursor: 'Add this to the MCP settings of Cursor, or to the file below, and reload.',
    vscode: 'Save this in your project, then start the server from the MCP view of Copilot Chat.',
  },
  elsewhere:
    'Using something else? Windsurf, Codex, Zed and most other agents can add a remote MCP server in their settings. Give them the address above.',
};
