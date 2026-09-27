import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Showcase',
  title: 'Hier erstellt, jetzt in Betrieb',
  intro:
    'Lambdas, die ihre Eigentümer zur Ansicht freigegeben haben. Alle sind online – jede Karte öffnet die laufende Anwendung. Zuletzt häufig genutzte Anwendungen erscheinen zuerst.',
  counted: (total) => (total === 1 ? '1 Lambda' : `${total} Lambdas`),
  failed: 'Der Showcase konnte nicht geladen werden.',
  loadingMore: 'Weitere werden geladen …',
  showMore: 'Weitere anzeigen',
  nothingTitle: 'Noch keine Einträge',
  nothing: (tab) => (
    <>
      Sie haben eine funktionsfähige Anwendung erstellt? Öffnen Sie ihr Kontrollzentrum, wählen Sie {tab('Showcase')}{' '}
      und ergänzen Sie einen Titel, eine kurze Beschreibung und ein Bild. Der Eintrag erscheint hier, solange die
      Anwendung online ist.
    </>
  ),
  buildOne: 'Anwendung erstellen',
  yoursTitle: 'Ihre Anwendung im Showcase',
  yours: (tab) => (
    <>
      Öffnen Sie das Kontrollzentrum Ihres Lambdas und wählen Sie {tab('Showcase')}, oder beauftragen Sie den Agenten,
      der es erstellt hat. Dies ist nur mit dem Editor-Schlüssel möglich, und der Eintrag kann jederzeit wieder entfernt
      werden.
    </>
  ),
  buildSomething: 'Anwendung erstellen',
};

export const card: Messages['card'] = {
  noPicture: 'Noch kein Bild',
  title: 'Titel',
  description: 'Was Besucher damit tun können.',
  opens: (title, address) => `${title}, öffnet ${address} in einem neuen Tab`,
};
