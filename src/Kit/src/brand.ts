import { useSyncExternalStore } from 'react';

/**
 * How the installation presents itself on its pages: the name they show, and
 * the picture beside it.
 *
 * GenHTTP Lambda's own, drawn with the platform's mark, until its operator
 * gives it a name or a logo - where its host lets them, which the community
 * edition does. From then on the bar of every page draws theirs, the titles
 * end in their name, and the tab shows their picture: their logo, or the
 * first letter of their name on a plate where they gave none.
 *
 * The server hands it to the scripts with every page it sends
 * (`#lambda-brand`, see Web/PageBranding.cs), and only where it is not the
 * platform's - so the first render draws what the page was sent with, and
 * nothing is asked for before the bar is drawn.
 *
 * The colours and the fonts a page is drawn with are no part of it: they are
 * values of its style sheet (`#lambda-look`, for what index.css leaves open),
 * so no script has to know whose they are.
 */
export interface Brand {
  /** The name the pages show: GenHTTP Lambda, or what the operator named it. */
  name: string;
  /**
   * Where the picture beside the name is: the operator's logo, or the first
   * letter of their name on a plate. Nothing where the pages draw the
   * platform's mark, which is how they tell the platform's own apart.
   */
  logo: string | null;
}

/** GenHTTP Lambda, as every installation is until its operator says otherwise. */
export const PLATFORM: Brand = { name: 'GenHTTP Lambda', logo: null };

/**
 * A brand made of placeholders, which the build renders the pages of a site
 * that may be branded with a second time, and the server fills in (see
 * Web/Provider/SitePrerender.cs) - a brand is drawn as other markup than the
 * platform's mark, not as another value in it.
 */
export const BRAND_PLACEHOLDERS: Brand = { name: '__LAMBDA_BRAND_NAME__', logo: '__LAMBDA_BRAND_LOGO__' };

/** Whether the installation presents itself as its operator's rather than as GenHTTP Lambda. */
export const isOwn = (brand: Brand) => brand.logo !== null;

const ELEMENT = 'lambda-brand';

/** The style sheet the server sends a page with where its colours or fonts are the operator's. */
const LOOK = 'lambda-look';

let rendering: Brand | null = null;

/** Called by the build before rendering a page, never in a browser. */
export function prerenderBrand(brand: Brand) {
  rendering = brand;
}

let current: Brand | undefined;

const listeners = new Set<() => void>();

/** The brand the page was sent with, or the platform's where it was sent with none. */
function read(): Brand {
  if (rendering !== null) {
    return rendering;
  }

  if (current === undefined) {
    try {
      const element = document.getElementById(ELEMENT);
      const said = element?.textContent ? (JSON.parse(element.textContent) as Partial<Brand>) : null;

      current = said && typeof said.name === 'string' && typeof said.logo === 'string' ? { name: said.name, logo: said.logo } : PLATFORM;
    } catch {
      current = PLATFORM;
    }
  }

  return current;
}

const subscribe = (changed: () => void) => {
  listeners.add(changed);
  return () => listeners.delete(changed);
};

/** How the installation presents itself, as the page was sent - and as the operator changes it in the panel. */
export function useBrand(): Brand {
  return useSyncExternalStore(subscribe, read, read);
}

/**
 * The brand as the operator just saved it, which every part of the open page
 * draws from now on - the bar, the title, the icon of the tab - without the
 * page being loaded again. Every page opened afterwards is sent with it.
 */
export function changeBrand(brand: Brand) {
  current = brand;

  showIcon(brand);

  listeners.forEach((listener) => listener());
}

/**
 * The colours and the fonts as the operator just saved them, in which the
 * open page is drawn from now on - or the platform's own, where there are
 * none any more.
 */
export function changeLook(sheet: string | null) {
  document.getElementById(LOOK)?.remove();

  if (sheet !== null) {
    const style = document.createElement('style');

    style.id = LOOK;
    style.textContent = sheet;

    document.head.appendChild(style);
  }
}

/** The icon of the tab: the brand's picture, or the bundle's own icons for the platform's. */
function showIcon(brand: Brand) {
  for (const link of Array.from(document.head.querySelectorAll('link[rel~="icon"], link[rel="apple-touch-icon"]'))) {
    link.remove();
  }

  const add = (rel: string, href: string, type?: string) => {
    const link = document.createElement('link');

    link.rel = rel;
    link.href = href;

    if (type) {
      link.type = type;
    }

    document.head.appendChild(link);
  };

  if (brand.logo !== null) {
    add('icon', brand.logo);
  } else {
    const base = import.meta.env.BASE_URL;

    add('icon', `${base}favicon.svg`, 'image/svg+xml');
    add('apple-touch-icon', `${base}apple-touch-icon.png`);
  }
}
