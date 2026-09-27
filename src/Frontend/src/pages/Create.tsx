import { useEffect, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';

import { ApiError, api, type KeyStatus, type Platform, type Starter } from '../api';
import { IconExternal, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { usePageMeta } from '../meta';

type Step = 'what' | 'key';

export function Create() {
  usePageMeta({ title: 'Create a Lambda', index: false });

  const navigate = useNavigate();
  const toast = useToast();

  // a demo's own page links here with itself chosen, which skips the choosing
  const [query] = useSearchParams();
  const wanted = query.get('from');

  const [platform, setPlatform] = useState<Platform | null>(null);
  const [step, setStep] = useState<Step>('what');
  const [starter, setStarter] = useState<Starter | null>(null);
  const [key, setKey] = useState('');
  const [accepted, setAccepted] = useState(false);
  const [availability, setAvailability] = useState<KeyStatus | null>(null);
  const [checking, setChecking] = useState(false);
  const [creating, setCreating] = useState(false);

  const check = useRef(0);

  useEffect(() => {
    api.platform().then(setPlatform).catch(() => undefined);
  }, []);

  useEffect(() => {
    if (platform === null || wanted === null || starter !== null) {
      return;
    }

    const match = platform.starters.find((s) => s.id === wanted);

    if (match) {
      setStarter(match);
      setStep('key');
    }
  }, [platform, wanted, starter]);

  // the key is validated while it is typed, but only the newest answer counts
  useEffect(() => {
    const trimmed = key.trim();

    if (trimmed.length === 0) {
      setAvailability(null);
      setChecking(false);
      return;
    }

    setChecking(true);

    const attempt = ++check.current;

    const timer = window.setTimeout(async () => {
      try {
        const result = await api.key(trimmed);

        if (attempt === check.current) {
          setAvailability(result);
        }
      } catch {
        if (attempt === check.current) {
          setAvailability(null);
        }
      } finally {
        if (attempt === check.current) {
          setChecking(false);
        }
      }
    }, 350);

    return () => window.clearTimeout(timer);
  }, [key]);

  const trimmed = key.trim();
  const blocked = trimmed.length > 0 && availability !== null && !availability.available;
  const canSubmit = accepted && !creating && !blocked && !checking && starter !== null;

  function choose(chosen: Starter) {
    setStarter(chosen);
    setStep('key');
  }

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    if (!canSubmit || starter === null) {
      return;
    }

    setCreating(true);

    try {
      const lambda = await api.create(trimmed.length > 0 ? trimmed : null, starter.id);

      navigate(`/editor/${lambda.privateKey}`, { state: { created: true } });
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The lambda could not be created.', 'error');
      setCreating(false);
    }
  }

  // the server leaves out what is null, so a missing demo is how the empty one is told apart
  const demos = platform?.starters.filter((s) => s.demo) ?? [];
  const blank = platform?.starters.find((s) => !s.demo) ?? null;

  return (
    <div className="mx-auto w-full max-w-3xl px-5 py-14 sm:py-20">
      {step === 'what' && (
        <>
          <h1 className="text-2xl font-bold tracking-tight">What would you like to build?</h1>
          <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
            Pick the closest one and you start with a copy of something that already works - yours to change. Or
            start from nothing.
          </p>

          {platform === null ? (
            <div className="surface mt-8 flex items-center gap-2 px-5 py-4 text-sm text-slate-500">
              <IconSpinner /> Loading…
            </div>
          ) : (
            <>
              <section className="mt-8 grid gap-3 sm:grid-cols-2">
                {demos.map((candidate) => (
                  <div
                    key={candidate.id}
                    className="surface flex flex-col transition-colors hover:border-accent-500 dark:hover:border-accent-400"
                  >
                    <button type="button" onClick={() => choose(candidate)} className="flex-1 px-5 pb-2 pt-4 text-left">
                      <span className="block text-[15px] font-medium">{candidate.title}</span>
                      <span className="mt-1 block text-sm leading-relaxed text-slate-600 dark:text-slate-400">
                        {candidate.description}
                      </span>
                    </button>
                    <div className="flex items-center justify-between gap-3 px-5 pb-3.5 pt-1 text-[13px]">
                      <a
                        href={candidate.demo!}
                        target="_blank"
                        rel="noreferrer"
                        className="inline-flex items-center gap-1 text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400"
                      >
                        See it running <IconExternal className="h-3 w-3" />
                      </a>
                      <button
                        type="button"
                        onClick={() => choose(candidate)}
                        className="font-medium text-accent-600 hover:underline dark:text-accent-400"
                      >
                        Start from this
                      </button>
                    </div>
                  </div>
                ))}
              </section>

              {blank && (
                <button
                  type="button"
                  onClick={() => choose(blank)}
                  className="mt-3 block w-full border border-dashed border-slate-300 px-5 py-4 text-left transition-colors hover:border-accent-500 dark:border-ink-700 dark:hover:border-accent-400"
                >
                  <span className="block text-[15px] font-medium">{blank.title}</span>
                  <span className="mt-1 block text-sm text-slate-600 dark:text-slate-400">{blank.description}</span>
                </button>
              )}
            </>
          )}
        </>
      )}

      {step === 'key' && starter !== null && (
        <>
          <h1 className="text-2xl font-bold tracking-tight">Give it an address</h1>
          <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
            {starter.demo ? (
              <>
                <strong className="font-medium text-ink-900 dark:text-slate-100">{starter.title}</strong> - your lambda
                starts as a copy of the demo, and everything in it is yours to change.
              </>
            ) : (
              <>Your lambda starts empty, ready for whatever you have in mind.</>
            )}{' '}
            <button type="button" onClick={() => setStep('what')} className="text-accent-600 hover:underline dark:text-accent-400">
              Pick something else
            </button>
          </p>

          <form onSubmit={submit} className="mt-8 space-y-6">
            <div>
              <label htmlFor="key" className="mb-1.5 block text-sm font-medium">
                Public key
              </label>

              <div className="flex items-center gap-2">
                <span className="shrink-0 font-mono text-sm text-slate-500">/lambda/</span>
                <div className="relative flex-1">
                  <input
                    id="key"
                    value={key}
                    onChange={(event) => setKey(event.target.value)}
                    placeholder="my-first-lambda"
                    autoComplete="off"
                    autoFocus
                    spellCheck={false}
                    className="field font-mono"
                    aria-describedby="key-hint"
                  />
                  {checking && (
                    <IconSpinner className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  )}
                </div>
              </div>

              <p
                id="key-hint"
                className={`mt-2 text-xs ${
                  blocked ? 'text-red-500' : availability?.available ? 'text-emerald-500' : 'text-slate-500'
                }`}
              >
                {blocked
                  ? availability?.reason
                  : availability?.available
                    ? `"${availability.publicKey}" is free.`
                    : 'Lower case letters, digits and dashes. Three characters or more. Leave it empty for a random one.'}
              </p>
            </div>

            <div className="surface px-4 py-3.5">
              <label className="flex cursor-pointer items-start gap-3">
                <input
                  type="checkbox"
                  checked={accepted}
                  onChange={(event) => setAccepted(event.target.checked)}
                  className="mt-0.5 h-4 w-4 shrink-0 accent-accent-500"
                />
                <span className="text-sm font-medium">I accept the terms of service</span>
              </label>

              <p className="mt-3 whitespace-pre-line text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">
                {platform?.terms ?? 'Loading the terms…'}
              </p>

              {/* the short version is what gets read; the full one has to be
                  reachable without losing what has been filled in so far */}
              <a
                href="/terms"
                target="_blank"
                rel="noreferrer"
                className="mt-2 inline-block text-[13px] text-accent-600 hover:underline dark:text-accent-400"
              >
                Read the full terms of service
              </a>
            </div>

            <div className="flex gap-2">
              <button type="button" onClick={() => setStep('what')} className="btn-ghost">
                Back
              </button>
              <button type="submit" disabled={!canSubmit} className="btn-primary flex-1 py-2.5 text-[15px]">
                {creating && <IconSpinner />}
                {creating ? 'Creating…' : 'Create my lambda'}
              </button>
            </div>

            <p className="text-center text-xs text-slate-500">
              The next screen shows your editor link. It is the only way back in, so keep it.
            </p>
          </form>
        </>
      )}
    </div>
  );
}
