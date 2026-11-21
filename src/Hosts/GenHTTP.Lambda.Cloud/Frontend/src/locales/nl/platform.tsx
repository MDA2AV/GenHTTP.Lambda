import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'AI-app-builder, hosting inbegrepen',
  title: 'Bouwen duurt een minuut.',
  titleAccent: 'De rest zit er al in.',
  intro:
    'Beschrijf een app aan onze AI-agent - of aan Claude Code, Codex of Cursor - en hij staat binnen enkele minuten online, voor je gehost, met een link om te delen. Alles wat hem nuttig houdt is er al: zijn items, zijn versies en waar hij voor dient, wie hem bezoekt, en een eigen domein zodra hij aanslaat.',
  build: 'Bouw iets',
  ownAgent: 'Gebruik je eigen agent',
  free: 'Gratis. Geen account, geen creditcard.',
  heroAlt: 'Een tennisclub vraagt om een website voor het reserveren van banen en de agent antwoordt dat die online staat - met een knop om hem te openen en de editorlink om te bewaren',
  example: 'Elke afbeelding op deze pagina toont dezelfde voorbeeldapp: de baanreservering van een tennisclub.',

  contents: 'Op deze pagina',

  makeTitle: 'Bouw hem, pas hem aan',
  makeText: 'Zeg wat je nodig hebt, en blijf het zeggen. Daarvoor hoeft niemand code te schrijven of te lezen.',

  behindTitle: 'Een database, versiegeschiedenis en analytics voor elke app',
  behindText:
    'Wat een app nuttig houdt, is alles rond zijn code - er voor wie hem maakte, niet alleen voor wie code kan lezen.',

  yoursTitle: 'Van jou om verder mee te gaan',
  yoursText: 'Voor ontwikkelaars, en voor iedereen die wil dat zijn app deze site overleeft.',

  features: {
    build: {
      name: 'Bouwen',
      title: 'Beschrijf het, en het staat online',
      text: 'Vertel onze agent wat je nodig hebt, in je eigen woorden - of vraag het aan Claude Code, Codex of Cursor. Een paar minuten later draait de app op een eigen adres, klaar om te delen. Je hoeft geen server, database of account in te stellen.',
      points: ['Onze agent op de bouwpagina, of je eigen agent via MCP', 'Binnen minuten een link om te delen', 'Gratis, zonder account'],
      alt: 'De reserveringspagina: vier banen, uur voor uur, met de gereserveerde uren bij naam',
    },
    change: {
      name: 'Aanpassen',
      title: 'Vraag ernaar, pas hem aan',
      text: 'Je apps blijven in een lijst op de bouwpagina staan, elk met een gesprek. Vraag hoe het ermee gaat - wie hem bezocht, wat mensen invulden, of er iets misgaat - of zeg wat er anders moet, en de agent antwoordt of past hem aan. Of geef de editorlink aan je eigen agent.',
      points: ['Antwoorden over bezoekers, items en fouten', 'Vanzelf online zodra het werkt, of als concept bewaard zodat je het eerst probeert', 'De code blijft uit beeld tenzij je hem wilt zien'],
      alt: 'De bouwpagina: de club vroeg hoeveel banen deze week geboekt waren, de agent antwoordde, en de lessen van de trainer staan klaar om te proberen in een concept',
    },
    drafts: {
      name: 'Concepten',
      title: 'Probeer een wijziging voordat iemand haar ziet',
      text: 'Een grotere wijziging maak je in een concept: de app zoals hij wordt, op een eigen adres, met een kopie van de items om hem op te proberen. Niets wat je daar doet raakt de echte. Is het goed, dan zet je het met één klik online.',
      points: ['Een eigen adres, om het te proberen en aan anderen te laten zien', 'Testdata: een kopie van de items, niet de items zelf', 'Online zetten als het goed is, of weggooien'],
      alt: 'Een concept van de reserveringspagina, waarin de lessen van de trainer paars staan - in een kopie van de reserveringen',
    },
    history: {
      name: 'Geschiedenis',
      title: 'Elke wijziging een versie, met een weg terug',
      text: 'Elke wijziging wordt een nieuwe versie, beschreven door wat ze veranderde. Gaat er iets mis, dan is de versie ervoor één klik weg - op hetzelfde adres, met dezelfde items.',
      points: ['Wat er gevraagd werd, en wat het veranderde', 'Met één klik terug naar een eerdere versie', 'Items zijn van alle versies samen, dus teruggaan kost er geen'],
      alt: 'De geschiedenis van de app: zijn wijzigingen onder elkaar, de nieuwste online',
    },
    about: {
      name: 'Over de app',
      title: 'Hij weet wat hij is',
      text: 'Bij elke wijziging schrijft de agent op waar de app voor dient, wie hem gebruikt en waarom hij werkt zoals hij werkt. Jij leest het in gewone taal, en de volgende agent leest het voordat hij iets aanpast - zodat een wijziging voortbouwt op wat er is in plaats van te gokken.',
      points: ['Wat hij doet en voor wie, in gewone taal', 'De keuzes erachter, voor wie hem hierna aanpast', 'Hoe hij getest wordt, bewaard bij elke versie'],
      alt: 'Over je app: de baanreservering in gewone taal beschreven, wie hem gebruikt en wat hij doet',
    },
    records: {
      name: 'Items',
      title: 'Items in een eigen database',
      text: 'Een app die dingen bewaart - reserveringen, aanmeldingen, scores - bewaart ze in een eigen database, en ze blijven staan als de app verandert. Lees ze tabel voor tabel in de editor, naast de bestanden die de app opsloeg, zoals de weeklijst voor het prikbord.',
      points: ['Een database voor elke app die er een nodig heeft', 'Elke tabel, leesbaar in de editor', 'De bestanden die de app opslaat, ernaast opgesomd'],
      alt: 'De items van de app: zijn reserveringen en zijn leden, tabel voor tabel gelezen',
    },
    keys: {
      name: 'Sleutels en wachtwoorden',
      title: 'Sleutels en wachtwoorden blijven geheim',
      text: 'Heeft een app een wachtwoord nodig, of de sleutel van een andere dienst, dan vraagt de agent er alleen bij naam om. Jij typt de waarde in de editor, en vanaf dan ziet niemand hem meer - jij niet, de agent niet. Alleen de app leest hem.',
      points: ['De agent ziet de waarde nooit', 'Versleuteld bewaard, nooit meer getoond', 'Te vervangen wanneer je wilt'],
      alt: 'Sleutels en wachtwoorden: het wachtwoord van de receptie is ingesteld, en de waarde is verborgen',
    },
    analytics: {
      name: 'Analytics',
      title: 'Zie wie er langskomt, zonder cookies',
      text: 'Zet analytics aan en zie hoeveel mensen de app openden, waar ze vandaan kwamen en op welke apparaten - geteld op onze server, zonder cookies en zonder script in je pagina’s.',
      points: ['Bezoekers, paginaweergaven en waar ze vandaan kwamen', 'Geen cookies, en niets wat een bezoeker bij naam noemt', 'Uit totdat je het aanzet'],
      alt: 'Analytics: de bezoekers van de afgelopen dertig dagen, dag voor dag',
    },
    domain: {
      name: 'Domein',
      title: 'Een eigen domein',
      text: 'Elke app begint op een adres van ons. Slaat hij aan, dan kan hij verhuizen naar een domein dat van jou is: dezelfde app en dezelfde editor, op een adres dat mensen onthouden. Links naar het oude adres blijven werken - ze leiden naar het nieuwe.',
      points: ['Voor Premium-apps: schrijf ons', 'Dezelfde app, editor en items', 'Het oude adres stuurt bezoekers door'],
      alt: 'Het adres van de app verhuist van zijn adres op genhttp.run naar het eigen domein van de club',
    },
    code: {
      name: 'Code en git',
      title: 'De code, wanneer je maar wilt',
      text: 'Zet de editor op de volledige weergave, en de code van elke versie staat er, met de logs en de cijfers. Kloon de app met git en push een wijziging, of download hem als .NET-project dat overal draait. Het is jouw code.',
      points: ['Elk bestand, vergeleken van versie tot versie', 'Git: een commit naar main is een versie, een branch is een concept', 'Een download die op zichzelf draait, items inbegrepen'],
      alt: 'De volledige weergave van de editor: de bestanden van de app en zijn code',
    },
    publish: {
      name: 'Showcase en bron',
      title: 'Laat het zien, of publiceer de code',
      text: 'Zet de app in de showcase zodat anderen hem vinden, of publiceer de code onder een open-sourcelicentie zodat iedereen hem kan lezen, een ster kan geven en kan downloaden. Beide zijn jouw keuze, en geen van beide deelt je items.',
      points: ['In de showcase, met een afbeelding en een beschrijving', 'De code onder MIT of een andere licentie', 'Nooit je items, sleutels of bezoekers'],
      alt: 'De gepubliceerde code van de baanreservering: bestanden, documentatie en versies, onder de MIT-licentie',
    },
  },

  draftBadge: 'Concept',
  domainOwn: 'Premium',
  domainSubject: 'Een domein voor mijn app',
  domainAsk: 'Vraag naar een domein',
  zoom: 'Groter tonen',
  close: 'Sluiten',

  closeTitle: 'Gratis te proberen.',
  closeAccent: 'Geen account.',
  closeBuild: 'Bouw iets',
  closeShip: 'Publiceer wat je bouwde',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda is open source. Gebruik het hier, of {selfHosting('draai het op je eigen server')} - de code staat op{' '}
      {github('GitHub')}.
    </>
  ),
};
