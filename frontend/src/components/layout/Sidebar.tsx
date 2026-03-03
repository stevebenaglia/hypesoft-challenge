"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession } from "next-auth/react";
import { LayoutDashboard, Package, Tag } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { cn } from "@/lib/utils";
import type { DashboardSummary } from "@/types/api";

function NavItem({
  href,
  label,
  icon: Icon,
  badge,
  onClick,
}: {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  badge?: number;
  onClick?: () => void;
}) {
  const pathname = usePathname();
  const isActive = href === "/" ? pathname === "/" : pathname.startsWith(href);

  return (
    <Link
      href={href}
      onClick={onClick}
      className={cn(
        "flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors",
        isActive
          ? "bg-violet-600 text-white"
          : "text-zinc-600 hover:bg-zinc-100 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
      )}
    >
      <Icon
        className={cn(
          "h-4.5 w-4.5 shrink-0",
          isActive ? "text-white" : "text-zinc-500 dark:text-zinc-400"
        )}
      />
      <span className="flex-1">{label}</span>
      {badge != null && badge > 0 && (
        <span
          className={cn(
            "flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[10px] font-semibold",
            isActive
              ? "bg-white/20 text-white"
              : "bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-400"
          )}
        >
          {badge > 99 ? "99+" : badge}
        </span>
      )}
    </Link>
  );
}

function SidebarContent({ onNavClick }: { onNavClick?: () => void }) {
  const { data: session } = useSession();
  const isAdmin = session?.user.roles.includes("admin");
  const t = useTranslations("nav");

  const { data: dashboard } = useQuery<DashboardSummary | null>({
    queryKey: ["dashboard-sidebar"],
    queryFn: async () => {
      if (!session?.accessToken) return null;
      const res = await fetch("/api/dashboard", {
        headers: { Authorization: `Bearer ${session.accessToken}` },
      });
      if (!res.ok) return null;
      return res.json();
    },
    enabled: !!session?.accessToken,
    staleTime: 60_000,
  });

  const lowStockCount = dashboard?.lowStockProducts.length ?? 0;

  const mainNav = [
    { href: "/", label: t("dashboard"), icon: LayoutDashboard, showBadge: false },
    { href: "/products", label: t("products"), icon: Package, showBadge: true },
  ];

  const adminNav = [
    { href: "/categories", label: t("categories"), icon: Tag, showBadge: false },
  ];

  return (
    <div className="flex h-full w-64 flex-col border-r border-zinc-200 bg-white dark:border-zinc-700 dark:bg-zinc-900">
      {/* Brand */}
      <div className="flex items-center gap-3 px-5 py-5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-violet-600 text-sm font-bold text-white">
          H
        </div>
        <span className="text-lg font-bold text-zinc-900 dark:text-zinc-50">
          Hypesoft
        </span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-0.5 px-3">
        <p className="mb-2 px-3 text-[10px] font-semibold uppercase tracking-widest text-zinc-400">
          {t("general")}
        </p>
        {mainNav.map((item) => (
          <NavItem
            key={item.href}
            href={item.href}
            label={item.label}
            icon={item.icon}
            badge={item.showBadge ? lowStockCount : undefined}
            onClick={onNavClick}
          />
        ))}
        {isAdmin &&
          adminNav.map((item) => (
            <NavItem
              key={item.href}
              href={item.href}
              label={item.label}
              icon={item.icon}
              onClick={onNavClick}
            />
          ))}
      </nav>
    </div>
  );
}

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

export default function Sidebar({ open, onClose }: SidebarProps) {
  return (
    <>
      {/* Desktop sidebar */}
      <div className="hidden md:flex md:shrink-0">
        <SidebarContent />
      </div>

      {/* Mobile overlay + drawer */}
      {open && (
        <>
          <div
            className="fixed inset-0 z-40 bg-black/40 md:hidden"
            onClick={onClose}
          />
          <div className="fixed inset-y-0 left-0 z-50 md:hidden">
            <SidebarContent onNavClick={onClose} />
          </div>
        </>
      )}
    </>
  );
}
