import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Édition Community',
  headline: 'La plateforme sur laquelle vos agents créent des applications.',
  headlineAccent: 'Sur votre propre serveur.',
  intro:
    'Décrivez une application et l’agent de création l’écrit – ou laissez Claude Code, Codex ou Cursor le faire via MCP. Chaque application est un handler GenHTTP en C#, compilé sur ce serveur et en ligne en quelques secondes, avec une base de données, des clés et mots de passe, des versions et git.',
  describe: 'Décrire une application',
  connect: 'Connecter votre agent',
  write: 'Écrire le code vous-même',

  buildTitle: 'Décrivez-la',
  buildText: 'Une phrase suffit. L’agent de création écrit l’application, la met en ligne et vous donne le lien pour la modifier plus tard.',
  noAgent: 'Aucun agent de création n’est connecté à cette installation. Connectez votre propre agent ci-dessous – il fait la même chose.',

  connectTitle: 'Connectez votre agent',
  connectText:
    'Votre agent de code reçoit la plateforme sous forme d’outils : créer une application, écrire et vérifier son code, essayer une modification dans un brouillon avec une copie des données, la mettre en ligne et lire ses journaux.',
  connectThen: 'Demandez-lui ensuite quelque chose, par exemple : « Crée un livre d’or, avec une page où j’approuve les entrées. »',

  featuresTitle: 'Ce que reçoit chaque application',
  features: [
    { title: 'Une adresse', text: 'En ligne sous /lambda/ sur ce serveur dès qu’elle compile – ou sur un domaine à elle.' },
    { title: 'Entrées', text: 'Une base de données SQLite à elle, ses tables créées par des migrations, lues et écrites avec Entity Framework Core.' },
    { title: 'Fichiers', text: 'Un dossier pour ce que l’application enregistre : téléversements, exports, tout ce qui n’est pas une entrée.' },
    { title: 'Clés et mots de passe', text: 'Conservés sous scellés sur le serveur. L’agent indique ceux dont il a besoin, vous les saisissez, et personne ne peut les relire.' },
    { title: 'Brouillons et versions', text: 'Une modification est essayée dans un brouillon avec une copie des données, puis mise en ligne. Chaque version est conservée, à un clic pour revenir en arrière.' },
    { title: 'Git', text: 'Chaque application est un dépôt : clonez-le, poussez un commit pour la mettre en ligne, poussez une branche pour commencer un brouillon.' },
    { title: 'Visiteurs', text: 'Pages vues, provenance des visiteurs et leurs pays – comptés sur le serveur, sans cookies, dès que vous l’activez.' },
    { title: 'Votre code', text: 'Téléchargez n’importe quelle application comme un simple projet .NET et exécutez-la où vous voulez, ou publiez son code source pour que d’autres le lisent.' },
  ],

  runsTitle: 'Comment elle fonctionne',
  runs: [
    { title: 'Un seul conteneur', text: 'Le serveur, l’éditeur, l’API et toutes les applications dans un seul processus, et tout ce qu’ils conservent dans un seul dossier.' },
    {
      title: 'Par chemin ou par hôte',
      text: 'Les applications répondent sous /lambda/ à cette adresse. Donnez un domaine à l’installation pour elles, et chacune reçoit à la place un sous-domaine à elle.',
    },
    {
      title: 'Du code de confiance',
      text: 'Les applications sont compilées dans le serveur et leur code est vérifié pour ce qu’elles ne doivent pas faire – mais elles partagent son processus. Exécutez ce que vous exécuteriez vous-même.',
    },
    { title: 'Open source', text: 'Sous licence MIT : lisez-le, modifiez-le, exécutez-le où vous voulez.' },
  ],
  repository: 'Le code source sur GitHub',

  aroundTitle: 'Jetez un œil',
  around: {
    demos: { title: 'Partir d’une démo', text: 'Une API sur des entrées, une connexion, des mises à jour en direct, un jeu : chacune est une application terminée à copier et modifier.' },
    showcase: { title: 'Vitrine', text: 'Les applications que leurs propriétaires ont choisi de montrer ici.' },
    guide: { title: 'Guide', text: 'Ce qu’une application peut faire et comment, dans l’ordre où vous en avez besoin.' },
    api: { title: 'API REST', text: 'La même chose que les outils, via HTTP, décrite en OpenAPI.' },
    sources: { title: 'Sources publiées', text: 'Des applications dont les propriétaires ont publié le code, à lire et à télécharger.' },
    admin: { title: 'Administration', text: 'Les applications, les limites, les blocages et le journal, derrière le jeton d’administration.' },
  },
};
