import { redirect } from "next/navigation";
import { getServerSession } from "next-auth";
import { getProviders } from "next-auth/react";
import { authOptions } from "@/lib/auth";
import SignInButton from "./SignInButton";

export default async function SignInPage() {
  const session = await getServerSession(authOptions);

  if (session) {
    redirect("/");
  }

  const providers = await getProviders();

  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-100 dark:bg-zinc-900">
      <div className="w-full max-w-sm rounded-2xl border border-zinc-200 bg-white px-8 py-10 shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
        <h1 className="mb-1 text-center text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
          Hypesoft
        </h1>
        <p className="mb-8 text-center text-sm text-zinc-500 dark:text-zinc-400">
          Faça login para continuar
        </p>

        <div className="flex flex-col gap-3">
          {providers &&
            Object.values(providers).map((provider) => (
              <SignInButton
                key={provider.id}
                providerId={provider.id}
                providerName={provider.name}
              />
            ))}
        </div>
      </div>
    </div>
  );
}
