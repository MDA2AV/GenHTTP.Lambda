import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { ApiError, api, type Availability, type Platform, type Template, type TemplateGroup } from '../api';
import { IconCheck, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';

type Step = 'kind' | 'template' | 'key';

export function Create() {
  const navigate = useNavigate();
  const toast = useToast();

  const [platform, setPlatform] = useState<Platform | null>(null);
  const [step, setStep] = useState<Step>('kind');
  const [group, setGroup] = useState<TemplateGroup | null>(null);
  const [template, setTemplate] = useState<Template | null>(null);
  const [key, setKey] = useState('');
  const [accepted, setAccepted] = useState(false);
  const [availability, setAvailability] = useState<Availability | null>(null);
  const [checking, setChecking] = useState(false);
  const [creating, setCreating] = useState(false);

  const check = useRef(0);

  useEffect(() => {
    api.platform().then(setPlatform).catch(() => undefined);
  }, []);

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
        const result = await api.checkKey(trimmed);

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
  const canSubmit = accepted && !creating && !blocked && !checking && template !== null;

  function pickGroup(chosen: TemplateGroup) {
    setGroup(chosen);
    setTemplate(chosen.templates[0] ?? null);
    setStep('template');
  }

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    if (!canSubmit) {
      return;
    }

    setCreating(true);

    try {
      const lambda = await api.create(trimmed.length > 0 ? trimmed : null, template?.id ?? null);

      navigate(`/editor/${lambda.privateKey}`, { state: { created: true } });
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The lambda could not be created.', 'error');
      setCreating(false);
    }
  }

  return (
    <div className="mx-auto w-full max-w-2xl px-5 py-14 sm:py-20">
      <h1 className="text-2xl font-bold tracking-tight">Create a lambda</h1>
      <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
        Start from an example, then pick the key it will be hosted at.
      </p>

      <Steps step={step} group={group} template={template} onGo={setStep} />

      {step === 'kind' && (
        <section className="mt-8 space-y-3">
          {platform === null ? (
            <Loading />
          ) : (
            platform.templates.map((candidate) => (
              <button
                key={candidate.id}
                type="button"
                onClick={() => pickGroup(candidate)}
                className="surface block w-full px-5 py-4 text-left transition-colors hover:border-accent-500 dark:hover:border-accent-400"
              >
                <span className="block text-[15px] font-medium">{candidate.name}</span>
                <span className="mt-1 block text-sm text-slate-600 dark:text-slate-400">{candidate.description}</span>
              </button>
            ))
          )}
        </section>
      )}

      {step === 'template' && group !== null && (
        <section className="mt-8 space-y-3">
          {group.templates.map((candidate) => {
            const active = candidate.id === template?.id;

            return (
              <div
                key={candidate.id}
                className={`surface ${active ? 'border-accent-500 dark:border-accent-400' : ''}`}
              >
                <button
                  type="button"
                  onClick={() => setTemplate(candidate)}
                  className="flex w-full items-start gap-3 px-5 py-4 text-left"
                >
                  <span
                    className={`mt-0.5 flex h-4 w-4 shrink-0 items-center justify-center border ${
                      active
                        ? 'border-accent-500 bg-accent-500 text-white dark:border-accent-400 dark:bg-accent-400 dark:text-ink-950'
                        : 'border-slate-400 dark:border-ink-700'
                    }`}
                  >
                    {active && <IconCheck className="h-3 w-3" />}
                  </span>

                  <span>
                    <span className="block text-[15px] font-medium">{candidate.name}</span>
                    <span className="mt-1 block text-sm text-slate-600 dark:text-slate-400">
                      {candidate.description}
                    </span>
                  </span>
                </button>

                {active && (
                  <pre className="max-h-56 overflow-auto border-t border-slate-200 bg-slate-50 px-5 py-3 font-mono text-[12px] leading-relaxed text-slate-700 dark:border-ink-800 dark:bg-ink-950 dark:text-slate-300">
                    {candidate.code}
                  </pre>
                )}
              </div>
            );
          })}

          <div className="flex gap-2 pt-2">
            <button type="button" onClick={() => setStep('kind')} className="btn-ghost">
              Back
            </button>
            <button type="button" onClick={() => setStep('key')} disabled={template === null} className="btn-primary">
              Continue
            </button>
          </div>
        </section>
      )}

      {step === 'key' && (
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
          </div>

          <div className="flex gap-2">
            <button type="button" onClick={() => setStep('template')} className="btn-ghost">
              Back
            </button>
            <button type="submit" disabled={!canSubmit} className="btn-primary flex-1 py-2.5 text-[15px]">
              {creating && <IconSpinner />}
              {creating ? 'Creating…' : `Create from "${template?.name ?? 'template'}"`}
            </button>
          </div>

          <p className="text-center text-xs text-slate-500">
            The next screen shows your editor link. It is the only way back in, so keep it.
          </p>
        </form>
      )}
    </div>
  );
}

/** The three choices, with the ones already made offered as a way back. */
function Steps({
  step,
  group,
  template,
  onGo,
}: {
  step: Step;
  group: TemplateGroup | null;
  template: Template | null;
  onGo: (step: Step) => void;
}) {
  const items: { id: Step; label: string; value: string | null }[] = [
    { id: 'kind', label: 'What it does', value: group?.name ?? null },
    { id: 'template', label: 'Example', value: template?.name ?? null },
    { id: 'key', label: 'Key', value: null },
  ];

  return (
    <ol className="mt-7 flex items-stretch border border-slate-200 dark:border-ink-800">
      {items.map((item, index) => {
        const done = item.value !== null && item.id !== step;
        const active = item.id === step;

        return (
          <li key={item.id} className={`flex-1 ${index > 0 ? 'border-l border-slate-200 dark:border-ink-800' : ''}`}>
            <button
              type="button"
              disabled={!done}
              onClick={() => onGo(item.id)}
              className={`block w-full px-4 py-2.5 text-left ${
                active ? 'bg-accent-500/[0.06] dark:bg-accent-400/[0.08]' : ''
              } ${done ? 'hover:bg-slate-50 dark:hover:bg-ink-850' : ''}`}
            >
              <span
                className={`block text-[11px] uppercase tracking-wide ${
                  active ? 'text-accent-500 dark:text-accent-400' : 'text-slate-500'
                }`}
              >
                {index + 1}. {item.label}
              </span>
              <span className="mt-0.5 block truncate text-sm font-medium">
                {item.value ?? (active ? 'Choosing…' : '—')}
              </span>
            </button>
          </li>
        );
      })}
    </ol>
  );
}

function Loading() {
  return (
    <div className="surface flex items-center gap-2 px-5 py-4 text-sm text-slate-500">
      <IconSpinner /> Loading the examples…
    </div>
  );
}
