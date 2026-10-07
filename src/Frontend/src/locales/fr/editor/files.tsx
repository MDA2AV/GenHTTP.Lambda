import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const files: EditorMessages['files'] = {
  version: 'Version',
  shown: (version, online, newest) => `Version ${version}${online ? ', en ligne' : newest ? ', la plus récente' : ''}`,
  optionOnline: ' (en ligne)',
  count: (files) => count(files, 'fichier', 'fichiers'),
  usage: (files, used, of) => `${files}, ${used} sur ${of}`,
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
