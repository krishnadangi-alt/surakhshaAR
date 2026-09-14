export interface ModuleAnalyticsData {
  moduleId: string;
  moduleName: string;
  iconName: string;
  totalEnrolled: number;
  completedCount: number;
  certifiedCount: number;
  passRate: number;
  averageScore: number;
  criticalErrorCount: number;
  monthlyPerformance: { month: string; pass: number; fail: number; avgScore: number }[];
  scenarios: { name: string; attempts: number; passRate: number; avgScore: number; criticalErrors: number }[];
}

export const mockModules: ModuleAnalyticsData[] = [
  {
    moduleId: 'm-fire',
    moduleName: 'Fire Safety',
    iconName: 'Flame',
    totalEnrolled: 5420,
    completedCount: 4890,
    certifiedCount: 4620,
    passRate: 91.2,
    averageScore: 89.4,
    criticalErrorCount: 84,
    monthlyPerformance: [
      { month: 'Apr', pass: 720, fail: 60, avgScore: 88 },
      { month: 'May', pass: 780, fail: 55, avgScore: 89 },
      { month: 'Jun', pass: 810, fail: 70, avgScore: 87 },
      { month: 'Jul', pass: 860, fail: 62, avgScore: 90 },
      { month: 'Aug', pass: 920, fail: 58, avgScore: 91 },
      { month: 'Sep', pass: 800, fail: 48, avgScore: 92 },
    ],
    scenarios: [
      { name: 'Class A/B Portable Extinguisher PASS Method', attempts: 1840, passRate: 94.5, avgScore: 92.1, criticalErrors: 18 },
      { name: 'Electrical Panel Flashover CO2 Extinction', attempts: 1520, passRate: 88.2, avgScore: 86.8, criticalErrors: 32 },
      { name: 'Heavy Dumper Engine Compartment Foam Drill', attempts: 1120, passRate: 85.0, avgScore: 84.2, criticalErrors: 24 },
      { name: 'Chemical Fire & Foam System Operation', attempts: 940, passRate: 90.1, avgScore: 89.0, criticalErrors: 10 },
    ]
  },
  {
    moduleId: 'm-gas',
    moduleName: 'Gas Safety',
    iconName: 'Wind',
    totalEnrolled: 4850,
    completedCount: 4120,
    certifiedCount: 3890,
    passRate: 84.6,
    averageScore: 83.8,
    criticalErrorCount: 142,
    monthlyPerformance: [
      { month: 'Apr', pass: 610, fail: 110, avgScore: 81 },
      { month: 'May', pass: 650, fail: 95, avgScore: 82 },
      { month: 'Jun', pass: 690, fail: 105, avgScore: 83 },
      { month: 'Jul', pass: 720, fail: 90, avgScore: 84 },
      { month: 'Aug', pass: 770, fail: 85, avgScore: 85 },
      { month: 'Sep', pass: 680, fail: 68, avgScore: 86 },
    ],
    scenarios: [
      { name: 'Blast Furnace CO Gas Isolation & LOTO', attempts: 1650, passRate: 86.4, avgScore: 85.2, criticalErrors: 42 },
      { name: 'Methane CH4 Sensor Calibration & Evacuation', attempts: 1420, passRate: 79.2, avgScore: 78.6, criticalErrors: 64 },
      { name: 'SCBA Mask Donning & Positive Pressure Check', attempts: 1180, passRate: 92.0, avgScore: 91.0, criticalErrors: 12 },
      { name: 'SO2 Acid Plant Scrubber Emergency Shutdown', attempts: 600, passRate: 84.0, avgScore: 82.5, criticalErrors: 24 },
    ]
  },
  {
    moduleId: 'm-mach',
    moduleName: 'Machinery Safety',
    iconName: 'Cog',
    totalEnrolled: 4010,
    completedCount: 3510,
    certifiedCount: 3340,
    passRate: 88.5,
    averageScore: 87.1,
    criticalErrorCount: 96,
    monthlyPerformance: [
      { month: 'Apr', pass: 520, fail: 70, avgScore: 85 },
      { month: 'May', pass: 560, fail: 65, avgScore: 86 },
      { month: 'Jun', pass: 600, fail: 68, avgScore: 87 },
      { month: 'Jul', pass: 640, fail: 58, avgScore: 88 },
      { month: 'Aug', pass: 670, fail: 52, avgScore: 89 },
      { month: 'Sep', pass: 520, fail: 40, avgScore: 90 },
    ],
    scenarios: [
      { name: 'Overhead Gantry Crane Interlock & E-Stop', attempts: 1410, passRate: 92.1, avgScore: 91.4, criticalErrors: 16 },
      { name: 'Power Press Light Curtain Guard Verification', attempts: 1150, passRate: 86.8, avgScore: 85.0, criticalErrors: 38 },
      { name: 'Conveyor Belt Pinch Point Protection & LOTO', attempts: 980, passRate: 87.5, avgScore: 86.2, criticalErrors: 28 },
      { name: 'Hydraulic Crusher Maintenance Isolation', attempts: 470, passRate: 84.2, avgScore: 83.1, criticalErrors: 14 },
    ]
  }
];
