import type { SourceMessages } from '../en/source';

/** Un nombre et son nom, au singulier pour 0 et 1 comme on le dit en français. */
const count = (value: number, one: string, many: string) => `${value} ${Math.abs(value) < 2 ? one : many}`;

/**
 * Les textes des pages du code publié, /source et chaque projet en dessous, en français. Les espaces
 * insécables avant : ; ? ! et dans « » sont écrites   (&nbsp; dans le JSX).
 */
export const source: SourceMessages = {
  shell: {
    section: 'Open source',
    home: 'GenHTTP Lambda, page d’accueil',
  },

  lambda: {
    label: 'Qu’est-ce qu’une lambda ?',
    text: 'Une application web sur GenHTTP Lambda : quelqu’un décrit ce qu’il veut, un agent IA l’écrit en C#, et en quelques minutes elle est en ligne à sa propre adresse – chaque version est gardée, avec ce qu’elle a changé.',
    build: 'Créer la vôtre',
  },

  catalog: {
    eyebrow: 'Open source',
    title: 'Découvrez comment sont faites les applications d’ici',
    intro:
      'Des lambdas dont les auteurs ont publié le code : chaque version, ce qu’elle a changé, sa documentation et ses tests. Lisez-le ici, ou téléchargez un projet qui tourne partout où tourne .NET.',
    searchLabel: 'Rechercher dans les projets',
    searchPlaceholder: 'Rechercher par nom ou par fonction',
    orderLabel: 'Tri',
    orders: {
      stars: 'Les plus étoilés',
      updated: 'Modifiés récemment',
      published: 'Publiés récemment',
    },
    counted: (total) => count(total, 'projet', 'projets'),
    failed: 'Impossible de charger les projets.',
    loadingMore: 'Chargement de la suite…',
    showMore: 'Afficher plus',
    nothingTitle: 'Rien de publié pour l’instant',
    nothing: (tab) => (
      <>
        Vous avez construit quelque chose dont d’autres pourraient s’inspirer&nbsp;? Ouvrez le tableau de bord de votre
        lambda, choisissez {tab('Open source')} puis une licence&nbsp;: son code apparaîtra ici.
      </>
    ),
    noMatchTitle: 'Aucun résultat',
    noMatch: (query) => `Aucun projet publié ne mentionne « ${query} ».`,
    clear: 'Afficher tous les projets',
    yoursTitle: 'Publiez le vôtre',
    yours: (tab) => (
      <>
        Ouvrez le tableau de bord de votre lambda et choisissez {tab('Open source')}, ou demandez à l’agent qui l’a
        construite de la publier. Seule la personne qui a la clé d’édition peut le faire, sous la licence de son choix – et
        ce que garde l’application, ses enregistrements, ses fichiers et ses clés, n’en fait jamais partie.
      </>
    ),
    build: 'Créer quelque chose',
    online: 'En ligne',
    offline: 'Hors ligne',
    changed: (ago) => `modifié ${ago}`,
    stars: (value) => count(value, 'étoile', 'étoiles'),
  },

  project: {
    loading: 'Chargement du code source…',
    failed: 'Impossible de charger le code source.',
    missingTitle: 'Aucun code source publié ici',
    missing: 'Son auteur l’a peut-être retiré, ou il n’y a jamais eu de lambda à cette adresse.',
    all: 'Tous les projets',
    by: (name) => `par ${name}`,
    versions: (value) => count(value, 'version', 'versions'),
    onlineAt: (address) => <>En ligne à l’adresse {address}</>,
    offline: 'Hors ligne pour le moment',
    openApp: 'Ouvrir l’application',
    opens: (address) => `Ouvre ${address} dans un nouvel onglet`,
    published: (ago) => `Publié ${ago}`,
    changed: (ago) => `Modifié ${ago}`,
    picture: (name) => `Aperçu de ${name}`,
    tabsLabel: 'À lire',
    tabs: {
      code: 'Code',
      docs: 'Documentation',
      tests: 'Tests',
      changes: 'Modifications',
    },
  },

  versions: {
    label: 'Version',
    choose: 'Lire une autre version',
    newest: 'la plus récente',
    online: 'en ligne',
    older: (version, ago, newest) =>
      `Vous lisez la version ${version}, enregistrée ${ago}. La plus récente est la version ${newest}.`,
    toNewest: 'Lire la plus récente',
    noChange: 'Aucune note sur ce qu’elle a changé',
  },

  star: {
    star: 'Étoile',
    add: 'Donner une étoile à ce projet',
    remove: 'Retirer votre étoile',
    count: (value) => count(value, 'étoile', 'étoiles'),
    failed: 'Impossible d’enregistrer l’étoile.',
  },

  download: {
    button: 'Télécharger',
    title: (version) => `Version ${version} sous forme de projet`,
    what:
      'Un projet .NET 10 avec un Dockerfile, sa documentation, ses tests et sa licence. Ce que garde l’application – ses enregistrements, les fichiers qu’elle a enregistrés, ses clés – n’en fait pas partie.',
    zip: 'Télécharger le ZIP',
    preparing: 'Préparation du projet…',
    slow: 'Au premier téléchargement, la version est empaquetée pendant que vous patientez.',
    failed: 'Impossible de préparer le projet. Réessayez dans un instant.',
    run: 'Le lancer',
    local: 'Avec le SDK .NET 10 :',
    container: 'Ou dans un conteneur :',
    agent: 'Ou confiez le dossier à votre agent de code et construisez à partir de là, dans le respect de sa licence.',
    copy: 'Copier',
    copied: 'Copié',
  },

  tree: {
    label: 'Fichiers',
    files: (value) => count(value, 'fichier', 'fichiers'),
    packing: 'Empaquetage de cette version…',
    packingSlow: 'Une version est empaquetée la première fois que quelqu’un la lit, ce qui prend un moment si elle est grande.',
    failed: 'Impossible de charger les fichiers de cette version.',
    legend: 'Légende',
    kinds: {
      code: 'Le code propre à la lambda',
      asset: 'Ce qu’elle sert : pages, scripts, styles, images – et ses migrations de base de données',
      docs: 'Ce qu’elle est, et pourquoi elle est construite ainsi',
      tests: 'Comment elle est testée',
      platform: 'Ce qui remplace la plateforme',
      project: 'L’hôte, le build, le conteneur et la licence',
    },
    short: {
      code: 'Code',
      asset: 'Servi',
      docs: 'Docs',
      tests: 'Tests',
      platform: 'Plateforme',
      project: 'Projet',
    },
  },

  file: {
    loading: 'Chargement…',
    failed: 'Impossible de charger ce fichier.',
    missing: (path) => `Il n’y a pas de ${path} dans cette version.`,
    binary: 'Ce fichier n’est pas du texte.',
    tooLarge: 'Ce fichier est trop long pour être affiché ici.',
    download: 'Télécharger',
    raw: 'Brut',
    rawTitle: 'Ouvrir le fichier tel quel',
    copy: 'Copier',
    copied: 'Copié',
    lines: (value) => count(value, 'ligne', 'lignes'),
    plain: 'Affiché sans couleurs : il est long.',
    line: (line) => `Ligne ${line}`,
  },

  docs: {
    pages: 'Pages',
    product: 'Ce que c’est',
    decisions: 'Décisions',
    loading: 'Chargement…',
    failed: 'Impossible de charger cette page.',
    noneTitle: 'Rien n’est écrit sur cette version',
    none: 'Sa documentation se trouverait dans docs/ : ce qu’est l’application, à qui elle s’adresse, et pourquoi elle est construite ainsi.',
  },

  tests: {
    files: 'Scripts et données',
    noneTitle: 'Cette version ne dit rien de ses tests',
    none: 'La façon dont elle est testée se trouverait dans tests/README.md, avec à côté les scripts qu’elle lance.',
  },

  changes: {
    title: 'Toutes les versions, de la plus récente à la plus ancienne',
    intro: 'Une version ne change plus une fois enregistrée. Chacune dit en une ligne ce qu’elle a changé.',
    agent: 'Écrit par un agent',
    online: 'en ligne',
    browse: 'Lire le code',
    noChange: 'Pas de note',
  },

  licenses: {
    MIT: 'Chacun peut l’utiliser, le modifier et le diffuser, dans n’importe quel projet, tant que la licence et la mention de copyright l’accompagnent.',
    'Apache-2.0': 'Comme MIT, avec en plus une licence de brevet de chaque contributeur, et les modifications signalées comme telles.',
    'BSD-3-Clause': 'Comme MIT, et personne ne peut utiliser le nom des auteurs pour promouvoir ce qu’il en a fait.',
    'MPL-2.0': 'Les modifications de ces fichiers restent sous la même licence ; ils peuvent être combinés avec du code sous n’importe quelle autre.',
    'GPL-3.0-or-later': 'Quiconque le diffuse, modifié ou non, diffuse aussi son code source sous la même licence.',
    'AGPL-3.0-or-later': 'Comme la GPL, et proposer une copie modifiée à d’autres via le réseau compte comme une diffusion.',
    Unlicense: 'Versé dans le domaine public : chacun peut en faire ce qu’il veut, sans condition.',
  },

  kinds: {
    Permissive: 'Permissive',
    Copyleft: 'Copyleft',
    PublicDomain: 'Domaine public',
  },
};
