import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Pas activé ici',
  off: (write, mcp) => (
    <>
      Cette installation n’a pas d’agent de création. Vous pouvez quand même {write('écrire le code vous-même')}, ou
      brancher votre propre Claude sur {mcp}.
    </>
  ),

  title: 'Dites ce que vous voulez.',
  intro:
    'Votre app est créée, mise en ligne, et vous recevez un lien à envoyer à qui vous voulez. Sans compte, rien à installer. Et elle garde ses données (scores, messages, inscriptions) : tous ceux qui l’ouvrent voient la même chose.',
  placeholder: 'crée un…',
  working: 'en cours…',
  shortcut: 'Ctrl + Entrée',
  building: 'Création…',
  buildIt: 'Créer',
  builtBy: 'Avec',
  password: 'mot de passe',
  fable:
    'Fable est protégé par un mot de passe pendant la phase de test. Il n’a pas de limite de temps : il continue jusqu’à ce que l’app soit finie, pas jusqu’à la fin du chrono.',
  onlyNew:
    'Ici, on ne crée que de nouvelles apps. Pour faire évoluer une app existante, donnez son lien d’édition à votre propre agent de code (voir plus bas).',
  ideas: [
    'un mur où chacun peut laisser un message d’une ligne',
    'un tableau des meilleurs scores pour un jeu de dés',
    'un sondage où chacun vote et voit les résultats',
    'un livre d’or pour mon mariage',
    'un compte à rebours vers une date, visible par tous',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Une création avant la vôtre. Vous passez juste après.' : `${waiting} créations en attente avant la vôtre.`,
  starting: 'Démarrage…',

  yourApp: 'Votre app',
  further: 'Pour aller plus loin',
  keep: 'Gardez bien ce lien. C’est le seul moyen d’y revenir, et personne ne peut le récupérer, pas même nous. Ajoutez-le à vos favoris avant de fermer cet onglet.',
  change:
    'Cette page sert uniquement à créer. Pour modifier cette app, branchez votre propre agent de code (voir plus bas), donnez-lui le lien d’édition et dites-lui ce que vous voulez changer.',
  copyLink: 'Copier le lien d’édition',
  lifetime: (offline, removed) =>
    `Elle reste en ligne tant qu’on l’utilise : après ${offline} jours sans visite ni modification, elle passe hors ligne, et après ${removed} jours, elle est supprimée. Pour la remettre en ligne, ouvrez l’éditeur et cliquez sur Déployer.`,
  openEditor: 'Ouvrir l’éditeur',
  another: 'Créer autre chose',

  keepGoing: 'Continuer avec votre agent',
  orOwn: 'Ou utilisez votre propre agent',
  ownText:
    'Le champ ci-dessus, c’est un Claude qui tourne sur ce serveur. Si vous avez déjà le vôtre, branchez-le ici : il sait faire la même chose (créer une lambda, écrire le code, la mettre en ligne), sans limite quotidienne et sans passer par cette page.',
  thenAsk: 'Ensuite, demandez-lui ce que vous voulez, comme vous le feriez ici.',
  claudeWeb: 'Claude sur le web',
  claudeWebHow:
    'Paramètres, puis « Connectors », puis « Add custom connector ». Collez l’adresse ci-dessus comme URL de serveur MCP distant. Pas de clé, pas de connexion.',
  howToChange:
    'C’est aussi comme ça qu’on modifie une app déjà créée : donnez le lien d’édition à votre agent et dites-lui quoi faire.',
  more: 'En savoir plus sur les agents',

  failedToStart: 'La demande n’est pas passée.',
  noAnswer: 'C’est terminé, mais sans dire ce qui s’est passé.',
  failed: 'Ça n’a pas marché.',
};
