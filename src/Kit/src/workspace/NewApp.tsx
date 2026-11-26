import { useEffect, useRef, useState, type ReactNode } from 'react';

import { api, ApiError, followRun, isOver, type AgentOffer, type BuildProgress, type BuildResult } from '../api';
import { AgentChoice, useAgentChoice } from '../components/AgentChoice';
import { useAttachments } from '../components/useAttachments';
import { useLanguage, useT } from '../i18n';
import { Link } from '../i18n/links';
import { Composer } from './Composer';
import { Asked, BuildAnswer, type Following } from './Turns';
import { keepTurn, pend, pendingOf, remember, saidIn, useWorkspace, type KnownApp, type Turn } from './store';

/**
 * A new app: the first message, before there is anything to talk about.
 *
 * Before anything is asked, this is the page that creates an app - what
 * the site says about it (intro), the box in the middle of it, ideas to start
 * from, and what the site answers for whoever wants to know first (after) -
 * all in the markup as it is prerendered, for whoever finds the page by
 * searching for it.
 *
 * Once asked, it is the first turn of a conversation: the build follows,
 * and when it is done the app is remembered in this browser and its
 * conversation goes on where the build ended. The build is remembered while
 * it runs too, so a reload - or a tab closed and opened again within the hour
 * the agent keeps it - finds it again, and with it the editor key it makes.
 *
 * A build that asks something before it makes an app stays here: the next
 * message answers it, and the build carries on from there.
 */
export function NewApp({
  intro,
  after,
  offered,
  offer,
  onBuilt,
  onLeft,
  allowance,
}: {
  intro: ReactNode;
  after?: ReactNode;
  /** Whether building is offered here; until the server has said, it is. */
  offered: boolean;
  /** The models and efforts the operator offers; nothing until the server has said. */
  offer: AgentOffer | null;
  /** Called with the app once it is built and remembered. */
  onBuilt: (app: KnownApp) => void;
  /** How many builds and changes this caller has left today, as the build's start said. */
  onLeft: (left: number) => void;
  /** What is left today, where it is known: from the agent, or the last build's start. */
  allowance?: { left: number; perDay: number } | null;
}) {
  const t = useT();
  const said = t.buildBox;
  const words = t.workspace;
  const kept = useWorkspace();

  const language = useLanguage();

  const [prompt, setPrompt] = useState('');
  const [turn, setTurn] = useState<Turn | null>(null);
  /** The turns before it, while a build asks before it makes an app. */
  const [earlier, setEarlier] = useState<Turn[]>([]);
  const before = useRef(earlier);
  before.current = earlier;
  const [following, setFollowing] = useState<Following | null>(null);
  const choice = useAgentChoice(offer);
  const attachments = useAttachments(offer?.attachments, words.files);
  const [sending, setSending] = useState(false);

  const field = useRef<HTMLTextAreaElement>(null);
  const listening = useRef<(() => void) | null>(null);
  const hooks = useRef({ onBuilt });
  hooks.current = { onBuilt };

  // a build asked for before a reload, which the agent may still be on
  const pending = pendingOf(kept);

  useEffect(() => {
    if (pending && turn === null) {
      setTurn({ id: pending.id, kind: 'build', at: pending.at, prompt: pending.prompt, state: 'queued' });
      follow(pending.id, pending.prompt, pending.at);
    }
    // only a build found on arrival is taken up; one started here is followed already
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pending?.id]);

  useEffect(() => () => listening.current?.(), []);

  /**
   * Follows a build as the platform pushes it, until it ended. Where the
   * stream cannot be had, it is read once: forgotten, ended, or still under
   * way - and then followed again a moment later.
   */
  function follow(id: string, asked: string, at: number, files?: Turn['attachments']) {
    listening.current?.();

    const take = (job: BuildProgress) => {
      setFollowing({
        run: { state: job.state, waiting: job.waiting ?? 0, seconds: job.seconds ?? 0, limit: job.limit ?? null, wait: job.wait ?? null },
        steps: job.steps ?? [],
        received: Date.now(),
        files: job.files ?? null,
      });

      if (isOver(job.state)) {
        finish(id, asked, at, job, files ?? job.attachments ?? undefined);
      } else {
        setTurn((was) => (was && was.state !== job.state ? { ...was, state: job.state as Turn['state'] } : was));
      }
    };

    const gone = async () => {
      try {
        const job = await api.builds.progress(id);

        take(job);

        if (!isOver(job.state)) {
          window.setTimeout(() => follow(id, asked, at, files), 3000);
        }
      } catch (error) {
        // forgotten: nothing more will come of it
        if (error instanceof ApiError && error.status === 404) {
          pend(null);
          setTurn(null);
          setFollowing(null);
        } else {
          window.setTimeout(() => follow(id, asked, at, files), 5000);
        }
      }
    };

    listening.current = followRun<BuildProgress>(api.builds.events(id), 'progress', take, (job) => isOver(job.state), () => void gone());
  }

  function finish(id: string, asked: string, at: number, job: BuildProgress, files?: Turn['attachments']) {
    const result: BuildResult = job.result ?? { ok: false, error: said.noAnswer };
    const ended: Turn = {
      id,
      kind: 'build',
      at,
      prompt: asked,
      state: job.state === 'done' && (result.ok || result.asked === true) ? 'done' : 'failed',
      built: result,
      said: saidIn(job.steps ?? []),
      seconds: job.seconds ?? 0,
      attachments: files,
      files: job.files ?? undefined,
    };

    pend(null);
    setFollowing(null);

    // made an app - asking something or not: the conversation goes on there
    if (ended.state === 'done' && result.privateKey && result.publicKey) {
      const app = { privateKey: result.privateKey, publicKey: result.publicKey, online: result.deployed !== false };

      // the questions asked on the way to it are its conversation too
      const turns = [...before.current, ended];

      remember(app, turns[0]);
      turns.slice(1).forEach((next) => keepTurn(app.privateKey, next));
      setTurn(null);
      setEarlier([]);
      hooks.current.onBuilt({ ...app, added: Date.now(), opened: Date.now(), turns });
      return;
    }

    setTurn(ended);
  }

  // a build that asked something and made no app yet: the next message answers it
  const answering = turn?.built?.asked === true && !turn.built.privateKey ? turn : null;

  async function start() {
    const asked = prompt.trim();

    if (asked.length < 3 || sending || attachments.uploading) return;

    setSending(true);

    try {
      const sent = attachments.files;
      const started = await api.builds.start(asked, {
        ...choice.request,
        attachments: attachments.ids.length > 0 ? attachments.ids : undefined,
        language,
        continues: answering?.id,
        history: answering ? [{ prompt: answering.prompt, reply: answering.built?.question ?? null }] : undefined,
      });
      const at = Date.now();

      pend({ id: started.id, prompt: asked, at, model: started.model ?? undefined });

      if (started.left != null) {
        onLeft(started.left);
      }

      setPrompt('');
      attachments.clear();

      if (answering) {
        setEarlier((was) => [...was, answering]);
      }

      setTurn({ id: started.id, kind: 'build', at, prompt: asked, state: 'queued', attachments: sent, continues: answering?.id });
      follow(started.id, asked, at, sent);
    } catch (error) {
      setTurn({
        id: `refused-${Date.now()}`,
        kind: 'build',
        at: Date.now(),
        prompt: asked,
        state: 'failed',
        built: { ok: false, error: error instanceof ApiError ? error.message : said.failedToStart },
      });
    } finally {
      setSending(false);
    }
  }

  const ready = prompt.trim().length >= 3 && !sending && !attachments.uploading && choice.ready;
  const running = turn?.state === 'queued' || turn?.state === 'running';

  const again = () => {
    if (!turn) return;

    setPrompt(turn.prompt);
    setTurn(null);
    setEarlier([]);
    window.requestAnimationFrame(() => field.current?.focus());
  };

  const choosing = choice.options.some((option) => option.values.length > 1);

  const footer =
    choosing || allowance ? (
      <>
        <AgentChoice choice={choice} />
        {allowance && <span className="ml-auto tabular-nums">{words.left(allowance.left, allowance.perDay)}</span>}
      </>
    ) : undefined;

  /* ---------------------------------------------- before anything was asked */

  if (!turn) {
    return (
      <div className="min-h-0 flex-1 overflow-y-auto">
        <div className="mx-auto w-full max-w-2xl px-4 pb-10 pt-12 sm:px-6 sm:pt-20">
          {intro}

          {offered ? (
            <div className="mt-10">
              <Composer
                value={prompt}
                onChange={setPrompt}
                onSend={start}
                placeholder={said.placeholder}
                label={said.placeholder}
                ready={ready}
                busy={sending}
                large
                field={field}
                footer={footer}
                attachments={offer?.attachments ? attachments : null}
              />

              {choice.guarded && <p className="mt-2 px-1 text-xs text-slate-500">{said.unlimited}</p>}

              <div className="mt-5 flex flex-wrap justify-center gap-2">
                {said.ideas.map((idea) => (
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
            </div>
          ) : (
            <div className="surface mt-10 p-5 text-center">
              <p className="text-sm text-slate-600 dark:text-slate-400">{words.noBuilds}</p>
              <Link to="/ship" className="btn-primary mt-4">
                {words.connectOwn}
              </Link>
            </div>
          )}

          {after}
        </div>
      </div>
    );
  }

  /* ---------------------------------------------- the first turn */

  return (
    <>
      <div className="min-h-0 flex-1 overflow-y-auto">
        <div className="mx-auto w-full max-w-3xl space-y-5 px-4 py-8 sm:px-6">
          {earlier.map((before) => (
            <div key={before.id} className="space-y-5">
              <Asked files={before.attachments}>{before.prompt}</Asked>
              <BuildAnswer turn={before} />
            </div>
          ))}
          <Asked files={turn.attachments}>{turn.prompt}</Asked>
          <BuildAnswer turn={turn} following={following} onAgain={again} />
        </div>
      </div>

      <div className="shrink-0 px-4 pb-4 sm:px-6">
        <div className="mx-auto w-full max-w-3xl">
          {answering && <p className="mb-2 px-1 text-xs font-medium text-accent-700 dark:text-accent-400">{words.answering}</p>}
          <Composer
            value={prompt}
            onChange={setPrompt}
            onSend={start}
            placeholder={answering ? words.placeholderAnswer : said.placeholder}
            label={answering ? words.placeholderAnswer : said.placeholder}
            ready={ready && !running}
            busy={sending}
            disabled={running || !offered}
            field={field}
            footer={footer}
            attachments={offer?.attachments ? attachments : null}
          />
        </div>
      </div>
    </>
  );
}
