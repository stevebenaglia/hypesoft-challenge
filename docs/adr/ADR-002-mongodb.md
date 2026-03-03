# ADR-002: MongoDB como Banco de Dados Principal / MongoDB as Primary Database

- **Status**: Aceito / Accepted
- **Data / Date**: 2025-01-15

---

## Contexto

O sistema de inventário gerencia produtos e categorias. Os documentos de produtos contêm dados aninhados (referência de categoria, metadados) e podem evoluir em schema ao longo do tempo. A equipe tem experiência com bancos de dados de documentos, e escalabilidade horizontal é um requisito futuro. Busca de texto completo em nome e descrição de produtos é necessária sem um mecanismo de busca separado.

## Decisão

Usar **MongoDB** como banco único, acessado via:

1. **MongoDB.EntityFrameworkCore** (v8.1.0) — provider EF Core para queries LINQ, configuração de entidades e materialização de tipos.
2. **MongoDB.Driver** (v2.28.0) — driver nativo usado diretamente em `ProductRepository` para operações avançadas: busca `$text`, pipelines de agregação (`GetCountByCategoryAsync`, `GetTotalStockValueAsync`) e criação de índices.

Um **índice de texto** em `{ Name: "text", Description: "text" }` é criado na inicialização pelo `DatabaseInitializer` para suportar busca de texto completo eficiente sem necessidade de Elasticsearch ou Atlas Search.

## Consequências

**Positivas:**
- Schema flexível acomoda extensões futuras de atributos de produtos sem migrações.
- O operador nativo `$text` fornece busca tokenizada de texto completo nativamente.
- Escalonamento horizontal via sharding é suportado nativamente.
- O pipeline de agregação permite cálculos eficientes no servidor (valor total do estoque, produtos por categoria).

**Negativas:**
- O provider EF Core para MongoDB é menos maduro que os providers relacionais; queries LINQ complexas podem exigir fallback para o driver nativo.
- Sem transações ACID entre múltiplos documentos sem gerenciamento explícito de sessão.
- A abordagem dual-driver (EF Core + driver nativo no mesmo repositório) adiciona complexidade, mas foi necessária para suporte a queries avançadas.

---

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
