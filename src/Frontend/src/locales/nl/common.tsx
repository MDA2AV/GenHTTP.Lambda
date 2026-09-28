import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Hoofdmenu',
  build: 'Website maken',
  ship: 'Publiceren',
  showcase: 'Showcase',
  enterprise: 'Enterprise',
  docs: 'Docs',
  admin: 'Admin',
  lightMode: 'Overschakelen naar lichte modus',
  darkMode: 'Overschakelen naar donkere modus',
  openMenu: 'Menu openen',
  closeMenu: 'Menu sluiten',
  language: 'Taal',
  terms: 'Gebruiksvoorwaarden',
  privacy: 'Privacybeleid',
  imprint: 'Colofon',
  writeCode: 'Schrijf de code zelf',
  contact: 'Contact',
};

export const common: Messages['common'] = {
  loading: 'Laden…',
  loadingEditor: 'Editor laden…',
  editorFailed: 'De editor kon niet worden geladen',
  editorFailedWhy: 'Meestal komt dit doordat de site is bijgewerkt terwijl dit tabblad openstond.',
  reload: 'Pagina herladen',
  backToStart: 'Terug naar de startpagina',
  tryAgain: 'Opnieuw proberen',
  copy: 'Kopiëren',
  copied: 'Gekopieerd',
  copyToClipboard: 'Naar klembord kopiëren',
  openInNewTab: 'Openen in nieuw tabblad',
  close: 'Sluiten',
  operatorCountry: 'Duitsland',
};

export const notFound: Messages['notFound'] = {
  title: 'Pagina niet gevonden',
  heading: 'Deze pagina bestaat niet',
  text: 'Misschien is de link verouderd. Of de lambda waar hij naar verwees, is verwijderd.',
};

export const missing: Messages['missing'] = {
  title: 'Hier draait niets',
  heading: 'Hier draait niets',
  notDeployed: (key) => (
    <>
      Er staat een lambda op {key}, maar die is nu niet gedeployd. Gratis deployments blijven online zolang ze
      gebruikt worden. Na een maand zonder bezoek of wijzigingen gaan ze offline. Wie de editorlink heeft, kan de
      lambda weer online zetten.
    </>
  ),
  unknown: (key) => (
    <>
      Op {key} staat geen lambda. Misschien heeft deze sleutel nooit bestaan, of is de lambda erachter verwijderd.
    </>
  ),
  create: 'Hier een lambda aanmaken',
};

export const abuse: Messages['abuse'] = {
  report: 'Misbruik melden',
  title: 'Een lambda melden',
  write: 'Mail ons',
  subject: 'Misbruikmelding',
  intro:
    'Iedereen kan hier code online zetten. Soms zet iemand dus iets online wat niet mag. Probeert een pagina hier mensen te misleiden, valt hij iets aan of gebruikt hij materiaal waar hij geen recht op heeft? Laat het ons weten, dan halen we hem weg.',
  how: (mailbox, strong, path) => (
    <>
      Mail naar {mailbox} met {strong('het adres van de pagina')} (dat ziet eruit als {path}) en een zin over wat er
      mis is. Een screenshot helpt. Je hebt geen account nodig en je hoeft deze site niet zelf te gebruiken.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Wat er daarna gebeurt.')} Een mens leest je melding. Overtreedt de lambda de{' '}
      {terms('gebruiksvoorwaarden')}, dan halen we hem offline, meestal binnen een dag. We vertellen niet wie hem
      online heeft gezet. En we kunnen niet beloven dat we op elke melding reageren. Maar we lezen ze allemaal.
    </>
  ),
  danger:
    'Is iemand in direct gevaar, of wordt er een misdrijf gepleegd? Neem dan ook contact op met de politie of andere instanties. Wij kunnen een pagina weghalen, maar verder niets.',
};
