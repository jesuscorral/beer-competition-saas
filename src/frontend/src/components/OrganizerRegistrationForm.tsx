import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { registerOrganizer } from '../api/auth';
import {
    createOrganizerRegistrationSchema,
    OrganizerRegistrationFormData,
    RegisterOrganizerRequest,
} from '../schemas/auth';

export function OrganizerRegistrationForm() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const { register, handleSubmit, formState: { errors } } = useForm<OrganizerRegistrationFormData>({
    resolver: zodResolver(createOrganizerRegistrationSchema()),
  });

  const registrationMutation = useMutation({
    mutationFn: registerOrganizer,
    onSuccess: (data) => {
      toast.success(t('auth.register.success'));
      
      // Store tenant ID for next step (competition creation)
      localStorage.setItem('tenant_id', data.tenantId);
      localStorage.setItem('user_id', data.userId);
      
      // Navigate to competition creation page
      navigate('/competitions/create', {
        state: {
          isFirstCompetition: true,
          tenantId: data.tenantId,
        },
      });
    },
    onError: (error: any) => {
      const errorMessage = error.response?.data?.error || t('auth.register.error');
      toast.error(errorMessage);
    },
  });

  const onSubmit = (formData: OrganizerRegistrationFormData) => {
    const requestData: RegisterOrganizerRequest = {
      email: formData.email,
      password: formData.password,
      organizationName: formData.organizationName,
      planName: formData.planName,
    };

    registrationMutation.mutate(requestData);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Organization Information */}
      <div className="bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">
          {t('auth.register.organizationInfo')}
        </h2>

        <div className="space-y-4">
          <div>
            <label htmlFor="organizationName" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.register.organizationName')}
            </label>
            <input
              id="organizationName"
              type="text"
              {...register('organizationName')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
              placeholder=""
            />
            {errors.organizationName && (
              <p className="text-red-600 text-sm mt-1">{errors.organizationName.message}</p>
            )}
          </div>

          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.register.email')}
            </label>
            <input
              id="email"
              type="email"
              {...register('email')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
              placeholder="organizer@brewery.com"
            />
            {errors.email && (
              <p className="text-red-600 text-sm mt-1">{errors.email.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* Account Security */}
      <div className="bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">
          {t('auth.register.accountSecurity')}
        </h2>

        <div className="space-y-4">
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.register.password')}
            </label>
            <input
              id="password"
              type="password"
              {...register('password')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
            />
            {errors.password && (
              <p className="text-red-600 text-sm mt-1">{errors.password.message}</p>
            )}
            <p className="text-sm text-gray-500 mt-1">
              {t('auth.validation.passwordHint')}
            </p>
          </div>

          <div>
            <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.register.confirmPassword')}
            </label>
            <input
              id="confirmPassword"
              type="password"
              {...register('confirmPassword')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
            />
            {errors.confirmPassword && (
              <p className="text-red-600 text-sm mt-1">{errors.confirmPassword.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* Subscription Plan */}
      <div className="bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">
          {t('auth.register.subscriptionPlan')}
        </h2>

        <div>
          <label htmlFor="planName" className="block text-sm font-medium text-gray-700 mb-1">
            {t('auth.register.selectPlan')}
          </label>
          <select
            id="planName"
            {...register('planName')}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
            defaultValue="TRIAL"
          >
            <option value="TRIAL">
              {t('auth.plans.TRIAL.name')} - {t('auth.plans.TRIAL.description')}
            </option>
            <option value="BASIC">
              {t('auth.plans.BASIC.name')} - {t('auth.plans.BASIC.description')}
            </option>
            <option value="STANDARD">
              {t('auth.plans.STANDARD.name')} - {t('auth.plans.STANDARD.description')}
            </option>
            <option value="PRO">
              {t('auth.plans.PRO.name')} - {t('auth.plans.PRO.description')}
            </option>
          </select>
          {errors.planName && (
            <p className="text-red-600 text-sm mt-1">{errors.planName.message}</p>
          )}
        </div>
      </div>

      {/* Submit Button */}
      <div className="flex justify-end space-x-4">
        <button
          type="button"
          onClick={() => navigate('/')}
          className="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
        >
          {t('common.cancel')}
        </button>
        <button
          type="submit"
          disabled={registrationMutation.isPending}
          className="px-6 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {registrationMutation.isPending
            ? t('auth.register.submitting')
            : t('auth.register.submit')}
        </button>
      </div>
    </form>
  );
}
