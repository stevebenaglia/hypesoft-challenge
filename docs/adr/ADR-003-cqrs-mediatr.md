# ADR-003: CQRS + MediatR

- **Status**: Accepted
- **Date**: 2025-01-15

## Context

The Application layer needs a consistent way to dispatch commands and queries from controllers to handlers without direct coupling. Cross-cutting concerns (validation, logging, performance monitoring) must be applied uniformly across all use cases. As the system grows, adding new use cases should not require modifying existing code.

## Decision

Adopt **CQRS (Command Query Responsibility Segregation)** via **MediatR** (v12.4.1):

- **Commands** (`CreateProductCommand`, `UpdateStockCommand`, etc.) — mutate state, return a DTO.
- **Queries** (`GetProductsQuery`, `GetDashboardSummaryQuery`, etc.) — read state, return a DTO or collection.
- **Pipeline Behaviors** — two cross-cutting behaviors registered globally:
  - `ValidationPipelineBehavior<TRequest, TResponse>` — runs all FluentValidation validators before the handler executes; returns 422 on failure.
  - `LoggingPipelineBehavior<TRequest, TResponse>` — logs request start/end with duration; emits a warning for requests exceeding 500ms.

## Consequences

**Positive:**
- Controllers are thin — they only dispatch commands/queries via `IMediator.Send()`.
- Cross-cutting concerns (validation, logging) are added once as pipeline behaviors, not duplicated per handler.
- Each handler has a single responsibility and is independently unit-testable.
- Adding new use cases requires only a new Command/Query + Handler pair, with no changes to existing code (Open/Closed Principle).

**Negative:**
- Indirection introduced by MediatR can make call flow harder to trace without tooling.
- Slight performance overhead from pipeline reflection; negligible at this scale.
- Simple CRUD operations gain little from CQRS; the pattern is more valuable as complexity grows.
