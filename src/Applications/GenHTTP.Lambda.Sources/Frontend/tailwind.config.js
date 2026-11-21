import preset from '../../Kit/tailwind.preset.js';

/** @type {import('tailwindcss').Config} */
export default {
  presets: [preset],
  // the kit's components are part of this bundle, and so are their classes
  content: ['./index.html', './src/**/*.{ts,tsx}', '../../Kit/src/**/*.{ts,tsx}'],
};
