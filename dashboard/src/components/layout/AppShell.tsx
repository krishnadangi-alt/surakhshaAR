import React, { useState } from 'react';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { NotificationsDrawer } from './NotificationsDrawer';
import { SearchModal } from './SearchModal';
import type { DateRange, DateRangePreset } from '../../types';

interface AppShellProps {
  currentScreen: string;
  onNavigate: (screen: string, param?: string) => void;
  onLogout: () => void;
  children: React.ReactNode;
  screenTitle: string;
  screenDescription?: string;
  selectedDateRange: DateRangePreset;
  onDateRangeChange: (preset: DateRangePreset) => void;
  customRange?: DateRange;
  onCustomRangeChange?: (range: DateRange) => void;
}

export const AppShell: React.FC<AppShellProps> = ({
  currentScreen,
  onNavigate,
  onLogout,
  children,
  screenTitle,
  screenDescription,
  selectedDateRange,
  onDateRangeChange,
  customRange,
  onCustomRangeChange,
}) => {
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [isSearchOpen, setIsSearchOpen] = useState(false);

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-suraksha-bg text-suraksha-text">
      {/* Navigation Sidebar */}
      <Sidebar
        currentScreen={currentScreen}
        onNavigate={(screen) => onNavigate(screen)}
        isCollapsed={isSidebarCollapsed}
        onToggleCollapse={() => setIsSidebarCollapsed(!isSidebarCollapsed)}
        onLogout={onLogout}
      />

      {/* Main Content Area */}
      <div className="flex flex-1 flex-col overflow-hidden">
        <Topbar
          title={screenTitle}
          description={screenDescription}
          selectedDateRange={selectedDateRange}
          onDateRangeChange={onDateRangeChange}
          customRange={customRange}
          onCustomRangeChange={onCustomRangeChange}
          onOpenNotifications={() => setIsNotificationsOpen(true)}
          onOpenSearch={() => setIsSearchOpen(true)}
        />

        {/* Screen Page Viewport */}
        <main className="flex-1 overflow-y-auto p-6 space-y-6">
          {children}
        </main>
      </div>

      {/* Slide-over Drawers & Overlays */}
      <NotificationsDrawer isOpen={isNotificationsOpen} onClose={() => setIsNotificationsOpen(false)} />
      <SearchModal
        isOpen={isSearchOpen}
        onClose={() => setIsSearchOpen(false)}
        onNavigate={(screen, param) => onNavigate(screen, param)}
      />
    </div>
  );
};
