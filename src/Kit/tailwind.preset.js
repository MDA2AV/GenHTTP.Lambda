/**
 * The look every bundle shares, as a preset: each bundle's tailwind.config.js
 * names it and lists its own sources with the kit's, so the classes the kit's
 * components use are in every bundle's stylesheet.
 *
 * @type {import('tailwindcss').Config}
 */

// The palette Google uses on its own surfaces. The scale names are the ones the
// components already reference, so the theme lives here rather than in markup.
const grey = {
  50: '#f8f9fa',
  100: '#f1f3f4',
  200: '#e8eaed',
  300: '#dadce0',
  400: '#bdc1c6',
  500: '#9aa0a6',
  600: '#80868b',
  700: '#5f6368',
  800: '#3c4043',
  900: '#202124',
};

/** A colour kept as a variable, as its channels - so an opacity can still be written behind it. */
const shade = (variable) => `rgb(var(${variable}) / <alpha-value>)`;

export default {
  darkMode: 'class',
  theme: {
    // Panels, cards and inputs are square - that is the flat look the rest of
    // the page is drawn in. Only "full" survives the flattening, for the things
    // that should read as something to press rather than as a region: a pill
    // has no corners to be boxy with.
    borderRadius: {
      none: '0',
      sm: '0',
      DEFAULT: '0',
      md: '0',
      lg: '0',
      xl: '0',
      '2xl': '0',
      '3xl': '0',
      full: '9999px',
    },
    extend: {
      colors: {
        grey,
        // surfaces of the dark theme, darkest first
        ink: {
          950: '#202124',
          900: '#292a2d',
          850: '#303134',
          800: '#3c4043',
          700: '#5f6368',
          600: '#80868b',
        },
        // Google blue: the light tint carries the dark theme, where the
        // saturated one would not hold its contrast. Variables, since the
        // operator of a community installation may give it an accent of their
        // own: what they are is in index.css, and what theirs come to is
        // worked out by the server (Services/Branding/BrandPalette.cs).
        // fill, hover and ink are a button filled with the accent, under the
        // pointer, and what is written on it.
        accent: {
          400: shade('--accent-400'),
          500: shade('--accent-500'),
          600: shade('--accent-600'),
          700: shade('--accent-700'),
          fill: shade('--accent-fill'),
          hover: shade('--accent-hover'),
          ink: shade('--accent-ink'),
        },
        // the purple of the logo, for what is special rather than what is
        // interactive - the production tier - or the operator's second accent.
        // The dark shade carries text on the light theme, the light one on
        // the dark theme; ink is what is written on 500.
        logo: {
          400: shade('--second-400'),
          500: shade('--second-500'),
          700: shade('--second-700'),
          ink: shade('--second-ink'),
        },
        // Google cyan, for the enterprise tier: an installation of its own
        // rather than a place on the shared one, so a colour of its own too
        enterprise: {
          400: '#78d9ec',
          500: '#12b5cb',
          700: '#007b83',
        },
        // the greys the light theme is drawn with
        slate: grey,
        // the remaining three brand colours, standing in for the status palette
        emerald: {
          50: '#e6f4ea',
          400: '#81c995',
          500: '#1e8e3e',
          600: '#188038',
          950: '#0d2b16',
        },
        red: {
          50: '#fce8e6',
          300: '#f6aea9',
          400: '#f28b82',
          500: '#d93025',
          600: '#c5221f',
          900: '#a50e0e',
          950: '#3c1614',
        },
        amber: {
          50: '#fef7e0',
          400: '#fdd663',
          500: '#f9ab00',
          600: '#e37400',
        },
        // Google pink, for what is neither the program nor written about it -
        // what the program is built from - so it is told apart from the four
        // kinds the brand colours already mark
        pink: {
          400: '#ff8bcb',
          500: '#e52592',
        },
      },
      fontFamily: {
        /*
         * The stack the reference site uses: whatever the system draws its own
         * interface in, with Inter ahead of it for anyone who has it
         * (--font-system, in index.css, where Japanese and Korean name their
         * own). Nothing is fetched - a face downloaded before the first word
         * can be read is a poor trade for a page whose whole claim is speed -
         * unless the operator gave the installation a font: the page then
         * comes with it under the first name, which is nobody's otherwise.
         */
        sans: ['"Lambda Brand Text"', 'var(--font-system)'],
        mono: [
          'Geist Mono', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco',
          'Consolas', 'Liberation Mono', 'Courier New', 'monospace',
        ],
      },
    },
  },
  plugins: [],
};
