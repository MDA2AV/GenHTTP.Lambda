import { useEffect, useRef, useState } from 'react';

/**
 * Shows its children once they have been scrolled to.
 *
 * The observer is disconnected on the first sighting: this is an entrance, and
 * something that fades out again when scrolled past is a distraction rather
 * than an introduction.
 */
export function Reveal({
  children,
  delay = 0,
  className = '',
}: {
  children: React.ReactNode;
  delay?: number;
  className?: string;
}) {
  const host = useRef<HTMLDivElement>(null);
  const [shown, setShown] = useState(false);

  useEffect(() => {
    const element = host.current;

    if (element === null) {
      return;
    }

    // asked not to animate: it is simply there, and no observer is needed
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      setShown(true);
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          setShown(true);
          observer.disconnect();
        }
      },
      // a little before the edge, so it has arrived by the time it is read -
      // and everything above the view counts too, because a fast fling can
      // carry an element from below the screen to above it without it ever
      // being reported as inside, which left it invisible for good in Safari
      { rootMargin: '100000px 0px -12% 0px' },
    );

    observer.observe(element);

    return () => observer.disconnect();
  }, []);

  return (
    <div
      ref={host}
      className={`transition-all duration-700 ease-out motion-reduce:transition-none ${
        shown ? 'translate-y-0 opacity-100' : 'translate-y-6 opacity-0'
      } ${className}`}
      style={{ transitionDelay: shown ? `${delay}ms` : '0ms' }}
    >
      {children}
    </div>
  );
}
