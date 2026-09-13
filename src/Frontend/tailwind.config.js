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
    // nothing on a Google surface has a rounded corner, and the scale is
    // flattened rather than removed so existing utilities keep resolving
    borderRadius: {
      none: '0',
      sm: '0',
      DEFAULT: '0',
      md: '0',
      lg: '0',
      xl: '0',
      '2xl': '0',
      '3xl': '0',
      full: '0',
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
        // Google sets its interface in Roboto, falling back to whatever the
        // system uses for the same job
        sans: ['Roboto', 'Arial', 'ui-sans-serif', 'system-ui', '-apple-system', 'Segoe UI', 'sans-serif'],
        mono: ['Roboto Mono', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'Consolas', 'monospace'],
      },
    },
  },
  plugins: [],
};
