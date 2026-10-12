import { statusLabels } from "@/lib/format";
import type { MatchStatus } from "@/types/api";

const badgeClasses: Record<MatchStatus, string> = {
  Scheduled: "bg-amber-50 text-amber-800 ring-amber-200",
  Played: "bg-emerald-50 text-emerald-800 ring-emerald-200",
};

export function MatchStatusBadge({ status }: { status: MatchStatus }) {
  return (
    <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${badgeClasses[status]}`}>
      {statusLabels[status]}
    </span>
  );
}
