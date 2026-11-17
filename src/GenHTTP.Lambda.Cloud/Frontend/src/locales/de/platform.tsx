import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'KI-App-Builder, Hosting inklusive',
  title: 'Gebaut ist sie in einer Minute.',
  titleAccent: 'Alles andere ist schon eingebaut.',
  intro:
    'Beschreiben Sie unserem KI-Agenten eine App – oder Claude Code, Codex oder Cursor –, und sie ist in Minuten online, für Sie gehostet, mit einem Link zum Teilen. Alles, was sie nützlich hält, ist schon da: ihre Einträge, ihre Versionen und ihr Zweck, wer sie besucht, und eine eigene Domain, sobald sie ankommt.',
  build: 'Etwas bauen',
  ownAgent: 'Eigenen Agenten nutzen',
  free: 'Kostenlos. Ohne Anmeldung, ohne Kreditkarte.',
  heroAlt: 'Der Editor einer App zur Platzbuchung: online unter der eigenen Domain des Vereins, wie viele Personen sie heute genutzt haben und ihre letzte Änderung',
  example: 'Jedes Bild auf dieser Seite zeigt dieselbe Beispiel-App: die Platzbuchung eines Tennisvereins.',

  contents: 'Auf dieser Seite',

  makeTitle: 'Bauen und ändern',
  makeText: 'Sagen Sie, was Sie brauchen, und sagen Sie es immer wieder. Dafür muss niemand Code schreiben oder lesen.',

  behindTitle: 'Eine Datenbank, Versionsverlauf und Analyse für jede App',
  behindText:
    'Nützlich macht eine App alles rund um ihren Code – für alle da, die sie gemacht haben, nicht nur für Menschen, die Code lesen können.',

  yoursTitle: 'Zum Weiterführen',
  yoursText: 'Für Entwickler und für alle, die möchten, dass ihre App diese Seite überlebt.',

  features: {
    build: {
      name: 'Bauen',
      title: 'Beschreiben Sie sie, und sie ist online',
      text: 'Sagen Sie unserem Agenten mit eigenen Worten, was Sie brauchen – oder bitten Sie Claude Code, Codex oder Cursor darum. Wenige Minuten später läuft die App unter einer eigenen Adresse und lässt sich teilen. Ein Server, eine Datenbank oder ein Konto sind nicht einzurichten.',
      points: ['Unser Agent auf der Bauen-Seite oder Ihr eigener über MCP', 'Ein Link zum Teilen in wenigen Minuten', 'Kostenlos, ohne Anmeldung'],
      alt: 'Die Seite zur Platzbuchung: vier Plätze, Stunde für Stunde, mit den Namen der gebuchten Zeiten',
    },
    change: {
      name: 'Ändern',
      title: 'Fragen und ändern',
      text: 'Ihre Apps bleiben auf der Bauen-Seite aufgelistet, jede mit einem Gespräch. Fragen Sie, wie es einer App geht – wer sie besucht hat, was Leute eingegeben haben, ob etwas nicht stimmt –, oder sagen Sie, was anders sein soll, und der Agent antwortet oder ändert sie. Oder geben Sie ihren Editor-Link an Ihren eigenen Agenten.',
      points: ['Antworten zu Besuchern, Einträgen und Fehlern', 'Von selbst online, sobald es funktioniert, oder als Entwurf zum Ausprobieren aufgehoben', 'Der Code bleibt außer Sicht, solange Sie ihn nicht sehen wollen'],
      alt: 'Die Bauen-Seite: Der Verein hat gefragt, wie viele Plätze diese Woche gebucht waren, der Agent hat geantwortet, und die Trainerstunden sind in einem Entwurf zum Ausprobieren bereit',
    },
    drafts: {
      name: 'Entwürfe',
      title: 'Eine Änderung ausprobieren, bevor sie jemand sieht',
      text: 'Eine größere Änderung entsteht in einem Entwurf: die App, wie sie sein wird, unter einer eigenen Adresse, mit einer Kopie der Einträge zum Ausprobieren. Nichts, was dort passiert, berührt die echten Einträge. Ist sie richtig, stellt ein Klick sie online.',
      points: ['Eine eigene Adresse zum Ausprobieren und Zeigen', 'Testdaten: eine Kopie der Einträge, nicht die Einträge selbst', 'Online stellen, wenn es passt, oder verwerfen'],
      alt: 'Ein Entwurf der Buchungsseite, in dem die Trainerstunden violett angezeigt werden – in einer Kopie der Buchungen',
    },
    history: {
      name: 'Verlauf',
      title: 'Jede Änderung eine Version, mit einem Weg zurück',
      text: 'Jede Änderung wird eine neue Version, beschrieben durch das, was sie geändert hat. Geht eine schief, ist die Version davor einen Klick entfernt – unter derselben Adresse, mit denselben Einträgen.',
      points: ['Was gewünscht war und was sich geändert hat', 'Mit einem Klick zurück zu einer früheren Version', 'Die Einträge gehören allen Versionen gemeinsam, deshalb geht beim Zurückgehen keiner verloren'],
      alt: 'Der Verlauf der App: ihre Änderungen untereinander, die neueste online',
    },
    about: {
      name: 'Über die App',
      title: 'Sie weiß, was sie ist',
      text: 'Mit jeder Änderung schreibt der Agent auf, wofür die App da ist, wer sie nutzt und warum sie so funktioniert. Sie lesen es in einfachen Worten, und der nächste Agent liest es, bevor er etwas ändert – so baut eine Änderung auf dem auf, was da ist, statt zu raten.',
      points: ['Was sie tut und für wen, in einfachen Worten', 'Die Entscheidungen dahinter, für alle, die sie als Nächstes ändern', 'Wie sie getestet wird, mit jeder Version aufbewahrt'],
      alt: 'Über Ihre App: die Platzbuchung in einfachen Worten beschrieben, wer sie nutzt und was sie tut',
    },
    records: {
      name: 'Einträge',
      title: 'Einträge in einer eigenen Datenbank',
      text: 'Eine App, die etwas aufbewahrt – Buchungen, Anmeldungen, Punktestände –, hält es in einer eigenen Datenbank, und es bleibt erhalten, wenn sich die App ändert. Lesen Sie die Einträge Tabelle für Tabelle im Editor, neben den Dateien, die sie gespeichert hat, etwa der Wochenliste für das Schwarze Brett.',
      points: ['Eine Datenbank für jede App, die eine braucht', 'Jede Tabelle, im Editor lesbar', 'Die Dateien, die die App speichert, daneben aufgelistet'],
      alt: 'Die Einträge der App: ihre Buchungen und ihre Mitglieder, Tabelle für Tabelle gelesen',
    },
    keys: {
      name: 'Schlüssel und Passwörter',
      title: 'Schlüssel und Passwörter bleiben geheim',
      text: 'Braucht eine App ein Passwort oder den Schlüssel zu einem anderen Dienst, fragt der Agent nur mit dem Namen danach. Den Wert tragen Sie im Editor ein, und von da an sieht ihn niemand mehr – weder Sie noch der Agent. Nur die App liest ihn.',
      points: ['Der Agent sieht den Wert nie', 'Verschlüsselt aufbewahrt, nie wieder angezeigt', 'Jederzeit ersetzbar'],
      alt: 'Schlüssel und Passwörter: Das Passwort der Anmeldung ist gesetzt, sein Wert bleibt verborgen',
    },
    analytics: {
      name: 'Analyse',
      title: 'Sehen, wer sie besucht – ohne Cookies',
      text: 'Schalten Sie die Analyse ein und sehen Sie, wie viele Personen die App geöffnet haben, woher sie kamen und auf welchen Geräten – gezählt auf unserem Server, ohne Cookies und ohne Skript in Ihren Seiten.',
      points: ['Besucher, Seitenaufrufe und Herkunft', 'Keine Cookies und nichts, was einen Besucher benennt', 'Aus, bis Sie sie einschalten'],
      alt: 'Analyse: die Besucher der letzten dreißig Tage, Tag für Tag',
    },
    domain: {
      name: 'Domain',
      title: 'Eine eigene Domain',
      text: 'Jede App beginnt unter einer Adresse von uns. Findet sie Anklang, kann sie auf eine Domain umziehen, die Ihnen gehört: dieselbe App und derselbe Editor, unter einer Adresse, die man sich merkt. Links zur alten Adresse funktionieren weiter – sie führen zur neuen.',
      points: ['Für Premium-Apps: schreiben Sie uns', 'Dieselbe App, derselbe Editor, dieselben Einträge', 'Die alte Adresse leitet Besucher weiter'],
      alt: 'Die Adresse der App zieht von ihrer Adresse auf genhttp.run auf die eigene Domain des Vereins um',
    },
    code: {
      name: 'Code und git',
      title: 'Der Code, wann immer Sie ihn wollen',
      text: 'Schalten Sie den Editor auf die vollständige Ansicht, und der Code jeder Version ist da, mit Protokollen und Zahlen. Klonen Sie die App mit git und pushen Sie eine Änderung, oder laden Sie sie als .NET-Projekt herunter, das überall läuft. Es ist Ihr Code.',
      points: ['Jede Datei, von Version zu Version verglichen', 'Git: Ein Commit auf main ist eine Version, ein Branch ist ein Entwurf', 'Ein Download, der eigenständig läuft, samt Einträgen'],
      alt: 'Die vollständige Ansicht des Editors: die Dateien der App und ihr Code',
    },
    publish: {
      name: 'Showcase und Quellcode',
      title: 'Zeigen oder den Code veröffentlichen',
      text: 'Nehmen Sie die App in den Showcase auf, damit andere sie finden, oder veröffentlichen Sie ihren Code unter einer Open-Source-Lizenz, damit ihn alle lesen, mit einem Stern versehen und herunterladen können. Beides ist Ihre Entscheidung, und keines teilt ihre Einträge.',
      points: ['Im Showcase, mit Bild und Beschreibung', 'Ihr Code unter MIT oder einer anderen Lizenz', 'Nie ihre Einträge, Schlüssel oder Besucher'],
      alt: 'Der veröffentlichte Code der Platzbuchung: ihre Dateien, ihre Dokumentation und ihre Versionen unter der MIT-Lizenz',
    },
  },

  draftBadge: 'Entwurf',
  domainOwn: 'Premium',
  domainSubject: 'Eine Domain für meine App',
  domainAsk: 'Nach einer Domain fragen',
  zoom: 'Größer anzeigen',
  close: 'Schließen',

  closeTitle: 'Kostenlos zum Ausprobieren.',
  closeAccent: 'Ohne Anmeldung.',
  closeBuild: 'Etwas bauen',
  closeShip: 'Veröffentlichen, was Sie gebaut haben',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda ist Open Source. Nutzen Sie es hier oder {selfHosting('betreiben Sie es auf einem eigenen Server')} – der Code liegt auf{' '}
      {github('GitHub')}.
    </>
  ),
};
