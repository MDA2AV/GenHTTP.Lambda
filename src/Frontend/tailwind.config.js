/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        ink: {
          950: '#0b0e14',
          900: '#10141c',
          850: '#151a24',
          800: '#1b2230',
          700: '#273040',
          600: '#3a4658',
        },
        accent: {
          400: '#6ea8fe',
          500: '#4b8bf5',
          600: '#3570dd',
        },
      },
      fontFamily: {
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'Consolas', 'monospace'],
      },
    },
  },
  plugins: [],
};
