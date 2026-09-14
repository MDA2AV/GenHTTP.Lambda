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
  /**
   * "line" for a measure that exists at every instant, "step" for one that
   * counts what happened during an interval - drawing those sloped would
   * claim the value moved smoothly between two readings, which it did not.
   * "stacked" for parts of a whole, where the outline is the total and each
   * band is a share of it.
   */
  shape?: 'line' | 'step' | 'stacked';
}

const PAD = { top: 12, right: 12, bottom: 22, left: 56 };

/**
 * A multi series line chart over time, drawn as SVG so it carries no library.
 *
 * One y axis on purpose: two scales in one frame invite a comparison the
 * numbers do not support. Where two measures do not share a scale they get two
 * charts instead.
 */
export function Chart({ title, hint, labels, series, format, dark, height = 190, shape = 'line' }: Props) {
  const clip = useId();
  const [hover, setHover] = useState<number | null>(null);
  const [table, setTable] = useState(false);

  const count = labels.length;
  const width = 720;
  const plotW = width - PAD.left - PAD.right;
  const plotH = height - PAD.top - PAD.bottom;

  // stacked bands are read against the total, so the axis has to reach the top
  // of the pile rather than the tallest single band
  const stacks = series.map((_, si) =>
    labels.map((_, i) => series.slice(0, si + 1).reduce((sum, s) => sum + (s.values[i] ?? 0), 0)));

  const max = shape === 'stacked'
    ? Math.max(1, ...(stacks[stacks.length - 1] ?? [0]))
    : Math.max(1, ...series.flatMap((s) => s.values));
  // the floor stays at zero: a memory curve read against a cropped axis makes
  // every wobble look like a leak
  const ticks = [...new Set([0, max / 2, max].map((t) => format(t)))].map((label) => ({
    label,
    at: [0, max / 2, max].find((t) => format(t) === label) ?? 0,
  }));

  const x = (i: number) => (count < 2 ? plotW / 2 : (i / (count - 1)) * plotW);
  const y = (v: number) => plotH - (v / max) * plotH;

  const ink = dark ? '#9aa0a6' : '#5f6368';
  const grid = dark ? '#3c4043' : '#e8eaed';
  // the page behind the chart, used to hold the bands apart
  const surface = dark ? '#292a2d' : '#ffffff';

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
              <g key={tick.label}>
                <line x1={0} x2={plotW} y1={y(tick.at)} y2={y(tick.at)} stroke={grid} strokeWidth={1} />
                <text x={-8} y={y(tick.at) + 3.5} textAnchor="end" fontSize={10} fill={ink}>
                  {tick.label}
                </text>
              </g>
            ))}

            <g clipPath={`url(#${clip})`}>
              {shape === 'stacked'
                ? series.map((s, si) => (
                    <g key={s.label}>
                      <path
                        d={band(stacks[si], si > 0 ? stacks[si - 1] : null, x, y, plotH)}
                        fill={s.color[dark ? 1 : 0]}
                        stroke="none"
                      />
                      {/* a gap in the page colour, so neighbouring bands read as
                          two things rather than one gradient */}
                      {si > 0 && (
                        <path
                          d={trace(stacks[si - 1], x, y, 'line')}
                          fill="none"
                          stroke={surface}
                          strokeWidth={2}
                        />
                      )}
                    </g>
                  ))
                : series.map((s) => (
                    <path
                      key={s.label}
                      d={trace(s.values, x, y, shape)}
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
                {series.map((s, si) => (
                  <circle
                    key={s.label}
                    cx={x(hover)}
                    cy={y(shape === 'stacked' ? stacks[si][hover] ?? 0 : s.values[hover] ?? 0)}
                    r={4}
                    fill={s.color[dark ? 1 : 0]}
                    stroke={surface}
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

/** Builds the path, holding each value flat across its interval when stepped. */
function trace(values: number[], x: (i: number) => number, y: (v: number) => number, shape: 'line' | 'step' | 'stacked') {
  if (values.length === 0) {
    return '';
  }

  const parts = [`M ${x(0)},${y(values[0])}`];

  for (let i = 1; i < values.length; i++) {
    if (shape === 'step') {
      parts.push(`L ${x(i)},${y(values[i - 1])}`);
    }

    parts.push(`L ${x(i)},${y(values[i])}`);
  }

  return parts.join(' ');
}

/**
 * One band of a stack: along its own top, then back along the one below it.
 */
function band(
  top: number[],
  below: number[] | null,
  x: (i: number) => number,
  y: (v: number) => number,
  floor: number,
) {
  if (top.length === 0) {
    return '';
  }

  const forward = top.map((v, i) => `${i === 0 ? 'M' : 'L'} ${x(i)},${y(v)}`);

  const back = below
    ? below.map((v, i) => `L ${x(i)},${y(v)}`).reverse()
    : [`L ${x(top.length - 1)},${floor}`, `L ${x(0)},${floor}`];

  return [...forward, ...back, 'Z'].join(' ');
}

function Caption({ title, hint }: { title: string; hint?: string }) {
  return (
    <figcaption>
      <h2 className="text-sm font-medium">{title}</h2>
      {hint && <p className="mt-0.5 text-xs text-slate-500">{hint}</p>}
    </figcaption>
  );
}
