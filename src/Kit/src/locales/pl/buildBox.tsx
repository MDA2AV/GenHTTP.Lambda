import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Chcę aplikację, która…',
  password: 'hasło',
  unlimited:
    'Ten wybór wymaga hasła tej strony, a wtedy dostaje czas i kroki ustawione dla niego, a nie zwykłe.',
  ideas: [
    'aplikację dla naszego klubu, w której członkowie zapisują się na wydarzenia',
    'listę zapisów na naszą imprezę składkową, żeby nikt nie przyniósł tego samego dania',
    'księgę gości na nasze wesele',
    'ankietę, w której ludzie głosują i widzą wyniki',
    'ranking na nasz cotygodniowy quiz',
    'stronę urodzinową, na której znajomi zostawiają życzenia',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Przed twoją jest jeszcze jedna aplikacja – potem twoja kolej.' : `Aplikacji przed twoją: ${waiting}.`,
  starting: 'Zaczynamy…',
  online: 'Twoja aplikacja jest online',
  notOnline: 'Twoja aplikacja powstała, ale nie trafiła do sieci.',
  open: 'Otwórz swoją aplikację',
  steps: {
    guide: 'Przygotowania',
    examples: 'Przeglądanie przykładów',
    create: 'Wybieranie adresu twojej aplikacji',
    write: 'Pisanie twojej aplikacji',
    improve: 'Ulepszanie twojej aplikacji',
    check: 'Szukanie błędów',
    online: 'Publikowanie online',
    trying: 'Wypróbowywanie',
    looking: 'Przeglądanie twojej aplikacji',
    forRecords: 'Przygotowywanie miejsca na wpisy',
    forKeys: 'Przygotowywanie miejsca na klucze i hasła',
    forFiles: 'Przygotowywanie miejsca na to, co aplikacja zapisze',
    records: 'Przeglądanie wpisów',
    keys: 'Sprawdzanie, jakich kluczy i haseł potrzebuje',
    addFile: 'Dodawanie pliku',
    removeFile: 'Usuwanie pliku',
    files: 'Przeglądanie tego, co zostało zapisane',
    services: 'Prośba o korzystanie z innych usług',
    site: (site) => `Przeglądanie strony ${site}`,
    search: 'Szukanie czegoś w internecie',
    attachment: 'Przeglądanie przesłanego pliku',
    showcase: 'Dodawanie do galerii',
    file: 'Tworzenie pliku dla ciebie',
  },
  further: 'Twój link do edytora',
  keep:
    'Zachowaj ten link w bezpiecznym miejscu. To jedyna droga do twojej aplikacji z innego urządzenia i nikt go nie odzyska – my też nie.',
  copyLink: 'Kopiuj link do edytora',
  lifetime: (offline, removed) =>
    `Utrzymujemy ją online, dopóki jest używana: po ${offline} dniach bez odwiedzin i zmian zostaje wyłączona, a po ${removed} dniach usunięta. Możesz ją znów udostępnić tutaj albo w jej edytorze.`,

  failedToStart: 'Nie udało się wysłać prośby.',
  noAnswer: 'Agent skończył, ale nie napisał, co się stało.',
  failed: 'Nie udało się.',
  profiles: {
    enterprise: {
      placeholder: 'Potrzebujemy aplikacji, która…',
      ideas: [
        'formularz wniosku o urlop, zatwierdzany przez przełożonego',
        'kalendarz rezerwacji naszych sal konferencyjnych',
        'listę kontrolną wdrażania nowych współpracowników',
        'spis naszego sprzętu i tego, kto go ma',
        'listę otwartych zgłoszeń do pomocy technicznej i osób, które się nimi zajmują',
        'formularz opinii o naszych szkoleniach',
      ],
    },
  },
};
