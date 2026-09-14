import { useState } from 'react';
import { Link } from 'react-router-dom';

import { ABUSE_MAILBOX } from '../abuse';
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

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="text-xs text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400"
      >
        Report abuse
      </button>

      <Dialog
        title="Report a lambda"
        open={open}
        onClose={() => setOpen(false)}
        footer={
          <>
            <button type="button" onClick={() => setOpen(false)} className="btn-ghost">
              Close
            </button>

            <a className="btn-primary" href={`mailto:${ABUSE_MAILBOX}?subject=${encodeURIComponent('Abuse report')}`}>
              Write to us
            </a>
          </>
        }
      >
        <div className="space-y-3 text-sm leading-relaxed text-slate-600 dark:text-slate-400">
          <p>
            Anybody can put code online here, which means somebody sometimes puts up something they should
            not. If a page hosted here is trying to trick people, attacking something, or using material it
            has no right to, tell us and we will take it down.
          </p>

          <p>
            Write to{' '}
            <a
              className="font-mono text-accent-600 hover:underline dark:text-accent-400"
              href={`mailto:${ABUSE_MAILBOX}`}
            >
              {ABUSE_MAILBOX}
            </a>{' '}
            and include <strong className="font-medium text-slate-700 dark:text-slate-300">the address of the
            page</strong> - it looks like <span className="font-mono">/lambda/some-key/</span> - and a
            sentence about what is wrong with it. A screenshot helps. You do not need an account and you do
            not need to be a user of this site.
          </p>

          <p>
            <strong className="font-medium text-slate-700 dark:text-slate-300">What happens next.</strong> A
            person reads it. If it breaks the{' '}
            <Link
              to="/terms"
              target="_blank"
              rel="noreferrer"
              className="text-accent-600 hover:underline dark:text-accent-400"
            >
              terms of service
            </Link>
            , the lambda is taken offline, usually within a day. We will not tell you who put it there, and
            we cannot promise to write back about every report - but every one of them is read.
          </p>

          <p className="text-xs">
            If somebody is in immediate danger, or a crime is being committed, please contact your local
            authorities as well. We can remove a page; we cannot do anything else.
          </p>
        </div>
      </Dialog>
    </>
  );
}
