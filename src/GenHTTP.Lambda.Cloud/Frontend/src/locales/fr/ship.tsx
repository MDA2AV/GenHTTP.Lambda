import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hébergement gratuit pour le vibe coding',
  title: 'De localhost à tous les écrans.',
  intro:
    'Vous avez créé une app avec Claude Code, Codex ou Cursor, mais elle ne tourne que sur votre machine. Demandez à votre agent de la mettre en ligne ici. Quelques minutes plus tard, elle a un lien public que tout le monde peut ouvrir, sa propre base de données et une connexion en direct avec tous ceux qui l’ont ouverte : on peut y jouer, discuter et poster à plusieurs.',
  facts: ['Gratuit', 'Sans inscription', 'Sans carte bancaire', 'Rien à installer'],
  connect: 'Connecter votre agent',
  seeOthers: 'Voir ce que d’autres ont publié',

  stepsTitle: 'De localhost à un lien public en trois étapes',
  step: (n) => `Étape ${n}`,
  steps: [
    {
      title: 'Branchez votre agent',
      body: 'Ajoutez une adresse, celle d’un serveur MCP distant, à Claude Code, Codex, Cursor ou l’agent de votre choix. Ça prend moins d’une minute, et vous ne le faites qu’une fois.',
    },
    {
      title: 'Demandez-lui de publier',
      body: 'Dites-lui de mettre l’app en ligne ici. Il la prépare, la déploie et vérifie qu’elle répond. Pas de dépôt GitHub, pas de pipeline de déploiement, pas de Docker.',
    },
    {
      title: 'Partagez le lien',
      body: 'Vous recevez une adresse publique et un lien d’édition privé. Envoyez la première à qui vous voulez. Gardez le second : c’est lui qui vous permet de modifier l’app plus tard.',
    },
  ],

  togetherTitle: 'Plus qu’un hébergement : base de données et multijoueur intégrés.',
  together:
    'La plupart des hébergeurs donnent à chaque visiteur sa propre copie de l’app, et chacun joue dans son coin : ce qu’un navigateur garde dans son localStorage, le suivant ne le voit jamais. Ici, chaque app a sa propre base de données et une connexion en direct avec tous ceux qui l’ont ouverte. Ce que fait une personne apparaît aussitôt chez les autres, et ce qu’on y poste est encore là le lendemain.',
  together2:
    'Pas de compte Supabase ou Firebase à créer, pas de backend à brancher, pas de serveur à louer. Demandez-le comme vous l’expliqueriez à un ami.',
  kinds: [
    { name: 'Jeux multijoueurs', ask: 'Permets à huit amis max de rejoindre la même partie et de voir les coups des autres en direct.' },
    { name: 'Salons de chat', ask: 'Ajoute un salon où tous ceux qui ont le lien peuvent discuter, et garde les cent derniers messages.' },
    { name: 'Listes partagées', ask: 'Rends la liste d’affaires à emporter modifiable par toute l’équipe en même temps.' },
    { name: 'Classements', ask: 'Tiens un classement avec le meilleur temps de chacun et affiche le top 10 sur l’écran d’accueil.' },
    { name: 'Mini réseaux sociaux', ask: 'Permets aux invités du mariage de poster leurs photos sur un mur commun et de liker celles des autres.' },
  ],
  quote: (text) => `« ${text} »`,

  connectTitle: 'Connectez Claude Code, Codex ou Cursor une fois pour toutes',
  connectText:
    'Donnez à votre agent cette adresse, celle de notre serveur MCP. Il saura ensuite publier ici, sans clé ni connexion.',
  sayLike: 'Ensuite, dans votre projet, dites par exemple',
  asks: [
    'Publie cette app sur GenHTTP Lambda et envoie-moi le lien.',
    'Mets les meilleurs scores en commun, pour que tout le monde voie le même classement.',
  ],

  domainChip: 'Quand ça décolle',
  domainTitle: 'Donnez-lui son propre nom de domaine',
  domainText:
    'La même app, le même lien d’édition, mais à une adresse qui vous appartient. Plus facile à dire, plus facile à retenir, et ça fait sérieux quand on commence à la partager.',
  domainSubject: 'Un nom de domaine pour mon app',
  domainAsk: 'Parlons de votre domaine',

  questionsTitle: 'Avant la mise en ligne',
  questions: (offline, removed, showcase, terms) => [
    [
      'C’est vraiment gratuit ?',
      <>
        Oui. Sans inscription, sans carte bancaire, sans période d’essai. Votre app reste en ligne tant qu’on l’utilise.
        Après {offline} jours sans la moindre visite ni modification, elle est mise hors ligne, et après {removed} jours,
        elle est supprimée.
      </>,
    ],
    [
      'Claude Code, Codex ou Cursor peuvent-ils déployer mon app ici ?',
      'Oui, comme tout autre agent capable d’ajouter un serveur MCP distant. Connectez-le une fois avec l’adresse ci-dessus, puis demandez-lui de publier : il déploie l’app, vérifie qu’elle répond et vous envoie le lien.',
    ],
    [
      'Pourquoi mes amis ne peuvent-ils pas ouvrir mon lien localhost ?',
      'Parce que localhost, c’est votre propre ordinateur : l’adresse ne marche que là, et seulement tant que l’app tourne. Un tunnel lui prête une adresse publique tant que votre portable reste allumé. Mise en ligne ici, l’app tourne sur nos serveurs, avec un lien qui marche encore quand votre portable est fermé.',
    ],
    [
      'Ai-je besoin d’un serveur, d’un backend ou de Supabase ?',
      'Non. Chaque app a sa propre base de données, son stockage de fichiers et une connexion en direct avec tous ceux qui l’ont ouverte. Pas de serveur à louer, pas de second service à configurer, et rien à faire tourner de votre côté non plus.',
    ],
    [
      'Puis-je rendre mon jeu multijoueur sans gérer de serveur ?',
      'Oui. Ce qu’un navigateur garde dans son localStorage, le suivant ne le voit jamais : la partie commune doit donc vivre sur un serveur, et ici, c’est le nôtre. Demandez à votre agent de rendre le jeu multijoueur, et chaque coup arrive chez tous ceux qui l’ont ouvert.',
    ],
    [
      'Faut-il coder mon app d’une façon particulière ?',
      'Non, votre agent s’en occupe. Les pages, les images et les styles sont publiés tels quels. Tout ce qui doit tourner côté serveur, l’agent l’adapte à la plateforme. Vous décrivez ce que l’app doit faire, il se charge de la traduction.',
    ],
    [
      'Comment la modifier ensuite ?',
      'Avec le lien d’édition reçu à la publication. Donnez-le à votre agent avec la prochaine modification, ou ouvrez-le dans votre navigateur. Chaque modification crée une nouvelle version à la même adresse, et vous pouvez revenir à une ancienne à tout moment.',
    ],
    [
      'Puis-je pousser dessus avec git ?',
      'Oui. Chaque app est aussi un dépôt git : clonez-la depuis l’adresse sous Cloner dans l’éditeur, modifiez-la avec vos propres outils ou votre agent, puis poussez. Chaque commit poussé sur main devient la prochaine version, et une branche poussée devient un brouillon avec son propre aperçu.',
    ],
    [
      'Où vont mes clés d’API ?',
      'Pas dans le code. Votre agent demande une clé par son nom, et vous saisissez la valeur dans l’éditeur. Personne ne peut la relire, ni l’éditeur, ni l’agent.',
    ],
    [
      'Puis-je emporter mon code ?',
      'Oui, il vous appartient. Téléchargez-le depuis l’éditeur quand vous voulez, sous forme de projet autonome, base de données comprise – ou clonez-le avec git, chaque version comprise.',
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
  closeFacts: 'Gratuit. Sans inscription. Rien à installer.',

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
    'Base de données et temps réel : chat, multijoueur, records',
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

};
