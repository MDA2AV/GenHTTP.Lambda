import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Bouw een app door hem te beschrijven',
  intro:
    'Vertel de bouwagent van deze installatie wat je nodig hebt. Hij schrijft de app, zet hem hier online en houdt hem in je lijst - om er later op dezelfde manier naar te vragen en hem aan te passen.',
  other: (ship, create) => (
    <>
      Of {ship('koppel je eigen agent')}, of {create('schrijf de code zelf')}.
    </>
  ),
  profiles: {
    enterprise: {
      intro:
        'Vertel de agent in je eigen woorden wat je nodig hebt. Die bouwt de app, zet hem hier online en bewaart hem in je lijst - om er later naar te vragen en hem op dezelfde manier aan te passen.',
    },
  },
};
