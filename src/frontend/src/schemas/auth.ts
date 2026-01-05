import { z } from 'zod';
import i18n from '../i18n';

/**
 * Factory function to create organizer registration schema with i18n support.
 * Must be called as a function to get fresh translations.
 */
export const createOrganizerRegistrationSchema = () => z.object({
  email: z.string()
    .min(1, i18n.t('auth.validation.emailRequired'))
    .email(i18n.t('auth.validation.emailInvalid'))
    .max(255, i18n.t('auth.validation.emailMaxLength')),
  
  password: z.string()
    .min(8, i18n.t('auth.validation.passwordMinLength'))
    .regex(/[A-Z]/, i18n.t('auth.validation.passwordUppercase'))
    .regex(/[a-z]/, i18n.t('auth.validation.passwordLowercase'))
    .regex(/[0-9]/, i18n.t('auth.validation.passwordDigit')),
  
  confirmPassword: z.string()
    .min(1, i18n.t('auth.validation.confirmPasswordRequired')),
  
  organizationName: z.string()
    .min(1, i18n.t('auth.validation.organizationRequired'))
    .max(255, i18n.t('auth.validation.organizationMaxLength')),
  
  planName: z.enum(['TRIAL', 'BASIC', 'STANDARD', 'PRO'], {
    errorMap: () => ({ message: i18n.t('auth.validation.planRequired') })
  })
}).refine((data) => data.password === data.confirmPassword, {
  message: i18n.t('auth.validation.passwordMismatch'),
  path: ['confirmPassword']
});

export type OrganizerRegistrationFormData = z.infer<ReturnType<typeof createOrganizerRegistrationSchema>>;

/**
 * DTO sent to backend (without confirmPassword)
 */
export interface RegisterOrganizerRequest {
  email: string;
  password: string;
  organizationName: string;
  planName: string;
}

/**
 * Response from backend after successful registration
 */
export interface OrganizerRegistrationResponse {
  tenantId: string;
  userId: string;
}
