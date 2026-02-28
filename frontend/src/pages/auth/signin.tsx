import type { GetServerSidePropsContext } from "next";
import { getServerSession } from "next-auth";
import { getProviders, signIn } from "next-auth/react";
import { authOptions } from "@/pages/api/auth/[...nextauth]";

interface SignInProps {
  providers: Awaited<ReturnType<typeof getProviders>>;
}

export default function SignIn({ providers }: SignInProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-100 dark:bg-zinc-900">
      <div className="w-full max-w-sm rounded-2xl border border-zinc-200 bg-white px-8 py-10 shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
        <h1 className="mb-1 text-center text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
          Hypesoft
        </h1>
        <p className="mb-8 text-center text-sm text-zinc-500 dark:text-zinc-400">
          Faça login para continuar
        </p>

        {providers &&
          Object.values(providers).map((provider) => (
            <button
              key={provider.name}
              onClick={() => signIn(provider.id, { callbackUrl: "/" })}
              className="flex w-full items-center justify-center rounded-lg bg-zinc-900 px-4 py-3 text-sm font-medium text-white transition-colors hover:bg-zinc-700 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
            >
              Entrar com {provider.name}
            </button>
          ))}
      </div>
    </div>
  );
}

export async function getServerSideProps(context: GetServerSidePropsContext) {
  const session = await getServerSession(context.req, context.res, authOptions);

  if (session) {
    return { redirect: { destination: "/", permanent: false } };
  }

  const providers = await getProviders();
  return { props: { providers: providers ?? {} } };
}
