# Hypesoft Challenge — Sistema de Gestão de Inventário

Aplicação full-stack de gestão de inventário para e-commerce construída com **.NET 9** e **Next.js 16**, com Clean Architecture, CQRS, busca full-text no MongoDB, invalidação de cache por geração e autenticação via Keycloak.

---

## Funcionalidades

- **Gestão de produtos** — CRUD completo com paginação server-side, busca full-text (índice `$text` do MongoDB em nome + descrição), filtro por categoria, filtro de estoque baixo e ordenação por colunas
- **Gestão de categorias** — CRUD completo com contagem de produtos por categoria em tempo real e ordenação por colunas
- **Controle de estoque** — modal dedicado com limites impostos pelo domínio (0–1.000.000 unidades); limite de estoque baixo em < 10 unidades
- **Dashboard** — total de produtos, valor total do estoque, lista de estoque baixo e gráfico de produtos por categoria (Chart.js)
- **Controle de acesso por papéis** — papel `admin` pode criar/editar/excluir; papel `user` é somente leitura; aplicado tanto no frontend quanto no backend
- **Cache** — `IDistributedCache` compatível com Redis com invalidação por contador de geração para listas paginadas
- **Segurança** — validação JWT (Keycloak), rate limiting, HSTS, CSP, `X-Content-Type-Options`, CORS
- **Observabilidade** — logging estruturado com Serilog (console + arquivo)

---

## Arquitetura

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
       │ EF Core + driver nativo
┌──────▼──────┐
│   MongoDB   │
└─────────────┘
```

### Backend — Clean Architecture + DDD + CQRS

| Camada | Projeto | Responsabilidade |
|---|---|---|
| Domínio | `Hypesoft.Domain` | Entidades, Value Objects, Domain Events, interfaces de repositório |
| Aplicação | `Hypesoft.Application` | Commands, Queries, Handlers MediatR, DTOs, FluentValidation, Interfaces |
| Infraestrutura | `Hypesoft.Infrastructure` | EF Core / MongoDB, implementação de repositórios, Cache, Serviços |
| API | `Hypesoft.API` | Controllers REST, Middleware, configuração de DI |

### Frontend — Next.js App Router

| Diretório | Função |
|---|---|
| `app/(authenticated)/` | Páginas protegidas (produtos, categorias, dashboard) |
| `components/forms/` | `ProductFormModal`, `CategoryFormModal`, `UpdateStockModal` |
| `components/charts/` | Wrappers Chart.js |
| `hooks/` | `useProductMutations`, `useCategoryMutations` |
| `services/` | `productService`, `categoryService`, `dashboardService` |

---

## Início Rápido (Docker)

### Pré-requisitos

- [Docker Desktop 4.0+](https://www.docker.com/products/docker-desktop/)
- Git

### Passos

```bash
# 1. Clone o repositório
git clone https://github.com/seu-usuario/hypesoft-challenge.git
cd hypesoft-challenge

# 2. Configure as variáveis de ambiente
cp .env.example .env
# Edite o .env e preencha os valores necessários (veja Variáveis de Ambiente abaixo)

# 3. Suba todos os serviços
docker compose up -d

# 4. Aguarde ~60 s para o Keycloak concluir o import inicial e acesse:
#    http://localhost          → Aplicação (via Nginx)
#    http://localhost:3000     → Frontend (direto)
#    http://localhost:5000/swagger → Documentação da API
#    http://localhost:8080     → Keycloak Admin Console
#    http://localhost:8081     → MongoDB Express (admin do DB)
```

### Contas pré-configuradas

| Usuário | Senha | Papéis |
|---|---|---|
| `admin` | `admin` | `admin`, `user` |
| `user` | `user` | `user` |

---

## Variáveis de Ambiente

Copie `.env.example` para `.env` e preencha os valores:

```env
# PostgreSQL (banco do Keycloak)
POSTGRES_PASSWORD=change-me

# Credenciais do admin do Keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=change-me

# Credenciais do banco do Keycloak (deve coincidir com POSTGRES_PASSWORD)
KC_DB_USERNAME=keycloak
KC_DB_PASSWORD=change-me

# Segredo do NextAuth — gere com: openssl rand -base64 32
NEXTAUTH_SECRET=change-me-min-32-chars

# Segredo do cliente OAuth do Keycloak (configurado no realm do Keycloak)
KEYCLOAK_SECRET=change-me
```

> O arquivo `.env` está no `.gitignore` e nunca deve ser commitado.

---

## Desenvolvimento Local

### Backend

```bash
cd backend

# Requer MongoDB + Keycloak em execução (ex.: docker compose up mongo keycloak -d)
dotnet restore Hypesoft.sln
dotnet run --project src/Hypesoft.API/Hypesoft.API.csproj
```

API disponível em `http://localhost:5000` — Swagger UI em `http://localhost:5000/swagger`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend disponível em `http://localhost:3000`.

---

## Executando os Testes

### Backend — Testes Unitários (124 testes)

```bash
cd backend
dotnet test tests/Hypesoft.UnitTests/
```

Cobre: entidades e value objects do domínio, validadores FluentValidation, handlers MediatR (dependências mockadas).

### Backend — Testes de Integração (requer Docker)

```bash
cd backend
dotnet test tests/Hypesoft.IntegrationTests/
```

Usa Testcontainers para subir uma instância real do MongoDB. Testa todos os controllers e repositórios da API.

### Frontend — Testes de Componentes (16 testes)

```bash
cd frontend
npm test
```

Usa Vitest + React Testing Library. Cobre formatadores utilitários, `StatCard` e `ThemeProvider`.

### Testes E2E — Playwright (requer stack em execução)

```bash
# Suba o stack completo primeiro
docker compose up -d

cd e2e
npm install
npm test
```

Cobre: fluxo CRUD completo (categoria → produto → atualização de estoque → exclusão), controle de acesso por papéis, fluxo de autenticação.

### Testes de Mutação

```bash
cd backend
dotnet stryker
```

Alvo nas camadas `Hypesoft.Domain` e `Hypesoft.Application` usando a suíte de testes unitários.

---

## Referência da API

Documentação interativa completa disponível em `http://localhost:5000/swagger` com o backend em execução.

### Endpoints Principais

| Método | Caminho | Auth | Descrição |
|---|---|---|---|
| `GET` | `/api/products` | User | Lista produtos (paginado, filtrável) |
| `POST` | `/api/products` | Admin | Cria produto |
| `PUT` | `/api/products/{id}` | Admin | Atualiza produto |
| `DELETE` | `/api/products/{id}` | Admin | Remove produto |
| `PATCH` | `/api/products/{id}/stock` | Admin | Atualiza quantidade em estoque |
| `GET` | `/api/categories` | User | Lista todas as categorias (com contagem de produtos) |
| `POST` | `/api/categories` | Admin | Cria categoria |
| `PUT` | `/api/categories/{id}` | Admin | Atualiza categoria |
| `DELETE` | `/api/categories/{id}` | Admin | Remove categoria |
| `GET` | `/api/dashboard` | User | Resumo do dashboard |

### Parâmetros de Query — Listagem de Produtos

| Parâmetro | Tipo | Padrão | Descrição |
|---|---|---|---|
| `pageNumber` | int | `1` | Número da página |
| `pageSize` | int | `10` | Itens por página |
| `searchTerm` | string | — | Busca full-text (nome + descrição) |
| `categoryId` | string | — | Filtro por ID de categoria |
| `lowStockOnly` | bool | `false` | Retorna apenas itens com estoque < 10 |

---

## Estrutura do Projeto

```
hypesoft-challenge/
├── backend/
│   ├── src/
│   │   ├── Hypesoft.Domain/          # Entidades, Value Objects, Domain Events
│   │   ├── Hypesoft.Application/     # Handlers CQRS, DTOs, Validadores
│   │   ├── Hypesoft.Infrastructure/  # EF Core, Repositórios, Cache, Keycloak
│   │   └── Hypesoft.API/             # Controllers REST, Middleware
│   └── tests/
│       ├── Hypesoft.UnitTests/       # xUnit + Moq + FluentAssertions
│       └── Hypesoft.IntegrationTests/# Testcontainers + WebApplicationFactory
├── frontend/
│   └── src/
│       ├── app/                      # Páginas Next.js App Router
│       ├── components/               # UI, formulários, gráficos, layout
│       ├── hooks/                    # Hooks de mutação
│       └── services/                 # Serviços cliente da API
├── e2e/                              # Testes E2E Playwright
├── keycloak/realms/                  # Realm Keycloak pré-configurado
├── nginx/                            # Configuração do reverse proxy
├── docs/                             # Arquitetura, regras de domínio, contratos da API
├── Dockerfile.backend
├── Dockerfile.frontend
├── docker-compose.yml
└── .env.example
```

---

## Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| Framework backend | .NET 9, ASP.NET Core |
| Arquitetura | Clean Architecture + DDD + CQRS + MediatR |
| Banco de dados | MongoDB (EF Core provider + driver nativo) |
| Validação | FluentValidation |
| Mapeamento | AutoMapper |
| Cache | `IDistributedCache` (in-memory; substituível por Redis sem mudanças no código) |
| Logging | Serilog (console + arquivo) |
| Autenticação | Keycloak 26 (OAuth2 / OIDC) |
| Framework frontend | Next.js 16 (App Router), React 19, TypeScript |
| Componentes UI | Shadcn/ui + TailwindCSS |
| Data fetching | TanStack Query v5 |
| Formulários | React Hook Form + Zod |
| Gráficos | Chart.js |
| Testes unitários | xUnit, Moq, FluentAssertions, Vitest, RTL |
| Testes de integração | Testcontainers, WebApplicationFactory |
| Testes E2E | Playwright |
| Testes de mutação | Stryker.NET |
| Containerização | Docker, Docker Compose |
| Reverse proxy | Nginx |

---

---

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
