import type { NotificationItem } from '../types';

export const mockNotifications: NotificationItem[] = [
  {
    id: 'n-1',
    title: 'Critical Safety Failure Recorded',
    message: 'Worker Manoj Kumar Gupta (JHK-DC-5534) triggered 2 critical errors in Fire Extinguisher AR drill.',
    timestamp: '10 minutes ago',
    type: 'critical',
    read: false,
  },
  {
    id: 'n-2',
    title: 'Certificates Expiring (Bokaro Zone)',
    message: '14 compliance certificates in Blast Furnace Shop #2 require annual renewal in 7 days.',
    timestamp: '1 hour ago',
    type: 'warning',
    read: false,
  },
  {
    id: 'n-3',
    title: 'Retraining Completed Successfully',
    message: 'Vikramaditya Oraon (JHK-RC-3088) completed LOTO Retraining with +20% score improvement.',
    timestamp: '3 hours ago',
    type: 'success',
    read: true,
  },
  {
    id: 'n-4',
    title: 'Day 30 Retention Audit Due',
    message: 'Pankaj Kumar Saw (JHK-RM-9102) is scheduled for Day 30 compliance audit tomorrow.',
    timestamp: '5 hours ago',
    type: 'info',
    read: true,
  }
];
