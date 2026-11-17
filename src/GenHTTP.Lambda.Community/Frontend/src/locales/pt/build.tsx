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
};
