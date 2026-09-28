import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Showcase',
  title: 'Hier gebaut, jetzt online',
  intro:
    'Lambdas, die ihre Besitzer zeigen möchten. Alle sind online – jede Karte öffnet die echte App. Was zuletzt genutzt wurde, steht oben.',
  counted: (total) => (total === 1 ? '1 Lambda' : `${total} Lambdas`),
  failed: 'Der Showcase konnte nicht geladen werden.',
  loadingMore: 'Lädt weitere …',
  showMore: 'Mehr anzeigen',
  nothingTitle: 'Noch nichts zu sehen',
  nothing: (tab) => (
    <>
      Sie haben etwas gebaut, das funktioniert? Öffnen Sie das Kontrollzentrum, wählen Sie {tab('Showcase')} und fügen
      Sie einen Titel, ein paar Worte und ein Bild hinzu. Solange die App online ist, erscheint sie hier.
    </>
  ),
  buildOne: 'App bauen',
  yoursTitle: 'Ihre App im Showcase?',
  yours: (tab) => (
    <>
      Öffnen Sie das Kontrollzentrum Ihres Lambdas und wählen Sie {tab('Showcase')} – oder bitten Sie den Agenten, der es
      gebaut hat. Das kann nur, wer den Editor-Link hat. Und Sie können den Eintrag jederzeit wieder entfernen.
    </>
  ),
  buildSomething: 'App bauen',
};

export const card: Messages['card'] = {
  noPicture: 'Noch kein Bild',
  title: 'Titel',
  description: 'Was Besucher damit machen können.',
  opens: (title, address) => `${title}, öffnet ${address} in einem neuen Tab`,
};
