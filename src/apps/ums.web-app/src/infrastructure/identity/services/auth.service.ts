import { publicClient } from '@infra/http/publicClient';

export interface ForgotPasswordPayload {
  tenantCode: string;
  email: string;
}

export interface ForgotPasswordResponse {
  message: string;
  simulatedTemporaryPassword: string | null;
}

export interface SignupUserPayload {
  tenantCode: string;
  displayName: string;
  email: string;
  password: string;
}

export interface SignupUserResponse {
  userAccountId: string;
  message: string;
}

export interface TenantSignupPayload {
  companyName: string;
  companyReference: string;
  contactName: string;
  contactEmail: string;
}

export interface TenantSignupResponse {
  tenantSignupRequestId: string;
  message: string;
}

const authService = {
  forgotPassword: async (payload: ForgotPasswordPayload): Promise<ForgotPasswordResponse> => {
    const { data } = await publicClient.post('/auth/forgot-password', {
      tenantCode: payload.tenantCode,
      email: payload.email,
    });
    return data as ForgotPasswordResponse;
  },

  signupUser: async (payload: SignupUserPayload): Promise<SignupUserResponse> => {
    const { data } = await publicClient.post('/auth/user-signup', {
      tenantCode: payload.tenantCode,
      displayName: payload.displayName,
      email: payload.email,
      password: payload.password,
    });
    return data as SignupUserResponse;
  },

  signupTenant: async (payload: TenantSignupPayload): Promise<TenantSignupResponse> => {
    const { data } = await publicClient.post('/auth/tenant-signup', {
      companyName: payload.companyName,
      companyReference: payload.companyReference,
      contactName: payload.contactName,
      contactEmail: payload.contactEmail,
    });
    return data as TenantSignupResponse;
  },
};

export default authService;
