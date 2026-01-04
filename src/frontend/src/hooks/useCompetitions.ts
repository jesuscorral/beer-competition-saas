import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import { CompetitionFormData } from '../schemas/competition';

// Type definition for Competition (matches backend DTO)
export interface Competition {
  id: string;
  name: string;
  description?: string;
  registrationDeadline: string;
  judgingStartDate: string;
  judgingEndDate?: string;
  status: string;
  maxEntriesPerEntrant: number;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Hook to fetch all competitions for the current tenant.
 * Uses TanStack Query for caching, loading states, and automatic refetching.
 */
export function useCompetitions() {
  return useQuery<Competition[]>({
    queryKey: ['competitions'],
    queryFn: async () => {
      const response = await apiClient.get<Competition[]>('/api/competitions');
      return response.data;
    },
    staleTime: 30000, // Consider data fresh for 30 seconds
    refetchOnWindowFocus: true, // Refetch when user returns to tab
  });
}

/**
 * Hook to create a new competition.
 * Automatically invalidates competitions list cache on success.
 */
export function useCreateCompetition() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CompetitionFormData) => {
      // Convert date strings to ISO 8601 format with time
      const payload = {
        name: data.name,
        description: data.description || null,
        registrationDeadline: new Date(data.registrationDeadline).toISOString(),
        judgingStartDate: new Date(data.judgingStartDate).toISOString(),
      };
      
      const response = await apiClient.post('/api/competitions', payload);
      return response.data;
    },
    onSuccess: () => {
      // Invalidate and refetch competitions list
      queryClient.invalidateQueries({ queryKey: ['competitions'] });
    },
  });
}
