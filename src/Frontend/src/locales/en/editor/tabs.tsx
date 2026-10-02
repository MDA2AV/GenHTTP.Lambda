
/** The strip of files above the code. */
export const tabs = {
  codeName: 'Letters, digits, dashes and underscores, ending in .cs',
  slashes: 'No leading or trailing slash, and under 120 characters.',
  deep: 'At most six folders deep.',
  characters: 'Letters, digits, dashes, underscores and dots, separated by slashes.',
  extension: 'It needs an extension, so it can be served as the right thing.',
  context: 'In .lambda/, only docs/ and tests/ - letters, digits, dashes, underscores and dots, separated by slashes.',
  contextFiles: 'Documentation and tests: part of the version, never compiled or served',
  exists: 'There is already a file with that name.',
  remove: (name: string) => `Remove ${name}? Its contents go with it.`,
  there: (name: string) => `${name} is already there.`,
  entry: 'The snippet: what it returns is what gets served',
  errors: 'has errors',
  removeFile: (name: string) => `Remove ${name}`,
  removeTitle: 'Remove this file',
  placeholder: 'Types.cs, site/index.html or .lambda/docs/api.md',
  newFile: 'New file',
  uploadTitle: 'Upload a file - an image, a font, a page',
  upload: 'Upload a file',
};
