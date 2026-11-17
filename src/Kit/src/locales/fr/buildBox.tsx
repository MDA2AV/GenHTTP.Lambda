import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Je voudrais un site web qui…',
  password: 'mot de passe',
  fable:
    'Fable est protégé par un mot de passe pendant la phase de test. Il n’a pas de limite de temps : il continue jusqu’à ce que votre site web soit fini, pas jusqu’à la fin du chrono.',
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
    `Nous le gardons en ligne tant qu’il est utilisé : après ${offline} jours sans visite ni modification, il est mis hors ligne, puis supprimé après ${removed} jours. Ouvrez l’éditeur pour le remettre en ligne.`,
  openEditor: 'Ouvrir l’éditeur',

  failedToStart: 'La demande n’est pas passée.',
  noAnswer: 'C’est terminé, mais sans dire ce qui s’est passé.',
  failed: 'Ça n’a pas marché.',
};
