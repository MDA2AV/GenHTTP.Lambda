import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Dirección para su agente',
  editorLink: 'Enlace de edición: para su agente y para nadie más',
  yourAgent: 'Su agente',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Ejecute esto una vez en un terminal. A partir de entonces, Claude Code puede crear y cambiar apps aquí desde cualquier proyecto.',
    claude: (strong) => (
      <>
        En Claude (web o escritorio), abra {strong('Configuración')}, luego {strong('Connectors')}, y elija{' '}
        {strong('Add custom connector')}. Pegue la dirección de arriba y guarde. Sin clave ni inicio de sesión.
      </>
    ),
    cursor: 'Añada esto a la configuración MCP de Cursor, o al archivo de abajo, y recargue.',
    vscode: 'Guarde esto en su proyecto y luego inicie el servidor desde la vista MCP de Copilot Chat.',
  },
  elsewhere:
    '¿Usa otra herramienta? Windsurf, Codex, Zed y la mayoría de los agentes pueden añadir un servidor MCP remoto desde su configuración. Indíqueles la dirección de arriba.',
};
