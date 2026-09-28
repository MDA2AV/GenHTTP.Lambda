import type { ReactNode } from 'react';

import { OPERATOR } from '../contact';
import { useT } from '../i18n';

/** A link inside the running text of a legal page. */
export const LEGAL_LINK = 'text-accent-600 hover:underline dark:text-accent-400';

interface Props {
  title: string;
  /** What a translation says about the English version being the one that applies; nothing in English. */
  binding: ((english: (text: string) => ReactNode) => ReactNode) | null;
  /** Where the English version is. */
  english: string;
  intro: ReactNode;
  children: ReactNode;
}

/**
 * The frame of the terms and the privacy policy: one narrow column to be read
 * from the top, with the note that a translation is only a translation.
 */
export function LegalPage({ title, binding, english, intro, children }: Props) {
  return (
    <div className="mx-auto w-full max-w-2xl px-5 py-14 sm:py-20">
      <h1 className="text-2xl font-bold tracking-tight">{title}</h1>

      {binding && (
        <p className="mt-4 border-l-2 border-accent-500/50 pl-4 text-sm text-slate-500 dark:border-accent-400/50">
          {binding((text) => (
            // a link to the other language, which has to leave this one
            <a className={LEGAL_LINK} href={english} hrefLang="en" lang="en">
              {text}
            </a>
          ))}
        </p>
      )}

      <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">{intro}</p>

      {children}
    </div>
  );
}

export function LegalSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="mt-8">
      <h2 className="text-base font-semibold">{title}</h2>
      <div className="mt-2 space-y-3 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{children}</div>
    </section>
  );
}

/**
 * Who runs this site and where, as the privacy policy and the legal notice
 * both give it - with whatever else belongs under it, like a mailbox.
 */
export function OperatorAddress({ children }: { children?: ReactNode }) {
  const country = useT().common.operatorCountry;

  return (
    <address className="not-italic">
      {OPERATOR.name}
      <br />
      {OPERATOR.street}
      <br />
      {OPERATOR.city}
      <br />
      {country}
      {children && (
        <>
          <br />
          {children}
        </>
      )}
    </address>
  );
}
