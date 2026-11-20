import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Chcę stronę internetową, która…',
  password: 'hasło',
  unlimited:
    'Ten model wymaga hasła, a wtedy działa bez limitu czasu: pracuje, aż twoja strona będzie gotowa, a nie tylko dopóki nie upłynie czas.',
  ideas: [
    'stronę dla naszego klubu, na której członkowie zapisują się na wydarzenia',
    'listę zapisów na naszą imprezę składkową, żeby nikt nie przyniósł tego samego dania',
    'księgę gości na nasze wesele',
    'ankietę, w której ludzie głosują i widzą wyniki',
    'ranking na nasz cotygodniowy quiz',
    'stronę urodzinową, na której znajomi zostawiają życzenia',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Przed twoją jest jeszcze jedna strona – potem twoja kolej.' : `Stron przed twoją: ${waiting}.`,
  starting: 'Zaczynamy…',
  online: 'Twoja strona jest online',
  notOnline: 'Twoja strona powstała, ale nie trafiła do sieci.',
  open: 'Otwórz swoją stronę',
  steps: {
    guide: 'Przygotowania',
    examples: 'Przeglądanie przykładów',
    create: 'Wybieranie adresu twojej strony',
    write: 'Pisanie twojej strony',
    improve: 'Ulepszanie twojej strony',
    check: 'Szukanie błędów',
    online: 'Publikowanie online',
    trying: 'Wypróbowywanie',
    looking: 'Przeglądanie twojej strony',
    forRecords: 'Przygotowywanie miejsca na wpisy',
    forKeys: 'Przygotowywanie miejsca na klucze i hasła',
    forFiles: 'Przygotowywanie miejsca na to, co strona zapisze',
    records: 'Przeglądanie wpisów',
    keys: 'Sprawdzanie, jakich kluczy i haseł potrzebuje',
    addFile: 'Dodawanie pliku',
    removeFile: 'Usuwanie pliku',
    files: 'Przeglądanie tego, co zostało zapisane',
    services: 'Prośba o korzystanie z innych usług',
  },
  further: 'Twój link do edytora',
  keep:
    'Zachowaj ten link w bezpiecznym miejscu. To jedyna droga do twojej strony z innego urządzenia i nikt go nie odzyska – my też nie.',
  copyLink: 'Kopiuj link do edytora',
  lifetime: (offline, removed) =>
    `Utrzymujemy ją online, dopóki jest używana: po ${offline} dniach bez odwiedzin i zmian zostaje wyłączona, a po ${removed} dniach usunięta. Możesz ją znów udostępnić tutaj albo w jej edytorze.`,

  failedToStart: 'Nie udało się wysłać prośby.',
  noAnswer: 'Agent skończył, ale nie napisał, co się stało.',
  failed: 'Nie udało się.',
};
