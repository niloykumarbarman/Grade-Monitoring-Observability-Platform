# Grade Monitoring & Observability Platform

A microservices-based platform built with **.NET 9** (backend) and **React + TypeScript + Vite** (frontend), demonstrating Clean Architecture, event-driven communication via **RabbitMQ**, containerized deployment with **Docker/Kubernetes/Helm**, and a full observability stack (**Prometheus, Grafana, Loki, Tempo, OpenTelemetry**).

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Services](#services)
- [Event-Driven Communication](#event-driven-communication)
- [Getting Started](#getting-started)
- [Building the Project](#building-the-project)
- [Observability](#observability)
- [CI/CD](#cicd)
- [Roadmap](#roadmap)

---

## Architecture Overview

The platform follows a **Clean Architecture** pattern per microservice (Domain → Application → Infrastructure → API), with cross-cutting concerns (event bus, common utilities, observability) extracted into shared `BuildingBlocks` libraries.
┌──────────────────┐
            │   React Frontend  │
            └─────────┬─────────┘
                      │ REST / HTTPS
                      ▼
    ┌──────────────────────────────────┐
    │            API Layer             │
    │  OrderService.API  GradeService.API │
    └───────────┬──────────┬───────────┘
                │          │
     ┌──────────▼──┐   ┌───▼──────────┐
     │ OrderService │   │ GradeService │
     │ (Clean Arch) │   │ (Clean Arch) │
     └──────┬───────┘   └──────┬───────┘
            │                  │
            │   RabbitMQ EventBus  │
            └────────►◄────────┘
             OrderConfirmedIntegrationEvent

---

## Project Structure
GradeMonitoringObservabilityPlatform22/

│

├── src/

│   ├── frontend/                       # React + TypeScript + Vite SPA

│   │   └── src/

│   │       ├── app/                    # App entry, routing

│   │       ├── assets/                 # Global styles (index.css — Tailwind directives)

│   │       ├── components/, features/, hooks/, services/, store/, types/, utils/

│   │

│   └── services/

│       ├── BuildingBlocks/

│       │   ├── Common/                 # Shared domain primitives, exceptions, pagination

│       │   ├── EventBus/               # RabbitMQ-based event bus abstraction

│       │   │   ├── Abstractions/       # IEventBus, IIntegrationEventHandler

│       │   │   ├── Events/             # IntegrationEvent base class

│       │   │   ├── IntegrationEvents/  # Shared cross-service events

│       │   │   └── RabbitMQ/           # RabbitMQEventBus implementation

│       │   └── Observability/          # Logging, Metrics, Tracing extensions

│       │

│       ├── OrderService/               # Microservice #1

│       │   ├── OrderService.Domain

│       │   ├── OrderService.Application

│       │   ├── OrderService.Infrastructure

│       │   ├── OrderService.API         # Controllers/OrdersController.cs — publishes OrderConfirmedIntegrationEvent

│       │   └── tests/ (Unit, Integration, Functional)

│       │

│       └── GradeService/               # Microservice #2

│           ├── src/

│           │   ├── GradeService.Domain

│           │   ├── GradeService.Application

│           │   ├── GradeService.Infrastructure

│           │   │   └── EventHandlers/  # OrderConfirmedEventHandler — persists grade via IGradeRepository

│           │   └── GradeService.API

│           └── tests/ (Unit, Integration, Functional)

│

├── deploy/

│   ├── docker/                         # Dockerfiles + prod compose

│   ├── k8s/                            # Namespaces, deployments, services, ingress, hpa

│   ├── helm/enterprise-platform/       # Helm chart (dev/prod values)

│   └── monitoring/

│       ├── prometheus/, grafana/, loki/, tempo/, otel-collector/, alertmanager/

│

├── .github/workflows/

│   ├── frontend-ci.yml                 # Build frontend on push/PR

│   ├── backend-ci.yml                  # Restore/build/test .NET solution

│   └── deploy.yml                      # Build & push images, deploy to k8s

│

└── docs/

├── architecture/, api/, runbooks/


---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 9, ASP.NET Core Web API, Clean Architecture |
| Messaging | RabbitMQ.Client 7.x (async API) |
| Frontend | React 18.2, TypeScript 5.2, Vite 5.1, TanStack Query 5.17, Zustand 4.5, Tailwind CSS 3.4 |
| Frontend Icons / Charts | lucide-react 0.323, Recharts 2.10 |
| Frontend Utilities | axios, clsx, tailwind-merge, react-router-dom |
| Observability | Prometheus, Grafana, Loki, Tempo, OpenTelemetry Collector, Alertmanager |
| Containerization | Docker, Docker Compose |
| Orchestration | Kubernetes, Helm |
| CI/CD | GitHub Actions |
| Testing | xUnit (Unit / Integration / Functional) |

### Frontend Design Notes

- Styling is **utility-first Tailwind CSS** — no separate component CSS files; layout/spacing/color all done via class names.
- The global stylesheet (`src/assets/index.css`) only declares the three `@tailwind` directives plus a system font stack (`-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, sans-serif`) and a light gray app background (`#f9fafb`). No custom font is loaded — this is intentional for fast load times, but see [Roadmap](#roadmap) for a suggested upgrade.
- `tailwind.config.js` currently uses the **default theme** (no custom colors, spacing, or font-family extensions yet).
- Icons are from `lucide-react`; charts (bar chart, pie chart) are from `Recharts`.
- Dashboard and Grades pages use gradient stat cards, colored grade badges (A+/A/B/C), and circular score indicators — built entirely with Tailwind utility classes.


---

## Services

### OrderService

Handles order creation and confirmation. On order confirmation, it publishes an `OrderConfirmedIntegrationEvent` to notify other services (`OrderService.API/Controllers/OrdersController.cs`).

- `OrderService.Domain` — Entities, enums
- `OrderService.Application` — Use cases (MediatR + FluentValidation + AutoMapper)
- `OrderService.Infrastructure` — EF Core persistence, repositories, identity
- `OrderService.API` — REST endpoints, publishes `OrderConfirmedIntegrationEvent` on order confirmation

### GradeService

Listens for order confirmation events and creates or updates a student's grade accordingly, demonstrating asynchronous, event-driven inter-service communication.

- `GradeService.Domain` — Grade entities
- `GradeService.Application` — Grade-related use cases
- `GradeService.Infrastructure` — Persistence + **`OrderConfirmedEventHandler`**, which upserts the grade (creates if new, updates score if it already exists) via `IGradeRepository` and commits with `SaveChangesAsync()`
- `GradeService.API` — REST endpoints + RabbitMQ subscription bootstrap

---

## Event-Driven Communication

A shared `BuildingBlocks.EventBus` library wraps RabbitMQ behind a clean abstraction:

```csharp
public interface IEventBus
{
    Task PublishAsync(IntegrationEvent eventData);
    void Subscribe<TIntegrationEvent, TIntegrationEventHandler>()
        where TIntegrationEvent : IntegrationEvent
        where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;
    void Unsubscribe<TIntegrationEvent, TIntegrationEventHandler>()
        where TIntegrationEvent : IntegrationEvent
        where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;
}
```

**Flow:**

1. `OrderService` confirms an order and publishes `OrderConfirmedIntegrationEvent { OrderId, StudentId, CourseCode, Score }` via `_eventBus.PublishAsync(...)` in `OrdersController`.
2. RabbitMQ routes the message via a direct exchange (`enterprise_event_bus`) to the `OrderConfirmedIntegrationEvent` queue.
3. `GradeService.API` subscribes to this event on startup:
```csharp
   eventBus.Subscribe<OrderConfirmedIntegrationEvent, OrderConfirmedEventHandler>();
```
4. `OrderConfirmedEventHandler` (in `GradeService.Infrastructure`) consumes the event, checks for an existing grade record for that student/course, and either creates a new `Grade` or updates the existing score — then persists via `SaveChangesAsync()`.

> **Status:** Event contract, publish call, subscriber, and the full create-or-update persistence logic are all implemented and build cleanly end-to-end. What remains is **runtime verification** — running both services together against a live RabbitMQ instance (e.g. via `docker-compose up`) to confirm the message actually round-trips in practice. See [Roadmap](#roadmap).

---

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- Docker & Docker Compose
- RabbitMQ (via `docker-compose` or local install)

### Backend

```bash
cd src/services/OrderService
dotnet restore OrderService.sln
dotnet build OrderService.sln

cd ../GradeService
dotnet restore GradeService.sln
dotnet build GradeService.sln
```

### Frontend

```bash
cd src/frontend
npm install
npm run dev
```

### Full stack (Docker Compose)

```bash
docker-compose up --build
```


---

## Building the Project

Both solutions build cleanly with zero errors:

```bash
# OrderService (7 projects: Domain, Application, Infrastructure, API, 3x Tests)
cd src/services/OrderService && dotnet build OrderService.sln

# GradeService (7 projects: Domain, Application, Infrastructure, API, 3x Tests)
cd src/services/GradeService && dotnet build GradeService.sln
```

Frontend build (TypeScript check + Vite production build, with manual chunk-splitting for vendor libraries — react, react-query, recharts, zustand):

```bash
cd src/frontend && npm run build
```

**Verified status (last checked):**

| Check | Result |
|---|---|
| `OrderService.sln` build | ✅ 0 errors (4 warnings — `AutoMapper` advisory, package-level only) |
| `GradeService.sln` build | ✅ 0 errors, 0 warnings |
| OrderService test suite | ✅ 5/5 passing (Unit, Integration, Functional) |
| Frontend `eslint` (`--max-warnings 0`) | ✅ Passing |
| Frontend `tsc` + `vite build` | ✅ Passing |

---

## Observability

| Tool | Purpose |
|---|---|
| Prometheus | Metrics scraping & alerting rules |
| Grafana | Dashboards (`deploy/monitoring/grafana/dashboards/api-overview.json` — request rate, error rate, p95 latency, active pods) |
| Loki | Log aggregation |
| Tempo | Distributed tracing |
| OpenTelemetry Collector | Unified telemetry pipeline |
| Alertmanager | Alert routing |

---

## CI/CD

GitHub Actions workflows under `.github/workflows/`:

- **`frontend-ci.yml`** — installs deps and builds the React app on changes to `src/frontend/**`.
- **`backend-ci.yml`** — restores, builds (Release), and tests the OrderService solution on changes to `src/services/**`.
- **`deploy.yml`** — builds & pushes Docker images to GHCR, then deploys to Kubernetes (requires `KUBE_CONFIG` secret) and verifies rollout status.

---

## Roadmap

- [x] Wire `OrderService` to publish `OrderConfirmedIntegrationEvent` on order confirmation.
- [x] Persist actual grade updates in `GradeService` (create-or-update via `IGradeRepository`).
- [ ] Run a full `docker-compose up` pass to verify the RabbitMQ publish → consume flow end-to-end at runtime (code is complete; live verification is the remaining gap).
- [ ] Run the `GradeService` test suite (`dotnet test GradeService.sln`) — only `OrderService` has been verified so far.
- [ ] Add an API Gateway (YARP/Ocelot) as a single entry point for the frontend.
- [ ] Add JWT-based authentication service.
- [ ] Populate Grafana dashboards with live metrics via a local `docker-compose up` run.
- [ ] Expand test coverage (handler-level unit tests, integration tests for the event flow).
- [ ] **UI polish** — see suggestions below for moving from "clean utility UI" to a more premium look.

### Suggested UI/Design Upgrades

The current UI (Dashboard + Grade Records pages) is clean and functional — gradient stat cards, colored grade badges, bar/pie charts. To push it toward a more premium feel without a redesign:

- **Custom typeface**: swap the system font stack for a distinct sans-serif (e.g. `Inter`, `Geist`, or `Plus Jakarta Sans`) via a Google Fonts link in `index.html`, then extend `tailwind.config.js` → `theme.extend.fontFamily`.
- **Tailwind theme tokens**: define a custom color palette and spacing scale in `tailwind.config.js` instead of relying on Tailwind defaults, so the brand colors (purple/green/pink/orange already used) become named tokens (`primary`, `success`, `warning`, `danger`) rather than ad-hoc utility classes.
- **Micro-interactions**: subtle hover/transition states on stat cards and table rows (`transition-all`, `hover:shadow-lg`, `hover:-translate-y-0.5`).
- **Skeleton loading states** for the Dashboard/Grades data fetch instead of a blank flash.
- **Dark mode** via Tailwind's `dark:` variant, toggled and persisted in the Zustand store.
- **Empty/error states** for the Grades table and charts when there's no data or the API call fails.

---

## License

This project is for educational/portfolio purposes.
