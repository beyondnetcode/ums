import { describe, it, expect, beforeEach } from 'vitest';
import { useAuthStore } from './auth.store';

describe('auth.store', () => {
  beforeEach(() => {
    useAuthStore.setState({
      user: null,
      isAuthenticated: false,
    });
  });

  it('initializes with correct state shape', () => {
    const state = useAuthStore.getState();
    expect(state).toHaveProperty('user');
    expect(state).toHaveProperty('isAuthenticated');
    expect(state).toHaveProperty('login');
    expect(state).toHaveProperty('logout');
  });

  it('logs in a user', () => {
    const testUser = {
      id: 'test-123',
      username: 'testuser',
      email: 'test@example.com',
      role: 'admin',
    };

    useAuthStore.getState().login(testUser);

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(true);
    expect(state.user).toEqual(testUser);
  });

  it('logs out a user', () => {
    const testUser = {
      id: 'test-123',
      username: 'testuser',
      email: 'test@example.com',
      role: 'admin',
    };

    useAuthStore.getState().login(testUser);
    expect(useAuthStore.getState().isAuthenticated).toBe(true);

    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
  });

  it('selects user username via selector', () => {
    const testUser = {
      id: 'test-123',
      username: 'selector-test',
      email: 'test@example.com',
      role: 'user',
    };

    useAuthStore.getState().login(testUser);

    const username = useAuthStore.getState().user?.username;
    expect(username).toBe('selector-test');
  });

  it('overwrites previous user on subsequent login', () => {
    const user1 = { id: 'u1', username: 'first', email: 'a@b.com', role: 'user' };
    const user2 = { id: 'u2', username: 'second', email: 'c@d.com', role: 'admin' };

    useAuthStore.getState().login(user1);
    expect(useAuthStore.getState().user?.username).toBe('first');

    useAuthStore.getState().login(user2);
    expect(useAuthStore.getState().user?.username).toBe('second');
    expect(useAuthStore.getState().isAuthenticated).toBe(true);
  });
});
