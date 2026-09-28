import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Enterprise',
  title: 'Wypróbuj za darmo, uruchom u siebie',
  intro:
    'Tutaj wszystko jest za darmo i bez konta. Gdy twój zespół potrzebuje aplikacji, które działają na stałe, z logowaniem przez wasze SSO, weź własną instalację – w chmurze albo na własnych serwerach.',

  free: 'Darmowy',
  freeTagline: 'Do testowania pomysłów',
  forever: 'na zawsze',
  buildOne: 'Zbuduj aplikację',
  freeFeatures: (offline, removed) => [
    'Nieograniczona liczba lambd, bez konta',
    'Wbudowany agent albo twój własny przez MCP',
    'Lambda działa, dopóki ktoś z niej korzysta',
    `Wyłączenie po ${offline} dniach bez odwiedzin, usunięcie po ${removed} dniach`,
    'Dostępna pod ścieżką na wspólnym hoście',
  ],
  freeNote: 'Bez karty i bez rejestracji. Utwórz lambdę i już jest twoja.',

  name: 'Enterprise',
  tagline: 'Dla zespołów, które chcą własnej instancji',
  perUser: 'za użytkownika / mies.',
  contact: 'Napisz do nas',
  features: [
    'Własna instancja: w chmurze albo na własnych serwerach',
    'Jedna usługa obsługuje wszystkie aplikacje',
    'Logowanie przez własne SSO',
    'Wbudowane zasady governance i compliance waszej firmy',
    'Aplikacje działają na stałe, nic nie jest usuwane',
    'Własny agent przez MCP',
    'Priorytetowe wsparcie',
  ],
  users: (count) => <>Użytkownicy: {count}</>,
  perMonth: ' / mies.',
  price: (amount) => `${amount}\u00A0$`,
  perUserPrice: (amount) => `${amount}\u00A0$ za użytkownika / mies.`,

  compareTitle: 'Porównaj plany',
  compareText: 'Oba działają na tej samej platformie. Różnica jest w tym, jak długo i gdzie działa twoja aplikacja.',
  included: 'W cenie',
  notIncluded: 'Brak',
  groups: (offline, removed) => [
    {
      title: 'Budowanie',
      rows: [
        ['Lambdy', 'Bez limitu', 'Bez limitu'],
        ['Wbudowany agent', true, false],
        ['Własny agent przez MCP', true, true],
        ['Edytor, wersje i logi', true, true],
        ['Galeria', true, 'Własna'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Wyłączenie nieużywanej aplikacji', `Po ${offline} dniach`, 'Nigdy'],
        ['Usunięcie nieużywanej aplikacji', `Po ${removed} dniach`, 'Nigdy'],
        ['Instancja', 'Wspólna', 'Własna'],
        ['Gdzie działa', 'W naszej chmurze', 'Chmura lub własne serwery'],
        ['Co utrzymujesz', 'Nic', 'Jedną usługę'],
        ['Własne domeny', false, true],
      ],
    },
    {
      title: 'Kontrola',
      rows: [
        ['Logowanie', 'Niepotrzebne', 'Własne SSO'],
        ['Wasze zasady governance i compliance dla agentów', false, true],
        ['Konsola administracyjna', false, true],
        ['Dane oddzielone od innych klientów', false, true],
        ['Wsparcie', 'Społeczność', 'Priorytetowe'],
      ],
    },
  ],

  questionsTitle: 'Pytania',
  questions: [
    [
      'Czy potrzebuję konta, żeby zacząć?',
      'Nie. Do darmowej lambdy wystarczy link do edytora, który dostajesz przy jej tworzeniu.',
    ],
    [
      'Kto w Enterprise liczy się jako użytkownik?',
      'Każdy, kto loguje się przez wasze SSO – czy to, żeby budować w edytorze, czy żeby korzystać z aplikacji wdrożonej w waszej instalacji. Kto otwiera aplikację bez logowania, nie jest liczony.',
    ],
    [
      'Czy wbudowany agent jest dostępny w Enterprise?',
      'Nie. Twój zespół podłącza do waszej instalacji własnego agenta (np. Claude, Claude Code albo inny z obsługą MCP) – w ramach planu, który już macie u jego dostawcy.',
    ],
    [
      'Skąd agenci znają nasze zasady compliance?',
      'Wbudowujemy wasze zasady governance i compliance w to, co platforma przekazuje agentom przez MCP. Dostaje je każdy agent, którego podłączy twój zespół, już podczas pisania kodu. Dzięki temu aplikacje od razu trzymają się waszych zasad i nikt nie musi znać ich na pamięć.',
    ],
    [
      'Czy potrzebujemy Kubernetesa albo klastra?',
      'Nie. Wszystkie aplikacje działają w jednej usłudze, więc nie ma podów do rozkładania ani orkiestracji dla każdej aplikacji z osobna. Utrzymanie instalacji to utrzymanie tej jednej usługi.',
    ],
    [
      'Gdzie działa instalacja Enterprise?',
      'Tam, gdzie zechcecie. Możemy ją hostować w naszej chmurze albo może działać na waszym koncie w chmurze lub na waszych serwerach – wszędzie tam, gdzie działają kontenery. Tak czy inaczej pomożemy ją skonfigurować i będziemy ją aktualizować.',
    ],
  ],
  anythingElse: (mail) => <>Masz inne pytanie? Napisz na {mail}.</>,
};
