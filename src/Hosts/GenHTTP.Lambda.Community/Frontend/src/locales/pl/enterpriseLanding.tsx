import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Aplikacje do pracy',
  headline: 'Opisz aplikację, której potrzebujesz w pracy.',
  headlineAccent: 'Agent AI zbuduje ją i udostępni tutaj.',
  intro:
    'Formularz, kalendarz rezerwacji, lista dla zespołu: powiedz własnymi słowami, co ma robić. Po kilku minutach masz link, który możesz wysłać współpracownikom - a później zmieniasz aplikację w ten sam sposób, bez pisania kodu.',
  describe: 'Opisz aplikację',
  own: 'Użyj własnego agenta AI',

  examplesTitle: 'Co tu powstaje',
  examples: [
    { title: 'Formularze i wnioski', text: 'Urlopy, zakupy, opinie: ludzie wypełniają, a osoba decydująca zatwierdza.' },
    { title: 'Rezerwacje', text: 'Sale, biurka, sprzęt: kto co ma i kiedy.' },
    { title: 'Listy i rejestry', text: 'Zgłoszenia, stany magazynowe, wdrażanie nowych osób: co jest otwarte i kto się tym zajmuje.' },
    { title: 'Strony dla zespołu', text: 'Grafik, poradnik, wiadomości: jeden link, który otwierają wszyscy.' },
  ],

  stepsTitle: 'Jak to działa',
  steps: [
    {
      title: 'Opisz ją',
      text: 'Powiedz, co ma robić aplikacja, tak jakbyś tłumaczył to współpracownikowi. Pomysły pod polem to dobry punkt wyjścia.',
    },
    {
      title: 'Udostępnij link',
      text: 'Po kilku minutach aplikacja jest tutaj online. Wyślij jej link osobom, które będą z niej korzystać, i zachowaj swój link do edytora: dzięki niemu zmieniasz aplikację.',
    },
    {
      title: 'Poproś o więcej',
      text: 'W tym samym polu: coś nowego, coś innego albo pytanie, jak jej idzie - kto jej używał i co zostało wpisane. Wcześniejsze wersje są zachowane, więc poprzednią można znów udostępnić.',
    },
  ],

  questionsTitle: 'Warto wiedzieć',
  questions: (showcase) => [
    ['Czy muszę umieć programować?', 'Nie. Opisujesz, co ma robić aplikacja, a agent ją napisze. Nigdy nie musisz zaglądać do kodu.'],
    ['Kto może otworzyć moją aplikację?', 'Każdy, kto ma jej link i ma dostęp do tej strony. Wyślij go osobom, które mają z niej korzystać.'],
    ['Gdzie są przechowywane dane wpisane przez ludzi?', 'Każda aplikacja trzyma swoje wpisy w tej instalacji, którą prowadzi twoja organizacja.'],
    [
      'Jak wrócić do mojej aplikacji?',
      'Ta przeglądarka pamięta aplikacje zbudowane tutaj, obok pola, w którym je zbudowałeś. Na innym urządzeniu otwórz link do edytora, który dostałeś razem z aplikacją - zachowaj go w bezpiecznym miejscu, bo nikt nie może go odzyskać.',
    ],
    [
      'Coś nie działa. Co teraz?',
      'Powiedz agentowi, co jest nie tak, tak samo jak przy prośbie o aplikację. Jeśli zmiana pogorszyła sprawę, udostępnij ponownie poprzednią wersję w edytorze aplikacji.',
    ],
    ['Co zbudowali współpracownicy?', <>Aplikacje, które chcieli pokazać, są w {showcase('Galerii')}.</>],
  ],
};
