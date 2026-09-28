import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Polityka prywatności',
  binding: (english) => (
    <>To tłumaczenie ma charakter wyłącznie informacyjny. Wiążąca jest {english('wersja angielska')}.</>
  ),
  intro:
    'Czego ta strona dowiaduje się o tobie, co z tym robi, jak długo to przechowuje i kto jeszcze może to zobaczyć. W skrócie: nie ma kont, reklam ani śledzenia. Serwer zapisuje, kto i o co go prosił, żeby dało się utrzymać go w działaniu i namierzyć nadużycia. A to, o co prosisz agenta do budowania, trafia do firmy Anthropic, której model pisze aplikację.',
  sections: {
    whoTitle: 'Kto odpowiada za dane',
    who: 'Tę stronę prowadzi osoba wskazana poniżej. Jest ona administratorem danych osobowych przetwarzanych przez stronę w rozumieniu unijnego ogólnego rozporządzenia o ochronie danych (RODO). W każdej sprawie opisanej na tej stronie pisz na ten adres:',

    requestsTitle: 'Co serwer zapisuje przy każdym żądaniu',
    requests: [
      'Każde żądanie do tej strony i do każdej hostowanej tu lambdy trafia do logu serwera: adres IP, z którego przyszło, adres, w imieniu którego – jak podaje – zostało przekazane dalej, przeglądarka lub program, który je wysłał, adres, o który prosiło, czas i to, jaką dostało odpowiedź. Serwer sprawdza też, do jakiego kraju, miasta i sieci należy adres IP. Robi to we własnej bazie danych, bez pytania kogokolwiek innego.',
      'Dzięki temu znajdujemy błędy, ustalamy, co przeciąża serwer, i namierzamy zgłoszone nam nadużycia. Adres IP służy też – wyłącznie w pamięci – do ograniczania liczby żądań i zadań budowania, jakie może zlecić jeden odwiedzający. Bez tych danych nie da się odpowiedzieć na żądanie. Podstawą prawną jest nasz prawnie uzasadniony interes w prowadzeniu usługi i dbaniu o jej bezpieczeństwo (art. 6 ust. 1 lit. f RODO).',
      'Administratorzy serwisu mogą zobaczyć wszystko. Właściciel lambdy widzi przy każdym żądaniu do niej kraj i przeglądarkę, ale nie adres IP.',
    ],

    logsTitle: 'Jak długo przechowujemy log',
    logs: 'Log jest przechowywany w dwóch miejscach: w pamięci serwera, która jest czyszczona przy każdym restarcie, oraz w wyjściu konsoli serwera, które jest usuwane przy każdej aktualizacji. Oba mają stały rozmiar, więc każdy nowy wpis wypycha najstarszy, a to, jak długo wpis przetrwa, zależy od ruchu na stronie. Nic z logu nie trafia do archiwum.',

    contentTitle: 'Co tu umieszczasz',
    content: (days) =>
      `Lambda to jej kod, pliki, ustawienia i notatki zapisywane przy jej wersjach – o tym, o co proszono i co się zmieniło. Wszystko to jest przechowywane na serwerze, żeby lambda mogła działać i być edytowana. Darmowa lambda jest usuwana razem ze wszystkimi wersjami około ${days} dni po ostatniej zmianie lub odwiedzinach, a od razu, gdy usunie ją ktoś, kto ma link do edytora. Każdy, kto ma link do edytora, może przeczytać wszystko. To, co dodasz do galerii, widzą wszyscy. Administratorzy serwisu zaglądają do lambdy, gdy muszą – żeby zająć się zgłoszeniem albo zadbać o bezpieczeństwo serwera. Podstawą prawną jest świadczenie usługi, o którą prosisz (art. 6 ust. 1 lit. b RODO).`,

    agentTitle: 'Co mówisz agentowi do budowania',
    agent: (policy) => (
      <>
        To, co wpisujesz w pole na stronie „Zbuduj”, trafia do Anthropic PBC w Stanach Zjednoczonych. Ta firma prowadzi
        model Claude, który pisze aplikację. Co Anthropic robi z tymi danymi, opisuje{' '}
        {policy('polityka prywatności tej firmy')}. Stany Zjednoczone nie chronią danych osobowych tak jak UE. Twoja
        prośba jest tam wysyłana, bo bez tego nie da się zbudować tego, o co prosisz
        (art. 6 ust. 1 lit. b i art. 49 ust. 1 lit. b RODO). Nie wpisuj więc niczego, czym nie chcesz się dzielić.
      </>
    ),
    agentKept:
      'Agent zapisuje twoją prośbę – często własnymi słowami – jako notatkę do wersji, którą pisze. Kilkaset pierwszych znaków trafia do logu usługi budowania, który również ma stały rozmiar. Jeśli zamiast tego używasz własnego agenta, na przykład Claude albo Claude Code, to, co mu mówisz, trafia do jego dostawcy, a nie do nas. My dostajemy tylko kod i notatki, które tu przesyła.',

    lambdasTitle: 'O tym, co robi lambda, decyduje jej właściciel',
    lambdas:
      'Lambdę pisze ten, kto ma link do edytora, a nie my. O co pyta odwiedzających i co robi z tymi danymi, zależy od tej osoby, a ta polityka tego nie obejmuje – poza opisanym wyżej logiem żądań, który serwer prowadzi dla każdej lambdy. Regulamin zabrania używania lambdy do zbierania danych osobowych innych ludzi. Jeśli trafisz na taką, która to robi, zgłoś ją.',

    mailTitle: 'Gdy do nas piszesz',
    mail: 'Jeśli do nas napiszesz – żeby zgłosić nadużycie albo w innej sprawie – używamy twojego adresu i wiadomości, żeby ci odpowiedzieć i zająć się sprawą. Usuwamy je, gdy nie są już do tego potrzebne (art. 6 ust. 1 lit. f RODO).',

    storageTitle: 'Pliki cookie i twoja przeglądarka',
    storage:
      'Jest jeden plik cookie o nazwie lang. Zapamiętuje wybrany przez ciebie język, żeby adresy bez języka otwierały się właśnie w nim, i jest ważny przez rok. Pamięć samej przeglądarki przechowuje jasny lub ciemny motyw, kilka ustawień stron, z których korzystasz, a w przypadku administratorów serwisu – ich token. Nic z tego nie służy do śledzenia cię i nic nie trafia do nikogo innego: nie ma analityki ani reklam, a nic nie jest ładowane z innych stron, nawet czcionki. Wszystko to służy wyłącznie temu, o co prosisz, więc nie wymaga zgody (§ 25 ust. 2 pkt 2 niemieckiej ustawy TDDDG).',

    hostingTitle: 'Gdzie są przechowywane dane',
    hosting:
      'Serwer, na którym to wszystko działa, jest wynajmowany od dostawcy hostingu w Unii Europejskiej. Tam są przechowywane dane opisane na tej stronie.',

    rightsTitle: 'Twoje prawa',
    rights: (mailbox) => (
      <>
        Możesz zapytać, jakie dane o tobie przechowujemy, i poprosić o ich kopię. Możesz też zażądać ich sprostowania,
        usunięcia lub ograniczenia przetwarzania oraz wnieść sprzeciw wobec wszystkiego, co robimy na podstawie naszego
        prawnie uzasadnionego interesu (art. 15–21 RODO). Napisz na {mailbox}. Nie ma kont, więc znajdziemy twoje dane
        tylko wtedy, gdy podpowiesz nam, jak ich szukać: podasz adres IP i przybliżony czas wizyty albo adres swojej
        lambdy. Żadne decyzje wywołujące wobec ciebie skutki prawne lub w podobny sposób istotnie na ciebie wpływające
        nie są podejmowane automatycznie (art. 22 RODO).
      </>
    ),
    complaint:
      'Możesz też złożyć skargę do organu nadzorczego ds. ochrony danych – tam, gdzie mieszkasz, albo tam, gdzie mamy siedzibę. Dla nas właściwy jest pełnomocnik ds. ochrony danych i wolności informacji niemieckiego kraju związkowego Badenia-Wirtembergia (LfDI Baden-Württemberg).',
  },
  change: 'Ta polityka zmienia się razem ze stroną. Obowiązuje wersja opublikowana na tej stronie.',
  updated: 'Ostatnia zmiana: 28 września 2026 r.',
};
