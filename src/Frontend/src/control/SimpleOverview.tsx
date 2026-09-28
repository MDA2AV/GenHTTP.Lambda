import { useLayoutEffect, useRef, useState } from 'react';

import { absoluteAddress } from '../address';
import { isActive, isDemo, type VersionInfo } from '../api';
import { CopyField } from '../components/CopyField';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconCheck, IconCopy, IconDraft, IconExternal, IconPlay, IconSpark, IconSpinner } from '../components/Icons';
import { tagOf, useEditorT, useLanguage } from '../i18n';
import { Allowance, OwnAgent, remembered, useDraft } from './ChangeTab';
import type { Control } from './context';
import { count, span } from './format';
import { AgentMark, Ago, Figure, Section, Sparkline } from './ui';

/**
 * The overview of the simple view, for somebody who had the app built and
 * wants it to do something else - not to look after a server.
 *
 * In the order they would ask: is it there and where, is anything wrong,
 * what should be different, is anybody using it, and what changed so far -
 * with a way back to any earlier state. Nothing here says version,
 * deployment, file or log: those are the full view's words for the same
 * things.
 */
export function SimpleOverview({ control }: { control: Control }) {
  const said = useEditorT().simple;
  const { lambda } = control;
  const demo = isDemo(lambda.tier);
  const problems = (control.summary?.recentProblems.length ?? 0) > 0;

  return (
    <Section title={said.title}>
      <div className="max-w-3xl space-y-10">
        <Status control={control} />

        {problems && !demo && <Problems control={control} />}

        {!demo && <Ask control={control} />}

        {!demo && control.features.length > 0 && <Drafts control={control} />}

        <Today control={control} />

        <History control={control} />

        {!demo && (
          <section>
            <h2 className="text-sm font-medium">{said.keepTitle}</h2>
            <p className="mt-1 max-w-xl text-[13px] text-slate-500">{said.keepText}</p>
            <div className="mt-3 max-w-xl">
              <CopyField value={`${window.location.origin}${lambda.editorPath}`} />
            </div>
          </section>
        )}
      </div>
    </Section>
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

/* ------------------------------------------------------------ what next */

/**
 * The box of the Change section, brought to where the owner lands: what they
 * type is sent from here, and the Change section opens to show it happen.
 * It shares its draft with the box there, so nothing typed is lost going
 * back and forth.
 */
function Ask({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.simple;
  const language = useLanguage();
  const { agent } = control;
  const state = agent.state;

  const [prompt, setPrompt] = useDraft(control.lambda.publicKey);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const field = useRef<HTMLTextAreaElement>(null);

  // grows with what is in it, like the box in the Change section
  useLayoutEffect(() => {
    const box = field.current;

    if (box) {
      box.style.height = 'auto';
      box.style.height = `${Math.min(box.scrollHeight, 240)}px`;
    }
  }, [prompt]);

  // not known yet, or under way - the frame says so above every section
  if (!state || isActive(state.job)) {
    return null;
  }

  if (!state.available) {
    return (
      <section>
        <h2 className="text-[15px] font-medium">{said.ownTitle}</h2>
        <OwnAgent control={control} open />
      </section>
    );
  }

  const online = remembered();
  const spent = state.left <= 0;
  const wanted = prompt.trim();
  const ready = wanted.length >= 3 && !sending && !spent;

  const reset = new Date();
  reset.setUTCHours(24, 0, 0, 0);

  async function send() {
    if (!ready) {
      return;
    }

    setSending(true);
    setError(null);

    const refused = await agent.start({ prompt: wanted, deploy: online, language });

    setSending(false);

    if (refused) {
      setError(refused);
    } else {
      setPrompt('');
      control.askAgent();
    }
  }

  return (
    <section>
      <label htmlFor="simple-ask" className="block text-[15px] font-medium">{said.askTitle}</label>

      <div className="surface mt-3 focus-within:border-accent-500 focus-within:ring-1 focus-within:ring-accent-500 dark:focus-within:border-accent-400 dark:focus-within:ring-accent-400">
        <textarea
          id="simple-ask"
          ref={field}
          value={prompt}
          onChange={(event) => setPrompt(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' && (event.metaKey || event.ctrlKey)) {
              event.preventDefault();
              void send();
            }
          }}
          rows={2}
          maxLength={2000}
          disabled={sending || spent}
          placeholder={said.askPlaceholder}
          className="block min-h-[4.5rem] w-full resize-none bg-transparent px-4 py-3 text-[15px] leading-relaxed outline-none placeholder:text-slate-400 disabled:opacity-60 dark:placeholder:text-slate-600"
        />

        <div className="flex flex-wrap items-center gap-x-4 gap-y-2 border-t border-slate-200 px-3 py-2 dark:border-ink-800">
          <span className="text-xs text-slate-500">{online ? said.askOnline : said.askDraft}</span>
          <span className="ml-auto"><Allowance state={state} /></span>
          <button type="button" onClick={send} disabled={!ready} className="btn-primary !px-4 !py-1.5 text-[13px]">
            {sending ? <IconSpinner className="h-3.5 w-3.5" /> : <IconSpark className="h-3.5 w-3.5" />}
            {sending ? said.asking : said.ask}
          </button>
        </div>
      </div>

      {spent && (
        <p className="mt-3 text-sm text-amber-600 dark:text-amber-400">
          {t.change.noneLeft(reset.toLocaleTimeString(tagOf(language), { hour: '2-digit', minute: '2-digit' }))}
        </p>
      )}

      {error && (
        <p className="mt-3 flex items-start gap-2 text-sm text-red-500">
          <IconAlert className="mt-0.5 h-4 w-4 shrink-0" /> {error}
        </p>
      )}

      {wanted === '' && !spent && (
        <div className="mt-3 flex flex-wrap gap-1.5">
          {t.change.ideas.map((idea) => (
            <button
              key={idea}
              type="button"
              onClick={() => {
                setPrompt(idea);
                field.current?.focus();
              }}
              className="rounded-full border border-slate-200 px-3 py-1 text-[13px] text-slate-600 transition-colors hover:border-slate-300 hover:text-slate-900 dark:border-ink-800 dark:text-slate-400 dark:hover:border-ink-700 dark:hover:text-slate-200"
            >
              {idea}
            </button>
          ))}
        </div>
      )}
    </section>
  );
}

/**
 * The drafts waiting to be tried - left by a change that was not to go online
 * by itself - with the two things done with one: trying it, and opening it.
 */
function Drafts({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.simple;

  return (
    <section>
      <h2 className="text-sm font-medium">{said.drafts}</h2>
      <p className="mt-1 max-w-xl text-[13px] text-slate-500">{said.draftsHint}</p>

      <ul className="surface mt-3 divide-y divide-slate-200 dark:divide-ink-800">
        {control.features.map((feature) => (
          <li key={feature.key} className="flex flex-wrap items-center gap-x-4 gap-y-2 px-4 py-3">
            <IconDraft className="h-4 w-4 shrink-0 text-slate-400" />
            <div className="min-w-0 flex-1">
              <p className="truncate text-[15px] font-medium">{feature.name}</p>
              {feature.change && <p className="mt-0.5 truncate text-[13px] text-slate-500" title={feature.change}>{feature.change}</p>}
            </div>
            <span className="flex shrink-0 items-center gap-3">
              {feature.online && (
                <a href={absoluteAddress(feature.previewPath)} target="_blank" rel="noreferrer"
                   className="inline-flex items-center gap-1 text-[13px] font-medium text-accent-600 hover:underline dark:text-accent-400">
                  <IconExternal className="h-3.5 w-3.5" />
                  {t.features.openPreview}
                </a>
              )}
              <button type="button" onClick={() => control.openFeature(feature.key)} className="btn-ghost !px-3 !py-1 text-[13px]">
                {said.openDraft}
              </button>
            </span>
          </li>
        ))}
      </ul>
    </section>
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
    <section>
      <h2 className="text-sm font-medium">{said.activity}</h2>
      <div className="surface mt-3 grid grid-cols-2 gap-6 p-5">
        <div>
          <Figure value={count(traffic.dayRequests)} label={said.hits} title={said.hitsTitle} />
          <div className="mt-2"><Sparkline values={traffic.hourly} label={t.summary.hourly} /></div>
        </div>
        <Figure value={traffic.lastSeen ? <Ago at={traffic.lastSeen} /> : said.noVisit} label={said.lastVisit} />
      </div>
    </section>
  );
}

/* ------------------------------------------------------------ what changed */

const SHOWN = 4;

/**
 * Every version, as the change it was: what it did and when, which one
 * visitors get, and a way back to any other. Going back is deploying an
 * older version, which is all it ever was - but said as what it does.
 */
function History({ control }: { control: Control }) {
  const said = useEditorT().simple;
  const { versions, lambda, busy } = control;
  const [all, setAll] = useState(false);
  const [asking, setAsking] = useState<VersionInfo | null>(null);

  if (versions.length === 0) {
    return null;
  }

  const demo = isDemo(lambda.tier);
  const active = lambda.activeVersion;
  const shown = all ? versions : versions.slice(0, SHOWN);

  // newer than what is online - or nothing is online - is putting it online;
  // older is going back
  const forward = (version: number) => active == null || version > active;

  return (
    <section>
      <h2 className="text-sm font-medium">{said.history}</h2>
      <p className="mt-1 max-w-xl text-[13px] text-slate-500">{said.historyHint}</p>

      <ol className="mt-4">
        {shown.map((version, index) => {
          const online = version.version === active;
          const last = index === shown.length - 1;

          return (
            <li key={version.version} className="relative flex gap-4 pb-5 last:pb-0">
              {/* the line that ties one change to the one before it */}
              {!last && <span className="absolute left-[5px] top-4 h-full w-px bg-slate-200 dark:bg-ink-800" aria-hidden="true" />}

              <span
                className={`relative mt-1.5 h-[11px] w-[11px] shrink-0 rounded-full border-2 ${
                  online ? 'border-emerald-500 bg-emerald-500' : 'border-slate-300 bg-white dark:border-ink-700 dark:bg-ink-950'
                }`}
                aria-hidden="true"
              />

              <div className="min-w-0 flex-1">
                <p className={`break-words text-[15px] leading-snug ${version.change ? '' : 'text-slate-500'}`}>
                  {version.change ?? (version.origin === 'template' ? said.created : said.noNote)}
                </p>
                <p className="mt-1 flex flex-wrap items-center gap-2 text-[13px] text-slate-500">
                  <Ago at={version.created} />
                  <AgentMark origin={version.origin} />
                  {online && (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 px-2 py-px text-[11px] font-medium text-emerald-600 dark:text-emerald-400">
                      <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" aria-hidden="true" />
                      {said.isOnline}
                    </span>
                  )}
                </p>
              </div>

              {!online && !demo && (
                <button
                  type="button"
                  onClick={() => setAsking(version)}
                  disabled={busy !== null}
                  className="mt-0.5 shrink-0 self-start text-[13px] font-medium text-accent-600 hover:underline disabled:opacity-50 dark:text-accent-400"
                >
                  {forward(version.version) ? said.putThisOnline : said.goBack}
                </button>
              )}
            </li>
          );
        })}
      </ol>

      {versions.length > SHOWN && (
        <button
          type="button"
          onClick={() => setAll((was) => !was)}
          className="mt-4 pl-[27px] text-[13px] text-slate-500 hover:text-slate-800 hover:underline dark:hover:text-slate-200"
        >
          {all ? said.fewer : said.more(versions.length - SHOWN)}
        </button>
      )}

      <Dialog
        title={asking && forward(asking.version) ? said.putOnlineTitle : said.goBackTitle}
        open={asking !== null}
        onClose={() => setAsking(null)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(null)} className="btn-ghost">
              {said.cancel}
            </button>
            <button
              type="button"
              className="btn-primary"
              onClick={() => {
                const version = asking?.version;

                setAsking(null);

                if (version != null) {
                  void control.deploy(version);
                }
              }}
            >
              {asking && forward(asking.version) ? said.putOnlineConfirm : said.goBackConfirm}
            </button>
          </>
        }
      >
        {asking?.change && <p className="border-l-2 border-slate-300 pl-3 text-slate-700 dark:border-ink-700 dark:text-slate-300">{asking.change}</p>}
        <p className="text-slate-600 dark:text-slate-400">{asking && forward(asking.version) ? said.putOnlineText : said.goBackText}</p>
      </Dialog>
    </section>
  );
}
