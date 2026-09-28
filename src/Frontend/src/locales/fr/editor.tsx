import type { EditorMessages } from '../en/editor';

/** Un nombre et son nom, au singulier pour 0 et 1 comme on le dit en français. */
const count = (value: number, one: string, many: string) => `${value} ${Math.abs(value) < 2 ? one : many}`;

/** Les textes de l’éditeur en français. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'j' },
    amount: (value, unit) => `${value} ${unit}`,
    pair: (larger, smaller) => `${larger} ${smaller}`,
    never: 'jamais',
    justNow: 'à l’instant',
    ago: (span) => `il y a ${span}`,
    in: (span) => `dans ${span}`,
    origins: {
      agent: 'agent',
      template: 'modèle',
      admin: 'opérateur',
      system: 'plateforme',
      api: 'API / éditeur',
      unknown: 'inconnu',
    },
    endings: {
      replaced: 'remplacé par un déploiement plus récent',
      stopped: 'mis hors ligne',
      expired: 'expiré faute de visites',
      admin: 'mis hors ligne par l’opérateur',
      ended: 'terminé',
    },
    whatThisIs: 'Explications',
    byAgent: 'par un agent',
    writtenByAgent: 'Écrit par un agent',
    more: 'Plus',
    of: (used, total) => `${used} sur ${total}`,
    online: (version) => `En ligne · v${version}`,
    onlineTitle: (version) => `En ligne, sert la version ${version}`,
    offline: 'Hors ligne',
    offlineTitle: 'Hors ligne : rien n’est servi',
    premium:
      'Premium : peut répondre sur son propre domaine, a plus de place pour le code, les assets et les données, et reste en ligne même sans aucune activité',
    demo: 'Démo : gardée en ligne par cette installation, en lecture seule',
    tier: (tier) => `Offre ${tier}`,
    entrances: {
      title: 'Accès via',
      note: 'Depuis le démarrage du serveur, connexions WebSocket comprises.',
    },
    chart: {
      showChart: 'Voir le graphique',
      showValues: 'Voir les valeurs',
      none: 'Aucune mesure pour l’instant.',
      time: 'Heure',
    },
    diagnostics: {
      compiles: 'Le code compile.',
      none: 'Aucun message pour l’instant. Vérifiez ou déployez pour compiler votre code.',
      line: (line) => `ligne ${line}`,
    },
  },

  frame: {
    title: 'Éditeur',
    sections: {
      overview: 'Vue d’ensemble',
      showcase: 'Vitrine',
      domain: 'Domaine',
      files: 'Fichiers',
      versions: 'Versions',
      deployments: 'Déploiements',
      stats: 'Stats',
      logs: 'Logs',
      code: 'Code',
    },
    sectionsLabel: 'Sections',
    loadFailed: 'Impossible de charger cette lambda.',
    online: (version) => `La version ${version} est en ligne.`,
    deployFailed: 'Impossible de déployer la lambda.',
    offline: 'Mise hors ligne. Le code est toujours là.',
    offlineFailed: 'Impossible de mettre la lambda hors ligne.',
    leave: 'Les modifications non enregistrées du code seront perdues. Quitter quand même ?',
    nothingTitle: 'Ce lien n’ouvre rien',
    createNew: 'Créer une nouvelle lambda',
    loading: 'Chargement de votre lambda…',
    moreActions: 'Plus d’actions',
    redeploy: (version) => `Redéployer la version ${version}`,
    takeOffline: 'Mettre hors ligne',
    copyLink: 'Copier le lien',
    copyPrivate: 'Copier le lien privé',
    privateLink: 'Toute personne qui a ce lien peut modifier la lambda. Gardez-le pour vous.',
    rename: 'Changer l’adresse',
    download: 'Télécharger en projet .NET',
    delete: 'Supprimer cette lambda',
    deploy: (version) => `Déployer la version ${version}`,
    problems: 'Il y a eu des erreurs récemment',
    demoTitle: 'Une démo, gardée en ligne par cette installation, en lecture seule.',
    demo: (start) => (
      <>
        Lisez son code, son historique, ce qu’elle stocke et ses logs : elle est là pour ça. Pour la modifier,{' '}
        {start('créez votre propre lambda à partir de celle-ci')}.
      </>
    ),
    keep: 'Gardez ce lien. C’est le seul moyen de revenir à cette lambda.',
    gotIt: 'Compris',
    rejected: (version) => `La version ${version} n’a pas été mise en ligne`,
    refused: 'Le déploiement a été refusé',
    openCode: 'Ouvrir le code',
    close: 'Fermer',
    notCompiling: 'Le code ne compile pas. Ce qui était en ligne l’est toujours.',
    moved: (path) => `Nouvelle adresse : ${path}.`,
    deleteTitle: 'Supprimer cette lambda ?',
    cancel: 'Annuler',
    deleteForGood: 'Supprimer définitivement',
    deleteFailed: 'Impossible de supprimer la lambda.',
    deleteText: (key) => (
      <>Toutes les versions, leurs fichiers, l’historique et l’adresse {key} disparaissent avec elle. C’est irréversible.</>
    ),
    openInTab: 'Ouvrir dans un nouvel onglet',
    open: (address) => `Ouvrir ${address} dans un nouvel onglet`,
    copyAddress: 'Copier l’adresse',
    renameFailed: 'Impossible de changer l’adresse.',
    moveIt: 'Déplacer',
    renameText: 'L’ancienne adresse cesse de fonctionner tout de suite : pensez à mettre à jour ce qui pointe vers elle.',
  },

  summary: {
    reading: 'Chargement de l’état…',
    hint: (since, kept, retention, tier) =>
      `Le trafic est compté depuis le dernier démarrage du serveur (${since}). ` +
      (kept
        ? `Une lambda reste en ligne tant qu’on l’utilise, et elle est supprimée après ${retention} jours sans visite ni modification.`
        : `Cette lambda est dans l’offre ${tier}, qui la garde en ligne et stockée, même sans aucune activité.`),
    onlineFor: (duration, version) => (
      <>
        En ligne depuis {duration('un moment')}, sert la version {version}.
      </>
    ),
    offline: 'Hors ligne. Rien n’est servi tant qu’aucune version n’est déployée.',
    nothing: 'Rien n’a encore été écrit.',
    requestsToday: 'requêtes aujourd’hui',
    lastHour: (count) => `${count} dans la dernière heure`,
    hourly: 'Requêtes par heure sur les dernières 24 heures',
    failed: 'en échec',
    failedTitle: (failed, rejected) =>
      `${count(failed, 'erreur serveur', 'erreurs serveur')}, ${count(rejected, 'requête introuvable ou refusée', 'requêtes introuvables ou refusées')}, sur les dernières 24 heures`,
    average: 'temps de réponse moyen',
    noneYet: 'pas encore',
    lastVisit: 'dernière visite',
    problems: 'Il y a eu des erreurs récemment',
    openLog: 'Ouvrir les logs',
    latest: 'Dernière modification',
    allVersions: 'Toutes les versions',
    noDescription: 'Sans description',
    version: (version) => `Version ${version}`,
    notOnline: 'pas encore en ligne',
    wanted: 'Ce qui était demandé',
    noVersions: 'Aucune version pour l’instant.',
    storage: 'Stockage',
    browse: 'Parcourir',
    code: 'Code',
    codeWhy: 'Le C# est compilé, jamais servi.',
    characters: 'caractères',
    assets: 'Assets',
    assetsPublic: 'Publics : le code les sert.',
    assetsPrivate: 'Pas servis par le code.',
    data: 'Données',
    dataPublic: 'Publiques : le code sert le workspace.',
    dataPrivate: 'Privées, réservées à la lambda.',
  },

  files: {
    hint: (b) => (
      <>
        Le {b('Code')} est compilé et jamais servi. Les {b('Assets')} (pages, styles, images) sont enregistrés avec chaque
        version, et publics si le code les sert. Les {b('Données')} sont ce que la lambda écrit pendant qu’elle tourne :
        elles ne font partie d’aucune version, et ne sont publiques que si le code les sert.
      </>
    ),
    edit: 'Modifier cette version',
    version: 'Version',
    shown: (version, online, newest) => `Version ${version}${online ? ', en ligne' : newest ? ', la plus récente' : ''}`,
    optionOnline: ' (en ligne)',
    readFailed: 'Impossible de lire cette version.',
    dataFailed: 'Impossible de lire les données.',
    noVersion: 'Aucune version à afficher pour l’instant.',
    label: 'Fichiers',
    code: 'Code',
    codeWhy: 'Compilé dans la lambda, jamais servi.',
    count: (files) => count(files, 'fichier', 'fichiers'),
    codeUsage: (files, used, of) => `${files}, ${used} sur ${of} caractères`,
    usage: (files, used, of) => `${files}, ${used} sur ${of}`,
    noCode: 'Pas de code dans cette version.',
    assets: 'Assets',
    assetsPublic: 'Publics : cette version les sert avec Assets.',
    assetsPrivate: 'Enregistrés avec le code, mais cette version ne les sert pas.',
    noAssets: 'Aucun dans cette version.',
    data: 'Données',
    dataPublic: 'Publiques : cette version les sert avec Workspace.',
    dataPrivate: 'Privées, réservées à la lambda. Ne font partie d’aucune version.',
    uploadFailed: (path) => `Impossible d’importer ${path}.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `Supprimer ${path} et ${held === 1 ? 'le fichier qu’il contient' : `les ${held} fichiers qu’il contient`} ?`
        : `Supprimer le dossier ${path} ?`,
    deleteFile: (path) => `Supprimer ${path} ? La lambda ne le trouvera plus.`,
    deleteFailed: 'Impossible de supprimer.',
    full: 'Plus de place pour les données',
    uploadInto: (folder) => `Importer dans ${folder}`,
    upload: 'Importer',
    reading: 'Lecture…',
    noData: 'Rien pour l’instant. Ce que la lambda enregistre pendant qu’elle tourne apparaîtra ici.',
    delete: (path) => `Supprimer ${path}`,
    deleteShort: 'Supprimer',
    fileFailed: 'Impossible de lire le fichier.',
    pick: 'Choisissez un fichier pour voir son contenu.',
    tooLarge: (name, size) => (
      <>
        {name} fait {size}, trop gros pour être affiché ici.
      </>
    ),
    download: 'Télécharger',
    readingFile: (name) => `Lecture de ${name}…`,
    missing: (name) => `Cette version n’a pas de fichier nommé ${name}.`,
    saved: 'enregistré',
    notText: 'Ce n’est pas du texte. Téléchargez-le pour voir ce qu’il contient.',
  },

  versions: {
    hint: (limit) =>
      `Chaque version garde ce qui a été demandé et ce qu’elle a changé, quand son auteur l’a précisé. Au-delà de ${limit} versions, les plus anciennes sont supprimées ; celle qui est en ligne, jamais.`,
    none: 'Aucune version pour l’instant.',
    noDescription: 'Sans description',
    online: 'en ligne',
    putOnline: 'Mettre cette version en ligne',
    rollBackTitle: 'Remettre en ligne cette ancienne version',
    deploy: 'Déployer',
    rollBack: 'Restaurer',
    readFailed: 'Impossible de lire cette version.',
    comparing: 'Comparaison…',
    unchanged: 'Rien n’a changé depuis la version précédente.',
    first: 'La première version.',
    status: { added: 'ajouté', removed: 'supprimé', changed: 'modifié', same: 'inchangé' },
    browse: 'Parcourir ses fichiers',
    edit: 'Modifier à partir d’ici',
    binary: 'Ce n’est pas du texte : pas de lignes à comparer.',
    tooLarge: 'Trop gros pour une comparaison ligne à ligne.',
  },

  deployments: {
    hint: (until) =>
      `Un déploiement reste en ligne tant qu’on l’utilise${until ? ` (sans visite, fin prévue : ${until})` : ''}. Un nouveau déploiement, ou la moindre visite, relance le compteur.`,
    takeOffline: 'Mettre hors ligne',
    readFailed: 'Impossible de lire l’historique.',
    reading: 'Chargement de l’historique…',
    none: 'Rien n’a encore été déployé.',
    noDescription: 'Sans description',
    deployed: (when, by) => `Déployé : ${when} (${by})`,
    duration: 'Durée en ligne',
    online: 'en ligne',
    short: {
      replaced: 'remplacé',
      stopped: 'mis hors ligne',
      expired: 'expiré',
      admin: 'par l’opérateur',
      ended: 'terminé',
    },
    putBack: (version) => `Remettre la version ${version} en ligne`,
    timeline: 'Ce qui était en ligne ces sept derniers jours',
    block: (version, from, to) => `Version ${version} : ${from} – ${to ?? 'maintenant'}`,
    weekAgo: 'il y a une semaine',
    now: 'maintenant',
  },

  stats: {
    readFailed: 'Impossible de lire les chiffres.',
    range: 'Période',
    lastHour: 'Dernière heure',
    lastDay: 'Dernières 24 heures',
    hint: (since) =>
      `Chiffres gardés en mémoire depuis le dernier démarrage du serveur (${since}). Un redémarrage les remet à zéro.`,
    reading: 'Chargement des chiffres…',
    requests: 'requêtes',
    websockets: (value) => `et ${count(value, 'connexion WebSocket', 'connexions WebSocket')}`,
    failed: 'en échec',
    serverErrors: (value) => count(value, 'erreur serveur', 'erreurs serveur'),
    rejected: 'introuvables ou refusées',
    average: 'temps de réponse moyen',
    sent: (amount) => `${amount} envoyés`,
    nobody: (hour) => (hour ? 'Aucune requête dans la dernière heure.' : 'Aucune requête ces dernières 24 heures.'),
    requestsTitle: 'Requêtes',
    per: (hour) => (hour ? 'Par minute.' : 'Par tranche de 15 minutes.'),
    answered: 'Traitées',
    rejectedSeries: 'Introuvables ou refusées',
    failedSeries: 'En échec',
    timeTitle: 'Temps de réponse',
    averagePer: (hour) => (hour ? 'Moyenne par minute.' : 'Moyenne par tranche de 15 minutes.'),
    averageSeries: 'Moyenne',
    mostAsked: 'Les plus demandés',
    path: 'Chemin',
    requestsColumn: 'Requêtes',
    failedColumn: 'Échecs',
    averageColumn: 'Moyenne',
    since: 'Depuis le démarrage du serveur.',
  },

  logs: {
    readFailed: 'Impossible de lire les logs.',
    hint: (capturing) =>
      'Les requêtes, ce que la lambda affiche et ce qui plante, en direct.' +
      (capturing ? '' : ' Cette installation ne garde pas ce que les lambdas affichent : seules les requêtes et les erreurs apparaissent.') +
      ' Les logs sont gardés en mémoire et partagés par toutes les lambdas de l’installation : ils remontent de quelques minutes à quelques heures, et repartent de zéro à chaque redémarrage. Les adresses des visiteurs ne sont pas affichées.',
    search: 'Rechercher',
    searchLabel: 'Rechercher dans les logs',
    resume: 'Afficher les nouvelles lignes au fur et à mesure',
    pause: 'Ne plus ajouter de lignes pendant que vous lisez',
    paused: 'En pause',
    live: 'En direct',
    show: 'Afficher',
    all: 'Tout',
    requests: 'Requêtes',
    output: 'Sortie',
    problems: 'Erreurs',
    reading: 'Chargement des logs…',
    noProblems: 'Aucune erreur, du moins dans ce que les logs ont gardé.',
    nothing: 'Rien pour l’instant. Ouvrez l’adresse de la lambda : ses requêtes apparaîtront ici.',
    noMatch: 'Aucun résultat.',
    identical: (value) => count(value, 'ligne identique', 'lignes identiques'),
    at: (domain) => `, sur ${domain}`,
    from: (country) => `, pays : ${country}`,
  },

  showcase: {
    loadFailed: 'Impossible de charger la fiche vitrine.',
    loading: 'Chargement…',
    title: 'un titre',
    description: 'une description',
    picture: 'une image',
    updated: 'La fiche vitrine est à jour.',
    listed: 'Elle est maintenant sur la page Vitrine.',
    waiting: 'Enregistré. La fiche apparaîtra dans la vitrine dès que la lambda sera en ligne.',
    saveFailed: 'Impossible d’enregistrer la fiche vitrine.',
    removed: 'Retirée de la vitrine.',
    removeFailed: 'Impossible de retirer la fiche vitrine.',
    wrongType: 'Ce n’est pas une image PNG, JPEG, GIF ou WebP.',
    tooLarge: (size, limit) => `Ce fichier fait ${size} ; une image ne peut pas dépasser ${limit}.`,
    unreadable: 'Impossible de lire ce fichier.',
    hint: (tool) => (
      <>
        La vitrine liste les lambdas que leurs auteurs ont choisi de montrer, les plus utilisées récemment en premier.
        Seule la personne qui a la clé d’édition peut y ajouter une lambda ou l’en retirer, et la lambda n’y figure que
        tant qu’elle est en ligne. Un agent peut faire de même avec son outil {tool}.
      </>
    ),
    open: 'Ouvrir la vitrine',
    switch: 'Montrer cette lambda dans la vitrine',
    listedNow: 'Publiée dans la vitrine. N’importe quel visiteur peut l’ouvrir.',
    notListed: 'Enregistré, mais pas en vitrine : la lambda est hors ligne. Elle réapparaîtra dès qu’elle sera redéployée.',
    off: 'Désactivé. Cette lambda n’apparaît nulle part tant que vous n’activez pas cette option et n’enregistrez pas.',
    offline: 'La lambda est hors ligne : la fiche attendra son déploiement. Seules les lambdas qui répondent sont listées.',
    titleLabel: 'Titre',
    titlePlaceholder: 'Scores du quiz au bar',
    descriptionLabel: 'Description',
    descriptionPlaceholder:
      'Les équipes entrent leurs réponses sur leur téléphone, l’animateur les corrige, et le tableau des scores se met à jour pour toute la salle.',
    save: 'Enregistrer les modifications',
    add: 'Ajouter à la vitrine',
    takeOff: 'Retirer',
    needs: (missing) =>
      `Il manque encore ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} et ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Certains champs sont trop longs.',
    allSaved: 'Tout est enregistré.',
    preview: 'Aperçu',
    card: (address) => <>Voici la carte que voient les visiteurs. Elle ouvre {address}.</>,
    confirm: 'Retirer de la vitrine ?',
    keep: 'La garder',
    confirmText: 'Le titre, la description et l’image seront supprimés. La lambda elle-même reste telle quelle.',
    pictureLabel: 'Image',
    formats: (limit) => `PNG, JPEG, GIF ou WebP, ${limit} max.`,
    notSaved: 'pas encore enregistrée',
    replace: 'Déposez-en une nouvelle ici pour la remplacer.',
    drop: 'Déposez une image ici.',
    advice: 'Une capture d’écran, ou un court GIF de l’app en action, de préférence au format 16:10.',
    another: 'En choisir une autre',
    choose: 'Choisir un fichier',
    keepSaved: 'Garder l’image enregistrée',
    clear: 'Effacer',
  },

  domain: {
    readFailed: 'Impossible de lire le domaine.',
    reaching: (domain) => `Les requêtes vers ${domain} arrivent désormais sur cette lambda.`,
    saveFailed: 'Impossible d’enregistrer le domaine.',
    removed: 'Domaine retiré. La lambda répond toujours à son adresse ici.',
    removeFailed: 'Impossible de retirer le domaine.',
    hint:
      'Une lambda premium peut répondre sur son propre domaine (tout le domaine, depuis la racine) en plus de son adresse ici. Faites pointer le domaine vers ce serveur, saisissez-le ici, et ses requêtes arriveront sur la lambda.',
    loading: 'Chargement…',
    example: 'votre-domaine.fr',
    open: (domain) => `Ouvrir ${domain}`,
    label: 'Le domaine sur lequel elle répond',
    serving: (domain) => <>Répond désormais sur {domain}, en plus de son adresse ici.</>,
    none: 'Aucun pour l’instant. Un sous-domaine comme shop.example.com, ou un domaine entier comme example.com.',
    change: 'Modifier',
    use: 'Utiliser ce domaine',
    remove: 'Retirer',
    confirm: 'Retirer le domaine ?',
    keep: 'Le garder',
    confirmText: (domain) => (
      <>
        Dès maintenant, les requêtes vers {domain} n’arriveront plus sur cette lambda. Son adresse ici ne change pas,
        pas plus que ce que dit le DNS du domaine.
      </>
    ),
    point: 'Faites pointer le domaine vers ce serveur',
    check: 'Revérifier',
    records:
      'Chez le gestionnaire DNS du domaine, ajoutez ces deux enregistrements. Laissez de côté l’enregistrement AAAA si vous ne voulez pas être joignable en IPv6.',
    type: 'Type',
    name: 'Nom',
    value: 'Valeur',
    pointsHere: (domain) => <>{domain} pointe ici.</>,
    alsoElsewhere: (addresses) =>
      ` Il pointe aussi vers ${addresses}, qui n’est pas ce serveur : les visiteurs envoyés là-bas n’atteindront pas la lambda.`,
    elsewhere: (addresses) => `Il pointe vers ${addresses}, qui n’est pas encore ce serveur.`,
    wait: 'Un changement peut mettre un moment à être visible partout, jusqu’au TTL de l’ancien enregistrement.',
    cname: 'Utiliser plutôt un enregistrement CNAME',
    cnameText: (target) => (
      <>
        Un sous-domaine peut aussi pointer vers {target} avec un enregistrement CNAME : il suit alors ce serveur si ses
        adresses changent un jour. Mais il y a des inconvénients :
      </>
    ),
    cnameRoot: (example) => (
      <>
        Impossible pour un domaine entier ({example} lui-même) : la norme n’autorise pas de CNAME à côté des
        enregistrements que tout domaine a à sa racine. Certains fournisseurs proposent à la place un enregistrement
        ALIAS, ANAME ou « aplati » qui fonctionne dans ce cas.
      </>
    ),
    cnameAlone: 'Rien d’autre ne peut cohabiter sur le même nom : pas d’enregistrement MX pour les e-mails, pas de TXT pour les vérifications.',
    cnameLookup: 'Les résolveurs des visiteurs font une requête de plus avant d’arriver.',
    copy: 'Copier',
    copyValue: (value) => `Copier ${value}`,
  },

  code: {
    title: 'Code',
    version: (version) => `version ${version}`,
    edited: ', modifiée',
    online: ', en ligne',
    loadFailed: 'Impossible de charger cette version.',
    compiles: 'Ça compile.',
    notYet: 'Ça ne compile pas encore.',
    checkFailed: 'Impossible de vérifier le code.',
    saved: (version) => `Enregistré en version ${version}.`,
    isOnline: (version) => `La version ${version} est en ligne.`,
    notOnline: 'La mise en ligne a échoué. Le compilateur explique pourquoi ci-dessous.',
    failed: 'Ça n’a pas marché.',
    unchanged: 'Rien n’a changé depuis le dernier enregistrement.',
    demo: 'C’est une démo : tout est en lecture seule. Pour la modifier, créez votre propre lambda à partir de celle-ci. ',
    edit: 'Modifiez le code à la main. Enregistrer crée une nouvelle version sans toucher à ce qui est en ligne ; déployer la met en ligne. ',
    files: (entry, cs) => (
      <>
        {entry} renvoie ce qui est servi, les autres fichiers {cs} contiennent des types, et tout autre fichier est servi
        tel quel. Ctrl-S enregistre, F12 va à une déclaration.
      </>
    ),
    newer: (version) => ` La version ${version} est plus récente que celle ouverte ici.`,
    check: 'Vérifier',
    save: 'Enregistrer',
    deploy: 'Déployer',
    binary: (size) => `Ce n’est pas du texte : rien à modifier. Le fichier est servi tel quel et pèse ${size} ko.`,
    saveAndDeploy: 'Enregistrer et déployer',
    saveVersion: 'Enregistrer une nouvelle version',
    cancel: 'Annuler',
    what: 'Qu’est-ce que ça change ? Facultatif, affiché dans l’historique.',
    placeholder: 'Ajoute un formulaire de contact',
    goToDefinition: 'Atteindre la définition',
  },

  tabs: {
    codeName: 'Lettres, chiffres, tirets et tirets bas, avec l’extension .cs',
    slashes: 'Pas de slash au début ni à la fin, et moins de 120 caractères.',
    deep: 'Six niveaux de dossiers au maximum.',
    characters: 'Lettres, chiffres, tirets, tirets bas et points, séparés par des slashs.',
    extension: 'Il faut une extension, pour que le fichier soit servi correctement.',
    exists: 'Un fichier porte déjà ce nom.',
    remove: (name) => `Supprimer ${name} ? Son contenu sera supprimé aussi.`,
    there: (name) => `${name} existe déjà.`,
    entry: 'Le snippet : ce qu’il renvoie est ce qui est servi',
    errors: 'contient des erreurs',
    removeFile: (name) => `Supprimer ${name}`,
    removeTitle: 'Supprimer ce fichier',
    placeholder: 'Types.cs ou site/index.html',
    newFile: 'Nouveau fichier',
    uploadTitle: 'Importer un fichier (image, police, page)',
    upload: 'Importer un fichier',
  },
};
