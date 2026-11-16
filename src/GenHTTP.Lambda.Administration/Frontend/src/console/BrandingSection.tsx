import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';

import { ApiError, api, refusedToken, type AdminBranding } from '@genhttp-lambda/kit/api';
import { PLATFORM, changeBrand, type Brand } from '@genhttp-lambda/kit/brand';
import { BrandMark } from '@genhttp-lambda/kit/components/BrandLink';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconSpinner, IconUpload } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { Card, Reading, actionButton, bytes } from './parts';

/** The logo as the form holds it: as saved, a picture chosen and not uploaded yet, or taken away. */
type LogoDraft = { kind: 'saved' } | { kind: 'chosen'; file: File; url: string } | { kind: 'removed' };

/**
 * The name and the logo the installation's pages show in place of GenHTTP
 * Lambda's - where its host lets the operator give it some.
 *
 * The two are fields of one form, saved together, with what the bar and a
 * tab will look like drawn above it as they are typed and chosen: in both
 * themes, since a logo drawn for one may vanish on the other. Going back to
 * the platform's own is the one action, in the header, and says what it does
 * first.
 */
export function BrandingSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [saved, setSaved] = useState<AdminBranding | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [logo, setLogo] = useState<LogoDraft>({ kind: 'saved' });
  const [problem, setProblem] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [resetting, setResetting] = useState(false);
  const [confirming, setConfirming] = useState(false);

  const fail = useCallback((error: unknown, fallback: string) => {
    if (error instanceof ApiError && error.status === 404) {
      // the panel answers, so it is not the token: this installation's host
      // does not let its operator brand it
      setFailure('The name and the logo of this installation are its host’s, and are not changed here.');
    } else if (refusedToken(error)) {
      deny();
    } else {
      setFailure(fallback);
    }
  }, [deny]);

  const take = useCallback((branding: AdminBranding) => {
    setSaved(branding);
    setName(branding.name ?? '');
    setLogo({ kind: 'saved' });
  }, []);

  useEffect(() => {
    api.admin.branding(token)
      .then(take)
      .catch((error) => fail(error, 'The branding could not be read.'));
  }, [token, take, fail]);

  // a picture chosen is let go of once another is, or none, or the section closes
  useEffect(() => () => {
    if (logo.kind === 'chosen') {
      URL.revokeObjectURL(logo.url);
    }
  }, [logo]);

  const limits = saved?.limits;

  const trimmed = name.trim();

  const nameProblem = limits && trimmed.length > limits.name ? `At most ${limits.name} characters.` : null;

  const dirty = saved !== null && (trimmed !== (saved.name ?? '') || logo.kind === 'chosen' || (logo.kind === 'removed' && saved.logo !== null));

  /** The brand as it will look once saved, for the preview. */
  const drafted = useMemo<Brand>(() => {
    if (saved === null) {
      return PLATFORM;
    }

    const shown = trimmed === '' ? PLATFORM.name : trimmed;

    if (logo.kind === 'chosen') {
      return { name: shown, logo: logo.url };
    }

    if (logo.kind === 'saved' && saved.logo !== null) {
      return { name: shown, logo: saved.picture };
    }

    return trimmed === '' ? PLATFORM : { name: shown, logo: monogram(shown) };
  }, [saved, trimmed, logo]);

  function choose(file: File | undefined) {
    if (!file || !limits) {
      return;
    }

    if (!limits.types.includes(file.type)) {
      setProblem('That is no PNG, JPEG, GIF, WebP or SVG image.');
      return;
    }

    if (file.size > limits.logoBytes) {
      setProblem(`That picture is ${bytes(file.size)}; a logo may be ${bytes(limits.logoBytes)} at most.`);
      return;
    }

    setProblem(null);
    setLogo({ kind: 'chosen', file, url: URL.createObjectURL(file) });
  }

  async function save() {
    if (saved === null || !dirty || nameProblem !== null || saving) {
      return;
    }

    setSaving(true);

    try {
      let now = saved;

      if (trimmed !== (saved.name ?? '')) {
        now = await api.admin.rename(token, trimmed === '' ? null : trimmed);
      }

      if (logo.kind === 'chosen') {
        now = await api.admin.uploadLogo(token, logo.file);
      } else if (logo.kind === 'removed' && saved.logo !== null) {
        await api.admin.removeLogo(token);
        now = await api.admin.branding(token);
      }

      take(now);
      show(now);

      toast('The branding is saved. Every page shows it from the next one anybody opens.', 'success');
    } catch (error) {
      if (refusedToken(error) && !(error instanceof ApiError && error.status === 404)) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : 'The branding could not be saved.', 'error');

        // what did get saved before the rest was refused
        api.admin.branding(token).then((read) => {
          setSaved(read);
          show(read);
        }).catch(() => undefined);
      }
    } finally {
      setSaving(false);
    }
  }

  async function reset() {
    setResetting(true);

    try {
      await api.admin.resetBranding(token);

      const read = await api.admin.branding(token);

      take(read);
      show(read);
      setConfirming(false);

      toast('The pages show GenHTTP Lambda again.', 'success');
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : 'The branding could not be reset.', 'error');
      }
    } finally {
      setResetting(false);
    }
  }

  const own = saved !== null && (saved.name !== null || saved.logo !== null);

  return (
    <Section
      title="Branding"
      hint="What the installation's pages show in place of GenHTTP Lambda: in the bar of every page, at the end of every title, in link previews and as the icon of the browser tab. No restart needed."
      actions={own && (
        <button type="button" onClick={() => setConfirming(true)} className={`btn-ghost ${actionButton}`}>
          Go back to GenHTTP Lambda
        </button>
      )}
    >
      {saved === null ? (
        <Reading what="the branding" failure={failure} />
      ) : (
        <form
          onSubmit={(event) => {
            event.preventDefault();
            void save();
          }}
          className="space-y-6"
        >
          <Card title="Preview" hint="The bar of a page and a browser tab, as they will look once saved - in the light theme and in the dark one.">
            <div className="grid gap-3 lg:grid-cols-2">
              <Preview brand={drafted} dark={false} />
              <Preview brand={drafted} dark />
            </div>
          </Card>

          <div className="surface divide-y divide-grey-300 dark:divide-ink-800">
            <Row
              id="branding-name"
              title="Name"
              note="What the pages call the installation. Empty for GenHTTP Lambda."
            >
              <div className="relative">
                <input
                  id="branding-name"
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                  placeholder={PLATFORM.name}
                  autoComplete="off"
                  aria-invalid={nameProblem !== null}
                  className={`field py-1.5 pr-14 text-sm ${nameProblem ? '!border-red-500 !ring-red-500' : ''}`}
                />
                <span className="pointer-events-none absolute inset-y-0 right-3 flex items-center text-xs tabular-nums text-slate-400">
                  {trimmed.length}/{limits?.name}
                </span>
              </div>
              {nameProblem && <p className="mt-1 text-xs text-red-500">{nameProblem}</p>}
            </Row>

            <Row
              id="branding-logo"
              title="Logo"
              note={
                <>
                  Beside the name, at the height of a line, and the icon of the browser tab. A square mark reads best, and one with
                  a background of its own on both themes. PNG, JPEG, GIF, WebP or SVG, {limits ? bytes(limits.logoBytes) : ''} at
                  most. Without one, the first letter of the name on a plate.
                </>
              }
            >
              <LogoField
                saved={saved}
                draft={logo}
                types={limits?.types ?? []}
                onChoose={choose}
                onRemove={() => {
                  setProblem(null);
                  setLogo(saved.logo !== null ? { kind: 'removed' } : { kind: 'saved' });
                }}
              />
              {problem && <p className="mt-1.5 text-xs text-red-500">{problem}</p>}
            </Row>
          </div>

          <div className="sticky bottom-0 -mx-4 flex flex-wrap items-center gap-3 border-t border-grey-300 bg-white/95 px-4 py-3 backdrop-blur dark:border-ink-800 dark:bg-ink-950/95 md:mx-0 md:px-0">
            <button type="submit" disabled={!dirty || nameProblem !== null || saving} className="btn-primary">
              {saving && <IconSpinner />}
              Save branding
            </button>

            {dirty && (
              <button type="button" onClick={() => take(saved)} disabled={saving} className="btn-ghost">
                Discard changes
              </button>
            )}

            <span className="text-[13px] text-slate-500">{dirty ? 'Not saved yet.' : 'Everything is saved.'}</span>
          </div>

          <Card title="Where it shows">
            <ul className="list-disc space-y-1.5 pl-5 text-[13px] text-slate-600 dark:text-slate-400">
              <li>The bar of every page: the front page, the showcase, the editor, the published sources and this panel.</li>
              <li>The end of every page's title, and the name a link preview gives the site.</li>
              <li>The icon of the browser tab, and of the site added to a phone's home screen.</li>
              <li>
                Link previews of the front page and the showcase show the logo where it is a PNG, JPEG, GIF or WebP image, and no
                picture otherwise - their own pictures have GenHTTP Lambda drawn into them.
              </li>
            </ul>
            <p className="mt-3 text-[13px] text-slate-500">
              GenHTTP Lambda stays named where the software is meant: the link to its source and its community, the API&rsquo;s
              description, the tools agents connect with, and the line apps built here are asked to carry at their foot.
            </p>
          </Card>

          <Card title="For agents">
            <p className="text-[13px] text-slate-600 dark:text-slate-400">
              An agent you give the admin token changes the same: <code className="font-mono text-[12px]">PUT /api/v1/admin/branding</code>{' '}
              with the name and <code className="font-mono text-[12px]">PUT /api/v1/admin/branding/logo</code> with the picture as the
              body, or the MCP tool <code className="font-mono text-[12px]">customize_installation</code>.
            </p>
          </Card>
        </form>
      )}

      <Dialog
        title="Go back to GenHTTP Lambda?"
        open={confirming}
        onClose={() => setConfirming(false)}
        footer={
          <>
            <button type="button" onClick={() => setConfirming(false)} className="btn-ghost">Keep them</button>
            <button type="button" onClick={() => void reset()} disabled={resetting} className="btn-primary">
              {resetting && <IconSpinner />}
              Go back
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          The pages show GenHTTP Lambda again, with its own mark, from the next one anybody opens. The name and the logo are
          deleted: to bring the logo back, upload it again.
        </p>
      </Dialog>
    </Section>
  );
}

/** The brand as it is shown to everybody: what the bar and the kit draw from now on. */
function show(branding: AdminBranding) {
  changeBrand(branding.picture === null ? PLATFORM : { name: branding.shown, logo: branding.picture });
}

/** A setting: what it is on the left, its field on the right. */
function Row({ id, title, note, children }: { id: string; title: string; note: ReactNode; children: ReactNode }) {
  return (
    <div className="grid gap-3 p-4 sm:grid-cols-[1fr_minmax(0,22rem)] sm:items-start sm:gap-6">
      <div className="min-w-0">
        <label htmlFor={id} className="text-[15px] font-medium">{title}</label>
        <p className="mt-1 text-[13px] text-slate-500">{note}</p>
      </div>

      <div className="min-w-0">{children}</div>
    </div>
  );
}

/**
 * The logo as it is, or as chosen: a tile with the picture, on which a file
 * may be dropped too, a button to choose one, and one to take it away.
 */
function LogoField({ saved, draft, types, onChoose, onRemove }: {
  saved: AdminBranding;
  draft: LogoDraft;
  types: string[];
  onChoose: (file: File | undefined) => void;
  onRemove: () => void;
}) {
  const input = useRef<HTMLInputElement>(null);
  const [over, setOver] = useState(false);

  const shown = draft.kind === 'chosen' ? draft.url : draft.kind === 'saved' && saved.logo !== null ? saved.picture : null;

  const described = draft.kind === 'chosen'
    ? `${draft.file.name}, ${bytes(draft.file.size)} - not saved yet`
    : draft.kind === 'removed'
      ? 'Taken away - not saved yet'
      : saved.logo !== null
        ? `${saved.logo.type.replace('image/', '').replace('+xml', '').toUpperCase()}, ${bytes(saved.logo.bytes)}`
        : 'None';

  return (
    <div className="flex items-center gap-3">
      <div
        onDragOver={(event) => {
          event.preventDefault();
          setOver(true);
        }}
        onDragLeave={() => setOver(false)}
        onDrop={(event) => {
          event.preventDefault();
          setOver(false);
          onChoose(event.dataTransfer.files[0]);
        }}
        className={`flex h-16 w-16 shrink-0 items-center justify-center border border-dashed bg-[repeating-conic-gradient(#e8eaed_0%_25%,#fff_0%_50%)] bg-[length:12px_12px] dark:bg-[repeating-conic-gradient(#303134_0%_25%,#292a2d_0%_50%)] ${
          over ? 'border-accent-500' : 'border-grey-300 dark:border-ink-700'
        }`}
        title="Drop a picture here"
      >
        {shown ? <img src={shown} alt="The logo" className="max-h-12 max-w-12 object-contain" /> : <IconUpload className="h-5 w-5 text-slate-400" />}
      </div>

      <div className="min-w-0 flex-1">
        <p className="truncate text-[13px] text-slate-600 dark:text-slate-400">{described}</p>

        <div className="mt-1.5 flex flex-wrap gap-1.5">
          <button type="button" onClick={() => input.current?.click()} className={`btn-ghost border border-grey-300 dark:border-ink-700 ${actionButton}`}>
            {shown ? 'Choose another' : 'Choose a picture'}
          </button>

          {shown && (
            <button type="button" onClick={onRemove} className={`btn-ghost ${actionButton}`}>
              Take it away
            </button>
          )}
        </div>

        <input
          ref={input}
          id="branding-logo"
          type="file"
          accept={types.join(',')}
          className="sr-only"
          tabIndex={-1}
          onChange={(event) => {
            onChoose(event.target.files?.[0]);
            event.target.value = '';
          }}
        />
      </div>
    </div>
  );
}

/** The colours of a theme, written out: the panel is in one of them, and the preview shows both. */
const THEMES = {
  light: { page: 'border-grey-300 bg-white text-ink-900', strip: 'bg-grey-200', tab: 'bg-white' },
  dark: { page: 'border-ink-800 bg-ink-950 text-grey-100', strip: 'bg-ink-900', tab: 'bg-ink-950' },
};

/**
 * The bar of a page and a tab of the browser in one theme, drawn with the
 * kit's own brand so they look as the pages will.
 */
function Preview({ brand, dark }: { brand: Brand; dark: boolean }) {
  const theme = dark ? THEMES.dark : THEMES.light;

  return (
    <div aria-label={dark ? 'In the dark theme' : 'In the light theme'} role="img">
      <div className={`border ${theme.page}`}>
        {/* the tab, its icon and its title */}
        <div className={`flex items-end gap-2 px-2 pt-2 ${theme.strip}`}>
          <div className={`flex min-w-0 max-w-[16rem] items-center gap-2 px-3 py-1.5 text-xs ${theme.tab}`}>
            {brand.logo !== null ? (
              <img src={brand.logo} alt="" className="h-4 w-4 shrink-0 object-contain" />
            ) : (
              <img src={`${import.meta.env.BASE_URL}favicon.svg`} alt="" className="h-4 w-4 shrink-0" />
            )}
            <span className="truncate">Showcase - {brand.name}</span>
          </div>
        </div>

        {/* the bar */}
        <div className="flex items-center gap-4 px-4 py-3">
          <div className={`flex ${brand.logo !== null ? 'min-w-0' : 'shrink-0'} items-center gap-2.5 whitespace-nowrap font-semibold tracking-tight`}>
            <BrandMark brand={brand} />
          </div>

          <span className="ml-auto shrink-0 px-2 text-sm text-slate-500">Showcase</span>
        </div>
      </div>
    </div>
  );
}

/**
 * The first letter of a name on a square plate, as the server draws it for an
 * installation named without a logo (Services/Branding/Monogram.cs) - for the
 * preview of a name that is not saved yet.
 */
function monogram(name: string): string {
  const letter = [...new Intl.Segmenter().segment(name)].map((s) => s.segment).find((s) => /[\p{L}\p{N}]/u.test(s)) ?? name.slice(0, 1) ?? '?';

  const escaped = letter.toLocaleUpperCase('en').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><rect width="64" height="64" fill="#1a73e8"/><text x="32" y="32" dy=".35em" text-anchor="middle" font-family="-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif" font-size="36" font-weight="600" fill="#ffffff">${escaped}</text></svg>`;

  return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
}
