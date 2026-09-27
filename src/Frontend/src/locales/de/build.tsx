import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Auf dieser Installation nicht verfügbar',
  off: (write, mcp) => (
    <>
      Diese Installation verfügt über keinen Build-Agenten. Sie können den Code jedoch {write('selbst schreiben')} oder
      Ihren eigenen Claude mit {mcp} verbinden.
    </>
  ),

  title: 'Beschreiben Sie, was Sie benötigen.',
  intro:
    'Die Anwendung wird erstellt und online gestellt, und Sie erhalten einen Link, den Sie mit anderen teilen können. Ohne Konto, ohne Installation – und sie kann Daten speichern, etwa Punktestände, Nachrichten oder Einträge, sodass alle Nutzer denselben Stand sehen.',
  placeholder: 'Erstelle eine …',
  working: 'in Arbeit …',
  shortcut: 'Strg + Enter',
  building: 'Wird erstellt',
  buildIt: 'Erstellen',
  builtBy: 'Erstellt mit',
  password: 'Passwort',
  fable:
    'Fable ist während der Erprobung durch ein Passwort geschützt. Es arbeitet ohne Zeitlimit, bis die Anwendung fertiggestellt ist.',
  onlyNew:
    'Hier werden ausschließlich neue Anwendungen erstellt. Um eine bestehende Anwendung weiterzuentwickeln, übergeben Sie deren Editor-Link Ihrem eigenen Coding-Agenten – siehe unten.',
  ideas: [
    'eine Pinnwand, auf der jede Person eine kurze Nachricht hinterlassen kann',
    'eine Bestenliste für ein Würfelspiel',
    'eine Umfrage mit sichtbarem Zwischenergebnis',
    'ein Gästebuch für unsere Hochzeit',
    'einen Countdown bis zu einem gemeinsamen Termin',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Ein Auftrag vor Ihrem – Sie sind als Nächstes an der Reihe.' : `${waiting} Aufträge vor Ihrem.`,
  starting: 'Wird gestartet …',

  yourApp: 'Ihre Anwendung',
  further: 'Zur Weiterentwicklung',
  keep: 'Bitte bewahren Sie diesen Link sicher auf. Er ist der einzige Zugang und kann nicht wiederhergestellt werden – auch nicht durch uns. Legen Sie ein Lesezeichen an, bevor Sie diesen Tab schließen.',
  change:
    'Diese Seite erstellt ausschließlich neue Anwendungen. Um diese Anwendung zu ändern, verbinden Sie Ihren eigenen Coding-Agenten wie unten beschrieben, übergeben ihm den Editor-Link und beschreiben die gewünschte Änderung.',
  copyLink: 'Editor-Link kopieren',
  lifetime: (offline, removed) =>
    `Die Anwendung bleibt online, solange sie genutzt wird: Nach ${offline} Tagen ohne Aufrufe oder Änderungen wird sie offline genommen, nach ${removed} Tagen entfernt. Über den Editor und „Bereitstellen“ lässt sie sich wieder online stellen.`,
  openEditor: 'Editor öffnen',
  another: 'Weitere Anwendung erstellen',

  keepGoing: 'Mit Ihrem eigenen Agenten weiterarbeiten',
  orOwn: 'Oder Ihren eigenen Agenten verwenden',
  ownText:
    'Das Eingabefeld oben nutzt einen Claude, der auf diesem Server läuft. Wenn Sie bereits einen eigenen Agenten verwenden, können Sie ihn stattdessen verbinden. Er verfügt über dieselben Möglichkeiten – ein Lambda anlegen, den Code schreiben, es online stellen –, ohne Tageslimit und ohne diese Seite.',
  thenAsk: 'Anschließend beschreiben Sie Ihre Anforderung wie hier.',
  claudeWeb: 'Claude im Web',
  claudeWebHow:
    'Einstellungen, dann „Connectors“, dann „Add custom connector“. Fügen Sie die obige Adresse als URL des entfernten MCP-Servers ein. Ein Schlüssel oder eine Anmeldung ist nicht erforderlich.',
  howToChange:
    'Auf diesem Weg ändern Sie auch eine bestehende Anwendung: Übergeben Sie Ihrem Agenten den Editor-Link und beschreiben Sie die Änderung.',
  more: 'Mehr zur Nutzung eines Agenten',

  failedToStart: 'Die Anfrage konnte nicht gestartet werden.',
  noAnswer: 'Der Vorgang wurde ohne Rückmeldung beendet.',
  failed: 'Der Vorgang ist fehlgeschlagen.',
};
