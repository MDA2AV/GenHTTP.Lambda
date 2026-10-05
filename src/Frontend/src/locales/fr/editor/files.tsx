import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      Les fichiers d’une version : le programme. Le {b('Code')} est compilé et jamais servi. Les {b('Assets')} (pages,
      scripts, styles, images) sont enregistrés avec le code, déployés et restaurés avec lui, et publics si le code les
      sert. Ce que la lambda garde pendant qu’elle tourne n’est pas ici : ce sont ses {b('Données')}.
    </>
  ),
  scope: (version, data) => (
    <>
      Ces fichiers appartiennent à la version {version} et changent avec elle. Ce que la lambda garde pendant qu’elle
      tourne est le même pour toutes les versions, et se trouve dans {data('Données')}.
    </>
  ),
  edit: 'Modifier cette version',
  version: 'Version',
  shown: (version, online, newest) => `Version ${version}${online ? ', en ligne' : newest ? ', la plus récente' : ''}`,
  optionOnline: ' (en ligne)',
  readFailed: 'Impossible de lire cette version.',
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
  context: 'Documentation et tests',
  contextWhy: 'Jamais compilés, jamais servis : ce qui est écrit sur cette version, pour qui la lit ou la modifie.',
  contextUsage: (files, size) => `${files}, ${size} – comptés avec les assets`,
  noContext: 'Rien n’est encore écrit sur cette version.',
  development: 'Espace de développement',
  developmentWhy: 'Jamais compilé ni servi : ce à partir de quoi les assets sont construits, par celui qui le modifie.',
  noDevelopment: 'Aucun - un front-end construit avec une chaîne d’outils garde son projet ici.',
  data: 'Données',
  dataPublic: 'Publiques : le code en ligne les sert avec Workspace.',
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
};
