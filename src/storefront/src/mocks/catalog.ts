import type { ProductPage } from '../shared/api/client';
import products from '../../../../contracts/examples/products.json';
export async function mockProducts(): Promise<ProductPage> { return structuredClone(products); }
