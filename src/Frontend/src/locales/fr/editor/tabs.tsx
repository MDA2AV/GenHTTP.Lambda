import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Un fichier C# porte un nom fait de lettres, chiffres, tirets, tirets bas et points, commençant par une lettre et se terminant par .cs, 40 caractères au maximum.',
  name: 'Lettres, chiffres et - _ . + @ ( ) [ ] { } $ ~, dossiers séparés par des slashs, sans espaces et sans nom se terminant par un point.',
  taken: 'Une lambda exportée ou clonée a déjà un fichier ou un dossier de ce nom à la racine. Placez-le dans un dossier, ou donnez-lui un autre nom.',
  lambda: 'Une lambda ne garde plus de dossier .lambda/ : sa documentation va dans docs/, ses tests dans tests/.',
  assets: 'Ce qu’une lambda sert se trouve désormais dans ses ressources : ajoutez-le là.',
  resourceName: 'Lettres, chiffres, tirets, tirets bas et points, séparés par des slashs, six dossiers de profondeur au maximum - et une extension, pour qu’il soit servi comme il faut.',
  exists: 'Un fichier porte déjà ce nom.',
  remove: (name) => `Supprimer ${name} ? Son contenu sera supprimé aussi.`,
  removeFolder: (name, files) => `Supprimer ${name} et ${files === 1 ? 'le fichier' : `les ${files} fichiers`} qu’il contient ?`,
  there: (name) => `${name} existe déjà.`,
  entry: 'Le snippet : ce qu’il renvoie est ce qui est servi',
  errors: 'contient des erreurs',
  removeFile: (name) => `Supprimer ${name}`,
  removeTitle: 'Supprimer',
  codePlaceholder: 'Store.cs, models/Item.cs ou docs/notes.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'Importer un fichier',
};
