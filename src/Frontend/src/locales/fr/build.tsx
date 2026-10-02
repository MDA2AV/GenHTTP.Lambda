import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Créer un site web avec l’IA.',
  intro:
    'Décrivez avec vos propres mots le site web ou l’application que vous avez en tête. L’IA le crée pour vous, nous l’hébergeons, et il est en ligne en quelques minutes, avec un lien à envoyer à qui vous voulez. Gratuit, sans coder, sans inscription.',
  placeholder: 'Je voudrais un site web qui…',
  shortcut: 'Ctrl + Entrée',
  buildIt: 'Créer mon site',
  builtBy: 'Créé par',
  password: 'mot de passe',
  fable:
    'Fable est protégé par un mot de passe pendant la phase de test. Il n’a pas de limite de temps : il continue jusqu’à ce que votre site web soit fini, pas jusqu’à la fin du chrono.',
  onlyNew:
    'Ici, vous créez de nouveaux sites. Pour modifier un site existant, ouvrez son lien d’édition et décrivez ce qui doit changer sous « Modifier ».',
  ideas: [
    'un site pour notre association où les membres s’inscrivent aux événements',
    'une liste pour notre auberge espagnole, pour que personne n’apporte le même plat',
    'un livre d’or pour notre mariage',
    'un sondage où chacun vote et voit les résultats',
    'un classement pour notre soirée quiz hebdomadaire',
    'une page d’anniversaire où les amis laissent leurs vœux',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Un site passe avant le vôtre : vous êtes le suivant.' : `${waiting} sites passent avant le vôtre.`,
  starting: 'Démarrage…',
  asked: 'Votre demande',
  leaveOpen: 'Gardez cette page ouverte. Le lien pour modifier votre site plus tard ne s’affiche qu’ici, une fois qu’il est prêt.',
  log: 'Ce que l’IA a fait',
  online: 'Votre site est en ligne',
  notOnline: 'Votre site a été créé, mais il n’a pas été mis en ligne.',
  open: 'Ouvrir votre site',
  steps: {
    guide: 'Préparation',
    examples: 'Coup d’œil à des exemples',
    create: 'Choix d’une adresse pour votre site',
    write: 'Écriture de votre site',
    improve: 'Amélioration de votre site',
    check: 'Recherche d’erreurs',
    online: 'Mise en ligne',
    trying: 'Essai du site',
    looking: 'Relecture de votre site',
    forRecords: 'Préparation de l’espace pour ses entrées',
    forKeys: 'Préparation de l’espace pour les clés et mots de passe',
    forFiles: 'Préparation de l’espace pour ce qu’il enregistre',
    records: 'Coup d’œil à ses entrées',
    keys: 'Vérification des clés et mots de passe nécessaires',
    addFile: 'Ajout d’un fichier',
    removeFile: 'Suppression d’un fichier',
    files: 'Coup d’œil à ce qu’il a enregistré',
  },

  points: [
    {
      title: 'Sans savoir coder',
      text: 'Dites avec vos propres mots ce que votre site doit faire, comme vous l’expliqueriez à un ami. L’IA le crée pour vous, sans aucune compétence technique.',
    },
    {
      title: 'Hébergement gratuit inclus',
      text: 'Votre site tourne sur nos serveurs. Pas d’hébergeur ni de serveur à payer, pas de nom de domaine à acheter, rien à installer : la sécurité et les mises à jour sont prises en charge.',
    },
    {
      title: 'En ligne en quelques minutes',
      text: 'Vous recevez tout de suite un lien à partager. Le site retient ce que les gens y saisissent (inscriptions, votes, messages, scores), pour que tout le monde voie la même chose.',
    },
  ],

  questionsTitle: 'Avant de commencer',
  questions: (offline, removed) => [
    [
      'L’IA peut-elle vraiment créer mon site web gratuitement ?',
      `Oui. Décrivez-le avec vos propres mots : l’IA le crée, le met en ligne et vous donne le lien. Sans inscription, sans carte bancaire, sans période d’essai. Il reste en ligne tant qu’il est utilisé : après ${offline} jours sans visite ni modification, il est mis hors ligne, puis supprimé après ${removed} jours.`,
    ],
    [
      'Faut-il un hébergeur, un serveur ou un nom de domaine ?',
      'Non. Votre site tourne sur nos serveurs, hébergement, sécurité et mises à jour compris. Vous recevez un lien tout de suite : pas de nom de domaine à acheter non plus.',
    ],
    [
      'Peut-on créer une application sans savoir coder ?',
      'Oui. Vous ne voyez jamais de code. Dites ce qu’elle doit faire, comme vous l’expliqueriez à un ami, et l’IA s’occupe du reste : un site web, une petite application ou un jeu.',
    ],
    [
      'Les gens peuvent-ils s’inscrire, voter, laisser un message ?',
      'Oui. Votre site retient ce que les gens y saisissent : tous ceux qui ouvrent le lien voient les mêmes inscriptions, votes et scores.',
    ],
    [
      'Comment les autres l’ouvrent-ils ?',
      'Avec le lien, dans n’importe quel navigateur, sur téléphone comme sur ordinateur. Rien à installer, et pas de boutique d’applications à passer.',
    ],
    [
      'Comment le modifier plus tard ?',
      'Ouvrez le lien d’édition reçu avec votre site et décrivez ce qui doit changer, comme ici. Si une modification ne vous plaît pas, vous pouvez revenir à ce qu’il y avait avant.',
    ],
  ],

  yourApp: 'Votre site',
  further: 'Pour le modifier plus tard',
  keep:
    'Gardez bien ce lien. C’est le seul moyen d’y revenir, et personne ne peut le récupérer, pas même nous. Ajoutez-le à vos favoris avant de fermer cet onglet.',
  change:
    'Pour modifier votre site, ouvrez le lien d’édition et décrivez ce qui doit changer sous « Modifier », comme ici. Votre propre assistant IA peut aussi le faire, comme expliqué ci-dessous.',
  copyLink: 'Copier le lien d’édition',
  lifetime: (offline, removed) =>
    `Nous le gardons en ligne tant qu’il est utilisé : après ${offline} jours sans visite ni modification, il est mis hors ligne, puis supprimé après ${removed} jours. Ouvrez l’éditeur pour le remettre en ligne.`,
  openEditor: 'Ouvrir l’éditeur',
  another: 'Créer un autre site',

  keepGoing: 'Continuez avec votre propre assistant IA',
  orOwn: 'Ou utilisez votre propre assistant IA',
  ownText:
    'Vous utilisez déjà Claude ou un autre assistant IA ? Connectez-le ici : il crée et modifie des sites pour vous de la même façon. Nous les hébergeons, vous n’avez donc toujours rien à configurer. Aucune limite quotidienne.',
  ownTitle: 'Créez votre site avec votre assistant IA',
  ownOnly:
    'Connectez Claude ou un autre assistant IA à l’adresse ci-dessous, puis décrivez le site souhaité. Il le crée, nous l’hébergeons sur nos serveurs, et il est en ligne aussitôt, avec un lien à partager.',
  thenAsk:
    'Dites-lui ensuite ce que vous souhaitez, par exemple : « Crée un site pour notre chorale avec le calendrier de nos concerts. »',
  howToChange:
    'C’est aussi ainsi que vous modifiez un site plus tard : donnez à votre assistant le lien d’édition et dites-lui ce qui doit changer.',

  failedToStart: 'La demande n’est pas passée.',
  noAnswer: 'C’est terminé, mais sans dire ce qui s’est passé.',
  failed: 'Ça n’a pas marché.',
};
