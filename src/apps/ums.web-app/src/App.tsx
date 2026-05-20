import React, { useState } from 'react';
import { MainLayout } from './presentation/shared/layouts/MainLayout';
import { TenantDashboardScreen } from './presentation/identity/screens/TenantDashboardScreen';
import { ProfileScreen } from './presentation/identity/screens/ProfileScreen';
import { LoginScreen } from './presentation/identity/screens/LoginScreen';
import { useAuthStore } from './application/stores/auth.store';

export default function App() {
  const [activeTab, setActiveTab] = useState<'tenants' | 'profile' | 'login'>('tenants');
  const isDarkMode = useAuthStore((state) => state.isDarkMode);

  return (
    <div className={isDarkMode ? 'dark' : ''}>
      <MainLayout activeTab={activeTab} setActiveTab={setActiveTab}>
        {activeTab === 'tenants' && <TenantDashboardScreen />}
        {activeTab === 'profile' && <ProfileScreen />}
        {activeTab === 'login' && <LoginScreen />}
      </MainLayout>
    </div>
  );
}
