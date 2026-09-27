import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navigazione principale',
  build: 'Crea',
  ship: 'Pubblica',
  showcase: 'Vetrina',
  enterprise: 'Aziende',
  docs: 'Documentazione',
  admin: 'Admin',
  lightMode: 'Passa al tema chiaro',
  darkMode: 'Passa al tema scuro',
  openMenu: 'Apri il menu',
  closeMenu: 'Chiudi il menu',
  language: 'Lingua',
};

export const common: Messages['common'] = {
  loading: 'Caricamento…',
  loadingEditor: 'Caricamento dell’editor…',
  editorFailed: 'Impossibile caricare l’editor',
  editorFailedWhy: 'In genere significa che il sito è stato aggiornato mentre questa scheda era aperta.',
  reload: 'Ricarica la pagina',
  backToStart: 'Torna alla home',
  tryAgain: 'Riprova',
  copy: 'Copia',
  copied: 'Copiato',
  copyToClipboard: 'Copia negli appunti',
  openInNewTab: 'Apri in una nuova scheda',
  close: 'Chiudi',
};

export const notFound: Messages['notFound'] = {
  title: 'Pagina non trovata',
  heading: 'Questa pagina non esiste',
  text: 'Il link potrebbe non essere più valido, oppure il lambda a cui rimandava è stato eliminato.',
};

export const missing: Messages['missing'] = {
  title: 'Nessuna applicazione attiva a questo indirizzo',
  heading: 'Nessuna applicazione attiva a questo indirizzo',
  notDeployed: (key) => (
    <>
      All’indirizzo {key} esiste un lambda, che al momento però non è distribuito. Nel piano gratuito le distribuzioni
      restano online finché vengono utilizzate e vengono disattivate dopo un mese senza visite né modifiche. Chi dispone
      del link di modifica può rimetterlo online.
    </>
  ),
  unknown: (key) => (
    <>
      All’indirizzo {key} non è ospitato alcun lambda. La chiave potrebbe non essere mai esistita, oppure il lambda
      corrispondente è stato eliminato.
    </>
  ),
  create: 'Crea un lambda',
};

export const abuse: Messages['abuse'] = {
  report: 'Segnala un abuso',
  title: 'Segnala un lambda',
  write: 'Scrivici',
  subject: 'Segnalazione di abuso',
  intro:
    'Su questa piattaforma chiunque può pubblicare codice, e talvolta vengono pubblicati contenuti non consentiti. Se una pagina ospitata qui cerca di ingannare, attacca altri sistemi o utilizza materiale senza averne i diritti, La preghiamo di segnalarcelo: provvederemo a rimuoverla.',
  how: (mailbox, strong, path) => (
    <>
      Scriva a {mailbox} indicando {strong('l’indirizzo della pagina')} – nel formato {path} – e una breve descrizione del
      problema. Uno screenshot è utile. Non sono necessari un account né l’utilizzo di questo sito.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Cosa accade in seguito.')} Ogni segnalazione viene esaminata da una persona. Se il lambda viola i{' '}
      {terms('termini di servizio')}, viene disattivato, di norma entro un giorno. Non comunichiamo chi lo ha pubblicato e
      non possiamo rispondere a ogni segnalazione, ma tutte vengono lette.
    </>
  ),
  danger:
    'Se una persona è in pericolo immediato o è in corso un reato, La preghiamo di contattare anche le autorità competenti. Possiamo rimuovere una pagina, ma non possiamo intraprendere altre azioni.',
};
