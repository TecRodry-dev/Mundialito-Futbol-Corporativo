"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";

const NAV_LINKS = [
  { href: "/", label: "Tabla" },
  { href: "/scorers", label: "Goleadores" },
  { href: "/matches", label: "Partidos" },
  { href: "/teams", label: "Equipos" },
] as const;

/** Barra superior: menú horizontal en escritorio y desplegable (hamburguesa) en móvil. */
export function AppHeader() {
  const pathname = usePathname();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  function linkClass(href: string) {
    const isActive = pathname === href;
    return `rounded-md px-3 py-2 text-sm font-medium transition-colors ${
      isActive ? "bg-emerald-700 text-white" : "text-emerald-50 hover:bg-emerald-600"
    }`;
  }

  return (
    <header className="bg-emerald-800 text-white shadow">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3 sm:px-6">
        <Link href="/" className="flex items-center gap-2 text-lg font-semibold">
          <span aria-hidden="true">⚽</span>
          Mundialito Corporativo
        </Link>

        <nav aria-label="Principal" className="hidden gap-1 md:flex">
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} className={linkClass(link.href)}>
              {link.label}
            </Link>
          ))}
        </nav>

        <button
          type="button"
          onClick={() => setIsMenuOpen((open) => !open)}
          aria-expanded={isMenuOpen}
          aria-controls="mobile-menu"
          aria-label={isMenuOpen ? "Cerrar menú" : "Abrir menú"}
          className="rounded-md p-2 hover:bg-emerald-700 md:hidden"
        >
          <span aria-hidden="true" className="text-xl leading-none">
            {isMenuOpen ? "✕" : "☰"}
          </span>
        </button>
      </div>

      {isMenuOpen && (
        <nav id="mobile-menu" aria-label="Principal móvil" className="border-t border-emerald-700 px-4 pb-3 md:hidden">
          <div className="flex flex-col gap-1 pt-2">
            {NAV_LINKS.map((link) => (
              <Link key={link.href} href={link.href} onClick={() => setIsMenuOpen(false)} className={linkClass(link.href)}>
                {link.label}
              </Link>
            ))}
          </div>
        </nav>
      )}
    </header>
  );
}
