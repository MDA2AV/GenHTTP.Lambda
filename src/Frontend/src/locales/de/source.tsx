import type { SourceMessages } from '../en/source';

/** Die Texte der Seiten des veröffentlichten Codes, /source und jedes Projekt darunter, auf Deutsch. */
export const source: SourceMessages = {
  shell: {
    section: 'Open Source',
    home: 'GenHTTP Lambda, zur Startseite',
  },

  lambda: {
    label: 'Was ist ein Lambda?',
    text: 'Eine Web-App auf GenHTTP Lambda: Jemand sagt, was er möchte, ein KI-Agent schreibt die App in C#, und nach wenigen Minuten ist sie unter einer eigenen Adresse online – jede Version bleibt erhalten, samt dem, was sie geändert hat.',
    build: 'Eigene App bauen',
  },

  catalog: {
    eyebrow: 'Open Source',
    title: 'Sehen Sie, wie die Apps hier gebaut sind',
    intro:
      'Lambdas, deren Besitzer ihren Code veröffentlicht haben: jede Version, was sie geändert hat, ihre Dokumentation und ihre Tests. Lesen Sie den Code hier, oder laden Sie ein Projekt herunter, das überall läuft, wo .NET läuft.',
    searchLabel: 'Projekte durchsuchen',
    searchPlaceholder: 'Nach Name oder Funktion suchen',
    orderLabel: 'Sortierung',
    orders: {
      stars: 'Meiste Sterne',
      updated: 'Zuletzt geändert',
      published: 'Zuletzt veröffentlicht',
    },
    counted: (total) => (total === 1 ? '1 Projekt' : `${total} Projekte`),
    failed: 'Die Projekte konnten nicht geladen werden.',
    loadingMore: 'Weitere werden geladen …',
    showMore: 'Mehr anzeigen',
    nothingTitle: 'Noch nichts veröffentlicht',
    nothing: (tab) => (
      <>
        Haben Sie etwas gebaut, von dem andere lernen könnten? Öffnen Sie das Kontrollzentrum Ihres Lambdas, wählen Sie{' '}
        {tab('Open Source')} und eine Lizenz – dann erscheint der Code hier.
      </>
    ),
    noMatchTitle: 'Keine Treffer',
    noMatch: (query) => `Kein veröffentlichtes Projekt erwähnt „${query}“.`,
    clear: 'Alle Projekte anzeigen',
    yoursTitle: 'Ihr eigenes veröffentlichen',
    yours: (tab) => (
      <>
        Öffnen Sie das Kontrollzentrum Ihres Lambdas und wählen Sie {tab('Open Source')}, oder bitten Sie den Agenten,
        der es gebaut hat, es zu veröffentlichen. Das kann nur, wer den Editor-Schlüssel hat, unter der Lizenz seiner
        Wahl – und was die App aufbewahrt, ihre Datensätze, Dateien und Schlüssel, gehört nie dazu.
      </>
    ),
    build: 'Selbst etwas bauen',
    online: 'Online',
    offline: 'Offline',
    changed: (ago) => `geändert ${ago}`,
    stars: (count) => (count === 1 ? '1 Stern' : `${count} Sterne`),
  },

  project: {
    loading: 'Der Quellcode lädt …',
    failed: 'Der Quellcode konnte nicht geladen werden.',
    missingTitle: 'Hier ist kein Quellcode veröffentlicht',
    missing: 'Vielleicht hat sein Besitzer ihn zurückgezogen, oder unter dieser Adresse gab es nie ein Lambda.',
    all: 'Alle Projekte',
    by: (name) => `von ${name}`,
    versions: (count) => (count === 1 ? '1 Version' : `${count} Versionen`),
    onlineAt: (address) => <>Online unter {address}</>,
    offline: 'Gerade offline',
    openApp: 'App öffnen',
    opens: (address) => `Öffnet ${address} in einem neuen Tab`,
    published: (ago) => `Veröffentlicht ${ago}`,
    changed: (ago) => `Geändert ${ago}`,
    picture: (name) => `So sieht ${name} aus`,
    tabsLabel: 'Was Sie lesen können',
    tabs: {
      code: 'Code',
      docs: 'Dokumentation',
      tests: 'Tests',
      changes: 'Änderungen',
    },
  },

  versions: {
    label: 'Version',
    choose: 'Andere Version lesen',
    newest: 'neueste',
    online: 'online',
    older: (version, ago, newest) =>
      `Sie lesen Version ${version}, gespeichert ${ago}. Die neueste ist Version ${newest}.`,
    toNewest: 'Neueste Version lesen',
    noChange: 'Keine Angabe, was sie geändert hat',
  },

  star: {
    star: 'Stern',
    add: 'Diesem Projekt einen Stern geben',
    remove: 'Ihren Stern zurücknehmen',
    count: (count) => (count === 1 ? '1 Stern' : `${count} Sterne`),
    failed: 'Der Stern konnte nicht gespeichert werden.',
  },
  clone: {
    button: 'Code',
    title: 'Mit git klonen',
    what: (oldest, newest) =>
      oldest === newest
        ? `Die Version ist der Commit auf main, getaggt als v${newest}.`
        : `Jede Version kommt als Commit auf main mit, getaggt von v${oldest} bis v${newest} – main ist die neueste.`,
    readOnly:
      'Nur lesbar. Um darauf aufzubauen, starten Sie ein eigenes Lambda und übernehmen Sie diese Dateien – AGENTS.md im Klon sagt, wie, und die Lizenz, was Sie damit dürfen.',
  },

  download: {
    title: (version) => `Version ${version} als Projekt`,
    what:
      'Ein .NET-10-Projekt mit Dockerfile, seiner Dokumentation, seinen Tests und seiner Lizenz. Was die App aufbewahrt – ihre Datensätze, die Dateien, die sie gespeichert hat, ihre Schlüssel –, gehört nicht dazu.',
    zip: 'ZIP herunterladen',
    preparing: 'Das Projekt wird vorbereitet …',
    slow: 'Beim ersten Download wird eine Version gepackt, während Sie warten.',
    failed: 'Das Projekt konnte nicht vorbereitet werden. Versuchen Sie es gleich noch einmal.',
    run: 'Ausführen',
    local: 'Mit dem .NET 10 SDK:',
    container: 'Oder in einem Container:',
    agent: 'Oder geben Sie den Ordner Ihrem Coding-Agenten und bauen Sie darauf auf – im Rahmen der Lizenz.',
    copy: 'Kopieren',
    copied: 'Kopiert',
  },

  tree: {
    label: 'Dateien',
    files: (count) => (count === 1 ? '1 Datei' : `${count} Dateien`),
    packing: 'Diese Version wird gepackt …',
    packingSlow: 'Eine Version wird gepackt, wenn sie zum ersten Mal gelesen wird. Bei einer großen dauert das einen Moment.',
    failed: 'Die Dateien dieser Version konnten nicht geladen werden.',
    legend: 'Was ist was',
    kinds: {
      code: 'Der eigene Code des Lambdas',
      asset: 'Was es ausliefert: Seiten, Scripts, Styles, Bilder – und seine Datenbank-Migrationen',
      docs: 'Was es ist und warum es so gebaut ist',
      tests: 'Wie es getestet wird',
      dev: 'Woraus sein Code oder seine Assets mit einem Build-Werkzeug gebaut werden',
      platform: 'Ersatz für die Plattform',
      project: 'Host, Build, Container und Lizenz',
    },
    short: {
      code: 'Code',
      asset: 'Ausgeliefert',
      docs: 'Doku',
      tests: 'Tests',
      dev: 'Build',
      platform: 'Plattform',
      project: 'Projekt',
    },
  },

  file: {
    loading: 'Lädt …',
    failed: 'Diese Datei konnte nicht geladen werden.',
    missing: (path) => `Diese Version enthält keine Datei ${path}.`,
    binary: 'Diese Datei ist kein Text.',
    tooLarge: 'Diese Datei ist zu lang, um sie hier anzuzeigen.',
    download: 'Herunterladen',
    raw: 'Raw',
    rawTitle: 'Die Datei unverändert öffnen',
    copy: 'Kopieren',
    copied: 'Kopiert',
    lines: (count) => (count === 1 ? '1 Zeile' : `${count} Zeilen`),
    plain: 'Ohne Farben angezeigt: Die Datei ist lang.',
    line: (line) => `Zeile ${line}`,
  },

  docs: {
    pages: 'Seiten',
    product: 'Worum es geht',
    decisions: 'Entscheidungen',
    loading: 'Lädt …',
    failed: 'Diese Seite konnte nicht geladen werden.',
    noneTitle: 'Über diese Version ist nichts geschrieben',
    none: 'Ihre Dokumentation stünde in docs/: was die App ist, für wen sie gedacht ist und warum sie so gebaut ist, wie sie ist.',
  },

  tests: {
    files: 'Scripts und Daten',
    noneTitle: 'Diese Version sagt nichts über ihre Tests',
    none: 'Wie sie getestet wird, stünde in tests/README.md, mit den Scripts, die dabei laufen, daneben.',
  },

  changes: {
    title: 'Alle Versionen, die neueste zuerst',
    intro: 'Eine Version ändert sich nie mehr, sobald sie gespeichert ist. Jede sagt in einer Zeile, was sie geändert hat.',
    agent: 'Von einem Agenten geschrieben',
    online: 'online',
    browse: 'Code lesen',
    noChange: 'Keine Angabe',
  },

  licenses: {
    MIT: 'Jeder darf den Code nutzen, ändern und weitergeben, für jeden Zweck, solange Lizenz und Copyright-Vermerk dabeibleiben.',
    'Apache-2.0': 'Wie MIT, dazu eine Patentlizenz von allen, die beigetragen haben; Änderungen müssen als solche gekennzeichnet sein.',
    'BSD-3-Clause': 'Wie MIT, und niemand darf mit den Namen der Autoren für das werben, was er daraus gemacht hat.',
    'MPL-2.0': 'Änderungen an diesen Dateien bleiben unter derselben Lizenz; die Dateien dürfen mit Code unter jeder anderen Lizenz kombiniert werden.',
    'GPL-3.0-or-later': 'Wer den Code weitergibt, ob geändert oder nicht, gibt auch seinen Quelltext unter derselben Lizenz weiter.',
    'AGPL-3.0-or-later': 'Wie die GPL – und eine geänderte Fassung anderen über das Netzwerk anzubieten, gilt als Weitergabe.',
    Unlicense: 'Gemeinfrei: Jeder darf ohne Bedingungen alles damit tun.',
  },

  kinds: {
    Permissive: 'Freizügig',
    Copyleft: 'Copyleft',
    PublicDomain: 'Gemeinfrei',
  },
};
