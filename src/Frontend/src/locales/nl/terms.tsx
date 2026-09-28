import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Gebruiksvoorwaarden',
  binding: (english) => (
    <>Deze vertaling is alleen ter informatie. Juridisch geldt de {english('Engelse versie')}.</>
  ),
  intro:
    'Dit is een gratis dienst om dingen uit te proberen. Hij draait code van onbekenden op gedeelde infrastructuur. Dat werkt alleen als iedereen zich aan een paar regels houdt.',
  sections: {
    forbiddenTitle: 'Wat je hier niet mag zetten',
    forbidden: [
      'Geen malware, geen phishing, geen cryptominers. Niets wat andere systemen aanvalt, scant, overspoelt of op een andere manier verstoort, hier of waar dan ook. Niets waarmee je iemand lastigvalt. Niets wat je niet mag publiceren. Daaronder vallen ook code, teksten, afbeeldingen en merken van anderen.',
      'Gebruik een lambda niet om persoonsgegevens van anderen op te slaan of door te sturen. Aan een openbaar adres is niets privé, en dit platform biedt je geen manier om zulke gegevens veilig te bewaren.',
    ],
    actionTitle: 'Wat wij eraan mogen doen',
    action:
      'Alles wat hier gedeployd is, kan op elk moment offline gehaald of verwijderd worden, zonder waarschuwing en zonder dat we het hoeven uit te leggen. In de praktijk gebeurt dat als iets de regels hierboven overtreedt, als het de server bedreigt die iedereen deelt, of als iemand het meldt en gelijk blijkt te hebben.',
    lastingTitle: 'Hoe lang iets blijft',
    lasting: (hours, days) =>
      `Een deployment blijft ongeveer ${hours} uur bereikbaar. Een lambda die je niet hebt geopend, wordt met alle versies van de code verwijderd, ongeveer ${days} dagen nadat je er voor het laatst iets mee hebt gedaan. Opslaan of deployen telt ook, dus alles waar je aan werkt, blijft bestaan. Niets hier is een back-up: bewaar zelf een kopie van code die je belangrijk vindt.`,
    keyTitle: 'Je editorlink is je wachtwoord',
    key: 'Iedereen met de editorlink kan die lambda lezen en aanpassen. Er zit geen account en geen wachtwoord achter. Publiceer je de link, dan geef je iedereen de mogelijkheid om de lambda aan te passen. Een verloren link is niet te herstellen.',
    warrantyTitle: 'Geen garantie',
    warranty:
      'De dienst wordt geleverd zoals hij is. Er is geen garantie dat hij werkt, blijft werken of bewaart wat je erin zet. Hij kan op elk moment herstart, veranderd of uitgezet worden. Bouw er niets op wat belangrijk is voor jou of voor iemand anders.',
    reportTitle: 'Iets melden',
    report: (mailbox, front) => (
      <>
        Doet een lambda hier iets wat niet mag? Mail dan het adres ervan naar {mailbox}. Op de {front('startpagina')}{' '}
        lees je wat je moet meesturen.
      </>
    ),
  },
  change: 'Deze voorwaarden kunnen veranderen. De versie op deze pagina is de versie die geldt.',

  short:
    "Lambda's draaien op gedeelde infrastructuur. Als je er een aanmaakt, ga je akkoord met het volgende. Je deployt geen malware, phishingpagina's of cryptominers, en niets wat andere systemen aanvalt, scant of overspoelt. Je publiceert geen content die je niet mag publiceren. Iedereen die de editorlink kent, kan je lambda aanpassen, dus behandel hem als een wachtwoord. Gratis lambda's blijven online zolang ze gebruikt worden: een lambda die een maand lang door niemand bezocht of bewerkt wordt, gaat offline. Gebeurt er daarna nog twee maanden niets, dan wordt hij verwijderd. Alles wat je deployt, kan op elk moment worden verwijderd.",
};
