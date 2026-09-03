import type { Metadata } from 'next';
import React from 'react';
import './globals.css';
import { RootLayoutClient } from './layout-client';

export const metadata: Metadata = {
  title: 'Trauma-Informed Community Platform',
  description: 'A calm, privacy-first wellness and community platform focused on mental health education, peer support, and recovery resources.',
  viewport: 'width=device-width, initial-scale=1, maximum-scale=5',
  icons: {
    icon: '/favicon.ico',
  },
  openGraph: {
    title: 'Trauma-Informed Community Platform',
    description: 'Free mental health support platform',
    type: 'website',
  },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <meta httpEquiv="X-UA-Compatible" content="ie=edge" />
      </head>
      <body className="antialiased bg-gray-50">
        <RootLayoutClient>{children}</RootLayoutClient>
      </body>
    </html>
  );
}
