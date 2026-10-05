import type { ReactNode } from 'react';

type Node = ReactNode;

export const files = {
  hint: (b: (text: string) => Node) => (
    <>
      The files of one version - the program. {b('Code')} is compiled and never served. {b('Assets')} - pages,
      scripts, styles, images - are saved with the code, deployed and rolled back with it, and are public if the code
      serves them. What the lambda keeps while it runs is not here: that is its {b('Data')}.
    </>
  ),
  scope: (version: number, data: (text: string) => Node) => (
    <>
      These belong to version {version} and change with it. What the lambda keeps while it runs is the same for every
      version, and is under {data('Data')}.
    </>
  ),
  edit: 'Edit this version',
  version: 'Version',
  shown: (version: number, online: boolean, newest: boolean) =>
    `Version ${version}${online ? ', online' : newest ? ', newest' : ''}`,
  optionOnline: ' (online)',
  readFailed: 'That version could not be read.',
  noVersion: 'There is no version to show yet.',
  label: 'Files',
  code: 'Code',
  codeWhy: 'Compiled into the lambda, never served.',
  count: (files: number) => (files === 1 ? '1 file' : `${files} files`),
  codeUsage: (files: string, used: string, of: string) => `${files}, ${used} of ${of} characters`,
  usage: (files: string, used: string, of: string) => `${files}, ${used} of ${of}`,
  noCode: 'No code in this version.',
  assets: 'Assets',
  assetsPublic: 'Public: this version serves them with Assets.',
  assetsPrivate: 'Saved with the code, but this version does not serve them.',
  noAssets: 'None in this version.',
  context: 'Documentation and tests',
  contextWhy: 'Never compiled and never served: what is written about this version, for whoever reads or changes it.',
  contextUsage: (files: string, size: string) => `${files}, ${size} - counted with the assets`,
  noContext: 'Nothing written about this version yet.',
  /** What the version is built from - its build folder - as a group of its files. */
  build: 'Build',
  buildWhy: 'Never compiled and never served: what the code or the assets are built from, by whoever changes them.',
  noBuild: 'Nothing - where a build tool makes the code or the assets, what it makes them from is kept here.',
  data: 'Data',
  dataPublic: 'Public: the code online serves it with Workspace.',
  dataPrivate: 'Private to the lambda. Not part of any version.',
  uploadFailed: (path: string) => `${path} could not be uploaded.`,
  deleteFolder: (path: string, held: number) =>
    held > 0
      ? `Delete ${path} and the ${held === 1 ? '1 file' : `${held} files`} in it?`
      : `Delete the folder ${path}?`,
  deleteFile: (path: string) => `Delete ${path}? The lambda will not find it any more.`,
  deleteFailed: 'It could not be deleted.',
  full: 'The data is full',
  uploadInto: (folder: string) => `Upload into ${folder}`,
  upload: 'Upload',
  reading: 'Reading…',
  noData: 'Nothing yet. What the lambda saves while it runs appears here.',
  delete: (path: string) => `Delete ${path}`,
  deleteShort: 'Delete',
  fileFailed: 'The file could not be read.',
  pick: 'Pick a file to see what is in it.',
  tooLarge: (name: Node, size: string) => (
    <>
      {name} is {size}, too large to show here.
    </>
  ),
  download: 'Download',
  readingFile: (name: string) => `Reading ${name}…`,
  missing: (name: string) => `This version has no file called ${name}.`,
  saved: 'saved',
  notText: 'Not text. Download it to look inside.',
};
