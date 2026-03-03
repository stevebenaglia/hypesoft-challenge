# ADR-002: MongoDB as Primary Database

- **Status**: Accepted
- **Date**: 2025-01-15

## Context

The inventory system manages products and categories. Product documents contain nested data (category reference, metadata) and may evolve in schema over time. The team has experience with document databases, and horizontal scaling is a future requirement. Full-text search on product name and description is needed without a separate search engine.

## Decision

Use **MongoDB** as the sole database, accessed via:

1. **MongoDB.EntityFrameworkCore** (v8.1.0) — EF Core provider for LINQ queries, entity configuration, and type materialisation.
2. **MongoDB.Driver** (v2.28.0) — native driver used directly in `ProductRepository` for advanced operations: `$text` search, aggregation pipelines (`GetCountByCategoryAsync`, `GetTotalStockValueAsync`), and index creation.

A **text index** on `{ Name: "text", Description: "text" }` is created at startup by `DatabaseInitializer` to support efficient full-text search without requiring Elasticsearch or Atlas Search.

## Consequences

**Positive:**
- Flexible schema accommodates future product attribute extensions without migrations.
- Native `$text` operator provides tokenized full-text search out of the box.
- Horizontal scaling via sharding is natively supported.
- Aggregation pipeline enables efficient server-side calculations (total stock value, products by category).

**Negative:**
- EF Core provider for MongoDB is less mature than relational providers; complex LINQ queries may require fallback to native driver.
- No ACID transactions across multiple documents without explicit session management.
- Dual-driver approach (EF Core + native driver in same repository) adds complexity but was necessary for advanced query support.
