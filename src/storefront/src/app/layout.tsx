import Link from 'next/link';
import type { ReactNode } from 'react';
import './globals.css';
export const metadata = { title: 'EC Market', description: 'Sàn thương mại đa ngành hàng' };
export default function Layout({ children }: { children: ReactNode }) {
  return <html lang="vi"><body><header><Link href="/">EC Market</Link><span>Điện tử · Thời trang · Gia dụng</span></header><main>{children}</main></body></html>;
}
