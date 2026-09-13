import { useId, useState } from 'react';

export interface Series {
  label: string;
  /** Light and dark steps, each validated against its own surface. */
  color: [light: string, dark: string];
  values: number[];
}

interface Props {
  title: string;
  hint?: string;
  labels: string[];
  series: Series[];
  /** Turns a value into what a reader should see. */
  format: (value: number) => string;
  dark: boolean;
  height?: number;
}

const PAD = { top: 12, right: 12, bottom: 22, left: 56 };

/**
 * A multi series line chart over time, drawn as SVG so it carries no library.
 *
 * One y axis on purpose: two scales in one frame invite a comparison the
 * numbers do not support. Where two measures do not share a scale they get two
 * charts instead.
 */
export function Chart({ title, hint, labels, series, format, dark, height = 190 }: Props) {
  const clip = useId();
  const [hover, setHover] = useState<number | null>(null);
  const [table, setTable] = useState(false);

  const count = labels.length;
  const width = 720;
  const plotW = width - PAD.left - PAD.right;
  const plotH = height - PAD.top - PAD.bottom;

  const all = series.flatMap((s) => s.values);
  const max = Math.max(1, ...all);
  // the floor stays at zero: a memory curve read against a cropped axis makes
  // every wobble look like a leak
  const ticks = [0, max / 2, max];

  const x = (i: number) => (count < 2 ? plotW / 2 : (i / (count - 1)) * plotW);
  const y = (v: number) => plotH - (v / max) * plotH;

  const ink = dark ? '#9aa0a6' : '#5f6368';
  const grid = dark ? '#3c4043' : '#e8eaed';

  if (count === 0) {
    return (
      <figure className="surface p-4">
        <Caption title={title} hint={hint} />
        <p className="py-10 text-center text-sm text-slate-500">No readings yet.</p>
      </figure>
    );
  }

  return (
    <figure className="surface p-4">
      <div className="flex items-start justify-between gap-4">
        <Caption title={title} hint={hint} />
        <button
          type="button"
          onClick={() => setTable((open) => !open)}
          className="shrink-0 text-xs text-accent-500 hover:underline dark:text-accent-400"
        >
          {table ? 'Show chart' : 'Show values'}
        </button>
      </div>

      {/* identity never rests on colour alone: every series is named here */}
      <ul className="mt-3 flex flex-wrap gap-x-4 gap-y-1">
        {series.map((s) => (
          <li key={s.label} className="flex items-center gap-1.5 text-xs text-slate-600 dark:text-slate-400">
            <span className="h-2 w-2 shrink-0" style={{ background: s.color[dark ? 1 : 0] }} aria-hidden="true" />
            {s.label}
            {hover !== null && <span className="font-mono text-slate-500">{format(s.values[hover] ?? 0)}</span>}
          </li>
        ))}
      </ul>

      {table ? (
        <div className="mt-3 max-h-64 overflow-auto">
          <table className="w-full text-left text-xs">
            <thead className="sticky top-0 bg-white text-slate-500 dark:bg-ink-900">
              <tr>
                <th className="py-1 pr-3 font-medium">Time</th>
                {series.map((s) => (
                  <th key={s.label} className="py-1 pr-3 font-medium">{s.label}</th>
                ))}
              </tr>
            </thead>
            <tbody className="font-mono">
              {labels.map((label, i) => (
                <tr key={label + i} className="border-t border-slate-200 dark:border-ink-800">
                  <td className="py-1 pr-3 text-slate-500">{label}</td>
                  {series.map((s) => (
                    <td key={s.label} className="py-1 pr-3">{format(s.values[i] ?? 0)}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <svg
          viewBox={`0 0 ${width} ${height}`}
          className="mt-2 w-full"
          role="img"
          aria-label={`${title}. ${series.map((s) => s.label).join(', ')}.`}
          onMouseLeave={() => setHover(null)}
          onMouseMove={(event) => {
            const box = event.currentTarget.getBoundingClientRect();
            const at = ((event.clientX - box.left) / box.width) * width - PAD.left;
            const index = Math.round((at / plotW) * (count - 1));
            setHover(Math.min(count - 1, Math.max(0, index)));
          }}
        >
          <defs>
            <clipPath id={clip}>
              <rect x={0} y={-4} width={plotW} height={plotH + 4} />
            </clipPath>
          </defs>

          <g transform={`translate(${PAD.left},${PAD.top})`}>
            {ticks.map((tick) => (
              <g key={tick}>
                <line x1={0} x2={plotW} y1={y(tick)} y2={y(tick)} stroke={grid} strokeWidth={1} />
                <text x={-8} y={y(tick) + 3.5} textAnchor="end" fontSize={10} fill={ink}>
                  {format(tick)}
                </text>
              </g>
            ))}

            <g clipPath={`url(#${clip})`}>
              {series.map((s) => (
                <polyline
                  key={s.label}
                  points={s.values.map((v, i) => `${x(i)},${y(v)}`).join(' ')}
                  fill="none"
                  stroke={s.color[dark ? 1 : 0]}
                  strokeWidth={2}
                  strokeLinejoin="round"
                  strokeLinecap="round"
                />
              ))}
            </g>

            {hover !== null && (
              <g>
                <line x1={x(hover)} x2={x(hover)} y1={0} y2={plotH} stroke={ink} strokeWidth={1} strokeDasharray="3 3" />
                {series.map((s) => (
                  <circle
                    key={s.label}
                    cx={x(hover)}
                    cy={y(s.values[hover] ?? 0)}
                    r={4}
                    fill={s.color[dark ? 1 : 0]}
                    stroke={dark ? '#202124' : '#ffffff'}
                    strokeWidth={2}
                  />
                ))}
              </g>
            )}

            <text x={0} y={plotH + 15} fontSize={10} fill={ink}>{labels[0]}</text>
            <text x={plotW} y={plotH + 15} fontSize={10} fill={ink} textAnchor="end">{labels[count - 1]}</text>
            {hover !== null && (
              <text x={x(hover)} y={plotH + 15} fontSize={10} fill={ink} textAnchor="middle" fontWeight={600}>
                {labels[hover]}
              </text>
            )}
          </g>
        </svg>
      )}
    </figure>
  );
}

function Caption({ title, hint }: { title: string; hint?: string }) {
  return (
    <figcaption>
      <h2 className="text-sm font-medium">{title}</h2>
      {hint && <p className="mt-0.5 text-xs text-slate-500">{hint}</p>}
    </figcaption>
  );
}
