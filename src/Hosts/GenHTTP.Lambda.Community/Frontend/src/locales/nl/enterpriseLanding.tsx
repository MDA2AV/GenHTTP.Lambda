import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Apps voor je werk',
  headline: 'Beschrijf de app die je werk nodig heeft.',
  headlineAccent: 'Een AI-agent bouwt hem en zet hem hier online.',
  intro:
    'Een formulier, een boekingskalender, een lijst voor je team: zeg in je eigen woorden wat de app moet doen. Binnen een paar minuten heb je een link om met je collega’s te delen - en later pas je de app op dezelfde manier aan, zonder code te schrijven.',
  describe: 'Beschrijf een app',
  own: 'Gebruik je eigen AI-agent',

  examplesTitle: 'Wat mensen hier bouwen',
  examples: [
    { title: 'Formulieren en aanvragen', text: 'Verlof, inkopen, feedback: mensen vullen het in en wie erover beslist, keurt het goed.' },
    { title: 'Reserveringen', text: 'Vergaderruimtes, werkplekken, apparatuur: wie heeft wat, en wanneer.' },
    { title: 'Lijsten en overzichten', text: 'Supportaanvragen, voorraad, onboarding: wat staat er open en wie pakt het op.' },
    { title: 'Pagina’s voor een team', text: 'Een rooster, een handboek, nieuws: één link die iedereen opent.' },
  ],

  stepsTitle: 'Zo werkt het',
  steps: [
    {
      title: 'Beschrijf het',
      text: 'Zeg wat de app moet doen, zoals je het aan een collega zou uitleggen. De ideeën onder het vak zijn een goed begin.',
    },
    {
      title: 'Deel de link',
      text: 'Een paar minuten later staat de app hier online. Stuur de link naar de mensen die hem gebruiken en bewaar je editorlink: daarmee pas je de app aan.',
    },
    {
      title: 'Vraag om meer',
      text: 'In hetzelfde vak: iets nieuws, iets anders, of hoe het ermee gaat - wie hem gebruikte en wat er is ingevoerd. Eerdere versies blijven bewaard, dus de vorige kan weer online worden gezet.',
    },
  ],

  questionsTitle: 'Handig om te weten',
  questions: (showcase) => [
    ['Moet ik kunnen programmeren?', 'Nee. Jij beschrijft wat de app moet doen en de agent schrijft hem. Je hoeft nooit naar de code te kijken.'],
    ['Wie kan mijn app openen?', 'Wie de link heeft en deze site kan bereiken. Stuur hem naar de mensen die hem moeten gebruiken.'],
    ['Waar wordt bewaard wat mensen invoeren?', 'Elke app bewaart zijn eigen items op deze installatie, die je organisatie beheert.'],
    [
      'Hoe kom ik terug bij mijn app?',
      'Deze browser onthoudt de apps die je hier hebt gebouwd, naast het vak waarin je ze bouwde. Open op een ander apparaat de editorlink die je bij de app kreeg - bewaar hem op een veilige plek, want niemand kan hem terughalen.',
    ],
    [
      'Er werkt iets niet. Wat nu?',
      'Vertel de agent wat er mis is, op dezelfde manier als waarop je om de app vroeg. Heeft een wijziging het erger gemaakt, zet dan in de editor van de app de vorige versie weer online.',
    ],
    ['Wat hebben collega’s gebouwd?', <>De apps die zij willen laten zien, staan in de {showcase('showcase')}.</>],
  ],
};
