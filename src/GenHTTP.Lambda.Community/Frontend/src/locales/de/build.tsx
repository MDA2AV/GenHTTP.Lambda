import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'App bauen, indem Sie sie beschreiben',
  intro:
    'Sagen Sie dem Build-Agenten dieser Installation, was Sie brauchen. Er schreibt die App, stellt sie hier online und führt sie in Ihrer Liste – um später auf dieselbe Weise nachzufragen und sie zu ändern.',
  other: (ship, create) => (
    <>
      Oder {ship('verbinden Sie Ihren eigenen Agenten')} oder {create('schreiben Sie den Code selbst')}.
    </>
  ),
};
