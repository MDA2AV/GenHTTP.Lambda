import { useEffect, useState, type ReactNode } from 'react';
import { createPortal } from 'react-dom';

import { IconClose, IconLock } from '@genhttp-lambda/kit/components/Icons';

/** Where the screenshots of the features page are, as the bundle serves them. */
const MEDIA = '/media/platform';

interface Props {
  /**
   * The picture, by its name: "records" is records-light.webp and
   * records-dark.webp, so it matches the theme the page is read in - or
   * records.webp alone, with single, for a page that has one look.
   */
  name: string;
  single?: boolean;
  /** Its size in pixels, so the page keeps its place while it loads. */
  width: number;
  height: number;
  alt: string;
  /** The address in the bar above it, as a browser would show it. */
  address: string;
  /** A word beside the address - that it is a draft, say. */
  badge?: ReactNode;
  /** What "show larger" is called, for whoever cannot see the picture. */
  zoom: string;
  close: string;
}

/**
 * A screenshot in the frame of a browser window: the address it was taken
 * at above it, and the picture of the theme the page is in - both are in the
 * markup, the stylesheet shows one, and the other is never loaded. Pressing
 * it shows it as large as the screen allows, since a picture of a whole
 * editor is smaller than it was taken.
 */
export function Screen({ name, single, width, height, alt, address, badge, zoom, close }: Props) {
  const [open, setOpen] = useState(false);

  const light = `${MEDIA}/${name}${single ? '' : '-light'}.webp`;
  const dark = `${MEDIA}/${name}${single ? '' : '-dark'}.webp`;

  const pictures = (className: string) =>
    single ? (
      <img src={light} alt={alt} width={width} height={height} loading="lazy" decoding="async" className={className} />
    ) : (
      <>
        <img src={light} alt={alt} width={width} height={height} loading="lazy" decoding="async" className={`${className} dark:hidden`} />
        <img src={dark} alt={alt} width={width} height={height} loading="lazy" decoding="async" className={`${className} hidden dark:block`} />
      </>
    );

  return (
    <figure className="surface overflow-hidden shadow-xl shadow-grey-900/5 dark:shadow-black/30">
      <div className="flex items-center gap-3 border-b border-grey-200 bg-grey-50 px-3.5 py-2.5 dark:border-ink-800 dark:bg-ink-850" aria-hidden="true">
        <span className="flex shrink-0 gap-1.5">
          <span className="h-2.5 w-2.5 rounded-full bg-grey-300 dark:bg-ink-700" />
          <span className="h-2.5 w-2.5 rounded-full bg-grey-300 dark:bg-ink-700" />
          <span className="h-2.5 w-2.5 rounded-full bg-grey-300 dark:bg-ink-700" />
        </span>
        <span className="flex min-w-0 flex-1 items-center gap-1.5 bg-white px-2.5 py-1 font-mono text-xs text-grey-700 dark:bg-ink-900 dark:text-grey-300">
          <IconLock className="h-3 w-3 shrink-0 text-grey-500" />
          <span className="truncate">{address}</span>
        </span>
        {badge}
      </div>

      <button type="button" onClick={() => setOpen(true)} className="block w-full cursor-zoom-in" aria-label={`${zoom}: ${alt}`}>
        {pictures('block h-auto w-full')}
      </button>

      {/* only opened in a browser, which knows the theme: the larger one is the picture of that one alone */}
      {open && (
        <Larger onClose={() => setOpen(false)} close={close}>
          <img
            src={single || !document.documentElement.classList.contains('dark') ? light : dark}
            alt={alt}
            width={width}
            height={height}
            className="mx-auto block h-auto max-h-[90vh] w-auto max-w-full"
          />
        </Larger>
      )}
    </figure>
  );
}

/** The picture over the page, as large as the screen allows; any click or escape closes it. */
function Larger({ children, onClose, close }: { children: ReactNode; onClose: () => void; close: string }) {
  useEffect(() => {
    const onKey = (event: KeyboardEvent) => event.key === 'Escape' && onClose();

    window.addEventListener('keydown', onKey);
    // the page under it stays where it was
    const overflow = document.documentElement.style.overflow;
    document.documentElement.style.overflow = 'hidden';

    return () => {
      window.removeEventListener('keydown', onKey);
      document.documentElement.style.overflow = overflow;
    };
  }, [onClose]);

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      className="fixed inset-0 z-50 flex cursor-zoom-out items-center justify-center bg-grey-900/80 p-3 backdrop-blur-sm sm:p-6"
      onClick={onClose}
    >
      <button type="button" onClick={onClose} className="absolute right-3 top-3 p-2 text-white/80 hover:text-white" aria-label={close}>
        <IconClose className="h-6 w-6" />
      </button>
      <div className="shadow-2xl">{children}</div>
    </div>,
    document.body,
  );
}
