import { IconExternal } from './Icons';

interface Props {
  title: string;
  description: string;
  publicKey: string;
  /** Where the picture is, or a data address while it has not been saved yet. */
  image?: string | null;
  /** Where the card leads; none for the preview in the editor. */
  href?: string;
}

/**
 * One lambda on the showcase page.
 *
 * The picture carries it, so the picture gets most of the card and the words
 * are what is left: a title, a few lines cut where they overflow, and the
 * address it answers at, because that is what somebody will actually open.
 * The editor draws its preview with the same card, so what an owner sees
 * while writing is exactly what everybody else will see.
 */
export function ShowcaseCard({ title, description, publicKey, image, href }: Props) {
  const body = (
    <>
      <div className="relative aspect-[16/10] overflow-hidden bg-grey-100 dark:bg-ink-850">
        {image ? (
          <img
            src={image}
            alt=""
            loading="lazy"
            decoding="async"
            className="h-full w-full object-cover transition-transform duration-500 ease-out group-hover:scale-[1.03]"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-sm text-slate-400">No picture yet</div>
        )}
      </div>

      <div className="flex flex-1 flex-col p-4">
        <h3 className="line-clamp-1 font-semibold tracking-tight transition-colors group-hover:text-accent-600 dark:group-hover:text-accent-400">
          {title || <span className="text-slate-400">Title</span>}
        </h3>

        <p className="mt-1.5 line-clamp-3 flex-1 whitespace-pre-line text-sm leading-relaxed text-slate-600 dark:text-slate-400">
          {description || <span className="text-slate-400">What a visitor can do with it.</span>}
        </p>

        <p className="mt-3 flex items-center gap-1.5 font-mono text-xs text-slate-500">
          <span className="truncate">/lambda/{publicKey}/</span>
          <IconExternal className="h-3 w-3 shrink-0 opacity-0 transition-opacity group-hover:opacity-100" />
        </p>
      </div>
    </>
  );

  const frame = 'surface group flex h-full flex-col overflow-hidden transition-[border-color,box-shadow] duration-200';

  if (!href) {
    return <div className={frame}>{body}</div>;
  }

  return (
    <a
      href={href}
      target="_blank"
      rel="noreferrer"
      className={`${frame} hover:border-accent-500/60 hover:shadow-lg focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent-500 dark:hover:border-accent-400/50`}
      aria-label={`${title}, opens /lambda/${publicKey}/ in a new tab`}
    >
      {body}
    </a>
  );
}
