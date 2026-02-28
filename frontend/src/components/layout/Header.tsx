"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession, signOut } from "next-auth/react";
import { Button } from "@/components/ui/button";

export default function Header() {
  const { data: session } = useSession();
  const pathname = usePathname();

  const isAdmin = session?.user.roles.includes("admin");

  const navLink = (href: string) =>
    pathname === href
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
            <Link href="/" className={navLink("/")}>
              Dashboard
            </Link>
            <Link href="/products" className={navLink("/products")}>
              Produtos
            </Link>
            {isAdmin && (
              <Link href="/categories" className={navLink("/categories")}>
                Categorias
              </Link>
            )}
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
          <Button
            variant="outline"
            size="sm"
            onClick={() => signOut({ callbackUrl: "/" })}
          >
            Sair
          </Button>
        </div>
      </div>
    </header>
  );
}
