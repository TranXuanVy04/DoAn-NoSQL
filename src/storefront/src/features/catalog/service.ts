import { request, type ProductPage } from '../../shared/api/client';
import { mockProducts } from '../../mocks/catalog';
export async function getProducts(): Promise<ProductPage> {
  return process.env.NEXT_PUBLIC_USE_MOCK_API === 'true' ? mockProducts() : request<ProductPage>('/api/products');
}
