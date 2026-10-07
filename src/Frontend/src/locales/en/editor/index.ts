import { change } from './change';
import { clone } from './clone';
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

/**
 * The words of the editor in English, fetched with the editor. Every other
 * language's catalog is typed as this one.
 *
 * A file for each section of the editor, in every language: a change to one
 * section touches that file, and its translator reads that section alone.
 */
export const editor = {
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
  clone,
  tabs,
};

export type EditorMessages = typeof editor;
