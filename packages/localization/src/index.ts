/**
 * Localization infrastructure for the trauma-informed platform.
 * This provides the basic structure for later language packs and translation resources.
 */

export type SupportedLocale = 'en' | 'es' | 'fr' | 'de';

export const supportedLocales: SupportedLocale[] = ['en', 'es', 'fr', 'de'];

export const defaultLocale: SupportedLocale = 'en';

export function getLocaleLabel(locale: SupportedLocale): string {
  const labels: Record<SupportedLocale, string> = {
    en: 'English',
    es: 'Español',
    fr: 'Français',
    de: 'Deutsch',
  };

  return labels[locale];
}

export function isSupportedLocale(locale: string): locale is SupportedLocale {
  return supportedLocales.includes(locale as SupportedLocale);
}
