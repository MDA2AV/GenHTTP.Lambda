import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Enterprise',
  title: 'Kostenlos testen, eigenständig betreiben',
  intro:
    'Alle Funktionen hier sind kostenlos und ohne Konto nutzbar. Benötigt Ihr Unternehmen Anwendungen, die dauerhaft online bleiben und hinter einer eigenen Anmeldung liegen, erhalten Sie eine eigene Installation – in der Cloud oder im eigenen Rechenzentrum.',

  free: 'Free',
  freeTagline: 'Zum Ausprobieren',
  forever: 'dauerhaft',
  buildOne: 'Anwendung erstellen',
  freeFeatures: (offline, removed) => [
    'Unbegrenzte Anzahl an Lambdas, ohne Konto',
    'Integrierter Agent oder eigener Agent über MCP',
    'Online, solange die Anwendung genutzt wird',
    `Offline nach ${offline} Tagen ohne Aufrufe, entfernt nach ${removed} Tagen`,
    'Bereitstellung unter einem Pfad des gemeinsamen Hosts',
  ],
  freeNote: 'Keine Kreditkarte, keine Registrierung. Erstellen Sie ein Lambda – es gehört Ihnen.',

  name: 'Enterprise',
  tagline: 'Für Unternehmen mit eigener Instanz',
  perUser: 'pro Nutzer und Monat',
  contact: 'Kontakt aufnehmen',
  features: [
    'Eigene Instanz, in der Cloud oder im eigenen Rechenzentrum',
    'Ein einziger Dienst betreibt alle Anwendungen',
    'Anmeldung über Ihr eigenes SSO',
    'Ihre Governance- und Compliance-Vorgaben integriert',
    'Anwendungen bleiben dauerhaft online und werden nie entfernt',
    'Eigene Agenten über MCP',
    'Priorisierter Support',
  ],
  users: (count) => <>{count} Nutzer</>,
  perMonth: ' / Monat',
  price: (amount) => `${amount} $`,
  perUserPrice: (amount) => `${amount} $ pro Nutzer und Monat`,

  compareTitle: 'Die Tarife im Vergleich',
  compareText:
    'Beide Tarife basieren auf derselben Plattform. Sie unterscheiden sich darin, wie lange Ihre Anwendung vorgehalten wird und wo sie betrieben wird.',
  included: 'Enthalten',
  notIncluded: 'Nicht enthalten',
  groups: (offline, removed) => [
    {
      title: 'Entwicklung',
      rows: [
        ['Lambdas', 'Unbegrenzt', 'Unbegrenzt'],
        ['Integrierter Agent', true, false],
        ['Eigener Agent über MCP', true, true],
        ['Editor, Versionen und Logs', true, true],
        ['Showcase', true, 'Eigener'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Offline bei Nichtnutzung', `Nach ${offline} Tagen`, 'Nie'],
        ['Entfernt bei Nichtnutzung', `Nach ${removed} Tagen`, 'Nie'],
        ['Instanz', 'Geteilt', 'Eigene'],
        ['Betrieb', 'In unserer Cloud', 'Cloud oder eigenes Rechenzentrum'],
        ['Ihr Betriebsaufwand', 'Keiner', 'Ein einzelner Dienst'],
        ['Eigene Domains', false, true],
      ],
    },
    {
      title: 'Kontrolle',
      rows: [
        ['Anmeldung', 'Nicht erforderlich', 'Ihr eigenes SSO'],
        ['Ihre Governance- und Compliance-Vorgaben für Agenten', false, true],
        ['Administrationskonsole', false, true],
        ['Daten getrennt von anderen Kunden', false, true],
        ['Support', 'Community', 'Priorisiert'],
      ],
    },
  ],

  questionsTitle: 'Häufige Fragen',
  questions: [
    [
      'Benötige ich für den Einstieg ein Konto?',
      'Nein. Für ein kostenloses Lambda benötigen Sie lediglich den Editor-Link, den Sie beim Erstellen erhalten.',
    ],
    [
      'Wer zählt bei Enterprise als Nutzer?',
      'Alle Personen, die sich über Ihr SSO anmelden – sei es, um im Editor zu entwickeln oder um eine auf Ihrer Installation bereitgestellte Anwendung zu nutzen. Wer eine Anwendung ohne Anmeldung aufruft, wird nicht gezählt.',
    ],
    [
      'Ist der integrierte Agent in Enterprise enthalten?',
      'Nein. Ihr Team verwendet einen eigenen Agenten – Claude, Claude Code oder einen anderen MCP-fähigen Client – und verbindet ihn mit Ihrer Installation, im Rahmen Ihres bestehenden Vertrags mit dem jeweiligen Anbieter.',
    ],
    [
      'Wie erhalten Agenten unsere Compliance-Vorgaben?',
      'Wir integrieren Ihre Governance- und Compliance-Vorgaben in die Informationen, die die Plattform Agenten über MCP bereitstellt. Jeder verbundene Agent erhält sie beim Schreiben von Code, sodass die Anwendungen Ihren Vorgaben entsprechen, ohne dass alle Beteiligten diese im Detail kennen müssen.',
    ],
    [
      'Benötigen wir Kubernetes oder einen Cluster?',
      'Nein. Alle Anwendungen laufen innerhalb eines einzigen Dienstes. Es müssen weder Pods verteilt noch einzelne Anwendungen orchestriert werden – der Betrieb der Installation umfasst genau diesen einen Dienst.',
    ],
    [
      'Wo wird eine Enterprise-Installation betrieben?',
      'Nach Ihrer Wahl: Wir hosten sie in unserer Cloud, oder sie läuft in Ihrem Cloud-Konto bzw. auf Ihren eigenen Servern – überall dort, wo Container betrieben werden können. In beiden Fällen unterstützen wir Sie bei der Einrichtung und halten die Installation aktuell.',
    ],
  ],
  anythingElse: (mail) => <>Weitere Fragen? Schreiben Sie an {mail}.</>,
};
