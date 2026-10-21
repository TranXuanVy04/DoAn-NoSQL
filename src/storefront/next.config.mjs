import path from 'node:path';
import { fileURLToPath } from 'node:url';
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const config = { turbopack: { root: repoRoot }, outputFileTracingRoot: repoRoot };
export default config;
