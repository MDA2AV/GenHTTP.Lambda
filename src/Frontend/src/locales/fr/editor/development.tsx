import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Espace de développement',
  hint: 'Ce à partir de quoi les assets d’une version sont construits, là où une chaîne d’outils les construit : le projet de son front-end, avec ses sources, sa configuration et son fichier de verrouillage. Il est conservé avec chaque version et jamais compilé ni servi. Celui qui le modifie - votre agent, dans un clone - le construit là où il travaille et l’enregistre avec ce qu’il a construit : cette plateforme ne construit rien. Il se lit donc ici, sans s’y modifier.',
  overview: 'Vue d’ensemble',
  files: 'Fichiers',
  scope: (version) =>
    `Ce à partir de quoi les assets de la version ${version} sont construits - conservé avec elle, jamais compilé ni servi, et construit par celui qui le modifie, jamais ici.`,
  scopeDraft: 'Ce à partir de quoi les assets de ce brouillon sont construits - conservé avec lui, jamais compilé ni servi, et construit par celui qui le modifie, jamais ici.',
  reading: 'Lecture de l’espace de développement…',
  readFailed: 'L’espace de développement n’a pas pu être lu.',

  emptyTitle: (version) => `Pas d’espace de développement dans la version ${version}`,
  emptyTitleDraft: 'Pas d’espace de développement dans ce brouillon',
  emptyText: (code) => (
    <>
      Lorsqu’un front-end est construit avec une chaîne d’outils - React, Vue ou Svelte avec Vite, TypeScript,
      Tailwind - son projet est conservé ici, avec chaque version : ce à partir de quoi les assets sont construits.
      Votre agent le construit là où il travaille et enregistre les sources avec ce qu’elles ont produit - dans un
      clone, c’est le dossier {code('dev/')}. Un front-end en HTML, CSS et JavaScript simples n’en a pas besoin.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} dans un clone explique à un agent de code comment en mettre un en place.</>
  ),

  projects: 'Projets',
  atTheTop: 'l’espace de développement lui-même',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: 'Construit avec',
  build: 'Build',
  noBuild: 'Pas de script de build dans son package.json.',
  into: 'Construit dans',
  intoAssets: (folder, files, size) => (
    <>
      {folder} des assets - {files === 1 ? '1 fichier' : `${files} fichiers`}, {size} dans cette version
    </>
  ),
  intoNothing: (folder) => <>{folder} des assets - qui ne contient rien dans cette version</>,
  packages: 'Paquets',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? '1 pour l’exécution' : `${runtime} pour l’exécution`}, ${tooling === 1 ? '1 pour le build' : `${tooling} pour le build`}`,
  showPackages: 'Les afficher',
  hidePackages: 'Les masquer',
  runtime: 'Pour l’exécution',
  tooling: 'Pour le build',
  missing: (page, files) => (
    <>
      {page} fait référence à {files.length === 1 ? 'un fichier' : `${files.length} fichiers`} qui {files.length === 1 ? 'ne figure' : 'ne figurent'} pas
      parmi les assets ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}) : ce que le build a écrit n’a
      pas été enregistré en entier, et la page ne se charge pas.
    </>
  ),
  noLock: 'Pas de fichier de verrouillage : le prochain build peut installer d’autres versions de ses paquets que le précédent.',
  noIgnore: 'Pas de .gitignore : ce que sa chaîne d’outils installe et construit peut se retrouver dans une version.',

  inVersion: (version) => `Dans la version ${version}`,
  inDraft: 'Dans ce brouillon',
  comparedWith: (version) => `par rapport à la version ${version}`,
  first: 'La première version qui en a un.',
  both: (here, assets) =>
    `${here === 1 ? '1 fichier modifié' : `${here} fichiers modifiés`} ici, et ${assets === 1 ? '1 fichier' : `${assets} fichiers`} des assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 fichier modifié' : `${here} fichiers modifiés`} ici, et aucun des assets : sauf si la modification n’exigeait pas de build, les visiteurs obtiennent ce qu’ils obtenaient avant.`,
  builtOnly: (folder) => (
    <>Ce qui est construit dans {folder} a changé, et rien ici : une modification faite dans ce que le build a écrit est annulée par le prochain build.</>
  ),
  assetsOnly: 'Rien n’a changé ici.',
  unchanged: 'Rien n’a changé ici ni dans les assets.',
  showChanges: 'Afficher les modifications',
  hideChanges: 'Masquer les modifications',
  noChanges: 'Rien n’a changé ici.',

  readme: 'Comment il est construit',
  noReadme: (code) => (
    <>
      Rien n’explique comment il est construit. Un {code('README.md')} à la racine de l’espace de développement - les
      commandes, et l’endroit où va le build - est ce à partir de quoi le prochain agent construit.
    </>
  ),
  readOnly: 'Lecture seule : il se modifie là où il est construit.',
  noFiles: 'Aucun fichier.',
};
