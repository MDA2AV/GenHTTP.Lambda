import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Plateforme de codage agentique',
  headline: 'Décrivez une app.',
  headlineAccent: 'Votre agent la met en ligne.',
  intro:
    'Sondages, livres d’or, classements, petites boutiques. Décrivez ce qu’il vous faut à notre agent, ou à celui que vous utilisez déjà. Vous recevez une app qui marche, avec un lien à partager. Et elle reste modifiable : vous pourrez la peaufiner bien après la première version.',
  build: 'Créer une app',
  ownAgent: 'Utiliser votre agent',
  free: 'Gratuit. Sans compte, rien à installer.',
  seeIt: 'Voir la démo',

  videoTitle: 'D’une phrase à une app en ligne',
  videoText:
    'Une fenêtre de navigation privée, aucun compte, une seule demande sur la page Créer. Puis l’app terminée, ouverte depuis son lien, comme la verra n’importe quel visiteur.',
  videoNote: 'La création est accélérée. Tout le reste est en temps réel.',
  tryIt: 'À vous de jouer',

  oneShotTitle: 'Pas du jetable',
  oneShotText:
    'La plupart des générateurs vous livrent un résultat, et c’est tout. Ici, l’app continue de tourner là où elle a été créée, et vous pouvez la faire évoluer avec votre agent.',
  steps: [
    {
      title: 'Dites ce que vous voulez',
      body: 'Avec vos propres mots, à l’agent du site ou au vôtre. Pas de code, rien à configurer, pas de compte.',
      alt: 'La page Créer, avec une demande de sondage pour choisir où déjeuner',
    },
    {
      title: 'Recevez une app et un lien',
      body: 'L’app est créée, déployée, et vous recevez une adresse publique à partager. Elle garde ses données (votes, scores, messages) : tous ceux qui l’ouvrent voient la même chose.',
      alt: 'Le sondage terminé, ouvert dans un navigateur',
    },
    {
      title: 'Continuez à l’améliorer',
      body: 'Chaque app a son lien d’édition privé. Donnez-le à votre agent avec la prochaine modification, ou ouvrez-le vous-même. Chaque modification crée une nouvelle version, et l’adresse ne change pas.',
      alt: 'Le tableau de bord du sondage : ses versions, chacune avec la demande, ce qui a changé et la différence avec la précédente',
    },
  ],
  weekLater: 'Une semaine plus tard',
  weekAsk:
    'Voici le lien d’édition de mon sondage du midi. Tu peux fermer le vote à 11 h le vendredi et afficher le gagnant en haut ?',
  weekAnswer:
    'C’est fait. La version 4 est en ligne, à la même adresse. La version 3 est toujours là si vous voulez revenir en arrière.',

  agentsTitle: 'Branchez votre agent préféré',
  agentsText:
    'Vous utilisez déjà Claude ou un autre assistant ? Connectez-le à cette adresse. Il pourra créer, déployer et modifier des apps ici, sans quitter la conversation en cours.',
  agents: [
    {
      name: 'Claude sur le web ou sur ordinateur',
      how: 'Ouvrez les paramètres, puis « Connectors », et choisissez « Add custom connector ». Collez l’adresse ci-dessus. Pas de clé d’API, pas de connexion.',
    },
    {
      name: 'Claude Code',
      how: 'Lancez cette commande une fois dans un terminal :',
    },
    {
      name: 'Autres clients MCP',
      how: 'Cursor, VS Code, Codex et les autres clients MCP acceptent les serveurs distants. Donnez-leur la même adresse.',
    },
  ],
  thenAsk: (em) => (
    <>
      Ensuite, il suffit de demander : {em('fais une liste d’inscription pour notre sortie d’équipe et mets-la en ligne')}.
    </>
  ),

  contactTitle: 'Parlons-en',
  contactText:
    'Besoin d’aide ? Un projet plus ambitieux ? Envie d’une solution faite pour vous ? Nous serons ravis de vous lire.',
  mailTitle: 'Écrivez-nous',
  mailText: 'Pour vos projets, vos questions, et tout ce que vous préférez aborder en privé.',
  discordTitle: 'Rejoignez le Discord',
  discordText: 'Montrez ce que vous avez créé, trouvez de l’aide pour la suite et discutez directement avec l’équipe.',
  discordLink: 'Le Discord GenHTTP',

  terms: 'Conditions d’utilisation',
  writeCode: 'Écrire le code vous-même',
  contact: 'Contact',
};
