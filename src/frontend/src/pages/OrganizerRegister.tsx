import { useTranslation } from 'react-i18next';
import { OrganizerRegistrationForm } from '../components/OrganizerRegistrationForm';

export function OrganizerRegister() {
  const { t } = useTranslation();

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          {t('auth.register.title')}
        </h1>
        <p className="text-gray-600 mt-2">
          {t('auth.register.subtitle')}
        </p>
        
        <div className="mt-4 p-4 bg-amber-50 border border-amber-200 rounded-lg">
          <h2 className="text-sm font-semibold text-amber-900 mb-2">
            {t('auth.register.twoStepProcess')}
          </h2>
          <ol className="list-decimal list-inside space-y-1 text-sm text-amber-800">
            <li>{t('auth.register.step1')}</li>
            <li>{t('auth.register.step2')}</li>
          </ol>
        </div>
      </div>

      <OrganizerRegistrationForm />
    </div>
  );
}
