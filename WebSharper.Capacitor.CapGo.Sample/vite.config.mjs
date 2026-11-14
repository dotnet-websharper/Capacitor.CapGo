import { defineConfig } from 'vite'
import path from "path";

export default defineConfig({
  root: path.resolve('wwwroot'),
  build: {
    outDir: path.resolve('dist'),
    emptyOutDir: true
  }
})
