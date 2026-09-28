import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Privacybeleid',
  binding: (english) => (
    <>Deze vertaling is alleen ter informatie. Juridisch geldt de {english('Engelse versie')}.</>
  ),
  intro:
    'Wat we via deze site over je te weten komen, wat we ermee doen, hoe lang we het bewaren en wie het verder te zien krijgt. Kort gezegd: er zijn geen accounts, geen advertenties en er is geen tracking. De server houdt bij wie wat heeft opgevraagd, zodat hij kan blijven draaien en misbruik te herleiden is. En wat je aan de bouwagent vraagt, gaat naar Anthropic, want het model van Anthropic schrijft de app.',
  sections: {
    whoTitle: 'Wie er verantwoordelijk is',
    who: 'Deze site wordt beheerd door de persoon hieronder. Volgens de Algemene verordening gegevensbescherming (AVG) van de EU is die persoon verantwoordelijk voor de persoonsgegevens die hier worden verwerkt. Voor alles op deze pagina kun je naar dit adres schrijven:',

    requestsTitle: 'Wat de server bij elk verzoek bijhoudt',
    requests: [
      'Elk verzoek aan deze site, en aan elke lambda die hier draait, komt in het log van de server: het IP-adres waar het vandaan kwam, het adres waarvoor het volgens eigen zeggen is doorgestuurd, de browser of het programma dat het stuurde, het adres dat werd opgevraagd, wanneer dat was en welk antwoord het kreeg. De server zoekt ook op bij welk land, welke plaats en welk netwerk het IP-adres hoort. Dat doet hij in een database die hij zelf bijhoudt, dus er wordt niemand anders iets gevraagd.',
      'Zo vinden we fouten, zo zien we waardoor een server overbelast raakt, en zo sporen we misbruik op dat bij ons gemeld wordt. Het IP-adres wordt ook gebruikt, alleen in het geheugen, om te beperken hoeveel verzoeken en builds één bezoeker kan doen. Zonder deze gegevens kan een verzoek niet beantwoord worden. De rechtsgrond is ons gerechtvaardigd belang om de dienst te laten draaien en veilig te houden (art. 6 lid 1 sub f AVG).',
      'Beheerders kunnen alles inzien. De eigenaar van een lambda ziet bij elk verzoek aan die lambda het land en de browser, maar niet het IP-adres.',
    ],

    logsTitle: 'Hoe lang het log bewaard blijft',
    logs: 'Het log staat op twee plekken: in het geheugen van de server, dat bij elke herstart wordt leeggemaakt, en in de console-uitvoer van de server, die bij elke update wordt verwijderd. Allebei hebben ze een vaste grootte, dus elke nieuwe regel duwt de oudste eruit. Hoe lang een regel blijft staan, hangt af van hoe druk het op de site is. Niets uit het log wordt in een archief bewaard.',

    contentTitle: 'Wat je hier neerzet',
    content: (days) =>
      `Een lambda bestaat uit zijn code, zijn bestanden, zijn instellingen en de notities die bij zijn versies worden opgeslagen: wat er werd gevraagd en wat er veranderde. Dat staat allemaal op de server, zodat de lambda kan draaien en bewerkt kan worden. Een gratis lambda wordt met al zijn versies verwijderd ongeveer ${days} dagen nadat hij voor het laatst is gewijzigd of bezocht, en meteen als iemand met de editorlink hem verwijdert. Iedereen met de editorlink kan alles inzien. Wat je in de showcase zet, is voor iedereen zichtbaar. En beheerders bekijken een lambda als het moet, om een melding af te handelen of de server veilig te houden. De rechtsgrond is het leveren van de dienst waar je om hebt gevraagd (art. 6 lid 1 sub b AVG).`,

    agentTitle: 'Wat je aan de bouwagent vraagt',
    agent: (policy) => (
      <>
        Wat je op de pagina ‘App maken’ in het invoervak typt, wordt naar Anthropic PBC in de Verenigde Staten
        gestuurd. Anthropic draait Claude, het model dat de app schrijft. Wat Anthropic ermee doet, staat in{' '}
        {policy('het eigen privacybeleid van Anthropic')}. De Verenigde Staten beschermen persoonsgegevens niet zoals de
        EU dat doet. Je verzoek gaat daarheen omdat dat nodig is om te bouwen wat je vroeg
        (art. 6 lid 1 sub b en art. 49 lid 1 sub b AVG). Zet er dus niets in wat je niet met anderen wilt delen.
      </>
    ),
    agentKept:
      'De agent bewaart je verzoek, vaak in zijn eigen woorden, als notitie bij de versie die hij schrijft. De eerste paar honderd tekens ervan komen in het log van de bouwdienst, dat ook een vaste grootte heeft. Gebruik je in plaats daarvan je eigen agent, zoals Claude of Claude Code? Dan gaat wat je hem vertelt naar de aanbieder van die agent, niet naar ons. Wij krijgen alleen de code en de notities die hij hierheen stuurt.',

    lambdasTitle: 'Wat een lambda doet, bepaalt de eigenaar',
    lambdas:
      'Een lambda wordt geschreven door wie de editorlink heeft, niet door ons. Wat hij aan bezoekers vraagt en wat hij daarmee doet, bepaalt die persoon, en dat valt buiten dit privacybeleid. Alleen het verzoeklog hierboven, dat de server voor elke lambda bijhoudt, valt er wel onder. Volgens de gebruiksvoorwaarden mag je een lambda niet gebruiken om persoonsgegevens van anderen te verzamelen. Kom je er een tegen die dat wel doet? Meld het dan.',

    mailTitle: 'Als je ons mailt',
    mail: 'Als je ons schrijft, om misbruik te melden of over iets anders, gebruiken we je adres en je bericht om je te antwoorden en af te handelen wat je schreef. We verwijderen ze zodra we ze daarvoor niet meer nodig hebben (art. 6 lid 1 sub f AVG).',

    storageTitle: 'Cookies en je browser',
    storage:
      "Er is één cookie, met de naam lang. Die onthoudt welke taal je hebt gekozen, zodat adressen zonder taal in die taal openen, en blijft een jaar bewaard. De opslag van je browser zelf onthoudt of je de lichte of donkere modus gebruikt, een paar instellingen van de pagina's die je gebruikt en, voor beheerders, hun token. Niets daarvan wordt gebruikt om je te volgen en niets gaat naar anderen: er zijn geen analytics en geen advertenties, en er wordt niets van andere sites geladen, zelfs geen lettertypen. Omdat dit allemaal alleen doet waar je zelf om vraagt, is er geen toestemming nodig (§ 25 lid 2 nr. 2 van de Duitse TDDDG).",

    hostingTitle: 'Waar het wordt bewaard',
    hosting:
      'De server waarop dit allemaal draait, wordt gehuurd van een hostingbedrijf in de Europese Unie. Wat op deze pagina staat beschreven, wordt daar opgeslagen.',

    rightsTitle: 'Je rechten',
    rights: (mailbox) => (
      <>
        Je kunt vragen wat we over je hebben opgeslagen en om een kopie daarvan. Je kunt het laten corrigeren of
        verwijderen, het gebruik ervan laten beperken, en bezwaar maken tegen alles wat we doen op grond van ons
        gerechtvaardigd belang (art. 15 tot en met 21 AVG). Mail daarvoor naar {mailbox}. Er zijn geen accounts, dus we
        kunnen alleen vinden wat van jou is als je ons vertelt hoe: het IP-adres dat je gebruikte en ongeveer wanneer, of
        het adres van je lambda. Er worden over jou geen automatische beslissingen genomen die rechtsgevolgen hebben of
        je op een vergelijkbare manier sterk raken (art. 22 AVG).
      </>
    ),
    complaint:
      'Je kunt ook een klacht indienen bij een toezichthouder voor gegevensbescherming, waar je woont of waar wij zitten. Voor ons is dat de toezichthouder voor gegevensbescherming en informatievrijheid van de Duitse deelstaat Baden-Württemberg (LfDI Baden-Württemberg).',
  },
  change: 'Dit beleid verandert als de site verandert. De versie op deze pagina is de versie die geldt.',
  updated: 'Laatst gewijzigd op 28 september 2026.',
};
