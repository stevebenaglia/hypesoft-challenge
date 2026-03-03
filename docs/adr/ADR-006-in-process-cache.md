# ADR-006: In-Process Distributed Cache Instead of Redis

- **Status**: Accepted
- **Date**: 2025-01-15

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
