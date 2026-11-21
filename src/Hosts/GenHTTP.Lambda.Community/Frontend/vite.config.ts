import { defineConfig } from 'vite';

import { bundle } from '../../Kit/vite';

// served by the host at the root of the site, from the folder it is built into
export default defineConfig(bundle({ root: import.meta.dirname, base: '/', outDir: '../bundle', social: true }));
