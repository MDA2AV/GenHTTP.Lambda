import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { api, type Platform } from '../api';
import { ABUSE_MAILBOX } from '../abuse';
import { PAGES, usePageMeta } from '../meta';

/**
 * The terms in full, as their own page so they can be linked to.
 *
 * The editor shows the short version beside the checkbox, because a wall of
 * text above a button is not read by anyone. This is what that short version
 * is short for, and the two have to agree - the limits here are read from the
 * installation rather than written down twice.
 */
export function Terms() {
  usePageMeta(PAGES['/terms']);

  const [platform, setPlatform] = useState<Platform | null>(null);

  useEffect(() => {
    api.platform().then(setPlatform).catch(() => undefined);
  }, []);

  const hours = platform?.deploymentLifetimeHours ?? 24;
  const days = platform?.retentionDays ?? 30;

  return (
    <div className="mx-auto w-full max-w-2xl px-5 py-14 sm:py-20">
      <h1 className="text-2xl font-bold tracking-tight">Terms of service</h1>

      <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
        This is a free service for trying things out. It runs code written by strangers on shared
        infrastructure, which is only workable if everybody keeps to a few rules.
      </p>

      <Section title="What you may not put here">
        <p>
          No malware, no phishing, no crypto miners. Nothing that attacks, scans, floods or otherwise
          interferes with other systems, here or anywhere else. Nothing that harasses anybody. Nothing you
          have no right to publish - that includes other people's code, text, images and trademarks.
        </p>
        <p>
          Do not use a lambda to store or forward personal data about other people. There is nothing private
          about a public address, and this platform offers you no way to keep such data safe.
        </p>
      </Section>

      <Section title="What we may do about it">
        <p>
          Anything deployed here can be taken offline or removed at any time, with no notice and no
          obligation to explain. In practice that happens when something breaks the rules above, when it
          threatens the machine everybody else is sharing, or when somebody reports it and they turn out to
          be right.
        </p>
      </Section>

      <Section title="How long anything lasts">
        <p>
          A deployment stays reachable for about {hours} hours. A lambda you have not opened is removed,
          with every version of its code, about {days} days after you last touched it. Saving or deploying
          counts as touching it, so anything you are working on stays. Nothing here is a backup: keep your
          own copy of code you care about.
        </p>
      </Section>

      <Section title="Your editor link is your password">
        <p>
          Anybody who has the editor link can read and change that lambda, and there is no account and no
          password behind it. If you publish the link, you have published the ability to change it. There is
          no way to recover one that is lost.
        </p>
      </Section>

      <Section title="No warranty">
        <p>
          The service is provided as it is, with no guarantee that it works, keeps working, or keeps
          anything you put into it. It may be restarted, changed or switched off at any time. Do not build
          anything on it that matters to you or to anybody else.
        </p>
      </Section>

      <Section title="Reporting something">
        <p>
          If a lambda hosted here is doing something it should not, write to{' '}
          <a className="text-accent-600 hover:underline dark:text-accent-400" href={`mailto:${ABUSE_MAILBOX}`}>
            {ABUSE_MAILBOX}
          </a>{' '}
          with its address. See <Link className="text-accent-600 hover:underline dark:text-accent-400" to="/">the front page</Link> for
          what to include.
        </p>
      </Section>

      <p className="mt-10 text-xs text-slate-500">
        These terms can change. The version that applies is the one on this page.
      </p>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="mt-8">
      <h2 className="text-base font-semibold">{title}</h2>
      <div className="mt-2 space-y-3 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{children}</div>
    </section>
  );
}
