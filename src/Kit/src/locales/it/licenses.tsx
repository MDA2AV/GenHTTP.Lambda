import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Chiunque può usarlo, modificarlo e ridistribuirlo, in qualsiasi progetto, purché la licenza e l’avviso di copyright restino con il codice.',
  'Apache-2.0': 'Come MIT, con in più una licenza sui brevetti da parte di chiunque abbia contribuito, e le modifiche segnalate come tali.',
  'BSD-3-Clause': 'Come MIT, e nessuno può usare il nome degli autori per promuovere quello che ne ha ricavato.',
  'MPL-2.0': 'Le modifiche a questi file restano sotto la stessa licenza; i file si possono combinare con codice sotto qualsiasi altra.',
  'GPL-3.0-or-later': 'Chi lo ridistribuisce, modificato o no, deve ridistribuirne anche il codice sorgente con la stessa licenza.',
  'AGPL-3.0-or-later': 'Come la GPL, e offrire una copia modificata ad altri tramite la rete conta come ridistribuirla.',
  Unlicense: 'Donato al pubblico dominio: chiunque può farne quello che vuole, senza condizioni.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Permissiva',
  Copyleft: 'Copyleft',
  PublicDomain: 'Pubblico dominio',
};
