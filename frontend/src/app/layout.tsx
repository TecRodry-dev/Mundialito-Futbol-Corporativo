import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { AppHeader } from "@/components/layout/AppHeader";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    default: "Mundialito Corporativo",
    template: "%s | Mundialito Corporativo",
  },
  description: "Gestión del Mundialito de Fútbol Corporativo: tabla de posiciones, goleadores, partidos y equipos.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="es" className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}>
      <body className="flex min-h-full flex-col bg-zinc-50 text-zinc-900">
        <AppHeader />
        <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-6 sm:px-6 lg:py-10">{children}</main>
        <footer className="border-t border-zinc-200 py-4 text-center text-xs text-zinc-500">
          Mundialito de Fútbol Corporativo
        </footer>
      </body>
    </html>
  );
}
