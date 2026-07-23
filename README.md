# 🏗️ EnterpriseKit V2 — .NET 10 Enterprise Starter Kit

[![CI](https://github.com/qmmughal/enterprise-starter-kit-v2/actions/workflows/ci.yml/badge.svg)](https://github.com/qmmughal/enterprise-starter-kit-v2/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Preferred starter kit** for new projects. Looking for .NET 8 LTS? Use [enterprise-starter-kit](https://github.com/qmmughal/enterprise-starter-kit).

A **production-grade, open-source Enterprise Starter Kit** targeting **.NET 10** combining:
- 🧱 **Clean Architecture** — strict separation of concerns across 4 layers
- ⚡ **CQRS + MediatR** — commands and queries with a full pipeline
- 📬 **Transactional Outbox Pattern** — safe domain event delivery
- 🔐 **ABP Framework** — multi-tenancy, identity, OpenIddict OIDC
- 📊 **Serilog** — structured JSON logging
- 🐘 **PostgreSQL** + **Redis** + **MailHog** via Docker Compose

---

## 📐 Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│  HttpApi  (Controllers, Middleware, Program.cs)                      │
│    ↓ sends IRequest via ISender                                      │
├─────────────────────────────────────────────────────────────────────┤
│  Application  (Commands, Queries, Validators, Event Handlers)        │
│    ↓ calls repository interfaces, raises domain events               │
├─────────────────────────────────────────────────────────────────────┤
│  Domain  (Aggregates, Value Objects, Events, Repository Interfaces)  │
│    ← ZERO external dependencies                                       │
├─────────────────────────────────────────────────────────────────────┤
│  Infrastructure  (EF Core, Repositories, Outbox Relay, Redis)        │
└─────────────────────────────────────────────────────────────────────┘
```

### MediatR Pipeline (per request)

```
Request → LoggingBehaviour → ValidationBehaviour → TransactionBehaviour → Handler
```

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Clone & spin up infrastructure

```bash
git clone https://github.com/qmmughal/enterprise-starter-kit-v2.git
cd enterprise-starter-kit-v2

docker compose up -d
```

This starts:
| Service | Port | Purpose |
|---|---|---|
| PostgreSQL 16 | `5432` | Primary database |
| Redis 7 | `6379` | Distributed cache |
| MailHog | `1025` / `8025` | SMTP mock + Web UI |

### 2. Apply database migrations

```bash
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/EnterpriseKit.Infrastructure \
  --startup-project src/HttpApi/EnterpriseKit.HttpApi

dotnet ef database update \
  --startup-project src/HttpApi/EnterpriseKit.HttpApi
```

### 3. Run the API

```bash
dotnet run --project src/HttpApi/EnterpriseKit.HttpApi
```

Swagger UI: **http://localhost:5000** (served at root in development)
MailHog UI: **http://localhost:8025**

---

## 🧪 Running Tests

```bash
# All tests
dotnet test

# Domain unit tests only
dotnet test tests/Domain.UnitTests

# Application unit tests only
dotnet test tests/Application.UnitTests
```

---

## 🗂️ Project Structure

```
EnterpriseKit/
├── src/
│   ├── Domain/EnterpriseKit.Domain/         ← ZERO deps, pure C#
│   │   ├── Common/                          ← Entity, AggregateRoot, ValueObject
│   │   ├── Exceptions/                      ← DomainException, NotFoundException
│   │   ├── Interfaces/Repositories/         ← Repository contracts
│   │   └── Orders/                          ← Order aggregate + Money VO + Events
│   │
│   ├── Application/EnterpriseKit.Application/
│   │   ├── Common/Behaviours/               ← Logging, Validation, Transaction
│   │   ├── Common/Mappings/                 ← AutoMapper profiles
│   │   └── Orders/
│   │       ├── Commands/                    ← PlaceOrder, CancelOrder
│   │       ├── Queries/                     ← GetOrderById, GetOrders (paged)
│   │       └── EventHandlers/               ← OrderPlacedEventHandler
│   │
│   ├── Infrastructure/EnterpriseKit.Infrastructure/
│   │   ├── Persistence/                     ← ApplicationDbContext, EF configs
│   │   ├── Outbox/                          ← OutboxMessage, OutboxRelayService
│   │   └── DependencyInjection/             ← InfrastructureServiceExtensions
│   │
│   └── HttpApi/EnterpriseKit.HttpApi/
│       ├── Controllers/                     ← OrdersController
│       ├── Middleware/                      ← GlobalExceptionMiddleware (RFC 7807)
│       ├── Program.cs
│       └── Dockerfile
│
├── tests/
│   ├── Domain.UnitTests/
│   └── Application.UnitTests/
│
├── infra/postgres/init.sql
├── docker-compose.yml
└── docker-compose.override.yml
```

---

## 📬 Transactional Outbox Pattern

Domain events are **never published inline**. The flow is:

```
Command Handler
  → mutates aggregate (raises domain event)
  → SaveChangesAsync()
       → OutboxExtensions.DispatchDomainEventsToOutbox()
            → serializes events → INSERT into outbox_messages
            → all inside ONE transaction ✓

OutboxRelayService (BackgroundService, polls every 5s)
  → reads unprocessed outbox rows
  → deserializes → IPublisher.Publish()
  → marks row as processed
```

This guarantees **at-least-once delivery**. Event handlers must be idempotent.

---

## 🔐 Authentication

The API uses **JWT Bearer** authentication. Configure your identity provider in `appsettings.json`:

```json
{
  "Auth": {
    "Authority": "https://your-identity-server",
    "Audience": "enterprise-kit-api"
  }
}
```

For local development without an identity server, you can disable auth by removing `[Authorize]` from controllers or using a local JWT tool.

---

## 🛠️ Adding a New Feature

Follow this checklist for any new bounded-context feature:

```
1. Domain:       Add Entity/AggregateRoot + Value Objects + Domain Events
2. Domain:       Add IRepository interface
3. Application:  Add Command/Query records
4. Application:  Add FluentValidation validator
5. Application:  Add IRequestHandler<> handler
6. Application:  Add INotificationHandler<> for domain events
7. Infrastructure: Add EF IEntityTypeConfiguration<>
8. Infrastructure: Add concrete Repository implementation
9. Infrastructure: Register in InfrastructureServiceExtensions
10. HttpApi:     Add Controller action → mediator.Send(command)
11. Tests:       Domain unit tests, validator tests, handler tests
```

---

## 📄 License

MIT — see [LICENSE](LICENSE).
