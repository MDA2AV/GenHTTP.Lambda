import { isOwn, useBrand, type Brand } from '../brand';
import { Link } from '../i18n/links';
import { IconLogo } from './Icons';

/**
 * The installation's name and picture at the start of the bar, leading to its
 * front page: GenHTTP Lambda's mark and name, or its operator's.
 *
 * The platform's own is drawn as it always was, to the byte, since the pages
 * of every installation that is not branded were rendered with it.
 */
export function BrandLink({ label, nameClassName }: {
  /** What a screen reader calls the link, where the name may be out of sight. */
  label?: string;
  /** Where the name is shown, if not always. */
  nameClassName?: string;
}) {
  const brand = useBrand();

  // a brand of the operator's gives way to the links of the bar rather than
  // pushing them out of it: the name is cut short instead
  const room = isOwn(brand) ? 'min-w-0' : 'shrink-0';

  return (
    <Link to="/" className={`flex ${room} items-center gap-2.5 whitespace-nowrap font-semibold tracking-tight`} aria-label={label}>
      <BrandMark brand={brand} nameClassName={nameClassName} />
    </Link>
  );
}

/**
 * A brand as the bar draws it: the platform's mark with its name, or the
 * operator's picture with theirs - for the bar, and for the panel to show
 * what a brand not saved yet will look like.
 *
 * Their name in plain words, beside their picture at the height of the
 * platform's mark: a square one reads best, a wide one is held to a width
 * that leaves the bar room on a phone.
 */
export function BrandMark({ brand, nameClassName }: { brand: Brand; nameClassName?: string }) {
  if (brand.logo === null) {
    return (
      <>
        <IconLogo />
        {/* the name is the mark plus the letter the thing is named after */}
        <span className={nameClassName}>
          GenHTTP{' '}
          <span className="text-accent-500" title="Lambda">
            <span aria-hidden="true" className="text-[1.15em] leading-none">λ</span>
            <span className="sr-only">Lambda</span>
          </span>
        </span>
      </>
    );
  }

  return (
    <>
      <img src={brand.logo} alt="" className="h-6 w-auto max-w-[7.5rem] shrink-0 object-contain" />
      <span className={nameClassName ? `min-w-0 truncate ${nameClassName}` : 'min-w-0 truncate'}>{brand.name}</span>
    </>
  );
}
