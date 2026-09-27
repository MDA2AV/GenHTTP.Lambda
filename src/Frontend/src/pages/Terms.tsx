import { ABUSE_MAILBOX } from '../abuse';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePublicPage } from '../meta';
import { useLifetimes } from '../site';

const LINK = 'text-accent-600 hover:underline dark:text-accent-400';

/**
 * The terms in full, as their own page so they can be linked to.
 *
 * The editor shows the short version beside the checkbox, because a wall of
 * text above a button is not read by anyone. This is what that short version
 * is short for, and the two have to agree - the limits here are read from the
 * installation rather than written down twice.
 *
 * Every language has them, and every translation says that the English ones
 * are the ones that apply.
 */
export function Terms() {
  usePublicPage('/terms');

  const said = useT().terms;
  const part = said.sections;

  const { lifetimeHours: hours, retentionDays: days } = useLifetimes();

  return (
    <div className="mx-auto w-full max-w-2xl px-5 py-14 sm:py-20">
      <h1 className="text-2xl font-bold tracking-tight">{said.title}</h1>

      {said.binding && (
        <p className="mt-4 border-l-2 border-accent-500/50 pl-4 text-sm text-slate-500 dark:border-accent-400/50">
          {said.binding((text) => (
            // a link to the other language, which has to leave this one
            <a className={LINK} href="/en/terms" hrefLang="en" lang="en">
              {text}
            </a>
          ))}
        </p>
      )}

      <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">{said.intro}</p>

      <Section title={part.forbiddenTitle}>
        {part.forbidden.map((paragraph) => (
          <p key={paragraph}>{paragraph}</p>
        ))}
      </Section>

      <Section title={part.actionTitle}>
        <p>{part.action}</p>
      </Section>

      <Section title={part.lastingTitle}>
        <p>{part.lasting(hours, days)}</p>
      </Section>

      <Section title={part.keyTitle}>
        <p>{part.key}</p>
      </Section>

      <Section title={part.warrantyTitle}>
        <p>{part.warranty}</p>
      </Section>

      <Section title={part.reportTitle}>
        <p>
          {part.report(
            <a className={LINK} href={`mailto:${ABUSE_MAILBOX}`}>
              {ABUSE_MAILBOX}
            </a>,
            (text) => (
              <Link className={LINK} to="/">
                {text}
              </Link>
            ),
          )}
        </p>
      </Section>

      <p className="mt-10 text-xs text-slate-500">{said.change}</p>
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
