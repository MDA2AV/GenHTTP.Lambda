import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Adresse pour votre agent',
  editorLink: 'Lien d’édition : pour votre agent, et pour personne d’autre',
  yourAgent: 'Votre agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Exécutez cette commande une fois dans un terminal. Ensuite, Claude Code peut créer et modifier des apps ici depuis n’importe quel projet.',
    claude: (strong) => (
      <>
        Dans Claude sur le web ou sur ordinateur, ouvrez les {strong('Paramètres')}, puis {strong('Connectors')}, et
        choisissez {strong('Add custom connector')}. Collez l’adresse ci-dessus et enregistrez. C’est tout.
      </>
    ),
    cursor: 'Ajoutez ceci aux paramètres MCP de Cursor, ou au fichier ci-dessous, puis rechargez.',
    vscode: 'Enregistrez ce fichier dans votre projet, puis démarrez le serveur depuis la vue MCP de Copilot Chat.',
  },
  elsewhere:
    'Vous utilisez autre chose ? Windsurf, Codex, Zed et la plupart des autres agents peuvent ajouter un serveur MCP distant dans leurs paramètres. Donnez-leur l’adresse ci-dessus.',
};
