import type { Messages } from '../en';

export const platform: Messages['platform'] = {
  menu: 'Fonctionnalités',

  eyebrow: 'Fonctionnalités',
  title: 'La création prend une minute.',
  titleAccent: 'Le reste est déjà intégré.',
  intro:
    'Une app créée ici, ce n’est pas seulement le code écrit par une IA. Ce qui la rend utile vient après : ses entrées, ses versions et son objectif, savoir qui la visite, un domaine à elle quand elle prend son essor – le tout au même endroit, ouvert avec le lien que vous recevez à sa création.',
  build: 'Créer quelque chose',
  ownAgent: 'Utiliser votre propre agent',
  free: 'Gratuit. Sans inscription, sans carte bancaire.',
  heroAlt: 'L’éditeur d’une app de réservation de courts : en ligne sur le domaine du club, combien de personnes l’ont utilisée aujourd’hui, et sa dernière modification',
  example: 'Toutes les images de cette page montrent la même app d’exemple : la réservation des courts d’un club de tennis.',

  contents: 'Sur cette page',

  makeTitle: 'La créer, la modifier',
  makeText: 'Dites ce dont vous avez besoin, et continuez à le dire. Personne n’a besoin d’écrire ni de lire du code pour cela.',

  behindTitle: 'Derrière chaque app',
  behindText:
    'Ce qui rend une app utile, c’est tout ce qui entoure son code – là pour celui qui l’a créée, pas seulement pour celui qui sait lire du code.',

  yoursTitle: 'À vous d’aller plus loin',
  yoursText: 'Pour les développeurs, et pour tous ceux qui veulent que leur app survive à ce site.',

  features: {
    build: {
      name: 'Créer',
      title: 'Décrivez-la, et elle est en ligne',
      text: 'Dites à notre agent ce dont vous avez besoin, avec vos mots, ou demandez à Claude Code, Codex ou Cursor. Quelques minutes plus tard, l’app tourne à sa propre adresse, prête à être partagée. Pas de serveur, de base de données ni de compte à configurer.',
      points: ['Notre agent sur la page de création, ou le vôtre via MCP', 'Un lien à partager en quelques minutes', 'Gratuit, sans inscription'],
      alt: 'La page de réservation des courts : quatre courts, heure par heure, avec les heures réservées nommées',
    },
    change: {
      name: 'Modifier',
      title: 'Modifiez-la en demandant',
      text: 'Chaque app a un éditeur, ouvert avec le lien d’édition reçu à sa création. Dites ce qui doit changer : l’agent modifie l’app et vous prévient quand regarder. Ou donnez le lien à votre propre agent avec la prochaine modification.',
      points: ['Une phrase suffit', 'En ligne automatiquement dès que ça marche, ou gardée pour que vous l’essayiez d’abord', 'Le code reste caché, sauf si vous le voulez'],
      alt: 'La section Modifier : le club a demandé les cours du coach, et l’agent indique que la modification est prête à être essayée',
    },
    drafts: {
      name: 'Brouillons',
      title: 'Essayez une modification avant que quiconque la voie',
      text: 'Une modification plus importante se fait dans un brouillon : l’app telle qu’elle sera, à sa propre adresse, avec une copie des entrées pour l’essayer. Rien de ce qui s’y passe ne touche les vraies entrées. Quand tout est bon, un clic la met en ligne.',
      points: ['Sa propre adresse, pour l’essayer et la montrer', 'Données de test : une copie des entrées, pas les entrées elles-mêmes', 'Mis en ligne quand c’est bon, ou abandonné'],
      alt: 'Un brouillon de la page de réservation, où les cours du coach apparaissent en violet, dans une copie des réservations',
    },
    history: {
      name: 'Historique',
      title: 'Chaque modification devient une version, avec un retour possible',
      text: 'Chaque modification devient une nouvelle version, décrite par ce qu’elle a changé. Si l’une tourne mal, la version précédente est à un clic, à la même adresse, avec les mêmes entrées.',
      points: ['Ce qui a été demandé, et ce que cela a changé', 'Retour à une version antérieure en un clic', 'Les entrées sont communes à toutes les versions : revenir en arrière n’en fait perdre aucune'],
      alt: 'L’historique de l’app : ses modifications, les unes sous les autres, la plus récente en ligne',
    },
    about: {
      name: 'À propos',
      title: 'Elle sait ce qu’elle est',
      text: 'À chaque modification, l’agent note à quoi sert l’app, qui l’utilise et pourquoi elle fonctionne ainsi. Vous le lisez en mots simples, et l’agent suivant le lit avant de rien changer : une modification s’appuie ainsi sur l’existant au lieu de deviner.',
      points: ['Ce qu’elle fait et pour qui, en mots simples', 'Les décisions qui la sous-tendent, pour celui qui la modifiera ensuite', 'Comment elle est testée, conservé avec chaque version'],
      alt: 'À propos de votre app : la réservation des courts décrite en mots simples, qui l’utilise et ce qu’elle fait',
    },
    records: {
      name: 'Entrées',
      title: 'Des entrées dans une base de données à elle',
      text: 'Une app qui garde des choses – réservations, inscriptions, scores – les range dans sa propre base de données, et elles restent quand l’app change. Lisez-les table par table dans l’éditeur, à côté des fichiers qu’elle a enregistrés, comme la liste hebdomadaire pour le panneau d’affichage.',
      points: ['Une base de données pour chaque app qui en a besoin', 'Chaque table, lisible dans l’éditeur', 'Les fichiers enregistrés par l’app, listés à côté'],
      alt: 'Les entrées de l’app : ses réservations et ses membres, lues table par table',
    },
    keys: {
      name: 'Clés et mots de passe',
      title: 'Les clés et mots de passe restent secrets',
      text: 'Quand une app a besoin d’un mot de passe ou de la clé d’un autre service, l’agent ne la demande que par son nom. Vous saisissez la valeur dans l’éditeur, et dès lors personne ne la voit, ni vous, ni l’agent. Seule l’app la lit.',
      points: ['L’agent ne voit jamais la valeur', 'Conservée chiffrée, jamais réaffichée', 'Remplacée quand vous voulez'],
      alt: 'Clés et mots de passe : le mot de passe de l’accueil est défini, et sa valeur est masquée',
    },
    analytics: {
      name: 'Audience',
      title: 'Voyez qui la visite, sans cookies',
      text: 'Activez l’audience et voyez combien de personnes ont ouvert l’app, d’où elles viennent et sur quels appareils, le tout compté sur notre serveur, sans cookies et sans script dans vos pages.',
      points: ['Visiteurs, pages vues et provenance', 'Pas de cookies, et rien qui désigne un visiteur', 'Désactivée tant que vous ne l’activez pas'],
      alt: 'Audience : les visiteurs des trente derniers jours, jour par jour',
    },
    domain: {
      name: 'Domaine',
      title: 'Un domaine à elle',
      text: 'Chaque app démarre à une adresse à nous. Quand elle prend, elle peut passer sur un domaine qui vous appartient : la même app et le même éditeur, à une adresse dont on se souvient. Les liens vers l’ancienne adresse continuent de marcher : ils mènent à la nouvelle.',
      points: ['Pour les apps Premium : écrivez-nous', 'La même app, le même éditeur, les mêmes entrées', 'L’ancienne adresse renvoie les visiteurs'],
      alt: 'L’adresse de l’app passant de son adresse sur genhttp.run au domaine du club',
    },
    code: {
      name: 'Code et git',
      title: 'Le code, quand vous le voulez',
      text: 'Passez l’éditeur en vue complète, et le code de chaque version est là, avec ses journaux et ses chiffres. Clonez l’app avec git et poussez une modification, ou téléchargez-la comme un projet .NET qui tourne partout. C’est votre code.',
      points: ['Chaque fichier, comparé d’une version à l’autre', 'Git : un commit sur main est une version, une branche est un brouillon', 'Un téléchargement autonome, entrées comprises'],
      alt: 'La vue complète de l’éditeur : les fichiers de l’app et son code',
    },
    publish: {
      name: 'Vitrine et sources',
      title: 'Montrez-la, ou publiez son code',
      text: 'Placez l’app dans la vitrine pour que d’autres la trouvent, ou publiez son code sous une licence open source pour que chacun puisse le lire, l’aimer et le télécharger. Les deux sont votre choix, et aucun ne partage ses entrées.',
      points: ['Dans la vitrine, avec une image et une description', 'Son code sous MIT ou une autre licence', 'Jamais ses entrées, ses clés ni ses visiteurs'],
      alt: 'Le code publié de la réservation des courts : ses fichiers, sa documentation et ses versions, sous licence MIT',
    },
  },

  draftBadge: 'Brouillon',
  domainOwn: 'Premium',
  domainSubject: 'Un domaine pour mon app',
  domainAsk: 'Se renseigner sur un domaine',
  zoom: 'Agrandir',
  close: 'Fermer',

  closeTitle: 'Gratuit à essayer.',
  closeAccent: 'Sans inscription.',
  closeBuild: 'Créer quelque chose',
  closeShip: 'Publier ce que vous avez créé',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda est open source. Utilisez-la ici, ou {selfHosting('faites-la tourner sur votre propre serveur')} : le code est sur{' '}
      {github('GitHub')}.
    </>
  ),
};
