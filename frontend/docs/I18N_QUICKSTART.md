# i18n Quick Start Guide

Quick reference for using internationalization in the Beer Competition SaaS Platform frontend.

## For Developers

### 1. Use Translation in Component

```tsx
import { useTranslation } from 'react-i18next';

export function MyComponent() {
  const { t } = useTranslation();
  
  return (
    <div>
      <h1>{t('myFeature.title')}</h1>
      <p>{t('myFeature.description')}</p>
    </div>
  );
}
```

### 2. Add Translation Key

**Add to BOTH language files:**

`src/locales/en/translation.json`:
```json
{
  "myFeature": {
    "title": "My Feature",
    "description": "This is my feature description"
  }
}
```

`src/locales/es/translation.json`:
```json
{
  "myFeature": {
    "title": "Mi Característica",
    "description": "Esta es la descripción de mi característica"
  }
}
```

### 3. Translation with Variables

**JSON:**
```json
{
  "welcome": "Welcome, {{name}}!",
  "itemCount": "You have {{count}} items"
}
```

**Component:**
```tsx
{t('welcome', { name: 'John' })}
// Output: "Welcome, John!"

{t('itemCount', { count: 5 })}
// Output: "You have 5 items"
```

### 4. Change Language Programmatically

```tsx
const { i18n } = useTranslation();

// Change to Spanish
i18n.changeLanguage('es');

// Change to English
i18n.changeLanguage('en');

// Get current language
console.log(i18n.language); // 'en' or 'es'
```

## Common Patterns

### Form Labels
```tsx
<label htmlFor="email">
  {t('form.email.label')}
</label>
<input 
  id="email" 
  placeholder={t('form.email.placeholder')} 
/>
```

### Button Text
```tsx
<button>
  {isLoading ? t('common.loading') : t('form.submit')}
</button>
```

### Error Messages
```tsx
{error && (
  <p className="error">{t('form.error.required')}</p>
)}
```

### Toast Notifications
```tsx
toast.success(t('competition.create.success', { id: competitionId }));
toast.error(t('competition.create.error'));
```

## Translation Key Conventions

### Naming Pattern
```
{feature}.{subfeature}.{element}.{property}
```

### Examples
```
home.welcome                          // Simple text
navbar.createCompetition             // Action button
competition.create.title             // Page title
competition.create.name.label        // Form field label
competition.create.name.placeholder  // Form field placeholder
competition.create.success           // Success message
```

### Organization
- **common**: Shared across all pages (loading, error, success)
- **navbar**: Navigation bar elements
- **{feature}**: Feature-specific (home, competition, judging, etc.)
  - **{action}**: Action-specific (create, edit, list, etc.)
    - **{field}**: Field-specific (name, description, etc.)

## Quick Checklist

When adding new translated text:

- [ ] Add key to `locales/en/translation.json`
- [ ] Add key to `locales/es/translation.json` (same path)
- [ ] Use `useTranslation()` hook in component
- [ ] Replace hardcoded text with `t('key.path')`
- [ ] Test in both languages
- [ ] Check browser console for missing keys

## Testing

### Manual Test
1. Start dev server: `npm run dev`
2. Open http://localhost:5173
3. Click language selector (EN/ES)
4. Verify all text changes
5. Refresh page → Language should persist
6. Check browser console → No missing key warnings

### Check Current Language
```tsx
// In component
const { i18n } = useTranslation();
console.log('Current language:', i18n.language);

// In browser console
localStorage.getItem('i18nextLng')
```

## Troubleshooting

### "Key not found" warning
→ Add missing key to **both** en and es translation files

### Language not switching
→ Check if `LanguageSelector` component is rendered
→ Clear localStorage: `localStorage.removeItem('i18nextLng')`

### TypeScript error on translation key
→ Restart TypeScript server (VSCode: Ctrl+Shift+P → "Restart TS Server")

### Translation not updating
→ Restart dev server: `npm run dev`

## File Locations

- **Config**: `src/i18n.ts`
- **Types**: `src/i18next.d.ts`
- **English**: `src/locales/en/translation.json`
- **Spanish**: `src/locales/es/translation.json`
- **Selector**: `src/components/LanguageSelector.tsx`
- **Docs**: `docs/I18N.md`

## Resources

- Full Guide: [docs/I18N.md](I18N.md)
- Architecture: [docs/I18N_ARCHITECTURE.md](I18N_ARCHITECTURE.md)
- Implementation: [docs/I18N_IMPLEMENTATION.md](I18N_IMPLEMENTATION.md)
- Examples: [src/examples/i18n-examples.tsx](../examples/i18n-examples.tsx)

---

**Need Help?** Check [docs/I18N.md](I18N.md) for detailed documentation.
