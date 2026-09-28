import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Z twojego laptopa na każdy ekran.',
  intro:
    'Masz aplikację zrobioną z agentem AI, ale działa tylko na twoim komputerze? Poproś agenta, żeby opublikował ją tutaj. Po kilku minutach aplikacja ma publiczny link, który każdy może otworzyć. Do tego zapamiętuje dane, więc ludzie mogą w niej razem grać, czatować i dodawać wpisy.',
  facts: ['Za darmo', 'Bez konta', 'Bez instalacji'],
  connect: 'Podłącz agenta',
  seeOthers: 'Zobacz, co opublikowali inni',

  stepsTitle: 'Trzy kroki, a jeden to tylko zdanie',
  step: (n) => `Krok ${n}`,
  steps: [
    {
      title: 'Podłącz raz',
      body: 'Dodaj jeden adres do swojego agenta (Claude, Cursor albo inny). Zajmie ci to niecałą minutę, a robisz to tylko raz.',
    },
    {
      title: 'Poproś o publikację',
      body: 'Powiedz agentowi, żeby wrzucił aplikację tutaj. Spakuje ją, opublikuje i sprawdzi, czy działa.',
    },
    {
      title: 'Udostępnij link',
      body: 'Dostajesz publiczny adres i prywatny link do edytora. Pierwszy wyślij, komu chcesz. Drugi zachowaj dla siebie – dzięki niemu zmienisz później aplikację.',
    },
  ],

  togetherTitle: 'To nie tylko strona. To miejsce spotkań.',
  together:
    'Większość hostingów daje każdemu odwiedzającemu osobną kopię aplikacji i każdy gra sam. Tutaj każda aplikacja ma własną pamięć i połączenie na żywo ze wszystkimi, którzy mają ją otwartą. Ruch jednej osoby od razu widzą wszyscy pozostali, a to, co ktoś napisze, jutro nadal tam będzie.',
  together2:
    'Żadnej bazy danych do zakładania, żadnej dodatkowej usługi do podpinania. Poproś o to własnymi słowami, jak w rozmowie ze znajomym.',
  kinds: [
    { name: 'Gry wieloosobowe', ask: 'Niech w jednej rundzie gra do ośmiu znajomych i każdy widzi ruchy innych na żywo.' },
    { name: 'Czaty', ask: 'Dodaj pokój, w którym może pisać każdy, kto ma link. Niech zapamiętuje ostatnie sto wiadomości.' },
    { name: 'Wspólne listy', ask: 'Zrób z listy rzeczy do spakowania taką, którą cały zespół może edytować naraz.' },
    { name: 'Wyniki i rekordy', ask: 'Dodaj ranking z najlepszym czasem każdego gracza i pokaż pierwszą dziesiątkę na ekranie startowym.' },
    { name: 'Małe serwisy społecznościowe', ask: 'Niech goście weselni wrzucają zdjęcia na jedną tablicę i lajkują nawzajem swoje.' },
  ],
  quote: (text) => `„${text}”`,

  connectTitle: 'Podłącz agenta tylko raz',
  connectText: 'Podaj agentowi ten adres. Od tej chwili wie, jak tu publikować – bez klucza i bez logowania.',
  sayLike: 'Potem w swoim projekcie napisz na przykład',
  asks: [
    'Opublikuj tę aplikację na GenHTTP Lambda i wyślij mi link.',
    'Niech najlepsze wyniki będą wspólne, żeby wszyscy widzieli ten sam ranking.',
  ],

  domainChip: 'Kiedy się przyjmie',
  domainTitle: 'Nadaj jej własną nazwę',
  domainText:
    'Ta sama aplikacja, ten sam link do edytora, ale pod adresem, który należy do ciebie. Łatwiej go powiedzieć, łatwiej zapamiętać i wygląda poważniej, kiedy ludzie zaczną go podawać dalej.',
  domainSubject: 'Domena dla mojej aplikacji',
  domainAsk: 'Zapytaj o domenę',

  questionsTitle: 'Zanim zapytasz',
  questions: (offline, removed, showcase, terms) => [
    [
      'Czy to naprawdę za darmo?',
      <>
        Tak. Bez karty, bez okresu próbnego i bez konta. Aplikacja działa, dopóki ktoś z niej korzysta. Po {offline}{' '}
        dniach bez żadnych odwiedzin i zmian zostaje wyłączona, a po {removed} dniach usunięta.
      </>,
    ],
    [
      'Czy moja aplikacja musi być zbudowana w określony sposób?',
      'Nie, to zadanie agenta. Strony, obrazki i style trafiają tu bez zmian, a to, co musi działać na serwerze, agent dopasuje do tej platformy. Ty mówisz, co aplikacja ma robić – resztę załatwia agent.',
    ],
    [
      'Jak ją później zmienić?',
      'Linkiem do edytora, który dostajesz przy publikacji. Daj go agentowi razem z kolejną zmianą albo otwórz go w przeglądarce. Każda zmiana to nowa wersja pod tym samym adresem, a do starszej możesz wrócić w każdej chwili.',
    ],
    [
      'Kto zobaczy moją aplikację?',
      <>Każdy, komu dasz link. Nie ma jej na żadnej liście, chyba że dodasz ją do {showcase('galerii')}.</>,
    ],
    [
      'Czy jest coś, czego nie mogę tu opublikować?',
      <>
        Kilka rzeczy, na przykład wszystko, co szkodzi ludziom albo ich oszukuje. {terms('Regulamin')} jest krótki i
        napisany prostym językiem.
      </>,
    ],
  ],

  closeTitle: 'U ciebie działa.',
  closeAccent: 'Niech działa też u nich.',
  noAgent: 'Nie masz agenta? Zbuduj tutaj',
  closeFacts: 'Za darmo. Bez konta. Bez instalacji.',

  scene: {
    label:
      'Agent dostaje prośbę o publikację aplikacji. Adres zmienia się z localhost na publiczny link i dołączają kolejne osoby.',
    ask: 'Wrzuć mój quiz do sieci, żeby znajomi mogli dołączyć.',
    live: 'Gotowe, działa. Oto twój link.',
    publishing: 'Publikuję…',
    public: 'Publiczny',
    onlyYou: 'Tylko ty',
    app: 'Piątkowy quiz',
    playing: (count) => <>{count} w grze</>,
    you: 'Ty',
  },

  compareTitle: 'Najkrótsza droga od „działa” do „wypróbuj”',
  compareText:
    'Vercel, Cloudflare i Lovable to świetne miejsca do uruchamiania aplikacji. Ale wszędzie zaczynasz od formularza rejestracji, a gdy aplikacja ma dzielić dane między odwiedzających – także od konfiguracji kolejnej usługi. Tak to wygląda, gdy zaczynasz od zera.',
  rows: [
    'Start bez konta',
    'Publikacja z agenta, którego już używasz',
    'Wspólne dane na żywo: czat, gry wieloosobowe, rekordy',
    'Koszt pierwszego linku',
  ],
  us: ['Tak', 'Podłącz raz, potem tylko proś', 'Wbudowane w każdą aplikację', 'Za darmo'],
  rivals: [
    ['Wymaga rejestracji', 'Po zalogowaniu w ich narzędziach', 'Trzeba dodać bazę danych', 'Darmowy plan'],
    ['Wymaga rejestracji', 'Po zalogowaniu w ich narzędziach', 'Możliwe, po konfiguracji', 'Darmowy plan'],
    ['Wymaga rejestracji', 'W ich własnym edytorze', 'Przez podłączony backend', 'Darmowy plan, limit kredytów'],
  ],
  compareNote:
    'Stan na wrzesień 2026, dla kogoś bez konta w żadnym z tych serwisów. Plany i funkcje innych usług się zmieniają – szczegóły sprawdź u nich.',

};
