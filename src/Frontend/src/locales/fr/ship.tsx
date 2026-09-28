import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'De votre machine à tous les écrans.',
  intro:
    'Vous avez créé quelque chose avec votre agent de code, mais ça ne tourne que sur votre machine. Demandez-lui de le publier ici. Quelques minutes plus tard, vous avez un lien public que tout le monde peut ouvrir. Et l’app garde ses données : on peut y jouer, discuter et poster à plusieurs.',
  facts: ['Gratuit', 'Sans compte', 'Rien à installer'],
  connect: 'Connecter votre agent',
  seeOthers: 'Voir ce que d’autres ont publié',

  stepsTitle: 'Trois étapes, dont une qui tient en une phrase',
  step: (n) => `Étape ${n}`,
  steps: [
    {
      title: 'Branchez votre agent',
      body: 'Ajoutez une adresse à Claude, Cursor ou l’agent de votre choix. Ça prend moins d’une minute, et vous ne le faites qu’une fois.',
    },
    {
      title: 'Demandez-lui de publier',
      body: 'Dites-lui de mettre l’app en ligne ici. Il la prépare, la publie et vérifie qu’elle répond.',
    },
    {
      title: 'Partagez le lien',
      body: 'Vous recevez une adresse publique et un lien d’édition privé. Envoyez la première à qui vous voulez. Gardez le second : c’est lui qui vous permet de modifier l’app plus tard.',
    },
  ],

  togetherTitle: 'Pas juste une page. Un endroit où l’on se retrouve.',
  together:
    'La plupart des hébergeurs donnent à chaque visiteur sa propre copie de l’app, et chacun joue dans son coin. Ici, chaque app a sa propre mémoire et une connexion en direct avec tous ceux qui l’ont ouverte. Ce que fait une personne apparaît aussitôt chez les autres, et ce qu’on y poste est encore là le lendemain.',
  together2:
    'Pas d’abonnement à une base de données, pas de second service à brancher. Demandez-le comme vous l’expliqueriez à un ami.',
  kinds: [
    { name: 'Jeux multijoueurs', ask: 'Permets à huit amis max de rejoindre la même partie et de voir les coups des autres en direct.' },
    { name: 'Salons de chat', ask: 'Ajoute un salon où tous ceux qui ont le lien peuvent discuter, et garde les cent derniers messages.' },
    { name: 'Listes partagées', ask: 'Rends la liste d’affaires à emporter modifiable par toute l’équipe en même temps.' },
    { name: 'Scores et records', ask: 'Tiens un classement avec le meilleur temps de chacun et affiche le top 10 sur l’écran d’accueil.' },
    { name: 'Mini réseaux sociaux', ask: 'Permets aux invités du mariage de poster leurs photos sur un mur commun et de liker celles des autres.' },
  ],
  quote: (text) => `« ${text} »`,

  connectTitle: 'Connectez votre agent, une fois pour toutes',
  connectText: 'Donnez cette adresse à votre agent. Il saura ensuite publier ici, sans clé ni connexion.',
  sayLike: 'Ensuite, dans votre projet, dites par exemple',
  asks: [
    'Publie cette app sur GenHTTP Lambda et envoie-moi le lien.',
    'Mets les meilleurs scores en commun, pour que tout le monde voie le même classement.',
  ],

  domainChip: 'Quand ça décolle',
  domainTitle: 'Donnez-lui son propre nom',
  domainText:
    'La même app, le même lien d’édition, mais à une adresse qui vous appartient. Plus facile à dire, plus facile à retenir, et ça fait sérieux quand on commence à la partager.',
  domainSubject: 'Un nom de domaine pour mon app',
  domainAsk: 'Parlons de votre domaine',

  questionsTitle: 'Questions fréquentes',
  questions: (offline, removed, showcase, terms) => [
    [
      'C’est vraiment gratuit ?',
      <>
        Oui. Pas de carte bancaire, pas d’essai, pas de compte. Votre app reste en ligne tant qu’on l’utilise. Après{' '}
        {offline} jours sans la moindre visite ni modification, elle est mise hors ligne, et après {removed} jours, elle
        est supprimée.
      </>,
    ],
    [
      'Faut-il coder mon app d’une façon particulière ?',
      'Non, votre agent s’en occupe. Les pages, les images et les styles sont publiés tels quels. Tout ce qui doit tourner côté serveur, l’agent l’adapte à la plateforme. Vous décrivez ce que l’app doit faire, il se charge de la traduction.',
    ],
    [
      'Comment la modifier ensuite ?',
      'Avec le lien d’édition reçu à la publication. Donnez-le à votre agent avec la prochaine modification, ou ouvrez-le dans votre navigateur. Chaque modification que vous demandez devient une version à part entière, à la même adresse, et vous pouvez revenir à une ancienne à tout moment.',
    ],
    [
      'Qui peut voir mon app ?',
      <>
        Tous ceux à qui vous donnez le lien. Elle n’est listée nulle part, sauf si vous choisissez de l’ajouter à la{' '}
        {showcase('vitrine')}.
      </>,
    ],
    [
      'Y a-t-il des choses que je ne peux pas publier ?',
      <>
        Quelques-unes, comme tout ce qui nuit aux gens ou les trompe. Les {terms('conditions')} sont courtes et écrites
        simplement.
      </>,
    ],
  ],

  closeTitle: 'Ça marche chez vous.',
  closeAccent: 'Faites que ça marche chez eux.',
  noAgent: 'Pas d’agent ? Créez votre app ici',
  closeFacts: 'Gratuit. Sans compte. Rien à installer.',

  scene: {
    label: 'On demande à un agent de publier une app. L’adresse passe de localhost à un lien public, et des gens arrivent.',
    ask: 'Mets mon jeu de quiz en ligne pour que mes potes puissent jouer.',
    live: 'C’est en ligne. Voici votre lien.',
    publishing: 'Publication…',
    public: 'Public',
    onlyYou: 'Privé',
    app: 'Quiz du vendredi soir',
    playing: (count) => <>{count} en jeu</>,
    you: 'Vous',
  },

  compareTitle: 'Le chemin le plus court entre « ça marche » et « essaie ça »',
  compareText:
    'Vercel, Cloudflare et Lovable sont d’excellents endroits pour faire tourner vos projets. Mais tout commence par un formulaire d’inscription. Et dès que votre app doit partager quoi que ce soit entre visiteurs, il faut configurer un second service. Voici ce que ça donne quand on part de zéro.',
  rows: [
    'Démarrer sans compte',
    'Publier depuis votre agent habituel',
    'Données partagées en direct : chat, multijoueur, records',
    'Coût du premier lien',
  ],
  us: ['Oui', 'Une connexion, puis on demande', 'Intégré à chaque app', 'Gratuit'],
  rivals: [
    ['Inscription requise', 'Après connexion à ses outils', 'Base de données à ajouter', 'Offre gratuite'],
    ['Inscription requise', 'Après connexion à ses outils', 'Possible, avec configuration', 'Offre gratuite'],
    ['Inscription requise', 'Dans son propre éditeur', 'Via un backend connecté', 'Offre gratuite, crédits limités'],
  ],
  compareNote:
    'Situation en septembre 2026, pour quelqu’un qui n’a de compte nulle part. Les offres et fonctionnalités des autres services évoluent : vérifiez les détails auprès d’eux.',

  yourAgent: 'Votre agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Lancez ceci une fois dans un terminal. Tous les projets que vous ouvrirez ensuite pourront publier ici.',
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
