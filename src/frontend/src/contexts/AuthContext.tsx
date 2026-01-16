import { createContext, ReactNode, useContext, useEffect, useState } from 'react';

interface User {
  id: string;
  email: string;
  roles: string[];
  tenantId?: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isOrganizer: boolean;
  isJudge: boolean;
  isEntrant: boolean;
  isSteward: boolean;
  login: (user: User) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  // Load user from localStorage on mount
  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch (error) {
        console.error('Error parsing stored user:', error);
        localStorage.removeItem('user');
      }
    }
  }, []);

  const login = (userData: User) => {
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData));
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('user');
    localStorage.removeItem('auth_token');
    localStorage.removeItem('tenant_id');
    localStorage.removeItem('user_id');
  };

  const isAuthenticated = user !== null;
  const isOrganizer = user?.roles.includes('organizer') ?? false;
  const isJudge = user?.roles.includes('judge') ?? false;
  const isEntrant = user?.roles.includes('entrant') ?? false;
  const isSteward = user?.roles.includes('steward') ?? false;

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated,
        isOrganizer,
        isJudge,
        isEntrant,
        isSteward,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
