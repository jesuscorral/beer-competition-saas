import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export function Home() {
  const { t } = useTranslation();
  const { isAuthenticated, isOrganizer } = useAuth();
  
  return (
    <div className="max-w-4xl mx-auto">
      <div className="text-center py-12">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          {t('home.welcome')}
        </h1>
        <p className="text-xl text-gray-600 mb-8">
          {t('home.subtitle')}
        </p>
        
        <div className="grid md:grid-cols-2 gap-6 mt-12">
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h2 className="text-2xl font-semibold mb-3">{t('home.organizeCompetitions.title')}</h2>
            <p className="text-gray-600 mb-4">
              {t('home.organizeCompetitions.description')}
            </p>
            <div className="flex gap-3">
              <Link
                to="/competitions"
                className="inline-block px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                {t('home.organizeCompetitions.viewAction')}
              </Link>
              {isAuthenticated && isOrganizer && (
                <Link
                  to="/competitions/create"
                  className="inline-block px-6 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700 transition-colors"
                >
                  {t('home.organizeCompetitions.action')}
                </Link>
              )}
            </div>
          </div>
          
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h2 className="text-2xl font-semibold mb-3">{t('home.judgeOffline.title')}</h2>
            <p className="text-gray-600 mb-4">
              {t('home.judgeOffline.description')}
            </p>
            <button
              disabled
              className="inline-block px-6 py-2 bg-gray-400 text-white rounded-lg cursor-not-allowed"
            >
              {t('home.judgeOffline.action')}
            </button>
          </div>
        </div>

        <div className="mt-12 p-6 bg-blue-50 rounded-lg">
          <h3 className="text-lg font-semibold text-blue-900 mb-2">
            {t('home.mvpPhase.title')}
          </h3>
          <p className="text-blue-800">
            {t('home.mvpPhase.description')}
          </p>
        </div>
      </div>
    </div>
  );
}
