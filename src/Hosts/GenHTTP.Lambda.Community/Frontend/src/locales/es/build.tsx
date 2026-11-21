import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Cree una app describiéndola',
  intro:
    'Cuente al agente de creación de esta instalación lo que necesita. Escribe la app, la pone en línea aquí y la guarda en su lista, para consultarla y cambiarla más adelante del mismo modo.',
  other: (ship, create) => (
    <>
      O {ship('conecte su propio agente')}, o {create('escriba el código usted mismo')}.
    </>
  ),
  profiles: {
    enterprise: {
      intro:
        'Cuente al agente con sus propias palabras lo que necesita. Él crea la app, la pone en línea aquí y la guarda en su lista, para que pueda consultarla y cambiarla después de la misma manera.',
    },
  },
};
