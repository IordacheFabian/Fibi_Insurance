import { motion } from "framer-motion";
import { PageHeader } from "@/components/ui/PageHeader";
import { GitBranch, ArrowRight } from "lucide-react";
import { StatusChip } from "@/components/ui/StatusChip";

const endorsements = [
  {
    id: "e1",
    policyNumber: "POL-2025-001",
    clientName: "Meridian Holdings Ltd",
    fromVersion: 2,
    toVersion: 3,
    reason: "Premium adjustment due to additional coverage",
    effectiveDate: "2025-02-15",
    premiumChange: "+£5,000",
    status: "active",
  },
  {
    id: "e2",
    policyNumber: "POL-2025-003",
    clientName: "Atlas Property Group",
    fromVersion: 1,
    toVersion: 2,
    reason: "Building valuation update — industrial expansion",
    effectiveDate: "2025-03-01",
    premiumChange: "+£12,000",
    status: "active",
  },
  {
    id: "e3",
    policyNumber: "POL-2024-018",
    clientName: "Crown Estates International",
    fromVersion: 3,
    toVersion: 4,
    reason: "Reduction of coverage scope",
    effectiveDate: "2024-10-01",
    premiumChange: "-£3,500",
    status: "expired",
  },
];

export default function EndorsementsPage() {
  return (
    <div className="space-y-6">
      <PageHeader title="Endorsements" description="Policy modifications and version history" />

      {/* Timeline */}
      <div className="space-y-4">
        {endorsements.map((e, i) => (
          <motion.div
            key={e.id}
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: i * 0.1 }}
            className="glass-card-hover p-5 relative"
          >
            {/* Timeline connector */}
            {i < endorsements.length - 1 && (
              <div className="absolute left-[29px] top-[60px] bottom-[-20px] w-px bg-border" />
            )}

            <div className="flex items-start gap-4">
              <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center shrink-0 relative z-10">
                <GitBranch className="h-5 w-5 text-primary" />
              </div>

              <div className="flex-1">
                <div className="flex items-center gap-2 flex-wrap">
                  <span className="font-mono text-xs font-medium">{e.policyNumber}</span>
                  <span className="flex items-center gap-1 text-xs text-muted-foreground">
                    v{e.fromVersion} <ArrowRight className="h-3 w-3" /> v{e.toVersion}
                  </span>
                  <StatusChip status={e.status} />
                </div>
                <p className="text-sm font-medium mt-1">{e.clientName}</p>
                <p className="text-xs text-muted-foreground mt-0.5">{e.reason}</p>

                <div className="flex items-center gap-6 mt-3 text-xs">
                  <div>
                    <span className="text-muted-foreground">Effective: </span>
                    <span className="font-medium">{new Date(e.effectiveDate).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" })}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Premium Impact: </span>
                    <span className={e.premiumChange.startsWith("+") ? "text-warning font-semibold" : "text-success font-semibold"}>
                      {e.premiumChange}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </motion.div>
        ))}
      </div>
    </div>
  );
}
