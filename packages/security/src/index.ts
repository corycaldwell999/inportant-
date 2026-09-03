/**
 * Trauma-Informed Platform - Client-Side Security Utilities
 * Helpers for common security patterns and best practices
 */

// ============================================================================
// Token Management
// ============================================================================

export interface TokenStorage {
  getToken(): string | null;
  setToken(token: string, expiresIn?: number): void;
  removeToken(): void;
  isExpired(): boolean;
}

/**
 * Secure HTTP-only cookie storage (recommended for production)
 * In development, use localStorage with HTTPS only
 */
export const createSecureTokenStorage = (): TokenStorage => {
  const storageKey = 'tip_auth_token';
  const expiryKey = 'tip_auth_expiry';

  return {
    getToken(): string | null {
      if (typeof window === 'undefined') return null;
      return localStorage.getItem(storageKey);
    },
    setToken(token: string, expiresIn?: number): void {
      if (typeof window === 'undefined') return;
      localStorage.setItem(storageKey, token);
      if (expiresIn) {
        const expiry = Date.now() + expiresIn * 1000;
        localStorage.setItem(expiryKey, expiry.toString());
      }
    },
    removeToken(): void {
      if (typeof window === 'undefined') return;
      localStorage.removeItem(storageKey);
      localStorage.removeItem(expiryKey);
    },
    isExpired(): boolean {
      if (typeof window === 'undefined') return true;
      const expiry = localStorage.getItem(expiryKey);
      if (!expiry) return false;
      return Date.now() > parseInt(expiry);
    },
  };
};

// ============================================================================
// Input Sanitization
// ============================================================================

/**
 * Basic HTML sanitization to prevent XSS
 * Use in addition to Content Security Policy headers
 */
export function sanitizeHtml(html: string): string {
  const div = document.createElement('div');
  div.textContent = html;
  return div.innerHTML;
}

/**
 * Sanitize URLs to prevent javascript: protocol attacks
 */
export function sanitizeUrl(url: string): string {
  const lowerUrl = url.toLowerCase().trim();
  if (lowerUrl.startsWith('javascript:') || lowerUrl.startsWith('data:')) {
    return '';
  }
  return url;
}

// ============================================================================
// CSRF Protection
// ============================================================================

/**
 * Generate a CSRF token
 */
export function generateCsrfToken(): string {
  const array = new Uint8Array(32);
  crypto.getRandomValues(array);
  return Array.from(array, (byte) => byte.toString(16).padStart(2, '0')).join('');
}

// ============================================================================
// Password Validation
// ============================================================================

export interface PasswordStrength {
  score: number; // 0-4
  feedback: string[];
  isValid: boolean;
}

/**
 * Evaluate password strength
 * Requires: 10+ chars, uppercase, lowercase, number
 */
export function evaluatePasswordStrength(password: string): PasswordStrength {
  const feedback: string[] = [];
  let score = 0;

  if (password.length >= 10) score++;
  else feedback.push('At least 10 characters required');

  if (password.length >= 16) score++;
  else if (password.length >= 12) score++;

  if (/[A-Z]/.test(password)) score++;
  else feedback.push('Include at least one uppercase letter');

  if (/[a-z]/.test(password)) score++;
  else feedback.push('Include at least one lowercase letter');

  if (/[0-9]/.test(password)) score++;
  else feedback.push('Include at least one number');

  if (/[^A-Za-z0-9]/.test(password)) score++;
  else feedback.push('Consider adding a special character');

  return {
    score: Math.min(4, score),
    feedback,
    isValid:
      password.length >= 10 &&
      /[A-Z]/.test(password) &&
      /[a-z]/.test(password) &&
      /[0-9]/.test(password),
  };
}

// ============================================================================
// Rate Limiting
// ============================================================================

export interface RateLimitConfig {
  maxAttempts: number;
  windowMs: number;
}

/**
 * Client-side rate limiting for forms and API calls
 */
export class ClientRateLimiter {
  private attempts: Map<string, number[]> = new Map();
  private maxAttempts: number;
  private windowMs: number;

  constructor(config: RateLimitConfig) {
    this.maxAttempts = config.maxAttempts;
    this.windowMs = config.windowMs;
  }

  isAllowed(key: string): boolean {
    const now = Date.now();
    const attempts = this.attempts.get(key) || [];

    // Remove old attempts outside the window
    const recent = attempts.filter((time) => now - time < this.windowMs);

    if (recent.length >= this.maxAttempts) {
      return false;
    }

    recent.push(now);
    this.attempts.set(key, recent);
    return true;
  }

  getRemainingTime(key: string): number {
    const attempts = this.attempts.get(key) || [];
    if (attempts.length === 0) return 0;
    const oldest = Math.min(...attempts);
    const remaining = oldest + this.windowMs - Date.now();
    return Math.max(0, remaining);
  }
}

// ============================================================================
// Secure Random Values
// ============================================================================

/**
 * Generate a cryptographically secure random string
 */
export function generateSecureRandom(length: number = 32): string {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  const array = new Uint8Array(length);
  crypto.getRandomValues(array);
  return Array.from(array, (byte) => chars[byte % chars.length]).join('');
}
