import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Une plateforme de codage agentique',
  headline: 'Décrivez une application.',
  headlineAccent: 'Votre agent la met en ligne.',
  intro:
    'Sondages, livres d’or, classements, petites boutiques : décrivez votre besoin à notre agent ou à celui que vous utilisez déjà, et vous obtenez une application fonctionnelle accompagnée d’un lien à partager. L’application reste modifiable et peut évoluer bien après sa première version.',
  build: 'Créer une application',
  ownAgent: 'Utiliser votre propre agent',
  free: 'Gratuit. Sans compte, sans installation.',
  seeIt: 'Voir une démonstration',

  videoTitle: 'D’une phrase à une application en ligne',
  videoText:
    'Une fenêtre de navigation privée, aucun compte et une seule demande sur la page Créer – puis l’application terminée, ouverte depuis son lien, telle que la verra tout visiteur.',
  videoNote: 'La création est présentée en accéléré. Le reste est en temps réel.',
  tryIt: 'Essayer',

  oneShotTitle: 'Bien plus qu’un résultat ponctuel',
  oneShotText:
    'La plupart des générateurs livrent un résultat, et s’arrêtent là. Ici, l’application continue de fonctionner là où elle a été créée : vous et votre agent pouvez la faire évoluer à tout moment.',
  steps: [
    {
      title: 'Décrivez votre besoin',
      body: 'Formulez-le simplement, auprès de l’agent de ce site ou de celui que vous utilisez déjà. Sans code, sans configuration, sans compte.',
      alt: 'La page Créer, avec une demande de sondage pour le déjeuner',
    },
    {
      title: 'Une application fonctionnelle et un lien',
      body: 'L’application est créée, déployée et vous est remise sous forme d’adresse publique à partager. Elle conserve ses données – votes, scores, messages – afin que chaque utilisateur voie le même état.',
      alt: 'Le sondage terminé, ouvert dans un navigateur',
    },
    {
      title: 'Améliorez-la en continu',
      body: 'Chaque application dispose d’un lien d’édition privé. Transmettez-le à votre agent avec la prochaine modification, ou ouvrez-le vous-même. Chaque modification devient une nouvelle version, à la même adresse.',
      alt: 'Le centre de contrôle du sondage : ses versions, chacune avec la demande, la modification et la différence avec la précédente',
    },
  ],
  weekLater: 'Une semaine plus tard',
  weekAsk:
    'Voici le lien d’édition de mon sondage. Merci de clore le vote à 11 h le vendredi et d’afficher le résultat en haut de la page.',
  weekAnswer:
    'C’est fait. La version 4 est en ligne à la même adresse, et la version 3 reste disponible en cas de retour arrière.',

  agentsTitle: 'Utilisez l’agent de votre choix',
  agentsText:
    'Vous travaillez déjà avec Claude ou un autre assistant ? Connectez-le à cette adresse : il pourra créer, déployer et mettre à jour des applications ici, directement depuis votre conversation.',
  agents: [
    {
      name: 'Claude sur le web ou sur ordinateur',
      how: 'Ouvrez les Paramètres, puis « Connectors », et choisissez « Add custom connector ». Collez l’adresse ci-dessus – aucune clé d’API ni connexion n’est requise.',
    },
    {
      name: 'Claude Code',
      how: 'Exécutez une fois cette commande dans un terminal :',
    },
    {
      name: 'Autres clients MCP',
      how: 'Cursor, VS Code, Codex et les autres clients MCP prennent en charge les serveurs distants. Configurez-les avec la même adresse.',
    },
  ],
  thenAsk: (em) => (
    <>Il suffit ensuite de demander : {em('crée une liste d’inscription pour notre événement d’équipe et mets-la en ligne')}.</>
  ),

  contactTitle: 'Nous contacter',
  contactText:
    'Vous avez besoin d’aide, préparez un projet d’envergure ou recherchez une solution sur mesure ? Nous serons heureux d’échanger avec vous.',
  mailTitle: 'Par e-mail',
  mailText: 'Pour vos projets, vos demandes et tout sujet que vous préférez aborder en privé.',
  discordTitle: 'Rejoindre le Discord',
  discordText: 'Présentez vos réalisations, obtenez de l’aide et échangez directement avec l’équipe.',
  discordLink: 'Le Discord GenHTTP',

  terms: 'Conditions d’utilisation',
  writeCode: 'Écrire le code vous-même',
  contact: 'Contact',
};
