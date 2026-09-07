# SURAKSHAAR — Admin Dashboard

Professional admin/trainer dashboard for the SurakshaAR Industrial Safety Training &
Competency Management System. Built for the Government of India / Smart India Hackathon.

## Design

Modern government/enterprise safety-tech aesthetic. Subtle tricolor accent, animated
micro-interactions, information-first hierarchy. Feels alive without being flashy.

### Animation Features
- **AnimatedCounter** — KPI numbers count up from 0 on load
- **ProgressRing** — SVG circular progress with smooth fill animation
- **BarChart / DonutChart** — animate into view with stagger delays
- **Hover states** — cards lift, buttons scale, table rows highlight
- **Page transitions** — fade-in-up on route change
- **`prefers-reduced-motion`** — all animations disabled for accessibility

## Tech Stack

- React 18 + TypeScript + Vite
- React Router v6
- Custom CSS design system (no UI framework)

## Quick Start

```bash
cd dashboard
npm install
npm run dev      # → http://localhost:5173
```

### Demo Login
| Username | Password   | Role           |
|----------|------------|----------------|
| admin    | admin123   | Administrator  |
| trainer  | trainer123 | Trainer        |

## Pages

| Route               | Page           | Status       |
|---------------------|----------------|--------------|
| `/`                 | Overview       | Functional   |
| `/workers`          | Workers        | Functional   |
| `/workers/:id`      | Worker Detail  | Functional   |
| `/analytics`        | Analytics      | Functional   |
| `/certificates`     | Certifications | Placeholder  |
| `/settings`         | Settings       | Placeholder  |

## Backend Integration

API client (`src/services/api.ts`) tries the real FastAPI backend first,
falls back to mock data if unreachable. Same 3 endpoints as before:
`/api/v1/dashboard/summary`, `/dashboard/workers`, `/dashboard/workers/{id}`.

```
VITE_API_BASE=http://localhost:8000/api/v1
VITE_USE_MOCK=false
```
