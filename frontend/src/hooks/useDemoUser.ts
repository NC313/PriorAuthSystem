import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import type { DemoUser } from '../types';

export function useDemoUser() {
  const navigate = useNavigate();

  const role = localStorage.getItem('demo_role');
  const userJson = localStorage.getItem('demo_user');

  let user: DemoUser | null = null;
  if (role && userJson) {
    try {
      user = JSON.parse(userJson) as DemoUser;
    } catch {
      user = null;
    }
  }

  const logout = useCallback(() => {
    localStorage.removeItem('demo_role');
    localStorage.removeItem('demo_user');
    navigate('/');
  }, [navigate]);

  return { user, logout };
}
