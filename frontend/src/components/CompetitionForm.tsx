import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useCreateCompetition } from '../hooks/useCompetitions';
import { CompetitionFormData, createCompetitionSchema } from '../schemas/competition';

export function CompetitionForm() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors } } = useForm<CompetitionFormData>({
    resolver: zodResolver(createCompetitionSchema()),
  });

  const createCompetitionMutation = useCreateCompetition();

  const onSubmit = async (data: CompetitionFormData) => {
    try {
      const result = await createCompetitionMutation.mutateAsync(data);
      toast.success(t('competition.create.success', { id: result.id || 'N/A' }));
      setTimeout(() => navigate('/'), 2000);
    } catch (error: unknown) {
      let errorMessage = t('competition.create.error');

      if (error && typeof error === 'object' && 'response' in error) {
        const response = (error as { response?: { data?: { message?: string; title?: string } } }).response;
        const data = response?.data;
        if (data && typeof data === 'object') {
          const message = (data as { message?: string }).message;
          const title = (data as { title?: string }).title;
          errorMessage = message || title || errorMessage;
        }
      }
      toast.error(t('competition.create.errorMessage', { message: errorMessage }));
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold mb-6 text-gray-800">{t('competition.create.formTitle')}</h2>
      
      {/* Competition Name */}
      <div className="mb-4">
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
          {t('competition.create.name.label')} <span className="text-red-500">*</span>
        </label>
        <input
          id="name"
          type="text"
          {...register('name')}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
          placeholder={t('competition.create.name.placeholder')}
        />
        {errors.name && (
          <p className="text-red-500 text-sm mt-1">{errors.name.message}</p>
        )}
      </div>

      {/* Description */}
      <div className="mb-4">
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
          {t('competition.create.description.label')}
        </label>
        <textarea
          id="description"
          {...register('description')}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
          rows={4}
          placeholder={t('competition.create.description.placeholder')}
        />
        {errors.description && (
          <p className="text-red-500 text-sm mt-1">{errors.description.message}</p>
        )}
      </div>

      {/* Registration Deadline */}
      <div className="mb-4">
        <label htmlFor="registrationDeadline" className="block text-sm font-medium text-gray-700 mb-2">
          {t('competition.create.registrationDeadline.label')} <span className="text-red-500">*</span>
        </label>
        <input
          id="registrationDeadline"
          type="date"
          {...register('registrationDeadline')}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
        />
        {errors.registrationDeadline && (
          <p className="text-red-500 text-sm mt-1">{errors.registrationDeadline.message}</p>
        )}
      </div>

      {/* Judging Date */}
      <div className="mb-6">
        <label htmlFor="judgingStartDate" className="block text-sm font-medium text-gray-700 mb-2">
          {t('competition.create.judgingDate.label')} <span className="text-red-500">*</span>
        </label>
        <input
          id="judgingStartDate"
          type="date"
          {...register('judgingStartDate')}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
        />
        {errors.judgingStartDate && (
          <p className="text-red-500 text-sm mt-1">{errors.judgingStartDate.message}</p>
        )}
      </div>

      {/* Submit Button */}
      <button
        type="submit"
        disabled={createCompetitionMutation.isPending}
        className="w-full bg-amber-600 text-white py-3 rounded-lg font-semibold hover:bg-amber-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
      >
        {createCompetitionMutation.isPending ? t('competition.create.submitting') : t('competition.create.submit')}
      </button>
    </form>
  );
}
