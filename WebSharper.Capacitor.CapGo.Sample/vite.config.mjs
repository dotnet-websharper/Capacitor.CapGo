import { defineConfig } from 'vite'
import path from "path";

export default defineConfig({
  root: path.resolve('wwwroot'),
  // Relative asset URLs: the site is served from /<repo>/ on GitHub Pages, so
  // the default '/' base would look for /assets/... at the domain root.
  base: './',
  build: {
    outDir: path.resolve('dist'),
    emptyOutDir: true
  }
})
