# SurakshaAR

SurakshaAR is an end-to-end augmented reality (AR) platform for worker safety training and competence
assessment. It combines an AR worker application (Unity) with machine-learning-driven competency scoring
and vision detection, a backend API, an administrative dashboard, secure certificate issuance, and a
structured security framework.

> **Status:** This repository is currently **scaffolding only**. The directory structure, essential README
> files, and documentation placeholders are in place. No application logic has been implemented yet.

## Repository Layout

```
SurakshaAR/
│
├── worker-app/    # Unity AR application (training, assessment, fire & gas scenarios)
├── ml/            # Machine learning: competency scoring & computer vision
├── backend/       # Backend API services
├── dashboard/     # Web dashboard for administrators / trainers
├── security/      # Security framework: threat model, auth, authorization, data protection
├── certificate/   # Training certificate generation, verification, QR codes
├── assets/        # Shared non-code assets (3D, icons, textures, audio, reference)
└── docs/          # Requirements, architecture, safety procedures, API, testing, demo
```

## Modules

| Module | Path | Purpose | README |
|---|---|---|---|
| Worker AR App | `worker-app/` | Unity-based AR training & assessment for fire and gas safety | [worker-app/README.md](worker-app/README.md) |
| ML – Competency | `ml/competency/` | Scoring worker performance and detecting weaknesses | [ml/competency/README.md](ml/competency/README.md) |
| ML – Vision | `ml/vision/` | Computer-vision models & inference for scenario detection | [ml/vision/README.md](ml/vision/README.md) |
| Backend | `backend/` | API, models, services, auth, database | [backend/README.md](backend/README.md) |
| Dashboard | `dashboard/` | Web dashboard for monitoring and management | [dashboard/README.md](dashboard/README.md) |
| Security | `security/` | Threat model, authentication, authorization, API security, data protection | [security/README.md](security/README.md) |
| Certification | `certificate/` | Certificate generation, verification, and QR codes | [certificate/README.md](certificate/README.md) |
| Shared Assets | `assets/` | Cross-project non-code assets | — |

## Documentation

- **Requirements:** [problem-statement.md](docs/requirements/problem-statement.md)
- **Architecture:** [system-architecture.md](docs/architecture/system-architecture.md) ·
  [data-flow.md](docs/architecture/data-flow.md) · [integration.md](docs/architecture/integration.md)
- **Safety Procedures:** [fire-procedure.md](docs/safety/fire-procedure.md) ·
  [gas-procedure.md](docs/safety/gas-procedure.md)
- **API:** [API.md](docs/api/API.md)
- **Testing:** [test-plan.md](docs/testing/test-plan.md) · [test-cases.md](docs/testing/test-cases.md)
- **Demo:** [demo-flow.md](docs/demo/demo-flow.md) · [presentation.md](docs/demo/presentation.md)

## Getting Started

Module-specific setup instructions will be provided in each module README once implementation begins.
No dependencies have been installed yet.

## License

This project is licensed under the [MIT License](LICENSE).