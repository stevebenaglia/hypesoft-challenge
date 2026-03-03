# ADR-005: Next.js App Router with React Server Components

- **Status**: Accepted
- **Date**: 2025-01-15

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
