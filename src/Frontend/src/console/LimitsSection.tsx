import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';

import { ApiError, refusedToken, api, type AdminLimits, type TierLimits } from '../api';
import { IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { Section } from '../control/ui';
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

/** What each tier has, the same for both: a column each. */
const TIER: (Limit & { key: keyof TierLimits })[] = [
  { key: 'buildBytes', title: 'Build', unit: 'MB', bytes: true,
    note: 'A version: its code - the C#, and the documentation, the tests and whatever else is kept with it - and its resources, together. Kept in every version. The C# is compiled, so this also bounds what the compiler spends, which the whole server shares.' },
  { key: 'dataBytes', title: 'Data', unit: 'MB', bytes: true,
    note: 'What a lambda keeps while it runs: its database and its workspace, together. Holds from the next write; nothing already stored is removed.' },
  { key: 'versions', title: 'Versions kept', unit: '',
    note: 'Older ones are removed when a new one is saved, never the one online.' },
  { key: 'features', title: 'Features open at once', unit: '',
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
  { key: 'requestsPerSecond', title: 'Requests per second', unit: '',
    note: 'Per client, across every lambda it calls.' },
  { key: 'buildsPerDay', title: 'Builds per day', unit: '',
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
 * What a lambda may have: the limits of the tiers as a table with a column per
 * tier, and the limits that have no tier apart from it, in one column.
 */
export function LimitsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [saved, setSaved] = useState<AdminLimits | null>(null);
  const [draft, setDraft] = useState<Draft>({});
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

  const input = (limit: Limit, key: string, column?: string) => (
    <Input
      id={`limit-${key.replace('.', '-')}`}
      label={column ? `${limit.title}, ${column}` : limit.title}
      column={column}
      unit={limit.unit}
      value={draft[key] ?? ''}
      changed={changed(key)}
      valid={parse(draft[key] ?? '', limit.bytes) !== null}
      onChange={(value) => setDraft((current) => ({ ...current, [key]: value }))}
    />
  );

  return (
    <Section
      title="Limits"
      hint="What a lambda may have. A change applies to what is checked next - a save, an upload, a deploy, a connection to a database - and takes nothing away that a lambda already has. No restart needed."
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
          className="space-y-10"
        >
          <section>
            <h2 className="text-sm font-medium">Per tier</h2>
            <p className="mt-1 text-[13px] text-slate-500">
              What one lambda may have. A demo is held to the free tier; premium never to less than free.
            </p>

            <div className="surface mt-3">
              <div className="hidden border-b border-grey-300 px-4 py-2.5 text-xs font-medium uppercase tracking-wide text-slate-500 dark:border-ink-800 sm:grid sm:grid-cols-[1fr_10rem_10rem] sm:gap-4">
                <span>Limit</span>
                <span>Free</span>
                <span>Premium</span>
              </div>

              <div className="divide-y divide-grey-300 dark:divide-ink-800">
                {TIER.map((limit) => (
                  <Row key={limit.key} limit={limit}>
                    {input(limit, `free.${limit.key}`, 'Free')}
                    {input(limit, `premium.${limit.key}`, 'Premium')}
                  </Row>
                ))}

                {LIFETIME.map((limit) => (
                  <Row key={limit.key} limit={limit}>
                    {input(limit, limit.key, 'Free')}
                    <Kept />
                  </Row>
                ))}
              </div>
            </div>
          </section>

          <section>
            <h2 className="text-sm font-medium">For everybody</h2>
            <p className="mt-1 text-[13px] text-slate-500">
              Counted per visitor rather than per lambda, so there is no tier to them.
            </p>

            <div className="surface mt-3 divide-y divide-grey-300 dark:divide-ink-800">
              {EVERYBODY.map((limit) => (
                <Row key={limit.key} limit={limit} single>
                  {input(limit, limit.key)}
                </Row>
              ))}
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

/**
 * One limit: what it is on the left, and on the right a value per tier, or a
 * single one in the second column for a limit that has no tier.
 */
function Row({ limit, single = false, children }: { limit: Limit; single?: boolean; children: ReactNode }) {
  return (
    <div className="grid gap-3 p-4 sm:grid-cols-[1fr_10rem_10rem] sm:items-center sm:gap-4">
      <div className="min-w-0">
        <p className="text-[15px] font-medium">{limit.title}</p>
        <p className="mt-1 text-[13px] text-slate-500">{limit.note}</p>
      </div>

      <div className={`grid gap-3 sm:contents ${single ? 'grid-cols-1' : 'grid-cols-2'}`}>{children}</div>
    </div>
  );
}

/** Where a premium lambda has no limit: it stays online and is kept. */
function Kept() {
  return (
    <div>
      <span className="mb-1 block text-xs text-slate-500 sm:hidden">Premium</span>
      <p className="py-1.5 text-[13px] text-slate-500">never - kept online</p>
    </div>
  );
}

function Input({ id, label, column, unit, value, changed, valid, onChange }: {
  id: string;
  label: string;
  /** The tier the column is for, said above the field where the columns have no header - on a phone. */
  column?: string;
  unit: string;
  value: string;
  changed: boolean;
  valid: boolean;
  onChange: (value: string) => void;
}) {
  const hours = unit === 'hours' ? Number(value) : NaN;

  return (
    <div className="min-w-0">
      {column && <label htmlFor={id} className="mb-1 block text-xs text-slate-500 sm:hidden">{column}</label>}

      <div className="relative">
        <input
          id={id}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          inputMode={unit === 'MB' ? 'decimal' : 'numeric'}
          autoComplete="off"
          aria-label={label}
          aria-invalid={!valid}
          style={{ paddingRight: `${(unit === '' ? 0.75 : unit.length * 0.5 + 1.4) + (changed ? 0.75 : 0)}rem` }}
          className={`field py-1.5 text-right font-mono text-sm ${valid ? '' : '!border-red-500 !ring-red-500'}`}
        />
        <span className="pointer-events-none absolute inset-y-0 right-3 flex items-center gap-1.5 text-xs text-slate-400">
          {changed && <span className="h-1.5 w-1.5 rounded-full bg-amber-500" title="Changed, not saved yet" />}
          {unit}
        </span>
      </div>

      {Number.isFinite(hours) && hours >= 48 && (
        <span className="mt-1 block text-right text-xs text-slate-400">{Math.round((hours / 24) * 10) / 10} days</span>
      )}
    </div>
  );
}
