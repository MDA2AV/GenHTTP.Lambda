import type { AgentStep } from '../api';
import type { KitMessages } from '../locales/en';

/*
 * What the agent is doing, in the words of the workspace rather than those of
 * its tools: the newest thing it did that somebody who wants a website would
 * put into words - getting ready, choosing an address, checking it for
 * mistakes. Only the newest, since what it said about it is in the log
 * underneath.
 */

type Words = KitMessages['buildBox']['steps'];

type AppWords = KitMessages['workspace']['steps'];

/** What a build is doing: from nothing to an app with an address. */
export function buildDoing(steps: AgentStep[], words: Words): string | null {
  return newest(steps, (step, before) => {
    switch (step.kind) {
      case 'read':
        // before it has an address of its own, what it reads is an example
        return before.some((s) => s.kind === 'create') ? words.looking : words.examples;
      case 'write':
        return before.some((s) => s.kind === 'write') ? words.improve : words.write;
      default:
        return shared(step, words);
    }
  });
}

/** What a message about an app is doing: answering, changing a setting, or changing the app in a draft. */
export function changeDoing(steps: AgentStep[], words: Words, app: AppWords): string | null {
  return newest(steps, (step) => {
    switch (step.kind) {
      case 'read':
        return app.reading;
      case 'write':
        return app.changing;
      case 'logs':
        return app.errors;
      case 'feature':
        return app.draft;
      case 'update':
        return app.upToDate;
      case 'merge':
        return app.putOnline;
      case 'discard':
        return app.discard;
      case 'analytics':
        return app.visitors;
      case 'source':
        return app.publish;
      default:
        return shared(step, words);
    }
  });
}

function shared(step: AgentStep, words: Words): string | null {
  switch (step.kind) {
    case 'guide':
      return words.guide;
    case 'demos':
      return words.examples;
    case 'create':
      return words.create;
    case 'check':
      return words.check;
    case 'deploy':
      return words.online;
    case 'logs':
      return words.trying;
    case 'data':
      return step.data === 'database'
        ? words.forRecords
        : step.data === 'secrets'
          ? words.forKeys
          : step.data === 'workspace'
            ? words.forFiles
            : null;
    case 'records':
      return words.records;
    case 'secrets':
      return words.keys;
    case 'connectivity':
      return words.services;
    case 'upload':
      return words.addFile;
    case 'delete':
      return words.removeFile;
    case 'list':
      return words.files;
    case 'web':
      return step.site ? words.site(step.site) : null;
    case 'search':
      return words.search;
    case 'attachment':
      return words.attachment;
    case 'showcase':
      return words.showcase;
    case 'file':
      return words.file;
    default:
      return null;
  }
}

function newest(steps: AgentStep[], say: (step: AgentStep, before: AgentStep[]) => string | null): string | null {
  for (let index = steps.length - 1; index >= 0; index--) {
    const text = say(steps[index], steps.slice(0, index));

    if (text != null) {
      return text;
    }
  }

  return null;
}
