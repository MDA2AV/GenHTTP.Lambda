import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Podłącz swojego agenta',
  title: 'Publikuj z agenta, którego już używasz.',
  intro:
    'Claude Code, Codex, Cursor albo dowolny agent obsługujący MCP dostaje tę instalację jako narzędzia: buduje aplikację, wypróbowuje zmianę w szkicu, udostępnia ją i czyta jej logi. Każda aplikacja odpowiada pod własnym adresem i ma własną bazę danych.',
  stepsTitle: 'Trzy kroki do aplikacji online',
  step: (n) => `Krok ${n}`,
  steps: [
    {
      title: 'Podłącz raz',
      body: 'Dodaj do swojego agenta adres serwera MCP tej instalacji. Nie potrzeba żadnego klucza: kto ma klucz edytora aplikacji, może zmieniać tę aplikację i nic poza nią.',
    },
    {
      title: 'Poproś o zbudowanie lub publikację',
      body: 'Opisz nową aplikację albo poproś o opublikowanie tej z twojego projektu. Agent napisze kod, sprawdzi, czy się kompiluje, i udostępni aplikację.',
    },
    {
      title: 'Zachowaj link do edytora',
      body: 'Dostajesz adres aplikacji i link do jej edytora. Adres jest dla wszystkich; link do edytora to sposób, w jaki ty – albo twój agent – zmienicie aplikację później.',
    },
  ],
  sayLike: 'Potem powiedz coś w rodzaju',
  asks: ['Zbuduj księgę gości ze stroną, na której zatwierdzam wpisy.', 'Opublikuj tę aplikację tutaj i wyślij mi link.'],
  noAgent: 'Nie masz agenta? Użyj agenta budowania',

  questionsTitle: 'Warto wiedzieć',
  questions: (create) => [
    [
      'Jakie agenty mogą się połączyć?',
      'Każdy agent, który potrafi dodać zdalny serwer MCP przez HTTP: Claude Code, Claude, Codex, Cursor, VS Code i inne, pod adresem powyżej.',
    ],
    [
      'Jak zmienić aplikację później?',
      'Podaj agentowi link do edytora aplikacji przy kolejnej zmianie albo otwórz go sam: edytor pokazuje jej wersje, kod, wpisy i logi. Zmiana jest wypróbowywana w szkicu, z kopią danych, zanim zostanie udostępniona.',
    ],
    [
      'Czy mogę pracować nad aplikacją przez git?',
      'Tak. Każda aplikacja jest repozytorium git: sklonuj je z edytora, wypchnij commit do main, aby zapisać wersję, albo wypchnij gałąź, aby rozpocząć szkic z własnym podglądem.',
    ],
    [
      'Gdzie trafiają klucze API i hasła?',
      'Nie do kodu. Agent czyta je po nazwie, a wartości wpisujesz w edytorze, gdzie nikt nie odczyta ich z powrotem – agent też nie.',
    ],
    [
      'Czy mogę napisać aplikację bez agenta?',
      <>Tak: {create('napisz kod samodzielnie')}, zaczynając od jednego z dem. Aplikacja to handler C# GenHTTP, kompilowany na tym serwerze.</>,
    ],
  ],
};
