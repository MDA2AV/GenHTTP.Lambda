import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'De l’idée au site web.',
  intro:
    'Décrivez le site web ou l’application que vous avez en tête. L’IA le crée pour vous, nous l’hébergeons sur nos serveurs, et il est en ligne aussitôt, avec un lien à envoyer à qui vous voulez. Sans programmer, sans hébergement à configurer, sans compte.',
  placeholder: 'Je voudrais un site web qui…',
  working: 'en cours…',
  shortcut: 'Ctrl + Entrée',
  building: 'Création',
  buildIt: 'Créer mon site',
  builtBy: 'Créé par',
  password: 'mot de passe',
  fable:
    'Fable est protégé par un mot de passe pendant la phase de test. Il n’a pas de limite de temps : il continue jusqu’à ce que l’app soit finie, pas jusqu’à la fin du chrono.',
  onlyNew:
    'Ici, vous créez de nouveaux sites. Pour modifier un site existant, ouvrez son lien d’édition et décrivez ce qui doit changer sous « Modifier ».',
  ideas: [
    'un site pour notre association où les membres s’inscrivent aux événements',
    'un livre d’or pour notre mariage',
    'un sondage où chacun vote et voit les résultats',
    'un tableau des scores pour notre soirée quiz hebdomadaire',
    'un compte à rebours jusqu’à notre ouverture, visible par tous',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Un site passe avant le vôtre : vous êtes le suivant.' : `${waiting} sites passent avant le vôtre.`,
  starting: 'Démarrage…',

  points: [
    {
      title: 'Décrit, pas programmé',
      text: 'Dites avec vos propres mots ce que votre site doit faire. Aucune compétence technique ni en programmation n’est nécessaire.',
    },
    {
      title: 'Hébergement inclus',
      text: 'Votre site tourne sur nos serveurs. Hébergement, sécurité et mises à jour sont pris en charge : vous n’avez rien à configurer ni à entretenir.',
    },
    {
      title: 'En ligne en quelques minutes',
      text: 'Vous recevez tout de suite un lien à partager. Le site peut aussi retenir des données (inscriptions, votes, scores) pour que tout le monde voie la même chose.',
    },
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
