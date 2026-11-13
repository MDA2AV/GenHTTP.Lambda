import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community edition',
  headline: 'Het platform waarop je agents apps bouwen.',
  headlineAccent: 'Op je eigen server.',
  intro:
    'Beschrijf een app en de build-agent schrijft hem - of laat Claude Code, Codex of Cursor het doen via MCP. Elke app is een C#-handler voor GenHTTP, op deze server gecompileerd en binnen enkele seconden online, met een database, sleutels en wachtwoorden, versies en git.',
  describe: 'Beschrijf een app',
  connect: 'Koppel je agent',
  write: 'Schrijf de code zelf',

  buildTitle: 'Beschrijf het',
  buildText: 'Eén zin is genoeg. De build-agent schrijft de app, zet hem online en geeft je de link om hem later aan te passen.',
  noAgent: 'Aan deze installatie is geen build-agent gekoppeld. Koppel hieronder je eigen agent - die doet hetzelfde.',

  connectTitle: 'Koppel je agent',
  connectText:
    'Je coding-agent krijgt het platform als tools: een app maken, de code schrijven en controleren, een wijziging uitproberen in een concept met een kopie van de data, online zetten en de logs lezen.',
  connectThen: 'Vraag hem daarna om iets, bijvoorbeeld: “Bouw een gastenboek, met een pagina waar ik de berichten goedkeur.”',

  featuresTitle: 'Wat elke app krijgt',
  features: [
    { title: 'Een adres', text: 'Online onder /lambda/ op deze server zodra de code compileert - of op een eigen domein.' },
    { title: 'Items', text: 'Een eigen SQLite-database, met tabellen die door migraties worden gemaakt, gelezen en geschreven met Entity Framework Core.' },
    { title: 'Bestanden', text: 'Een map voor wat de app opslaat: uploads, exports, alles wat geen item is.' },
    { title: 'Sleutels en wachtwoorden', text: 'Verzegeld bewaard op de server. De agent zegt welke hij nodig heeft, jij voert ze in en niemand leest ze terug.' },
    { title: 'Concepten en versies', text: 'Een wijziging probeer je uit in een concept met een kopie van de data en zet je daarna online. Elke versie blijft bewaard, met één klik terug te zetten.' },
    { title: 'Git', text: 'Elke app is een repository: kloon hem, push een commit om hem online te zetten, push een branch om een concept te starten.' },
    { title: 'Bezoekers', text: 'Paginaweergaven, waar bezoekers vandaan kwamen en hun landen - op de server geteld, zonder cookies, zodra je het aanzet.' },
    { title: 'Jouw code', text: 'Download elke app als gewoon .NET-project en draai hem overal, of publiceer de broncode zodat anderen hem kunnen lezen.' },
  ],

  runsTitle: 'Hoe het draait',
  runs: [
    { title: 'Eén container', text: 'De server, de editor, de API en elke app in één proces, en alles wat ze bewaren in één map.' },
    {
      title: 'Op pad of op host',
      text: 'Apps antwoorden onder /lambda/ op dit adres. Geef de installatie een domein voor ze, dan krijgt elke app in plaats daarvan een eigen subdomein.',
    },
    {
      title: 'Code die je vertrouwt',
      text: 'Apps worden in de server gecompileerd en hun code wordt gecontroleerd op wat ze niet mogen doen - maar ze delen zijn proces. Draai wat je ook zelf zou draaien.',
    },
    { title: 'Open source', text: 'MIT-licentie: lees het, pas het aan, draai het waar je wilt.' },
  ],
  repository: 'De broncode op GitHub',

  aroundTitle: 'Kijk rond',
  around: {
    demos: { title: 'Begin met een demo', text: 'Een API over items, een login, live updates, een spel: elk een complete app om te kopiëren en aan te passen.' },
    showcase: { title: 'Showcase', text: 'De apps die hun eigenaren hier willen laten zien.' },
    guide: { title: 'Gids', text: 'Wat een app kan en hoe, in de volgorde waarin je het nodig hebt.' },
    api: { title: 'REST API', text: 'Hetzelfde als de tools, via HTTP, beschreven in OpenAPI.' },
    sources: { title: 'Gepubliceerde broncode', text: 'Apps waarvan de eigenaren hun code hebben gepubliceerd, om te lezen en te downloaden.' },
    admin: { title: 'Beheer', text: 'De apps, de limieten, de blokkades en het logboek, achter het admin-token.' },
  },
};
