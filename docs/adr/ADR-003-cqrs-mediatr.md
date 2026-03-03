# ADR-003: CQRS + MediatR

- **Status**: Aceito / Accepted
- **Data / Date**: 2025-01-15

---

## Contexto

A camada de Application precisa de uma forma consistente de despachar commands e queries dos controllers para os handlers sem acoplamento direto. Preocupações transversais (validação, logging, monitoramento de performance) devem ser aplicadas uniformemente em todos os casos de uso. À medida que o sistema cresce, adicionar novos casos de uso não deve exigir modificação do código existente.

## Decisão

Adotar **CQRS (Command Query Responsibility Segregation)** via **MediatR** (v12.4.1):

- **Commands** (`CreateProductCommand`, `UpdateStockCommand`, etc.) — mutam estado, retornam um DTO.
- **Queries** (`GetProductsQuery`, `GetDashboardSummaryQuery`, etc.) — leem estado, retornam um DTO ou coleção.
- **Pipeline Behaviors** — dois behaviors transversais registrados globalmente:
  - `ValidationPipelineBehavior<TRequest, TResponse>` — executa todos os validators FluentValidation antes do handler; retorna 422 em caso de falha.
  - `LoggingPipelineBehavior<TRequest, TResponse>` — registra início/fim da requisição com duração; emite warning para requisições que excedam 500ms.

## Consequências

**Positivas:**
- Controllers são enxutos — apenas despachamos commands/queries via `IMediator.Send()`.
- Preocupações transversais (validação, logging) são adicionadas uma única vez como pipeline behaviors, sem duplicação por handler.
- Cada handler tem responsabilidade única e é testável unitariamente de forma independente.
- Adicionar novos casos de uso requer apenas um novo par Command/Query + Handler, sem alterações no código existente (Princípio Aberto/Fechado).

**Negativas:**
- A indireção introduzida pelo MediatR pode dificultar o rastreamento do fluxo de chamadas sem ferramentas adequadas.
- Leve overhead de performance pela reflexão do pipeline; negligível nessa escala.
- Operações CRUD simples ganham pouco com CQRS; o padrão agrega mais valor à medida que a complexidade cresce.

---

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
