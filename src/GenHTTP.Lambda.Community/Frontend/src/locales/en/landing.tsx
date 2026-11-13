/**
 * The front page of an installation somebody runs for themselves: what the
 * platform does and how to start, for developers - technical, but without
 * jargon where a plain word does, and nothing it does not do.
 */
export const landing = {
  eyebrow: 'GenHTTP Lambda · Community edition',
  headline: 'The platform your agents build apps on.',
  headlineAccent: 'Running on your own server.',
  intro:
    'Describe an app and the build agent writes it - or let Claude Code, Codex or Cursor do it over MCP. Every app is a C# GenHTTP handler, compiled on this server and online in seconds, with a database, keys and passwords, versions and git.',
  describe: 'Describe an app',
  connect: 'Connect your agent',
  write: 'Write the code yourself',

  buildTitle: 'Describe it',
  buildText: 'A sentence is enough. The build agent writes the app, puts it online and gives you the link to change it later.',
  noAgent: 'No build agent is connected to this installation. Connect your own agent below - it does the same.',

  connectTitle: 'Connect your agent',
  connectText:
    'Your coding agent gets the platform as tools: create an app, write and check its code, try a change in a draft with a copy of the data, put it online and read its logs.',
  connectThen: 'Then ask it for something, for example: “Build a guestbook, with a page where I approve the entries.”',

  featuresTitle: 'What every app gets',
  features: [
    { title: 'An address', text: 'Online below /lambda/ on this server as soon as it compiles - or at a domain of its own.' },
    { title: 'Records', text: 'A SQLite database of its own, its tables made by migrations, read and written with Entity Framework Core.' },
    { title: 'Files', text: 'A folder for what the app saves: uploads, exports, anything that is not a record.' },
    { title: 'Keys and passwords', text: 'Kept sealed on the server. The agent says which it needs, you enter them, and nobody reads them back.' },
    { title: 'Drafts and versions', text: 'A change is tried in a draft with a copy of the data, then put online. Every version is kept, one click away from going back.' },
    { title: 'Git', text: 'Every app is a repository: clone it, push a commit to put it online, push a branch to start a draft.' },
    { title: 'Visitors', text: 'Page views, where visitors came from and their countries - counted on the server, without cookies, once you switch it on.' },
    { title: 'Your code', text: 'Download any app as a plain .NET project and run it anywhere, or publish its source for others to read.' },
  ],

  runsTitle: 'How it runs',
  runs: [
    { title: 'One container', text: 'The server, the editor, the API and every app in one process, and everything they keep in one folder.' },
    {
      title: 'By path or by host',
      text: 'Apps answer below /lambda/ on this address. Give the installation a domain for them, and each gets a subdomain of its own instead.',
    },
    {
      title: 'Code you trust',
      text: 'Apps are compiled into the server, and their code is checked for what they must not do - but they share its process. Run what you would run yourself.',
    },
    { title: 'Open source', text: 'MIT licensed: read it, change it, run it wherever you like.' },
  ],
  repository: 'The source on GitHub',

  aroundTitle: 'Look around',
  around: {
    demos: { title: 'Start from a demo', text: 'An API over records, a login, live updates, a game: each a finished app to copy and change.' },
    showcase: { title: 'Showcase', text: 'The apps their owners chose to show here.' },
    guide: { title: 'Guide', text: 'What an app can do and how, in the order you need it.' },
    api: { title: 'REST API', text: 'The same as the tools, over HTTP, described in OpenAPI.' },
    sources: { title: 'Published sources', text: 'Apps whose owners published their code, to read and download.' },
    admin: { title: 'Administration', text: 'The apps, the limits, the blocks and the log, behind the admin token.' },
  },
};
