import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * Browsing files: the workspace under Data, and a version's pages and the
 * files beside them under Documentation and Tests.
 */
export const files = {
  version: 'Version',
  shown: (version: number, online: boolean, newest: boolean) =>
    `Version ${version}${online ? ', online' : newest ? ', newest' : ''}`,
  optionOnline: ' (online)',
  count: (files: number) => (files === 1 ? '1 file' : `${files} files`),
  usage: (files: string, used: string, of: string) => `${files}, ${used} of ${of}`,
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
