import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Cria uma app a descrevê-la',
  intro:
    'Diz ao agente de criação desta instalação o que precisas. Ele escreve a app, põe-na online aqui e guarda-a na tua lista - para perguntares e alterares mais tarde, da mesma maneira.',
  other: (ship, create) => (
    <>
      Ou {ship('liga o teu próprio agente')}, ou {create('escreve tu o código')}.
    </>
  ),
  profiles: {
    enterprise: {
      intro:
        'Diz ao agente por palavras tuas o que precisas. Ele cria a app, põe-na online aqui e guarda-a na tua lista, para depois perguntares e alterares da mesma forma.',
    },
  },
};
