import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useLocation } from 'react-router-dom';
import { CompetitionForm } from '../components/CompetitionForm';

export function CompetitionCreate() {
  const { t } = useTranslation();
  const location = useLocation();
  
  // Check if tenantId was passed from registration
  useEffect(() => {
    const state = location.state as { tenantId?: string; fromRegistration?: boolean } | null;
    if (state?.fromRegistration && state?.tenantId) {
      console.log('Creating competition after registration for tenant:', state.tenantId);
    }
  }, [location.state]);
  
  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">{t('competition.create.title')}</h1>
        <p className="text-gray-600 mt-2">
          {t('competition.create.subtitle')}
        </p>
      </div>
      
      <CompetitionForm />
    </div>
  );
}
