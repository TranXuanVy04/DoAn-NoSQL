import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
const spec = JSON.parse(await readFile(new URL('../../contracts/openapi/ecommerce.v1.json', import.meta.url)));
test('Checkout never accepts client prices', () => {
  assert.deepEqual(Object.keys(spec.components.schemas.OrderInput.properties.items.items.properties).sort(), ['productId','quantity']);
});
test('Analytics requires Admin authentication', () => {
  for (const [path, ops] of Object.entries(spec.paths)) if(path.startsWith('/api/analytics/')) assert.deepEqual(ops.get.security, [{adminCookie:[]}]);
});
