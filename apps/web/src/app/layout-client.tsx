'use client';

import { AuthProvider } from '@/lib/auth-context';
import { ReactNode } from 'react';

export function RootLayoutClient({ children }: { children: ReactNode }) {
  return (
    <AuthProvider>
      <a href="#main-content" className="sr-only">
        Skip to main content
      </a>
      <main id="main-content">{children}</main>
    </AuthProvider>
  );
}
