import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Enterprise',
  title: 'Kostenlos testen, selbst betreiben',
  intro:
    'Hier ist alles kostenlos und ohne Konto. Braucht Ihr Team Apps, die dauerhaft online bleiben und hinter dem eigenen Login liegen? Dann bekommen Sie eine eigene Installation – in der Cloud oder On-Premises.',

  free: 'Free',
  freeTagline: 'Zum Ausprobieren',
  forever: 'für immer',
  buildOne: 'App bauen',
  freeFeatures: (offline, removed) => [
    'Unbegrenzt viele Lambdas, ohne Konto',
    'Unser Agent oder Ihr eigener per MCP',
    'Online, solange es genutzt wird',
    `Offline nach ${offline} Tagen ohne Besuche, gelöscht nach ${removed} Tagen`,
    'Unter einer Subdomain der gemeinsamen Domain',
  ],
  freeNote: 'Keine Kreditkarte, keine Registrierung. Lambda erstellen – und es gehört Ihnen.',

  name: 'Enterprise',
  tagline: 'Für Teams mit eigener Instanz',
  perUser: 'pro Nutzer / Monat',
  contact: 'Kontakt aufnehmen',
  features: [
    'Eigene Instanz, Cloud oder On-Premises',
    'Ein einziger Dienst für alle Apps',
    'Login über Ihr eigenes SSO',
    'Ihre Governance- und Compliance-Regeln eingebaut',
    'Apps bleiben dauerhaft online, nichts wird gelöscht',
    'Eigener Agent per MCP',
    'Priorisierter Support',
  ],
  users: (count) => <>{count} Nutzer</>,
  perMonth: ' / Monat',
  price: (amount) => `${amount} $`,
  perUserPrice: (amount) => `${amount} $ pro Nutzer / Monat`,

  compareTitle: 'Die Tarife im Vergleich',
  compareText: 'Beide laufen auf derselben Plattform. Der Unterschied: wie lange Ihre App bleibt – und wo sie läuft.',
  included: 'Enthalten',
  notIncluded: 'Nicht enthalten',
  groups: (offline, removed) => [
    {
      title: 'Bauen',
      rows: [
        ['Lambdas', 'Unbegrenzt', 'Unbegrenzt'],
        ['Eingebauter Agent', true, false],
        ['Eigener Agent per MCP', true, true],
        ['Editor, Versionen und Logs', true, true],
        ['Showcase', true, 'Eigener'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Offline, wenn ungenutzt', `Nach ${offline} Tagen`, 'Nie'],
        ['Gelöscht, wenn ungenutzt', `Nach ${removed} Tagen`, 'Nie'],
        ['Instanz', 'Geteilt', 'Eigene'],
        ['Läuft', 'In unserer Cloud', 'Cloud oder On-Premises'],
        ['Was Sie betreiben', 'Nichts', 'Einen einzigen Dienst'],
        ['Eigene Domains', false, true],
      ],
    },
    {
      title: 'Kontrolle',
      rows: [
        ['Login', 'Nicht nötig', 'Ihr eigenes SSO'],
        ['Ihre Governance- und Compliance-Regeln für Agenten', false, true],
        ['Admin-Konsole', false, true],
        ['Daten getrennt von anderen Kunden', false, true],
        ['Support', 'Community', 'Priorisiert'],
      ],
    },
  ],

  questionsTitle: 'Fragen',
  questions: [
    [
      'Brauche ich zum Start ein Konto?',
      'Nein. Ein kostenloses Lambda braucht nur den Editor-Link, den Sie beim Erstellen bekommen.',
    ],
    [
      'Wer zählt bei Enterprise als Nutzer?',
      'Jeder, der sich über Ihr SSO anmeldet – egal ob er im Editor baut oder eine App auf Ihrer Installation nutzt. Wer eine App ohne Login aufruft, zählt nicht.',
    ],
    [
      'Ist der eingebaute Agent bei Enterprise dabei?',
      'Nein. Ihr Team bringt seinen eigenen Agenten mit – Claude, Claude Code oder einen anderen MCP-Client – und verbindet ihn mit Ihrer Installation. Dafür gilt der Tarif, den Sie beim jeweiligen Anbieter schon haben.',
    ],
    [
      'Wie lernen Agenten unsere Compliance-Regeln?',
      'Wir bauen Ihre Governance- und Compliance-Regeln in das ein, was die Plattform Agenten per MCP mitteilt. Jeder Agent Ihres Teams bekommt sie beim Schreiben von Code. So halten sich die Apps an Ihre Regeln, ohne dass alle sie auswendig kennen müssen.',
    ],
    [
      'Brauchen wir Kubernetes oder einen Cluster?',
      'Nein. Alle Apps laufen in einem einzigen Dienst. Sie müssen keine Pods verteilen und nichts pro App orchestrieren. Wer die Installation betreibt, betreibt nur diesen einen Dienst.',
    ],
    [
      'Wo läuft eine Enterprise-Installation?',
      'Wo Sie möchten. Wir hosten sie für Sie in unserer Cloud, oder sie läuft in Ihrem Cloud-Konto oder auf Ihren eigenen Servern – überall, wo Container laufen. In jedem Fall helfen wir bei der Einrichtung und halten sie aktuell.',
    ],
  ],
  anythingElse: (mail) => <>Noch etwas? Schreiben Sie an {mail}.</>,
};
