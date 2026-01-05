import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { LanguageSelector } from './LanguageSelector';

export function Navbar() {
  const { t } = useTranslation();
  
  return (
    <nav className="bg-white shadow-sm border-b border-gray-200">
      <div className="container mx-auto px-4">
        <div className="flex justify-between items-center h-16">
          <Link to="/" className="flex items-center space-x-2">
            <span className="text-2xl">🍺</span>
            <span className="text-xl font-bold text-gray-800">
              {t('navbar.title')}
            </span>
          </Link>
          
          <div className="flex items-center space-x-4">
            <LanguageSelector />
            <Link
              to="/register"
              className="px-4 py-2 text-gray-700 hover:text-amber-600 transition-colors"
            >
              {t('navbar.register')}
            </Link>
            <Link
              to="/competitions/create"
              className="px-4 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700 transition-colors"
            >
              {t('navbar.createCompetition')}
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
}
