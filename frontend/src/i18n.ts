import i18n from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { initReactI18next } from 'react-i18next';

import enTranslation from './locales/en/translation.json';
import esTranslation from './locales/es/translation.json';

export const resources = {
  en: {
    translation: enTranslation,
  },
  es: {
    translation: esTranslation,
  },
} as const;

i18n
  .use(LanguageDetector) // Detect user language
  .use(initReactI18next) // Pass i18n instance to react-i18next
  .init({
    resources,
    fallbackLng: 'en', // Default language if detection fails
    debug: import.meta.env.DEV, // Enable debug in development
    
    interpolation: {
      escapeValue: false, // React already escapes values
    },

    detection: {
      // Order and from where user language should be detected
      order: ['localStorage', 'navigator', 'htmlTag'],
      
      // Cache user language in localStorage
      caches: ['localStorage'],
      
      // Keys to lookup language from
      lookupLocalStorage: 'i18nextLng',
    },
  });

export default i18n;
