import type { EditorMessages } from '../en/editor';

/** Les textes de l’éditeur en français. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'j' },
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
      expired: 'expiré faute d’utilisation',
      admin: 'mis hors ligne par l’opérateur',
      ended: 'terminé',
    },
    whatThisIs: 'Explication',
    byAgent: 'par un agent',
    writtenByAgent: 'Écrit par un agent',
    more: 'Plus',
    of: (used, total) => `${used} sur ${total}`,
    online: (version) => `En ligne · v${version}`,
    onlineTitle: (version) => `En ligne, version ${version} servie`,
    offline: 'Hors ligne',
    offlineTitle: 'Hors ligne : rien n’est servi',
    premium:
      'Premium : peut répondre sur son propre nom de domaine, dispose de davantage d’espace pour le code, les ressources et les données, et reste en ligne quelle que soit son activité',
    demo: 'Démo : maintenue en ligne par cette installation, en lecture seule',
    tier: (tier) => `Offre ${tier}`,
    entrances: {
      title: 'Accès via',
      note: 'Depuis le démarrage du serveur, connexions WebSocket incluses.',
    },
    chart: {
      showChart: 'Afficher le graphique',
      showValues: 'Afficher les valeurs',
      none: 'Aucune mesure pour le moment.',
      time: 'Heure',
    },
    diagnostics: {
      compiles: 'Le code compile.',
      none: 'Aucun message pour le moment. Vérifiez ou déployez le code pour le compiler.',
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
      stats: 'Statistiques',
      logs: 'Journaux',
      code: 'Code',
    },
    sectionsLabel: 'Sections',
    loadFailed: 'Ce lambda n’a pas pu être chargé.',
    online: (version) => `La version ${version} est en ligne.`,
    deployFailed: 'Le lambda n’a pas pu être déployé.',
    offline: 'Mis hors ligne. Le code est conservé.',
    offlineFailed: 'Le lambda n’a pas pu être mis hors ligne.',
    leave: 'Vos modifications non enregistrées seront perdues. Quitter malgré tout ?',
    nothingTitle: 'Ce lien ne donne accès à aucun lambda',
    createNew: 'Créer un nouveau lambda',
    loading: 'Chargement de votre lambda…',
    moreActions: 'Autres actions',
    redeploy: (version) => `Redéployer la version ${version}`,
    takeOffline: 'Mettre hors ligne',
    copyLink: 'Copier le lien',
    copyPrivate: 'Copier le lien privé',
    privateLink: 'Toute personne disposant de ce lien peut modifier le lambda. Gardez-le confidentiel.',
    rename: 'Modifier l’adresse',
    download: 'Télécharger en tant que projet .NET',
    delete: 'Supprimer ce lambda',
    deploy: (version) => `Déployer la version ${version}`,
    problems: 'Des erreurs se sont produites récemment',
    demoTitle: 'Une démo, maintenue en ligne par cette installation et en lecture seule.',
    demo: (start) => (
      <>
        Son code, son historique, ses données et ses journaux sont consultables à titre d’exemple. Pour la modifier,{' '}
        {start('créez un lambda à partir de celle-ci')}.
      </>
    ),
    keep: 'Conservez ce lien précieusement. C’est le seul moyen d’accéder à ce lambda.',
    gotIt: 'Compris',
    rejected: (version) => `La version ${version} n’a pas été mise en ligne`,
    refused: 'Le déploiement a été refusé',
    openCode: 'Ouvrir le code',
    close: 'Fermer',
    notCompiling: 'Le code ne compile pas. La version précédemment en ligne le reste.',
    moved: (path) => `Désormais accessible à l’adresse ${path}.`,
    deleteTitle: 'Supprimer ce lambda ?',
    cancel: 'Annuler',
    deleteForGood: 'Supprimer définitivement',
    deleteFailed: 'Le lambda n’a pas pu être supprimé.',
    deleteText: (key) => (
      <>Toutes les versions, les fichiers, l’historique et l’adresse {key} seront supprimés. Cette action est irréversible.</>
    ),
    openInTab: 'Ouvrir dans un nouvel onglet',
    open: (address) => `Ouvrir ${address} dans un nouvel onglet`,
    copyAddress: 'Copier l’adresse',
    renameFailed: 'L’adresse n’a pas pu être modifiée.',
    moveIt: 'Modifier l’adresse',
    renameText: 'L’ancienne adresse cesse immédiatement de fonctionner ; pensez à mettre à jour les liens qui y renvoient.',
  },

  summary: {
    reading: 'Lecture de l’état…',
    hint: (since, kept, retention, tier) =>
      `Le trafic est comptabilisé depuis le dernier démarrage du serveur (${since}). ` +
      (kept
        ? `Un lambda reste en ligne tant qu’il est utilisé et est supprimé après ${retention} jours sans visite ni modification.`
        : `Ce lambda relève de l’offre ${tier}, qui le maintient en ligne et le conserve quelle que soit son activité.`),
    onlineFor: (duration, version) => (
      <>
        En ligne depuis {duration('un certain temps')}, version {version} servie.
      </>
    ),
    offline: 'Hors ligne. Rien n’est servi tant qu’aucune version n’est déployée.',
    nothing: 'Aucun code n’a encore été écrit.',
    requestsToday: 'requêtes aujourd’hui',
    lastHour: (count) => `${count} au cours de la dernière heure`,
    hourly: 'Requêtes par heure sur les dernières 24 heures',
    failed: 'en échec',
    failedTitle: (failed, rejected) =>
      `${failed} erreurs serveur, ${rejected} introuvables ou refusées, sur les dernières 24 heures`,
    average: 'temps de réponse moyen',
    noneYet: 'aucune',
    lastVisit: 'dernière visite',
    problems: 'Erreurs récentes',
    openLog: 'Ouvrir le journal',
    latest: 'Dernière modification',
    allVersions: 'Toutes les versions',
    noDescription: 'Sans description',
    version: (version) => `Version ${version}`,
    notOnline: 'pas encore en ligne',
    wanted: 'Demande initiale',
    noVersions: 'Aucune version pour le moment.',
    storage: 'Stockage',
    browse: 'Parcourir',
    code: 'Code',
    codeWhy: 'Le C# est compilé, jamais servi.',
    characters: 'caractères',
    assets: 'Ressources',
    assetsPublic: 'Public : le code les sert.',
    assetsPrivate: 'Non servies par le code.',
    data: 'Données',
    dataPublic: 'Public : le code sert le workspace.',
    dataPrivate: 'Réservées au lambda.',
  },

  files: {
    hint: (b) => (
      <>
        Le {b('Code')} est compilé et jamais servi. Les {b('Ressources')} – pages, feuilles de style, images – sont
        enregistrées avec chaque version et sont publiques si le code les sert. Les {b('Données')} sont écrites par le
        lambda pendant son exécution ; elles ne font partie d’aucune version et ne sont publiques que si le code les sert.
      </>
    ),
    edit: 'Modifier cette version',
    version: 'Version',
    shown: (version, online, newest) => `Version ${version}${online ? ', en ligne' : newest ? ', la plus récente' : ''}`,
    optionOnline: ' (en ligne)',
    readFailed: 'Cette version n’a pas pu être lue.',
    dataFailed: 'Les données n’ont pas pu être lues.',
    noVersion: 'Aucune version à afficher pour le moment.',
    label: 'Fichiers',
    code: 'Code',
    codeWhy: 'Compilé dans le lambda, jamais servi.',
    count: (files) => (files === 1 ? '1 fichier' : `${files} fichiers`),
    codeUsage: (files, used, of) => `${files}, ${used} caractères sur ${of}`,
    usage: (files, used, of) => `${files}, ${used} sur ${of}`,
    noCode: 'Aucun code dans cette version.',
    assets: 'Ressources',
    assetsPublic: 'Public : cette version les sert via Assets.',
    assetsPrivate: 'Enregistrées avec le code, mais non servies par cette version.',
    noAssets: 'Aucune dans cette version.',
    data: 'Données',
    dataPublic: 'Public : cette version les sert via Workspace.',
    dataPrivate: 'Réservées au lambda. Ne font partie d’aucune version.',
    uploadFailed: (path) => `${path} n’a pas pu être téléversé.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `Supprimer ${path} et ${held === 1 ? 'le fichier qu’il contient' : `les ${held} fichiers qu’il contient`} ?`
        : `Supprimer le dossier ${path} ?`,
    deleteFile: (path) => `Supprimer ${path} ? Le lambda n’y aura plus accès.`,
    deleteFailed: 'La suppression a échoué.',
    full: 'L’espace de données est plein',
    uploadInto: (folder) => `Téléverser dans ${folder}`,
    upload: 'Téléverser',
    reading: 'Lecture…',
    noData: 'Aucune donnée pour le moment. Ce que le lambda enregistre pendant son exécution apparaîtra ici.',
    delete: (path) => `Supprimer ${path}`,
    deleteShort: 'Supprimer',
    fileFailed: 'Le fichier n’a pas pu être lu.',
    pick: 'Sélectionnez un fichier pour afficher son contenu.',
    tooLarge: (name, size) => (
      <>
        {name} fait {size}, ce qui est trop volumineux pour être affiché ici.
      </>
    ),
    download: 'Télécharger',
    readingFile: (name) => `Lecture de ${name}…`,
    missing: (name) => `Cette version ne contient aucun fichier nommé ${name}.`,
    saved: 'enregistré',
    notText: 'Ce n’est pas un fichier texte. Téléchargez-le pour en consulter le contenu.',
  },

  versions: {
    hint: (limit) =>
      `Chaque version conserve la demande et la modification, lorsque leur auteur les a précisées. Au-delà de ${limit} versions, les plus anciennes sont supprimées ; la version en ligne est toujours conservée.`,
    none: 'Aucune version pour le moment.',
    noDescription: 'Sans description',
    online: 'en ligne',
    putOnline: 'Mettre cette version en ligne',
    rollBackTitle: 'Remettre en ligne cette version antérieure',
    deploy: 'Déployer',
    rollBack: 'Restaurer',
    readFailed: 'Cette version n’a pas pu être lue.',
    comparing: 'Comparaison…',
    unchanged: 'Aucune modification par rapport à la version précédente.',
    first: 'La première version.',
    status: { added: 'ajouté', removed: 'supprimé', changed: 'modifié', same: 'inchangé' },
    browse: 'Parcourir ses fichiers',
    edit: 'Modifier à partir d’ici',
    binary: 'Ce n’est pas un fichier texte ; aucune comparaison ligne à ligne n’est possible.',
    tooLarge: 'Trop volumineux pour une comparaison ligne à ligne.',
  },

  deployments: {
    hint: (until) =>
      `Un déploiement reste en ligne tant qu’il est utilisé${until ? ` – sans activité, jusqu’au ${until}` : ''}. Chaque nouveau déploiement et chaque visite relancent ce délai.`,
    takeOffline: 'Mettre hors ligne',
    readFailed: 'L’historique n’a pas pu être lu.',
    reading: 'Lecture de l’historique…',
    none: 'Rien n’a encore été déployé.',
    noDescription: 'Sans description',
    deployed: (when, by) => `Déployé le ${when} par ${by}`,
    duration: 'Durée de mise en ligne',
    online: 'en ligne',
    short: {
      replaced: 'remplacé',
      stopped: 'mis hors ligne',
      expired: 'expiré',
      admin: 'par l’opérateur',
      ended: 'terminé',
    },
    putBack: (version) => `Remettre en ligne la version ${version}`,
    timeline: 'Versions en ligne au cours des sept derniers jours',
    block: (version, from, to) => `Version ${version}, du ${from} ${to ? `au ${to}` : 'à maintenant'}`,
    weekAgo: 'il y a une semaine',
    now: 'maintenant',
  },

  stats: {
    readFailed: 'Les statistiques n’ont pas pu être lues.',
    range: 'Période',
    lastHour: 'Dernière heure',
    lastDay: 'Dernières 24 heures',
    hint: (since) =>
      `Comptabilisé en mémoire depuis le dernier démarrage du serveur (${since}). Un redémarrage réinitialise ces chiffres.`,
    reading: 'Lecture des statistiques…',
    requests: 'requêtes',
    websockets: (count) => `et ${count} connexions WebSocket`,
    failed: 'en échec',
    serverErrors: (count) => `${count} erreurs serveur`,
    rejected: 'introuvables ou refusées',
    average: 'temps de réponse moyen',
    sent: (amount) => `${amount} envoyés`,
    nobody: (hour) => (hour ? 'Aucun appel au cours de la dernière heure.' : 'Aucun appel au cours des dernières 24 heures.'),
    requestsTitle: 'Requêtes',
    per: (hour) => (hour ? 'Par minute.' : 'Par tranche de 15 minutes.'),
    answered: 'Traitées',
    rejectedSeries: 'Introuvables ou refusées',
    failedSeries: 'En échec',
    timeTitle: 'Temps de réponse',
    averagePer: (hour) => (hour ? 'Moyenne par minute.' : 'Moyenne par tranche de 15 minutes.'),
    averageSeries: 'Moyenne',
    mostAsked: 'Chemins les plus demandés',
    path: 'Chemin',
    requestsColumn: 'Requêtes',
    failedColumn: 'Échecs',
    averageColumn: 'Moyenne',
    since: 'Depuis le démarrage du serveur.',
  },

  logs: {
    readFailed: 'Le journal n’a pas pu être lu.',
    hint: (capturing) =>
      'Les requêtes, les sorties du lambda et les erreurs, en temps réel.' +
      (capturing ? '' : ' Cette installation ne conserve pas les sorties des lambdas ; seules les requêtes et les erreurs apparaissent.') +
      ' Le journal est conservé en mémoire et partagé entre tous les lambdas de cette installation : il couvre de quelques minutes à quelques heures et est vidé à chaque redémarrage. Les adresses des visiteurs ne sont pas affichées.',
    search: 'Rechercher',
    searchLabel: 'Rechercher dans le journal',
    resume: 'Afficher les nouvelles lignes au fil de l’eau',
    pause: 'Suspendre l’ajout de nouvelles lignes',
    paused: 'En pause',
    live: 'En direct',
    show: 'Afficher',
    all: 'Tout',
    requests: 'Requêtes',
    output: 'Sorties',
    problems: 'Erreurs',
    reading: 'Lecture du journal…',
    noProblems: 'Le journal actuel ne contient aucune erreur.',
    nothing: 'Aucune entrée pour le moment. Ouvrez l’adresse du lambda pour que ses requêtes apparaissent ici.',
    noMatch: 'Aucun résultat.',
    identical: (count) => `${count} lignes identiques`,
    at: (domain) => `, via ${domain}`,
    from: (country) => `, depuis ${country}`,
  },

  showcase: {
    loadFailed: 'La fiche vitrine n’a pas pu être chargée.',
    loading: 'Chargement…',
    title: 'un titre',
    description: 'une description',
    picture: 'une image',
    updated: 'La fiche vitrine a été mise à jour.',
    listed: 'L’application figure désormais dans la vitrine.',
    waiting: 'Enregistré. La fiche apparaîtra dans la vitrine dès que le lambda sera en ligne.',
    saveFailed: 'La fiche vitrine n’a pas pu être enregistrée.',
    removed: 'Retiré de la vitrine.',
    removeFailed: 'La fiche vitrine n’a pas pu être retirée.',
    wrongType: 'Ce fichier n’est pas une image PNG, JPEG, GIF ou WebP.',
    tooLarge: (size, limit) => `Ce fichier fait ${size} ; la taille maximale est de ${limit}.`,
    unreadable: 'Ce fichier n’a pas pu être lu.',
    hint: (tool) => (
      <>
        La vitrine présente les lambdas que leurs propriétaires ont choisi de montrer, les plus utilisés récemment en
        premier. Seul le détenteur de la clé d’édition peut y ajouter ou en retirer un lambda, qui n’y figure que tant
        qu’il est en ligne. Un agent peut faire de même avec l’outil {tool}.
      </>
    ),
    open: 'Ouvrir la vitrine',
    switch: 'Présenter ce lambda dans la vitrine',
    listedNow: 'Actuellement présenté. Les visiteurs de la vitrine peuvent l’ouvrir.',
    notListed: 'Enregistré mais non présenté : le lambda est hors ligne. Il réapparaîtra après son prochain déploiement.',
    off: 'Désactivé. Ce lambda n’est présenté nulle part tant que vous n’activez pas cette option et n’enregistrez pas.',
    offline: 'Le lambda est hors ligne ; la fiche attendra son déploiement. Seuls les lambdas accessibles sont présentés.',
    titleLabel: 'Titre',
    titlePlaceholder: 'Tableau des scores du quiz',
    descriptionLabel: 'Description',
    descriptionPlaceholder:
      'Les équipes saisissent leurs réponses sur leur téléphone, l’animateur les corrige et le tableau des scores se met à jour pour toute la salle.',
    save: 'Enregistrer les modifications',
    add: 'Ajouter à la vitrine',
    takeOff: 'Retirer',
    needs: (missing) =>
      `Il manque encore ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} et ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Certains champs sont trop longs.',
    allSaved: 'Toutes les modifications sont enregistrées.',
    preview: 'Aperçu',
    card: (address) => <>Voici la carte telle que la verront les visiteurs. Elle ouvre {address}.</>,
    confirm: 'Retirer de la vitrine ?',
    keep: 'Conserver',
    confirmText: 'Le titre, la description et l’image seront supprimés. Le lambda lui-même reste inchangé.',
    pictureLabel: 'Image',
    formats: (limit) => `PNG, JPEG, GIF ou WebP, ${limit} maximum`,
    notSaved: 'non enregistrée',
    replace: 'Déposez une nouvelle image ici pour la remplacer.',
    drop: 'Déposez une image ici.',
    advice: 'Une capture d’écran ou un court GIF de l’application au format 16:10 convient le mieux.',
    another: 'Choisir une autre image',
    choose: 'Choisir un fichier',
    keepSaved: 'Conserver l’image enregistrée',
    clear: 'Effacer',
  },

  domain: {
    readFailed: 'Le nom de domaine n’a pas pu être lu.',
    reaching: (domain) => `Les requêtes adressées à ${domain} atteignent désormais ce lambda.`,
    saveFailed: 'Le nom de domaine n’a pas pu être enregistré.',
    removed: 'Le nom de domaine a été retiré. Le lambda reste accessible à son adresse sur cette plateforme.',
    removeFailed: 'Le nom de domaine n’a pas pu être retiré.',
    hint:
      'Un lambda Premium peut répondre sur son propre nom de domaine – intégralement, depuis la racine – en plus de son adresse sur cette plateforme. Faites pointer le domaine vers ce serveur, saisissez-le ici, et les requêtes qui lui sont adressées atteindront le lambda.',
    loading: 'Chargement…',
    example: 'votre-domaine.fr',
    open: (domain) => `Ouvrir ${domain}`,
    label: 'Nom de domaine du lambda',
    serving: (domain) => <>{domain} est actuellement servi, en plus de l’adresse sur cette plateforme.</>,
    none: 'Aucun pour le moment. Un sous-domaine comme shop.example.com ou un domaine complet comme example.com.',
    change: 'Modifier',
    use: 'Utiliser ce domaine',
    remove: 'Retirer',
    confirm: 'Retirer le nom de domaine ?',
    keep: 'Conserver',
    confirmText: (domain) => (
      <>
        Les requêtes adressées à {domain} n’atteindront plus ce lambda, avec effet immédiat. Son adresse sur cette
        plateforme reste inchangée, de même que la configuration DNS du domaine.
      </>
    ),
    point: 'Faire pointer le domaine vers ce serveur',
    check: 'Vérifier à nouveau',
    records:
      'Chez le gestionnaire DNS du domaine, ajoutez les deux enregistrements suivants. Vous pouvez omettre l’enregistrement AAAA si le domaine ne doit pas être accessible en IPv6.',
    type: 'Type',
    name: 'Nom',
    value: 'Valeur',
    pointsHere: (domain) => <>{domain} pointe vers ce serveur.</>,
    alsoElsewhere: (addresses) =>
      ` Il est également résolu vers ${addresses}, qui n’est pas ce serveur – les visiteurs dirigés vers cette adresse n’atteindront pas le lambda.`,
    elsewhere: (addresses) => `Le domaine est résolu vers ${addresses}, qui n’est pas encore ce serveur.`,
    wait: 'Une modification peut mettre un certain temps à se propager – jusqu’à la durée de vie (TTL) de l’ancien enregistrement.',
    cname: 'Utiliser plutôt un enregistrement CNAME',
    cnameText: (target) => (
      <>
        Un sous-domaine peut également pointer vers {target} au moyen d’un enregistrement CNAME ; il suivra alors ce
        serveur si ses adresses venaient à changer. Cette solution présente toutefois des inconvénients :
      </>
    ),
    cnameRoot: (example) => (
      <>
        Elle ne peut pas être utilisée pour un domaine complet ({example} lui-même) : la norme n’autorise pas de CNAME aux
        côtés des enregistrements que tout domaine possède à sa racine. Certains fournisseurs proposent à cet effet un
        enregistrement ALIAS, ANAME ou « aplati ».
      </>
    ),
    cnameAlone: 'Aucun autre enregistrement ne peut coexister sur le même nom – ni MX pour la messagerie, ni TXT pour les vérifications.',
    cnameLookup: 'Les résolveurs des visiteurs doivent effectuer une requête supplémentaire.',
    copy: 'Copier',
    copyValue: (value) => `Copier ${value}`,
  },

  code: {
    title: 'Code',
    version: (version) => `version ${version}`,
    edited: ', modifiée',
    online: ', en ligne',
    loadFailed: 'Cette version n’a pas pu être chargée.',
    compiles: 'Le code compile.',
    notYet: 'Le code ne compile pas encore.',
    checkFailed: 'Le code n’a pas pu être vérifié.',
    saved: (version) => `Enregistré en tant que version ${version}.`,
    isOnline: (version) => `La version ${version} est en ligne.`,
    notOnline: 'La mise en ligne a échoué. Consultez les messages du compilateur ci-dessous.',
    failed: 'L’opération a échoué.',
    unchanged: 'Aucune modification depuis le dernier enregistrement.',
    demo: 'Il s’agit d’une démo, en lecture seule. Créez un lambda à partir de celle-ci pour la modifier. ',
    edit: 'Modifiez le code manuellement. L’enregistrement crée une nouvelle version sans toucher à celle en ligne ; le déploiement la met en ligne. ',
    files: (entry, cs) => (
      <>
        {entry} renvoie ce qui est servi, les autres fichiers {cs} contiennent des types, et tout autre fichier est servi
        tel quel. Ctrl+S enregistre, F12 accède à une déclaration.
      </>
    ),
    newer: (version) => ` La version ${version} est plus récente que celle ouverte ici.`,
    check: 'Vérifier',
    save: 'Enregistrer',
    deploy: 'Déployer',
    binary: (size) => `Ce n’est pas un fichier texte ; il ne peut pas être modifié. Il est servi tel quel et pèse ${size} ko.`,
    saveAndDeploy: 'Enregistrer et déployer',
    saveVersion: 'Enregistrer une nouvelle version',
    cancel: 'Annuler',
    what: 'Que modifie cette version ? Facultatif – l’information apparaît dans l’historique.',
    placeholder: 'Ajoute un formulaire de contact',
    goToDefinition: 'Atteindre la définition',
  },

  tabs: {
    codeName: 'Lettres, chiffres, tirets et traits de soulignement, avec l’extension .cs',
    slashes: 'Pas de barre oblique au début ni à la fin, 120 caractères au maximum.',
    deep: 'Six niveaux de dossiers au maximum.',
    characters: 'Lettres, chiffres, tirets, traits de soulignement et points, séparés par des barres obliques.',
    extension: 'Une extension est nécessaire pour que le fichier soit servi correctement.',
    exists: 'Un fichier portant ce nom existe déjà.',
    remove: (name) => `Supprimer ${name} ? Son contenu sera également supprimé.`,
    there: (name) => `${name} existe déjà.`,
    entry: 'L’extrait principal : ce qu’il renvoie est servi',
    errors: 'contient des erreurs',
    removeFile: (name) => `Supprimer ${name}`,
    removeTitle: 'Supprimer ce fichier',
    placeholder: 'Types.cs ou site/index.html',
    newFile: 'Nouveau fichier',
    uploadTitle: 'Téléverser un fichier – image, police ou page',
    upload: 'Téléverser un fichier',
  },
};
