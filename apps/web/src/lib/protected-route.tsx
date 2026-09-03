import { ReactNode } from 'react';
import { redirect } from 'next/navigation';
import { useAuth } from '@/lib/auth-context';

/**
 * Protected Route Wrapper Component
 * Redirects unauthenticated users to login page
 * Use this to wrap pages that require authentication
 */
export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  // Show loading state while checking auth
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-b from-indigo-50 to-white">
        <div className="text-center">
          <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-indigo-100 mb-4">
            <div className="w-6 h-6 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
          </div>
          <p className="text-gray-600 font-medium">Loading...</p>
        </div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    redirect('/login');
  }

  return children;
}

/**
 * Hook to check if a page should be protected
 * Returns redirect if not authenticated
 */
export function useProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();

  if (!isLoading && !isAuthenticated) {
    redirect('/login');
  }

  return { isAuthenticated, isLoading };
}
