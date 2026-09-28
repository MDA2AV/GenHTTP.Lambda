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
    features: 'Changing it safely',
    files: 'More than one file',
    page: 'Serving a page',
    spa: 'A front end, step by step',
    storage: 'The two places files live',
    keeping: 'Keeping data',
    sockets: 'Websockets',
    limits: 'What it will not let you do',
    away: 'Taking it away',
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
    ['Overview', () => <>Whether it is online, how many requests it had today and how many failed, the latest change, and how much room is left.</>],
    [
      'Change',
      (k) => (
        <>
          Say what should be different, and the agent on this server does it while you watch. It works in a feature,
          tries it there, and merges it into the next version once it works. Switch off{' '}
          {k.b('Put it online when it is done')} to try the feature yourself first.
        </>
      ),
    ],
    ['Features', () => <>Changes worked on beside the lambda: each is tried at an address of its own and merged into the next version once it is right. Opened, a feature has its own code, data and logs.</>],
    ['Files', () => <>The files of a version: its code and assets, the program itself. A lock or a globe says whether the public can reach them.</>],
    ['Data', () => <>What the lambda keeps while it runs, shared by every version: the workspace. Look into it, upload and delete files, or switch it off.</>],
    ['Versions', () => <>What each version changed and what was asked for, and the difference to the one before. Deploy or roll back from here, or start a feature from any of them.</>],
    ['Deployments', () => <>What was online when, and what took it down.</>],
    ['Stats', () => <>Requests, failures, response times and the most asked-for paths, over the last hour or day.</>],
    ['Logs', () => <>Its requests, what it printed, and the stack trace of anything that went wrong, as it happens.</>],
    [
      'Code',
      (k) => (
        <>
          Writing it by hand. {k.b('Check')} compiles, {k.b('Save')} makes a version, {k.b('Deploy')} puts it online.
          In a feature, {k.b('Save')} keeps it in the feature and {k.b('Deploy preview')} puts it online at the
          feature's address. {k.code('Ctrl-S')} saves; {k.code('F12')} goes to a declaration.
        </>
      ),
    ],
  ] as [string, Text][],
  sections: ((k) => (
    <>
      Every section works the same way: its title, an {k.b('ⓘ')} that explains it, its actions on the right, and -
      where it has more than one view - a row of pills underneath. The pills of the code are its files.
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
    change: 'Keeps entries in the workspace so they survive a restart',
  },
  why2: ((k) => (
    <>
      Agents pass the same two fields to {k.code('write_code')}. In {k.b('Code')}, saving asks for the change. Both
      are optional; a long specification is cut at 4000 characters and a change at 500 rather than refused. A
      feature keeps its own two, and the version it is merged into takes them over.
    </>
  )) as Text,

  features: ((k) => (
    <>
      A version never changes once it is saved - which is what makes every one worth keeping: any of them can be
      compared with, and put back online exactly as it was. To change a lambda that people use, start a{' '}
      {k.b('feature')} instead.
    </>
  )) as Text,
  featureSteps: [
    (k) => (
      <>
        Start it under {k.b('Features')}, or from any version. It is a copy of that version's code and assets, and of
        the lambda's data.
      </>
    ),
    (k) => (
      <>
        Change it as often as it takes - in {k.b('Code')}, or by asking the agent. {k.b('Deploy preview')} puts it
        online at an address of its own, {k.code('/features/…/')}, against its own copy of the data. Visitors of the
        lambda see none of it, and nothing it writes reaches the lambda's data.
      </>
    ),
    (k) => (
      <>
        {k.b('Merge')} it once it is right: it becomes the next version, with its notes, and goes online at once if you
        want. The feature goes with it - its preview and its copy of the data.
      </>
    ),
  ] as Text[],
  featureSample: 'Leaderboard',
  featuresAside: (() => (
    <>
      Several features can be worked on at once. Only one based on the newest version can be merged, so that a merge
      never undoes a version saved after the feature began. When another was merged first, bring its changes in - or
      ask the agent to - and then base the feature on the newest version. Nothing merges on its own; that is
      deliberate.
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
    ['what it holds', 'the code and assets: the program, front end included', 'whatever the lambda writes, or somebody uploads'],
    ['when it changes', 'never - a change is a new version', 'the moment something is written to it'],
    ['a deploy', 'puts exactly these files online', 'never touches it'],
    ['rolling back', 'brings the old files back', 'no effect: every version shares it'],
    ['a feature', 'starts as a copy of them', 'works on a copy of it'],
    ['when it goes', 'with old versions, past the limit', 'with the lambda, or when you switch it off'],
  ] as [string, string, string][],
  reachedAs: 'reached from code as',
  storageAside:
    'They cannot be one place. If they were, a deploy would either wipe everything your lambda had written since, or nothing could ever be removed from what it ships. A game that keeps a leaderboard wants the second; the page it serves wants the first. So the page goes in the version, and the leaderboard in the data.',

  keeping: ((k) => (
    <>
      {k.code('Workspace')} is a private directory your lambda may read and write. It is the place for anything that
      has to outlive a request, or a deployment.
    </>
  )) as Text,
  keeping2: ((k) => (
    <>
      There is also {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, and {k.code('Tree')}/{k.code('Files')}/{k.code('App')} for serving it. Nothing else
      on the file system is reachable.
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

  limits:
    'Your code runs on a shared server, so some of C# is refused before it compiles: starting processes, opening sockets of your own, loading assemblies, reaching the file system outside your workspace, and reflection used to get around any of that.',
  limits2:
    'Everything else is there, including the whole of the GenHTTP module API. If something is refused you are told which line and why, not simply that it failed.',

  away: ((k) => (
    <>
      {k.b('Download')} in the editor gives you the whole thing as a .NET project: a solution you can open,{' '}
      {k.code('dotnet run')}, and keep. It has one package reference and no trace of this platform in it.
    </>
  )) as Text,
  away2: ((k) => (
    <>
      Your snippet becomes the body of {k.code('Program.cs')}, wrapped in a host that serves what it returns. Your
      other files come across exactly as you wrote them. {k.code('Workspace')} and {k.code('Assets')} become two
      folders beside the code, with the same methods, so nothing in your code has to change.
    </>
  )) as Text,
  awayAside:
    'Worth knowing before you build anything here: what you write is yours and it leaves whole. Nothing about running it on this machine locks it to this machine.',

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
      watch the same thing in the control center.
    </>
  )) as Text,
  more: 'More about that →',
  make: 'Make one',
};
