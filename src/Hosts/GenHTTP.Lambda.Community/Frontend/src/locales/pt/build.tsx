import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Crie um app descrevendo-o',
  intro:
    'Diga ao agente de criação desta instalação o que você precisa. Ele escreve o app, o coloca no ar aqui e o mantém na sua lista - para você perguntar e mudar depois, do mesmo jeito.',
  other: (ship, create) => (
    <>
      Ou {ship('conecte o seu próprio agente')}, ou {create('escreva o código você mesmo')}.
    </>
  ),
  profiles: {
    enterprise: {
      intro:
        'Diga ao agente com as suas próprias palavras o que você precisa. Ele cria o app, coloca no ar aqui e o mantém na sua lista, para você perguntar sobre ele e mudá-lo depois, do mesmo jeito.',
    },
  },
};
