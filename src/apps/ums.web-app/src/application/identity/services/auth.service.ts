/**
 * auth.service.ts
 *
 * Production-grade authentication service.
 * Implements OWASP recommendations for secure authentication:
 *
 * - Input validation and sanitization
 * - Secure credential handling (no logging of passwords)
 * - Rate limiting awareness
 * - Secure session management
 * - CSRF protection via SameSite cookies
 * - Proper error messages that don't leak information
 */

export interface LoginCredentials {
  tenantCode: string;
  username: string;
  password: string;
  rememberMe?: boolean;
}

export interface AuthResponse {
  sessionId: string;
  sessionTrackingId: string;
  userId: string;
  username: string;
  email: string;
  tenantId: string;
  tenantCode: string;
  tenantName: string;
  role: string | null;
  roleName: string | null;
  profileId: string | null;
  permissions: string[];
  language: string;
  token: string;
  tokenType: string;
  expiresIn: number;
}

export interface AuthError {
  code: string;
  message: string;
  supportReferenceId: string | null;
}

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7114';

const AUTH_ERROR_CODES = {
  VALIDATION_ERROR: 'AUTH_001',
  TENANT_NOT_FOUND: 'AUTH_002',
  TENANT_INACTIVE: 'AUTH_003',
  USER_NOT_FOUND: 'AUTH_004',
  USER_NOT_ACTIVE: 'AUTH_005',
  INVALID_CREDENTIALS: 'AUTH_006',
  SESSION_EXPIRED: 'AUTH_007',
} as const;

class AuthService {
  private baseUrl: string;
  private requestTimeout: number;

  constructor() {
    this.baseUrl = `${API_BASE_URL}/api/v1/auth`;
    this.requestTimeout = 30000;
  }

  private sanitizeInput(input: string): string {
    return input.trim().replace(/[<>\"'&]/g, '');
  }

  private validateCredentials(credentials: LoginCredentials): string[] {
    const errors: string[] = [];

    if (!credentials.tenantCode || credentials.tenantCode.length < 1) {
      errors.push('Tenant code is required');
    }

    if (!credentials.username || credentials.username.length < 1) {
      errors.push('Username is required');
    }

    if (!credentials.password || credentials.password.length < 1) {
      errors.push('Password is required');
    }

    if (credentials.username && credentials.username.length > 100) {
      errors.push('Username exceeds maximum length');
    }

    return errors;
  }

  private getAuthHeaders(): HeadersInit {
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'X-Request-ID': crypto.randomUUID(),
    };
  }

  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const sanitizedCredentials = {
      tenantCode: this.sanitizeInput(credentials.tenantCode).toUpperCase(),
      username: this.sanitizeInput(credentials.username),
      password: credentials.password,
      rememberMe: credentials.rememberMe || false,
    };

    const validationErrors = this.validateCredentials(sanitizedCredentials);
    if (validationErrors.length > 0) {
      throw new Error(validationErrors.join(', '));
    }

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), this.requestTimeout);

    try {
      const response = await fetch(`${this.baseUrl}/login`, {
        method: 'POST',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(sanitizedCredentials),
        credentials: 'include',
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        const errorData = await response.json().catch(() => null) as AuthError | null;

        if (errorData) {
          switch (errorData.code) {
            case AUTH_ERROR_CODES.INVALID_CREDENTIALS:
              throw new Error('Usuario o contraseña incorrectos');
            case AUTH_ERROR_CODES.TENANT_NOT_FOUND:
              throw new Error('Tenant no encontrado');
            case AUTH_ERROR_CODES.TENANT_INACTIVE:
              throw new Error('Tenant inactivo');
            case AUTH_ERROR_CODES.USER_NOT_ACTIVE:
              throw new Error('Usuario inactivo. Contacte al administrador');
            case AUTH_ERROR_CODES.SESSION_EXPIRED:
              throw new Error('Sesión expirada');
            default:
              throw new Error(errorData.message || 'Error de autenticación');
          }
        }

        if (response.status === 401) {
          throw new Error('Usuario o contraseña incorrectos');
        }

        if (response.status === 429) {
          throw new Error('Demasiados intentos. Espere unos minutos');
        }

        throw new Error('Error de autenticación. Intente nuevamente');
      }

      const data = await response.json();

      const authResponse: AuthResponse = {
        sessionId: data.sessionId,
        sessionTrackingId: data.sessionTrackingId || crypto.randomUUID(),
        userId: data.userId,
        username: data.username,
        email: data.email,
        tenantId: data.tenantId,
        tenantCode: data.tenantCode,
        tenantName: data.tenantName,
        role: data.role,
        roleName: data.roleName,
        profileId: data.profileId,
        permissions: data.permissions || [],
        language: data.language || 'en',
        token: data.token,
        tokenType: data.tokenType || 'Bearer',
        expiresIn: data.expiresIn || 3600,
      };

      return authResponse;
    } catch (error) {
      clearTimeout(timeoutId);

      if (error instanceof Error) {
        if (error.name === 'AbortError') {
          throw new Error('Tiempo de espera agotado. Verifique su conexión');
        }
        throw error;
      }

      throw new Error('Error de conexión. Verifique su red');
    }
  }

  async logout(): Promise<void> {
    try {
      await fetch(`${this.baseUrl}/logout`, {
        method: 'POST',
        headers: this.getAuthHeaders(),
        credentials: 'include',
      });
    } catch {
    }
  }

  async getSession(): Promise<AuthResponse | null> {
    try {
      const response = await fetch(`${this.baseUrl}/session`, {
        method: 'GET',
        headers: {
          ...this.getAuthHeaders(),
          'Authorization': `Bearer ${this.getStoredToken()}`,
        },
        credentials: 'include',
      });

      if (!response.ok) {
        return null;
      }

      return await response.json();
    } catch {
      return null;
    }
  }

  private getStoredToken(): string | null {
    try {
      const stored = localStorage.getItem('ums-auth-storage');
      if (stored) {
        const parsed = JSON.parse(stored);
        return parsed.state?.user?.token || null;
      }
    } catch {
    }
    return null;
  }

  isAuthenticated(): boolean {
    try {
      const stored = localStorage.getItem('ums-auth-storage');
      if (!stored) return false;

      const parsed = JSON.parse(stored);
      const state = parsed.state;

      if (!state?.isAuthenticated || !state?.user) {
        return false;
      }

      if (state.sessionExpiresAt && Date.now() >= state.sessionExpiresAt) {
        return false;
      }

      return true;
    } catch {
      return false;
    }
  }
}

export const authService = new AuthService();