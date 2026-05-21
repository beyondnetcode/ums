import React, { useState } from 'react';
import { TopAppBar } from './TopAppBar';
import { NavRail } from './NavRail';

export const MainLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [navCollapsed, setNavCollapsed] = useState(false);

  return (
    <div className="min-h-screen flex flex-col transition-colors duration-500 bg-m3-surface text-m3-on-surface">
      <div className="m3-glow top-0 left-1/4 w-[450px] h-[450px] bg-m3-primary/10 dark:bg-m3-primary/5" />
      <div className="m3-glow bottom-10 right-10 w-[350px] h-[350px] bg-indigo-500/10 dark:bg-indigo-500/5" />

      <TopAppBar navCollapsed={navCollapsed} onToggleNav={() => setNavCollapsed(!navCollapsed)} />

      <div className="flex-1 flex overflow-hidden">
        <NavRail collapsed={navCollapsed} />

        <main className="flex-1 overflow-y-auto flex flex-col relative">
          <div className="flex-1 flex flex-col p-5 min-h-0">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
};
