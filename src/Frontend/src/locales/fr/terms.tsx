import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Conditions d’utilisation',
  binding: (english) => (
    <>Cette traduction est fournie à titre informatif. Seule la {english('version anglaise')} fait foi.</>
  ),
  intro:
    'Ce service gratuit est destiné à l’expérimentation. Il exécute du code écrit par des tiers sur une infrastructure partagée, ce qui n’est possible que si chacun respecte quelques règles.',
  sections: {
    forbiddenTitle: 'Contenus interdits',
    forbidden: [
      'Aucun logiciel malveillant, aucun hameçonnage, aucun mineur de cryptomonnaie. Rien qui attaque, analyse, surcharge ou perturbe d’autres systèmes, ici ou ailleurs. Rien qui harcèle quiconque. Rien que vous n’avez pas le droit de publier, y compris le code, les textes, les images et les marques de tiers.',
      'N’utilisez pas un lambda pour stocker ou transmettre des données personnelles concernant d’autres personnes. Une adresse publique n’a rien de privé, et cette plateforme n’offre aucun moyen de protéger de telles données.',
    ],
    actionTitle: 'Nos mesures',
    action:
      'Tout contenu déployé ici peut être mis hors ligne ou supprimé à tout moment, sans préavis ni obligation de justification. Cela se produit notamment en cas de non-respect des règles ci-dessus, de menace pour le serveur partagé ou de signalement fondé.',
    lastingTitle: 'Durée de conservation',
    lasting: (hours, days) =>
      `Un déploiement reste accessible environ ${hours} heures. Un lambda que vous n’avez pas ouvert est supprimé, avec toutes les versions de son code, environ ${days} jours après votre dernière intervention. L’enregistrement et le déploiement comptent comme des interventions : un lambda sur lequel vous travaillez est donc conservé. Ce service ne constitue pas une sauvegarde : conservez votre propre copie de tout code important.`,
    keyTitle: 'Votre lien d’édition tient lieu de mot de passe',
    key: 'Toute personne disposant du lien d’édition peut lire et modifier le lambda correspondant ; aucun compte ni mot de passe n’y est associé. Publier ce lien revient à donner à d’autres la possibilité de le modifier. Un lien perdu ne peut pas être récupéré.',
    warrantyTitle: 'Absence de garantie',
    warranty:
      'Le service est fourni en l’état, sans garantie de fonctionnement, de continuité ni de conservation des contenus. Il peut être redémarré, modifié ou arrêté à tout moment. N’y hébergez rien d’important pour vous ou pour des tiers.',
    reportTitle: 'Signalements',
    report: (mailbox, front) => (
      <>
        Si un lambda hébergé ici a un comportement inapproprié, écrivez à {mailbox} en indiquant son adresse. Les
        informations utiles sont précisées sur la {front('page d’accueil')}.
      </>
    ),
  },
  change: 'Ces conditions peuvent évoluer. La version applicable est celle publiée sur cette page.',

  short:
    'Les lambdas s’exécutent sur une infrastructure partagée. En créant un lambda, vous vous engagez à ne déployer ni logiciel malveillant, ni page d’hameçonnage, ni mineur de cryptomonnaie, ni rien qui attaque, analyse ou surcharge d’autres systèmes, et à ne publier aucun contenu sans en détenir les droits. Toute personne connaissant le lien d’édition peut modifier votre lambda : traitez-le comme un mot de passe. Dans l’offre gratuite, les lambdas restent en ligne tant qu’ils sont utilisés : un lambda sans visite ni modification pendant un mois est mis hors ligne, puis supprimé après deux mois supplémentaires sans activité. Tout contenu déployé peut être supprimé à tout moment.',
};
