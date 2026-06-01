import axios from 'axios';
import { BASE_URL } from '@infra/http/request-context';

// Public client — no X-Tenant-Id or auth headers (used for anonymous endpoints like forgot-password)
const publicClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  withCredentials: false,
});

export interface ForgotPasswordPayload {
  tenantCode: string;
  email: string;
}

export interface ForgotPasswordResponse {
  message: string;
  simulatedTemporaryPassword: string | null;
}

const authService = {
  forgotPassword: async (payload: ForgotPasswordPayload): Promise<ForgotPasswordResponse> => {
    const { data } = await publicClient.post('/auth/forgot-password', {
      tenantCode: payload.tenantCode,
      email: payload.email,
    });
    return data as ForgotPasswordResponse;
  },
};

export default authService;
