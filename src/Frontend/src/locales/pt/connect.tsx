import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Endereço para o seu agente',
  editorLink: 'Link de edição: para o seu agente e para mais ninguém',
  yourAgent: 'Seu agente',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Rode isto uma vez em um terminal. A partir daí, o Claude Code pode criar e mudar apps aqui a partir de qualquer projeto.',
    claude: (strong) => (
      <>
        No Claude na web ou no desktop, abra {strong('Configurações')}, depois {strong('Connectors')}, e escolha{' '}
        {strong('Add custom connector')}. Cole o endereço acima e salve. Só isso.
      </>
    ),
    cursor: 'Adicione isto nas configurações de MCP do Cursor, ou no arquivo abaixo, e recarregue.',
    vscode: 'Salve isto no seu projeto e inicie o servidor pelo painel MCP do Copilot Chat.',
  },
  elsewhere:
    'Usa outra ferramenta? Windsurf, Codex, Zed e a maioria dos outros agentes aceitam um servidor MCP remoto nas configurações. Passe o endereço acima para eles.',
};
