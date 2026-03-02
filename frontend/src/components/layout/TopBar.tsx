"use client";

import { useSession, signOut } from "next-auth/react";
import { Sun, Moon, LogOut, Menu } from "lucide-react";
import { useTheme } from "@/components/providers/ThemeProvider";
import { cn } from "@/lib/utils";

interface TopBarProps {
  onMenuClick: () => void;
}

export default function TopBar({ onMenuClick }: TopBarProps) {
  const { data: session } = useSession();
  const { theme, toggle } = useTheme();

  const isAdmin = session?.user.roles.includes("admin");
  const initial = (session?.user.name ?? session?.user.email ?? "U")
    .charAt(0)
    .toUpperCase();

  return (
    <div className="flex h-14 shrink-0 items-center justify-between border-b border-zinc-200 bg-white px-4 dark:border-zinc-700 dark:bg-zinc-900 md:px-6">
      {/* Left: hamburger (mobile) + brand (mobile) */}
      <div className="flex items-center gap-3 md:hidden">
        <button
          onClick={onMenuClick}
          className="rounded-lg p-1.5 text-zinc-600 transition-colors hover:bg-zinc-100 dark:text-zinc-400 dark:hover:bg-zinc-800"
        >
          <Menu className="h-5 w-5" />
        </button>
        <div className="flex items-center gap-2">
          <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-violet-600 text-xs font-bold text-white">
            H
          </div>
          <span className="text-base font-bold text-zinc-900 dark:text-zinc-50">
            Hypesoft
          </span>
        </div>
      </div>

      {/* Spacer on desktop */}
      <div className="hidden md:block" />

      {/* Right: theme toggle + user info + logout */}
      <div className="flex items-center gap-2">
        {/* Theme toggle */}
        <button
          onClick={toggle}
          className={cn(
            "rounded-lg p-2 transition-colors",
            theme === "dark"
              ? "text-amber-400 hover:bg-zinc-800"
              : "text-zinc-500 hover:bg-zinc-100"
          )}
          title={theme === "dark" ? "Modo claro" : "Modo escuro"}
        >
          {theme === "dark" ? (
            <Sun className="h-4 w-4" />
          ) : (
            <Moon className="h-4 w-4" />
          )}
        </button>

        {/* Divider */}
        <div className="mx-1 h-6 w-px bg-zinc-200 dark:bg-zinc-700" />

        {/* User avatar + info */}
        <div className="flex items-center gap-2.5">
          <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-violet-100 text-xs font-semibold text-violet-700 dark:bg-violet-900/40 dark:text-violet-300">
            {initial}
          </div>
          <div className="hidden text-right sm:block">
            <p className="text-sm font-medium leading-tight text-zinc-900 dark:text-zinc-50">
              {session?.user.name ?? session?.user.email}
            </p>
            <p className="text-xs leading-tight text-zinc-500">
              {isAdmin ? "Administrador" : "Usuário"}
            </p>
          </div>
        </div>

        {/* Logout */}
        <button
          onClick={() => signOut({ callbackUrl: "/auth/signin" })}
          className="rounded-lg p-2 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 dark:hover:bg-zinc-800 dark:hover:text-zinc-300"
          title="Sair"
        >
          <LogOut className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}
