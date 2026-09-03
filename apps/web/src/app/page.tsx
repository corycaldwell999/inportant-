'use client';

import { useEffect } from 'react';
import { Button } from '@shared/ui/src/index';
import { useAuth } from '@/lib/auth-context';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function HomePage() {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth();

  // Redirect to dashboard if already authenticated
  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      router.push('/dashboard' as any);
    }
  }, [isAuthenticated, isLoading, router]);

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

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gradient-to-b from-indigo-50 to-white px-4 py-12">
      <div className="max-w-4xl mx-auto text-center">
        {/* Header */}
        <div className="mb-12">
          <h1 className="text-5xl md:text-6xl font-bold text-gray-900 mb-6">
            Trauma-Informed Community
          </h1>
          <p className="text-xl md:text-2xl text-gray-600 mb-8">
            A safe, private space for mental health education, wellness support, and peer connection
          </p>
        </div>

        {/* Key Features */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
          <div className="p-6 bg-white rounded-lg shadow-md border border-gray-200">
            <div className="text-4xl mb-4">🛡️</div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Privacy First</h3>
            <p className="text-gray-600 text-sm">Your data is yours. We never sell, share, or monetize your information.</p>
          </div>

          <div className="p-6 bg-white rounded-lg shadow-md border border-gray-200">
            <div className="text-4xl mb-4">💚</div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Trauma-Informed</h3>
            <p className="text-gray-600 text-sm">Designed with care to create psychological safety and support healing.</p>
          </div>

          <div className="p-6 bg-white rounded-lg shadow-md border border-gray-200">
            <div className="text-4xl mb-4">🤝</div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Community</h3>
            <p className="text-gray-600 text-sm">Connect with others, share experiences, and find support together.</p>
          </div>
        </div>

        {/* Main CTA */}
        <div className="bg-white border-2 border-indigo-200 rounded-lg p-8 mb-12">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">Get Started</h2>
          <p className="text-gray-600 mb-6">
            Join a supportive community focused on mental health, wellness, and recovery.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href={"/register" as any}>
              <Button variant="primary" size="lg">
                Create Account
              </Button>
            </Link>
            <Link href={"/login" as any}>
              <Button variant="secondary" size="lg">
                Sign In
              </Button>
            </Link>
          </div>
        </div>

        {/* Important Notice */}
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-6 max-w-2xl mx-auto mb-12">
          <p className="text-sm text-amber-800">
            <strong>Important:</strong> This platform is not an emergency service and does not replace
            professional mental health treatment. If you're in crisis, please contact a crisis hotline or
            emergency services.
          </p>
        </div>

        {/* Platform Goals */}
        <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-8 mb-12">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">Our Commitment</h2>
          <ul className="text-left max-w-2xl mx-auto space-y-3 text-gray-700">
            <li className="flex items-start gap-3">
              <span className="text-indigo-600 font-bold">✓</span>
              <span>Private, consent-based participation with full data control</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="text-indigo-600 font-bold">✓</span>
              <span>Free access to support and learning without pressure or monetization</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="text-indigo-600 font-bold">✓</span>
              <span>Accessible, trauma-informed design that prioritizes psychological safety</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="text-indigo-600 font-bold">✓</span>
              <span>Compassionate moderation and human-centered safety practices</span>
            </li>
          </ul>
        </div>
      </div>

      {/* Footer */}
      <footer className="mt-20 pt-8 border-t border-gray-200 w-full text-center text-sm text-gray-600">
        <p className="mb-4">
          <a href="#about" className="hover:text-indigo-600 mr-6">
            About
          </a>
          <a href="#privacy" className="hover:text-indigo-600 mr-6">
            Privacy
          </a>
          <a href="#accessibility" className="hover:text-indigo-600 mr-6">
            Accessibility
          </a>
          <a href="#contact" className="hover:text-indigo-600">
            Contact
          </a>
        </p>
        <p>© 2026 Trauma-Informed Community Platform. Free and open to all.</p>
      </footer>
    </div>
  );
}

