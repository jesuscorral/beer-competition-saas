import { OrganizerRegistrationResponse, RegisterOrganizerRequest } from '../schemas/auth';
import { apiClient } from './client';

/**
 * Register a new organizer (creates tenant, no competition)
 * Competition creation happens in a separate step
 */
export const registerOrganizer = async (
  data: RegisterOrganizerRequest
): Promise<OrganizerRegistrationResponse> => {
  const response = await apiClient.post<OrganizerRegistrationResponse>(
    '/api/user-registration/register-organizer',
    data
  );
  return response.data;
};

/**
 * Login (placeholder - to be implemented with Keycloak)
 */
export const login = async (email: string, password: string): Promise<{ token: string }> => {
  // TODO: Implement Keycloak OIDC flow
  throw new Error('Login not yet implemented');
};

/**
 * Logout (placeholder)
 */
export const logout = async (): Promise<void> => {
  localStorage.removeItem('auth_token');
  localStorage.removeItem('tenant_id');
};
