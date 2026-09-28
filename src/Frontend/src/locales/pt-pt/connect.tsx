import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Endereço para o teu agente',
  editorLink: 'Link de edição: para o teu agente e para mais ninguém',
  yourAgent: 'O teu agente',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Executa isto uma vez num terminal. A partir daí, o Claude Code pode criar e alterar apps aqui a partir de qualquer projeto.',
    claude: (strong) => (
      <>
        No Claude na web ou no computador, abre as {strong('Definições')}, depois {strong('Connectors')}, e escolhe{' '}
        {strong('Add custom connector')}. Cola o endereço acima e guarda. Já está.
      </>
    ),
    cursor: 'Adiciona isto às definições de MCP do Cursor, ou ao ficheiro abaixo, e recarrega.',
    vscode: 'Guarda isto no teu projeto e depois inicia o servidor na vista MCP do Copilot Chat.',
  },
  elsewhere:
    'Usas outra coisa? O Windsurf, o Codex, o Zed e a maioria dos outros agentes permitem adicionar um servidor MCP remoto nas definições. Dá-lhes o endereço acima.',
};
