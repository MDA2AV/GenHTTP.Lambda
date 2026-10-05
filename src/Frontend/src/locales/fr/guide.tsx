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
    written: 'Documentation et tests',
    features: 'Modifier sans risque',
    files: 'Plusieurs fichiers',
    page: 'Servir une page',
    spa: 'Un front-end, étape par étape',
    built: 'Ce à partir de quoi il est construit',
    storage: 'Les deux endroits où vivent les fichiers',
    database: 'Garder des enregistrements',
    keeping: 'Garder des fichiers',
    secrets: 'Clés et mots de passe',
    sockets: 'WebSockets',
    limits: 'Ce que vous ne pouvez pas faire',
    away: 'Repartir avec votre code',
    git: 'Travailler avec git',
    open: 'Publier le code',
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
    ['Vue d’ensemble', () => <>Ce qu’est l’application, en ligne ou non, le nombre de requêtes du jour et d’échecs, la dernière modification, et la place qui reste.</>],
    ['Documentation', () => <>Ce qu’est l’application, à qui elle s’adresse et pourquoi, et pourquoi elle est construite ainsi : rédigée par les agents, gardée avec chaque version.</>],
    [
      'Modifier',
      (k) => (
        <>
          Dites ce qui doit changer, et l’agent de ce serveur s’en charge sous vos yeux. Il essaie la modification
          dans un brouillon - une copie à sa propre adresse - et la met en ligne une fois qu’elle fonctionne.
          Désactivez {k.b('Mettre en ligne une fois terminé')} pour essayer vous-même le brouillon d’abord.
          Il ne travaille que sur votre app : une demande qui ne la concerne pas, ou qui vise à nuire, est refusée, et il dit pourquoi.
        </>
      ),
    ],
    ['Brouillons', () => <>Des modifications essayées avant d’être mises en ligne, chacune à sa propre adresse et sur ses propres données de test. Une fois ouvert, un brouillon a son propre code, ses propres données de test et ses propres logs. La section apparaît dès qu’il y a un brouillon.</>],
    ['Fichiers', () => <>Les fichiers d’une version : son code et ses assets, le programme lui-même. Un cadenas ou un globe indique si le public peut y accéder.</>],
    ['Données', () => <>Ce que la lambda garde pendant qu’elle tourne, partagé par toutes les versions : la base de données, le workspace et les secrets, chacun dans son onglet. Consultez les tables et les fichiers, envoyez des fichiers, définissez des secrets, ou activez et désactivez un type. La vue simple l’affiche dès que l’application garde quelque chose.</>],
    ['Versions', () => <>Ce que chaque version a changé, ce qui avait été demandé, et la différence avec la précédente. C’est ici qu’on déploie ou qu’on revient en arrière, ou qu’on démarre un brouillon à partir de n’importe quelle version.</>],
    ['Déploiements', () => <>Ce qui était en ligne, quand, et ce qui l’a arrêté.</>],
    ['Stats', () => <>Requêtes, échecs, temps de réponse et chemins les plus demandés, sur la dernière heure ou les dernières 24 heures.</>],
    ['Logs', () => <>Ses requêtes, ce qu’elle affiche, et la stack trace de tout ce qui plante, en direct.</>],
    [
      'Code',
      (k) => (
        <>
          Pour écrire le code à la main. {k.b('Vérifier')} compile, {k.b('Enregistrer')} crée une version,{' '}
          {k.b('Déployer')} met en ligne. Dans un brouillon, {k.b('Enregistrer')} garde le code dans le brouillon
          et l’affiche à l’adresse du brouillon.{' '}
          {k.code('Ctrl-S')} enregistre ; {k.code('F12')} va à une déclaration.
        </>
      ),
    ],
    ['Tests', () => <>Comment l’application est testée automatiquement, avec les scripts et les données de test prévus pour cela. Uniquement dans la vue complète.</>],
    ['Build', () => <>Ce à partir de quoi le code ou les assets sont construits là où un outil de build les produit, conservé avec chaque version, à lire plutôt qu’à modifier. Dans la vue complète, dès qu’une version le conserve.</>],
  ],
  sections: (k) => (
    <>
      Chaque section fonctionne de la même façon : son titre, un {k.b('ⓘ')} qui l’explique, ses actions à droite et, quand
      elle a plusieurs vues, une rangée d’onglets en dessous. Pour le code, les onglets sont ses fichiers. La vue
      complète range les sections en groupes : la façon dont on la trouve, là où se fait une modification, le
      programme et ses données, et son fonctionnement.
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
    change: 'Garde les messages dans la base de données pour qu’ils survivent à un redémarrage',
  },
  why2: (k) => (
    <>
      Les agents passent les deux mêmes champs à {k.code('write_code')}. Dans {k.b('Code')}, l’enregistrement vous
      demande le changement. Les deux sont facultatifs. Trop longs, ils ne sont pas refusés mais coupés : à
      4 000 caractères pour la spécification, à 500 pour le changement. Un brouillon a ses deux notes à lui, et la
      version dans laquelle il est intégré les reprend.
    </>
  ),

  written: (k) => (
    <>
      Chaque version garde, à côté de son programme, ce qui est écrit à son sujet : sa {k.b('documentation')} (ce
      qu’est l’application, à qui elle s’adresse et pourquoi, et pourquoi elle est construite ainsi) et ses{' '}
      {k.b('tests')}, qui disent comment vérifier automatiquement qu’elle fonctionne, avec les scripts et les données de
      test prévus pour cela. Les agents les rédigent avec une nouvelle lambda et les tiennent à jour à chaque
      modification. Le prochain agent qui modifie la lambda les lit d’abord : il sait ainsi à quoi sert l’application et
      ce qui doit continuer à marcher, ce que le code seul ne dit pas.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'ce qu’est l’application, à qui elle s’adresse, ce qu’on en fait et pourquoi'],
    ['.lambda/docs/decisions.md', 'les décisions techniques, et pourquoi elles ont été prises'],
    ['.lambda/tests/README.md', 'comment l’application est testée automatiquement, et comment lancer les tests'],
    ['.lambda/tests/…', 'les scripts et les données de test qu’utilisent les tests'],
  ],
  written2: (k) => (
    <>
      Ce sont des fichiers de la version comme les autres, dans le dossier {k.code('.lambda')} : l’historique montre ce
      qu’une version y a changé, revenir en arrière ramène la documentation qui valait pour cette version, et un
      brouillon en a sa propre copie, mise en ligne avec lui. Ils ne sont jamais compilés ni servis, et comptent dans la
      place que peuvent occuper les assets d’une version.
    </>
  ),
  written3: (k) => (
    <>
      Dans le tableau de bord, {k.b('Documentation')} montre les pages à lire, et {k.b('Tests')} comment l’application
      est testée, avec les fichiers qui l’accompagnent ; la version se choisit comme pour ses fichiers. Une page peut
      aussi y être modifiée, ce qui enregistre la version suivante. La vue simple appelle la documentation{' '}
      {k.b('À propos')} et ne montre que ce à quoi sert l’application : pour la corriger, dites-le à l’agent.
    </>
  ),
  writtenAside:
    'Ils sont rédigés dans la langue que vous utilisez avec l’agent, pour la personne ou l’agent qui modifiera l’application ensuite. Pas une copie du code : à quoi elle sert, et pourquoi.',

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
        Démarrez-le à partir de n’importe quelle version, dans {k.b('Versions')}, ou laissez l’agent en démarrer un. C’est
        une copie du code, des assets, de la documentation et des tests de cette version, et des données de la lambda.
      </>
    ),
    (k) => (
      <>
        Modifiez-le autant de fois qu’il le faut, dans {k.b('Code')} ou en le demandant à l’agent. Son aperçu répond à une
        adresse qui lui est propre, {k.code('/features/…/')}, avec ses propres données de test. Les visiteurs de la lambda
        n’en voient rien, et rien de ce qu’il écrit n’atteint les données de la lambda.
      </>
    ),
    (k) => (
      <>
        Une fois au point, cliquez sur {k.b('Mettre en ligne')} : il devient la prochaine version, avec ses notes, et passe
        en ligne. Le brouillon disparaît alors, avec son aperçu et ses données de test.
      </>
    ),
  ],
  featureSample: 'Classement',
  featuresAside: () => (
    <>
      On peut travailler sur plusieurs brouillons à la fois. Seul un brouillon à jour par rapport à la version la plus
      récente peut être mis en ligne, afin de ne jamais annuler une version enregistrée après son début. Si un autre a
      été mis en ligne avant, reportez ses modifications (ou demandez-le à l’agent), puis marquez le brouillon comme à
      jour. Rien ne passe en ligne tout seul, et c’est voulu. L’API appelle un brouillon une feature, et sa mise en ligne
      un merge.
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
  built: (k) => (
    <>
      Une partie d’une lambda peut être produite par un outil de build plutôt qu’écrite telle qu’elle est servie ou
      compilée : compilée, assemblée ou générée. La version contient ce que l’outil produit - comme assets, ou comme
      code - et à côté les fichiers à partir desquels il le produit, son {k.b('espace de développement')} :{' '}
      {k.code('.lambda/dev/')} dans la version, {k.code('dev/')} dans un clone, qui contient tout ce dont l’outil part.
      Votre agent modifie ces fichiers, lance le build là où il travaille et enregistre les deux dans la même version.
      Cette plateforme ne construit rien.
    </>
  ),
  built2: (k) => (
    <>
      Comme la documentation, il appartient à sa version : comparé dans l’historique, restauré, copié dans un
      brouillon, cloné, téléchargé et publié avec le code - et jamais compilé ni servi. Dans le tableau de bord,{' '}
      {k.b('Build')} l’affiche dès qu’une version en conserve un : comment il est construit, selon son README, ses
      fichiers, et si une version les a modifiés sans rien changer de ce qui en est construit. Il se lit là, sans s’y
      modifier - une modification se fait là où il est construit.
    </>
  ),
  builtAside:
    'Ce qui est écrit tel qu’il est servi ou compilé n’en a pas besoin. Ce qu’un build installe ou conserve pour lui-même - node_modules, par exemple - ne fait jamais partie d’une version : un .gitignore dans l’espace de développement l’en exclut.',

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
    ['ce qu’il contient', 'le code et les assets : le programme, front-end compris, ainsi que sa documentation, ses tests et ce à partir de quoi il est construit','tout ce que la lambda écrit, ou que quelqu’un importe'],
    ['quand il change', 'jamais : une modification donne une nouvelle version', 'dès que quelque chose y est écrit'],
    ['un déploiement', 'met exactement ces fichiers en ligne', 'n’y touche jamais'],
    ['revenir en arrière', 'restaure les anciens fichiers', 'aucun effet : toutes les versions les partagent'],
    ['un brouillon', 'commence par une copie de ces fichiers', 'travaille sur une copie de ces données'],
    ['quand il disparaît', 'avec les anciennes versions, au-delà de la limite', 'avec la lambda, ou quand vous le désactivez'],
  ],
  reachedAs: 'accessible dans le code via',
  storageAside:
    'Impossible d’en faire un seul endroit. Sinon, soit un déploiement effacerait tout ce que votre lambda a écrit depuis, soit rien ne pourrait jamais être retiré de ce qu’elle publie. Un jeu qui tient un classement a besoin du second cas ; la page qu’il sert, du premier. La page va donc dans la version, et le classement dans les données.',

  database: (k) => (
    <>
      Les enregistrements – entrées, comptes, commandes, votes – ont leur place dans la {k.b('base de données')} : une
      base SQLite propre à la lambda, que vous activez sous {k.b('Données')}. Le code ouvre une connexion avec{' '}
      {k.code('Database.GetConnection()')} et lit et écrit les données via{' '}
      {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, avec son propre contexte, qui mappe les
      tables :
    </>
  ),
  database2: (k) => (
    <>
      Ses tables sont créées par des {k.b('migrations')} : des fichiers SQL livrés avec la version dans{' '}
      {k.code('migrations/')}, appliqués dans l’ordre par {k.link('https://evolve-db.netlify.app/', 'Evolve')} au
      démarrage de la lambda – chacun une seule fois, si bien qu’une nouvelle version n’exécute que ce qui est nouveau.
      Ne modifiez jamais une migration déjà appliquée ; un changement de table, c’est le fichier suivant.
    </>
  ),
  database3: (k) => (
    <>
      Comme toutes les données, la base est partagée par toutes les versions, laissée intacte par les déploiements et
      les retours en arrière, et un brouillon travaille sur une copie. Sous {k.b('Données')}, vous voyez ses tables et
      leur contenu – la vue simple les appelle des entrées. {k.b('Télécharger en projet .NET')} l’emporte sous forme
      de simple fichier SQLite.
    </>
  ),
  databaseAside: (k) => (
    <>
      Créez un contexte là où vous en avez besoin, libérez-le ensuite, et utilisez-le de façon synchrone :{' '}
      {k.code('ToList')} et {k.code('SaveChanges')}, pas {k.code('ToListAsync')} et {k.code('SaveChangesAsync')}. Les
      tables sont créées par les migrations, jamais par Entity Framework. La démo{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} fait tout cela.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} est un dossier privé que votre lambda peut lire et écrire : l’endroit pour les fichiers –
      les images que quelqu’un importe, un document qu’elle produit, un modèle qu’elle charge. Les enregistrements ont
      leur place dans la base de données, et ce que l’on sait d’un fichier – qui l’a importé, quand – est aussi un
      enregistrement.
    </>
  ),
  keeping2: (k) => (
    <>
      Il y a aussi {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, et {k.code('Tree')}/{k.code('Files')}/{k.code('App')} pour le servir. Rien d’autre du
      système de fichiers n’est accessible.
    </>
  ),

  secrets: (k) => (
    <>
      Une clé d’API, un mot de passe ou un jeton a sa place dans les {k.b('secrets')}, pas dans le code – où chaque
      version, chaque téléchargement et chaque lecteur de l’historique l’aurait. Le code lit un secret par son nom :
    </>
  ),
  secrets2: (k) => (
    <>
      Activez les secrets sous {k.b('Données')} et définissez-y la valeur. Une fois enregistrée, elle n’est plus
      jamais affichée – ni à vous, ni à un agent ; vous pouvez seulement la remplacer. La liste indique quels noms le
      code lit sans qu’une valeur soit définie, et la vue d’ensemble les demande. {k.code('Secret.Exists')} indique
      si un secret est défini, pour du code qui s’en passe. Comme toutes les données, les secrets sont partagés par
      toutes les versions, et un brouillon travaille sur une copie.
    </>
  ),
  secretsAside: (k) => (
    <>
      Ils sont stockés chiffrés, avec une clé qui ne se trouve pas dans la base de données. Dans un projet téléchargé,{' '}
      {k.code('Secret.Read("NAME")')} lit la variable d’environnement {k.code('NAME')} – les valeurs elles-mêmes restent
      ici.
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
  sockets2: (k) => (
    <>
      Quand la page se contente d’écouter (un compteur, un fil d’actualité, un tableau des scores), les événements
      envoyés par le serveur sont plus simples : une seule longue réponse dans laquelle le serveur continue d’écrire, et
      que le navigateur rouvre tout seul en cas de coupure. La démo{' '}
      {k.link('/editor/demo-live', 'demo-live')} envoie ainsi chaque vote à tous ceux qui regardent. Dans les deux cas,
      c’est le serveur qui pousse ce qui a changé. Une page qui redemande toutes les quelques secondes envoie une
      requête à chaque fois, que quelque chose ait changé ou non, et reste malgré tout en retard.
    </>
  ),

  limits:
    'Votre code tourne sur un serveur partagé. Une partie de C# est donc refusée avant même la compilation : lancer des processus, ouvrir vos propres sockets, charger des assemblies, accéder au système de fichiers en dehors de votre workspace, et la réflexion utilisée pour contourner tout ça. De même pour l’attente d’une tâche avec .Result ou .Wait() au lieu de await : les requêtes s’exécutent sur un thread par cœur, et la tâche devrait se terminer sur le thread même qui l’attend.',
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
      {' '}{k.code('Secret')} y lit les variables d’environnement du même nom ; les valeurs restent ici. La
      documentation et les tests suivent dans {k.code('docs')} et {k.code('tests')}, et l’espace de développement dans{' '}
      {k.code('dev')}.
      {' '}{k.code('Database')} ouvre {k.code('database/database.db')}, que le téléchargement contient avec les
      enregistrements gardés par votre application.
    </>
  ),
  awayAside:
    'Bon à savoir avant de construire quoi que ce soit ici : ce que vous écrivez vous appartient, et repart avec vous en entier. Le faire tourner sur cette machine ne vous enferme pas sur cette machine.',
  git: (k) => (
    <>
      Chaque lambda est aussi un dépôt git. {k.b('Cloner')}, sur la vue d’ensemble du tableau de bord et à côté de son
      code, donne son adresse – celle de votre éditeur suivie du nom de l’app – et {k.code('git clone')} vous donne le
      projet que {k.b('Télécharger')} vous donne, chaque version étant un commit de {k.code('main')}, étiqueté{' '}
      {k.code('v1')}, {k.code('v2')} et ainsi de suite, et chaque brouillon une branche. Ouvrez-le dans votre propre
      éditeur, confiez-le à votre agent de code, lancez-le avec {k.code('dotnet run')}.
    </>
  ),
  git2: (k) => (
    <>
      Poussez, et c’est en place. Chaque commit poussé sur {k.code('main')} devient la prochaine version, sa première
      ligne étant la modification qu’il apporte – compilé d’abord, et refusé s’il ne compile pas – et{' '}
      {k.code('git push -o deploy')} la met en ligne. Une branche que vous poussez devient un brouillon, avec son aperçu
      en ligne à sa propre adresse ; poussez-la sur {k.code('main')}, ou ajoutez {k.code('-o merge')} à son dernier
      push, et elle devient la prochaine version. Ce que la plateforme place autour de votre code pour en faire un
      projet – {k.code('Program.cs')}, le fichier de projet, {k.code('Platform')} – ne fait pas partie de votre app :
      un push qui le modifie est refusé, avec l’explication. {k.code('AGENTS.md')} dans le dépôt explique le reste à un
      agent de code.
    </>
  ),
  gitAside:
    'L’adresse contient votre lien d’édition, comme celle de l’éditeur : qui la possède peut pousser. Ce que garde votre app – ses entrées, ses fichiers, ses clés et mots de passe – n’est jamais dans le dépôt.',

  open: (k) => (
    <>
      Si ce que vous avez construit peut servir à d’autres, publiez son code : ouvrez {k.b('Open source')} dans le
      tableau de bord, choisissez une licence – MIT, sauf si vous en voulez une autre – et activez la publication. Le
      code obtient sa propre page parmi les {k.link('/source', 'apps open source')}, où n’importe qui peut le lire, lui
      donner une étoile, télécharger n’importe quelle version sous la forme du même projet que {k.b('Télécharger')}{' '}
      vous donne, licence comprise, ou en cloner chaque version avec git.
    </>
  ),
  open2: () => (
    <>
      Chaque version est publiée, les plus anciennes aussi, avec sa documentation, ses tests, son espace de
      développement et la modification qu’elle a apportée. Ce que garde l’application n’est jamais publié – ses
      enregistrements, les fichiers qu’elle a enregistrés, les valeurs de ses clés et mots de passe –, pas plus que ce
      que vous avez demandé avec vos propres mots, ni qui utilise l’application. Désactivez la publication et la page
      disparaît ; ses étoiles sont conservées pour le jour où vous publierez à nouveau le code.
    </>
  ),
  openAside:
    'Tout ce qui est dans le code devient public, les versions précédentes comprises. Une clé ou un mot de passe a sa place avec les clés et mots de passe, sous Données, jamais dans le code – publié ou non.',

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
      lieu de le supposer. Vous voyez la même chose dans le tableau de bord. Il rédige aussi la documentation et les
      tests, les lit avant de modifier quoi que ce soit, et lance les tests sur l’adresse d’un brouillon avant de le
      mettre en ligne. Une page destinée à être trouvée reçoit un titre, une description, une icône et un aperçu qui
      s’affiche quand on partage son lien. Au pied des pages qu’il construit, il ajoute une petite ligne indiquant
      qu’elles ont été réalisées avec GenHTTP Lambda : dites-lui si vous préférez ne pas l’avoir, et il la retire.
    </>
  ),
  more: 'En savoir plus →',
  make: 'Créer une lambda',
};
