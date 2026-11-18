import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';

import { ApiError, api, refusedToken, type AdminGuide } from '@genhttp-lambda/kit/api';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { MarkdownPage } from '@genhttp-lambda/kit/components/MarkdownPage';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { renderMarkdown, type MarkdownPlace } from '@genhttp-lambda/kit/markdown';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { Card, Reading, actionButton, bytes } from './parts';

/** The guide is one page with nothing beside it: no pictures, no other pages. */
const ALONE: MarkdownPlace = { name: 'guide.md', picture: () => null, page: () => false };

const EXAMPLE = '- Every app is in German.\n- Keep no personal data beyond an email address.\n- Use the colours of acme.example/brand.';

/** The text as the server keeps it: line breaks as \n, trimmed. */
const kept = (text: string) => text.replace(/\r\n?/g, '\n').trim();

const utf8 = (text: string) => new TextEncoder().encode(text).length;

/**
 * The guide agents read first over MCP, and the section the operator writes
 * into it - on every installation that has an operator.
 *
 * The section is the setting, a form of two fields saved together, with what
 * it will look like drawn below it as it is typed. The guide around it is
 * shown as agents read it, rendered or as the Markdown they are handed.
 * Taking the section out again is the one action, in the header, and says
 * what it does first.
 */
export function GuideSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [read, setRead] = useState<AdminGuide | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [title, setTitle] = useState('');
  const [text, setText] = useState('');
  const [saving, setSaving] = useState(false);
  const [removing, setRemoving] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [raw, setRaw] = useState(false);

  const take = useCallback((guide: AdminGuide) => {
    setRead(guide);
    setTitle(guide.section?.title ?? '');
    setText(guide.section?.text ?? '');
  }, []);

  useEffect(() => {
    api.admin.guide(token)
      .then(take)
      .catch((error) => (refusedToken(error) ? deny() : setFailure('The guide could not be read.')));
  }, [token, take, deny]);

  const section = read?.section ?? null;
  const limits = read?.limits;

  const trimmed = title.trim();
  const body = kept(text);
  const size = utf8(body);

  const titleProblem = limits && trimmed.length > limits.title ? `At most ${limits.title} characters.` : null;
  const textProblem = limits && size > limits.textBytes ? `${bytes(size)} - at most ${bytes(limits.textBytes)}.` : null;

  const dirty = read !== null && (trimmed !== (section?.title ?? '') || body !== (section?.text ?? ''));
  const complete = trimmed !== '' && body !== '';

  // 23 KB of Markdown, rendered once per guide read rather than per keystroke
  const rendered = useMemo(() => (read === null ? null : renderMarkdown(read.guide, ALONE)), [read]);

  /** The section as agents will read it once saved. */
  const preview = useMemo(
    () => (dirty && complete ? renderMarkdown(`## ${trimmed}\n\n${body}`, ALONE) : null),
    [dirty, complete, trimmed, body],
  );

  async function reread() {
    take(await api.admin.guide(token));
  }

  async function save() {
    if (!dirty || !complete || titleProblem !== null || textProblem !== null || saving) {
      return;
    }

    setSaving(true);

    try {
      await api.admin.saveGuide(token, trimmed, body);
      await reread();

      toast('The section is saved. Agents read it from their next call to the guide.', 'success');
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : 'The section could not be saved.', 'error');
      }
    } finally {
      setSaving(false);
    }
  }

  async function remove() {
    setRemoving(true);

    try {
      await api.admin.removeGuide(token);
      await reread();

      setConfirming(false);
      toast('The section is removed. Agents read the platform’s guide alone from their next call.', 'success');
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : 'The section could not be removed.', 'error');
      }
    } finally {
      setRemoving(false);
    }
  }

  return (
    <Section
      title="Guide"
      hint="What agents read first over MCP - platform_guide, which the build agent calls before anything else too. A section of your own comes last in it, after the platform's: how lambdas are built here."
      actions={section && (
        <button type="button" onClick={() => setConfirming(true)} className={`btn-ghost ${actionButton}`}>
          Remove your section
        </button>
      )}
    >
      {read === null ? (
        <Reading what="the guide" failure={failure} />
      ) : (
        <div className="space-y-6">
          <form
            onSubmit={(event) => {
              event.preventDefault();
              void save();
            }}
            className="space-y-6"
          >
            <div className="surface divide-y divide-grey-300 dark:divide-ink-800">
              <div className="grid gap-3 p-4 sm:grid-cols-[1fr_minmax(0,22rem)] sm:items-start sm:gap-6">
                <Label id="guide-title" title="Title" note="The heading agents find your section under. None of the platform's own sections' names." />

                <div className="min-w-0">
                  <div className="relative">
                    <input
                      id="guide-title"
                      value={title}
                      onChange={(event) => setTitle(event.target.value)}
                      placeholder="Rules of this installation"
                      autoComplete="off"
                      aria-invalid={titleProblem !== null}
                      className={`field py-1.5 pr-14 text-sm ${titleProblem ? '!border-red-500 !ring-red-500' : ''}`}
                    />
                    <span className="pointer-events-none absolute inset-y-0 right-3 flex items-center text-xs tabular-nums text-slate-400">
                      {trimmed.length}/{limits?.title}
                    </span>
                  </div>
                  {titleProblem && <p className="mt-1 text-xs text-red-500">{titleProblem}</p>}
                </div>
              </div>

              <div className="space-y-3 p-4">
                <Label
                  id="guide-text"
                  title="Text"
                  note={
                    <>
                      How lambdas are built here, and what not - in a few lines: every agent reads the whole guide with every call.
                      Markdown, with <code className="font-mono text-[12px]">###</code> for headings inside it. Agents follow it wherever
                      the platform&rsquo;s guide above leaves them the choice; it changes nothing the platform requires or refuses.
                    </>
                  }
                />

                <textarea
                  id="guide-text"
                  value={text}
                  onChange={(event) => setText(event.target.value)}
                  placeholder={EXAMPLE}
                  rows={12}
                  spellCheck
                  aria-invalid={textProblem !== null}
                  className={`field resize-y font-mono text-[13px] leading-relaxed ${textProblem ? '!border-red-500 !ring-red-500' : ''}`}
                />

                <p className={`text-xs tabular-nums ${textProblem ? 'text-red-500' : 'text-slate-400'}`}>
                  {textProblem ?? `${bytes(size)} of ${limits ? bytes(limits.textBytes) : ''}`}
                </p>
              </div>
            </div>

            <div className="sticky bottom-0 -mx-4 flex flex-wrap items-center gap-3 border-t border-grey-300 bg-white/95 px-4 py-3 backdrop-blur dark:border-ink-800 dark:bg-ink-950/95 md:mx-0 md:px-0">
              <button type="submit" disabled={!dirty || !complete || titleProblem !== null || textProblem !== null || saving} className="btn-primary">
                {saving && <IconSpinner />}
                Save section
              </button>

              {dirty && (
                <button type="button" onClick={() => take(read)} disabled={saving} className="btn-ghost">
                  Discard changes
                </button>
              )}

              <span className="text-[13px] text-slate-500">
                {!dirty ? (section ? 'Everything is saved.' : 'No section of your own yet.') : complete ? 'Not saved yet.' : 'A title and a text, both.'}
              </span>
            </div>
          </form>

          {preview !== null && (
            <Card title="Your section, once saved" hint="Your title and text as agents will read them, at the end of the guide after a line saying they are yours.">
              <Rendered html={preview} />
            </Card>
          )}

          <Card
            title="The guide as agents read it"
            hint={`${bytes(read.bytes)} of Markdown, handed to an agent whole with every call to platform_guide.${section ? ' Your section is the last.' : ''}`}
          >
            <div className="mb-3 flex gap-1" role="group" aria-label="Show the guide">
              <ViewButton active={!raw} onClick={() => setRaw(false)}>Rendered</ViewButton>
              <ViewButton active={raw} onClick={() => setRaw(true)}>Markdown</ViewButton>
            </div>

            <div className="max-h-[40rem] overflow-y-auto border border-grey-300 p-4 [scrollbar-width:thin] dark:border-ink-800">
              {raw ? (
                <pre className="whitespace-pre-wrap break-words font-mono text-[12px] leading-relaxed text-slate-700 dark:text-slate-300">{read.guide}</pre>
              ) : (
                <Rendered html={rendered ?? ''} />
              )}
            </div>
          </Card>

          <Card title="For agents">
            <p className="text-[13px] text-slate-600 dark:text-slate-400">
              An agent you give the admin token changes the same: <code className="font-mono text-[12px]">PUT /api/v1/admin/guide</code>{' '}
              with the title and the text, <code className="font-mono text-[12px]">DELETE /api/v1/admin/guide</code> to take it out.
            </p>
          </Card>
        </div>
      )}

      <Dialog
        title="Remove your section from the guide?"
        open={confirming}
        onClose={() => setConfirming(false)}
        footer={
          <>
            <button type="button" onClick={() => setConfirming(false)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={() => void remove()} disabled={removing} className="btn-primary">
              {removing && <IconSpinner />}
              Remove
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Agents read the platform&rsquo;s guide alone from their next call on. The title and the text are deleted: to bring them
          back, write them again.
        </p>
      </Dialog>
    </Section>
  );
}

function Label({ id, title, note }: { id: string; title: string; note: ReactNode }) {
  return (
    <div className="min-w-0">
      <label htmlFor={id} className="text-[15px] font-medium">{title}</label>
      <p className="mt-1 text-[13px] text-slate-500">{note}</p>
    </div>
  );
}

/** Markdown as the rest of the site shows it: written by whoever, so raw markup is text. */
function Rendered({ html }: { html: string }) {
  const holder = useRef<HTMLDivElement>(null);

  return <MarkdownPage html={html} holder={holder} />;
}

function ViewButton({ active, onClick, children }: { active: boolean; onClick: () => void; children: ReactNode }) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={active}
      className={`px-2.5 py-1 text-[13px] ${active
        ? 'bg-slate-900 text-white dark:bg-slate-100 dark:text-ink-950'
        : 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-ink-850'}`}
    >
      {children}
    </button>
  );
}
