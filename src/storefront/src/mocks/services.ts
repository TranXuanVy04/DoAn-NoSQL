import type { components } from '../shared/api/generated';
import categories from '../../../../contracts/examples/categories.json';
import order from '../../../../contracts/examples/order.json';
import summary from '../../../../contracts/examples/summary.json';
import revenue from '../../../../contracts/examples/revenue.json';
import topProducts from '../../../../contracts/examples/top-products.json';
export type MockScenario = 'success' | 'empty' | 'validation' | 'server-error' | 'network-error';
async function respond<T>(value: T, scenario: MockScenario): Promise<T> {
  if(scenario === 'network-error') throw new TypeError('Failed to fetch (simulation)');
  if(scenario === 'validation') throw new Error('HTTP 400: quantity phải từ 1 đến 99');
  if(scenario === 'server-error') throw new Error('HTTP 500: Lỗi hệ thống');
  return structuredClone(value);
}
export const mockCategories = (scenario: MockScenario = 'success') => respond<components['schemas']['Category'][]>(scenario === 'empty' ? [] : categories as components['schemas']['Category'][], scenario);
// Order mock is a canned fixture; it does not create an order or prove stock safety.
export const mockOrder = (scenario: MockScenario = 'success') => respond<components['schemas']['Order']>(order as components['schemas']['Order'], scenario);
export const mockAnalytics = (scenario: MockScenario = 'success') => respond({summary, revenue: scenario === 'empty' ? [] : revenue, topProducts: scenario === 'empty' ? [] : topProducts}, scenario);
