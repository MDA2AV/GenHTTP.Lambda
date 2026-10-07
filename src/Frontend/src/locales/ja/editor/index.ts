import type { EditorMessages } from '../../en/editor';
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
 * エディターの日本語テキスト。
 *
 * 日本語の文は単語の間にスペースを入れないため、JSX のテキストは途中で改行せず、
 * 改行するのは {…} の直前か直後だけにしています。文をつなぐ部分（先頭や末尾の
 * スペース）も、日本語では付けません。
 */
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
  clone,
  tabs,
};
