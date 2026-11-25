import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError, api, refusedToken, type AdminBranding, type BrandPreview as Preview, type SharedFile, type SharedFiles } from '@genhttp-lambda/kit/api';
import { PLATFORM, changeBrand, changeLook, type Brand } from '@genhttp-lambda/kit/brand';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import { ColorField, FontField, LogoField, Row, parseColor, type LogoDraft } from './BrandFields';
import { BrandPreview, DraftFaces } from './BrandPreview';
import type { Access } from './context';
import { Card, Reading, actionButton, bytes } from './parts';
import { isSame, kindOf, refusal, share, sharedName } from './shared';

/** The platform's own accents, as index.css of the kit has them. */
const BLUE = '#1a73e8';

const PURPLE = '#aa55ff';

/** The fonts as the form holds them: the shared files each is made of. */
interface Fonts {
  text: SharedFile[];
  headings: SharedFile[];
}

const names = (files: SharedFile[]) => files.map((file) => file.name).join('\n');

/**
 * What the installation's pages show in place of GenHTTP Lambda and how they
 * are drawn - its name, its logo, its colours and its fonts - where its host
 * lets the operator say so.
 *
 * All of it is one form, saved together, with the top of the front page drawn
 * above it as it is typed and chosen: in both themes, since a logo or a
 * colour made for one may vanish on the other. What a colour comes to is
 * asked of the server, which draws the pages with it. A font is a shared
 * file, so one that is uploaded here is shared at once and only used once
 * the form is saved. Going back to the platform's own is the one action, in
 * the header, and says what it does first.
 */
export function BrandingSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [saved, setSaved] = useState<AdminBranding | null>(null);
  const [shared, setShared] = useState<SharedFiles | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [logo, setLogo] = useState<LogoDraft>({ kind: 'saved' });
  const [accent, setAccent] = useState('');
  const [second, setSecond] = useState('');
  const [preview, setPreview] = useState<Preview | null>(null);
  const [fonts, setFonts] = useState<Fonts>({ text: [], headings: [] });
  const [uploading, setUploading] = useState<keyof Fonts | null>(null);
  const [problems, setProblems] = useState<{ logo?: string; text?: string; headings?: string }>({});
  const [saving, setSaving] = useState(false);
  const [resetting, setResetting] = useState(false);
  const [confirming, setConfirming] = useState(false);

  const fail = useCallback((error: unknown, fallback: string) => {
    if (error instanceof ApiError && error.status === 404) {
      // the panel answers, so it is not the token: this installation's host
      // does not let its operator brand it
      setFailure('How this installation presents itself is its host’s, and is not changed here.');
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
    setAccent(branding.colors.accent ?? '');
    setSecond(branding.colors.second ?? '');
    setPreview({ colors: branding.colors, fonts: branding.fonts });
    setFonts(branding.fonts);
    setProblems({});
  }, []);

  useEffect(() => {
    Promise.all([api.admin.branding(token), api.admin.sharedFiles(token)])
      .then(([branding, files]) => {
        setShared(files);
        take(branding);
      })
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

  // what was entered as colours - undefined for what is none
  const [first, other] = [parseColor(accent), parseColor(second)];

  const colored = first !== undefined && other !== undefined;

  const [text, headings] = [names(fonts.text), names(fonts.headings)];

  // what they come to is the server's to say, asked as they change: a picker
  // dragged across its colours names dozens a second, so only where it rests
  useEffect(() => {
    if (saved === null || first === undefined || other === undefined) {
      return;
    }

    let current = true;

    const timer = window.setTimeout(() => {
      api.admin.previewBranding(token, first, other, text.split('\n').filter(Boolean), headings.split('\n').filter(Boolean))
        .then((read) => current && setPreview(read))
        .catch((error) => refusedToken(error) && deny());
    }, 60);

    return () => {
      current = false;
      window.clearTimeout(timer);
    };
  }, [token, deny, saved, first, other, text, headings]);

  const dirty = {
    name: saved !== null && trimmed !== (saved.name ?? ''),
    logo: logo.kind === 'chosen' || (logo.kind === 'removed' && saved !== null && saved.logo !== null),
    colors: saved !== null && (!colored || first !== (saved.colors.accent ?? null) || other !== (saved.colors.second ?? null)),
    fonts: saved !== null && (text !== names(saved.fonts.text) || headings !== names(saved.fonts.headings)),
  };

  const changed = dirty.name || dirty.logo || dirty.colors || dirty.fonts;

  const savable = changed && nameProblem === null && colored && !saving && uploading === null;

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

    const plate = preview?.colors.light.accent;

    return trimmed === '' ? PLATFORM : { name: shown, logo: monogram(shown, plate?.fill ?? BLUE, plate?.ink ?? '#ffffff') };
  }, [saved, trimmed, logo, preview]);

  const pictures = shared?.files.filter((file) => file.kind === 'picture') ?? [];
  const faces = shared?.files.filter((file) => file.kind === 'font') ?? [];

  function choose(file: File | undefined) {
    if (!file || !limits) {
      return;
    }

    if (!limits.types.includes(file.type)) {
      setProblems((was) => ({ ...was, logo: 'That is no PNG, JPEG, GIF, WebP or SVG image.' }));
      return;
    }

    takePicture(file, file.name);
  }

  /** A picture for the logo, from this computer or a shared one. */
  function takePicture(file: Blob, called: string) {
    if (limits && file.size > limits.logoBytes) {
      setProblems((was) => ({ ...was, logo: `That picture is ${bytes(file.size)}; a logo may be ${bytes(limits.logoBytes)} at most.` }));
      return;
    }

    setProblems((was) => ({ ...was, logo: undefined }));
    setLogo({ kind: 'chosen', file, name: called, url: URL.createObjectURL(file) });
  }

  async function chooseShared(picture: SharedFile) {
    try {
      const response = await fetch(picture.versionedPath);

      if (!response.ok) {
        throw new Error();
      }

      takePicture(await response.blob(), picture.name);
    } catch {
      setProblems((was) => ({ ...was, logo: `${picture.name} could not be read.` }));
    }
  }

  const put = (font: keyof Fonts, files: SharedFile[]) => setFonts((was) => ({ ...was, [font]: files }));

  /** Shares files of this computer and makes the font of them too. */
  async function upload(font: keyof Fonts, files: File[]) {
    if (files.length === 0 || shared === null) {
      return;
    }

    setUploading(font);
    setProblems((was) => ({ ...was, [font]: undefined }));

    const added: SharedFile[] = [];
    let refused: string | null = null;

    try {
      for (const file of files) {
        const called = sharedName(file.name);
        const there = shared.files.find((one) => one.name.toLowerCase() === called.toLowerCase());

        // shared already, as it is: it is that one that was meant
        if (there && there.kind === 'font' && (await isSame(file, there))) {
          added.push(there);
          continue;
        }

        refused = kindOf(called, shared.limits) !== 'font'
          ? `${file.name} is no WOFF2, WOFF, TrueType or OpenType font.`
          : there
            ? `There is another shared file called ${there.name}: choose it from the list, or replace it under Shared files.`
            : refusal(called, file, shared.limits);

        if (refused !== null) {
          break;
        }

        const answer = await share(token, called, file);

        if (typeof answer === 'string') {
          refused = answer;
          break;
        }

        added.push(answer);
      }

      if (added.length > 0) {
        setFonts((was) => ({ ...was, [font]: [...was[font], ...added.filter((file) => !was[font].some((taken) => taken.name === file.name))] }));
        setShared(await api.admin.sharedFiles(token));
      }
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        refused = 'The font could not be uploaded.';
      }
    } finally {
      setUploading(null);
      setProblems((was) => ({ ...was, [font]: refused ?? undefined }));
    }
  }

  async function save() {
    if (saved === null || !savable || first === undefined || other === undefined) {
      return;
    }

    setSaving(true);

    try {
      let now = saved;

      if (dirty.name) {
        now = await api.admin.rename(token, trimmed === '' ? null : trimmed);
      }

      if (dirty.colors) {
        now = await api.admin.saveColors(token, first, other);
      }

      if (dirty.fonts) {
        now = await api.admin.saveFonts(token, fonts.text.map((file) => file.name), fonts.headings.map((file) => file.name));
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

  const light = preview?.colors.light ?? {};
  const dark = preview?.colors.dark ?? {};

  const own = saved !== null && (saved.name !== null || saved.logo !== null || saved.styleSheet !== null);

  return (
    <Section
      title="Branding"
      hint="What the installation's pages show in place of GenHTTP Lambda, and the colours and fonts they are drawn with. No restart needed."
      actions={own && (
        <button type="button" onClick={() => setConfirming(true)} className={`btn-ghost ${actionButton}`}>
          Go back to GenHTTP Lambda
        </button>
      )}
    >
      {saved === null || preview === null ? (
        <Reading what="the branding" failure={failure} />
      ) : (
        <form
          onSubmit={(event) => {
            event.preventDefault();
            void save();
          }}
          className="space-y-6"
        >
          <DraftFaces text={fonts.text} headings={fonts.headings} />

          <Card title="Preview" hint="The front page and a browser tab, as they will look once saved.">
            <div className="grid gap-3 lg:grid-cols-2">
              <BrandPreview brand={drafted} theme={preview.colors.light} dark={false} text={fonts.text.length > 0} headings={fonts.headings.length > 0} />
              <BrandPreview brand={drafted} theme={preview.colors.dark} dark text={fonts.text.length > 0} headings={fonts.headings.length > 0} />
            </div>
          </Card>

          <div className="surface divide-y divide-grey-300 dark:divide-ink-800">
            <Row id="branding-name" title="Name" note="What the pages call the installation. Empty for GenHTTP Lambda.">
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
                  most. Without one, the first letter of the name on a plate of the accent.
                </>
              }
            >
              <LogoField
                saved={saved}
                draft={logo}
                types={limits?.types ?? []}
                pictures={pictures}
                onChoose={choose}
                onShared={(picture) => void chooseShared(picture)}
                onRemove={() => {
                  setProblems((was) => ({ ...was, logo: undefined }));
                  setLogo(saved.logo !== null ? { kind: 'removed' } : { kind: 'saved' });
                }}
              />
              {problems.logo && <p className="mt-1.5 text-xs text-red-500">{problems.logo}</p>}
            </Row>

            <Row
              id="branding-accent"
              title="Accent"
              note="Links, buttons and marks, and the first light of the moving background. Text in it takes a shade of it that can be read, in each theme."
            >
              <ColorField
                id="branding-accent"
                value={accent}
                platform={BLUE}
                text={first && light.accent && dark.accent ? { light: light.accent.text, dark: dark.accent.tint } : null}
                onChange={setAccent}
              />
            </Row>

            <Row
              id="branding-second"
              title="Second accent"
              note="What the platform draws in purple, such as a tier's badge, and the second light of the moving background - without one, the neighbour of the accent."
            >
              <ColorField
                id="branding-second"
                value={second}
                platform={first ? light.secondLight ?? PURPLE : PURPLE}
                text={other && light.second && dark.second ? { light: light.second.deep, dark: dark.second.tint } : null}
                onChange={setSecond}
              />
            </Row>

            <Row
              id="branding-font-text"
              title="Font"
              note={
                <>
                  What the pages are set in; code keeps its own. One variable font, or a file for each weight - WOFF2 is the smallest.
                  Served from here, so no visitor&rsquo;s browser asks anybody else. Its license has to allow it on the web.
                </>
              }
            >
              <FontField
                id="branding-font-text"
                chosen={fonts.text}
                offered={faces.filter((file) => !fonts.text.some((taken) => taken.name === file.name))}
                limit={limits?.fontFiles ?? 0}
                accept={accepted(shared)}
                busy={uploading === 'text'}
                problem={problems.text ?? null}
                notes={[thin(fonts.text), lacking(preview.fonts.textLacks)]}
                onAdd={(file) => put('text', [...fonts.text, file])}
                onRemove={(file) => put('text', fonts.text.filter((there) => there.name !== file.name))}
                onUpload={(files) => void upload('text', files)}
              />
            </Row>

            <Row id="branding-font-headings" title="Font of headings" note="Headings, and the name in the bar. Without one, the font above.">
              <FontField
                id="branding-font-headings"
                chosen={fonts.headings}
                offered={faces.filter((file) => !fonts.headings.some((taken) => taken.name === file.name))}
                limit={limits?.fontFiles ?? 0}
                accept={accepted(shared)}
                busy={uploading === 'headings'}
                problem={problems.headings ?? null}
                notes={[lacking(preview.fonts.headingLacks)]}
                onAdd={(file) => put('headings', [...fonts.headings, file])}
                onRemove={(file) => put('headings', fonts.headings.filter((there) => there.name !== file.name))}
                onUpload={(files) => void upload('headings', files)}
              />
            </Row>
          </div>

          <div className="sticky bottom-0 z-10 -mx-4 flex flex-wrap items-center gap-3 border-t border-grey-300 bg-white/95 px-4 py-3 backdrop-blur dark:border-ink-800 dark:bg-ink-950/95 md:mx-0 md:px-0">
            <button type="submit" disabled={!savable} className="btn-primary">
              {saving && <IconSpinner />}
              Save branding
            </button>

            {changed && (
              <button type="button" onClick={() => take(saved)} disabled={saving} className="btn-ghost">
                Discard changes
              </button>
            )}

            <span className="text-[13px] text-slate-500">{changed ? 'Not saved yet.' : 'Everything is saved.'}</span>
          </div>

          <Card title="Where it shows">
            <ul className="list-disc space-y-1.5 pl-5 text-[13px] text-slate-600 dark:text-slate-400">
              <li>The bar, the colours and the fonts of every page: the front page, the showcase, the editor, the published sources and this panel.</li>
              <li>The end of every page&rsquo;s title, the name a link preview gives the site, and the icon of the tab and of a phone&rsquo;s home screen.</li>
              <li>
                Link previews show the logo where it is a PNG, JPEG, GIF or WebP image, and no picture otherwise - the site&rsquo;s
                own have GenHTTP Lambda drawn into them.
              </li>
              <li>The &ldquo;Made with&rdquo; line, the pages of the hosting domain and a lambda&rsquo;s repository name it, in what is written after the change.</li>
            </ul>
            <p className="mt-3 text-[13px] text-slate-500">
              Not in the apps built here, whose looks are their own - for a house style, share its files under{' '}
              <Link to="/admin/shared" className="text-accent-600 hover:underline dark:text-accent-400">Shared files</Link> and name them in the{' '}
              <Link to="/admin/guide" className="text-accent-600 hover:underline dark:text-accent-400">Guide</Link>. GenHTTP Lambda stays named where
              the software is meant.
            </p>
          </Card>

          <Card title="For agents">
            <p className="text-[13px] text-slate-600 dark:text-slate-400">
              An agent you give the admin token changes the same: below <code className="font-mono text-[12px]">/api/v1/admin/branding</code> - the
              name, <code className="font-mono text-[12px]">/logo</code>, <code className="font-mono text-[12px]">/colors</code> and{' '}
              <code className="font-mono text-[12px]">/fonts</code> - or with <code className="font-mono text-[12px]">customize_installation</code> on
              the operator&apos;s MCP endpoint, <code className="font-mono text-[12px]">/admin/mcp</code>.
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
            <button type="button" onClick={() => setConfirming(false)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={() => void reset()} disabled={resetting} className="btn-primary">
              {resetting && <IconSpinner />}
              Go back
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          The pages show GenHTTP Lambda again, with its own mark, colours and fonts, from the next one anybody opens. The name, the
          logo and the colours are deleted: to bring the logo back, upload it again. The files of the fonts stay shared.
        </p>
      </Dialog>
    </Section>
  );
}

/** The brand as it is shown to everybody: what the bar and the kit draw from now on. */
function show(branding: AdminBranding) {
  changeBrand(branding.picture === null ? PLATFORM : { name: branding.shown, logo: branding.picture });
  changeLook(branding.styleSheet);
}

/** What a file dialog offers for a font. */
const accepted = (shared: SharedFiles | null) => shared?.limits.types.font.map((end) => `.${end}`).join(',') ?? '';

/** What is missing from a font to set a page in: the pages write words in bold, and slant few. */
function thin(files: SharedFile[]): string | null {
  const upright = files.flatMap((file) => (file.font && !file.font.italic ? [file.font] : []));

  if (files.length === 0) {
    return null;
  }

  if (upright.length === 0) {
    return 'Only italic: add an upright one.';
  }

  return upright.some((font) => font.to >= 600) ? null : 'No bold weight: browsers thicken the one there is. Add the bold file, or a variable font.';
}

/** The languages a font has too few letters for, whose words it would draw in two faces. */
function lacking(languages: string[]): string | null {
  if (languages.length === 0) {
    return null;
  }

  const called = languages.map((language) => LANGUAGES[language] ?? language);
  const listed = called.length === 1 ? called[0] : `${called.slice(0, -1).join(', ')} and ${called[called.length - 1]}`;

  return `Letters missing for ${listed}: those are drawn in another font, in the middle of a word. Add the file that has them.`;
}

/** The languages of the pages in Latin letters, as the panel names them. */
const LANGUAGES: Record<string, string> = {
  de: 'German', en: 'English', es: 'Spanish', fr: 'French', it: 'Italian', nl: 'Dutch', pl: 'Polish', pt: 'Portuguese', tr: 'Turkish',
};

/**
 * The first letter of a name on a square plate, as the server draws it for an
 * installation named without a logo (Services/Branding/Monogram.cs) - for the
 * preview of a name that is not saved yet.
 */
function monogram(name: string, plate: string, ink: string): string {
  const letter = [...new Intl.Segmenter().segment(name)].map((s) => s.segment).find((s) => /[\p{L}\p{N}]/u.test(s)) ?? name.slice(0, 1) ?? '?';

  const escaped = letter.toLocaleUpperCase('en').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><rect width="64" height="64" fill="${plate}"/><text x="32" y="32" dy=".35em" text-anchor="middle" font-family="-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif" font-size="36" font-weight="600" fill="${ink}">${escaped}</text></svg>`;

  return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
}
