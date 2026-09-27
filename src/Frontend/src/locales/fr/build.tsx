import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Non disponible sur cette installation',
  off: (write, mcp) => (
    <>
      Cette installation ne dispose pas d’agent de création. Vous pouvez néanmoins {write('écrire le code vous-même')} ou
      connecter votre propre Claude à {mcp}.
    </>
  ),

  title: 'Décrivez votre besoin.',
  intro:
    'L’application est créée et mise en ligne, et vous recevez un lien à partager. Sans compte, sans installation – et elle peut conserver des données, comme des scores, des messages ou des inscriptions, afin que chacun voie le même état.',
  placeholder: 'crée un…',
  working: 'en cours…',
  shortcut: 'Ctrl + Entrée',
  building: 'Création en cours',
  buildIt: 'Créer',
  builtBy: 'Créé avec',
  password: 'mot de passe',
  fable:
    'Fable est protégé par un mot de passe pendant sa phase d’essai. Il fonctionne sans limite de temps et poursuit jusqu’à ce que l’application soit terminée.',
  onlyNew:
    'Cette page crée uniquement de nouvelles applications. Pour faire évoluer une application existante, confiez son lien d’édition à votre propre agent – voir ci-dessous.',
  ideas: [
    'un mur où chacun peut laisser un court message',
    'un tableau des scores pour un jeu de dés',
    'un sondage avec les résultats en direct',
    'un livre d’or pour notre mariage',
    'un compte à rebours partagé jusqu’à une date',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Une création avant la vôtre – vous êtes le suivant.' : `${waiting} créations avant la vôtre.`,
  starting: 'Démarrage…',

  yourApp: 'Votre application',
  further: 'Pour la faire évoluer',
  keep: 'Conservez ce lien précieusement. C’est le seul moyen d’y accéder, et il ne peut pas être récupéré – y compris par nous. Ajoutez-le à vos favoris avant de fermer cet onglet.',
  change:
    'Cette page crée uniquement de nouvelles applications. Pour modifier celle-ci, connectez votre propre agent comme indiqué ci-dessous, transmettez-lui le lien d’édition et décrivez la modification souhaitée.',
  copyLink: 'Copier le lien d’édition',
  lifetime: (offline, removed) =>
    `L’application reste en ligne tant qu’elle est utilisée : après ${offline} jours sans visite ni modification, elle est mise hors ligne, et supprimée après ${removed} jours. Pour la remettre en ligne, ouvrez l’éditeur et cliquez sur Déployer.`,
  openEditor: 'Ouvrir l’éditeur',
  another: 'Créer une autre application',

  keepGoing: 'Poursuivre avec votre propre agent',
  orOwn: 'Ou utilisez votre propre agent',
  ownText:
    'Le champ ci-dessus utilise un Claude hébergé sur ce serveur. Si vous disposez déjà de votre propre agent, vous pouvez le connecter ici : il dispose des mêmes possibilités – créer un lambda, écrire le code, le mettre en ligne – sans limite quotidienne et sans passer par cette page.',
  thenAsk: 'Décrivez ensuite votre besoin, comme vous le feriez ici.',
  claudeWeb: 'Claude sur le web',
  claudeWebHow:
    'Paramètres, puis « Connectors », puis « Add custom connector ». Collez l’adresse ci-dessus comme URL du serveur MCP distant. Aucune clé ni connexion n’est requise.',
  howToChange:
    'C’est également ainsi que l’on modifie une application existante : transmettez le lien d’édition à votre agent et décrivez la modification.',
  more: 'En savoir plus sur l’utilisation d’un agent',

  failedToStart: 'La demande n’a pas pu être lancée.',
  noAnswer: 'Le traitement s’est terminé sans retour d’information.',
  failed: 'L’opération a échoué.',
};
