import type { ReactNode } from 'react';

type Node = ReactNode;

export const showcase = {
  loadFailed: 'The showcase could not be loaded.',
  loading: 'Loading…',
  title: 'a title',
  description: 'a description',
  picture: 'a picture',
  updated: 'The showcase entry is updated.',
  listed: 'It is on the showcase page now.',
  waiting: 'Saved. It appears on the showcase page once the lambda is online.',
  saveFailed: 'The showcase entry could not be saved.',
  removed: 'Taken off the showcase page.',
  removeFailed: 'The showcase entry could not be removed.',
  wrongType: 'That is not a PNG, JPEG, GIF or WebP image.',
  tooLarge: (size: string, limit: string) => `That is ${size}; a picture can be ${limit} at most.`,
  unreadable: 'That file could not be read.',
  hint: (tool: Node) => (
    <>
      The showcase page lists lambdas their owners chose to show, the ones in use lately first. Only whoever holds the
      editor key can put a lambda there or take it down, and it is only listed while it is online. An agent can do the
      same with its {tool} tool.
    </>
  ),
  open: 'Open the showcase',
  switch: 'Show this lambda on the showcase page',
  listedNow: 'Listed now. Anyone browsing the showcase can open it.',
  notListed: 'Saved, but not listed: the lambda is offline. It reappears once it is deployed again.',
  off: 'Off. Nothing about this lambda is shown anywhere until you switch this on and save.',
  offline: 'The lambda is offline, so the entry will wait until it is deployed. Only lambdas that answer are listed.',
  titleLabel: 'Title',
  titlePlaceholder: 'Pub quiz scoreboard',
  descriptionLabel: 'Description',
  descriptionPlaceholder:
    'Teams enter their answers on their phones, the host marks them, and the scoreboard updates for everyone in the room.',
  save: 'Save changes',
  add: 'Add to the showcase',
  takeOff: 'Take it off',
  needs: (missing: string[]) =>
    `Still needs ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} and ${missing[missing.length - 1]}` : missing[0]}.`,
  tooLong: 'Some of it is too long.',
  allSaved: 'Everything is saved.',
  preview: 'Preview',
  card: (address: Node) => <>This is the card visitors see. It opens {address}.</>,
  confirm: 'Take it off the showcase?',
  keep: 'Keep it',
  confirmText: 'The title, description and picture are deleted. The lambda itself stays exactly as it is.',
  pictureLabel: 'Picture',
  formats: (limit: string) => `PNG, JPEG, GIF or WebP, up to ${limit}`,
  notSaved: 'not saved yet',
  replace: 'Drop a new one here to replace it.',
  drop: 'Drop a picture here.',
  advice: 'A screenshot, or a short GIF of it in use, works best at 16:10.',
  another: 'Choose another',
  choose: 'Choose a file',
  keepSaved: 'Keep the saved one',
  clear: 'Clear',
};
