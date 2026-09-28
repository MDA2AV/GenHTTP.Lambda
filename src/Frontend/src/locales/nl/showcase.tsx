import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Showcase',
  title: 'Hier gebouwd, nu live',
  intro:
    "Lambda's die hun eigenaars graag laten zien. Ze staan allemaal online, dus elke kaart opent de echte app. Wat onlangs nog gebruikt is, staat vooraan.",
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambda's`),
  failed: 'De showcase kon niet worden geladen.',
  loadingMore: 'Meer laden…',
  showMore: 'Meer tonen',
  nothingTitle: 'Nog niets te zien',
  nothing: (tab) => (
    <>
      Iets gebouwd dat werkt? Open het dashboard, kies {tab('Showcase')} en voeg een titel, een paar woorden en een
      afbeelding toe. Zolang je app online is, staat hij hier.
    </>
  ),
  buildOne: 'App maken',
  yoursTitle: 'Jouw app hier?',
  yours: (tab) => (
    <>
      Open het dashboard van je lambda en kies {tab('Showcase')}, of vraag de agent die hem bouwde om hem hier te
      zetten. Alleen wie de editorsleutel heeft, kan dat. En je kunt hem er altijd weer af halen.
    </>
  ),
  buildSomething: 'Maak een app',
};

export const card: Messages['card'] = {
  noPicture: 'Nog geen afbeelding',
  title: 'Titel',
  description: 'Wat een bezoeker ermee kan doen.',
  opens: (title, address) => `${title}, opent ${address} in een nieuw tabblad`,
};
