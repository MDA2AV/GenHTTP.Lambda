import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Hauptnavigation',
  build: 'Erstellen',
  ship: 'Veröffentlichen',
  showcase: 'Showcase',
  enterprise: 'Enterprise',
  docs: 'Dokumentation',
  admin: 'Admin',
  lightMode: 'Helles Design aktivieren',
  darkMode: 'Dunkles Design aktivieren',
  openMenu: 'Menü öffnen',
  closeMenu: 'Menü schließen',
  language: 'Sprache',
};

export const common: Messages['common'] = {
  loading: 'Wird geladen …',
  loadingEditor: 'Der Editor wird geladen …',
  editorFailed: 'Der Editor konnte nicht geladen werden',
  editorFailedWhy: 'In der Regel wurde die Website aktualisiert, während dieser Tab geöffnet war.',
  reload: 'Seite neu laden',
  backToStart: 'Zur Startseite',
  tryAgain: 'Erneut versuchen',
  copy: 'Kopieren',
  copied: 'Kopiert',
  copyToClipboard: 'In die Zwischenablage kopieren',
  openInNewTab: 'In neuem Tab öffnen',
  close: 'Schließen',
};

export const notFound: Messages['notFound'] = {
  title: 'Seite nicht gefunden',
  heading: 'Diese Seite existiert nicht',
  text: 'Der Link ist möglicherweise veraltet, oder das Lambda, auf das er verwies, wurde gelöscht.',
};

export const missing: Messages['missing'] = {
  title: 'Unter dieser Adresse läuft nichts',
  heading: 'Unter dieser Adresse läuft nichts',
  notDeployed: (key) => (
    <>
      Unter {key} existiert ein Lambda, es ist derzeit jedoch nicht bereitgestellt. Im kostenlosen Tarif bleiben
      Bereitstellungen online, solange sie genutzt werden, und werden nach einem Monat ohne Aufrufe oder Änderungen
      offline genommen. Wer über den Editor-Link verfügt, kann das Lambda wieder online stellen.
    </>
  ),
  unknown: (key) => (
    <>
      Unter {key} ist kein Lambda gehostet. Der Schlüssel hat möglicherweise nie existiert, oder das zugehörige Lambda
      wurde gelöscht.
    </>
  ),
  create: 'Lambda erstellen',
};

export const abuse: Messages['abuse'] = {
  report: 'Missbrauch melden',
  title: 'Lambda melden',
  write: 'E-Mail schreiben',
  subject: 'Missbrauchsmeldung',
  intro:
    'Auf dieser Plattform kann jede Person Code veröffentlichen. Gelegentlich wird dabei auch Unzulässiges veröffentlicht. Wenn eine hier gehostete Seite Personen täuscht, andere Systeme angreift oder Inhalte ohne entsprechende Rechte verwendet, informieren Sie uns bitte – wir nehmen sie offline.',
  how: (mailbox, strong, path) => (
    <>
      Schreiben Sie an {mailbox} und nennen Sie {strong('die Adresse der Seite')} – sie hat die Form {path} – sowie
      kurz das Problem. Ein Screenshot ist hilfreich. Ein Konto oder eine Nutzung dieser Plattform ist dafür nicht
      erforderlich.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Weiteres Vorgehen.')} Jede Meldung wird von einem Menschen geprüft. Verstößt das Lambda gegen die{' '}
      {terms('Nutzungsbedingungen')}, wird es in der Regel innerhalb eines Tages offline genommen. Angaben zur
      verantwortlichen Person geben wir nicht heraus, und wir können nicht jede Meldung beantworten – gelesen wird jedoch
      jede.
    </>
  ),
  danger:
    'Wenn eine Person in unmittelbarer Gefahr ist oder eine Straftat begangen wird, wenden Sie sich bitte zusätzlich an die zuständigen Behörden. Wir können eine Seite entfernen, jedoch keine weiteren Maßnahmen ergreifen.',
};
