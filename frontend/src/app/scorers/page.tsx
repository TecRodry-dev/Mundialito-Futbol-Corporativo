import type { Metadata } from "next";
import { TopScorersView } from "@/components/scorers/TopScorersView";

export const metadata: Metadata = {
  title: "Goleadores",
};

export default function ScorersPage() {
  return <TopScorersView />;
}
