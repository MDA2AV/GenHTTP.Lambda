import { shownAddress } from '../address';
import type { LambdaTraffic } from '../api';
import { count } from './format';
import { useShared } from './words';

/**
 * Which way visitors came in - the lambda's own address or its domain - for
 * a lambda that has been reached at more than one since the server started:
 * it was given a domain, or a new key. For every other lambda there is only
 * the one way, and nothing to say about it.
 */
export function Entrances({ traffic, publicUrl }: { traffic: LambdaTraffic; publicUrl: string }) {
  const said = useShared().entrances;
  const entrances = traffic.entrances ?? [];

  if (entrances.length < 2) {
    return null;
  }

  const all = entrances.reduce((total, entrance) => total + entrance.requests, 0);

  return (
    <section className="mt-8">
      <h2 className="text-sm font-medium">{said.title}</h2>
      <ul className="mt-2 divide-y divide-slate-100 text-[13px] dark:divide-ink-850">
        {entrances.map((entrance) => (
          <li key={entrance.domain ?? ''} className="flex items-center gap-3 py-2">
            <span className="min-w-0 flex-1 truncate font-mono">
              {entrance.domain ?? shownAddress(publicUrl)}
            </span>
            <span className="w-24 shrink-0">
              <span className="block h-1 bg-slate-200 dark:bg-ink-800">
                <span className="block h-full bg-accent-500 dark:bg-accent-400" style={{ width: `${(entrance.requests / Math.max(1, all)) * 100}%` }} />
              </span>
            </span>
            <span className="w-16 shrink-0 text-right tabular-nums">{count(entrance.requests)}</span>
          </li>
        ))}
      </ul>
      <p className="mt-2 text-xs text-slate-400">{said.note}</p>
    </section>
  );
}
