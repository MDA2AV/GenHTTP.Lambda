/** @type {import('tailwindcss').Config} */

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

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
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
        // saturated one would not hold its contrast
        accent: {
          400: '#8ab4f8',
          500: '#1a73e8',
          600: '#1765cc',
          700: '#174ea6',
        },
        // the purple of the logo, for what is special rather than what is
        // interactive - the premium tier. The dark shade carries text on the
        // light theme, the light one on the dark theme.
        logo: {
          400: '#c99bff',
          500: '#aa55ff',
          700: '#7b2fd1',
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
      },
      fontFamily: {
        /*
         * The stack the reference site uses: whatever the system draws its own
         * interface in, with Inter ahead of it for anyone who has it. Nothing
         * is fetched - a face downloaded before the first word can be read is
         * a poor trade for a page whose whole claim is speed.
         */
        sans: [
          '-apple-system', 'BlinkMacSystemFont', 'Inter', 'system-ui', 'Segoe UI',
          'Roboto', 'Oxygen', 'Ubuntu', 'Cantarell', 'Open Sans', 'Helvetica Neue', 'sans-serif',
        ],
        mono: [
          'Geist Mono', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco',
          'Consolas', 'Liberation Mono', 'Courier New', 'monospace',
        ],
      },
    },
  },
  plugins: [],
};
