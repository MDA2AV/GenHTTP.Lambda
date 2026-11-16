import { useEffect, useId, useLayoutEffect, useRef, useState, type ReactNode, type RefObject } from 'react';
import { Link } from 'react-router-dom';

import { IconChevronDown, IconSpinner } from './Icons';

/*
 * The sections of a page that has many - the editor's, the operator's panel -
 * down a sidebar where there is room for one, and as a bar that opens them all
 * on a phone. The words are the page's: it hands in every label.
 */

/**
 * How much a section asks to be looked at: something went wrong, something
 * is wanted that is not there, something is under way, something ended.
 * In that order, which is the order a summary of several picks from.
 */
export type Call = 'problem' | 'wanted' | 'busy' | 'done';

const URGENCY: Call[] = ['problem', 'wanted', 'busy', 'done'];

/** One section, as the navigation lists it. */
export interface NavEntry {
  id: string;
  to: string;
  label: string;
  /** What is worth noticing about it, beside its name: a count, a dot, a spinner. */
  marker?: ReactNode;
  /** Whether that is something to look at - a dot or a spinner rather than a count. */
  calls?: Call;
}

/** Sections that belong together, under a name - or none, for what comes first. */
export interface NavGroup {
  id: string;
  label: string | null;
  entries: NavEntry[];
}

interface Props {
  /** What the list is called, for whoever cannot see it. */
  label: string;
  groups: NavGroup[];
  /** The section open, if it is one of them. */
  current: string;
  /** Opens a section, or reads the one already open again. */
  onOpen: (entry: NavEntry) => void;
}

const heading = 'px-3 pb-1 pt-5 text-[11px] font-medium uppercase tracking-wide text-slate-400';

/**
 * The sections in the sidebar, where there is room for a column: every one
 * of them in sight, in its group, the one open marked.
 */
export function SideNav({ label, groups, current, onOpen }: Props) {
  return (
    <nav aria-label={label} className="mt-2 hidden flex-col gap-0.5 md:flex">
      {groups.map((group) => (
        <div key={group.id} className="contents">
          {group.label && <span className={heading}>{group.label}</span>}
          {group.entries.map((entry) => (
            <Entry
              key={entry.id}
              entry={entry}
              current={entry.id === current}
              onOpen={onOpen}
              className="border-l-2 px-3 py-2 text-sm"
              on="border-accent-500 bg-slate-100 font-medium text-ink-900 dark:border-accent-400 dark:bg-ink-850 dark:text-slate-100"
              off="border-transparent text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200"
            />
          ))}
        </div>
      ))}
    </nav>
  );
}

/**
 * The sections on a phone, which has no room for a column beside the page,
 * nor for twenty of them in a row along its top - a row that scrolls shows
 * the first few and hides the rest past the edge, where nobody looks.
 *
 * So the section open is named in a bar that stays at the top as the page
 * moves, and the bar opens every section at once: the groups the sidebar
 * has, two columns of them, in one screen. Where something wants a look
 * that the bar hides - the log had a problem, the agent finished - the bar
 * says so with the same dot, so a closed list keeps nothing back. Whatever
 * would be at the foot of the sidebar is at the foot of the list.
 *
 * Opened, the list takes the screen below the bar: the page moves the bar to
 * the top first, so the list never opens into the little room left under
 * what the page shows above it - the lambda's name, say.
 */
export function PhoneNav({ label, groups, current, title, onOpen, footer, scroller }: Props & {
  /** What the section open is called, for when the list does not have it - the simple view, come to by a link to the code. */
  title: string;
  /** What the sidebar has under its sections, handed what closes the list. */
  footer?: (close: () => void) => ReactNode;
  /** What scrolls the page, to bring the bar to the top as the list opens. */
  scroller: RefObject<HTMLElement>;
}) {
  const [open, setOpen] = useState(false);
  const [room, setRoom] = useState<number | null>(null);

  const holder = useRef<HTMLDivElement>(null);
  const button = useRef<HTMLButtonElement>(null);
  const list = useRef<HTMLDivElement>(null);
  const id = useId();

  const all = groups.flatMap((group) => group.entries);
  const here = all.find((entry) => entry.id === current);
  const within = groups.find((group) => group.entries.some((entry) => entry.id === current));

  // what the others ask for, summed up as the most urgent of it
  const calling = URGENCY.find((call) => all.some((entry) => entry.id !== current && entry.calls === call));

  const close = (refocus = false) => {
    setOpen(false);

    if (refocus) {
      button.current?.focus();
    }
  };

  // the bar to the top, and the list the room below it
  useLayoutEffect(() => {
    if (!open || !button.current) {
      return;
    }

    const page = scroller.current;

    if (page && page.scrollHeight > page.clientHeight) {
      page.scrollTop += button.current.getBoundingClientRect().top - page.getBoundingClientRect().top;
    }

    setRoom(Math.max(240, window.innerHeight - button.current.getBoundingClientRect().bottom));

    // the section open is where somebody using keys goes on from
    list.current?.querySelector<HTMLElement>('[aria-current="page"]')?.focus({ preventScroll: true });
  }, [open, scroller]);

  useEffect(() => {
    if (!open) {
      return;
    }

    const escape = (event: KeyboardEvent) => event.key === 'Escape' && close(true);

    const outside = (event: PointerEvent) => {
      if (!holder.current?.contains(event.target as Node)) {
        close();
      }
    };

    document.addEventListener('keydown', escape);
    document.addEventListener('pointerdown', outside);

    return () => {
      document.removeEventListener('keydown', escape);
      document.removeEventListener('pointerdown', outside);
    };
  }, [open]);

  return (
    <div ref={holder} className="sticky top-0 z-20 md:hidden">
      {/* the page under the list is out of play while it is open, and a tap
          on it closes the list - it is inside the holder, so the listener for
          taps outside would not */}
      {open && <div aria-hidden="true" onClick={() => close()} className="fixed inset-0 bg-slate-900/20 dark:bg-black/50" />}

      <button
        ref={button}
        type="button"
        onClick={() => setOpen((was) => !was)}
        aria-expanded={open}
        aria-controls={id}
        className="relative flex w-full items-center gap-3 border-b border-slate-200 bg-white px-4 py-2.5 text-left focus-visible:outline focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-accent-500 dark:border-ink-800 dark:bg-ink-950"
      >
        <span className="min-w-0 flex-1">
          <span className="block text-[11px] font-medium uppercase tracking-wide text-slate-400">{within?.label ?? label}</span>
          <span className="block truncate text-[15px] font-medium">{here?.label ?? title}</span>
        </span>
        {!open && calling && <Summary call={calling} />}
        <IconChevronDown className={`h-4 w-4 shrink-0 text-slate-400 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div
          ref={list}
          id={id}
          style={room != null ? { maxHeight: room } : undefined}
          className="absolute inset-x-0 top-full overflow-y-auto overscroll-contain border-b border-slate-200 bg-white shadow-lg dark:border-ink-800 dark:bg-ink-950"
        >
          <nav aria-label={label} className="columns-2 gap-x-2 px-2 pb-3 pt-1">
            {/* a group is kept in one column, unless it is the only one - the
                simple view's - which would leave the other empty */}
            {groups.map((group) => (
              <div key={group.id} className={groups.length > 1 ? 'break-inside-avoid' : 'contents'}>
                {group.label && <div className={`${heading} !pt-4`}>{group.label}</div>}
                {group.entries.map((entry) => (
                  <Entry
                    key={entry.id}
                    entry={entry}
                    current={entry.id === current}
                    onOpen={(chosen) => {
                      close();
                      onOpen(chosen);
                    }}
                    className="min-h-11 px-3 py-2 text-[15px]"
                    on="bg-accent-500/10 font-medium text-accent-700 dark:bg-accent-400/10 dark:text-accent-400"
                    off="text-slate-700 active:bg-slate-100 dark:text-slate-300 dark:active:bg-ink-850"
                  />
                ))}
              </div>
            ))}
          </nav>

          {footer?.(() => close())}
        </div>
      )}
    </div>
  );
}

/** One section to open, with what is worth noticing about it beside its name. */
function Entry({ entry, current, onOpen, className, on, off }: {
  entry: NavEntry;
  current: boolean;
  onOpen: (entry: NavEntry) => void;
  className: string;
  on: string;
  off: string;
}) {
  return (
    <Link
      to={entry.to}
      onClick={(event) => {
        event.preventDefault();
        onOpen(entry);
      }}
      aria-current={current ? 'page' : undefined}
      className={`flex items-center gap-2 focus-visible:outline focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-accent-500 ${className} ${current ? on : off}`}
    >
      <span className="min-w-0 flex-1 break-words">{entry.label}</span>
      {entry.marker}
    </Link>
  );
}

/** The most urgent of what the sections the bar hides ask for. */
function Summary({ call }: { call: Call }) {
  if (call === 'busy') {
    return <IconSpinner className="h-3.5 w-3.5 shrink-0 text-accent-500 dark:text-accent-400" />;
  }

  const colour = call === 'problem' ? 'bg-red-500' : call === 'wanted' ? 'bg-amber-500' : 'bg-emerald-500';

  return <span aria-hidden="true" className={`h-2 w-2 shrink-0 rounded-full ${colour}`} />;
}
