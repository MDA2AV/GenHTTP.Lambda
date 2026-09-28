import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Lambda aanmaken',
  whatTitle: 'Wat wil je maken?',
  whatText:
    'Kies wat er het dichtst bij komt. Dan begin je met een kopie van iets wat al werkt, en die mag je helemaal aanpassen. Of begin met een lege lambda.',
  seeIt: 'Live bekijken',
  startFrom: 'Hiermee beginnen',
  starters: {
    'demo-crud': {
      title: 'Dingen bijhouden',
      description: 'Een lijst die mensen kunnen aanvullen, aanpassen en afvinken: taken, notities, bladwijzers of een kleine voorraad.',
    },
    'demo-registration': {
      title: 'Mensen laten aanmelden',
      description: "Mensen maken een account aan en loggen in, en zien pagina's die alleen voor hen zijn.",
    },
    'demo-game': {
      title: 'Een spel om samen te spelen',
      description: 'Iets wat meerdere mensen tegelijk spelen, live in hun browser.',
    },
    'demo-files': {
      title: "Bestanden en foto's delen",
      description: "Mensen uploaden foto's of documenten, en iedereen kan ze zien.",
    },
    'demo-live': {
      title: 'Live laten zien wat er gebeurt',
      description: 'Een pagina die zichzelf bijwerkt zodra er iets verandert: stemmen, scores, een dashboard.',
    },
    empty: {
      title: 'Iets anders',
      description: 'Begin met een lege lambda en bouw wat je maar wilt.',
    },
  },

  addressTitle: 'Geef hem een adres',
  fromDemo: (title) => (
    <>{title}: je lambda begint als kopie van de demo, en je mag alles erin aanpassen.</>
  ),
  fromNothing: 'Je lambda begint leeg, klaar voor wat je maar wilt.',
  pickAgain: 'Iets anders kiezen',
  publicKey: 'Publieke sleutel',
  free: (key) => `‘${key}’ is nog vrij.`,
  keyHint: 'Kleine letters, cijfers en streepjes. Minstens drie tekens. Laat het leeg voor een willekeurige sleutel.',
  accept: 'Ik ga akkoord met de gebruiksvoorwaarden',
  fullTerms: 'Lees de volledige gebruiksvoorwaarden',
  back: 'Terug',
  creating: 'Aanmaken…',
  submit: 'Lambda aanmaken',
  keepLink: 'Op het volgende scherm zie je je editorlink. Dat is de enige weg terug, dus bewaar hem goed.',
  failed: 'De lambda kon niet worden aangemaakt.',
};
