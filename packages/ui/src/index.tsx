/**
 * Trauma-Informed Platform - Shared Design System
 * Accessible, calm, and supportive UI components
 *
 * Design principles:
 * - Psychological safety and calm interactions
 * - WCAG 2.2 AA accessibility compliance
 * - Compassionate, non-shaming language
 * - Privacy-first and user-controlled
 * - Supportive visual language
 */

import React from 'react';

// ============================================================================
// Theme & Design Tokens
// ============================================================================

export const designTokens = {
  colors: {
    // Semantic colors - supportive and calm
    primary: '#6366f1', // Indigo - calming
    secondary: '#8b5cf6', // Purple - supportive
    success: '#10b981', // Emerald - gentle affirmation
    warning: '#f59e0b', // Amber - cautious
    danger: '#ef4444', // Red - only for urgent/destructive
    neutral: '#6b7280', // Gray
    background: '#ffffff',
    backgroundSecondary: '#f9fafb',
    text: '#1f2937',
    textSecondary: '#6b7280',
    border: '#e5e7eb',
    crisis: '#dc2626', // Red - only for crisis/emergency
  },
  spacing: {
    xs: '0.25rem',
    sm: '0.5rem',
    md: '1rem',
    lg: '1.5rem',
    xl: '2rem',
    '2xl': '3rem',
  },
  typography: {
    fontFamily: {
      sans: 'system-ui, -apple-system, sans-serif',
      mono: 'ui-monospace, monospace',
    },
    fontSize: {
      xs: '0.75rem',
      sm: '0.875rem',
      base: '1rem',
      lg: '1.125rem',
      xl: '1.25rem',
      '2xl': '1.5rem',
      '3xl': '1.875rem',
      '4xl': '2.25rem',
    },
    lineHeight: {
      tight: 1.25,
      normal: 1.5,
      relaxed: 1.75,
    },
  },
  borderRadius: {
    none: '0',
    sm: '0.125rem',
    md: '0.375rem',
    lg: '0.5rem',
    xl: '0.75rem',
    full: '9999px',
  },
  shadows: {
    sm: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
    md: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
    lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1)',
  },
};

// ============================================================================
// Accessible Button Component
// ============================================================================

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'tertiary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  isDisabled?: boolean;
}

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    { variant = 'primary', size = 'md', isLoading, isDisabled, children, ...props },
    ref,
  ) => {
    const baseClasses =
      'font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed';

    const variantClasses = {
      primary: 'bg-indigo-600 text-white hover:bg-indigo-700 focus:ring-indigo-500',
      secondary: 'bg-purple-600 text-white hover:bg-purple-700 focus:ring-purple-500',
      tertiary: 'bg-gray-200 text-gray-900 hover:bg-gray-300 focus:ring-gray-500',
      danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
    };

    const sizeClasses = {
      sm: 'px-3 py-1.5 text-sm',
      md: 'px-4 py-2 text-base',
      lg: 'px-6 py-3 text-lg',
    };

    return (
      <button
        ref={ref}
        disabled={isDisabled || isLoading}
        className={`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]}`}
        {...props}
      >
        {isLoading ? '...' : children}
      </button>
    );
  },
);

Button.displayName = 'Button';

// ============================================================================
// Accessible Form Input
// ============================================================================

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helpText?: string;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helpText, id, ...props }, ref) => {
    const inputId = id || `input-${Math.random()}`;

    return (
      <div className="w-full">
        {label && (
          <label htmlFor={inputId} className="block text-sm font-medium text-gray-900 mb-1">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={`
            w-full px-3 py-2 border rounded-lg
            focus:outline-none focus:ring-2 focus:ring-indigo-500
            ${error ? 'border-red-500 ring-red-500' : 'border-gray-300'}
            disabled:bg-gray-100 disabled:cursor-not-allowed
          `}
          aria-invalid={!!error}
          aria-describedby={error || helpText ? `${inputId}-message` : undefined}
          {...props}
        />
        {(error || helpText) && (
          <p
            id={`${inputId}-message`}
            className={`mt-1 text-sm ${error ? 'text-red-600' : 'text-gray-600'}`}
          >
            {error || helpText}
          </p>
        )}
      </div>
    );
  },
);

Input.displayName = 'Input';

// ============================================================================
// Accessible Card
// ============================================================================

export interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'elevated' | 'outlined';
}

export const Card = React.forwardRef<HTMLDivElement, CardProps>(
  ({ variant = 'default', className, ...props }, ref) => {
    const variantClasses = {
      default: 'bg-white border border-gray-200 rounded-lg',
      elevated: 'bg-white rounded-lg shadow-lg',
      outlined: 'bg-white border-2 border-indigo-200 rounded-lg',
    };

    return (
      <div ref={ref} className={`${variantClasses[variant]} ${className || ''}`} {...props} />
    );
  },
);

Card.displayName = 'Card';

// ============================================================================
// Supportive Reaction Selector
// ============================================================================

export type SupportiveReaction =
  | 'hear_you'
  | 'thinking_of_you'
  | 'thank_you'
  | 'not_alone'
  | 'sending_support'
  | 'helpful'
  | 'relate'
  | 'gentle_encouragement';

export const reactionEmojis: Record<SupportiveReaction, string> = {
  hear_you: '👂',
  thinking_of_you: '💭',
  thank_you: '🙏',
  not_alone: '🤝',
  sending_support: '💚',
  helpful: '✨',
  relate: '💙',
  gentle_encouragement: '🌱',
};

export const reactionLabels: Record<SupportiveReaction, string> = {
  hear_you: 'I hear you',
  thinking_of_you: 'Thinking of you',
  thank_you: 'Thank you for sharing',
  not_alone: "You're not alone",
  sending_support: 'Sending support',
  helpful: 'This was helpful',
  relate: 'I relate',
  gentle_encouragement: 'Gentle encouragement',
};

// ============================================================================
// Type Exports
// ============================================================================

export type ButtonVariant = 'primary' | 'secondary' | 'danger';

export function createButtonClassName(variant: ButtonVariant = 'primary') {
  const palette = {
    primary: 'background: #5b7c99; color: white;',
    secondary: 'background: #ece9e3; color: #1f2937;',
    danger: 'background: #a13b3b; color: white;',
  };

  return palette[variant];
}
