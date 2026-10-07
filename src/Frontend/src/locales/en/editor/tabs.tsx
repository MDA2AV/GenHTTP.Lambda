/** Adding, uploading and removing the files of the code view, and what their names may be. */
export const tabs = {
  /** Said of a name a C# file at the top of the code may not have. */
  codeName: 'A C# file at the top: letters, digits, dashes and underscores, starting with a letter and ending in .cs, 40 characters at most.',
  /** Said of a name any other file of the code may not have. */
  name: 'Letters, digits and - _ . + @ ( ) [ ] { } $ ~, folders separated by slashes, no spaces and no name ending in a dot.',
  /** Said of a name at the top that an exported or cloned project has for its own. */
  taken: 'An exported or cloned lambda has a file or folder of that name at the top. Put it into a folder, or call it something else.',
  /** Said of a name below .lambda/, where the documentation and the tests were kept once. */
  lambda: 'A lambda keeps no .lambda/ folder any more: its documentation goes in docs/, its tests in tests/.',
  /** Said of a name below assets/, where the files it serves were kept once. */
  assets: 'What a lambda serves is in its resources now - add it there.',
  /** Said of a name a resource may not have. */
  resourceName: 'Letters, digits, dashes, underscores and dots, separated by slashes, at most six folders deep - and an extension, so it is served as the right thing.',
  exists: 'There is already a file with that name.',
  remove: (name: string) => `Remove ${name}? Its contents go with it.`,
  removeFolder: (name: string, files: number) => `Remove ${name} and the ${files === 1 ? '1 file' : `${files} files`} in it?`,
  there: (name: string) => `${name} is already there.`,
  entry: 'The snippet: what it returns is what gets served',
  errors: 'has errors',
  removeFile: (name: string) => `Remove ${name}`,
  removeTitle: 'Remove',
  codePlaceholder: 'Store.cs, docs/notes.md or frontend/app.ts',
  resourcePlaceholder: 'web/index.html',
  upload: 'Upload a file',
};
