import type { AuthOptions } from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";

async function refreshAccessToken(token: any) {
  try {
    const url = `${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/token`;

    const response = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({
        client_id: process.env.KEYCLOAK_ID!,
        client_secret: process.env.KEYCLOAK_SECRET!,
        grant_type: "refresh_token",
        refresh_token: token.refreshToken,
      }),
    });

    const refreshedTokens = await response.json();

    if (!response.ok) throw refreshedTokens;

    return {
      ...token,
      accessToken: refreshedTokens.access_token,
      idToken: refreshedTokens.id_token,
      refreshToken: refreshedTokens.refresh_token ?? token.refreshToken,
      accessTokenExpires: Date.now() + refreshedTokens.expires_in * 1000,
    };
  } catch {
    return { ...token, error: "RefreshAccessTokenError" };
  }
}

export const authOptions: AuthOptions = {
  providers: [
    KeycloakProvider({
      clientId: process.env.KEYCLOAK_ID!,
      clientSecret: process.env.KEYCLOAK_SECRET!,
      issuer: process.env.KEYCLOAK_ISSUER!,
    }),
  ],

  callbacks: {
    async jwt({ token, account, profile }) {
      if (account && profile) {
        token.accessToken = account.access_token;
        token.idToken = account.id_token;
        token.refreshToken = account.refresh_token;
        token.accessTokenExpires = account.expires_at
          ? account.expires_at * 1000
          : Date.now() + 3600 * 1000;

        const profileWithRoles = profile as any;
        token.roles = profileWithRoles.roles ?? [];

        return token;
      }

      if (Date.now() < (token.accessTokenExpires ?? 0)) {
        return token;
      }

      return refreshAccessToken(token);
    },

    async session({ session, token }) {
      session.accessToken = token.accessToken;
      session.idToken = token.idToken;
      session.error = token.error;

      if (session.user) {
        session.user.roles = token.roles ?? [];
      }

      return session;
    },
  },

  events: {
    async signOut({ token }: { token: any }) {
      const issuer = process.env.KEYCLOAK_ISSUER;
      const idToken = token?.idToken;
      if (!idToken || !issuer) return;

      const logoutUrl =
        `${issuer}/protocol/openid-connect/logout` +
        `?id_token_hint=${idToken}` +
        `&post_logout_redirect_uri=${encodeURIComponent(process.env.NEXTAUTH_URL + "/")}`;

      try {
        await fetch(logoutUrl);
      } catch {
        console.error("Failed to call Keycloak end_session_endpoint");
      }
    },
  },

  pages: {
    signIn: "/auth/signin",
  },
};
