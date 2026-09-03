import { createContext, useContext, ReactNode, useState, useCallback, useEffect } from 'react';
import { createClient, TraumaPlatformClient } from '@shared/sdk/src/index';
import type { User, AuthToken } from '@shared/types/src/index';

interface AuthContextType {
  user: User | null;
  token: AuthToken | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  error: string | null;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  refreshToken: () => Promise<void>;
  client: TraumaPlatformClient;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<AuthToken | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Initialize API client with auth token
  const client = createClient({
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7001',
    apiVersion: 'v1',
  });

  // Load auth state from localStorage on mount
  useEffect(() => {
    const loadAuthState = () => {
      try {
        const savedToken = localStorage.getItem('auth_token');
        const savedUser = localStorage.getItem('auth_user');

        if (savedToken && savedUser) {
          const parsedToken = JSON.parse(savedToken) as AuthToken;
          const parsedUser = JSON.parse(savedUser) as User;
          
          setToken(parsedToken);
          setUser(parsedUser);
          // Set auth header for client
          client.setAccessToken(parsedToken.accessToken);
        }
      } catch (err) {
        console.error('Failed to load auth state:', err);
        localStorage.removeItem('auth_token');
        localStorage.removeItem('auth_user');
      } finally {
        setIsLoading(false);
      }
    };

    loadAuthState();
  }, [client]);

  const register = useCallback(
    async (email: string, password: string, displayName: string) => {
      setIsLoading(true);
      setError(null);

      try {
        const response = await client.register({
          email,
          password,
          displayName,
          acceptTerms: true,
          acceptPrivacyPolicy: true,
        });

        if (response.data.user && response.data.token) {
          setUser(response.data.user);
          setToken(response.data.token);
          client.setAccessToken(response.data.token.accessToken);

          // Save to localStorage
          localStorage.setItem('auth_token', JSON.stringify(response.data.token));
          localStorage.setItem('auth_user', JSON.stringify(response.data.user));
        } else {
          throw new Error('Registration failed - invalid response');
        }
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Registration failed';
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [client],
  );

  const login = useCallback(
    async (email: string, password: string) => {
      setIsLoading(true);
      setError(null);

      try {
        const response = await client.login({
          email,
          password,
        });

        if (response.data.user && response.data.token) {
          setUser(response.data.user);
          setToken(response.data.token);
          client.setAccessToken(response.data.token.accessToken);

          // Save to localStorage
          localStorage.setItem('auth_token', JSON.stringify(response.data.token));
          localStorage.setItem('auth_user', JSON.stringify(response.data.user));
        } else {
          throw new Error('Login failed - invalid response');
        }
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Login failed';
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [client],
  );

  const logout = useCallback(() => {
    setUser(null);
    setToken(null);
    setError(null);
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    client.clearAccessToken();
  }, [client]);

  const refreshAuthToken = useCallback(async () => {
    if (!token?.refreshToken) {
      logout();
      return;
    }

    try {
      const response = await client.refreshToken();

      if (response.data.token) {
        setToken(response.data.token);
        client.setAccessToken(response.data.token.accessToken);
        localStorage.setItem('auth_token', JSON.stringify(response.data.token));
      } else {
        logout();
      }
    } catch (err) {
      logout();
    }
  }, [token?.refreshToken, client, logout]);

  const value: AuthContextType = {
    user,
    token,
    isLoading,
    isAuthenticated: !!user && !!token,
    error,
    register,
    login,
    logout,
    refreshToken: refreshAuthToken,
    client,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
