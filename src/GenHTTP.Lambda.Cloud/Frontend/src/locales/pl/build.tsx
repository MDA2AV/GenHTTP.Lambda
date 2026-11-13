import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Stwórz stronę internetową z AI.',
  intro:
    'Opisz własnymi słowami stronę internetową lub aplikację, o której myślisz. AI zbuduje ją dla ciebie, my ją hostujemy, a po kilku minutach jest online – z linkiem, który możesz wysłać każdemu. Za darmo, bez programowania, bez rejestracji.',
  points: [
    {
      title: 'Bez programowania',
      text: 'Powiedz własnymi słowami, co ma robić twoja strona, jak w rozmowie ze znajomym. AI zbuduje ją dla ciebie – wiedza techniczna nie jest potrzebna.',
    },
    {
      title: 'Darmowy hosting w zestawie',
      text: 'Twoja strona działa na naszych serwerach. Nie kupujesz hostingu, serwera ani domeny i niczego nie instalujesz. Bezpieczeństwem i aktualizacjami zajmujemy się my.',
    },
    {
      title: 'Online w kilka minut',
      text: 'Od razu dostajesz link do udostępnienia. Strona zapamiętuje to, co wpisują ludzie – zapisy, głosy, wiadomości, wyniki – więc wszyscy widzą to samo.',
    },
  ],

  questionsTitle: 'Zanim zaczniesz',
  questions: (offline, removed) => [
    [
      'Czy AI naprawdę stworzy mi stronę internetową za darmo?',
      `Tak. Opisz ją własnymi słowami, a AI ją zbuduje, opublikuje i da ci link. Bez rejestracji, bez karty kredytowej, bez okresu próbnego. Strona działa, dopóki ludzie z niej korzystają: po ${offline} dniach bez odwiedzin i zmian zostaje wyłączona, a po ${removed} dniach usunięta.`,
    ],
    [
      'Czy potrzebuję hostingu, serwera albo domeny?',
      'Nie. Twoja strona działa na naszych serwerach, a hosting, bezpieczeństwo i aktualizacje są w zestawie. Link dostajesz od razu, więc nie musisz też kupować domeny.',
    ],
    [
      'Czy mogę stworzyć aplikację bez programowania?',
      'Tak. Nigdy nie zobaczysz kodu. Powiedz, co ma robić, jak w rozmowie ze znajomym, a AI zrobi resztę – stronę, małą aplikację albo grę.',
    ],
    [
      'Czy ludzie mogą coś wpisywać – zapisy, głosy, wiadomości?',
      'Tak. Twoja strona zapamiętuje to, co wpisują ludzie, więc każdy, kto otworzy link, widzi te same wpisy, głosy i wyniki.',
    ],
    [
      'Jak inni otworzą moją stronę?',
      'Przez link, w dowolnej przeglądarce, na telefonie albo komputerze. Niczego nie trzeba instalować i nie ma po drodze żadnego sklepu z aplikacjami.',
    ],
    [
      'Jak zmienić stronę później?',
      'Otwórz link do edytora, który dostajesz razem ze stroną, i opisz, co ma być inaczej – tak samo jak tutaj. Jeśli zmiana ci się nie spodoba, możesz wrócić do tego, jak było wcześniej.',
    ],
  ],

};
