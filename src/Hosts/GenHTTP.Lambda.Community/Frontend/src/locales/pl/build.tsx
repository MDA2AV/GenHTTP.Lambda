import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Zbuduj aplikację, opisując ją',
  intro:
    'Powiedz agentowi budowania tej instalacji, czego potrzebujesz. Napisze aplikację, udostępni ją tutaj i zachowa ją na twojej liście – żebyś mógł o nią pytać i ją zmieniać w ten sam sposób.',
  other: (ship, create) => (
    <>
      Albo {ship('podłącz własnego agenta')}, albo {create('napisz kod samodzielnie')}.
    </>
  ),
  profiles: {
    enterprise: {
      intro:
        'Opisz agentowi własnymi słowami, czego potrzebujesz. Zbuduje aplikację, udostępni ją tutaj i doda do twojej listy - żebyś mógł o nią pytać i zmieniać ją później w ten sam sposób.',
    },
  },
};
