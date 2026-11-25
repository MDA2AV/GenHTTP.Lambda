import type { CSSProperties } from 'react';

import type { BrandTheme, SharedFile } from '@genhttp-lambda/kit/api';
import { type Brand } from '@genhttp-lambda/kit/brand';
import { BrandMark } from '@genhttp-lambda/kit/components/BrandLink';

/** What the fonts that are not saved yet are called on this page, apart from the ones it is drawn in. */
const TEXT = 'Lambda Draft Text';

const HEADINGS = 'Lambda Draft Headings';

/**
 * The colours of a page that are nobody's brand, by theme, written out: the
 * panel is in one of them, and the preview shows both.
 */
const THEMES = {
  light: { page: 'border-grey-300 bg-white text-ink-900', strip: 'bg-grey-200', tab: 'bg-white', muted: 'text-grey-800', accent: 'text-accent-600', headline: 'text-accent-700', ghost: 'text-accent-500' },
  dark: { page: 'border-ink-800 bg-ink-950 text-grey-100', strip: 'bg-ink-900', tab: 'bg-ink-950', muted: 'text-grey-200', accent: 'text-accent-400', headline: 'text-accent-400', ghost: 'text-accent-400' },
};

/**
 * The fonts as the operator chose them and has not saved yet: faces for this
 * page to draw the preview with. The files are shared already, so they are
 * the ones every page will load.
 */
export function DraftFaces({ text, headings }: { text: SharedFile[]; headings: SharedFile[] }) {
  return <style>{faces(TEXT, text) + faces(HEADINGS, headings)}</style>;
}

/** The faces of a font, under a name of this page's own. */
export function faces(family: string, files: SharedFile[]): string {
  return files
    .filter((file) => file.font)
    .map(({ font, versionedPath }) => {
      const weight = font!.from === font!.to ? font!.from : `${font!.from} ${font!.to}`;

      return `@font-face{font-family:"${family}";src:url("${versionedPath}");font-weight:${weight};font-style:${font!.italic ? 'italic' : 'normal'};font-display:swap}`;
    })
    .join('');
}

/** A colour as the kit's style sheet keeps it: its channels, for an opacity to be written behind them. */
const channels = (hex: string) => [1, 3, 5].map((at) => parseInt(hex.slice(at, at + 2), 16)).join(' ');

/**
 * What the colours set of the kit's style sheet, as the server writes it into
 * the pages (Services/Branding/BrandStyle.cs) - for the preview, which is in
 * colours the page around it is not.
 */
function variables(theme: BrandTheme): CSSProperties {
  const set: Record<string, string | null | undefined> = {
    '--accent-400': theme.accent?.tint,
    '--accent-500': theme.accent?.line,
    '--accent-600': theme.accent?.text,
    '--accent-700': theme.accent?.deep,
    '--accent-fill': theme.accent?.fill,
    '--accent-hover': theme.accent?.hover,
    '--accent-ink': theme.accent?.ink,
    '--second-400': theme.second?.tint,
    '--second-500': theme.second?.line,
    '--second-700': theme.second?.deep,
    '--aurora-a': theme.firstLight,
    '--aurora-b': theme.secondLight,
  };

  return Object.fromEntries(Object.entries(set).flatMap(([name, hex]) => (hex ? [[name, channels(hex)]] : []))) as CSSProperties;
}

/**
 * The top of the front page in one theme, and the tab of a browser above it,
 * as they will look once saved: the bar, the moving background, a headline,
 * text, a link and the buttons - drawn with the kit's own brand and classes.
 */
export function BrandPreview({ brand, theme, dark, text, headings }: {
  brand: Brand;
  /** What the colours come to in this theme. */
  theme: BrandTheme;
  dark: boolean;
  /** Whether there is a font of the operator's for the text, and one for the headings. */
  text: boolean;
  headings: boolean;
}) {
  const look = dark ? THEMES.dark : THEMES.light;

  const set = (text ? `"${TEXT}", ` : '') + 'var(--font-system)';

  // in place of what the page around it is set in, which may have been saved before
  const fonts = {
    fontFamily: set,
    '--font-heading': headings ? `"${HEADINGS}", ${set}` : set,
    '--heading-synthesis': headings ? 'none' : 'auto',
  } as CSSProperties;

  return (
    <div aria-label={dark ? 'In the dark theme' : 'In the light theme'} role="img">
      <div className={`overflow-hidden border ${dark ? 'in-dark' : 'in-light'} ${look.page}`} style={{ ...variables(theme), ...fonts }}>
        {/* the tab, its icon and its title - the browser's, in the browser's own face */}
        <div className={`flex items-end gap-2 px-2 pt-2 ${look.strip}`} style={{ fontFamily: 'var(--font-system)' }}>
          <div className={`flex min-w-0 max-w-[16rem] items-center gap-2 px-3 py-1.5 text-xs ${look.tab}`}>
            <img src={brand.logo ?? `${import.meta.env.BASE_URL}favicon.svg`} alt="" className="h-4 w-4 shrink-0 object-contain" />
            <span className="truncate">Showcase - {brand.name}</span>
          </div>
        </div>

        <div className="relative isolate">
          {/* the moving background, held still: the same lights, as they lie behind a headline */}
          <div
            aria-hidden="true"
            className="absolute inset-0 -z-10"
            style={{
              backgroundImage: `radial-gradient(60% 85% at 12% 18%, rgb(var(--aurora-a) / ${dark ? 0.5 : 0.52}), transparent 72%), radial-gradient(55% 80% at 92% 92%, rgb(var(--aurora-b) / ${dark ? 0.44 : 0.46}), transparent 72%)`,
            }}
          />
          <div
            aria-hidden="true"
            className="absolute inset-0 -z-10"
            style={{ background: `linear-gradient(to bottom, transparent, ${dark ? 'rgb(32 33 36 / 0.45)' : 'rgb(255 255 255 / 0.4)'} 35%, ${dark ? 'rgb(32 33 36 / 0.45)' : 'rgb(255 255 255 / 0.4)'} 70%, transparent)` }}
          />

          {/* the bar */}
          <div className="flex items-center gap-4 px-4 py-3">
            <div className={`flex ${brand.logo !== null ? 'min-w-0' : 'shrink-0'} items-center gap-2.5 whitespace-nowrap font-semibold tracking-tight`}>
              <BrandMark brand={brand} />
            </div>

            <span className={`ml-auto shrink-0 px-2 text-sm font-medium ${look.ghost}`}>Showcase</span>
          </div>

          {/* the front page */}
          <div className="px-5 pb-8 pt-5 text-center">
            <p className={`text-[11px] font-medium uppercase tracking-[0.2em] ${look.accent}`}>{brand.name}</p>

            <p className="wordmark mt-2.5 text-[1.65rem] font-bold leading-[1.1] tracking-tight">
              Describe an app.
              <span className={`block ${look.headline}`}>It goes online.</span>
            </p>

            <p className={`mx-auto mt-3 max-w-xs text-[13px] leading-relaxed ${look.muted}`}>
              What people read here, with <b>a few words of weight</b> and <span className={`underline ${look.accent}`}>a link</span> among them.
            </p>

            <div className="mt-5 flex flex-wrap items-center justify-center gap-2">
              <span className="inline-flex items-center rounded-full bg-accent-fill px-5 py-2 text-sm font-medium text-accent-ink shadow-sm">Describe an app</span>
              <span className={`inline-flex items-center rounded-full px-4 py-2 text-sm font-medium ${look.ghost}`}>Connect your agent</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
