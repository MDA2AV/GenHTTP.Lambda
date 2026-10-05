import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Build',
  hint: 'Ce à partir de quoi les assets ou le code d’une version sont construits : des fichiers sur lesquels celui qui modifie l’application - votre agent, dans un clone - lance un outil de build, conservés avec chaque version, jamais compilés ni servis. Cette plateforme ne construit rien : ils se lisent ici, sans s’y modifier.',
  overview: 'Vue d’ensemble',
  files: 'Fichiers',
  scope: (version) =>
    `Ce à partir de quoi la version ${version} est construite - conservé avec elle, jamais compilé ni servi, et construit par celui qui la modifie, jamais ici.`,
  scopeDraft: 'Ce à partir de quoi ce brouillon est construit - conservé avec lui, jamais compilé ni servi, et construit par celui qui le modifie, jamais ici.',
  reading: 'Lecture de ce à partir de quoi il est construit…',
  readFailed: 'Impossible de lire ce à partir de quoi il est construit.',

  emptyTitle: (version) => `La version ${version} ne conserve rien à partir de quoi elle est construite`,
  emptyTitleDraft: 'Ce brouillon ne conserve rien à partir de quoi il est construit',
  emptyText: (code) => (
    <>
      Lorsque les assets ou le code d’une version sont produits par un outil de build - compilés, assemblés ou
      générés -, les fichiers à partir desquels ils sont produits sont conservés ici, avec chaque version : le dossier{' '}
      {code('build/')} dans un clone. Celui qui modifie l’application lance le build là où il travaille et enregistre les
      deux ensemble ; cette plateforme ne construit rien. Ce qui est écrit tel qu’il est servi ou compilé n’en a pas
      besoin.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} dans un clone explique à un agent de code comment l’utiliser.</>
  ),

  inVersion: (version) => `Dans la version ${version}`,
  inDraft: 'Dans ce brouillon',
  comparedWith: (version) => `par rapport à la version ${version}`,
  first: 'La première version qui le conserve.',
  both: (here, program) =>
    `${here === 1 ? '1 fichier modifié' : `${here} fichiers modifiés`} ici, et ${program === 1 ? '1 fichier' : `${program} fichiers`} du code et des assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 fichier modifié' : `${here} fichiers modifiés`} ici, et rien dans le code ni dans les assets : si ce qui a changé y est intégré par le build, il n’a pas été construit.`,
  programOnly: 'Rien n’a changé ici.',
  unchanged: 'Rien n’a changé ici, ni dans le code ou les assets.',
  showChanges: 'Afficher les modifications',
  hideChanges: 'Masquer les modifications',
  noChanges: 'Rien n’a changé ici.',

  readme: 'Comment il est construit',
  noReadme: (code) => (
    <>
      Rien n’explique comment il est construit. Un {code('README.md')} à la racine - les commandes, et où va le résultat
      du build - est ce à partir de quoi le prochain agent construit.
    </>
  ),
  readOnly: 'Lecture seule : il se modifie là où il est construit.',
  noFiles: 'Aucun fichier.',
};
