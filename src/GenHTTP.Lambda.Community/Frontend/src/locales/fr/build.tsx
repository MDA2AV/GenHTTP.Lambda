import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Créez une app en la décrivant',
  intro:
    'Dites à l’agent de création de cette installation ce dont vous avez besoin. Il écrit l’app, la met en ligne ici et la garde dans votre liste, pour que vous puissiez l’interroger et la modifier plus tard de la même façon.',
  other: (ship, create) => (
    <>
      Ou {ship('connectez votre propre agent')}, ou {create('écrivez le code vous-même')}.
    </>
  ),
};
