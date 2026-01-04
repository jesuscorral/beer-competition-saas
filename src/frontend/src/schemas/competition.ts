import { z } from 'zod';
import i18n from '../i18n';

/**
 * Factory function to create competition schema with i18n support.
 * Must be called as a function to get fresh translations.
 */
export const createCompetitionSchema = () => z.object({
  name: z.string()
    .min(1, i18n.t('competition.validation.nameRequired'))
    .max(255, i18n.t('competition.validation.nameMaxLength')),
  
  description: z.string()
    .max(1000, i18n.t('competition.validation.descriptionMaxLength'))
    .optional()
    .or(z.literal('')),
  
  registrationDeadline: z.string()
    .refine((date) => new Date(date) > new Date(), {
      message: i18n.t('competition.validation.registrationDeadlineFuture')
    }),
  
  judgingStartDate: z.string()
    .refine((date) => new Date(date) > new Date(), {
      message: i18n.t('competition.validation.judgingDateFuture')
    })
}).refine((data) => new Date(data.judgingStartDate) > new Date(data.registrationDeadline), {
  message: i18n.t('competition.validation.judgingDateAfterRegistration'),
  path: ['judgingStartDate']
});

export type CompetitionFormData = z.infer<ReturnType<typeof createCompetitionSchema>>;
