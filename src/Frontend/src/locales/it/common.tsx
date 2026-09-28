import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navigazione principale',
  build: 'Crea un sito',
  ship: 'Pubblica',
  showcase: 'Vetrina',
  enterprise: 'Enterprise',
  docs: 'Guida',
  admin: 'Admin',
  lightMode: 'Passa al tema chiaro',
  darkMode: 'Passa al tema scuro',
  openMenu: 'Apri il menu',
  closeMenu: 'Chiudi il menu',
  language: 'Lingua',
  terms: 'Termini di servizio',
  privacy: 'Informativa sulla privacy',
  imprint: 'Note legali',
  writeCode: 'Scrivi tu il codice',
  contact: 'Contatti',
};

export const common: Messages['common'] = {
  loading: 'Caricamento…',
  loadingEditor: 'Caricamento dell’editor…',
  editorFailed: 'Impossibile caricare l’editor',
  editorFailedWhy: 'Di solito vuol dire che il sito è stato aggiornato mentre questa scheda era aperta.',
  reload: 'Ricarica la pagina',
  backToStart: 'Torna alla home',
  tryAgain: 'Riprova',
  copy: 'Copia',
  copied: 'Copiato',
  copyToClipboard: 'Copia negli appunti',
  openInNewTab: 'Apri in una nuova scheda',
  close: 'Chiudi',
  operatorCountry: 'Germania',
};

export const notFound: Messages['notFound'] = {
  title: 'Pagina non trovata',
  heading: 'Questa pagina non esiste',
  text: 'Forse il link è vecchio, oppure la lambda a cui puntava è stata eliminata.',
};

export const missing: Messages['missing'] = {
  title: 'Qui non c’è niente online',
  heading: 'Qui non c’è niente online',
  notDeployed: (key) => (
    <>
      La lambda {key} esiste, ma al momento non è online. Nel piano gratuito i deployment restano online finché vengono
      usati, e dopo un mese senza visite né modifiche vanno offline. Chi ha il link di modifica può rimetterla online.
    </>
  ),
  unknown: (key) => (
    <>
      Non c’è nessuna lambda con la chiave {key}. Forse la chiave non è mai esistita, oppure la lambda è stata
      eliminata.
    </>
  ),
  create: 'Crea una lambda qui',
};

export const abuse: Messages['abuse'] = {
  report: 'Segnala un abuso',
  title: 'Segnala una lambda',
  write: 'Scrivici',
  subject: 'Segnalazione di abuso',
  intro:
    'Qui chiunque può mettere online del codice, quindi a volte qualcuno pubblica cose che non dovrebbe. Se una pagina ospitata qui cerca di ingannare le persone, attacca qualcosa o usa materiale senza averne il diritto, diccelo e la rimuoviamo.',
  how: (mailbox, strong, path) => (
    <>
      Scrivi a {mailbox} indicando {strong('l’indirizzo della pagina')} (è simile a {path}) e una frase su cosa non va.
      Uno screenshot aiuta. Non ti serve un account e non devi essere un utente di questo sito.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Cosa succede dopo.')} Una persona legge la tua segnalazione. Se la lambda viola i{' '}
      {terms('termini di servizio')}, viene messa offline, di solito entro un giorno. Non ti diremo chi l’ha pubblicata
      e non possiamo promettere di rispondere a ogni segnalazione, ma le leggiamo tutte.
    </>
  ),
  danger:
    'Se qualcuno è in pericolo immediato o è in corso un reato, contatta anche le autorità locali. Noi possiamo rimuovere una pagina, ma non possiamo fare altro.',
};
