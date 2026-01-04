// Example of using translations in a component
// This file demonstrates best practices for i18n usage

import { useTranslation } from 'react-i18next';

// Example 1: Basic text translation
export function WelcomeMessage() {
  const { t } = useTranslation();
  
  return (
    <div>
      <h1>{t('home.welcome')}</h1>
      <p>{t('home.subtitle')}</p>
    </div>
  );
}

// Example 2: Translation with interpolation
export function SuccessMessage({ competitionId }: { competitionId: string }) {
  const { t } = useTranslation();
  
  return (
    <div className="success">
      {t('competition.create.success', { id: competitionId })}
    </div>
  );
}

// Example 3: Changing language programmatically
export function LanguageChanger() {
  const { i18n } = useTranslation();
  
  const changeToSpanish = () => {
    i18n.changeLanguage('es');
  };
  
  const changeToEnglish = () => {
    i18n.changeLanguage('en');
  };
  
  return (
    <div>
      <button onClick={changeToEnglish}>English</button>
      <button onClick={changeToSpanish}>Español</button>
      <p>Current language: {i18n.language}</p>
    </div>
  );
}

// Example 4: Translation in form labels and placeholders
export function FormExample() {
  const { t } = useTranslation();
  
  return (
    <form>
      <label htmlFor="name">
        {t('competition.create.name.label')}
      </label>
      <input 
        id="name"
        type="text"
        placeholder={t('competition.create.name.placeholder')}
      />
    </form>
  );
}

// Example 5: Using translation with array maps (with type assertion for dynamic keys)
export function MenuItems() {
  const { t } = useTranslation();
  
  const menuKeys = ['home', 'competitions', 'judging', 'entries'];
  
  return (
    <nav>
      {menuKeys.map(key => (
        <a key={key} href={`/${key}`}>
          {/* Use type assertion for dynamic translation keys */}
          {t(`menu.${key}` as any)}
        </a>
      ))}
    </nav>
  );
}

// Example 6: Conditional text based on status (with type assertion)
export function StatusMessage({ status }: { status: 'pending' | 'approved' | 'rejected' }) {
  const { t } = useTranslation();
  
  return (
    <span className={`status-${status}`}>
      {/* Use type assertion for dynamic translation keys */}
      {t(`status.${status}` as any)}
    </span>
  );
}
