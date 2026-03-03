# Hypesoft Challenge — Inventory Management System

A full-stack e-commerce inventory management application built with **.NET 9** and **Next.js 16**, featuring Clean Architecture, CQRS, MongoDB full-text search, generation-based cache invalidation, and Keycloak authentication.

---

## Features

- **Product management** — full CRUD with server-side pagination, full-text search (MongoDB `$text` index on name + description), category filter, low-stock filter, and column sorting
- **Category management** — full CRUD with live product count per category and column sorting
- **Stock control** — dedicated update-stock modal with domain-enforced limits (0–1,000,000 units); low-stock threshold at < 10 units
- **Dashboard** — total products, total stock value, low-stock list, and a products-by-category doughnut chart (Chart.js)
- **Role-based access control** — `admin` role can create/edit/delete; `user` role is read-only; enforced on both frontend and backend
- **Caching** — Redis-compatible `IDistributedCache` with generation-counter invalidation for paginated product lists
- **Security** — JWT validation (Keycloak), rate limiting, HSTS, CSP, `X-Content-Type-Options`, CORS
- **Observability** — structured logging with Serilog (console + file sinks)

---

## Architecture

```
┌─────────────┐     OIDC/JWT      ┌─────────────┐
│  Next.js 16 │ ◄──────────────── │  Keycloak   │
│  (frontend) │                   │     26      │
└──────┬──────┘                   └─────────────┘
       │ REST/JSON
┌──────▼──────┐     IDistributed  ┌─────────────┐
│  .NET 9 API │ ──────Cache──────►│  In-Memory  │
│  (backend)  │                   │    Cache    │
└──────┬──────┘                   └─────────────┘
       │ EF Core + native driver
┌──────▼──────┐
│   MongoDB   │
└─────────────┘
```

### Backend — Clean Architecture + DDD + CQRS

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `Hypesoft.Domain` | Entities, Value Objects, Domain Events, Repository interfaces |
| Application | `Hypesoft.Application` | Commands, Queries, MediatR Handlers, DTOs, FluentValidation, Interfaces |
| Infrastructure | `Hypesoft.Infrastructure` | EF Core / MongoDB, Repository implementations, Cache, Services |
| API | `Hypesoft.API` | REST Controllers, Middleware, DI wiring |

### Frontend — Next.js App Router

| Directory | Purpose |
|---|---|
| `app/(authenticated)/` | Protected pages (products, categories, dashboard) |
| `components/forms/` | `ProductFormModal`, `CategoryFormModal`, `UpdateStockModal` |
| `components/charts/` | Chart.js wrappers |
| `hooks/` | `useProductMutations`, `useCategoryMutations` |
| `services/` | `productService`, `categoryService`, `dashboardService` |

---

## Quick Start (Docker)

### Prerequisites

- [Docker Desktop 4.0+](https://www.docker.com/products/docker-desktop/)
- Git

### Steps

```bash
# 1. Clone
git clone https://github.com/seu-usuario/hypesoft-challenge.git
cd hypesoft-challenge

# 2. Configure environment variables
cp .env.example .env
# Edit .env and fill in the required secrets (see Environment Variables below)

# 3. Start all services
docker compose up -d

# 4. Wait ~60 s for Keycloak to finish its first-run import, then open:
#    http://localhost          → Application (via Nginx)
#    http://localhost:3000     → Frontend (direct)
#    http://localhost:5000/swagger → API docs
#    http://localhost:8080     → Keycloak Admin Console
#    http://localhost:8081     → MongoDB Express (DB admin)
```

### Pre-configured accounts

| Username | Password | Roles |
|---|---|---|
| `admin` | `admin` | `admin`, `user` |
| `user` | `user` | `user` |

---

## Environment Variables

Copy `.env.example` to `.env` and fill in the values:

```env
# PostgreSQL (Keycloak DB)
POSTGRES_PASSWORD=change-me

# Keycloak admin credentials
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=change-me

# Keycloak DB credentials (must match POSTGRES_PASSWORD)
KC_DB_USERNAME=keycloak
KC_DB_PASSWORD=change-me

# NextAuth secret — generate with: openssl rand -base64 32
NEXTAUTH_SECRET=change-me-min-32-chars

# Keycloak OAuth client secret (from keycloak/realms/hypesoft-realm.json)
KEYCLOAK_SECRET=change-me
```

> The `.env` file is gitignored and must never be committed.

---

## Local Development

### Backend

```bash
cd backend

# Requires MongoDB + Keycloak running (e.g. via docker compose up mongo keycloak -d)
dotnet restore Hypesoft.sln
dotnet run --project src/Hypesoft.API/Hypesoft.API.csproj
```

API available at `http://localhost:5000` — Swagger UI at `http://localhost:5000/swagger`.

### Frontend

```bash
cd frontend
npm install
# Copy and adapt the environment file
cp .env.local.example .env.local   # if available, otherwise set vars manually
npm run dev
```

Frontend available at `http://localhost:3000`.

---

## Running Tests

### Backend — Unit Tests (124 tests)

```bash
cd backend
dotnet test tests/Hypesoft.UnitTests/
```

Covers: Domain entities & value objects, FluentValidation validators, MediatR handlers (mocked dependencies).

### Backend — Integration Tests (requires Docker)

```bash
cd backend
dotnet test tests/Hypesoft.IntegrationTests/
```

Uses Testcontainers to spin up a real MongoDB instance. Tests all API controllers and repositories.

### Frontend — Component Tests (16 tests)

```bash
cd frontend
npm test
```

Uses Vitest + React Testing Library. Covers utility formatters, `StatCard`, and `ThemeProvider`.

### E2E Tests — Playwright (requires running stack)

```bash
# Start the full stack first
docker compose up -d

cd e2e
npm install
npm test
```

Covers: full CRUD flow (category → product → stock update → delete), role-based access control, auth flow.

### Mutation Testing

```bash
cd backend
dotnet stryker
```

Targets `Hypesoft.Domain` and `Hypesoft.Application` layers using the unit test suite.

---

## API Reference

Full interactive docs available at `http://localhost:5000/swagger` when the backend is running.

### Main Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api/products` | User | List products (paginated, filterable) |
| `POST` | `/api/products` | Admin | Create product |
| `PUT` | `/api/products/{id}` | Admin | Update product |
| `DELETE` | `/api/products/{id}` | Admin | Delete product |
| `PATCH` | `/api/products/{id}/stock` | Admin | Update stock quantity |
| `GET` | `/api/categories` | User | List all categories (with product count) |
| `POST` | `/api/categories` | Admin | Create category |
| `PUT` | `/api/categories/{id}` | Admin | Update category |
| `DELETE` | `/api/categories/{id}` | Admin | Delete category |
| `GET` | `/api/dashboard` | User | Dashboard summary |

### Product List Query Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `pageNumber` | int | `1` | Page number |
| `pageSize` | int | `10` | Items per page |
| `searchTerm` | string | — | Full-text search (name + description) |
| `categoryId` | string | — | Filter by category ID |
| `lowStockOnly` | bool | `false` | Return only items with stock < 10 |

---

## Project Structure

```
hypesoft-challenge/
├── backend/
│   ├── src/
│   │   ├── Hypesoft.Domain/          # Entities, Value Objects, Domain Events
│   │   ├── Hypesoft.Application/     # CQRS Handlers, DTOs, Validators
│   │   ├── Hypesoft.Infrastructure/  # EF Core, Repositories, Cache, Keycloak
│   │   └── Hypesoft.API/             # REST Controllers, Middleware
│   └── tests/
│       ├── Hypesoft.UnitTests/       # xUnit + Moq + FluentAssertions
│       └── Hypesoft.IntegrationTests/# Testcontainers + WebApplicationFactory
├── frontend/
│   └── src/
│       ├── app/                      # Next.js App Router pages
│       ├── components/               # UI, forms, charts, layout
│       ├── hooks/                    # Mutation hooks
│       └── services/                 # API client services
├── e2e/                              # Playwright E2E tests
├── keycloak/realms/                  # Pre-configured Keycloak realm
├── nginx/                            # Reverse proxy config
├── docs/                             # Architecture, domain rules, API contracts
├── Dockerfile.backend
├── Dockerfile.frontend
├── docker-compose.yml
└── .env.example
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend framework | .NET 9, ASP.NET Core |
| Architecture | Clean Architecture + DDD + CQRS + MediatR |
| Database | MongoDB (EF Core provider + native driver) |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Caching | `IDistributedCache` (in-memory; swap-in Redis without code changes) |
| Logging | Serilog (console + file) |
| Auth | Keycloak 26 (OAuth2 / OIDC) |
| Frontend framework | Next.js 16 (App Router), React 19, TypeScript |
| UI components | Shadcn/ui + TailwindCSS |
| Data fetching | TanStack Query v5 |
| Forms | React Hook Form + Zod |
| Charts | Chart.js |
| Unit tests | xUnit, Moq, FluentAssertions, Vitest, RTL |
| Integration tests | Testcontainers, WebApplicationFactory |
| E2E tests | Playwright |
| Mutation tests | Stryker.NET |
| Containerisation | Docker, Docker Compose |
| Reverse proxy | Nginx |
