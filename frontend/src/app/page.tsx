import type { Metadata } from "next";
import { StandingsView } from "@/components/standings/StandingsView";

export const metadata: Metadata = {
  title: "Tabla de posiciones",
};

export default function HomePage() {
  return <StandingsView />;
}
