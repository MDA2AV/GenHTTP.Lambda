import type { EditorMessages } from '../../en/editor';
import { change } from './change';
import { code } from './code';
import { context } from './context';
import { data } from './data';
import { deployments } from './deployments';
import { domain } from './domain';
import { features } from './features';
import { files } from './files';
import { frame } from './frame';
import { logs } from './logs';
import { openSource } from './openSource';
import { shared } from './shared';
import { showcase } from './showcase';
import { simple } from './simple';
import { stats } from './stats';
import { summary } from './summary';
import { tabs } from './tabs';
import { versions } from './versions';

/** I testi dell’editor in italiano. */
export const editor: EditorMessages = {
  shared,
  frame,
  simple,
  change,
  summary,
  files,
  context,
  data,
  features,
  versions,
  deployments,
  stats,
  logs,
  showcase,
  openSource,
  domain,
  code,
  tabs,
};
