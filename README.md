
## Project Directory Structure

```text
enterprise-platform/
│
├── README.md
├── docker-compose.yml
├── docker-compose.override.yml
├── .gitignore
├── .editorconfig
├── Directory.Build.props
│
├── src/
│   │
│   ├── frontend/                          # React Frontend
│   │   ├── public/
│   │   ├── src/
│   │   │   ├── app/                        # App setup, routing, store
│   │   │   ├── components/                 # Reusable UI components
│   │   │   ├── features/                   # Feature-based modules
│   │   │   ├── hooks/                      # Custom React hooks
│   │   │   ├── services/                   # API clients (axios/fetch)
│   │   │   ├── store/                      # Redux/Zustand state
│   │   │   ├── types/                      # TypeScript types
│   │   │   ├── utils/                      # Helpers
│   │   │   └── assets/                     # Images, styles
│   │   ├── Dockerfile
│   │   ├── nginx.conf
│   │   ├── package.json
│   │   ├── tsconfig.json
│   │   └── vite.config.ts
│   │
│   └── services/                           # .NET 9 Microservices
│       │
│       ├── BuildingBlocks/                  # Shared cross-cutting libraries
│       │   ├── Common/
│       │   ├── EventBus/                     # Message broker abstractions
│       │   └── Observability/                # OpenTelemetry, logging
│       │
│       └── OrderService/                     # Example microservice (repeat per service)
│           ├── src/
│           │   ├── OrderService.Domain/           # ← Domain Layer
│           │   ├── OrderService.Application/       # ← Application Layer
│           │   ├── OrderService.Infrastructure/    # ← Infrastructure Layer
│           │   └── OrderService.API/               # ← Presentation Layer
│           ├── tests/
│           │   ├── OrderService.UnitTests/
│           │   ├── OrderService.IntegrationTests/
│           │   └── OrderService.FunctionalTests/
│           └── Dockerfile
│
├── deploy/
│   │
│   ├── docker/
│   │   ├── Dockerfile.api
│   │   ├── Dockerfile.frontend
│   │   └── docker-compose.prod.yml
│   │
│   ├── k8s/                                 # Kubernetes manifests
│   │   ├── namespaces/
│   │   ├── deployments/
│   │   ├── services/
│   │   ├── ingress/
│   │   ├── configmaps/
│   │   ├── secrets/
│   │   └── hpa/                               # Horizontal Pod Autoscalers
│   │
│   ├── helm/                               # Helm charts
│   │   └── enterprise-platform/
│   │       ├── Chart.yaml
│   │       ├── values.yaml
│   │       ├── values-dev.yaml
│   │       ├── values-prod.yaml
│   │       └── templates/
│   │           ├── deployment.yaml
│   │           ├── service.yaml
│   │           ├── ingress.yaml
│   │           ├── configmap.yaml
│   │           └── _helpers.tpl
│   │
│   └── monitoring/                         # Observability stack configs
│       ├── prometheus/
│       │   ├── prometheus.yml
│       │   └── alert.rules.yml
│       ├── grafana/
│       │   ├── dashboards/
│       │   └── datasources/
│       ├── loki/
│       │   └── loki-config.yml
│       ├── tempo/
│       │   └── tempo-config.yml
│       ├── otel-collector/
│       │   └── otel-collector-config.yml
│       └── alertmanager/
│           └── alertmanager.yml
│
├── .github/
│   └── workflows/                          # CI/CD pipelines
│       ├── frontend-ci.yml
│       ├── backend-ci.yml
│       └── deploy.yml
│
└── docs/
    ├── architecture/
    ├── api/
    └── runbooks/


## 🏗️ Clean Architecture Layers (.NET 9 Backend)

Clean Architecture organizes code into concentric layers where **dependencies point inward** — outer layers depend on inner layers, never the reverse. This is enforced via the **Dependency Inversion Principle**.

```text
       ┌─────────────────────────────────────┐
       │          API / Presentation         │ ──► Controllers, Minimal APIs
       │   ┌─────────────────────────────┐   │
       │   │        Infrastructure       │   │ ──► EF Core, External Services
       │   │   ┌─────────────────────┐   │   │
       │   │   │     Application     │   │   │ ──► Use Cases, CQRS
       │   │   │   ┌─────────────┐   │   │   │
       │   │   │   │   Domain    │   │   │   │ ──► Entities, Business Rules
       │   │   │   └─────────────┘   │   │   │
       │   │   └─────────────────────┘   │   │
       │   └─────────────────────────────┘   │
       └─────────────────────────────────────┘
                 Dependencies flow INWARD ──►

1. 🟢 Domain Layer (OrderService.Domain) — The Core
The innermost layer. Has zero dependencies on other layers or frameworks.

Entities — Core business objects (Order, OrderItem)

Value Objects — Immutable concepts (Money, Address)

Domain Events — Things that happened (OrderPlacedEvent)

Aggregates — Consistency boundaries

Domain Exceptions & Enums

Repository Interfaces — Contracts only, no implementation



2. 🔵 Application Layer (OrderService.Application)
Orchestrates the business use cases. Depends only on Domain.

Use Cases / Handlers — CQRS with MediatR (CreateOrderCommand, GetOrderQuery)

DTOs — Data transfer objects

Interfaces — Abstractions for infrastructure (IEmailService, IPaymentGateway)

Validation — FluentValidation rules

Behaviors / Pipelines — Logging, validation, transactions

Mapping — AutoMapper profiles

3. 🟠 Infrastructure Layer (OrderService.Infrastructure)
Implements the interfaces defined in inner layers. Depends on Application & Domain.

Persistence — EF Core DbContext, migrations, repository implementations

External Services — Email, payment gateways, message brokers

Caching — Redis

Identity — Authentication/authorization providers

Observability — OpenTelemetry instrumentation

4. 🔴 API / Presentation Layer (OrderService.API)
The entry point. Depends on all inner layers (typically via Application + Infrastructure DI).

Controllers / Minimal APIs — HTTP endpoints

Middleware — Exception handling, correlation IDs

Dependency Injection — Wiring everything together (Program.cs)

Filters, API Versioning, Swagger/OpenAPI

Health Checks

🔑 Key Principle: The Dependency Rule
API ──► Infrastructure ──► Application ──► Domain
          (All arrows point toward Domain)
The Domain knows nothing about databases, HTTP, or frameworks. This makes the core business logic testable, framework-independent, and durable even if you swap EF Core for Dapper or REST for gRPC.

## 🗺️ Step 2A: System Design Diagram (Mermaid.js)

```mermaid
graph TD
    User([User / Client]) -->|HTTPS| Ingress[Ingress LB]
    
    subgraph K8s [Kubernetes Cluster]
        Ingress -->|Route| APIGateway[API Gateway<br>Auth, Rate Limit]
        
        subgraph Front [Frontend]
            ReactApp[React Frontend App]
        end
        
        subgraph Backend [.NET 9 Microservices]
            APIGateway -->|HTTP / gRPC| OrderService[Order Service]
            APIGateway -->|HTTP / gRPC| IdentityService[Identity Service]
            APIGateway -->|HTTP / gRPC| AuditService[Audit Service]
        end

        subgraph Storage [Databases & Cache]
            OrderService --> DB1[(PostgreSQL)]
            IdentityService --> DB1
            OrderService --> Cache1[(Redis)]
        end

        subgraph Observability [Monitoring Stack]
            OrderService -.->|Traces, Metrics, Logs| Otel[OpenTelemetry Collector Sidecar]
            IdentityService -.->|Traces, Metrics, Logs| Otel
            AuditService -.->|Traces, Metrics, Logs| Otel
            
            Otel --> Prometheus[(Prometheus)]
            Otel --> Loki[(Loki)]
            Otel --> Tempo[(Tempo)]
            
            Grafana[Grafana Dashboard] --> Prometheus
            Grafana --> Loki
            Grafana --> Tempo
        end
    end

    User -->|Access| ReactApp

🔄 Traffic Flow Summary
React ──► Ingress LB ──► API Gateway (auth, rate limit) ──► .NET 9 Services ──► PostgreSQL / Redis, with every service exporting traces, metrics, and logs via an OpenTelemetry Collector sidecar to Prometheus / Loki / Tempo, visualized and alerted on in Grafana.

### 📊 Database Schema Highlights

| Feature | Purpose |
| :--- | :--- |
| **UUID PKs** | Distributed-friendly, non-guessable identifiers |
| **JSONB columns** | Flexible permissions, alert labels, audit diffs (+ GIN index) |
| **ENUM types** | Enforced severity/status/action values |
| **`correlation_id`** | Links audit logs to OpenTelemetry traces |
| **Soft lockout** (`failed_attempts`, `locked_until`) | Brute-force protection |
| **`updated_at` triggers** | Automatic timestamp maintenance |
| **Append-only `audit_logs`** (`BIGSERIAL`) | High-volume immutable audit trail |

✅ **Step 2 Complete.** The Mermaid diagram will render directly in any Markdown viewer that supports Mermaid (GitHub, Notion, Obsidian, VS Code, etc.).

### 📁 Step 3: Infrastructure Layer Structure (`OrderService.Infrastructure`)

```text
OrderService.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── AlertConfiguration.cs
│   │   └── AuditLogConfiguration.cs
│   └── Interceptors/
│       └── AuditableEntityInterceptor.cs
├── Repositories/
│   ├── Repository.cs (generic)
│   ├── UnitOfWork.cs
│   ├── UserRepository.cs
│   └── AlertRepository.cs
├── Identity/
│   ├── JwtTokenService.cs
│   └── CurrentUserService.cs
├── Authentication/
│   └── JwtBearerConfiguration.cs
└── DependencyInjection.cs

### ⚙️ App Settings Configuration (`OrderService.API`)

| Key | Section | Description / Default |
| :--- | :--- | :--- |
| **`DefaultConnection`** | `ConnectionStrings` | PostgreSQL connection string pointing to local/Docker container |
| **`SecretKey`** | `JwtSettings` | 256-bit signing key for validating client bearer tokens |
| **`Issuer` / `Audience`** | `JwtSettings` | Identity claims tracking token origin and target clients |
| **`LogLevel:Microsoft.EFCore`** | `Logging` | Verbose SQL query logging level outputted directly to stdout |

✅ **Step 6 Complete.** Configuration files and identity middleware parameters are fully documented.
