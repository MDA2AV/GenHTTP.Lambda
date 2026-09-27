import { useState } from 'react';

import { ABUSE_MAILBOX } from '../abuse';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { Dialog } from './Dialog';

/**
 * The way for somebody who is not a user of this platform to say that
 * something hosted on it is doing them harm.
 *
 * It is a mailbox rather than a form on purpose. A report needs a reply, and
 * the person making it is almost never signed in to anything here - a form
 * would take their words and give them nothing to follow up with.
 */
export function ReportAbuse() {
  const [open, setOpen] = useState(false);
  const t = useT();
  const said = t.abuse;

  const strong = (text: string) => <strong className="font-medium text-slate-700 dark:text-slate-300">{text}</strong>;

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="text-xs text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400"
      >
        {said.report}
      </button>

      <Dialog
        title={said.title}
        open={open}
        onClose={() => setOpen(false)}
        footer={
          <>
            <button type="button" onClick={() => setOpen(false)} className="btn-ghost">
              {t.common.close}
            </button>

            <a className="btn-primary" href={`mailto:${ABUSE_MAILBOX}?subject=${encodeURIComponent(said.subject)}`}>
              {said.write}
            </a>
          </>
        }
      >
        <div className="space-y-3 text-sm leading-relaxed text-slate-600 dark:text-slate-400">
          <p>{said.intro}</p>

          <p>
            {said.how(
              <a className="font-mono text-accent-600 hover:underline dark:text-accent-400" href={`mailto:${ABUSE_MAILBOX}`}>
                {ABUSE_MAILBOX}
              </a>,
              strong,
              <span className="font-mono">/lambda/some-key/</span>,
            )}
          </p>

          <p>
            {said.next(strong, (text) => (
              <Link
                to="/terms"
                target="_blank"
                rel="noreferrer"
                className="text-accent-600 hover:underline dark:text-accent-400"
              >
                {text}
              </Link>
            ))}
          </p>

          <p className="text-xs">{said.danger}</p>
        </div>
      </Dialog>
    </>
  );
}
