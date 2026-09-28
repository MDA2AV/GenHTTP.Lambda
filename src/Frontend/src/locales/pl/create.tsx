import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Utwórz lambdę',
  whatTitle: 'Co chcesz zbudować?',
  whatText:
    'Wybierz to, co jest najbliżej twojego pomysłu, a zaczniesz od kopii czegoś, co już działa – i możesz to dowolnie zmieniać. Albo zacznij od zera.',
  seeIt: 'Zobacz, jak działa',
  startFrom: 'Zacznij od tego',
  starters: {
    'demo-crud': {
      title: 'Wszystko pod kontrolą',
      description: 'Lista, do której ludzie mogą dodawać rzeczy, zmieniać je i odhaczać – zadania, notatki, zakładki albo mały magazyn.',
    },
    'demo-registration': {
      title: 'Rejestracja i logowanie',
      description: 'Ludzie zakładają konta i logują się, a niektóre strony widzą tylko oni.',
    },
    'demo-game': {
      title: 'Wspólna gra',
      description: 'Coś, w co kilka osób gra jednocześnie, na żywo w przeglądarkach.',
    },
    'demo-files': {
      title: 'Wspólne pliki i zdjęcia',
      description: 'Ludzie wrzucają zdjęcia albo dokumenty, a wszyscy inni mogą je zobaczyć.',
    },
    'demo-live': {
      title: 'Wszystko na żywo',
      description: 'Strona, która sama się odświeża, gdy tylko coś się zmieni – głosy, wyniki, dashboard.',
    },
    empty: {
      title: 'Coś innego',
      description: 'Zacznij od pustej lambdy i zbuduj, co tylko chcesz.',
    },
  },

  addressTitle: 'Wybierz adres',
  fromDemo: (title) => <>{title} – twoja lambda zaczyna jako kopia dema i możesz w niej zmienić wszystko.</>,
  fromNothing: 'Zaczynasz od pustej lambdy, gotowej na każdy pomysł.',
  pickAgain: 'Wybierz coś innego',
  publicKey: 'Klucz publiczny',
  free: (key) => `„${key}” jest wolny.`,
  keyHint: 'Małe litery, cyfry i myślniki. Co najmniej trzy znaki. Zostaw puste, a dostaniesz losowy.',
  accept: 'Akceptuję regulamin',
  fullTerms: 'Przeczytaj cały regulamin',
  back: 'Wstecz',
  creating: 'Tworzenie…',
  submit: 'Utwórz lambdę',
  keepLink: 'Na następnym ekranie zobaczysz link do edytora. To jedyna droga powrotu, więc go zachowaj.',
  failed: 'Nie udało się utworzyć lambdy.',
};
