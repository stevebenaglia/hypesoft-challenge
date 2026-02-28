import Link from "next/link";
import { useRouter } from "next/router";
import { useSession, signOut } from "next-auth/react";

export default function Header() {
  const { data: session } = useSession();
  const router = useRouter();

  const isAdmin = session?.user.roles.includes("admin");

  const navLink = (href: string, label: string) =>
    router.pathname === href
      ? "text-sm font-medium text-zinc-900 dark:text-zinc-50"
      : "text-sm text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-50 transition-colors";

  return (
    <header className="border-b border-zinc-200 bg-white dark:border-zinc-700 dark:bg-zinc-900">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <div className="flex items-center gap-8">
          <span className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">
            Hypesoft
          </span>
          <nav className="flex gap-6">
            <Link href="/" className={navLink("/", "Dashboard")}>
              Dashboard
            </Link>
            <Link href="/products" className={navLink("/products", "Produtos")}>
              Produtos
            </Link>
          </nav>
        </div>

        <div className="flex items-center gap-3">
          <div className="text-right">
            <p className="text-sm font-medium text-zinc-900 dark:text-zinc-50">
              {session?.user.name ?? session?.user.email}
            </p>
            <p className="text-xs text-zinc-500">
              {isAdmin ? "Administrador" : "Usuário"}
            </p>
          </div>
          <button
            onClick={() => signOut({ callbackUrl: "/" })}
            className="rounded-md bg-zinc-100 px-3 py-1.5 text-xs font-medium text-zinc-700 transition-colors hover:bg-zinc-200 dark:bg-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-700"
          >
            Sair
          </button>
        </div>
      </div>
    </header>
  );
}
