import { useState } from 'react';
import { AppShell } from './components/layout/AppShell';
import { LoginScreen } from './screens/LoginScreen';
import { OverviewScreen } from './screens/OverviewScreen';
import { WorkersScreen } from './screens/WorkersScreen';
import { WorkerDetailsScreen } from './screens/WorkerDetailsScreen';
import { AssessmentsScreen } from './screens/AssessmentsScreen';
import { ModuleAnalyticsScreen } from './screens/ModuleAnalyticsScreen';
import { CompetencyScreen } from './screens/CompetencyScreen';
import { RetrainingScreen } from './screens/RetrainingScreen';
import { CertificatesScreen } from './screens/CertificatesScreen';
import { RetentionScreen } from './screens/RetentionScreen';
import { ReportsScreen } from './screens/ReportsScreen';
import { UnauthorizedState } from './components/common/UnauthorizedState';
import type { AdminSession, DateRange, DateRangePreset } from './types';

const SESSION_KEY = 'surakshaar_admin_session';

function readStoredSession(): AdminSession | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (raw) return JSON.parse(raw) as AdminSession;
  } catch {
    // Ignore corrupted stored sessions.
  }
  return null;
}

/** Authorized screens for the Administrator role (mock role-based access). */
const ADMIN_ALLOWED_SCREENS = [
  'overview',
  'workers',
  'worker-details',
  'assessments',
  'modules',
  'competency',
  'retraining',
  'certificates',
  'retention',
  'reports',
];

export function App() {
  const [session, setSession] = useState<AdminSession | null>(readStoredSession());
  const [currentScreen, setCurrentScreen] = useState('overview');
  const [selectedWorkerId, setSelectedWorkerId] = useState('w-101');
  const [selectedDateRange, setSelectedDateRange] = useState<DateRangePreset>('Last 30 Days');
  const [customRange, setCustomRange] = useState<DateRange | null>(null);

  const isAuthenticated = session !== null;

  // If not authenticated, render Login screen (mock auth only).
  if (!isAuthenticated) {
    return (
      <LoginScreen
        onLogin={(domain) => {
          const nextSession: AdminSession = {
            name: 'Administrator',
            email: 'admin@surakshaar.jharkhand.gov.in',
            role: 'administrator',
            roleLabel: 'Administrator',
            domain: domain ?? 'Jharkhand Industrial Safety Command',
          };
          sessionStorage.setItem(SESSION_KEY, JSON.stringify(nextSession));
          setSession(nextSession);
        }}
      />
    );
  }

  const handleNavigate = (screen: string, param?: string) => {
    if (screen === 'worker-details') {
      if (param) setSelectedWorkerId(param);
      setCurrentScreen('worker-details');
    } else {
      setCurrentScreen(screen);
    }
  };

  const handleLogout = () => {
    sessionStorage.removeItem(SESSION_KEY);
    setSession(null);
    setCurrentScreen('overview');
  };

  // Mock role-based access guard.
  const isScreenAllowed = ADMIN_ALLOWED_SCREENS.includes(currentScreen);

  const getScreenMeta = () => {
    switch (currentScreen) {
      case 'overview':
        return {
          title: 'Safety & Compliance Overview',
          description: 'Monitor workforce training, assessments, competency and compliance.',
        };
      case 'workers':
        return {
          title: 'Workers Directory',
          description: 'Search, inspect dossiers, and manage AR training compliance for Jharkhand personnel.',
        };
      case 'worker-details':
        return {
          title: 'Worker Profile & Dossier',
          description: 'Detailed assessment history, competency radar, and certificate compliance.',
        };
      case 'assessments':
        return {
          title: 'Assessment Logs & Telemetry',
          description: 'Audit AR simulation assessment runs, step telemetry, and critical safety error flags.',
        };
      case 'modules':
        return {
          title: 'Module Performance Analytics',
          description: 'In-depth performance, completion rates, and critical error analysis for Fire, Gas & Machinery modules.',
        };
      case 'competency':
        return {
          title: 'Workforce Competency & Technical Skill Analytics',
          description: 'Statewide safety competency assessment, skill dimension breakdown, and vulnerability mapping.',
        };
      case 'retraining':
        return {
          title: 'Safety Retraining & Mandatory Re-evaluation',
          description: 'Operational queue for workers flagged for remedial AR training and score improvement tracking.',
        };
      case 'certificates':
        return {
          title: 'Compliance Certificate Registry & Verification',
          description: 'Simulated DEMO certificate registry for training compliance previews. Not an official government registry.',
        };
      case 'retention':
        return {
          title: 'Knowledge Retention Monitoring (Ebbinghaus Protocol)',
          description: 'Track post-training safety recall decay across Day 1, Day 7, and Day 30 workplace audits.',
        };
      case 'reports':
        return {
          title: 'Compliance Reports & Export Center',
          description: 'Configure granular filters and generate CSV/PDF demo report previews for state administration.',
        };
      default:
        return {
          title: 'Access Restricted',
          description: 'Unauthorized dashboard navigation.',
        };
    }
  };

  const screenMeta = getScreenMeta();

  return (
    <AppShell
      currentScreen={currentScreen}
      onNavigate={handleNavigate}
      onLogout={handleLogout}
      screenTitle={screenMeta.title}
      screenDescription={screenMeta.description}
      selectedDateRange={selectedDateRange}
      onDateRangeChange={setSelectedDateRange}
      customRange={customRange ?? undefined}
      onCustomRangeChange={setCustomRange}
    >
      {!isScreenAllowed ? (
        <UnauthorizedState onBackToOverview={() => handleNavigate('overview')} />
      ) : (
        <>
          {currentScreen === 'overview' && (
            <OverviewScreen
              onNavigateToWorker={(id) => handleNavigate('worker-details', id)}
              onNavigateToScreen={(scr) => handleNavigate(scr)}
              dateRange={selectedDateRange}
              customRange={customRange}
            />
          )}
          {currentScreen === 'workers' && (
            <WorkersScreen onSelectWorker={(id) => handleNavigate('worker-details', id)} />
          )}
          {currentScreen === 'worker-details' && (
            <WorkerDetailsScreen
              workerId={selectedWorkerId}
              onBack={() => handleNavigate('workers')}
            />
          )}
          {currentScreen === 'assessments' && <AssessmentsScreen />}
          {currentScreen === 'modules' && <ModuleAnalyticsScreen />}
          {currentScreen === 'competency' && (
            <CompetencyScreen onNavigateToScreen={(scr) => handleNavigate(scr)} />
          )}
          {currentScreen === 'retraining' && <RetrainingScreen />}
          {currentScreen === 'certificates' && <CertificatesScreen />}
          {currentScreen === 'retention' && <RetentionScreen />}
          {currentScreen === 'reports' && <ReportsScreen />}
        </>
      )}
    </AppShell>
  );
}

export default App;