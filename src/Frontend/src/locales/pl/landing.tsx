import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Kreator aplikacji AI z hostingiem',
  headline: 'Opisz aplikację.',
  headlineAccent: 'Twój agent wrzuci ją do sieci.',
  intro:
    'Ankiety, księgi gości, rankingi, gry wieloosobowe. Powiedz naszemu agentowi AI – albo temu, którego już używasz – czego potrzebujesz. Dostaniesz działającą aplikację, hostowaną przez nas, z linkiem do udostępnienia. Możesz ją dopracowywać jeszcze długo po pierwszej wersji.',
  build: 'Zbuduj coś',
  ownAgent: 'Użyj własnego agenta',
  free: 'Za darmo. Bez rejestracji, bez karty kredytowej, bez instalacji.',
  seeIt: 'Zobacz, jak to działa',

  videoTitle: 'Od jednego zdania do działającej aplikacji',
  videoText:
    'Prywatne okno przeglądarki, żadnego konta i jedna prośba na stronie „Zbuduj”. Potem gotowa aplikacja, otwarta z linku – tak, jak zobaczy ją każdy odwiedzający.',
  videoNote: 'Budowanie pokazujemy w przyspieszeniu. Reszta dzieje się w czasie rzeczywistym.',
  tryIt: 'Wypróbuj teraz',

  oneShotTitle: 'Więcej niż generator aplikacji',
  oneShotText:
    'Większość generatorów daje ci wynik i na tym koniec. Tutaj aplikacja działa tam, gdzie powstała, więc ty i twój agent możecie dalej nad nią pracować.',
  steps: [
    {
      title: 'Powiedz, czego chcesz',
      body: 'Opisz to zwykłymi słowami – agentowi na tej stronie albo temu, którego już używasz. Bez kodu, bez konfiguracji i bez konta.',
      alt: 'Strona „Zbuduj” z wpisaną prośbą o ankietę na lunch',
    },
    {
      title: 'Odbierz działającą aplikację i link',
      body: 'Agent buduje aplikację, a my ją hostujemy. Ty dostajesz jej publiczny adres do udostępnienia i nie konfigurujesz serwera, hostingu, domeny ani bazy danych: tym zajmujemy się my. Aplikacja zapamiętuje dane – głosy, wyniki, wiadomości – więc każdy, kto ją otworzy, widzi to samo.',
      alt: 'Gotowa ankieta na lunch otwarta w przeglądarce',
    },
    {
      title: 'Ulepszaj ją dalej',
      body: 'Każda aplikacja ma prywatny link do edytora. Daj go agentowi razem z kolejną zmianą albo otwórz go samodzielnie. Każda zmiana to nowa wersja, a adres zostaje ten sam.',
      alt: 'Centrum sterowania ankiety: jej wersje, a przy każdej prośba, opis zmiany i różnice względem poprzedniej',
    },
  ],
  weekLater: 'Tydzień później',
  weekAsk:
    'Tu masz link do edytora mojej ankiety na lunch. Niech głosowanie kończy się w piątki o 11, a zwycięzca będzie na samej górze.',
  weekAnswer:
    'Gotowe. Wersja 4 działa pod tym samym adresem. Wersja 3 wciąż jest dostępna, więc w razie czego możesz do niej wrócić.',

  agentsTitle: 'Podłącz własnego agenta: Claude, Codex, Cursor',
  agentsText:
    'Robisz już vibe coding z asystentem AI, takim jak Claude Code, Codex czy Cursor? Podłącz go pod ten adres – to zdalny serwer MCP, klucz nie jest potrzebny – a będzie tu budować, wdrażać i aktualizować aplikacje, prosto z rozmowy, którą właśnie prowadzisz.',
  thenAsk: (em) => (
    <>Potem po prostu poproś: {em('zrób listę zapisów na naszą integrację i wrzuć ją do sieci')}.</>
  ),
  hostIt: (link) => (
    <>Masz coś, co działa tylko na localhost? {link('Hostuj tu swoją aplikację z vibe codingu')}.</>
  ),

  contactTitle: 'Porozmawiajmy',
  contactText:
    'Potrzebujesz pomocy, planujesz coś większego albo szukasz rozwiązania szytego na miarę? Odezwij się.',
  mailTitle: 'Napisz do nas',
  mailText: 'W sprawie projektów, pytań i wszystkiego, o czym wolisz porozmawiać prywatnie.',
  discordTitle: 'Dołącz do Discorda',
  discordText: 'Pochwal się tym, co zbudujesz, zapytaj o kolejny krok i pogadaj bezpośrednio z zespołem.',
  discordLink: 'Discord GenHTTP',
};
