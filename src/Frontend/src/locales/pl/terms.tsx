import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Regulamin',
  binding: (english) => (
    <>To tłumaczenie ma charakter wyłącznie informacyjny. Wiążąca jest {english('wersja angielska')}.</>
  ),
  intro:
    'To darmowa usługa do testowania pomysłów. Uruchamia kod pisany przez nieznajomych na wspólnej infrastrukturze, a to działa tylko wtedy, gdy wszyscy trzymają się kilku zasad.',
  sections: {
    forbiddenTitle: 'Czego nie wolno tu umieszczać',
    forbidden: [
      'Żadnego złośliwego oprogramowania, phishingu ani koparek kryptowalut. Niczego, co atakuje, skanuje, zalewa ruchem lub w inny sposób zakłóca działanie innych systemów – tutaj ani nigdzie indziej. Niczego, co kogokolwiek nęka. Niczego, czego nie masz prawa publikować – dotyczy to także cudzego kodu, tekstów, obrazów i znaków towarowych.',
      'Nie używaj lambdy do przechowywania ani przekazywania danych osobowych innych ludzi. Publiczny adres nie ma w sobie nic prywatnego, a ta platforma nie daje ci żadnego sposobu, by takie dane chronić.',
    ],
    actionTitle: 'Co możemy z tym zrobić',
    action:
      'Wszystko, co tu wdrożono, możemy w dowolnej chwili wyłączyć lub usunąć, bez uprzedzenia i bez obowiązku wyjaśnień. W praktyce dzieje się tak, gdy coś łamie powyższe zasady, gdy zagraża serwerowi, z którego korzystają wszyscy inni, albo gdy ktoś to zgłosi i okaże się, że ma rację.',
    lastingTitle: 'Jak długo coś tu zostaje',
    lasting: (hours, days) =>
      `Wdrożenie jest dostępne przez około ${hours} godzin. Lambda, której nie otwierasz, zostaje usunięta razem ze wszystkimi wersjami kodu – około ${days} dni od twojej ostatniej aktywności przy niej. Zapisanie albo wdrożenie liczy się jako aktywność, więc to, nad czym pracujesz, zostaje. Nic tutaj nie jest kopią zapasową: trzymaj własną kopię kodu, na którym ci zależy.`,
    keyTitle: 'Link do edytora to twoje hasło',
    key: 'Każdy, kto ma link do edytora, może czytać i zmieniać tę lambdę – nie stoi za nim żadne konto ani hasło. Jeśli opublikujesz link, dasz każdemu możliwość wprowadzania zmian. Zgubionego linku nie da się odzyskać.',
    warrantyTitle: 'Brak gwarancji',
    warranty:
      'Usługa jest udostępniana w stanie, w jakim jest, bez gwarancji, że działa, będzie działać ani że zachowa cokolwiek, co w niej umieścisz. W każdej chwili może zostać zrestartowana, zmieniona albo wyłączona. Nie buduj na niej niczego, co jest ważne dla ciebie lub dla kogokolwiek innego.',
    reportTitle: 'Zgłaszanie problemów',
    report: (mailbox, front) => (
      <>
        Jeśli hostowana tu lambda robi coś, czego nie powinna, napisz na {mailbox} i podaj jej adres. Co jeszcze warto
        dołączyć, przeczytasz na {front('stronie głównej')}.
      </>
    ),
  },
  change: 'Ten regulamin może się zmienić. Obowiązuje wersja opublikowana na tej stronie.',

  short:
    'Lambdy działają na wspólnej infrastrukturze. Tworząc lambdę, zgadzasz się nie wdrażać złośliwego oprogramowania, stron phishingowych, koparek kryptowalut ani niczego, co atakuje, skanuje lub zalewa ruchem inne systemy, i nie publikować treści, do których nie masz praw. Każdy, kto zna link do edytora, może zmienić twoją lambdę, więc traktuj go jak hasło. Lambdy w darmowym planie działają, dopóki ktoś z nich korzysta: lambda, której nikt nie odwiedza ani nie edytuje przez miesiąc, zostaje wyłączona, a jeśli przez kolejne dwa miesiące nic się nie wydarzy – usunięta. Wszystko, co wdrożysz, może zostać usunięte w dowolnej chwili.',
};
