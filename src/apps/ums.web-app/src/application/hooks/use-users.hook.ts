import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';

const API_URL = 'http://localhost:3000/api';

export interface User {
  id: string;
  username: string;
  email: string;
  role: string;
  createdAt: string;
}

// LocalStorage helpers for resilient offline mode
const getLocalUsers = (): User[] => {
  const data = localStorage.getItem('ums_fallback_users');
  if (data) return JSON.parse(data);
  const defaults: User[] = [
    { id: '1', username: 'admin_root', email: 'admin@ums.com', role: 'admin', createdAt: new Date().toISOString() },
    { id: '2', username: 'alex_developer', email: 'alex@ums.com', role: 'user', createdAt: new Date().toISOString() }
  ];
  localStorage.setItem('ums_fallback_users', JSON.stringify(defaults));
  return defaults;
};

const saveLocalUser = (user: Omit<User, 'createdAt'>): User => {
  const list = getLocalUsers();
  const newUser: User = { ...user, createdAt: new Date().toISOString() };
  list.push(newUser);
  localStorage.setItem('ums_fallback_users', JSON.stringify(list));
  return newUser;
};

export const useGetUsers = () => {
  return useQuery<User[]>({
    queryKey: ['users'],
    queryFn: async () => {
      try {
        const response = await axios.get(`${API_URL}/users`);
        return response.data;
      } catch (error) {
        console.warn('[UMS Web] API offline. Falling back to local storage cache.', error);
        return getLocalUsers();
      }
    }
  });
};

export const useRegisterUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: { username: string; email: string; role: string }) => {
      try {
        const response = await axios.post(`${API_URL}/users/register`, {
          ...payload,
          password: 'Password123!' // default strong password for simulation
        });
        return response.data.data;
      } catch (error) {
        console.warn('[UMS Web] API offline. Simulating registration locally.', error);
        return saveLocalUser({
          id: Math.random().toString(36).substring(2, 11),
          username: payload.username,
          email: payload.email,
          role: payload.role
        });
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
    }
  });
};
