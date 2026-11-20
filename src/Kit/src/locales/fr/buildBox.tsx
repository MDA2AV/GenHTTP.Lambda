import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Je voudrais un site web qui…',
  password: 'mot de passe',
  unlimited:
    'Ce modèle demande le mot de passe, puis fonctionne sans limite de temps : il continue jusqu’à ce que votre site soit terminé, et non jusqu’à la fin du délai.',
  ideas: [
    'un site pour notre association où les membres s’inscrivent aux événements',
    'une liste pour notre auberge espagnole, pour que personne n’apporte le même plat',
    'un livre d’or pour notre mariage',
    'un sondage où chacun vote et voit les résultats',
    'un classement pour notre soirée quiz hebdomadaire',
    'une page d’anniversaire où les amis laissent leurs vœux',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Un site passe avant le vôtre : vous êtes le suivant.' : `${waiting} sites passent avant le vôtre.`,
  starting: 'Démarrage…',
  online: 'Votre site est en ligne',
  notOnline: 'Votre site a été créé, mais il n’a pas été mis en ligne.',
  open: 'Ouvrir votre site',
  steps: {
    guide: 'Préparation',
    examples: 'Coup d’œil à des exemples',
    create: 'Choix d’une adresse pour votre site',
    write: 'Écriture de votre site',
    improve: 'Amélioration de votre site',
    check: 'Recherche d’erreurs',
    online: 'Mise en ligne',
    trying: 'Essai du site',
    looking: 'Relecture de votre site',
    forRecords: 'Préparation de l’espace pour ses entrées',
    forKeys: 'Préparation de l’espace pour les clés et mots de passe',
    forFiles: 'Préparation de l’espace pour ce qu’il enregistre',
    records: 'Coup d’œil à ses entrées',
    keys: 'Vérification des clés et mots de passe nécessaires',
    addFile: 'Ajout d’un fichier',
    removeFile: 'Suppression d’un fichier',
    files: 'Coup d’œil à ce qu’il a enregistré',
    services: 'Demande d’utilisation d’autres services',
  },
  further: 'Votre lien d’édition',
  keep:
    'Gardez ce lien en lieu sûr. C’est le seul moyen d’accéder à votre site depuis un autre appareil, et personne ne peut le récupérer, pas même nous.',
  copyLink: 'Copier le lien d’édition',
  lifetime: (offline, removed) =>
    `Nous le gardons en ligne tant qu’il est utilisé : après ${offline} jours sans visite ni modification, il est mis hors ligne, puis supprimé après ${removed} jours. Vous pouvez le remettre en ligne ici, ou dans son éditeur.`,

  failedToStart: 'La demande n’est pas passée.',
  noAnswer: 'C’est terminé, mais sans dire ce qui s’est passé.',
  failed: 'Ça n’a pas marché.',
  profiles: {
    enterprise: {
      placeholder: 'Il nous faut une app qui…',
      unlimited:
        'Ce modèle demande le mot de passe, puis fonctionne sans limite de temps : il continue jusqu’à ce que votre app soit terminée, et non jusqu’à la fin du délai.',
      ideas: [
        'un formulaire de demande de congés, approuvé par le chef d’équipe',
        'un calendrier de réservation de nos salles de réunion',
        'une liste de contrôle pour l’accueil des nouveaux collègues',
        'un inventaire de notre matériel et de qui l’utilise',
        'une liste des demandes d’assistance ouvertes et de la personne qui s’en occupe',
        'un formulaire de retour sur nos formations',
      ],
      ahead: (waiting) => (waiting === 1 ? 'Une app passe avant la vôtre : vous êtes le suivant.' : `${waiting} apps passent avant la vôtre.`),
      online: 'Votre app est en ligne',
      notOnline: 'Votre app a été créée, mais elle n’a pas été mise en ligne.',
      open: 'Ouvrir votre app',
      steps: {
        create: 'Choix d’une adresse pour votre app',
        write: 'Écriture de votre app',
        improve: 'Amélioration de votre app',
        looking: 'Relecture de votre app',
      },
      keep: 'Gardez ce lien en lieu sûr. C’est le seul moyen d’accéder à votre app depuis un autre appareil, et personne ne peut le récupérer, pas même les personnes qui gèrent ce site.',
    },
  },
};
