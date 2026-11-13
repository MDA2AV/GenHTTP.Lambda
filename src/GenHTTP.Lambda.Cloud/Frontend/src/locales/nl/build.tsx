import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Website maken met AI.',
  intro:
    'Beschrijf in je eigen woorden de website of app die je voor ogen hebt. AI bouwt hem voor je, wij hosten hem en binnen een paar minuten staat hij online, met een link die je naar iedereen kunt sturen. Gratis, zonder programmeren, zonder account.',
  points: [
    {
      title: 'Zonder programmeren',
      text: 'Zeg in je eigen woorden wat je website moet doen, zoals je het aan een vriend zou vertellen. AI bouwt hem voor je, technische kennis is niet nodig.',
    },
    {
      title: 'Gratis hosting inbegrepen',
      text: 'Je website draait op onze servers. Geen hostingpakket, server of domeinnaam om te kopen, niets te installeren. Beveiliging en updates regelen wij.',
    },
    {
      title: 'Binnen enkele minuten online',
      text: 'Je krijgt meteen een link om te delen. De website onthoudt wat mensen invullen, zoals aanmeldingen, stemmen, berichten en scores, zodat iedereen hetzelfde ziet.',
    },
  ],

  questionsTitle: 'Voor je begint',
  questions: (offline, removed) => [
    [
      'Kan AI echt gratis een website voor me maken?',
      `Ja. Beschrijf hem in je eigen woorden en AI bouwt hem, zet hem online en geeft je de link. Geen account, geen creditcard, geen proefperiode. Hij blijft online zolang mensen hem gebruiken: na ${offline} dagen zonder bezoek of wijziging gaat hij offline, en na ${removed} dagen wordt hij verwijderd.`,
    ],
    [
      'Heb ik hosting, een server of een domeinnaam nodig?',
      'Nee. Je website draait op onze servers, met hosting, beveiliging en updates inbegrepen. Je krijgt meteen een link, dus je hoeft ook geen domeinnaam te kopen.',
    ],
    [
      'Kan ik een app maken zonder te kunnen programmeren?',
      'Ja. Je ziet nooit code. Zeg wat hij moet doen, zoals je het aan een vriend zou vertellen, en AI doet de rest: een website, een kleine app of een spel.',
    ],
    [
      'Kunnen mensen iets invullen, zoals aanmeldingen, stemmen of berichten?',
      'Ja. Je website onthoudt wat mensen invullen. Iedereen die de link opent, ziet dus dezelfde inzendingen, stemmen en scores.',
    ],
    [
      'Hoe openen anderen mijn website?',
      'Met de link, in elke browser, op hun telefoon of computer. Er hoeft niets geïnstalleerd te worden en er zit geen app store tussen.',
    ],
    [
      'Hoe pas ik mijn website later aan?',
      'Open de editorlink die je bij je website krijgt en beschrijf wat er anders moet, net als hier. Vind je een wijziging niet goed? Dan ga je gewoon terug naar hoe het eerst was.',
    ],
  ],

};
