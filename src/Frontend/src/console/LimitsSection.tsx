import { useCallback, useEffect, useMemo, useState } from 'react';

import { ApiError, refusedToken, api, type AdminLimits, type TierLimits } from '../api';
import { IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { Pills, Section } from '../control/ui';
import type { Access } from './context';

type Tier = 'free' | 'premium';

const MB = 1024 * 1024;

/** One limit as the form shows it. Bytes are edited in megabytes and sent as bytes. */
interface Limit {
  key: string;
  title: string;
  note: string;
  unit: string;
  bytes?: boolean;
}

/** What each tier has, the same for both: the switch at the top picks whose. */
const TIER: (Limit & { key: keyof TierLimits })[] = [
  { key: 'codeCharacters', title: 'Code', unit: 'characters',
    note: 'C# across every file of a lambda. It guards the time and memory the compiler spends, which the whole server shares.' },
  { key: 'assetBytes', title: 'Assets', unit: 'MB', bytes: true,
    note: 'What a version ships beside its code: the front end, the documentation and the tests. Kept in every version.' },
  { key: 'workspaceBytes', title: 'Workspace', unit: 'MB', bytes: true,
    note: 'The files a lambda saves while it runs. Compiled into the lambda, so a change compiles each one again on its next request.' },
  { key: 'databaseBytes', title: 'Database', unit: 'MB', bytes: true,
    note: 'How large its records may grow. Holds from the next connection; nothing already stored is removed.' },
  { key: 'versions', title: 'Versions kept', unit: 'versions',
    note: 'Older ones are removed when a new one is saved, never the one online.' },
  { key: 'features', title: 'Features open', unit: 'at once',
    note: 'Each holds a copy of the files, the workspace and the database.' },
];

/** Only the free tier's: a premium lambda is kept online and kept. */
const LIFETIME: Limit[] = [
  { key: 'offlineAfterHours', title: 'Offline after', unit: 'hours',
    note: 'Without visits or edits. Its address stops answering until it is used or deployed again.' },
  { key: 'removedAfterHours', title: 'Removed after', unit: 'hours',
    note: 'Without visits or edits: versions, features and data, all of it.' },
];

/** Counted per caller rather than per lambda, so there is no tier to them. */
const EVERYBODY: Limit[] = [
  { key: 'showcaseImageBytes', title: 'Showcase picture', unit: 'MB', bytes: true,
    note: 'Every visitor of the showcase downloads it.' },
  { key: 'requestsPerSecond', title: 'Requests', unit: 'per second',
    note: 'Per client, across every lambda it calls.' },
  { key: 'buildsPerDay', title: 'Builds', unit: 'per day',
    note: 'Builds and changes the agent of this installation makes for one address. Counted in memory.' },
];

type Draft = Record<string, string>;

const show = (value: number, bytes?: boolean) =>
  bytes ? String(Math.round((value / MB) * 100) / 100) : String(value);

function toDraft(limits: AdminLimits): Draft {
  const draft: Draft = {};

  for (const tier of ['free', 'premium'] as const) {
    for (const limit of TIER) {
      draft[`${tier}.${limit.key}`] = show(limits[tier][limit.key], limit.bytes);
    }
  }

  for (const limit of [...LIFETIME, ...EVERYBODY]) {
    draft[limit.key] = show(limits[limit.key as keyof AdminLimits] as number, limit.bytes);
  }

  return draft;
}

/** What a field says, as the number to send, or nothing when it is no number the server takes. */
function parse(text: string, bytes?: boolean): number | null {
  const value = Number(text.trim());

  if (text.trim() === '' || !Number.isFinite(value) || value <= 0) {
    return null;
  }

  if (bytes) {
    return Math.round(value * MB);
  }

  return Number.isInteger(value) ? value : null;
}

function fromDraft(draft: Draft): AdminLimits | null {
  const read = (key: string, bytes?: boolean) => parse(draft[key] ?? '', bytes);

  const tier = (name: Tier): TierLimits | null => {
    const values = TIER.map((limit) => read(`${name}.${limit.key}`, limit.bytes));

    if (values.some((v) => v === null)) return null;

    return Object.fromEntries(TIER.map((limit, i) => [limit.key, values[i]])) as unknown as TierLimits;
  };

  const free = tier('free');
  const premium = tier('premium');
  const rest = [...LIFETIME, ...EVERYBODY].map((limit) => read(limit.key, limit.bytes));

  if (free === null || premium === null || rest.some((v) => v === null)) {
    return null;
  }

  return {
    free,
    premium,
    ...Object.fromEntries([...LIFETIME, ...EVERYBODY].map((limit, i) => [limit.key, rest[i]])),
  } as AdminLimits;
}

/**
 * What a lambda may have in its tier: one form, the same fields for both
 * tiers, and a switch at the top between them.
 */
export function LimitsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [saved, setSaved] = useState<AdminLimits | null>(null);
  const [draft, setDraft] = useState<Draft>({});
  const [tier, setTier] = useState<Tier>('free');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fail = useCallback((problem: unknown, fallback: string) => {
    if (refusedToken(problem)) {
      deny();
    } else {
      setError(fallback);
    }
  }, [deny]);

  useEffect(() => {
    api.admin.limits(token)
      .then((limits) => {
        setSaved(limits);
        setDraft(toDraft(limits));
      })
      .catch((problem) => fail(problem, 'The limits could not be read.'));
  }, [token, fail]);

  const original = useMemo(() => (saved === null ? {} : toDraft(saved)), [saved]);

  const changed = (key: string) => saved !== null && draft[key] !== original[key];
  const dirty = Object.keys(draft).some(changed);
  const tierChanged = (name: Tier) => TIER.some((limit) => changed(`${name}.${limit.key}`));

  const parsed = fromDraft(draft);

  async function save() {
    if (parsed === null || saving) return;

    setSaving(true);

    try {
      const result = await api.admin.saveLimits(token, parsed);

      setSaved(result);
      setDraft(toDraft(result));

      toast('The limits are saved. They apply to the next save, upload, deploy or connection.', 'success');
    } catch (problem) {
      if (refusedToken(problem)) {
        deny();
      } else {
        toast(problem instanceof ApiError ? problem.message : 'The limits could not be saved.', 'error');
      }
    } finally {
      setSaving(false);
    }
  }

  const field = (limit: Limit, key: string) => (
    <Field
      key={key}
      id={`limit-${key.replace('.', '-')}`}
      limit={limit}
      value={draft[key] ?? ''}
      changed={changed(key)}
      valid={parse(draft[key] ?? '', limit.bytes) !== null}
      onChange={(value) => setDraft((current) => ({ ...current, [key]: value }))}
    />
  );

  return (
    <Section
      title="Limits"
      hint="What a lambda may have in its tier. A change applies to what is checked next - a save, an upload, a deploy, a connection to a database - and takes nothing away that a lambda already has. No restart needed."
      pills={saved !== null && (
        <Pills
          label="Tier"
          value={tier}
          onChange={setTier}
          options={(['free', 'premium'] as const).map((name) => ({
            value: name,
            label: (
              <>
                {name === 'free' ? 'Free' : 'Premium'}
                {tierChanged(name) && <span className="h-1.5 w-1.5 rounded-full bg-amber-500" aria-label="changed" />}
              </>
            ),
          }))}
        />
      )}
    >
      {saved === null ? (
        error !== null ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500">
            <IconSpinner /> Reading the limits…
          </div>
        )
      ) : (
        <form
          onSubmit={(event) => {
            event.preventDefault();
            void save();
          }}
          className="space-y-8"
        >
          <section>
            <h2 className="text-sm font-medium">{tier === 'free' ? 'A free lambda' : 'A premium lambda'}</h2>
            <p className="mt-1 text-[13px] text-slate-500">
              {tier === 'free'
                ? 'Every lambda unless it was made premium here, and the demos.'
                : 'Never less than a free lambda may have; saving refuses that.'}
            </p>

            <div className="surface mt-3 divide-y divide-grey-300 dark:divide-ink-800">
              {TIER.map((limit) => field(limit, `${tier}.${limit.key}`))}
            </div>
          </section>

          <section>
            <h2 className="text-sm font-medium">Lifetime</h2>

            {tier === 'free' ? (
              <>
                <p className="mt-1 text-[13px] text-slate-500">When a free lambda that nobody uses goes away.</p>
                <div className="surface mt-3 divide-y divide-grey-300 dark:divide-ink-800">
                  {LIFETIME.map((limit) => field(limit, limit.key))}
                </div>
              </>
            ) : (
              <p className="surface mt-3 p-4 text-[13px] text-slate-500">
                A premium lambda stays online and is kept, however long nobody visits or edits it.
              </p>
            )}
          </section>

          <section>
            <h2 className="text-sm font-medium">Everybody</h2>
            <p className="mt-1 text-[13px] text-slate-500">Counted per visitor rather than per lambda, so the same in both tiers.</p>

            <div className="surface mt-3 divide-y divide-grey-300 dark:divide-ink-800">
              {EVERYBODY.map((limit) => field(limit, limit.key))}
            </div>
          </section>

          <div className="sticky bottom-0 -mx-4 flex flex-wrap items-center gap-3 border-t border-grey-300 bg-white/95 px-4 py-3 backdrop-blur dark:border-ink-800 dark:bg-ink-950/95 md:mx-0 md:px-0">
            <button type="submit" disabled={!dirty || parsed === null || saving} className="btn-primary">
              {saving && <IconSpinner />}
              Save limits
            </button>

            {dirty && (
              <button type="button" onClick={() => setDraft(original)} disabled={saving} className="btn-ghost">
                Discard changes
              </button>
            )}

            <span className="text-[13px] text-slate-500">
              {parsed === null
                ? 'Every limit is a whole number greater than zero; sizes may have decimals.'
                : dirty
                  ? 'Not saved yet.'
                  : 'Everything is saved.'}
            </span>
          </div>
        </form>
      )}
    </Section>
  );
}

function Field({ id, limit, value, changed, valid, onChange }: {
  id: string;
  limit: Limit;
  value: string;
  changed: boolean;
  valid: boolean;
  onChange: (value: string) => void;
}) {
  const hours = limit.unit === 'hours' ? Number(value) : NaN;

  return (
    <div className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center">
      <div className="min-w-0 flex-1">
        <label htmlFor={id} className="flex items-center gap-2 text-[15px] font-medium">
          {limit.title}
          {changed && <span className="h-1.5 w-1.5 rounded-full bg-amber-500" title="Changed, not saved yet" />}
        </label>
        <p className="mt-1 text-[13px] text-slate-500">{limit.note}</p>
      </div>

      <div className="flex shrink-0 items-center gap-2 sm:w-64">
        <input
          id={id}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          inputMode={limit.bytes ? 'decimal' : 'numeric'}
          autoComplete="off"
          aria-invalid={!valid}
          className={`field py-1.5 text-right font-mono text-sm ${valid ? '' : '!border-red-500 !ring-red-500'}`}
        />
        <span className="w-24 shrink-0 text-[13px] text-slate-500">
          {limit.unit}
          {Number.isFinite(hours) && hours >= 48 && (
            <span className="block text-xs text-slate-400">{Math.round((hours / 24) * 10) / 10} days</span>
          )}
        </span>
      </div>
    </div>
  );
}
