import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Comment ça marche',
  intro:
    'Vous écrivez un snippet C#. Ce qu’il renvoie est hébergé à une adresse publique, en HTTPS, en quelques secondes. Voici tout ce qu’il faut savoir, dans l’ordre où vous le découvrirez.',
  contents: 'Sommaire',

  parts: {
    what: 'Qu’est-ce qu’une lambda ?',
    first: 'Votre première lambda',
    editor: 'Le tableau de bord',
    why: 'Dire pourquoi',
    features: 'Modifier sans risque',
    files: 'Plusieurs fichiers',
    page: 'Servir une page',
    spa: 'Un front-end, étape par étape',
    storage: 'Les deux endroits où vivent les fichiers',
    keeping: 'Garder des données',
    sockets: 'WebSockets',
    limits: 'Ce que vous ne pouvez pas faire',
    away: 'Repartir avec votre code',
    agents: 'Laisser faire un agent',
  },

  what: [
    (k) => (
      <>
        Une lambda est un snippet qui renvoie un handler GenHTTP. La plateforme le compile, le charge et monte ce qu’il
        renvoie sous votre propre adresse. Pas de projet, pas de fichier de build, pas d’instruction {k.code('using')}.
        Tous les modules GenHTTP sont déjà importés pour vous.
      </>
    ),
    (k) => (
      <>
        Voilà une lambda complète. Déployée sur {k.code('/lambda/your-key/')}, elle répond « hello » à chaque requête.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Le snippet est une suite d’{k.em('instructions')}, pas une classe. Il finit par renvoyer quelque chose qui sait
      servir des requêtes : un handler, ou un builder de handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Cliquez sur {k.b('Créer ma lambda')}. Vous obtenez une adresse publique et une clé d’édition. Cette clé est le
        seul moyen d’y revenir : gardez-la. Personne ne pourra la récupérer pour vous.
      </>
    ),
    () => (
      <>
        Vous arrivez sur son tableau de bord, avec un petit service REST déjà écrit en guise de première version. Ce
        n’est qu’un point de départ.
      </>
    ),
    (k) => (
      <>
        Donnez la clé d’édition à un agent et dites-lui quoi construire : il écrit de nouvelles versions via{' '}
        {k.link('/#agents', 'MCP')}. Ou ouvrez {k.b('Code')} et écrivez le code vous-même : {k.b('Vérifier')} compile
        sans rien enregistrer et vous montre ce qu’en dit le compilateur, avec le fichier et la ligne.
      </>
    ),
    (k) => (
      <>
        Cliquez sur {k.b('Déployer')}. Votre lambda est en ligne. Avant ça, rien n’est accessible. Chaque nouveau
        déploiement prolonge sa durée en ligne.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Le lien d’édition ouvre un tableau de bord plutôt qu’une zone de texte : ici, l’essentiel du code est écrit par des
      agents, donc l’écran montre d’abord comment va votre lambda. La barre latérale contient la lambda (en ligne ou non,
      son adresse, et un bouton quand une version plus récente attend d’être mise en ligne) et ses sections. Tout ce qui
      sert rarement, comme changer l’adresse ou supprimer la lambda, se trouve dans le menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Vue d’ensemble', () => <>En ligne ou non, le nombre de requêtes du jour et d’échecs, la dernière modification, et la place qui reste.</>],
    [
      'Modifier',
      (k) => (
        <>
          Dites ce qui doit changer, et l’agent de ce serveur s’en charge sous vos yeux. Il travaille dans un
          brouillon, y essaie la modification, et l’intègre à la prochaine version une fois que ça marche.
          Désactivez {k.b('Mettre en ligne une fois terminé')} pour essayer vous-même le brouillon d’abord.
        </>
      ),
    ],
    ['Brouillons', () => <>Des modifications préparées à côté de la lambda : chacune s’essaie à sa propre adresse et s’intègre à la prochaine version une fois au point. Une fois ouvert, un brouillon a son propre code, ses propres données et ses propres logs.</>],
    ['Fichiers', () => <>Les fichiers d’une version : son code et ses assets, le programme lui-même. Un cadenas ou un globe indique si le public peut y accéder.</>],
    ['Données', () => <>Ce que la lambda garde pendant qu’elle tourne, partagé par toutes les versions : le workspace. Regardez ce qu’il contient, importez et supprimez des fichiers, ou désactivez-le.</>],
    ['Versions', () => <>Ce que chaque version a changé, ce qui avait été demandé, et la différence avec la précédente. C’est ici qu’on déploie ou qu’on revient en arrière, ou qu’on démarre un brouillon à partir de n’importe quelle version.</>],
    ['Déploiements', () => <>Ce qui était en ligne, quand, et ce qui l’a arrêté.</>],
    ['Stats', () => <>Requêtes, échecs, temps de réponse et chemins les plus demandés, sur la dernière heure ou les dernières 24 heures.</>],
    ['Logs', () => <>Ses requêtes, ce qu’elle affiche, et la stack trace de tout ce qui plante, en direct.</>],
    [
      'Code',
      (k) => (
        <>
          Pour écrire le code à la main. {k.b('Vérifier')} compile, {k.b('Enregistrer')} crée une version,{' '}
          {k.b('Déployer')} met en ligne. Dans un brouillon, {k.b('Enregistrer')} garde le code dans le brouillon
          et {k.b('Déployer l’aperçu')} le met en ligne à l’adresse du brouillon.{' '}
          {k.code('Ctrl-S')} enregistre ; {k.code('F12')} va à une déclaration.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Chaque section fonctionne de la même façon : son titre, un {k.b('ⓘ')} qui l’explique, ses actions à droite et, quand
      elle a plusieurs vues, une rangée d’onglets en dessous. Pour le code, les onglets sont ses fichiers.
    </>
  ),
  editorAside:
    'Le trafic et les logs sont gardés en mémoire, pour suivre ce qui se passe, pas pour archiver : un redémarrage du serveur les remet à zéro. Les versions et l’historique des déploiements, eux, sont enregistrés.',

  why: (k) => (
    <>
      Une version, c’est le code, plus deux notes facultatives : {k.b('la spécification')}, ce que veut l’utilisateur et
      pourquoi, si possible avec ses mots, et {k.b('le changement')}, une ligne sur ce que fait la version. Elles
      s’affichent à côté du diff dans l’historique des versions. Le {k.em('pourquoi')} reste ainsi à côté du{' '}
      {k.em('quoi')}, pour vous, et pour le prochain agent qui lira l’historique avant de toucher à quoi que ce soit.
    </>
  ),
  whySample: {
    specification: 'Un livre d’or que les gens peuvent signer ; les messages doivent survivre à un redémarrage',
    change: 'Garde les messages dans le workspace pour qu’ils survivent à un redémarrage',
  },
  why2: (k) => (
    <>
      Les agents passent les deux mêmes champs à {k.code('write_code')}. Dans {k.b('Code')}, l’enregistrement vous
      demande le changement. Les deux sont facultatifs. Trop longs, ils ne sont pas refusés mais coupés : à
      4 000 caractères pour la spécification, à 500 pour le changement. Un brouillon a ses deux notes à lui, et la
      version dans laquelle il est intégré les reprend.
    </>
  ),

  features: (k) => (
    <>
      Une version ne change plus une fois enregistrée, et c’est ce qui fait que chacune vaut la peine d’être gardée : on
      peut comparer n’importe laquelle, et la remettre en ligne exactement telle qu’elle était. Pour modifier une lambda
      que des gens utilisent, démarrez plutôt un {k.b('brouillon')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Démarrez-le dans {k.b('Brouillons')}, ou à partir de n’importe quelle version. C’est une copie du code et
        des assets de cette version, et des données de la lambda.
      </>
    ),
    (k) => (
      <>
        Modifiez-le autant de fois qu’il le faut, dans {k.b('Code')} ou en le demandant à l’agent.{' '}
        {k.b('Déployer l’aperçu')} le met en ligne à sa propre adresse, {k.code('/features/…/')}, avec sa propre copie
        des données. Les visiteurs de la lambda n’en voient rien, et rien de ce qu’il écrit n’atteint les données de
        la lambda.
      </>
    ),
    (k) => (
      <>
        Une fois au point, cliquez sur {k.b('Intégrer')} : il devient la prochaine version, avec ses notes, et passe
        en ligne tout de suite si vous le voulez. Le brouillon disparaît alors, avec son aperçu et sa copie des
        données.
      </>
    ),
  ],
  featureSample: 'Classement',
  featuresAside: () => (
    <>
      On peut travailler sur plusieurs brouillons à la fois. Seul un brouillon basé sur la version la plus récente
      peut être intégré : ainsi, une intégration n’annule jamais une version enregistrée après le démarrage du
      brouillon. Si un autre a été intégré avant, reportez ses changements (ou demandez-le à l’agent), puis basez le
      brouillon sur la version la plus récente. Rien ne s’intègre tout seul, et c’est voulu.
    </>
  ),

  files: (k) => (
    <>
      Les types n’ont pas besoin d’être sous le code qui les utilise. Dans {k.b('Code')}, cliquez sur {k.b('+')} à côté
      des fichiers : le nouveau fichier est compilé avec le snippet, dans le même namespace, donc rien à importer pour y
      accéder. Un nom sans extension est considéré comme du C#.
    </>
  ),

  page: 'Il y a deux façons de servir une page, et une troisième pour ce que les gens importent à côté.',
  inlineTitle: 'Une page, écrite dans le code',
  inline: 'Parfait pour quelque chose de petit. La page fait partie du snippet.',
  folderTitle: 'Un dossier de vrais fichiers',
  folder:
    'Ce qu’il vous faut dès qu’il y a une feuille de style et un script. Les fichiers s’ajoutent comme un fichier C#, et sont servis exactement tels que vous les avez écrits. Rien ne les compile.',
  workspaceTitle: 'Des fichiers importés, depuis les données',
  workspace:
    'Pour ce que les gens importent ou ce que la lambda crée (photos, documents), servi à côté de l’app. Pas pour les pages de l’app elle-même : leur place est dans un dossier de fichiers, où elles sont versionnées avec le code qui en a besoin.',

  spa: (k) => (
    <>
      La deuxième façon, en entier. Chaque démo sert sa page ainsi, depuis un dossier nommé {k.code('web')} : ouvrez{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} pour en lire une. Les démos sont en lecture seule ; leur clé d’édition
      est leur nom.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Dans {k.b('Code')}, cliquez sur {k.b('+')} à côté des fichiers et tapez {k.code('site/index.html')}. Un nom qui
        contient un slash place le fichier dans un dossier ; un nom avec une extension donne le type du fichier.
      </>
    ),
    (k) => (
      <>
        Ajoutez {k.code('site/app.css')} et {k.code('site/app.js')} de la même façon. Votre page les appelle par leur nom,
        comme dans {k.code('href="app.css"')}, car le dossier est la racine de ce qui est servi, pas une partie de
        l’adresse.
      </>
    ),
    (k) => (
      <>
        Pour tout ce qui n’est pas du texte, comme une image ou une police, ouvrez un fichier dans {k.code('site')} et
        cliquez sur le bouton d’import à côté des fichiers : le fichier arrive dans le même dossier. Un PNG ne se tape
        pas dans un éditeur de texte, c’est donc par là qu’il faut passer.
      </>
    ),
    (k) => <>Dans {k.code('lambda.cs')}, servez le dossier :</>,
    (k) => (
      <>
        Cliquez sur {k.b('Déployer')}. {k.code('site/index.html')} répond sur {k.code('/')}, {k.code('site/app.css')} sur{' '}
        {k.code('/app.css')}, et toute adresse qui ne correspond à aucun fichier renvoie la page. Un front-end qui gère
        son propre routage marche donc aussi quand quelqu’un recharge un lien profond.
      </>
    ),
    () => <>Ajoutez une API à côté, pour que la page ait à qui parler :</>,
  ],

  storage: (k) => (
    <>
      Une lambda garde des fichiers à deux endroits, et l’éditeur les montre séparément : {k.b('Fichiers')} contient les
      fichiers d’une version (le programme), et {k.b('Données')} contient le workspace (ce que le programme garde). La
      différence, c’est {k.em('à qui ils appartiennent')}. Les fichiers d’une version appartiennent à cette version ; les
      données appartiennent à la lambda, et toutes les versions les partagent.
    </>
  ),
  savedWithCode: 'Dans une version',
  workspaceColumn: 'Dans les données',
  table: [
    ['ce qu’il contient', 'le code et les assets : le programme, front-end compris', 'tout ce que la lambda écrit, ou que quelqu’un importe'],
    ['quand il change', 'jamais : une modification donne une nouvelle version', 'dès que quelque chose y est écrit'],
    ['un déploiement', 'met exactement ces fichiers en ligne', 'n’y touche jamais'],
    ['revenir en arrière', 'restaure les anciens fichiers', 'aucun effet : toutes les versions les partagent'],
    ['un brouillon', 'commence par une copie de ces fichiers', 'travaille sur une copie de ces données'],
    ['quand il disparaît', 'avec les anciennes versions, au-delà de la limite', 'avec la lambda, ou quand vous le désactivez'],
  ],
  reachedAs: 'accessible dans le code via',
  storageAside:
    'Impossible d’en faire un seul endroit. Sinon, soit un déploiement effacerait tout ce que votre lambda a écrit depuis, soit rien ne pourrait jamais être retiré de ce qu’elle publie. Un jeu qui tient un classement a besoin du second cas ; la page qu’il sert, du premier. La page va donc dans la version, et le classement dans les données.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} est un dossier privé que votre lambda peut lire et écrire. C’est là que va tout ce qui doit
      survivre à une requête, ou à un déploiement.
    </>
  ),
  keeping2: (k) => (
    <>
      Il y a aussi {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, et {k.code('Tree')}/{k.code('Files')}/{k.code('App')} pour le servir. Rien d’autre du
      système de fichiers n’est accessible.
    </>
  ),

  sockets: (k) => (
    <>
      Pris en charge, et pas à moitié. La démo {k.link('/editor/demo-game', 'demo-game')} forme des paires de joueurs et
      fait tourner chaque partie sur le serveur. La forme la plus simple tient en trois callbacks :
    </>
  ),
  socketsAside: (k) => (
    <>
      Un piège où tout le monde tombe : un navigateur ne peut pas définir d’en-têtes lors du handshake WebSocket. Passez
      ce dont le handler a besoin dans la query string, où il le lit via {k.code('connection.Request.Header.Query')}, ou
      envoyez les secrets dans le premier message.
    </>
  ),

  limits:
    'Votre code tourne sur un serveur partagé. Une partie de C# est donc refusée avant même la compilation : lancer des processus, ouvrir vos propres sockets, charger des assemblies, accéder au système de fichiers en dehors de votre workspace, et la réflexion utilisée pour contourner tout ça.',
  limits2:
    'Tout le reste est là, y compris toute l’API des modules GenHTTP. Si quelque chose est refusé, on vous dit quelle ligne et pourquoi, pas simplement que ça a échoué.',

  away: (k) => (
    <>
      Dans l’éditeur, {k.b('Télécharger en projet .NET')} vous donne le tout : une solution que
      vous pouvez ouvrir, lancer avec {k.code('dotnet run')}, et garder. Elle n’a besoin que du package GenHTTP, et
      fournit un {k.code('Dockerfile')} pour la construire et l’exécuter en conteneur.
    </>
  ),
  away2: (k) => (
    <>
      Votre snippet devient {k.code('Project.cs')}, et {k.code('Program.cs')} sert ce qu’il renvoie. Vos autres
      fichiers sont repris exactement tels quels. {k.code('Workspace')} et {k.code('Assets')} deviennent deux dossiers à
      côté du programme, avec les mêmes méthodes, à part dans un dossier {k.code('Platform')} : rien à changer dans votre code.
    </>
  ),
  awayAside:
    'Bon à savoir avant de construire quoi que ce soit ici : ce que vous écrivez vous appartient, et repart avec vous en entier. Le faire tourner sur cette machine ne vous enferme pas sur cette machine.',

  agents: (k) => (
    <>
      Un endpoint MCP est disponible sur {k.code('/mcp')}. Branchez-y un agent, et il peut faire tout ce que fait
      l’éditeur : lire le guide, lire une démo en entier, écrire des fichiers, les compiler et déployer. C’est la même API en
      dessous.
    </>
  ),
  agents2: (k) => (
    <>
      Il explique ses choix au fur et à mesure ({k.code('write_code')} prend la spécification et le changement) et il
      peut regarder ce qu’il a déployé : {k.code('read_logs')} renvoie les requêtes récentes de la lambda, ce qu’elle a
      affiché, et la stack trace de chaque exception levée. C’est comme ça qu’un agent vérifie que son code marche, au
      lieu de le supposer. Vous voyez la même chose dans le tableau de bord.
    </>
  ),
  more: 'En savoir plus →',
  make: 'Créer une lambda',
};
