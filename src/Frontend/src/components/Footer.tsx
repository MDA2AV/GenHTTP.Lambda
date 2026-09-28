import { CONTACT_MAIL, DISCORD } from '../contact';
import { useLanguage, useT } from '../i18n';
import { Link } from '../i18n/links';
import { ReportAbuse } from './ReportAbuse';

const LINK = 'text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400';

/**
 * The bottom of every page that scrolls.
 *
 * Small, and under every page rather than behind a lambda: whoever needs the
 * report link is here because something hosted here did something to them,
 * and they have no reason to know the rest of this. The terms and the privacy
 * policy have to be a step away from wherever a visitor happens to be.
 *
 * The Impressum is linked from the German pages only, which is the operator's
 * choice: § 5 DDG binds a site run from Germany in whatever language it is
 * read, so linking it everywhere is the safe setting. The page itself exists
 * in every language either way.
 *
 * The editor and the admin panel fill the screen and have none. They are
 * somewhere a visitor works, not a page they read to the end.
 */
export function Footer() {
  const said = useT().shell;
  const language = useLanguage();

  return (
    <footer className="relative z-10 mx-auto w-full max-w-4xl px-5 pb-10">
      <div className="flex flex-wrap items-center justify-center gap-x-5 gap-y-2 border-t border-grey-300 pt-6 text-xs text-grey-700 dark:border-ink-800 dark:text-grey-300">
        <ReportAbuse />

        <Link to="/terms" className={LINK}>
          {said.terms}
        </Link>

        <Link to="/privacy" className={LINK}>
          {said.privacy}
        </Link>

        {language === 'de' && (
          <Link to="/imprint" className={LINK}>
            {said.imprint}
          </Link>
        )}

        <Link to="/editor/create" className={LINK}>
          {said.writeCode}
        </Link>

        <a href={`mailto:${CONTACT_MAIL}`} className={LINK}>
          {said.contact}
        </a>

        <a href={DISCORD} target="_blank" rel="noreferrer" className={LINK}>
          Discord
        </a>

        {/* the header carries this on anything wider than a phone */}
        <Link to="/admin" className={`${LINK} sm:hidden`}>
          {said.admin}
        </Link>
      </div>
    </footer>
  );
}
