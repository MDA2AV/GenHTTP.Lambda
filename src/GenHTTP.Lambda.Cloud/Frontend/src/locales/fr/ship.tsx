import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hébergement gratuit pour le vibe coding',
  title: 'De localhost à tous les écrans.',
  intro:
    'Vous avez créé une app avec Claude Code, Codex ou Cursor, mais elle ne tourne que sur votre machine. Demandez à votre agent de la mettre en ligne ici. Quelques minutes plus tard, elle a un lien public que tout le monde peut ouvrir, sa propre base de données et une connexion en direct avec tous ceux qui l’ont ouverte : on peut y jouer, discuter et poster à plusieurs.',
  facts: ['Gratuit', 'Sans inscription', 'Sans carte bancaire', 'Rien à installer'],

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
  sayLike: 'Ensuite, dans votre projet, dites par exemple',
  asks: [
    'Publie cette app sur GenHTTP Lambda et envoie-moi le lien.',
    'Mets les meilleurs scores en commun, pour que tout le monde voie le même classement.',
  ],

  questionsTitle: 'Avant la mise en ligne',
  questions: (offline, removed, showcase, terms, selfHosting) => [
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
      'Puis-je plutôt l’héberger sur mon propre serveur ?',
      <>
        Oui. GenHTTP Lambda est open source, et son édition Community est la même plateforme dans un seul conteneur
        Docker, pour votre propre serveur et votre propre agent. {selfHosting('Cloud ou auto-hébergement')} explique ce
        qui diffère.
      </>,
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
  noAgent: 'Pas d’agent ? Créez votre app ici',

};
