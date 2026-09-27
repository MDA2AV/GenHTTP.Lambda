import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

import { api } from '../api';
import { IconAlert } from '../components/Icons';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePageMeta } from '../meta';

/**
 * Reached when the server could not answer a /lambda/:publicKey request - the
 * key is unknown, or the lambda behind it is not deployed right now.
 */
export function LambdaMissing() {
  const t = useT();

  usePageMeta({ title: t.missing.title, index: false });

  const { publicKey = '' } = useParams();
  const [exists, setExists] = useState<boolean | null>(null);

  useEffect(() => {
    let active = true;

    api
      .key(publicKey)
      .then((status) => active && setExists(status.exists))
      .catch(() => active && setExists(null));

    return () => {
      active = false;
    };
  }, [publicKey]);

  return (
    <div className="mx-auto flex w-full max-w-xl flex-col items-start px-5 py-24">
      <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-amber-500/10 text-amber-500">
        <IconAlert className="h-5 w-5" />
      </div>

      <h1 className="mt-5 text-2xl font-bold tracking-tight">{t.missing.heading}</h1>

      <p className="mt-3 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
        {(exists === true ? t.missing.notDeployed : t.missing.unknown)(
          <code className="font-mono text-sm">{publicKey}</code>,
        )}
      </p>

      <div className="mt-8 flex flex-wrap gap-3">
        <Link to="/editor/create" className="btn-primary px-5 py-2.5">
          {t.missing.create}
        </Link>
        <Link to="/" className="btn-ghost px-5 py-2.5">
          {t.common.backToStart}
        </Link>
      </div>
    </div>
  );
}
