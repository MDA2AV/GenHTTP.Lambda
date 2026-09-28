import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Aviso legal',
  intro:
    'Quién gestiona este sitio. La ley alemana exige que todo sitio gestionado desde Alemania dé estos datos en un solo lugar fácil de encontrar (§ 5 DDG), y es este.',
  sections: {
    providerTitle: 'Prestador del servicio',
    contactTitle: 'Contacto',
    contact: (mail, abuse) => (
      <>
        Correo: {mail}. Para denunciar una lambda que esté causando daño, escribe a {abuse}.
      </>
    ),
    editorialTitle: 'Responsable del contenido',
    editorial:
      'Responsable de las páginas de este sitio según el § 18, apdo. 2 de la ley alemana MStV, pero no de las lambdas alojadas aquí, que escriben sus propios dueños:',
    dsaTitle: 'Punto de contacto según el Reglamento de Servicios Digitales',
    dsa: (mail) => (
      <>
        Las autoridades, la Comisión Europea y cualquier persona que use este servicio pueden escribirnos a {mail}, en alemán o en inglés (arts. 11 y 12 DSA).
      </>
    ),
  },
};
