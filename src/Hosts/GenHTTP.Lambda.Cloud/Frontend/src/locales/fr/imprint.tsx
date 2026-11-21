import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Mentions légales',
  intro:
    'Qui édite ce site. La loi allemande demande à tout site exploité depuis l’Allemagne de donner ces informations à un seul endroit, facile à trouver (§ 5 DDG) : le voici.',
  sections: {
    providerTitle: 'Éditeur du site',
    contactTitle: 'Contact',
    contact: (mail, abuse) => (
      <>
        E-mail : {mail}. Pour signaler une lambda qui cause du tort, écrivez à {abuse}.
      </>
    ),
    editorialTitle: 'Responsable du contenu',
    editorial:
      'Responsable des pages de ce site au sens du § 18, al. 2 de la loi allemande MStV, mais pas des lambdas hébergées ici, que leurs propriétaires écrivent eux-mêmes :',
    dsaTitle: 'Point de contact au titre du règlement sur les services numériques',
    dsa: (mail) => (
      <>
        Les autorités, la Commission européenne et toute personne qui utilise ce service peuvent nous écrire à {mail}, en allemand ou en anglais (art. 11 et 12 du DSA).
      </>
    ),
  },
};
