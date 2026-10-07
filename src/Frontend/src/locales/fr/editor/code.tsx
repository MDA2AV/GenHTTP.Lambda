import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Code',
  version: (version) => `version ${version}`,
  edited: ', modifiée',
  loadFailed: 'Impossible de charger cette version.',
  compiles: 'Ça compile.',
  notYet: 'Ça ne compile pas encore.',
  checkFailed: 'Impossible de vérifier le code.',
  saved: (version) => `Enregistré en version ${version}.`,
  featureSaved: 'Enregistré dans le brouillon. Déployez son aperçu pour l’essayer.',
  featureLoadFailed: 'Impossible de charger le brouillon.',
  previewOnline: 'L’aperçu est en ligne.',
  previewRefused: 'L’aperçu n’a pas changé. Le compilateur explique pourquoi ci-dessous.',
  isOnline: (version) => `La version ${version} est en ligne.`,
  notOnline: 'La mise en ligne a échoué. Le compilateur explique pourquoi ci-dessous.',
  failed: 'Ça n’a pas marché.',
  unchanged: 'Rien n’a changé depuis le dernier enregistrement.',
  demo: 'C’est une démo : tout est en lecture seule. Pour la modifier, créez votre propre lambda à partir de celle-ci.',
  hint: (b) => (
    <>
      Les fichiers d’une version. Son {b('code')} est le programme et tout ce qui est conservé avec lui : ses fichiers
      .cs sont compilés, dans n’importe quel dossier, et tous les autres fichiers (sa documentation, ses tests, ce à
      partir de quoi un front end est construit) sont conservés avec la version, sans jamais être compilés ni servis.
      Ses {b('ressources')} (pages, scripts, styles, images, migrations de la base de données) sont lues et servies
      pendant qu’elle tourne, et sont publiques là où le code les sert. Enregistrer crée une nouvelle version sans
      toucher à ce qui est en ligne ; pour essayer d’abord une modification, démarrez un brouillon. Ctrl-S enregistre,
      F12 va à une déclaration.
    </>
  ),
  hintFeature: (b) => (
    <>
      Les fichiers de ce brouillon : son {b('code')} (ses fichiers .cs sont compilés, dans n’importe quel dossier, le
      reste est conservé avec lui) et ses {b('ressources')}, lues et servies pendant qu’il tourne. Enregistrer les garde
      dans le brouillon et les affiche à l’adresse du brouillon ; vos visiteurs n’en voient rien tant que vous ne mettez
      pas le brouillon en ligne.
    </>
  ),
  inFeature: (name) => `dans « ${name} »`,
  changedElsewhere:
    'Le brouillon a été enregistré ailleurs depuis que vous l’avez ouvert, peut-être par l’agent. Chargez ce qui est enregistré avant d’enregistrer ici ; vos modifications ne seraient pas enregistrées par-dessus.',
  readAgain: 'Charger ce qui est enregistré',
  newer: (version) => `La version ${version} est plus récente que celle ouverte ici.`,
  check: 'Vérifier',
  save: 'Enregistrer',
  deploy: 'Déployer',
  deployPreviewTitle: 'Enregistrer, et mettre le brouillon en ligne à sa propre adresse pour l’essayer',
  binary: (size) => `Ce n’est pas du texte : il n’y a rien à modifier ici. Le fichier pèse ${size}.`,
  saveAndDeploy: 'Enregistrer et déployer',
  saveVersion: 'Enregistrer une nouvelle version',
  fromOlder: (version, newest) =>
    `Ce code part de la version ${version}, alors que la version ${newest} est plus récente. L’enregistrer en fait la version la plus récente, sans ce qui a été fait après la version ${version}.`,
  featureInstead: (start) => (
    <>
      Vous faites un essai ? {start('Mettez-le plutôt dans un nouveau brouillon')} : il a sa propre adresse,
      et aucune version n’est enregistrée avant que ce soit au point.
    </>
  ),
  cancel: 'Annuler',
  what: 'Qu’est-ce que ça change ? Facultatif, affiché dans l’historique.',
  placeholder: 'Ajoute un formulaire de contact',
  goToDefinition: 'Atteindre la définition',
  versionLabel: 'Version',
  shown: (version, online, newest) =>
    `Version ${version}${online ? ', en ligne' : newest ? ', la plus récente' : ''}`,
  optionOnline: ' (en ligne)',
  switchUnsaved: 'Ce que vous avez modifié ici n’est pas enregistré. Ouvrir quand même l’autre version ?',
  noVersion: 'Il n’y a pas encore de version à afficher.',
  label: 'Fichiers',
  codeGroup: 'Code',
  codeWhy: 'Jamais servi. Les fichiers .cs sont compilés, dans n’importe quel dossier ; le reste est conservé avec la version.',
  resources: 'Ressources',
  resourcesPublic: 'Publiques : cette version les sert avec Resources.',
  resourcesPrivate: 'Livrées avec la version, mais cette version ne les sert pas.',
  noResources: 'Aucune dans cette version.',
  count: (files) => (files === 1 ? '1 fichier' : `${files} fichiers`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Cette version occupe ${used} sur les ${of} qu’une version peut avoir, code et ressources ensemble.`,
  scope: (data) => (
    <>Ce que la lambda garde pendant qu’elle tourne est le même pour toutes les versions, et se trouve sous {data('Données')}.</>
  ),
  download: 'Télécharger',
  newIn: (group) => `Nouveau fichier dans ${group}`,
  uploadIn: (group) => `Importer dans ${group}`,
  pick: 'Choisissez un fichier pour voir son contenu.',
};
