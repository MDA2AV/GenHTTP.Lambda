import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Indirizzo per il Suo agente',
  editorLink: 'Link di modifica: solo per il Suo agente, per nessun altro',
  yourAgent: 'Il Suo agente',
  terminal: 'Terminale',
  setups: {
    claudeCode: 'Esegua questo comando una volta nel terminale. Da lì in poi Claude Code può creare e modificare app qui da qualsiasi progetto.',
    claude: (strong) => (
      <>
        In Claude sul web o su desktop, apra {strong('Impostazioni')}, poi {strong('Connectors')}, e scelga{' '}
        {strong('Add custom connector')}. Incolli l’indirizzo qui sopra e salvi. Nessuna chiave, nessun accesso.
      </>
    ),
    cursor: 'Aggiunga questo alle impostazioni MCP di Cursor, o al file qui sotto, e ricarichi.',
    vscode: 'Salvi questo file nel Suo progetto, poi avvii il server dalla vista MCP di Copilot Chat.',
  },
  elsewhere:
    'Usa qualcos’altro? Windsurf, Codex, Zed e quasi tutti gli altri agenti permettono di aggiungere un server MCP remoto nelle impostazioni. Indichi loro l’indirizzo qui sopra.',
};
