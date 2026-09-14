import { IconLock } from './Icons';

/**
 * What a page behind the token shows to someone who has not presented one.
 *
 * There is no form here on purpose: the token is asked for in one place, the
 * menu in the header, so that unlocking once opens everything it covers rather
 * than each page asking again in its own way.
 */
export function Locked({ title, what, denied }: { title: string; what: string; denied?: boolean }) {
  return (
    <div className="mx-auto w-full max-w-md px-5 py-24 text-center">
      <IconLock className="mx-auto h-8 w-8 text-slate-400" />

      <h1 className="mt-4 text-xl font-bold tracking-tight">{title}</h1>

      <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
        {denied
          ? 'That token was not accepted, or this installation has no administration.'
          : what}
      </p>

      <p className="mt-4 text-sm text-slate-500">
        Open <strong className="font-medium">Admin</strong> at the top of the page and enter the token.
      </p>
    </div>
  );
}
