import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7038';
const TENANT_ID = import.meta.env.VITE_TENANT_ID;
const DEV_TENANT_ID = '11111111-1111-1111-1111-111111111111';

// Warn developers if using default development tenant ID in production
if (import.meta.env.PROD && TENANT_ID === DEV_TENANT_ID) {
  console.warn(
    '⚠️ WARNING: Using default development tenant ID in production build. ' +
    'Set VITE_TENANT_ID environment variable to a valid production tenant ID.'
  );
}

// Warn if tenant ID is not configured
if (!TENANT_ID) {
  console.warn(
    '⚠️ WARNING: VITE_TENANT_ID not configured. ' +
    'Set VITE_TENANT_ID environment variable in your .env file.'
  );
}

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
  
  // Add tenant ID header (from environment variable - will come from auth JWT later)
  // This matches the development tenant inserted via Insert-DevelopmentTenant.ps1
  if (TENANT_ID) {
    config.headers['X-Tenant-ID'] = TENANT_ID;
  }
  
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
