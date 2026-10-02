import type { ReactNode } from 'react';

type Node = ReactNode;

/** The data of a lambda: what it keeps rather than what it is. */
export const data = {
  hint:
    'Data is what the lambda keeps while it runs. It belongs to the lambda, not to a version: every version reads and writes the same data, and nothing you do with versions changes it. It goes when the lambda is deleted, or when you switch that kind of data off.',
  facts: [
    ['Shared by every version', 'Whichever version is online reads and writes the same data.'],
    ['Kept when you deploy', 'Deploying or rolling back never touches it.'],
    ['Yours to switch', 'Each kind is on only while you want it. Switching one off deletes what it holds.'],
  ] as [string, string][],
  featureHint:
    'A copy of your app\'s data, just for this draft: its preview reads and writes only this copy, so trying things here never touches the real data. It goes when the draft does.',
  recopy: 'Reset the test data',
  recopyTitle: 'Replace it with a fresh copy of your app\'s data',
  recopyConfirm: 'Reset the test data?',
  recopyText:
    'Everything the preview wrote into it is replaced by a fresh copy of your app\'s data. Your app\'s own data is not touched.',
  keepCopy: 'Keep it',
  recopied: 'The test data is fresh again.',
  recopyFailed: 'The test data could not be reset.',
  copyContents: 'What the test data holds',
  kindsLabel: 'Kinds of data',
  kinds: {
    database: {
      name: 'Database',
      what: 'A SQLite database the lambda keeps its records in, reached with Database.GetConnection().',
      count: (items: number) => (items === 1 ? '1 table' : `${items} tables`),
      confirmOff: 'Switch the database off?',
      switchedOn: 'The database is on, and empty. The lambda starts again with it on its next request.',
      switchedOff: 'The database is off, and every table in it is deleted.',
    },
    workspace: {
      name: 'Workspace',
      what: 'Files the lambda reads and writes while it runs: uploads, records, anything it keeps.',
      count: (items: number) => (items === 1 ? '1 file' : `${items} files`),
      confirmOff: 'Switch the workspace off?',
      switchedOn: 'The workspace is on. The lambda can use it from its next request.',
      switchedOff: 'The workspace is off, and what it held is deleted.',
    },
    secrets: {
      name: 'Secrets',
      what: 'API keys, passwords and tokens the code reads by name. Once saved, a value is never shown again - not to you, not to an agent.',
      count: (items: number) => (items === 1 ? '1 secret' : `${items} secrets`),
      confirmOff: 'Switch secrets off?',
      switchedOn: 'Secrets are on. Add the values the code needs.',
      switchedOff: 'Secrets are off, and every value is deleted.',
    },
  } as Record<string, {
    name: string;
    what: string;
    count: (items: number) => string;
    confirmOff: string;
    switchedOn: string;
    switchedOff: string;
  }>,
  on: 'On',
  off: 'Off',
  byDefault: 'On by default',
  offText: 'Switched off. It holds nothing, and code that uses it fails until it is switched on again.',
  switchLabel: (name: string) => `${name} on or off`,
  confirmText: (what: string) => `Everything in it - ${what} - is deleted for good. This cannot be undone.`,
  confirmEmpty: 'It is empty, so nothing is lost.',
  inUse: 'The version online uses it, so it will fail where it does until you switch it on again.',
  deleteAndOff: 'Switch off and delete',
  keep: 'Keep it',
  switchFailed: 'That could not be switched.',
  readFailed: 'The data could not be read.',
  demo: 'A demo: its data is there to be read, not changed.',
  contents: 'What the workspace holds',
  browse: 'Files',
  offBrowse: 'The workspace is off, so there are no files to show.',
  missingDot: 'The code needs a secret that is not set',
  offDot: 'The code needs it, and it is switched off',

  /** The database: tables, and what is in them - read here, written by the lambda. */
  database: {
    contents: 'Tables',
    copyContents: 'Tables in the test data',
    readOnly: 'Read only: what is in here is written by the lambda.',
    empty: 'No tables yet',
    emptyText: 'The code makes them, with a migration in migrations/ that Evolve applies as the lambda starts. What it then keeps shows up here.',
    offTitle: 'The database is off',
    offText: 'Switch it on to keep records the code reads and writes with Entity Framework Core or SQL - entries, accounts, orders.',
    offWanted: 'The code connects to the database, but it is switched off: whatever connects fails until it is on.',
    switchOn: 'Switch on',
    howTo: 'In the code',
    howToText: (code: (text: string) => Node) => (
      <>
        {code('Database.GetConnection()')} opens a connection to it - for a {code('DbContext')} of Entity Framework Core, or for
        SQL - and the migrations in {code('migrations/')} make its tables, applied by Evolve. In a downloaded project it is the
        plain SQLite file {code('database/database.db')}.
      </>
    ),
    copy: 'Copy',
    rows: (count: number) => (count === 1 ? '1 row' : `${count.toLocaleString()} rows`),
    notCounted: 'not counted',
    view: 'view',
    migrations: 'Migrations',
    migrationsHint: 'The history Evolve keeps of the migrations it applied, not records of the app.',
    columns: 'Columns',
    primaryKey: 'primary key',
    required: 'required',
    defaultsTo: (value: string) => `defaults to ${value}`,
    untyped: 'any type',
    newestFirst: 'Newest first',
    oldestFirst: 'Oldest first',
    sortBy: (column: string) => `Sort by ${column}`,
    page: (from: number, to: number, total: number) => `${from.toLocaleString()}–${to.toLocaleString()} of ${total.toLocaleString()}`,
    previous: 'Previous page',
    next: 'Next page',
    reload: 'Read again',
    readFailed: 'The table could not be read.',
    noRows: 'Nothing in this table yet.',
    pick: 'Pick a table to see what is in it.',
    nullValue: 'NULL',
    bytes: (size: string) => `bytes · ${size}`,
    cut: (length: number) => `… ${length.toLocaleString()} characters in all`,
    row: (number: number) => `Row ${number.toLocaleString()}`,
    openRow: 'Show the whole row',
    close: 'Close',
  },

  /** The secrets: written and replaced, never read back. */
  secrets: {
    add: 'Add a secret',
    addTitle: 'Add a secret',
    replaceTitle: (name: string) => `Replace ${name}`,
    setTitle: (name: string) => `Set ${name}`,
    name: 'Name',
    nameHint: 'Letters, digits and underscores, not starting with a digit. The code reads it by this name.',
    nameInvalid: 'Use letters, digits and underscores, and do not start with a digit.',
    nameTaken: (name: string) => `${name} is set already. Saving replaces its value.`,
    value: 'Value',
    valuePlaceholder: 'Paste the key, password or token',
    show: 'Show',
    hide: 'Hide',
    sealed: 'Once saved, nobody can see it again - not you, not an agent. Only the code reads it.',
    inDraft: 'Only this draft\'s test data gets it. The secrets of your app stay as they are.',
    save: 'Save',
    cancel: 'Cancel',
    saved: (name: string) => `${name} is saved. The code reads it from its next request.`,
    saveFailed: 'The secret could not be saved.',
    replace: 'Replace',
    replaceHint: 'Give it a new value',
    delete: (name: string) => `Delete ${name}`,
    deleteTitle: (name: string) => `Delete ${name}?`,
    deleteUsed: 'The code reads it, so whatever reads it fails until it is set again.',
    deleteUnused: 'Nothing in the code reads it by this name.',
    deleteDraft: 'Only this draft\'s copy is deleted.',
    deleteConfirm: 'Delete',
    deleted: (name: string) => `${name} is deleted.`,
    deleteFailed: 'The secret could not be deleted.',
    used: 'read by the code',
    usedHint: 'The code reads it with Secret.Read or Secret.Exists.',
    unused: 'not read by the code',
    unusedHint: 'No code reads it by this name. Is it spelled the way the code spells it?',
    set: (when: Node) => <>set {when}</>,
    hidden: 'Hidden for good',
    missingTitle: (count: number) => (count === 1 ? 'The code needs a secret that is not set' : `The code needs ${count} secrets that are not set`),
    missingText: (count: number): string => (count === 1 ? 'Until it is set, whatever reads it fails.' : 'Until they are set, whatever reads them fails.'),
    contents: 'Stored secrets',
    copyContents: 'Secrets in the test data',
    setIt: 'Set it',
    optionalTitle: 'Optional',
    optionalText: 'The code asks whether these are set, and works without them.',
    empty: 'No secrets yet',
    emptyText: 'Keep API keys, passwords and tokens here rather than in the code, where every version, every download and everybody reading it would have them.',
    howTo: 'In the code',
    howToText: (code: (text: string) => Node) => (
      <>
        {code('Secret.Read("NAME")')} reads a value, {code('Secret.Exists("NAME")')} says whether it is set. In a
        downloaded project, both read the environment variable of the same name.
      </>
    ),
    copy: 'Copy',
    offTitle: 'Secrets are off',
    offText: 'Switch them on to keep API keys and passwords that the code can read and nobody can see.',
    offWanted: (names: Node) => <>The code reads {names}. Switch secrets on to set them.</>,
    switchOn: 'Switch on',
    readFailed: 'The secrets could not be read.',
  },

  /**
   * The data in the simple view, shown once there is any: what the app
   * saved, and the keys it uses - nothing about files as a program has them.
   */
  simple: {
    hint: 'What your app keeps - its records, the files it saved - and the keys it uses. It stays as it is whatever change is online.',
    workspace: 'Saved by your app',
    workspaceText: 'The files your app saved while people used it: uploads, pictures, documents.',
    workspaceSize: (files: string, size: string) => `${files}, ${size}`,
    more: (count: number) => `and ${count} more`,
    database: 'Records',
    databaseTitle: 'Records your app keeps',
    databaseText: 'What your app keeps while people use it - sign-ups, entries, votes - table by table. Nothing here changes when a new change goes online.',
    records: (count: number) => (count === 1 ? '1 record' : `${count.toLocaleString()} records`),
    noRecords: 'Your app has not kept any records yet.',
    secrets: 'Keys and passwords',
    secretsText: 'What your app uses to connect to other services. Once saved, nobody can see them - not you, not the agent.',
    needed: 'Your app needs this',
    enter: 'Enter it',
    change: 'Change',
    enterTitle: (name: string) => `Enter ${name}`,
    changeTitle: (name: string) => `Change ${name}`,
    switchesOn: 'Saving it lets your app use keys and passwords from now on.',
    saved: (name: string) => `${name} is saved. Your app uses it right away.`,
    nothing: 'Your app has not saved anything yet.',
  },
};
