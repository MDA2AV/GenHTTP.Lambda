import { useState } from 'react';
import { Link } from 'react-router-dom';

import { absoluteAddress } from '../address';
import { isActive, isDemo } from '../api';
import { IconAlert, IconCheck, IconCopy, IconExternal, IconKey, IconPlay, IconSpark, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { count, span } from './format';
import { Names } from './SecretsPanel';
import { AgentMark, Ago, Figure, Section, Sparkline } from './ui';

/**
 * The overview of the simple view, for somebody who had the app built and
 * wants it to do something else - not to look after a server.
 *
 * Only what tells them how things stand: is it there and where, is anything
 * wrong, what changed last and is anybody using it. Asking for a change, the
 * drafts and the history have sections of their own, as they do in the full
 * view; the button at the top is the way to the one they come for. Nothing
 * here says version, deployment, file or log: those are the full view's words
 * for the same things.
 */
export function SimpleOverview({ control }: { control: Control }) {
  const said = useEditorT().simple;
  const { lambda, agent } = control;
  const demo = isDemo(lambda.tier);
  const problems = (control.summary?.recentProblems.length ?? 0) > 0;

  // the frame says so above every section while a change is under way
  const ask = !demo && (agent.state?.available ?? false) && !isActive(agent.state?.job);

  return (
    <Section
      title={said.title}
      actions={ask && (
        <button type="button" onClick={() => control.askAgent()} className="btn-primary !px-4 !py-1.5 text-[13px]">
          <IconSpark className="h-3.5 w-3.5" />
          {said.askCta}
        </button>
      )}
    >
      <div className="max-w-4xl space-y-8">
        <About control={control} />

        <Status control={control} />

        {!demo && <NeedsKey control={control} />}

        {problems && !demo && <Problems control={control} />}

        <div className="grid gap-8 lg:grid-cols-2">
          <Latest control={control} />
          <Today control={control} />
        </div>
      </div>
    </Section>
  );
}

/* ------------------------------------------------------------ what it is */

/**
 * What the app is for, in the sentence or two the agent opens its
 * description with - worth reading on the first screen, since it is what the
 * agent understood from what was asked.
 */
function About({ control }: { control: Control }) {
  const said = useEditorT().simple;
  const about = control.summary?.documentation.about;

  if (!about) {
    return null;
  }

  return (
    <p className="text-[15px] leading-relaxed text-slate-700 dark:text-slate-300">
      {about}{' '}
      <Link to={`/editor/${control.privateKey}/docs`} className="whitespace-nowrap text-[13px] text-accent-500 hover:underline">{said.aboutMore}</Link>
    </p>
  );
}

/* ------------------------------------------------------------ is it there */

/** Whether the app is online and where, with the one thing to do about it. */
function Status({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.simple;
  const { lambda, summary, busy } = control;

  const live = lambda.activeVersion != null;
  const address = absoluteAddress(lambda.address);
  const shown = address.replace(/^https?:\/\//, '').replace(/\/$/, '');
  const latest = lambda.latestVersion;

  // saved, but not what visitors get - left so by a change that could not
  // go online, or by somebody who went back. Not while the agent works: what
  // it saved last may be something it is still fixing, and it puts online
  // what it finishes
  const pending = live && latest != null && latest !== lambda.activeVersion && !isActive(control.agent.state?.job) && !isDemo(lambda.tier);

  const since = summary?.activation ? span(summary.activation.seconds, t.shared) : null;

  return (
    <section className="surface">
      <div className="p-5">
        <div className="flex items-center gap-3">
          <Beacon live={live} />
          <h2 className="text-[17px] font-semibold tracking-tight">{live ? said.online : said.offline}</h2>
        </div>

        <p className="mt-1.5 pl-6 text-sm text-slate-600 dark:text-slate-400">
          {live
            ? since
              ? said.onlineFor(<strong className="font-medium text-ink-900 dark:text-slate-100">{since}</strong>)
              : said.onlineNow
            : said.offlineText}
        </p>

        <div className="mt-5 flex flex-wrap items-center gap-2 pl-6">
          <a
            href={live ? address : undefined}
            target="_blank"
            rel="noreferrer"
            title={address}
            className={`min-w-0 max-w-full truncate text-[15px] ${
              live ? 'font-medium text-accent-600 hover:underline dark:text-accent-400' : 'text-slate-400'
            }`}
          >
            {shown}
          </a>
          <Copy value={address} />

          <span className="ml-auto flex flex-wrap gap-2">
            {live ? (
              <a href={address} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
                <IconExternal className="h-3.5 w-3.5" />
                {said.openApp}
              </a>
            ) : (
              !isDemo(lambda.tier) && (
                <button type="button" onClick={() => control.deploy()} disabled={busy !== null} className="btn-primary !px-4 !py-1.5 text-[13px]">
                  {busy === 'deploy' ? <IconSpinner className="h-3.5 w-3.5" /> : <IconPlay className="h-3.5 w-3.5" />}
                  {said.putOnline}
                </button>
              )
            )}
          </span>
        </div>
      </div>

      {pending && (
        <div className="flex flex-wrap items-center gap-x-4 gap-y-3 border-t border-amber-500/30 bg-amber-500/5 px-5 py-3.5">
          <div className="min-w-0 flex-1">
            <p className="text-sm font-medium">{said.pending}</p>
            <p className="mt-0.5 text-[13px] text-slate-600 dark:text-slate-400">{said.pendingText}</p>
          </div>
          <button type="button" onClick={() => control.deploy(latest)} disabled={busy !== null} className="btn-primary !px-4 !py-1.5 text-[13px]">
            {busy === 'deploy' ? <IconSpinner className="h-3.5 w-3.5" /> : <IconPlay className="h-3.5 w-3.5" />}
            {said.putOnline}
          </button>
        </div>
      )}
    </section>
  );
}

/** A dot that breathes while the app is online, and sits still while it is not. */
function Beacon({ live }: { live: boolean }) {
  return (
    <span className="relative flex h-3 w-3 shrink-0" aria-hidden="true">
      {live && <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-60 motion-reduce:animate-none" />}
      <span className={`relative inline-flex h-3 w-3 rounded-full ${live ? 'bg-emerald-500' : 'bg-slate-400'}`} />
    </span>
  );
}

function Copy({ value }: { value: string }) {
  const said = useEditorT().simple;
  const [copied, setCopied] = useState(false);

  return (
    <button
      type="button"
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(value);
          setCopied(true);
          window.setTimeout(() => setCopied(false), 1500);
        } catch {
          // a clipboard that refuses is not worth an error
        }
      }}
      className="p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
      title={copied ? said.copied : said.copy}
      aria-label={said.copy}
    >
      {copied ? <IconCheck className="h-4 w-4 text-emerald-500" /> : <IconCopy className="h-4 w-4" />}
    </button>
  );
}

/* ------------------------------------------------------------ is it wrong */

/**
 * That the app reads a key nobody has given it yet - an API key the agent
 * wrote the code for and could not know. Only the owner can give it, so it
 * is said first, with the button that asks for it.
 */
function NeedsKey({ control }: { control: Control }) {
  const said = useEditorT().simple;
  const missing = control.summary?.storage.missingSecrets ?? [];

  if (missing.length === 0) {
    return null;
  }

  return (
    <section className="flex flex-wrap items-center gap-x-4 gap-y-3 border border-amber-500/40 bg-amber-500/5 px-5 py-4">
      <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-amber-500/15 text-amber-600 dark:text-amber-400">
        <IconKey className="h-[18px] w-[18px]" />
      </span>
      <div className="min-w-0 flex-1">
        <h2 className="text-sm font-medium">{said.needsKey(missing.length)}</h2>
        <p className="mt-0.5 text-[13px] text-slate-600 dark:text-slate-400">{said.needsKeyText(<Names names={missing} />)}</p>
      </div>
      <button type="button" onClick={() => control.openData('secrets', missing[0])} className="btn-primary !px-4 !py-1.5 text-[13px]">
        {said.enterKey}
      </button>
    </section>
  );
}

/**
 * That visitors ran into errors, without the errors: what they say is for
 * whoever fixes them, and here that is the agent.
 */
function Problems({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.simple;
  const agent = control.agent.state?.available ?? false;
  const running = isActive(control.agent.state?.job);

  return (
    <section className="flex flex-wrap items-center gap-x-4 gap-y-3 border-l-2 border-red-500 pl-4">
      <div className="min-w-0 flex-1">
        <h2 className="flex items-center gap-2 text-sm font-medium">
          <IconAlert className="h-4 w-4 shrink-0 text-red-500" />
          {said.problems}
        </h2>
        {agent && <p className="mt-1 text-[13px] text-slate-600 dark:text-slate-400">{said.problemsText}</p>}
      </div>

      {agent && !running && (
        <button type="button" onClick={() => control.askAgent(undefined, t.change.fixLog)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
          <IconSpark className="h-3.5 w-3.5" />
          {said.fix}
        </button>
      )}
    </section>
  );
}

/* ------------------------------------------------------------ what changed */

/** The last change, in its own words, with the way to all the others. */
function Latest({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.simple;
  const latest = control.versions[0];

  if (!latest) {
    return null;
  }

  return (
    <section className="flex flex-col">
      <div className="flex items-center justify-between gap-3">
        <h2 className="text-sm font-medium">{said.latest}</h2>
        <Link to={`/editor/${control.privateKey}/history`} className="text-[13px] text-accent-500 hover:underline">{said.allChanges}</Link>
      </div>

      <div className="surface mt-3 flex-1 p-5">
        <p className={`break-words text-[15px] leading-snug ${latest.change ? '' : 'text-slate-500'}`}>
          {latest.change ?? (latest.origin === 'template' ? said.created : said.noNote)}
        </p>
        <p className="mt-2 flex flex-wrap items-center gap-2 text-[13px] text-slate-500">
          <Ago at={latest.created} />
          <AgentMark origin={latest.origin} />
          {latest.version === control.lambda.activeVersion && <OnlineMark />}
        </p>
      </div>
    </section>
  );
}

/** That this is what visitors get. */
export function OnlineMark() {
  const said = useEditorT().simple;

  return (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 px-2 py-px text-[11px] font-medium text-emerald-600 dark:text-emerald-400">
      <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" aria-hidden="true" />
      {said.isOnline}
    </span>
  );
}

/* ------------------------------------------------------------ is it used */

function Today({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.simple;
  const traffic = control.summary?.traffic;

  if (!traffic) {
    return null;
  }

  return (
    <section className="flex flex-col">
      <h2 className="text-sm font-medium">{said.activity}</h2>
      <div className="surface mt-3 grid flex-1 grid-cols-2 gap-6 p-5">
        <div>
          <Figure value={count(traffic.dayRequests)} label={said.hits} title={said.hitsTitle} />
          <div className="mt-2"><Sparkline values={traffic.hourly} label={t.summary.hourly} /></div>
        </div>
        <Figure value={traffic.lastSeen ? <Ago at={traffic.lastSeen} /> : said.noVisit} label={said.lastVisit} />
      </div>
    </section>
  );
}
