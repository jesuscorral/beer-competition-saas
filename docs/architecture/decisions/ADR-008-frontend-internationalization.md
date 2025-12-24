# ADR-008: Frontend Internationalization (i18n)

**Date**: 2024-12-24  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform - Issue #80

---

## Context and Problem Statement

The Beer Competition Platform needs to support **multiple languages** to accommodate international homebrew competitions and users from different regions. The initial implementation focuses on **English** and **Spanish**, but the architecture must be extensible to add more languages easily.

**Requirements:**
- Support multiple languages (English, Spanish, future: French, German, Portuguese)
- Automatic language detection based on browser settings
- Language persistence across sessions (localStorage)
- Translation of all UI text: labels, buttons, validation messages, error messages
- Type-safe translation keys to prevent runtime errors
- Easy for developers to add new translations without modifying components
- Minimal performance impact (no large bundles)

**Challenges:**
- How to manage translations without coupling them to components?
- How to handle dynamic validation messages from Zod schemas?
- How to provide type safety for translation keys?
- How to detect and persist user language preference?

---

## Decision Drivers

- **Developer Experience**: Easy to add new translations
- **Type Safety**: Prevent typos in translation keys
- **Performance**: Minimal bundle size overhead
- **User Experience**: Automatic language detection, seamless switching
- **Maintainability**: Centralized translation files
- **Extensibility**: Easy to add new languages

---

## Considered Options

### 1. react-i18next + i18next (Chosen ✅)
**Approach**: Industry-standard i18n library with React bindings

**Pros:**
- ✅ **Mature ecosystem**: Most popular React i18n solution
- ✅ **Automatic detection**: Browser language + localStorage persistence
- ✅ **Type safety**: Full TypeScript support via declaration merging
- ✅ **React hooks**: `useTranslation()` hook for functional components
- ✅ **Interpolation**: Dynamic values in translations (`{{count}}`, `{{id}}`)
- ✅ **Lazy loading**: Load translations on demand (future optimization)
- ✅ **Pluralization**: Built-in support for plural rules
- ✅ **Small bundle**: ~5KB gzipped (react-i18next + i18next)

**Cons:**
- ❌ **Initial setup**: Requires configuration files
- ❌ **Learning curve**: Developers need to learn i18next API

---

### 2. react-intl (FormatJS)
**Approach**: ICU Message Format-based internationalization

**Pros:**
- ✅ **ICU standard**: Industry-standard message format
- ✅ **Rich formatting**: Date, time, number formatting built-in
- ✅ **React bindings**: Good React integration

**Cons:**
- ❌ **Bundle size**: Larger than i18next (~10KB gzipped)
- ❌ **Verbosity**: More boilerplate code
- ❌ **Less popular**: Smaller community than i18next

---

### 3. Custom i18n Solution
**Approach**: Build our own translation system

**Pros:**
- ✅ **Minimal**: Only what we need
- ✅ **Control**: Full control over implementation

**Cons:**
- ❌ **Reinventing wheel**: i18next already solves this
- ❌ **Maintenance burden**: Need to maintain ourselves
- ❌ **Missing features**: Would need to implement detection, pluralization, etc.

---

## Decision Outcome

**Chosen option**: **react-i18next + i18next**

### Architecture Overview

```
frontend/
├── src/
│   ├── i18n.ts                        # i18n configuration (LanguageDetector + resources)
│   ├── i18next.d.ts                   # TypeScript type definitions for autocomplete
│   ├── locales/
│   │   ├── en/
│   │   │   └── translation.json       # English translations
│   │   └── es/
│   │       └── translation.json       # Spanish translations
│   ├── components/
│   │   ├── LanguageSelector.tsx       # Language switcher (🇬🇧 EN / 🇪🇸 ES)
│   │   └── CompetitionForm.tsx        # Uses useTranslation() hook
│   └── schemas/
│       └── competition.ts             # Zod schema with i18n validation messages
```

### Configuration

**`i18n.ts`:**
- Loads translation resources from `locales/en/` and `locales/es/`
- Uses `i18next-browser-languagedetector` for automatic language detection
- Detection order: `localStorage` → `navigator` (browser language) → `htmlTag`
- Caches selected language in `localStorage` key: `i18nextLng`
- Fallback language: `en` (if detection fails)

**`i18next.d.ts`:**
- TypeScript declaration merging for type-safe translation keys
- Enables autocomplete in IDEs: `t('competition.create.title')` → suggests all keys
- Prevents typos at compile-time

**Translation Files (JSON):**
- Hierarchical structure: `common`, `navbar`, `home`, `competition`, `validation`
- Interpolation support: `"success": "Competition created! ID: {{id}}"`
- Consistent naming: snake_case for keys, nested objects for organization

### Usage Patterns

**1. Basic Translation (Component):**
```typescript
import { useTranslation } from 'react-i18next';

export function Navbar() {
  const { t } = useTranslation();
  return <h1>{t('navbar.title')}</h1>;
}
```

**2. Translation with Interpolation:**
```typescript
const { t } = useTranslation();
toast.success(t('competition.create.success', { id: competitionId }));
// Output: "Competition created successfully! ID: abc-123"
```

**3. Language Switching:**
```typescript
import { useTranslation } from 'react-i18next';

export function LanguageSelector() {
  const { i18n } = useTranslation();
  
  return (
    <button onClick={() => i18n.changeLanguage('es')}>
      🇪🇸 ES
    </button>
  );
}
```

**4. Validation Messages (Zod + i18n):**
```typescript
import i18n from '../i18n';
import { z } from 'zod';

export const createCompetitionSchema = () => z.object({
  name: z.string()
    .min(1, i18n.t('competition.validation.nameRequired'))
    .max(255, i18n.t('competition.validation.nameMaxLength')),
});
```
**Note:** Schema must be a **factory function** (`createCompetitionSchema()`) to get fresh translations when language changes.

---

## Consequences

### Positive
- ✅ **Type safety**: Autocomplete and compile-time checks for translation keys
- ✅ **User experience**: Automatic language detection, persistent preference
- ✅ **Maintainability**: All text externalized to JSON files (no hardcoded strings)
- ✅ **Extensibility**: Adding new languages requires only new JSON file
- ✅ **Developer experience**: `useTranslation()` hook is simple and intuitive
- ✅ **Validation messages**: All Zod error messages translated automatically

### Negative
- ❌ **Initial setup overhead**: Required configuration files and type definitions
- ❌ **Schema factory pattern**: Zod schemas must be functions (not constants) for dynamic translations
- ❌ **Translation coverage**: Need to ensure all UI text is translated (manual QA)

### Neutral
- ⚖️ **Bundle size**: +5KB gzipped (acceptable for i18n functionality)
- ⚖️ **Learning curve**: Developers need to learn `useTranslation()` hook

---

## Implementation Notes

### Language Detection Priority
1. **localStorage** (`i18nextLng` key) - User's explicit choice
2. **navigator.language** - Browser default language
3. **Fallback**: English (`en`)

### Translation File Structure
```json
{
  "common": {
    "loading": "Loading...",
    "error": "Error"
  },
  "navbar": {
    "title": "Beer Competition Platform"
  },
  "competition": {
    "create": {
      "title": "Create New Competition",
      "success": "Competition created! ID: {{id}}"
    },
    "validation": {
      "nameRequired": "Competition name is required",
      "judgingDateAfterRegistration": "Judging date must be after registration deadline"
    }
  }
}
```

### Adding a New Language
1. Create `frontend/src/locales/{languageCode}/translation.json`
2. Copy structure from `en/translation.json`
3. Translate all values
4. Add to `i18n.ts` resources:
   ```typescript
   import frTranslation from './locales/fr/translation.json';
   
   export const resources = {
     en: { translation: enTranslation },
     es: { translation: esTranslation },
     fr: { translation: frTranslation }, // New
   } as const;
   ```
5. Add button to `LanguageSelector.tsx`:
   ```typescript
   { code: 'fr', label: 'Français', flag: '🇫🇷' }
   ```

---

## Related Decisions

- **ADR-007**: Frontend Architecture (React 18 + TypeScript choice)
- **Issue #80**: Bootstrap React Frontend Project

---

## References

- [react-i18next Documentation](https://react.i18next.com/)
- [i18next Documentation](https://www.i18next.com/)
- [i18next-browser-languagedetector](https://github.com/i18next/i18next-browser-languageDetector)
- [TypeScript Integration Guide](https://www.i18next.com/overview/typescript)
- [Frontend i18n Documentation](../../../frontend/docs/I18N.md)

---

## Status

**Accepted** - Implemented in Issue #80 (2024-12-24)

Languages supported:
- 🇬🇧 English (en) - Default
- 🇪🇸 Spanish (es)

Future languages:
- 🇫🇷 French (fr) - Planned
- 🇩🇪 German (de) - Planned
- 🇵🇹 Portuguese (pt) - Planned
