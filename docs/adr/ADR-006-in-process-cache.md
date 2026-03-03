# ADR-006: Cache In-Process em vez de Redis / In-Process Distributed Cache Instead of Redis

- **Status**: Aceito / Accepted
- **Data / Date**: 2025-01-15

---

## Contexto

Queries de leitura frequentes (listagem de produtos, lista de categorias, resumo do dashboard) atingem o MongoDB a cada requisição. Uma camada de cache é necessária para reduzir latência e carga no banco de dados. A escolha padrão enterprise para cache distribuído é o Redis, mas isso introduz uma dependência adicional de infraestrutura.

## Decisão

Usar **`IDistributedCache` com implementação in-memory** (`AddDistributedMemoryCache`) em vez de Redis:

- `ICacheService` / `DistributedCacheService` encapsula `IDistributedCache` com serialização JSON e suporte a TTL.
- `ICacheInvalidationService` / `CacheInvalidationService` implementa um **padrão de contador de geração**: uma chave de geração compartilhada é incrementada a cada mutação de produto; todas as chaves de cache de lista de produtos embutem o número de geração (`products:g{gen}:p{page}:s{size}:...`), tornando entradas obsoletas inacessíveis sem exclusão explícita.
- TTLs: Categorias = 5 min, Listas de produtos = 2 min, Produto por ID = 5 min, Dashboard = 2 min.

A abstração (`ICacheService`) é projetada para que a implementação possa ser trocada para Redis (`AddStackExchangeRedisCache`) alterando apenas um registro de DI em `InfrastructureServiceExtensions`.

## Consequências

**Positivas:**
- Nenhum container adicional necessário — reduz a complexidade do Docker Compose e o tempo de cold-start.
- Zero overhead de rede para leituras de cache (mesmo processo).
- Suficiente para implantação em instância única, que cobre o escopo atual.
- A estratégia de invalidação de cache (geração bump) funciona corretamente sem Lua scripts ou funcionalidades específicas do Redis.

**Negativas:**
- O cache não é compartilhado entre múltiplas instâncias da API — um scale-out horizontal exigiria substituição da implementação por Redis.
- Cache in-memory consome memória do processo da API; datasets grandes podem causar pressão.
- O cache é perdido ao reiniciar a API, causando um spike de cold-start no MongoDB.

**Caminho de migração**: Substituir `services.AddDistributedMemoryCache()` por `services.AddStackExchangeRedisCache(...)` em `InfrastructureServiceExtensions.cs`. Nenhuma outra alteração necessária.

---

## Context

Frequent read queries (product listings, category list, dashboard summary) hit MongoDB on every request. A caching layer is needed to reduce latency and database load. The standard enterprise choice for distributed caching is Redis, but this introduces an additional infrastructure dependency.

## Decision

Use **`IDistributedCache` with the in-memory implementation** (`AddDistributedMemoryCache`) instead of Redis:

- `ICacheService` / `DistributedCacheService` wraps `IDistributedCache` with JSON serialisation and TTL support.
- `ICacheInvalidationService` / `CacheInvalidationService` implements a **generation-counter pattern**: a shared generation key is incremented on every product mutation; all product list cache keys embed the generation number (`products:g{gen}:p{page}:s{size}:...`), making stale entries unreachable without explicit deletion.
- TTLs: Categories = 5 min, Product lists = 2 min, Product by ID = 5 min, Dashboard = 2 min.

The abstraction (`ICacheService`) is designed so that the implementation can be swapped to Redis (`AddStackExchangeRedisCache`) by changing a single DI registration in `InfrastructureServiceExtensions`.

## Consequences

**Positive:**
- No additional container required — reduces Docker Compose complexity and cold-start time.
- Zero network overhead for cache reads (same process).
- Sufficient for a single-instance deployment, which covers the current scope.
- Cache invalidation strategy (generation bump) works correctly without Lua scripts or Redis-specific features.

**Negative:**
- Cache is not shared across multiple API instances — a horizontal scale-out would require replacing the implementation with Redis.
- In-memory cache consumes API process memory; large datasets may cause pressure.
- Cache is lost on API restart, causing a cold-start spike on MongoDB.

**Migration path**: Replace `services.AddDistributedMemoryCache()` with `services.AddStackExchangeRedisCache(...)` in `InfrastructureServiceExtensions.cs`. No other changes required.
