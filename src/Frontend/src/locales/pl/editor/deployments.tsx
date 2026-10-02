import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Wdrożenie działa, dopóki ktoś z niego korzysta${until ? ` – a jeśli nikt, to do ${until}` : ''}. Każde kolejne wdrożenie i każda wizyta zaczynają to odliczanie od nowa.`,
  takeOffline: 'Wyłącz',
  readFailed: 'Nie udało się odczytać historii.',
  reading: 'Odczytywanie historii…',
  none: 'Nic jeszcze nie wdrożono.',
  noDescription: 'Bez opisu',
  deployed: (when, by) => `Wdrożono ${when} (${by})`,
  duration: 'Jak długo było online',
  online: 'online',
  short: {
    replaced: 'zastąpione',
    stopped: 'wyłączone',
    expired: 'wygasłe',
    admin: 'przez administratora',
    ended: 'zakończone',
  },
  putBack: (version) => `Przywróć online wersję ${version}`,
  timeline: 'Co było online w ciągu ostatnich siedmiu dni',
  block: (version, from, to) => `Wersja ${version}, od ${from} do ${to ?? 'teraz'}`,
  weekAgo: 'tydzień temu',
  now: 'teraz',
};
