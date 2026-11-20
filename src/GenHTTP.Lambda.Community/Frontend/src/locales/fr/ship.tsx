import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Connectez votre agent',
  title: 'Publiez depuis l’agent que vous utilisez déjà.',
  intro:
    'Claude Code, Codex, Cursor ou tout agent compatible MCP dispose de cette installation comme d’outils : il crée une app, essaie une modification dans un brouillon, la met en ligne et lit ses journaux. Chaque app répond à une adresse à elle, avec sa propre base de données.',
  stepsTitle: 'Trois étapes pour une app en ligne',
  step: (n) => `Étape ${n}`,
  steps: [
    {
      title: 'Connectez-vous une fois',
      body: 'Ajoutez à votre agent l’adresse du serveur MCP de cette installation. Aucune clé n’est nécessaire : qui détient la clé d’édition d’une app peut modifier cette app, et rien d’autre.',
    },
    {
      title: 'Demandez-lui de créer ou de publier',
      body: 'Décrivez une nouvelle app, ou demandez-lui de publier celle de votre projet. Il écrit le code, vérifie qu’il compile et le met en ligne.',
    },
    {
      title: 'Gardez le lien d’édition',
      body: 'Vous recevez l’adresse de l’app et son lien d’édition. L’adresse est pour tout le monde ; le lien d’édition est ce qui vous permet, à vous ou à votre agent, de modifier l’app plus tard.',
    },
  ],
  sayLike: 'Dites ensuite quelque chose comme',
  asks: ['Crée un livre d’or, avec une page où j’approuve les entrées.', 'Publie cette app ici et envoie-moi le lien.'],
  noAgent: 'Pas d’agent ? Utilisez l’agent de création',

  questionsTitle: 'Bon à savoir',
  questions: (create) => [
    [
      'Quels agents peuvent se connecter ?',
      'Tout agent capable d’ajouter un serveur MCP distant en HTTP : Claude Code, Claude, Codex, Cursor, VS Code et d’autres, à l’adresse ci-dessus.',
    ],
    [
      'Comment modifier une app plus tard ?',
      'Donnez à votre agent le lien d’édition de l’app avec la prochaine modification, ou ouvrez le lien vous-même : l’éditeur montre ses versions, son code, ses entrées et ses journaux. Une modification est essayée dans un brouillon, avec une copie des données, avant d’être mise en ligne.',
    ],
    [
      'Puis-je travailler sur une app avec git ?',
      'Oui. Chaque app est un dépôt git : clonez-la depuis son éditeur, poussez un commit sur main pour enregistrer une version, ou poussez une branche pour démarrer un brouillon avec son propre aperçu.',
    ],
    [
      'Où vont les clés d’API et les mots de passe ?',
      'Pas dans le code. L’agent les lit par leur nom, et vous saisissez les valeurs dans l’éditeur, où personne ne peut les relire – l’agent non plus.',
    ],
    [
      'Puis-je écrire une app sans agent ?',
      <>Oui : {create('écrivez le code vous-même')}, en partant d’une des démos. Une app est un handler GenHTTP en C#, compilé sur ce serveur.</>,
    ],
  ],
  profiles: {
    enterprise: {
      asks: [
        'Crée un formulaire de demande de congés, avec une page où le chef d’équipe approuve chaque demande.',
        'Publie cette app ici et envoie-moi le lien.',
      ],
    },
  },
};
