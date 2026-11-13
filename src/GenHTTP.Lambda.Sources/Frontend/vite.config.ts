import { defineConfig } from 'vite';

import { bundle } from '../../Kit/vite';

// served by the component below /source/, from the folder it is built into
export default defineConfig(bundle({ root: import.meta.dirname, base: '/source/', outDir: '../bundle', social: true }));
