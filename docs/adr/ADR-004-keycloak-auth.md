# ADR-004: Keycloak para Gerenciamento de Identidade e Acesso / Keycloak for Identity and Access Management

- **Status**: Aceito / Accepted
- **Data / Date**: 2025-01-15

---

## Contexto

O sistema requer autenticação e autorização baseada em roles. Construir um servidor de identidade customizado exigiria esforço significativo (emissão de tokens, refresh, PKCE, JWKS, UI de gerenciamento de usuários). A solução deve suportar OAuth 2.0 / OpenID Connect e integrar tanto com o frontend Next.js (gerenciamento de sessão) quanto com o backend .NET (validação de JWT).

## Decisão

Usar **Keycloak 26** como provedor de identidade:

- **Backend**: Middleware JwtBearer valida tokens contra o endpoint JWKS do Keycloak (`MetadataAddress` aponta para `.well-known/openid-configuration`). `KeycloakClaimsTransformation` extrai roles de realm da claim `realm_access.roles` e as mapeia para `ClaimTypes.Role`, permitindo que `[Authorize(Roles = "admin")]` funcione nativamente.
- **Frontend**: `next-auth` (v4) com `KeycloakProvider` gerencia o fluxo de authorization code OAuth 2.0, refresh de token e armazenamento de sessão. Um handler customizado em `events.signOut` chama o `end_session_endpoint` do Keycloak com `id_token_hint` para logout SSO adequado.
- **Realm pré-configurado**: `keycloak/realms/hypesoft-realm.json` é importado na primeira inicialização, fornecendo configuração de cliente, roles (`admin`, `user`) e usuários seed (`admin/admin`, `user/user`).

## Consequências

**Positivas:**
- Conformidade total com OAuth 2.0 / OpenID Connect nativamente.
- Gerenciamento de usuários, atribuição de roles e SSO gerenciados pela UI do Keycloak — sem código customizado necessário.
- Validação de token verificada criptograficamente (RS256) usando chaves públicas do JWKS.
- Export do realm permite ambientes reproduzíveis via Docker Compose.

**Negativas:**
- Adiciona um container Keycloak + PostgreSQL ao stack, aumentando o tempo de cold-start.
- O desenvolvimento local requer o stack Docker completo em execução.
- `next-auth` v4 foi projetado para o Pages Router; a integração com App Router requer workarounds (`getServerSession` em vez de `getSession`).

---

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
