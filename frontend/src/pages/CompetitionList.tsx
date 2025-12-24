import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useCompetitions } from '../hooks/useCompetitions';

export function CompetitionList() {
  const { t } = useTranslation();
  const { data: competitions, isLoading, error } = useCompetitions();

  if (isLoading) {
    return (
      <div className="max-w-6xl mx-auto">
        <div className="flex items-center justify-center py-12">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">{t('common.loading')}</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-6xl mx-auto">
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 mt-6">
          <h3 className="text-lg font-semibold text-red-900 mb-2">
            {t('common.error')}
          </h3>
          <p className="text-red-700">
            {error instanceof Error ? error.message : t('competition.list.error')}
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">
          {t('competition.list.title')}
        </h1>
        <Link
          to="/competitions/create"
          className="px-4 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700 transition-colors"
        >
          {t('competition.list.createButton')}
        </Link>
      </div>

      {!competitions || competitions.length === 0 ? (
        <div className="bg-gray-50 border-2 border-dashed border-gray-300 rounded-lg p-12 text-center">
          <div className="text-6xl mb-4">🍺</div>
          <h3 className="text-xl font-semibold text-gray-700 mb-2">
            {t('competition.list.empty.title')}
          </h3>
          <p className="text-gray-600 mb-6">
            {t('competition.list.empty.description')}
          </p>
          <Link
            to="/competitions/create"
            className="inline-block px-6 py-3 bg-amber-600 text-white rounded-lg hover:bg-amber-700 transition-colors"
          >
            {t('competition.list.empty.action')}
          </Link>
        </div>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {competitions.map((competition) => (
            <div
              key={competition.id}
              className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden"
            >
              <div className="p-6">
                <div className="flex items-start justify-between mb-3">
                  <h2 className="text-xl font-semibold text-gray-900">
                    {competition.name}
                  </h2>
                  <span
                    className={`px-3 py-1 text-xs font-medium rounded-full ${
                      competition.status === 'Draft'
                        ? 'bg-gray-100 text-gray-700'
                        : competition.status === 'Open'
                        ? 'bg-green-100 text-green-700'
                        : competition.status === 'Judging'
                        ? 'bg-blue-100 text-blue-700'
                        : competition.status === 'Completed'
                        ? 'bg-purple-100 text-purple-700'
                        : 'bg-yellow-100 text-yellow-700'
                    }`}
                  >
                    {competition.status}
                  </span>
                </div>

                {competition.description && (
                  <p className="text-gray-600 text-sm mb-4 line-clamp-2">
                    {competition.description}
                  </p>
                )}

                <div className="space-y-2 text-sm text-gray-600">
                  <div className="flex items-center">
                    <span className="font-medium mr-2">📅 {t('competition.list.registrationDeadline')}:</span>
                    <span>{new Date(competition.registrationDeadline).toLocaleDateString()}</span>
                  </div>
                  <div className="flex items-center">
                    <span className="font-medium mr-2">🍻 {t('competition.list.judgingDate')}:</span>
                    <span>{new Date(competition.judgingStartDate).toLocaleDateString()}</span>
                  </div>
                  <div className="flex items-center">
                    <span className="font-medium mr-2">📊 {t('competition.list.maxEntries')}:</span>
                    <span>{competition.maxEntriesPerEntrant}</span>
                  </div>
                </div>
              </div>

              <div className="bg-gray-50 px-6 py-3 border-t border-gray-200">
                <button
                  className="text-amber-600 hover:text-amber-700 font-medium text-sm"
                  disabled
                >
                  {t('competition.list.viewDetails')} →
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {competitions && competitions.length > 0 && (
        <div className="mt-6 text-center text-sm text-gray-600">
          {t('competition.list.total', { count: competitions.length })}
        </div>
      )}
    </div>
  );
}
