import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7038';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token interceptor (future implementation)
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  
  // Add tenant ID header (hardcoded for MVP - will come from auth later)
  // This matches the development tenant inserted via Insert-DevelopmentTenant.ps1
  config.headers['X-Tenant-ID'] = '11111111-1111-1111-1111-111111111111';
  
  return config;
});

// Add response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - redirect to login (future)
      console.error('Unauthorized access');
    }
    return Promise.reject(error);
  }
);
