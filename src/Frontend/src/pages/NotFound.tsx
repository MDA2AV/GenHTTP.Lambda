import { Link } from 'react-router-dom';
import { usePageMeta } from '../meta';

export function NotFound() {
  usePageMeta({ title: 'Page Not Found', index: false });

  return (
    <div className="mx-auto flex w-full max-w-xl flex-col items-start px-5 py-24">
      <h1 className="text-2xl font-bold tracking-tight">This page does not exist</h1>
      <p className="mt-3 text-[15px] text-slate-600 dark:text-slate-400">
        The link may be stale, or the lambda it pointed at has been deleted.
      </p>
      <Link to="/" className="btn-primary mt-8 px-5 py-2.5">
        Back to the start
      </Link>
    </div>
  );
}
