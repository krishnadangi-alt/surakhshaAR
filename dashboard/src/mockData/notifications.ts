import type { NotificationItem } from '../types';

export const mockNotifications: NotificationItem[] = [
  {
    id: 'n-1',
    title: 'Critical Safety Failure Recorded',
    message: 'Worker Amit Singh Munda (JHK-CZ-4190) failed assessment (Critical Error: Bypassed PPE Selection in Hazard Zone).',
    timestamp: '10 minutes ago',
    type: 'critical',
    read: false,
  },
  {
    id: 'n-2',
    title: 'Certificates Expiring (Dhanbad Region-1)',
    message: '14 compliance certificates in Dhanbad Region-1 require annual renewal within 30 days.',
    timestamp: '1 hour ago',
    type: 'warning',
    read: false,
  },
  {
    id: 'n-3',
    title: 'Retraining Completed Successfully',
    message: 'Vikramaditya Oraon (JHK-SE-3088) completed Machinery Retraining with +20% score improvement.',
    timestamp: '3 hours ago',
    type: 'success',
    read: true,
  },
  {
    id: 'n-4',
    title: 'Day 30 Retention Check Scheduled',
    message: 'Pankaj Kumar Saw (JHK-SE-9102) is scheduled for Day 30 retention check tomorrow.',
    timestamp: '5 hours ago',
    type: 'info',
    read: true,
  }
];
