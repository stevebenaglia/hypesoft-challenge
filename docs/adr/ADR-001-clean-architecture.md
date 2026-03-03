# ADR-001: Clean Architecture + Domain-Driven Design

- **Status**: Aceito / Accepted
- **Data / Date**: 2025-01-15

---

## Contexto

O sistema precisa de uma separação clara entre regras de negócio e preocupações de infraestrutura para permitir a evolução independente de cada camada, facilitar os testes e garantir que a lógica de domínio não seja contaminada por detalhes de framework ou banco de dados. Múltiplos membros da equipe trabalharão em camadas distintas simultaneamente.

## Decisão

Adotar **Clean Architecture** combinada com princípios de **Domain-Driven Design (DDD)**, estruturada em quatro projetos independentes:

- `Hypesoft.Domain` — entidades, objetos de valor, eventos de domínio, interfaces de repositório, serviços de domínio. Zero dependências de bibliotecas externas.
- `Hypesoft.Application` — commands, queries, handlers, DTOs, validators, interfaces de aplicação. Depende apenas do Domain.
- `Hypesoft.Infrastructure` — DbContext EF Core, implementações de repositório, serviços de cache, integrações externas. Depende do Application.
- `Hypesoft.API` — controllers, middlewares, extensions, configuração de DI. Depende do Application e Infrastructure.

Fluxo de dependências: `API → Application ← Infrastructure`, com `Domain` no centro sem dependências externas.

## Consequências

**Positivas:**
- A lógica de domínio é totalmente isolada e testável sem mock de infraestrutura.
- A infraestrutura pode ser substituída (ex: trocar MongoDB por PostgreSQL) sem tocar em Application ou Domain.
- Cada camada evolui de forma independente, facilitando o desenvolvimento paralelo.
- Limites claros facilitam o onboarding de novos membros.

**Negativas:**
- Mais boilerplate comparado a uma arquitetura em camadas simples (projetos adicionais, profiles de mapeamento).
- Commands/Queries precisam ser mapeados para entidades de domínio e de volta para DTOs, adicionando overhead do AutoMapper.

---

## Context

The system requires a clear separation of business rules from infrastructure concerns to allow independent evolution of each layer, facilitate testing, and ensure that domain logic is not polluted by framework or database implementation details. Multiple team members will work across different layers simultaneously.

## Decision

Adopt **Clean Architecture** combined with **Domain-Driven Design (DDD)** principles, structured into four independent projects:

- `Hypesoft.Domain` — entities, value objects, domain events, repository interfaces, domain services. Zero dependencies on external libraries.
- `Hypesoft.Application` — commands, queries, handlers, DTOs, validators, application interfaces. Depends only on Domain.
- `Hypesoft.Infrastructure` — EF Core DbContext, repository implementations, cache services, external integrations. Depends on Application.
- `Hypesoft.API` — controllers, middlewares, extensions, DI configuration. Depends on Application and Infrastructure.

Dependency flow: `API → Application ← Infrastructure`, with `Domain` at the center with no outward dependencies.

## Consequences

**Positive:**
- Domain logic is fully isolated and independently testable with no mocking of infrastructure.
- Infrastructure can be swapped (e.g., replace MongoDB with PostgreSQL) without touching Application or Domain.
- Each layer can evolve independently, facilitating parallel development.
- Clear boundaries make onboarding easier.

**Negative:**
- More boilerplate compared to a simple layered architecture (additional project files, mapping profiles).
- Commands/Queries must be mapped to domain entities and back to DTOs, adding AutoMapper overhead.
