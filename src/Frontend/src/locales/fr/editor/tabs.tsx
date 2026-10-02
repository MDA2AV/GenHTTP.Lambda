import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Lettres, chiffres, tirets et tirets bas, avec l’extension .cs',
  slashes: 'Pas de slash au début ni à la fin, et moins de 120 caractères.',
  deep: 'Six niveaux de dossiers au maximum.',
  characters: 'Lettres, chiffres, tirets, tirets bas et points, séparés par des slashs.',
  extension: 'Il faut une extension, pour que le fichier soit servi correctement.',
  context: 'Dans .lambda/, seulement docs/ et tests/ : lettres, chiffres, tirets, tirets bas et points, séparés par des slashs.',
  contextFiles: 'Documentation et tests : font partie de la version, jamais compilés ni servis',
  exists: 'Un fichier porte déjà ce nom.',
  remove: (name) => `Supprimer ${name} ? Son contenu sera supprimé aussi.`,
  there: (name) => `${name} existe déjà.`,
  entry: 'Le snippet : ce qu’il renvoie est ce qui est servi',
  errors: 'contient des erreurs',
  removeFile: (name) => `Supprimer ${name}`,
  removeTitle: 'Supprimer ce fichier',
  placeholder: 'Types.cs, site/index.html ou .lambda/docs/api.md',
  newFile: 'Nouveau fichier',
  uploadTitle: 'Importer un fichier (image, police, page)',
  upload: 'Importer un fichier',
};
