import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * Cloning a lambda with git, offered in the full view only - on its overview
 * and beside its code. Said in git's words where they are git's (clone,
 * commit, main, branch), and in the editor's for what the platform makes of
 * them: a version, a draft, putting it online.
 */
export const clone = {
  button: 'Clone',
  title: 'Clone with git',
  intro:
    'Work on it with your own tools and coding agent: the repository is the project it runs as, every version a commit of main and every draft a branch.',
  /** Under the address, which holds the editor key. */
  keyWarning: 'The address holds the editor key: whoever has it can change the app. Keep it out of what you share.',
  /** In a draft's code: which branch of the clone it is. */
  draft: (branch: Node) => <>This draft is the branch {branch}.</>,
  pushing: 'Pushing',
  toMain: (deploy: Node) => <>A commit pushed to main is the next version, not yet online - {deploy} puts it online with the push.</>,
  toBranch: 'A branch pushed is a draft, its preview online at an address of its own.',
  agents: (file: Node) => <>{file} in the repository tells a coding agent the rest.</>,
  readOnly: 'A demo is read only: clone it to read it, and start a lambda of your own from it to change it.',
  copy: 'Copy',
  copied: 'Copied',
};
