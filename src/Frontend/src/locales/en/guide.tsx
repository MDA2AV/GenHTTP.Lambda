import type { ReactNode } from 'react';

import type { Kit } from '../kit';

type Text = (k: Kit) => ReactNode;

export const guide = {
  title: 'How this works',
  intro:
    'You write a snippet of C#. Whatever it returns is hosted at a public address, over HTTPS, in a few seconds. This is the whole of it, in the order you will meet it.',
  contents: 'Contents',

  parts: {
    what: 'What a lambda is',
    first: 'Your first one',
    editor: 'The control center',
    why: 'Saying why',
    written: 'Documentation and tests',
    features: 'Changing it safely',
    files: 'More than one file',
    page: 'Serving a page',
    spa: 'A front end, step by step',
    built: 'What it is built from',
    storage: 'The two places files live',
    database: 'Keeping records',
    keeping: 'Keeping files',
    secrets: 'Keys and passwords',
    sockets: 'Websockets',
    limits: 'What it will not let you do',
    away: 'Taking it away',
    git: 'Working on it with git',
    open: 'Publishing the code',
    agents: 'Letting an agent do it',
  },

  what: [
    (k) => (
      <>
        A lambda is a snippet that returns a GenHTTP handler. The platform compiles it, loads it, and mounts
        whatever it returned under your own address. There is no project, no build file and no {k.code('using')}{' '}
        statement. Every GenHTTP module is already imported for you.
      </>
    ),
    (k) => (
      <>
        That is a complete lambda. Deployed at {k.code('/lambda/your-key/')}, it answers every request with the
        word hello.
      </>
    ),
  ] as Text[],
  whatAside: ((k) => (
    <>
      The snippet is {k.em('statements')}, not a class. The last thing it does is return something that can serve
      requests: a handler, or a builder for one.
    </>
  )) as Text,

  first: [
    (k) => (
      <>
        Press {k.b('Create my lambda')}. You get a public address and an editor key. The key is the only way back
        in, so keep it. Nobody can recover it for you.
      </>
    ),
    () => (
      <>
        You land in its control center, with a small REST service already written as the first version. It is only
        a starting point.
      </>
    ),
    (k) => (
      <>
        Hand the editor key to an agent and tell it what to build - it writes new versions through{' '}
        {k.link('/#agents', 'MCP')}. Or open {k.b('Code')} and write it yourself: {k.b('Check')} compiles without
        storing anything and tells you what the compiler thinks, file and line.
      </>
    ),
    (k) => (
      <>
        Press {k.b('Deploy')}. Now it is online. Nothing is reachable before that, and deploying again extends how
        long it stays.
      </>
    ),
  ] as Text[],

  editor: ((k) => (
    <>
      The editor link opens a control center rather than a text box: most of the code here is written by agents, so
      the first thing on the screen is how your lambda is doing. The sidebar holds the lambda - whether it is online,
      its address, and a button when a newer version is waiting to go online - and its sections. Anything done
      rarely, like changing the address or deleting it, is behind the {k.b('⋯')} menu there.
    </>
  )) as Text,
  bits: [
    ['Overview', () => <>What the app is, whether it is online, how many requests it had today and how many failed, the latest change, and how much room is left.</>],
    ['Documentation', () => <>What the app is, who it is for and why, and why it is built the way it is - written by agents, kept with each version.</>],
    [
      'Change',
      (k) => (
        <>
          Say what should be different, and the agent on this server does it while you watch. It tries the change on a
          draft - a copy with an address of its own - and puts it online once it works. Switch off{' '}
          {k.b('Put it online when it is done')} to try the draft yourself first.
          It only works on your app: a request that is not about it, or that is meant to do harm, is turned down, and it says why.
        </>
      ),
    ],
    ['Drafts', () => <>Changes being tried before they go online, each at an address of its own and on test data of its own. Opened, a draft has its own code, test data and logs. The section is there once there is a draft.</>],
    ['Files', () => <>The files of a version: its code and assets, the program itself. A lock or a globe says whether the public can reach them.</>],
    ['Data', () => <>What the lambda keeps while it runs, shared by every version: the database, the workspace and the secrets, each a pill of its own. Look into the tables and files, upload files, set secrets, or switch a kind on or off. The simple view shows it once the app keeps something.</>],
    ['Versions', () => <>What each version changed and what was asked for, and the difference to the one before. Deploy or roll back from here, or start a draft from any of them.</>],
    ['Deployments', () => <>What was online when, and what took it down.</>],
    ['Stats', () => <>Requests, failures, response times and the most asked-for paths, over the last hour or day.</>],
    ['Logs', () => <>Its requests, what it printed, and the stack trace of anything that went wrong, as it happens.</>],
    [
      'Code',
      (k) => (
        <>
          Writing it by hand. {k.b('Check')} compiles, {k.b('Save')} makes a version, {k.b('Deploy')} puts it online.
          In a draft, {k.b('Save')} keeps it in the draft and shows it at the draft's address. {k.code('Ctrl-S')}{' '}
          saves; {k.code('F12')} goes to a declaration.
        </>
      ),
    ],
    ['Tests', () => <>How the app is tested automatically, with the scripts and test data for it. In the full view only.</>],
    ['Build', () => <>What the code or the assets are built from where a build tool makes them, kept with each version, to read rather than to edit. In the full view, once a version keeps it.</>],
  ] as [string, Text][],
  sections: ((k) => (
    <>
      Every section works the same way: its title, an {k.b('ⓘ')} that explains it, its actions on the right, and -
      where it has more than one view - a row of pills underneath. The pills of the code are its files. The full
      view gathers the sections in groups: how people find it, where a change is made, the program and its data,
      and how it runs.
    </>
  )) as Text,
  editorAside:
    'The traffic and the log are held in memory, for watching rather than keeping: a restart of the server begins them again. Versions and the deployment history are stored.',

  why: ((k) => (
    <>
      A version is the code, and optionally two notes about it: {k.b('the specification')}, what the user wants and
      why, in their words where possible, and {k.b('the change')}, one line on what the version does. They are shown
      beside the diff in the version history, so the {k.em('why')} survives next to the {k.em('what')} - for you,
      and for the next agent that reads the history before changing anything.
    </>
  )) as Text,
  whySample: {
    specification: 'A guest book people can sign; entries must survive a restart',
    change: 'Keeps entries in the database so they survive a restart',
  },
  why2: ((k) => (
    <>
      Agents pass the same two fields to {k.code('write_code')}. In {k.b('Code')}, saving asks for the change. Both
      are optional; a long specification is cut at 4000 characters and a change at 500 rather than refused. A
      draft keeps its own two, and the version it becomes takes them over.
    </>
  )) as Text,

  written: ((k) => (
    <>
      Every version keeps what is written about it beside its program: its {k.b('documentation')} - what the app is,
      who it is for and why, and why it is built the way it is - and its {k.b('tests')}: how to check automatically
      that it works, with the scripts and test data for it. Agents write them with a new lambda and keep them up to
      date with every change. The next agent to change the lambda reads them first, so it knows what the app is for
      and what has to keep working - which the code alone does not say.
    </>
  )) as Text,
  writtenFiles: [
    ['.lambda/docs/product.md', 'what the app is, who it is for, what people do with it and why'],
    ['.lambda/docs/decisions.md', 'the technical decisions, and why they were made'],
    ['.lambda/tests/README.md', 'how the app is tested automatically, and how to run the tests'],
    ['.lambda/tests/…', 'the scripts and test data the tests use'],
  ] as [string, string][],
  written2: ((k) => (
    <>
      They are files of the version like any other, in the folder {k.code('.lambda')}: the history shows what a
      version changed in them, rolling back brings back the documentation that was true of that version, and a draft
      has a copy of its own that goes online with it. They are never compiled and never served, and count towards
      what the assets of a version may come to.
    </>
  )) as Text,
  written3: ((k) => (
    <>
      In the control center, {k.b('Documentation')} shows the pages to read, and {k.b('Tests')} how the app is tested
      and the files beside it; the version is picked as it is for its files. A page can be edited there too, which
      saves the next version. The simple view calls the documentation {k.b('About')} and shows only what the app is
      for - to correct it, tell the agent.
    </>
  )) as Text,
  writtenAside:
    'They are written in the language you use with the agent, for whoever changes the app next - a person or an agent. Not a copy of the code: what it is for, and why.',

  features: ((k) => (
    <>
      A version never changes once it is saved - which is what makes every one worth keeping: any of them can be
      compared with, and put back online exactly as it was. To change a lambda that people use, try the change in
      a {k.b('draft')} first.
    </>
  )) as Text,
  featureSteps: [
    (k) => (
      <>
        Start it from any version under {k.b('Versions')}, or let the agent start one. It is a copy of that version's
        code, assets, documentation and tests, and of the lambda's data.
      </>
    ),
    (k) => (
      <>
        Change it as often as it takes - in {k.b('Code')}, or by asking the agent. Its preview answers at an address
        of its own, {k.code('/features/…/')}, against test data of its own. Visitors of the lambda see none of it,
        and nothing it writes reaches the lambda's data.
      </>
    ),
    (k) => (
      <>
        {k.b('Put online')} once it is right: it becomes the next version, with its notes, and goes online. The draft
        goes with it - its preview and its test data.
      </>
    ),
  ] as Text[],
  featureSample: 'Leaderboard',
  featuresAside: (() => (
    <>
      Several drafts can be worked on at once. Only one that is up to date with the newest version can go online,
      so that it never undoes a version saved after the draft began. When another went online first, bring its
      changes in - or ask the agent to - and mark the draft as up to date. Nothing goes online on its own; that is
      deliberate. The API calls a draft a feature, and putting it online a merge.
    </>
  )) as Text,

  files: ((k) => (
    <>
      Types do not have to sit underneath the code that uses them. In {k.b('Code')}, press {k.b('+')} beside the
      files and it is compiled beside the snippet, in the same namespace, so nothing has to be imported to be
      reached. A name with no extension is taken to be C#.
    </>
  )) as Text,

  page: 'There are two ways to serve a page, and one more for what people upload beside it.',
  inlineTitle: 'One page, written inline',
  inline: 'Fine for something small. The page is part of the snippet.',
  folderTitle: 'A folder of real files',
  folder:
    'What you want for anything with a stylesheet and a script. The files are added the same way a C# file is, and served exactly as written. Nothing compiles them.',
  workspaceTitle: 'Uploaded files, from the data',
  workspace:
    'For what people upload or the lambda creates - pictures, documents - served beside the app. Not for the pages of the app itself: those belong in a folder of files, where they are versioned with the code that needs them.',

  spa: ((k) => (
    <>
      The second of those, in full. Every demo serves its page this way from a folder called {k.code('web')} - open{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} to read one. Demos are read only; their editor key is their name.
    </>
  )) as Text,
  spaSteps: [
    (k) => (
      <>
        In {k.b('Code')}, press {k.b('+')} beside the files and type {k.code('site/index.html')}. A name with a slash
        in it puts the file in a folder; a name with an extension is taken as the file it says it is.
      </>
    ),
    (k) => (
      <>
        Add {k.code('site/app.css')} and {k.code('site/app.js')} the same way. Your page refers to them by name, as
        in {k.code('href="app.css"')}, because the folder is the root of what gets served rather than part of the
        address.
      </>
    ),
    (k) => (
      <>
        For anything that is not text, like an image or a font, open a file in {k.code('site')} and press the upload
        button beside the files: it lands in the same folder. A PNG cannot be typed into a text editor, so that is the
        way in.
      </>
    ),
    (k) => <>In {k.code('lambda.cs')}, serve the folder:</>,
    (k) => (
      <>
        Press {k.b('Deploy')}. {k.code('site/index.html')} answers at {k.code('/')}, {k.code('site/app.css')} at{' '}
        {k.code('/app.css')}, and any address matching no file is answered with the page, so a front end that does its
        own routing still works when somebody reloads on a deep link.
      </>
    ),
    () => <>Add an API beside it and the page has something to talk to:</>,
  ] as Text[],

  /** Assets or code a build tool makes, and the files it makes them from, kept beside them. */
  built: ((k) => (
    <>
      Some of a lambda may be made by a build tool rather than written as it is served or compiled: compiled, bundled or
      generated. The version holds what the tool makes - as its assets, or as its code - and beside it the files it
      makes them from, its {k.b('build folder')}: {k.code('.lambda/build/')} in the version, {k.code('build/')} in a
      clone, holding whatever the tool works from. Your agent changes those files, runs the build where it works and
      saves both in the same version. This platform builds nothing.
    </>
  )) as Text,
  built2: ((k) => (
    <>
      Like the documentation, it belongs to its version: compared in the history, rolled back, copied into a draft,
      cloned, downloaded and published with the code - and never compiled or served. In the control center,{' '}
      {k.b('Build')} shows it once a version keeps one: how it is built, as its README says, its files, and whether a
      version changed them without changing anything built from them. It is read there, not edited - a change to it is
      made where it is built.
    </>
  )) as Text,
  builtAside:
    'What is written as it is served or compiled needs none. What a build installs or keeps for itself - node_modules, for example - is never part of a version: a .gitignore in the build folder keeps it out.',

  storage: ((k) => (
    <>
      A lambda keeps files in two places, and the editor shows them apart: {k.b('Files')} holds the files of a
      version - the program - and {k.b('Data')} holds the workspace - what the program keeps. The difference is{' '}
      {k.em('whose they are')}. The files of a version belong to that version; the data belongs to the lambda, and
      every version shares it.
    </>
  )) as Text,
  savedWithCode: 'In a version',
  workspaceColumn: 'In the data',
  table: [
    ['what it holds', 'the code and assets: the program, front end included - and its documentation, its tests and what it is built from', 'whatever the lambda writes, or somebody uploads'],
    ['when it changes', 'never - a change is a new version', 'the moment something is written to it'],
    ['a deploy', 'puts exactly these files online', 'never touches it'],
    ['rolling back', 'brings the old files back', 'no effect: every version shares it'],
    ['a draft', 'starts as a copy of them', 'works on a copy of it'],
    ['when it goes', 'with old versions, past the limit', 'with the lambda, or when you switch it off'],
  ] as [string, string, string][],
  reachedAs: 'reached from code as',
  storageAside:
    'They cannot be one place. If they were, a deploy would either wipe everything your lambda had written since, or nothing could ever be removed from what it ships. A game that keeps a leaderboard wants the second; the page it serves wants the first. So the page goes in the version, and the leaderboard in the data.',

  database: ((k) => (
    <>
      Records - entries, accounts, orders, votes - belong in the {k.b('database')}: a SQLite database of the lambda's
      own, switched on under {k.b('Data')}. The code opens a connection with {k.code('Database.GetConnection()')} and
      reads and writes it through {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, with a
      context of its own that maps the tables:
    </>
  )) as Text,
  database2: ((k) => (
    <>
      Its tables are made by {k.b('migrations')}: SQL files shipped with the version in {k.code('migrations/')}, applied
      in order by {k.link('https://evolve-db.netlify.app/', 'Evolve')} as the lambda starts - each once, so a new
      version only ever runs what is new. Never change a migration that was applied; a change to a table is the next
      file.
    </>
  )) as Text,
  database3: ((k) => (
    <>
      Like all data, the database is shared by every version, left alone by deploys and rollbacks, and a draft works
      on a copy of it. Under {k.b('Data')} you see its tables and what is in them - the simple view calls them records.{' '}
      {k.b('Download')} carries it along as an ordinary SQLite file.
    </>
  )) as Text,
  databaseAside: ((k) => (
    <>
      Make a context where you need it and dispose of it, and use it synchronously - {k.code('ToList')} and{' '}
      {k.code('SaveChanges')}, not {k.code('ToListAsync')} and {k.code('SaveChangesAsync')}. The tables are the
      migrations' to make, never Entity Framework's. The {k.link('/editor/demo-crud', 'demo-crud')} demo does all of
      it.
    </>
  )) as Text,

  keeping: ((k) => (
    <>
      {k.code('Workspace')} is a private directory your lambda may read and write: the place for files - pictures
      somebody uploads, a document it makes, a model it loads. Records belong in the database, and what is known about
      a file - who uploaded it, when - is a record too.
    </>
  )) as Text,
  keeping2: ((k) => (
    <>
      There is also {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, and {k.code('Tree')}/{k.code('Files')}/{k.code('App')} for serving it. Nothing else
      on the file system is reachable.
    </>
  )) as Text,

  secrets: ((k) => (
    <>
      An API key, a password or a token belongs in the {k.b('secrets')}, not in the code - where every version,
      every download and everybody reading the history would have it. The code reads one by name:
    </>
  )) as Text,
  secrets2: ((k) => (
    <>
      Switch secrets on under {k.b('Data')} and set the value there. Once saved, it is never shown again - not to
      you, not to an agent; you can only replace it. The list says which names the code reads that have no value
      yet, and the overview asks for them. {k.code('Secret.Exists')} says whether one is set, for code that works
      without it. Like all data, secrets are shared by every version, and a draft works on a copy of them.
    </>
  )) as Text,
  secretsAside: ((k) => (
    <>
      They are stored encrypted, with a key that is not in the database. In a downloaded project,{' '}
      {k.code('Secret.Read("NAME")')} reads the environment variable {k.code('NAME')} - the values themselves stay
      here.
    </>
  )) as Text,

  sockets: ((k) => (
    <>
      Supported, and not an afterthought. The {k.link('/editor/demo-game', 'demo-game')} demo pairs players and runs
      every game on the server. The simplest form is three callbacks:
    </>
  )) as Text,
  socketsAside: ((k) => (
    <>
      One thing catches everybody: a browser cannot set headers on a websocket handshake. Pass what the handler needs
      in the query, where it reads it from {k.code('connection.Request.Header.Query')}, or send secrets as the first
      message.
    </>
  )) as Text,
  /** A paragraph below the websocket sample: pushing what changed rather than asking for it again and again. */
  sockets2: ((k) => (
    <>
      When the page only listens - a count, a feed, a scoreboard - server-sent events are simpler: one long response
      the server keeps writing to, which the browser reconnects by itself. The{' '}
      {k.link('/editor/demo-live', 'demo-live')} demo sends every vote to everybody watching that way. Either way the
      server pushes what changed. A page that asks again every few seconds sends a request each time, whether anything
      changed or not, and is still late.
    </>
  )) as Text,

  limits:
    'Your code runs on a shared server, so some of C# is refused before it compiles: starting processes, opening sockets of your own, loading assemblies, reaching the file system outside your workspace, and reflection used to get around any of that. So is waiting for a task with .Result or .Wait() instead of awaiting it: requests run on one thread per core, and the task would have to finish on the very thread that is waiting for it.',
  limits2:
    'Everything else is there, including the whole of the GenHTTP module API. If something is refused you are told which line and why, not simply that it failed.',

  away: ((k) => (
    <>
      {k.b('Download')} in the editor gives you the whole thing as a .NET project: a solution you can open,{' '}
      {k.code('dotnet run')}, and keep. It needs nothing but the GenHTTP package, and comes with a{' '}
      {k.code('Dockerfile')} to build and run it as a container.
    </>
  )) as Text,
  away2: ((k) => (
    <>
      Your snippet becomes {k.code('Project.cs')}, and {k.code('Program.cs')} serves what it returns. Your other files
      come across exactly as you wrote them. {k.code('Workspace')} and {k.code('Assets')} become two folders beside
      the program, with the same methods, kept apart in a {k.code('Platform')} folder - so nothing in your code has to
      change.
      {' '}{k.code('Secret')} reads environment variables of the same name there; the values stay here. The
      documentation and the tests come along in {k.code('docs')} and {k.code('tests')}, and the build folder in{' '}
      {k.code('build')}.
      {' '}{k.code('Database')} opens {k.code('database/database.db')}, which the download carries with the records
      your app kept.
    </>
  )) as Text,
  awayAside:
    'Worth knowing before you build anything here: what you write is yours and it leaves whole. Nothing about running it on this machine locks it to this machine.',

  git: ((k) => (
    <>
      Every lambda is a git repository as well. {k.b('Clone')}, on the overview of the control center and beside its
      code, has its address - the address of your editor with the name of the app after it - and{' '}
      {k.code('git clone')} gives you the project {k.b('Download')} gives you, with every version a commit of{' '}
      {k.code('main')}, tagged {k.code('v1')}, {k.code('v2')} and so on, and every draft a branch. Open it in your own
      editor, hand it to your coding agent, {k.code('dotnet run')} it.
    </>
  )) as Text,
  git2: ((k) => (
    <>
      Push and it is here. Each commit pushed to {k.code('main')} becomes the next version, its first line the change
      it made - compiled first, and refused if it does not compile - and {k.code('git push -o deploy')} puts it
      online. A branch you push becomes a draft, with its preview online at its own address; push it to{' '}
      {k.code('main')}, or add {k.code('-o merge')} to its last push, and it is the next version. What the platform
      puts around your code to make it a project - {k.code('Program.cs')}, the project file, {k.code('Platform')} -
      is no part of your app, so a push that changes it is refused and says why. {k.code('AGENTS.md')} in the
      repository tells a coding agent the rest.
    </>
  )) as Text,
  gitAside:
    'The address holds your editor key, like the address of the editor: whoever has it can push. What your app keeps - its records, its files, its keys and passwords - is never in the repository.',

  open: ((k) => (
    <>
      If what you built could help somebody else, publish its code: open {k.b('Open source')} in the control center,
      pick a license - MIT, unless you want another - and switch it on. Its code gets a page of its own among the{' '}
      {k.link('/source', 'open source apps')}, where anybody can read it, star it, download any version as the same
      project {k.b('Download')} gives you, with the license beside it, or clone every version of it with git.
    </>
  )) as Text,
  open2: (() => (
    <>
      Every version is published, the earlier ones too, with its documentation, its tests, its build folder and
      the change each one made. What the app keeps is never published - its records, the files it saved, the values of its keys and
      passwords - and neither is what you asked for in your own words, or who uses the app. Switch it off and the page
      is gone; its stars are kept for when you publish it again.
    </>
  )) as Text,
  openAside:
    'Everything in the code becomes public, the earlier versions included. A key or a password belongs with the keys and passwords under Data, never in the code - published or not.',

  agents: ((k) => (
    <>
      There is an MCP endpoint at {k.code('/mcp')}. Point an agent at it and it can do everything the editor does:
      read the guide, read a demo in full, write files, compile them, and deploy. It is the same API underneath.
    </>
  )) as Text,
  agents2: ((k) => (
    <>
      It says why as it goes - {k.code('write_code')} takes the specification and the change - and it can look at
      what it deployed: {k.code('read_logs')} answers with the lambda's recent requests, what it printed and the
      stack trace of anything it threw, which is how an agent finds out its code works rather than assuming it. You
      watch the same thing in the control center. It writes the documentation and the tests as it goes, reads them
      before it changes anything, and runs the tests against a draft's address before it puts the draft online. A
      page meant to be found gets a title, a description and an icon, and a preview for when its link is shared. At
      the foot of the pages it builds, it adds a small line saying they were made with GenHTTP Lambda - tell it if you
      would rather not have it, and it takes it out.
    </>
  )) as Text,
  more: 'More about that →',
  make: 'Make one',
};
