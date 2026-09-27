import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Comment ça marche',
  intro:
    'Vous écrivez un extrait de code C#. Ce qu’il renvoie est hébergé en quelques secondes à une adresse publique, en HTTPS. Cette page présente l’ensemble de la plateforme, dans l’ordre où vous la découvrirez.',
  contents: 'Sommaire',

  parts: {
    what: 'Qu’est-ce qu’un lambda ?',
    first: 'Votre premier lambda',
    editor: 'Le centre de contrôle',
    why: 'Documenter les changements',
    files: 'Plusieurs fichiers',
    page: 'Servir une page',
    spa: 'Un frontend, étape par étape',
    storage: 'Les deux emplacements des fichiers',
    keeping: 'Conserver des données',
    sockets: 'WebSockets',
    limits: 'Restrictions',
    away: 'Exporter votre code',
    agents: 'Travailler avec un agent',
  },

  what: [
    (k) => (
      <>
        Un lambda est un extrait de code qui renvoie un handler GenHTTP. La plateforme le compile, le charge et expose le
        résultat à votre propre adresse. Aucun projet, aucun fichier de build ni instruction {k.code('using')} ne sont
        nécessaires : tous les modules GenHTTP sont déjà importés.
      </>
    ),
    (k) => (
      <>
        Il s’agit d’un lambda complet. Déployé à l’adresse {k.code('/lambda/your-key/')}, il répond à chaque requête par le
        mot « hello ».
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      L’extrait se compose d’{k.em('instructions')}, et non d’une classe. Il se termine en renvoyant un objet capable de
      traiter les requêtes : un handler, ou un builder de handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Cliquez sur {k.b('Créer le lambda')}. Vous obtenez une adresse publique et une clé d’édition. Cette clé est le seul
        moyen d’y accéder et ne peut pas être récupérée : conservez-la précieusement.
      </>
    ),
    () => (
      <>
        Vous arrivez dans son centre de contrôle, où un petit service REST est déjà en place comme première version. Il
        sert uniquement de point de départ.
      </>
    ),
    (k) => (
      <>
        Confiez la clé d’édition à un agent en décrivant ce qu’il doit réaliser : il écrit de nouvelles versions via{' '}
        {k.link('/#agents', 'MCP')}. Vous pouvez aussi ouvrir {k.b('Code')} et écrire vous-même : {k.b('Vérifier')}{' '}
        compile sans enregistrer et affiche les messages du compilateur, avec fichier et ligne.
      </>
    ),
    (k) => (
      <>
        Cliquez sur {k.b('Déployer')}. Le lambda est alors en ligne ; rien n’est accessible auparavant. Un nouveau
        déploiement prolonge sa durée de mise en ligne.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Le lien d’édition ouvre un centre de contrôle plutôt qu’un simple éditeur de texte : la majeure partie du code étant
      écrite par des agents, l’écran présente d’abord l’état de votre lambda. La barre latérale indique s’il est en ligne,
      son adresse, un bouton lorsqu’une version plus récente attend d’être déployée, ainsi que ses différentes sections.
      Les actions ponctuelles, comme le changement d’adresse ou la suppression, se trouvent dans le menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Vue d’ensemble', () => <>L’état en ligne, le nombre de requêtes du jour et d’échecs, la dernière modification et l’espace disponible.</>],
    ['Fichiers', () => <>Les fichiers d’une version, et les données que le lambda enregistre pendant son exécution. Un cadenas ou un globe indique s’ils sont accessibles publiquement.</>],
    ['Versions', () => <>Les modifications et demandes associées à chaque version, et la différence avec la précédente. Déploiement ou retour arrière depuis cette section.</>],
    ['Déploiements', () => <>Ce qui était en ligne, à quel moment, et ce qui y a mis fin.</>],
    ['Statistiques', () => <>Requêtes, échecs, temps de réponse et chemins les plus demandés, sur la dernière heure ou les dernières 24 heures.</>],
    ['Journaux', () => <>Les requêtes, les sorties du lambda et la trace de pile de chaque erreur, en temps réel.</>],
    [
      'Code',
      (k) => (
        <>
          Édition manuelle. {k.b('Vérifier')} compile, {k.b('Enregistrer')} crée une version, {k.b('Déployer')} la met
          en ligne. {k.code('Ctrl+S')} enregistre ; {k.code('F12')} accède à une déclaration.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Toutes les sections sont organisées de la même façon : un titre, un {k.b('ⓘ')} qui l’explique, les actions à droite
      et – lorsqu’il existe plusieurs vues – une rangée de sélecteurs en dessous. Pour le code, ces sélecteurs sont les
      fichiers.
    </>
  ),
  editorAside:
    'Le trafic et le journal sont conservés en mémoire, pour le suivi et non pour l’archivage : un redémarrage du serveur les réinitialise. Les versions et l’historique des déploiements sont enregistrés durablement.',

  why: (k) => (
    <>
      Une version comprend le code et, en option, deux notes : {k.b('la spécification')}, c’est-à-dire ce que souhaite
      l’utilisateur et pourquoi, si possible dans ses propres termes, et {k.b('la modification')}, une ligne décrivant
      l’apport de la version. Elles s’affichent à côté du diff dans l’historique, de sorte que le {k.em('pourquoi')}{' '}
      reste associé au {k.em('quoi')} – pour vous, comme pour le prochain agent qui consultera l’historique avant
      d’intervenir.
    </>
  ),
  whySample: {
    specification: 'Un livre d’or à signer ; les entrées doivent survivre à un redémarrage',
    change: 'Enregistre les entrées dans le workspace pour qu’elles survivent à un redémarrage',
  },
  why2: (k) => (
    <>
      Les agents transmettent ces deux champs à {k.code('write_code')}. Dans {k.b('Code')}, l’enregistrement demande la
      modification. Les deux sont facultatifs ; une spécification trop longue est tronquée à 4000 caractères et une
      modification à 500, plutôt que refusée.
    </>
  ),

  files: (k) => (
    <>
      Les types n’ont pas besoin de figurer sous le code qui les utilise. Dans {k.b('Code')}, cliquez sur {k.b('+')} à
      côté des fichiers : le nouveau fichier est compilé avec l’extrait, dans le même espace de noms, sans import
      nécessaire. Un nom sans extension est traité comme un fichier C#.
    </>
  ),

  page: 'Trois approches sont possibles ; le choix dépend de l’emplacement de la page.',
  inlineTitle: 'Une page directement dans le code',
  inline: 'Adapté aux besoins simples. La page fait partie de l’extrait.',
  folderTitle: 'Un dossier de fichiers',
  folder:
    'L’approche adaptée dès qu’il y a une feuille de style et un script. Les fichiers s’ajoutent comme un fichier C# et sont servis tels qu’écrits, sans compilation.',
  workspaceTitle: 'Depuis le workspace',
  workspace: 'Lorsque la page est téléversée plutôt qu’écrite, et doit pouvoir changer sans nouveau déploiement.',

  spa: (k) => (
    <>
      La deuxième approche, en détail. Chaque démo sert sa page de cette façon depuis un dossier nommé {k.code('web')} –
      ouvrez {k.link('/editor/demo-crud', 'demo-crud')} pour en consulter une. Les démos sont en lecture seule ; leur clé
      d’édition correspond à leur nom.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Dans {k.b('Code')}, cliquez sur {k.b('+')} à côté des fichiers et saisissez {k.code('site/index.html')}. Un nom
        contenant une barre oblique place le fichier dans un dossier ; un nom avec extension est traité comme le type de
        fichier indiqué.
      </>
    ),
    (k) => (
      <>
        Ajoutez {k.code('site/app.css')} et {k.code('site/app.js')} de la même façon. Votre page y fait référence par leur
        nom, par exemple {k.code('href="app.css"')}, car le dossier constitue la racine de ce qui est servi et ne fait pas
        partie de l’adresse.
      </>
    ),
    (k) => (
      <>
        Pour les fichiers binaires, comme une image ou une police, ouvrez un fichier de {k.code('site')} et utilisez le
        bouton de téléversement à côté des fichiers : il est placé dans le même dossier.
      </>
    ),
    (k) => <>Dans {k.code('lambda.cs')}, servez le dossier :</>,
    (k) => (
      <>
        Cliquez sur {k.b('Déployer')}. {k.code('site/index.html')} répond sur {k.code('/')}, {k.code('site/app.css')} sur{' '}
        {k.code('/app.css')}, et toute adresse sans fichier correspondant renvoie la page : un frontend doté de son propre
        routage fonctionne donc aussi lors du rechargement d’un lien profond.
      </>
    ),
    () => <>Ajoutez une API à côté, avec laquelle la page pourra communiquer :</>,
  ],

  storage: (k) => (
    <>
      La section {k.b('Fichiers')} présente les deux – les fichiers d’une version et le workspace, sous le nom{' '}
      {k.b('Données')} – et indique lesquels sont accessibles publiquement. Les fichiers du code se modifient dans{' '}
      {k.b('Code')} ; les données peuvent être téléversées et supprimées dans {k.b('Fichiers')}. Ils diffèrent toutefois
      sur un point essentiel : {k.em('le moment où ils changent')}.
    </>
  ),
  savedWithCode: 'Enregistré avec le code',
  workspaceColumn: 'Workspace',
  table: [
    ['contenu', 'tous les fichiers du lambda, y compris le C#', 'tout ce qui a été écrit ou téléversé'],
    ['modification', 'lors de l’enregistrement ou du déploiement', 'dès qu’une écriture a lieu'],
    ['un déploiement', 'remplace l’ensemble', 'n’a aucun effet'],
    ['le retour à une version', 'restaure les anciens fichiers', 'aucun effet'],
    ['le clonage du lambda', 'est copié', 'n’est pas copié'],
  ],
  reachedAs: 'accessible dans le code via',
  storageAside:
    'Un répertoire unique n’est pas envisageable : un déploiement effacerait alors tout ce que le lambda a écrit depuis, ou rien ne pourrait jamais être retiré de ce qu’il publie. Un classement de jeu requiert la seconde propriété ; la page qui l’affiche, la première.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} est un répertoire privé que votre lambda peut lire et écrire. Il accueille tout ce qui doit
      survivre à une requête ou à un déploiement.
    </>
  ),
  keeping2: (k) => (
    <>
      Sont également disponibles {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, ainsi que {k.code('Tree')}/{k.code('Files')}/{k.code('App')} pour le servir. Le reste du
      système de fichiers est inaccessible.
    </>
  ),

  sockets: (k) => (
    <>
      Les WebSockets sont pleinement pris en charge. La démo {k.link('/editor/demo-game', 'demo-game')} met des joueurs en
      relation et exécute chaque partie sur le serveur. La forme la plus simple repose sur trois callbacks :
    </>
  ),
  socketsAside: (k) => (
    <>
      Un point d’attention fréquent : un navigateur ne peut pas définir d’en-têtes lors de la négociation WebSocket.
      Transmettez les valeurs nécessaires dans la query, où le handler les lit via{' '}
      {k.code('connection.Request.Header.Query')}, ou envoyez les informations confidentielles dans le premier message.
    </>
  ),

  limits:
    'Votre code s’exécute sur un serveur partagé ; certaines fonctionnalités de C# sont donc refusées avant compilation : lancer des processus, ouvrir vos propres sockets, charger des assemblies, accéder au système de fichiers hors de votre workspace, ainsi que la réflexion visant à contourner ces restrictions.',
  limits2:
    'Tout le reste est disponible, y compris l’intégralité de l’API des modules GenHTTP. En cas de refus, la ligne concernée et la raison vous sont indiquées.',

  away: (k) => (
    <>
      {k.b('Télécharger en tant que projet .NET')} dans l’éditeur fournit l’ensemble sous forme de projet .NET : une
      solution que vous pouvez ouvrir, exécuter avec {k.code('dotnet run')} et conserver. Elle ne comporte qu’une seule
      référence de package et aucune dépendance à cette plateforme.
    </>
  ),
  away2: (k) => (
    <>
      Votre extrait devient le corps de {k.code('Program.cs')}, intégré à un hôte qui sert ce qu’il renvoie. Vos autres
      fichiers sont repris tels quels. {k.code('Workspace')} et {k.code('Assets')} deviennent deux dossiers à côté du
      code, avec les mêmes méthodes : votre code n’a pas besoin d’être modifié.
    </>
  ),
  awayAside:
    'À savoir avant de commencer : le code que vous écrivez vous appartient et peut être exporté intégralement. L’exécuter sur cette plateforme ne vous y lie en rien.',

  agents: (k) => (
    <>
      Un point de terminaison MCP est disponible à l’adresse {k.code('/mcp')}. Un agent qui s’y connecte peut faire tout ce
      que permet l’éditeur : lire le guide, consulter une démo dans son intégralité, écrire des fichiers, les compiler et
      les déployer. Les deux reposent sur la même API.
    </>
  ),
  agents2: (k) => (
    <>
      L’agent documente ses choix au fil de l’eau – {k.code('write_code')} reçoit la spécification et la modification –
      et peut vérifier le résultat : {k.code('read_logs')} renvoie les requêtes récentes du lambda, ses sorties et la
      trace de pile de chaque exception. C’est ainsi qu’un agent constate que son code fonctionne, plutôt que de le
      supposer. Vous pouvez suivre les mêmes informations dans le centre de contrôle.
    </>
  ),
  more: 'En savoir plus →',
  make: 'Créer un lambda',
};
