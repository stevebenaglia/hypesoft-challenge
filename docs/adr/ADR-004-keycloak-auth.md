# ADR-004: Keycloak for Identity and Access Management

- **Status**: Accepted
- **Date**: 2025-01-15

## Context

The system requires authentication and role-based authorization. Building a custom identity server would require significant effort (token issuance, refresh, PKCE, JWKS, user management UI). The solution must support OAuth 2.0 / OpenID Connect and integrate with both the Next.js frontend (session management) and the .NET backend (JWT validation).

## Decision

Use **Keycloak 26** as the identity provider:

- **Backend**: JwtBearer middleware validates tokens against Keycloak's JWKS endpoint (`MetadataAddress` points to `.well-known/openid-configuration`). `KeycloakClaimsTransformation` extracts realm roles from the `realm_access.roles` claim and maps them to `ClaimTypes.Role` so `[Authorize(Roles = "admin")]` works natively.
- **Frontend**: `next-auth` (v4) with `KeycloakProvider` handles the OAuth 2.0 authorization code flow, token refresh, and session storage. A custom `events.signOut` handler calls Keycloak's `end_session_endpoint` with `id_token_hint` for proper SSO logout.
- **Pre-configured realm**: `keycloak/realms/hypesoft-realm.json` is imported on first boot, providing client configuration, roles (`admin`, `user`), and seeded users (`admin/admin`, `user/user`).

## Consequences

**Positive:**
- Full OAuth 2.0 / OpenID Connect compliance out of the box.
- User management, role assignment, and SSO handled by Keycloak UI — no custom code required.
- Token validation is cryptographically verified (RS256) using public keys from JWKS.
- Realm export enables reproducible environments via Docker Compose.

**Negative:**
- Adds a Keycloak + PostgreSQL container to the stack, increasing cold-start time.
- Local development requires the full Docker stack to be running.
- `next-auth` v4 with Keycloak requires careful token refresh implementation to avoid silent expiry.
