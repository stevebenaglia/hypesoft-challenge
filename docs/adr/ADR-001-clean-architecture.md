# ADR-001: Clean Architecture + Domain-Driven Design

- **Status**: Accepted
- **Date**: 2025-01-15

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
