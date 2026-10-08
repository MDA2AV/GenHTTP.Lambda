import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Hauptmenü',
  build: 'Website erstellen',
  ship: 'Veröffentlichen',
  showcase: 'Showcase',
  enterprise: 'Enterprise',
  docs: 'Docs',
  admin: 'Admin',
  lightMode: 'Zum hellen Design wechseln',
  darkMode: 'Zum dunklen Design wechseln',
  openMenu: 'Menü öffnen',
  closeMenu: 'Menü schließen',
  language: 'Sprache',
  terms: 'Nutzungsbedingungen',
  privacy: 'Datenschutz',
  imprint: 'Impressum',
  writeCode: 'Code selbst schreiben',
  contact: 'Kontakt',
};

export const common: Messages['common'] = {
  loading: 'Lädt …',
  loadingEditor: 'Editor lädt …',
  editorFailed: 'Der Editor konnte nicht geladen werden',
  pageFailed: 'Die Seite konnte nicht geladen werden',
  editorFailedWhy: 'Meist wurde die Seite aktualisiert, während dieser Tab offen war.',
  reload: 'Seite neu laden',
  backToStart: 'Zur Startseite',
  tryAgain: 'Nochmal versuchen',
  copy: 'Kopieren',
  copied: 'Kopiert',
  copyToClipboard: 'In die Zwischenablage kopieren',
  openInNewTab: 'In neuem Tab öffnen',
  close: 'Schließen',
  operatorCountry: 'Deutschland',
};

export const notFound: Messages['notFound'] = {
  title: 'Seite nicht gefunden',
  heading: 'Diese Seite gibt es nicht',
  text: 'Vielleicht ist der Link veraltet, oder das Lambda dahinter wurde gelöscht.',
};

export const abuse: Messages['abuse'] = {
  report: 'Missbrauch melden',
  title: 'Lambda melden',
  write: 'E-Mail schreiben',
  subject: 'Missbrauchsmeldung',
  intro:
    'Hier kann jeder Code online stellen. Manchmal ist leider auch etwas dabei, das hier nichts zu suchen hat. Täuscht eine Seite Menschen, greift sie etwas an oder nutzt sie Inhalte ohne Erlaubnis? Dann sagen Sie uns Bescheid, und wir nehmen sie offline.',
  how: (mailbox, strong, path) => (
    <>
      Schreiben Sie an {mailbox} und nennen Sie {strong('die Adresse der Seite')} – sie sieht aus wie {path} – und in
      einem Satz, was nicht stimmt. Ein Screenshot hilft. Sie brauchen kein Konto und müssen die Plattform nicht selbst
      nutzen.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Wie es weitergeht.')} Ein Mensch liest Ihre Meldung. Verstößt das Lambda gegen die{' '}
      {terms('Nutzungsbedingungen')}, nehmen wir es offline, meist innerhalb eines Tages. Wer es erstellt hat, verraten
      wir nicht. Wir können nicht auf jede Meldung antworten – aber wir lesen jede.
    </>
  ),
  danger:
    'Ist jemand in akuter Gefahr oder geschieht gerade eine Straftat, wenden Sie sich bitte auch an die Behörden vor Ort. Wir können eine Seite entfernen – mehr nicht.',
};
