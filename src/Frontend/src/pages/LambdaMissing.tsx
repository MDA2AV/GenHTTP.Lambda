import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';

import { api } from '../api';
import { IconAlert } from '../components/Icons';

/**
 * Reached when the server could not answer a /lambda/:publicKey request - the
 * key is unknown, or the lambda behind it is not deployed right now.
 */
export function LambdaMissing() {
  const { publicKey = '' } = useParams();
  const [exists, setExists] = useState<boolean | null>(null);

  useEffect(() => {
    let active = true;

    api
      .publicStatus(publicKey)
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

      <h1 className="mt-5 text-2xl font-bold tracking-tight">Nothing is running here</h1>

      <p className="mt-3 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
        {exists === true ? (
          <>
            There is a lambda at <code className="font-mono text-sm">{publicKey}</code>, but it is not
            deployed at the moment. Deployments in the free tier stay up while they are used, and are taken down after a month with nobody visiting or editing, having gone
            live - whoever owns the editor link can put it back online.
          </>
        ) : (
          <>
            No lambda is hosted at <code className="font-mono text-sm">{publicKey}</code>. The key may
            never have existed, or the lambda behind it has been deleted.
          </>
        )}
      </p>

      <div className="mt-8 flex flex-wrap gap-3">
        <Link to="/editor/create" className="btn-primary px-5 py-2.5">
          Create a lambda here
        </Link>
        <Link to="/" className="btn-ghost px-5 py-2.5">
          Back to the start
        </Link>
      </div>
    </div>
  );
}
