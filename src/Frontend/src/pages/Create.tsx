import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { ApiError, api, type Availability, type Platform } from '../api';
import { IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';

export function Create() {
  const navigate = useNavigate();
  const toast = useToast();

  const [platform, setPlatform] = useState<Platform | null>(null);
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
  const canSubmit = accepted && !creating && !blocked && !checking;

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    if (!canSubmit) {
      return;
    }

    setCreating(true);

    try {
      const lambda = await api.create(trimmed.length > 0 ? trimmed : null);

      navigate(`/editor/${lambda.privateKey}`, { state: { created: true } });
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The lambda could not be created.', 'error');
      setCreating(false);
    }
  }

  return (
    <div className="mx-auto w-full max-w-xl px-5 py-14 sm:py-20">
      <h1 className="text-2xl font-bold tracking-tight">Create a lambda</h1>
      <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
        Pick the key your handler will be hosted at. Leave it empty and you get a short random one.
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
                : 'Lower case letters, digits and dashes. Three characters or more.'}
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

        <button type="submit" disabled={!canSubmit} className="btn-primary w-full py-2.5 text-[15px]">
          {creating && <IconSpinner />}
          {creating ? 'Creating…' : 'Create lambda'}
        </button>

        <p className="text-center text-xs text-slate-500">
          The next screen shows your editor link. It is the only way back in, so keep it.
        </p>
      </form>
    </div>
  );
}
