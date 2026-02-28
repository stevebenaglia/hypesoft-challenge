"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession, signOut } from "next-auth/react";
import { Menu, X } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function Header() {
  const { data: session } = useSession();
  const pathname = usePathname();
  const [mobileOpen, setMobileOpen] = useState(false);

  const isAdmin = session?.user.roles.includes("admin");

  const navLink = (href: string) =>
    pathname === href
      ? "text-sm font-medium text-zinc-900 dark:text-zinc-50"
      : "text-sm text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-50 transition-colors";

  const mobileNavLink = (href: string) =>
    pathname === href
      ? "block px-4 py-2.5 text-sm font-medium text-zinc-900 dark:text-zinc-50 bg-zinc-100 dark:bg-zinc-800 rounded-lg"
      : "block px-4 py-2.5 text-sm text-zinc-600 dark:text-zinc-400 hover:bg-zinc-50 dark:hover:bg-zinc-800 rounded-lg transition-colors";

  return (
    <header className="border-b border-zinc-200 bg-white dark:border-zinc-700 dark:bg-zinc-900">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        {/* Brand + desktop nav */}
        <div className="flex items-center gap-8">
          <span className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">
            Hypesoft
          </span>
          <nav className="hidden md:flex gap-6">
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

        {/* Desktop user info + sign out */}
        <div className="hidden md:flex items-center gap-3">
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

        {/* Mobile hamburger */}
        <button
          className="md:hidden flex items-center justify-center rounded-lg p-2 text-zinc-600 hover:bg-zinc-100 dark:text-zinc-400 dark:hover:bg-zinc-800 transition-colors"
          onClick={() => setMobileOpen((o) => !o)}
          aria-label={mobileOpen ? "Fechar menu" : "Abrir menu"}
        >
          {mobileOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
      </div>

      {/* Mobile menu */}
      {mobileOpen && (
        <div className="md:hidden border-t border-zinc-200 dark:border-zinc-700 px-4 py-3 flex flex-col gap-1">
          <Link href="/" className={mobileNavLink("/")} onClick={() => setMobileOpen(false)}>
            Dashboard
          </Link>
          <Link href="/products" className={mobileNavLink("/products")} onClick={() => setMobileOpen(false)}>
            Produtos
          </Link>
          {isAdmin && (
            <Link href="/categories" className={mobileNavLink("/categories")} onClick={() => setMobileOpen(false)}>
              Categorias
            </Link>
          )}

          <div className="mt-3 border-t border-zinc-100 dark:border-zinc-800 pt-3 flex items-center justify-between">
            <div>
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
      )}
    </header>
  );
}
