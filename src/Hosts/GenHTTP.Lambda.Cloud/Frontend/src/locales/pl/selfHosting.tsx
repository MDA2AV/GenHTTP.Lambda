import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Self-hosting',

  eyebrow: 'Open source · chmura lub self-hosting',
  title: 'Używaj jej tutaj',
  titleAccent: 'albo uruchom u siebie.',
  intro:
    'GenHTTP Lambda jest open source na licencji MIT. genhttp.dev uruchamia ją dla każdego chętnego, a edycja Community to ta sama platforma jako obraz Dockera, dla tych, którzy wolą uruchomić ją sami. Obie budują i hostują te same aplikacje; różni je to, gdzie działają i kto się nimi opiekuje.',

  cloud: {
    kicker: 'Chmura',
    name: 'genhttp.dev',
    summary: 'Otwórz tę stronę i zacznij. Serwer, jego aktualizacje i limity są po naszej stronie.',
    points: ['Nic do instalowania ani uruchamiania', 'Aplikacje działają pod adresem na genhttp.run', 'Agent budujący w zestawie'],
    action: 'Zbuduj coś',
  },
  community: {
    kicker: 'Self-hosting',
    name: 'Edycja Community',
    summary: 'Jeden kontener Dockera na twojej maszynie. Serwer, jego aktualizacje i limity są po twojej stronie.',
    points: ['Jeden kontener, jeden wolumen, bez serwera bazy danych', 'Aplikacje działają pod twoim adresem', 'Agent budujący jako dodatkowy kontener'],
    action: 'Uruchom u siebie',
  },

  sameTitle: 'To samo w obu',
  sameText: 'Edycja Community nie jest okrojoną wersją. Każda aplikacja dostaje to samo, gdziekolwiek działa:',
  same: [
    'Agent budujący',
    'Twój własny agent przez MCP',
    'REST API',
    'Edytor, prosty i pełny',
    'Szkice z danymi testowymi',
    'Wersje i droga powrotna',
    'Wpisy, pliki, klucze i hasła',
    'Analityka bez ciasteczek',
    'Własne domeny',
    'Git i pobieranie',
    'Galeria i opublikowany kod',
  ],

  differTitle: 'Co się różni',
  aspect: 'Pytanie',
  rows: (offline) => [
    ['Gdzie działają aplikacje', 'Na naszym serwerze', 'Na twoim, w jednym kontenerze z jednym wolumenem'],
    ['Jak zacząć', 'Otwórz stronę budowania – bez konta', 'Uruchom obraz, potem otwórz jego adres'],
    ['Kto opiekuje się serwerem', 'My', 'Ty'],
    ['Ile to kosztuje', 'Nic na próbę; produkcja dla organizacji non-profit na zapytanie, dla firm w drodze umowy', 'Nic; serwer opłacasz sam'],
    ['Gdzie odpowiada aplikacja', 'Pod subdomeną genhttp.run', 'Pod twoim adresem albo pod subdomenami domeny, którą podasz'],
    ['Własna domena', 'Dla aplikacji na produkcji, rejestrowana przez ciebie', 'Dla każdej aplikacji, którą przeniesiesz na produkcję'],
    ['Aplikacje, z których nikt nie korzysta', `Wyłączone po ${offline} dniach bez odwiedzin i zmian, chyba że są na produkcji`, 'Jak ustawisz'],
    ['Limity i zasady', 'Nasze, oraz nasz regulamin', 'Twoje, ustawiane w panelu administratora'],
    ['Agent budujący', 'W zestawie, z limitem budów dziennie', 'Opcjonalny, z twoją własną subskrypcją Claude albo kluczem API'],
    ['Raporty użycia', 'Brak', 'Raz dziennie: wersja, maszyna, opcje i liczby dotyczące aplikacji – nigdy aplikacja ani osoba. Możesz je wyłączyć.'],
  ],

  moveTitle: 'Przenoszenie między nimi',
  move:
    'Te same aplikacje działają w obu, a twój agent przenosi je przez git: tworzy aplikację po drugiej stronie, kopiuje tam pliki starej i je wypycha. Pobranie to projekt .NET, który działa wszędzie, z wpisami w pliku SQLite.',

  pickTitle: 'Co ci odpowiada',
  pickCloud: {
    title: 'genhttp.dev, jeśli',
    points: [
      'chcesz mieć aplikację online bez uruchamiania czegokolwiek',
      'korzystający z niej ludzie są gdziekolwiek',
      'wolisz nie opiekować się serwerem',
    ],
  },
  pickCommunity: {
    title: 'Edycja Community, jeśli',
    points: [
      'twoje aplikacje i ich wpisy powinny zostać na twojej maszynie',
      'to narzędzia dla twojego zespołu, w twojej własnej sieci',
      'chcesz sam ustawiać limity i zasady',
    ],
  },

  startTitle: 'Jak zacząć',
  startCloud: {
    title: 'Na genhttp.dev',
    text: 'Opisz, czego potrzebujesz, na stronie budowania albo podłącz agenta, którego już używasz. Nie trzeba zakładać konta.',
  },
  startCommunity: {
    title: 'Na twoim serwerze',
    text: 'Jedno polecenie to uruchamia. Otwórz jego adres, podłącz agenta i buduj – resztę mówi README.',
  },
  connect: 'Podłącz swojego agenta',
  readme: 'Przeczytaj README',
  dockerHub: 'Obraz na Docker Hub',

  sourceTitle: 'Open source',
  source: (github) => (
    <>
      GenHTTP Lambda jest wydawana na licencji MIT. Jej kod, zgłoszenia i wydania są na {github('GitHubie')}.
    </>
  ),
};
