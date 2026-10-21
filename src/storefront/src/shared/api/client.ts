import type { components } from './generated';
export type Product = components['schemas']['Product'];
export type ProductPage = components['schemas']['ProductPage'];
export async function request<T>(path: string): Promise<T> {
  const base = process.env.NEXT_PUBLIC_API_BASE_URL;
  if (!base) throw new Error('Thiếu NEXT_PUBLIC_API_BASE_URL');
  const response = await fetch(`${base}${path}`, { credentials: 'include', cache: 'no-store' });
  if (!response.ok) throw new Error(`API trả HTTP ${response.status}`);
  return response.json() as Promise<T>;
}
