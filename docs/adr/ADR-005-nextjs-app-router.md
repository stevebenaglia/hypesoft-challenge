# ADR-005: Next.js App Router com React Server Components / Next.js App Router with React Server Components

- **Status**: Aceito / Accepted
- **Data / Date**: 2025-01-15

---

## Contexto

O frontend requer renderização server-side para performance de carregamento inicial e SEO, interatividade client-side para formulários e filtros em tempo real, e uma separação clara entre busca de dados e renderização de UI. O App Router do Next.js 14+ introduz React Server Components (RSC) como padrão, o que muda significativamente o modelo de busca de dados em relação ao Pages Router.

## Decisão

Usar **Next.js 16** com o **App Router** e React Server Components:

- **Server Components** (padrão): As páginas Dashboard, Products e Categories são `async` server components. Elas chamam `getServerSession()` e `apiFetch()` diretamente no servidor, enviando HTML com dados pré-renderizados.
- **Client Components** (`"use client"`): Partes interativas — `ProductsClient`, `CategoriesClient`, `DashboardView` — são client components que usam TanStack Query para estado local e mutations.
- **Route Groups**: O grupo de layout `(authenticated)/` aplica verificação de sessão server-side e redireciona para `/auth/signin` se não autenticado.
- **Middleware** (`middleware.ts`): `withAuth` do `next-auth/middleware` protege todas as rotas na edge, antes que a requisição chegue ao server component.
- **Dynamic imports**: `DashboardChart` usa `dynamic(() => import(...), { ssr: false })` para evitar problemas de hidratação do Chart.js no servidor.

## Consequências

**Positivas:**
- Carregamento inicial mais rápido — HTML é pré-renderizado com dados no servidor.
- Server Components eliminam waterfalls de busca de dados client-side no render inicial.
- Verificação de autenticação ocorre em dois níveis (middleware + layout), prevenindo acesso não autorizado.
- `ssr: false` no Chart.js evita incompatibilidades de hidratação do canvas.

**Negativas:**
- O modelo mental de RSC (limite server vs client) adiciona complexidade em relação ao Pages Router.
- `next-auth` v4 foi projetado para o Pages Router; a integração com App Router requer workarounds (`getServerSession` em vez de `getSession`).
- A mistura de server e client components requer serialização cuidadosa de props (sem funções, sem instâncias de classe entre os limites).

---

## Context

The frontend requires server-side rendering for initial page load performance and SEO, client-side interactivity for forms and real-time filters, and a clear separation between data-fetching and UI rendering. Next.js 14+ App Router introduces React Server Components (RSC) as the default, which changes the data-fetching model significantly compared to the Pages Router.

## Decision

Use **Next.js 16** with the **App Router** and React Server Components:

- **Server Components** (default): Dashboard, Products, and Categories pages are `async` server components. They call `getServerSession()` and `apiFetch()` directly on the server, sending HTML with data pre-rendered.
- **Client Components** (`"use client"`): Interactive parts — `ProductsClient`, `CategoriesClient`, `DashboardView` — are client components that use TanStack Query for local state and mutations.
- **Route Groups**: `(authenticated)/` layout group enforces session check server-side and redirects to `/auth/signin` if unauthenticated.
- **Middleware** (`middleware.ts`): `withAuth` from `next-auth/middleware` protects all routes at the edge, before the request reaches the server component.
- **Dynamic imports**: `DashboardChart` uses `dynamic(() => import(...), { ssr: false })` to avoid Chart.js hydration issues on the server.

## Consequences

**Positive:**
- Initial page load is faster — HTML is pre-rendered with data on the server.
- Server Components eliminate client-side data-fetching waterfalls for initial render.
- Authentication check happens at two levels (middleware + layout), preventing unauthorized access.
- `ssr: false` on Chart.js avoids canvas hydration mismatches.

**Negative:**
- RSC mental model (server vs client boundary) adds complexity compared to the Pages Router.
- `next-auth` v4 was designed for the Pages Router; App Router integration requires workarounds (`getServerSession` instead of `getSession`).
- Mixing server and client components requires careful prop serialisation (no functions, no class instances across the boundary).
