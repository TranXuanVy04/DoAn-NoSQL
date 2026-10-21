import { defineConfig } from 'eslint/config';
import nextVitals from 'eslint-config-next/core-web-vitals';
export default defineConfig([...nextVitals, { ignores: ['.next/**', 'src/shared/api/generated.d.ts'] }]);
