import type { Metadata } from "next";
import { MatchesView } from "@/components/matches/MatchesView";

export const metadata: Metadata = {
  title: "Partidos",
};

export default function MatchesPage() {
  return <MatchesView />;
}
