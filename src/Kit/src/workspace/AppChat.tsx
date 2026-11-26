import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { absoluteAddress } from '../address';
import { api, ApiError, followRun, isActive, type AgentState, type ChangeJob, type ChangeRequest, type Feature, type LambdaSummary } from '../api';
import { AgentChoice, useAgentChoice } from '../components/AgentChoice';
import { useAttachments } from '../components/useAttachments';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconDots, IconExternal, IconKey, IconSpinner } from '../components/Icons';
import { tagOf, useLanguage, useT } from '../i18n';
import { Link } from '../i18n/links';
import { useOrigin } from '../site';
import { Switch } from '../ui/ui';
import { Composer } from './Composer';
import { Asked, BuildAnswer, ChangeAnswer, type AppHand, type Following } from './Turns';
import { activeState, describe, forget, keepTurn, remember, saidIn, touch, type KnownApp, type Turn } from './store';

/**
 * The conversation about one app: what it is and whether it is online at the
 * top, what waits for its owner underneath, what was said so far, and the
 * box at the foot - for a question, a setting, or a change.
 *
 * Deliberately not the editor. The few things the owner is asked for are
 * here - putting it back online, the keys and passwords it waits for, a draft
 * to try and put online, undoing a change - and everything else is a link to
 * the editor away. The agent does the rest, asked in words.
 *
 * Each message is a change to the agent, through the same door as the
 * editor's Change section, so the operator's switch, the daily allowance and
 * one message at a time hold here as they do there. The conversation is
 * kept in this browser, and the newest turns go along with every message.
 *
 * What the agent does is pushed to the page as it happens, from wherever it
 * was asked; a message that ended with a question is answered by the next,
 * and one that waited for somebody, or ran out of time, goes on with a
 * button rather than being asked again.
 */

/** How many turns go along with a message, the newest ones; the server cuts them further. */
const HISTORY = 8;

/** How long the platform keeps a run - a question older than that has nothing to carry on from. */
const KEPT_RUNS = 29 * 24 * 60 * 60 * 1000;

/** Whether changes go online once they work - remembered with the editor's Change section, which asks the same. */
const ONLINE = 'lambda-change-online';

function rememberedOnline(): boolean {
  try {
    return localStorage.getItem(ONLINE) !== 'no';
  } catch {
    return true;
  }
}

/** What the agent answered to a turn, as the next message carries it along. */
function replyOf(turn: Turn): string | null {
  return turn.kind === 'build'
    ? (turn.built?.question ?? turn.built?.summary ?? turn.built?.error ?? null)
    : (turn.result?.question ?? turn.result?.summary ?? turn.result?.error ?? null);
}

/** Whether a turn ended with a question, which the next message answers. */
const askedIn = (turn: Turn | undefined) => (turn?.kind === 'build' ? turn.built?.asked === true : turn?.result?.asked === true);

export function AppChat({
  app,
  onLeft,
  onWorking,
  onForgotten,
}: {
  app: KnownApp;
  /** How many builds and changes this caller has left today, whenever the agent says. */
  onLeft: (left: number, perDay: number) => void;
  /** Whether the agent is working on this app. */
  onWorking: (working: boolean) => void;
  onForgotten: () => void;
}) {
  const t = useT();
  const said = t.workspace;
  const language = useLanguage();
  const { origin } = useOrigin();
  const key = app.privateKey;

  const [summary, setSummary] = useState<LambdaSummary | null>(null);
  const [gone, setGone] = useState(false);
  const [unreadable, setUnreadable] = useState(false);
  const [features, setFeatures] = useState<Feature[]>([]);
  const [agent, setAgent] = useState<AgentState | null>(null);
  const [following, setFollowing] = useState<Following | null>(null);

  const [prompt, setPrompt] = useState('');
  const [online, setOnline] = useState(rememberedOnline);
  const choice = useAgentChoice(agent?.offer);
  const attachments = useAttachments(agent?.offer.attachments, said.files);
  const [sending, setSending] = useState(false);
  const [stopping, setStopping] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [merging, setMerging] = useState<Feature | null>(null);
  const [, setParams] = useSearchParams();

  /** The draft the next message goes to, where the owner chose one; a new one otherwise. */
  const [target, setTarget] = useState<Feature | null>(null);
  const [forgetting, setForgetting] = useState(false);

  const field = useRef<HTMLTextAreaElement>(null);
  const scroller = useRef<HTMLDivElement>(null);

  /** Bumped to start following again, after a message is sent. */
  const [round, setRound] = useState(0);

  const turns = app.turns;
  const name = summary?.lambda.publicKey ?? app.publicKey;

  // read through a ref, so following the agent does not start over whenever
  // the conversation is written down
  const known = useRef(turns);
  known.current = turns;

  const lost = said.results.lost;

  const hooks = useRef({ onLeft, onWorking });
  hooks.current = { onLeft, onWorking };

  /* ------------------------------------------------ what the app is now */

  const read = useCallback(async () => {
    try {
      const [next, drafts] = await Promise.all([api.summary(key), api.feature.list(key).catch(() => [] as Feature[])]);

      setSummary(next);
      setFeatures(drafts);
      setUnreadable(false);

      describe(key, {
        publicKey: next.lambda.publicKey,
        about: next.documentation.about ?? null,
        online: next.lambda.deployedAt != null && !next.lambda.blocked,
      });
    } catch (error) {
      if (error instanceof ApiError && (error.status === 404 || error.status === 400)) {
        setGone(true);
      } else {
        setUnreadable(true);
      }
    }
  }, [key]);

  useEffect(() => {
    void read();
  }, [read]);

  /* ------------------------------------------------ the agent */

  const take = useCallback(
    (state: AgentState) => {
      setAgent(state);

      if (state.available) {
        hooks.current.onLeft(state.left, state.perDay);
      }

      const job: ChangeJob | null | undefined = state.job;

      // a turn the agent no longer knows - it forgets a job an hour after it
      // was asked, and a newer one replaces it - will not be answered any more
      for (const stale of known.current.filter((turn) => activeState(turn.state) && turn.id !== job?.id)) {
        keepTurn(key, { ...stale, state: 'failed', result: { ok: false, error: lost } });
      }

      if (!job) {
        setFollowing(null);
        hooks.current.onWorking(false);
        return;
      }

      const existing = known.current.find((turn) => turn.id === job.id);
      const active = isActive(job);

      hooks.current.onWorking(active);

      if (active) {
        setFollowing({ run: job, steps: job.steps, received: Date.now(), files: job.files ?? null });

        if (!existing || existing.state !== job.state) {
          keepTurn(key, {
            ...(existing ?? { id: job.id, kind: 'change', at: Date.now(), prompt: job.prompt, attachments: job.attachments ?? undefined }),
            state: job.state,
          });
        }

        return;
      }

      setFollowing(null);

      // ended - here or elsewhere, in the editor's Change section say: it is a
      // turn of this conversation all the same
      if (!existing || existing.state !== job.state || !existing.result) {
        keepTurn(key, {
          ...(existing ?? { id: job.id, kind: 'change', at: Date.now() - job.seconds * 1000, prompt: job.prompt }),
          state: job.state,
          result: job.result ?? null,
          said: saidIn(job.steps),
          seconds: job.seconds,
          attachments: existing?.attachments ?? job.attachments ?? undefined,
          files: job.files ?? undefined,
        });

        // what it did shows in what the app is now: online, its drafts, its keys
        if (existing && activeState(existing.state)) {
          void read();
        }
      }
    },
    [key, read, lost],
  );

  // read once, so a page opened after a reload finds where things stand, and
  // again whenever the page is looked at again; while a message is under way,
  // told what happens as it happens. Listening only then: a browser keeps six
  // connections to a site, which every open tab shares
  useEffect(() => {
    let alive = true;
    let timer: number | undefined;
    let close: (() => void) | undefined;
    let patience = 5000;

    const later = () => {
      if (!alive) return;

      window.clearTimeout(timer);
      timer = window.setTimeout(start, patience);
      patience = Math.min(patience * 2, 60_000);
    };

    const listen = () => {
      close?.();
      close = followRun<AgentState>(api.agent.events(key), 'state', (state) => alive && take(state), (state) => !isActive(state.job), later);
    };

    const start = async () => {
      try {
        const state = await api.agent.state(key);

        if (!alive) return;

        take(state);
        patience = 5000;

        if (isActive(state.job)) listen();
      } catch (error) {
        if (!alive) return;

        // not there, not this browser's to read, or no agent: asking again changes nothing
        if (error instanceof ApiError && [400, 403, 404].includes(error.status)) {
          if (error.status === 404) setAgent(null);
          return;
        }

        later();
      }
    };

    const onLook = () => {
      if (document.visibilityState === 'visible') void start();
    };

    void start();
    document.addEventListener('visibilitychange', onLook);

    return () => {
      alive = false;
      window.clearTimeout(timer);
      close?.();
      document.removeEventListener('visibilitychange', onLook);
    };
  }, [key, take, round]);

  useEffect(() => () => hooks.current.onWorking(false), []);

  /* ------------------------------------------------ following the conversation down */

  // new turns and new lines arrive at the bottom, so the page follows them -
  // unless the owner scrolled up to read something, which it leaves alone
  const following_down = useRef(true);

  useLayoutEffect(() => {
    const box = scroller.current;

    if (box && following_down.current) {
      box.scrollTop = box.scrollHeight;
    }
  }, [turns.length, following?.steps.length, summary]);

  /* ------------------------------------------------ asking */

  const running = turns.some((turn) => activeState(turn.state)) || isActive(agent?.job);
  const blocked = summary?.lambda.blocked != null;
  const offered = agent?.available ?? true;
  const spent = agent != null && agent.available && agent.left <= 0;
  const wanted = prompt.trim();
  const possible = !sending && !running && !spent && offered && !blocked && !gone && choice.ready && !attachments.uploading;
  const ready = wanted.length >= 3 && possible;

  // the newest turn asked something - recently enough that its run is still
  // kept - and the owner did not choose to ask something else: the next
  // message answers it
  const newest = turns.at(-1);
  const [notAnswering, setNotAnswering] = useState<string | null>(null);
  const answering =
    !running && askedIn(newest) && newest && newest.id !== notAnswering && Date.now() - newest.at < KEPT_RUNS ? newest : undefined;

  /**
   * Sends a message to the agent: what was typed - or, going on from a turn
   * that waited or was cut short, nothing but that it should go on.
   */
  async function ask(message: string | undefined, continues?: Turn) {
    setSending(true);
    setProblem(null);

    const history = turns
      .filter((turn) => !activeState(turn.state))
      .slice(-HISTORY)
      .map((turn) => ({ prompt: turn.prompt, reply: replyOf(turn) }));

    const sent = message ? attachments.files : [];

    const change: ChangeRequest = {
      prompt: message,
      deploy: online,
      ...choice.request,
      language,
      history,
      view: 'simple',
      attachments: sent.length > 0 ? sent.map((file) => file.id) : undefined,
      // a message for a draft the owner chose is no answer to a question
      continues: target ? undefined : continues?.id,
      feature: target && features.some((feature) => feature.key === target.key) ? target.key : undefined,
    };

    try {
      const state = await api.agent.start(key, change);

      if (state.job) {
        const turn: Turn = {
          id: state.job.id,
          kind: 'change',
          at: Date.now(),
          prompt: message ?? said.results.goOn,
          state: state.job.state,
          attachments: sent.length > 0 ? sent : undefined,
          continues: change.continues,
        };

        keepTurn(key, turn);

        // what take reads before the page draws again, so it does not make
        // the turn anew from what the agent was asked
        known.current = [...known.current.filter((was) => was.id !== turn.id), turn];
      }

      touch(key);
      following_down.current = true;

      if (message) {
        setPrompt('');
        attachments.clear();
      }

      setTarget(null);
      take(state);

      // under way now: followed as it goes on
      setRound((n) => n + 1);
    } catch (error) {
      setProblem(error instanceof ApiError ? error.message : t.buildBox.failedToStart);
      setRound((n) => n + 1);
    } finally {
      setSending(false);
    }
  }

  function send() {
    if (ready) void ask(wanted, answering);
  }

  function goOn(turn: Turn) {
    if (possible) void ask(undefined, turn);
  }

  async function stop() {
    setStopping(true);

    try {
      take(await api.agent.stop(key));
    } catch (error) {
      setProblem(error instanceof ApiError ? error.message : said.attention.failed);
    } finally {
      setStopping(false);
    }
  }

  /* ------------------------------------------------ doing what the answers offer */

  async function act(action: () => Promise<unknown>) {
    setBusy(true);
    setProblem(null);

    try {
      await action();
    } catch (error) {
      // the platform's refusals are said for developers; what went wrong here is said in the page's words
      setProblem(error instanceof Error && !(error instanceof ApiError) && error.message ? error.message : said.attention.failed);
    } finally {
      setBusy(false);
      await read();
    }
  }

  const hand: AppHand = {
    publicKey: name,
    address: summary ? absoluteAddress(summary.lambda.address) : null,
    active: summary?.lambda.deployedAt ? summary.lambda.activeVersion : null,
    features,
    busy,
    deploy: (version) =>
      void act(async () => {
        const deployed = await api.deploy(key, version);

        if (!deployed.success) {
          throw new Error(said.attention.failed);
        }
      }),
    putOnline: (feature) => setMerging(feature),
    ask: (text, feature) => {
      setTarget(feature);
      setPrompt(text);
      window.requestAnimationFrame(() => field.current?.focus());
    },
    goOn,
    possible,
    open: openApp,
    // what still waits as the app is now: keys entered since, a decision the
    // operator made since, are nothing to wait for any more
    live: (blockers) => {
      const lambda = summary?.lambda;
      const missing = summary?.storage.missingSecrets ?? [];

      const still = (blockers ?? [])
        .map((blocker) =>
          blocker.what === 'keys' ? { ...blocker, names: (blocker.names ?? []).filter((name) => missing.includes(name)) } : blocker)
        .filter((blocker) =>
          blocker.what === 'keys' ? (blocker.names ?? []).length > 0 : !lambda || lambda.connectivity?.status === 'Requested');

      return still.length > 0 ? still : null;
    },
  };

  /** Opens an app a message built beside this one - remembered here, as a build's is. */
  function openApp(publicKey: string) {
    const made = turns.flatMap((turn) => turn.result?.apps ?? []).find((app) => app.publicKey === publicKey);

    if (made) {
      remember({ privateKey: made.privateKey, publicKey: made.publicKey, online: true });
      setParams({ app: made.publicKey });
    }
  }

  const again = (turn: Turn) => {
    setPrompt(turn.prompt);
    window.requestAnimationFrame(() => field.current?.focus());
  };

  // the allowance starts again at midnight UTC, which is some other hour here
  const reset = new Date();
  reset.setUTCHours(24, 0, 0, 0);

  const editorLink = `${origin}/editor/${key}`;

  /* ------------------------------------------------ the page */

  return (
    <>
      <Header
        name={name}
        about={summary?.documentation.about ?? app.about ?? null}
        summary={summary}
        address={hand.address}
        editorLink={editorLink}
        onForget={() => setForgetting(true)}
      />

      <div
        ref={scroller}
        onScroll={(event) => {
          const box = event.currentTarget;
          following_down.current = box.scrollHeight - box.scrollTop - box.clientHeight < 80;
        }}
        className="min-h-0 flex-1 overflow-y-auto"
      >
        <div className="mx-auto w-full max-w-3xl space-y-5 px-4 py-6 sm:px-6">
          {gone && <Notice tone="bad" title={said.gone} text={said.goneText} />}
          {unreadable && !gone && <Notice tone="bad" title={said.unreadable} />}

          {summary && (
            <Attention
              privateKey={key}
              summary={summary}
              features={features}
              // the draft the last message left is offered under its answer already
              offered={turns.at(-1)?.result?.feature ?? null}
              busy={busy}
              onDeploy={() => hand.deploy()}
              onPutOnline={setMerging}
              onBringUp={(feature) => hand.ask(said.attention.bringUpAsk, feature)}
              onSaved={read}
            />
          )}

          {turns.length === 0 && !gone && offered && !blocked && (
            <div className="py-6 text-center">
              <h2 className="text-balance text-xl font-light tracking-tight">{said.firstTitle(name)}</h2>
              <p className="mx-auto mt-2 max-w-md text-sm text-slate-500">{said.firstText}</p>

              <div className="mt-5 flex flex-wrap justify-center gap-2">
                {said.suggestions.map((suggestion) => (
                  <button
                    key={suggestion}
                    type="button"
                    onClick={() => {
                      setPrompt(suggestion);
                      field.current?.focus();
                    }}
                    className="rounded-full border border-slate-200 px-3 py-1 text-[13px] text-slate-600 transition-colors hover:border-slate-300 hover:text-slate-900 dark:border-ink-800 dark:text-slate-400 dark:hover:border-ink-700 dark:hover:text-slate-200"
                  >
                    {suggestion}
                  </button>
                ))}
              </div>
            </div>
          )}

          {turns.map((turn) => (
            <div key={turn.id} className="space-y-4">
              <Asked files={turn.attachments}>{turn.prompt}</Asked>
              {turn.kind === 'build' ? (
                <BuildAnswer turn={turn} newest={turn === newest} live={hand.live} onGoOn={turn === newest && possible ? () => goOn(turn) : undefined} />
              ) : (
                <ChangeAnswer
                  turn={turn}
                  following={agent?.job?.id === turn.id ? following : null}
                  app={hand}
                  onAgain={() => again(turn)}
                  newest={turn === newest}
                />
              )}
            </div>
          ))}
        </div>
      </div>

      <div className="shrink-0 px-4 pb-4 sm:px-6">
        <div className="mx-auto w-full max-w-3xl">
          {problem && (
            <p className="mb-2 flex items-start gap-2 px-1 text-sm text-red-600 dark:text-red-400">
              <IconAlert className="mt-0.5 h-4 w-4 shrink-0" /> {problem}
            </p>
          )}

          {answering && !target && (
            <p className="mb-2 flex flex-wrap items-center gap-x-3 gap-y-1 px-1 text-xs">
              <span className="font-medium text-accent-700 dark:text-accent-400">{said.answering}</span>
              <button type="button" onClick={() => setNotAnswering(answering.id)} className="text-slate-500 underline hover:text-slate-800 dark:hover:text-slate-200">
                {said.askSomethingElse}
              </button>
            </p>
          )}

          {target && (
            <p className="mb-2 flex flex-wrap items-center gap-x-3 gap-y-1 px-1 text-xs text-slate-500">
              <span className="font-medium text-accent-700 dark:text-accent-400">{said.attention.inDraft(target.name)}</span>
              <button type="button" onClick={() => setTarget(null)} className="underline hover:text-slate-800 dark:hover:text-slate-200">
                {said.attention.leaveDraft}
              </button>
            </p>
          )}

          {agent && !agent.available ? (
            <div className="surface flex flex-wrap items-center justify-between gap-3 px-4 py-3 text-sm text-slate-600 dark:text-slate-400">
              <span>{said.noChanges}</span>
              <span className="flex gap-2">
                <a href={editorLink} target="_blank" rel="noreferrer" className="btn-ghost !px-3 !py-1 text-[13px]">
                  {said.openEditor}
                </a>
                <Link to="/ship" className="btn-ghost !px-3 !py-1 text-[13px]">
                  {said.connectOwn}
                </Link>
              </span>
            </div>
          ) : (
            <Composer
              value={prompt}
              onChange={setPrompt}
              onSend={send}
              onStop={agent?.offer.cancels === false ? undefined : stop}
              placeholder={answering ? said.placeholderAnswer : said.placeholderApp(name)}
              label={answering ? said.placeholderAnswer : said.placeholderApp(name)}
              ready={ready}
              busy={sending}
              running={running}
              stopping={stopping}
              disabled={blocked || gone}
              field={field}
              attachments={agent?.offer.attachments ? attachments : null}
              footer={
                <>
                  <span className="flex items-center gap-2" title={online ? said.goOnlineOn : said.goOnlineOff}>
                    <Switch
                      on={online}
                      labelledBy="workspace-online"
                      onToggle={() =>
                        setOnline((was) => {
                          try {
                            localStorage.setItem(ONLINE, was ? 'no' : 'yes');
                          } catch {
                            // a browser that keeps nothing asks every time, which is fine
                          }

                          return !was;
                        })
                      }
                    />
                    <span id="workspace-online">{said.goOnline}</span>
                  </span>

                  <AgentChoice choice={choice} />

                  {agent?.available && (
                    <span className={`ml-auto tabular-nums ${spent ? 'text-amber-600 dark:text-amber-400' : ''}`}>
                      {spent
                        ? said.noneLeft(reset.toLocaleTimeString(tagOf(language), { hour: '2-digit', minute: '2-digit' }))
                        : said.left(agent.left, agent.perDay)}
                    </span>
                  )}
                </>
              }
            />
          )}
        </div>
      </div>

      <Dialog
        title={said.attention.putOnlineTitle}
        open={merging !== null}
        onClose={() => setMerging(null)}
        footer={
          <>
            <button type="button" className="btn-ghost" onClick={() => setMerging(null)}>
              {said.cancel}
            </button>
            <button
              type="button"
              className="btn-primary"
              onClick={() => {
                const feature = merging;
                setMerging(null);

                if (feature) {
                  void act(async () => {
                    const merged = await api.feature.merge(key, feature.key, { deploy: true });

                    if (!merged.merged) {
                      throw new Error(said.attention.putOnlineFailed);
                    }
                  });
                }
              }}
            >
              {said.attention.putOnline}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.attention.putOnlineText}</p>
      </Dialog>

      <Dialog
        title={said.forgetTitle}
        open={forgetting}
        onClose={() => setForgetting(false)}
        footer={
          <>
            <button type="button" className="btn-ghost" onClick={() => setForgetting(false)}>
              {said.cancel}
            </button>
            <button
              type="button"
              className="btn-danger"
              onClick={() => {
                setForgetting(false);
                forget(key);
                onForgotten();
              }}
            >
              {said.forget}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.forgetText}</p>
      </Dialog>
    </>
  );
}

/* ------------------------------------------------------------ the top */

/** The app's name, what it is, whether it is online, and the way out to it and to its editor. */
function Header({
  name,
  about,
  summary,
  address,
  editorLink,
  onForget,
}: {
  name: string;
  about: string | null;
  summary: LambdaSummary | null;
  address: string | null;
  editorLink: string;
  onForget: () => void;
}) {
  const t = useT();
  const said = t.workspace;
  const [menu, setMenu] = useState(false);
  const [copied, setCopied] = useState(false);
  const holder = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!menu) return;

    const onPointer = (event: PointerEvent) => {
      if (!holder.current?.contains(event.target as Node)) setMenu(false);
    };
    const onKey = (event: KeyboardEvent) => event.key === 'Escape' && setMenu(false);

    document.addEventListener('pointerdown', onPointer);
    document.addEventListener('keydown', onKey);

    return () => {
      document.removeEventListener('pointerdown', onPointer);
      document.removeEventListener('keydown', onKey);
    };
  }, [menu]);

  const lambda = summary?.lambda;
  const state = !lambda ? null : lambda.blocked ? 'blocked' : lambda.deployedAt ? 'online' : 'offline';

  return (
    <header className="shrink-0 border-b border-slate-200 px-4 py-3 dark:border-ink-800 sm:px-6">
      <div className="mx-auto flex w-full max-w-3xl items-center gap-3">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <h1 className="truncate text-[17px] font-semibold">{name}</h1>
            {state && (
              <span
                className={`inline-flex shrink-0 items-center gap-1.5 rounded-full px-2 py-px text-[11px] font-medium ${
                  state === 'online'
                    ? 'bg-emerald-500/10 text-emerald-700 dark:text-emerald-400'
                    : state === 'blocked'
                      ? 'bg-red-500/10 text-red-600 dark:text-red-400'
                      : 'bg-slate-400/15 text-slate-600 dark:text-slate-400'
                }`}
              >
                <span className={`h-1.5 w-1.5 rounded-full ${state === 'online' ? 'bg-emerald-500' : state === 'blocked' ? 'bg-red-500' : 'bg-slate-400'}`} />
                {state === 'online' ? said.online : state === 'blocked' ? said.blocked : said.offline}
              </span>
            )}
          </div>
          {about && <p className="mt-0.5 truncate text-sm text-slate-500">{about}</p>}
        </div>

        {address && state === 'online' && (
          <a href={address} target="_blank" rel="noreferrer" className="btn-primary shrink-0 !px-3.5 !py-1.5 text-[13px]">
            <IconExternal className="h-3.5 w-3.5" />
            <span className="hidden sm:inline">{said.openApp}</span>
          </a>
        )}

        <div ref={holder} className="relative shrink-0">
          <button
            type="button"
            onClick={() => setMenu((was) => !was)}
            aria-expanded={menu}
            aria-label={said.more}
            title={said.more}
            className="btn-ghost !px-2 !py-1.5"
          >
            <IconDots className="h-4 w-4" />
          </button>

          {menu && (
            <div className="surface absolute right-0 top-full z-30 mt-1 w-60 py-1 shadow-lg">
              {address && (
                <a href={address} target="_blank" rel="noreferrer" className="block px-4 py-2 text-sm hover:bg-slate-100 dark:hover:bg-ink-850">
                  {said.openApp}
                </a>
              )}
              <a href={editorLink} target="_blank" rel="noreferrer" className="block px-4 py-2 text-sm hover:bg-slate-100 dark:hover:bg-ink-850">
                {said.openEditor}
              </a>
              <button
                type="button"
                onClick={() => {
                  navigator.clipboard?.writeText(editorLink);
                  setCopied(true);
                  window.setTimeout(() => setCopied(false), 1600);
                }}
                className="block w-full px-4 py-2 text-left text-sm hover:bg-slate-100 dark:hover:bg-ink-850"
              >
                {copied ? t.common.copied : said.copyEditor}
              </button>
              <div className="my-1 border-t border-slate-200 dark:border-ink-800" />
              <button
                type="button"
                onClick={() => {
                  setMenu(false);
                  onForget();
                }}
                className="block w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-500/10 dark:text-red-400"
              >
                {said.forget}
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}

/* ------------------------------------------------------------ what waits */

/**
 * What waits for the owner, above the conversation, and only while it does:
 * an app that is offline, the keys and passwords its code waits for, a
 * draft to try. Blocked, nothing else is offered.
 */
function Attention({
  privateKey,
  summary,
  features,
  offered,
  busy,
  onDeploy,
  onPutOnline,
  onBringUp,
  onSaved,
}: {
  privateKey: string;
  summary: LambdaSummary;
  features: Feature[];
  /** The draft offered under the last answer already. */
  offered: string | null;
  busy: boolean;
  onDeploy: () => void;
  onPutOnline: (feature: Feature) => void;
  onBringUp: (feature: Feature) => void;
  onSaved: () => Promise<void>;
}) {
  const said = useT().workspace.attention;
  const lambda = summary.lambda;

  if (lambda.blocked) {
    return <Notice tone="bad" title={said.blockedTitle} text={lambda.blockReason ?? said.blockedText} />;
  }

  const missing = summary.storage.missingSecrets ?? [];
  const drafts = features.filter((feature) => feature.key !== offered && feature.online);
  const offline = !lambda.deployedAt && lambda.latestVersion != null;

  if (!offline && missing.length === 0 && drafts.length === 0) {
    return null;
  }

  return (
    <div className="space-y-3">
      {offline && (
        <Notice tone="warn" title={said.offlineTitle} text={said.offlineText}>
          <button type="button" onClick={onDeploy} disabled={busy} className="btn-primary !px-4 !py-1.5 text-[13px]">
            {busy && <IconSpinner className="h-3.5 w-3.5" />}
            {said.putBack}
          </button>
        </Notice>
      )}

      {missing.length > 0 && (
        <Notice tone="warn" title={said.keysTitle(missing.length)} text={said.keysText}>
          <div className="w-full space-y-2">
            {missing.map((name) => (
              <KeyField key={name} privateKey={privateKey} name={name} enabled={summary.storage.secretsEnabled} onSaved={onSaved} />
            ))}
          </div>
        </Notice>
      )}

      {drafts.map((draft) => (
        <Notice key={draft.key} tone="ready" title={said.draftTitle(draft.name)} text={draft.mergeable ? said.draftText : said.outOfDate}>
          <a href={absoluteAddress(draft.previewPath)} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
            <IconExternal className="h-3.5 w-3.5" />
            {said.tryIt}
          </a>
          {draft.mergeable ? (
            <button type="button" onClick={() => onPutOnline(draft)} disabled={busy} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              {said.putOnline}
            </button>
          ) : (
            <button type="button" onClick={() => onBringUp(draft)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              {said.bringUp}
            </button>
          )}
        </Notice>
      ))}
    </div>
  );
}

/**
 * One key or password the app's code waits for: its name, and a field for
 * its value. The value goes to the platform and nowhere else - never to the
 * agent - and is not shown again; switching secrets on is part of entering
 * the first, as in the editor's simple view.
 */
function KeyField({ privateKey, name, enabled, onSaved }: { privateKey: string; name: string; enabled: boolean; onSaved: () => Promise<void> }) {
  const said = useT().workspace.attention;
  const [value, setValue] = useState('');
  const [saving, setSaving] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);

  async function save() {
    if (!value) return;

    setSaving(true);
    setProblem(null);

    try {
      if (!enabled) {
        await api.enableData(privateKey, 'secrets');
      }

      await api.secrets.set(privateKey, name, value);
      setValue('');
      await onSaved();
    } catch {
      setProblem(said.failed);
    } finally {
      setSaving(false);
    }
  }

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        void save();
      }}
      className="flex flex-wrap items-center gap-2"
    >
      <label htmlFor={`key-${name}`} className="flex min-w-[9rem] items-center gap-1.5 font-mono text-[13px]">
        <IconKey className="h-3.5 w-3.5 text-slate-400" />
        {name}
      </label>
      <input
        id={`key-${name}`}
        type="password"
        value={value}
        onChange={(event) => setValue(event.target.value)}
        placeholder={said.value}
        autoComplete="off"
        className="field min-w-0 flex-1 !py-1.5"
      />
      <button type="submit" disabled={saving || !value} className="btn-primary !px-4 !py-1.5 text-[13px]">
        {saving && <IconSpinner className="h-3.5 w-3.5" />}
        {said.save}
      </button>
      {problem && <p className="w-full text-xs text-red-600 dark:text-red-400">{problem}</p>}
    </form>
  );
}

/** Something the owner should know or do, in a line, a sentence and what to do about it. */
function Notice({ tone, title, text, children }: { tone: 'warn' | 'bad' | 'ready'; title: string; text?: string; children?: React.ReactNode }) {
  const look = {
    warn: 'border-amber-500/40 bg-amber-500/5',
    bad: 'border-red-500/40 bg-red-500/5',
    ready: 'border-accent-500/40 bg-accent-500/5',
  }[tone];

  return (
    <div className={`border px-4 py-3 ${look}`}>
      <p className="text-sm font-semibold">{title}</p>
      {text && <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{text}</p>}
      {children && <div className="mt-3 flex flex-wrap items-center gap-2">{children}</div>}
    </div>
  );
}
