import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'De votre ordinateur à tous les écrans.',
  intro:
    'Vous avez créé une application avec votre agent de programmation, mais elle ne fonctionne que sur votre machine. Demandez à l’agent de la publier ici. Quelques minutes plus tard, elle dispose d’un lien public accessible à tous et peut conserver des données : plusieurs personnes peuvent ainsi y jouer, discuter et publier ensemble.',
  facts: ['Gratuit', 'Sans compte', 'Sans installation'],
  connect: 'Connecter votre agent',
  seeOthers: 'Voir les applications publiées',

  stepsTitle: 'Trois étapes, dont une simple phrase',
  step: (n) => `Étape ${n}`,
  steps: [
    {
      title: 'Connexion unique',
      body: 'Ajoutez une adresse à Claude, Cursor ou à l’agent de votre choix. Cela prend moins d’une minute et ne se fait qu’une seule fois.',
    },
    {
      title: 'Demande de publication',
      body: 'Demandez à l’agent de mettre l’application en ligne ici. Il la prépare, la publie et vérifie qu’elle répond.',
    },
    {
      title: 'Partage du lien',
      body: 'Vous recevez une adresse publique et un lien d’édition privé. La première se partage librement ; conservez le second, il vous permettra de modifier l’application.',
    },
  ],

  togetherTitle: 'Plus qu’une page : un espace partagé.',
  together:
    'La plupart des hébergeurs servent une copie distincte de l’application à chaque visiteur, et chacun l’utilise seul. Ici, chaque application dispose de sa propre mémoire et d’une connexion en direct avec toutes les personnes qui l’ont ouverte. Une action d’un utilisateur apparaît immédiatement chez les autres, et les contributions sont conservées.',
  together2:
    'Aucune base de données à souscrire, aucun service supplémentaire à intégrer. Décrivez simplement la fonctionnalité souhaitée.',
  kinds: [
    { name: 'Jeux multijoueurs', ask: 'Permets à huit personnes au plus de rejoindre la même partie et de voir les coups des autres en direct.' },
    { name: 'Salons de discussion', ask: 'Ajoute un salon où toutes les personnes disposant du lien peuvent échanger, et conserve les cent derniers messages.' },
    { name: 'Listes partagées', ask: 'Transforme la liste de préparatifs en une liste que toute l’équipe peut modifier simultanément.' },
    { name: 'Scores et records', ask: 'Tiens un classement des meilleurs temps et affiche les dix premiers sur l’écran d’accueil.' },
    { name: 'Petits réseaux communautaires', ask: 'Permets aux invités du mariage de publier des photos sur un mur commun et de les apprécier.' },
  ],
  quote: (text) => `« ${text} »`,

  connectTitle: 'Connectez votre agent une seule fois',
  connectText:
    'Communiquez cette adresse à votre agent. Il saura ensuite publier ici, sans clé ni connexion.',
  sayLike: 'Ensuite, dans votre projet, formulez par exemple',
  asks: [
    'Publie cette application sur GenHTTP Lambda et envoie-moi le lien.',
    'Rends les meilleurs scores communs, pour que tout le monde voie le même classement.',
  ],

  domainChip: 'Pour les applications qui se développent',
  domainTitle: 'Un nom de domaine dédié',
  domainText:
    'La même application et le même lien d’édition, mais à une adresse qui vous appartient : plus simple à communiquer, plus facile à retenir et plus professionnelle lorsqu’elle est partagée.',
  domainSubject: 'Un nom de domaine pour mon application',
  domainAsk: 'Demander un nom de domaine',

  questionsTitle: 'Questions fréquentes',
  questions: (offline, removed, showcase, terms) => [
    [
      'Le service est-il réellement gratuit ?',
      <>
        Oui. Ni carte bancaire, ni période d’essai, ni compte. Votre application reste en ligne tant qu’elle est utilisée.
        Après {offline} jours sans visite ni modification, elle est mise hors ligne, puis supprimée après {removed} jours.
      </>,
    ],
    [
      'Mon application doit-elle respecter une structure particulière ?',
      'Non, votre agent s’en charge. Les pages, images et feuilles de style sont publiées telles quelles, et l’agent adapte à cette plateforme tout ce qui doit s’exécuter côté serveur. Vous décrivez le comportement attendu ; l’agent se charge de la mise en œuvre.',
    ],
    [
      'Comment la modifier par la suite ?',
      'Avec le lien d’édition reçu lors de la publication. Transmettez-le à votre agent avec la modification souhaitée, ou ouvrez-le dans votre navigateur. Chaque modification devient une nouvelle version à la même adresse, et vous pouvez revenir à une version antérieure à tout moment.',
    ],
    [
      'Qui peut voir mon application ?',
      <>
        Toute personne à qui vous transmettez le lien. Elle n’est répertoriée nulle part, sauf si vous choisissez de
        l’ajouter à la {showcase('vitrine')}.
      </>,
    ],
    [
      'Certains contenus sont-ils interdits ?',
      <>
        Oui, notamment tout ce qui nuit aux personnes ou les trompe. Les {terms('conditions d’utilisation')} sont brèves
        et rédigées simplement.
      </>,
    ],
  ],

  closeTitle: 'Elle fonctionne sur votre machine.',
  closeAccent: 'Rendez-la accessible à tous.',
  noAgent: 'Pas d’agent ? Créer ici',
  closeFacts: 'Gratuit. Sans compte. Sans installation.',

  scene: {
    label:
      'Un agent reçoit une demande de publication. L’adresse passe de localhost à un lien public, et des personnes rejoignent l’application.',
    ask: 'Mets mon jeu de quiz en ligne pour que mes amis puissent participer.',
    live: 'L’application est en ligne. Voici votre lien.',
    publishing: 'Publication…',
    public: 'Public',
    onlyYou: 'Local',
    app: 'Soirée quiz du vendredi',
    playing: 'en ligne',
    you: 'Vous',
  },

  compareTitle: 'Le chemin le plus court de « ça fonctionne » à « essayez-le »',
  compareText:
    'Vercel, Cloudflare et Lovable sont d’excellentes plateformes d’hébergement. Elles exigent toutefois une inscription et, dès que votre application doit partager des données entre visiteurs, la configuration d’un service supplémentaire. Voici la comparaison pour un démarrage sans compte existant.',
  rows: [
    'Démarrer sans compte',
    'Publier depuis l’agent que vous utilisez déjà',
    'Données partagées en direct : chat, multijoueur, records',
    'Coût jusqu’au premier lien',
  ],
  us: ['Oui', 'Une connexion, puis une simple demande', 'Intégré à chaque application', 'Gratuit'],
  rivals: [
    ['Inscription requise', 'Via ses propres outils, après connexion', 'Service de base de données à ajouter', 'Offre gratuite'],
    ['Inscription requise', 'Via ses propres outils, après connexion', 'Possible, avec configuration', 'Offre gratuite'],
    ['Inscription requise', 'Dans son propre éditeur', 'Via un backend connecté', 'Offre gratuite, crédits limités'],
  ],
  compareNote:
    'Situation en septembre 2026, pour une personne sans compte existant. Les offres et fonctionnalités des autres services évoluent ; veuillez vous référer à leurs informations.',

  yourAgent: 'Votre agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Exécutez cette commande une fois dans un terminal. Chaque projet ouvert ensuite pourra publier ici.',
    claude: (strong) => (
      <>
        Dans Claude sur le web ou sur ordinateur, ouvrez les {strong('Paramètres')}, puis {strong('Connectors')}, et
        choisissez {strong('Add custom connector')}. Collez l’adresse ci-dessus et enregistrez. Aucune autre étape n’est
        nécessaire.
      </>
    ),
    cursor: 'Ajoutez ceci aux paramètres MCP de Cursor, ou au fichier ci-dessous, puis rechargez.',
    vscode: 'Enregistrez ce fichier dans votre projet, puis démarrez le serveur depuis la vue MCP de Copilot Chat.',
  },
  elsewhere:
    'Vous utilisez un autre outil ? Windsurf, Codex, Zed et la plupart des autres agents permettent d’ajouter un serveur MCP distant dans leurs paramètres. Indiquez-leur l’adresse ci-dessus.',
};
