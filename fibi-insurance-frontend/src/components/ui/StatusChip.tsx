import { cn } from "@/lib/utils";

const statusStyles: Record<string, string> = {
  active: "bg-success/10 text-success border-success/20",
  paid: "bg-success/10 text-success border-success/20",
  approved: "bg-success/10 text-success border-success/20",
  pending: "bg-warning/10 text-warning border-warning/20",
  submitted: "bg-info/10 text-info border-info/20",
  under_review: "bg-info/10 text-info border-info/20",
  draft: "bg-muted text-muted-foreground border-border",
  expired: "bg-muted text-muted-foreground border-border",
  inactive: "bg-muted text-muted-foreground border-border",
  cancelled: "bg-destructive/10 text-destructive border-destructive/20",
  rejected: "bg-destructive/10 text-destructive border-destructive/20",
  overdue: "bg-destructive/10 text-destructive border-destructive/20",
  partial: "bg-warning/10 text-warning border-warning/20",
  none: "bg-muted text-muted-foreground border-border",
  low: "bg-success/10 text-success border-success/20",
  medium: "bg-warning/10 text-warning border-warning/20",
  high: "bg-destructive/10 text-destructive border-destructive/20",
};

const statusLabels: Record<string, string> = {
  under_review: "Under Review",
};

interface StatusChipProps {
  status: string;
  className?: string;
}

export function StatusChip({ status, className }: StatusChipProps) {
  return (
    <span className={cn(
      "status-chip border capitalize",
      statusStyles[status] || "bg-muted text-muted-foreground border-border",
      className
    )}>
      <span className={cn(
        "h-1.5 w-1.5 rounded-full",
        status === "active" || status === "paid" || status === "approved" || status === "low" ? "bg-success" :
        status === "pending" || status === "partial" || status === "medium" ? "bg-warning" :
        status === "submitted" || status === "under_review" ? "bg-info" :
        status === "cancelled" || status === "rejected" || status === "overdue" || status === "high" ? "bg-destructive" :
        "bg-muted-foreground"
      )} />
      {statusLabels[status] || status}
    </span>
  );
}
