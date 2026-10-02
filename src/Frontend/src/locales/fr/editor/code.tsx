import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Code',
  version: (version) => `version ${version}`,
  edited: ', modifiée',
  online: ', en ligne',
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
  demo: 'C’est une démo : tout est en lecture seule. Pour la modifier, créez votre propre lambda à partir de celle-ci. ',
  edit: 'Modifiez le code à la main. Enregistrer crée une nouvelle version sans toucher à ce qui est en ligne ; déployer la met en ligne. Pour essayer une modification d’abord, démarrez un brouillon. ',
  editFeature:
    'Le code de ce brouillon. Enregistrer le garde dans le brouillon : rien ne change pour les visiteurs de la lambda. Déployer le met en ligne à l’adresse du brouillon, pour l’essayer ; intégrer le brouillon en fait la prochaine version. ',
  inFeature: (name) => `dans « ${name} »`,
  changedElsewhere:
    'Le brouillon a été enregistré ailleurs depuis que vous l’avez ouvert, peut-être par l’agent. Chargez ce qui est enregistré avant d’enregistrer ici ; vos modifications ne seraient pas enregistrées par-dessus.',
  readAgain: 'Charger ce qui est enregistré',
  files: (entry, cs, context) => (
    <>
      {entry} renvoie ce qui est servi, les autres fichiers {cs} contiennent des types, et tout autre fichier est servi
      tel quel, sauf ce qui se trouve dans {context} : la documentation et les tests, jamais compilés ni servis. Ctrl-S
      enregistre, F12 va à une déclaration.
    </>
  ),
  newer: (version) => ` La version ${version} est plus récente que celle ouverte ici.`,
  check: 'Vérifier',
  save: 'Enregistrer',
  deploy: 'Déployer',
  deployPreviewTitle: 'Enregistrer, et mettre le brouillon en ligne à sa propre adresse pour l’essayer',
  binary: (size) => `Ce n’est pas du texte : rien à modifier. Le fichier est servi tel quel et pèse ${size} ko.`,
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
};
