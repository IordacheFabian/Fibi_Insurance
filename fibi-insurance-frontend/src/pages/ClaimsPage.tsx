import { useState } from "react";
import { motion } from "framer-motion";
import { Plus, Search, ShieldAlert, AlertCircle } from "lucide-react";
import { PageHeader } from "@/components/ui/PageHeader";
import { StatusChip } from "@/components/ui/StatusChip";
import { claims } from "@/data/sampleData";
import { useRole } from "@/contexts/RoleContext";
import { cn } from "@/lib/utils";

const statusTabs = ["all", "submitted", "under_review", "approved", "rejected"] as const;

export default function ClaimsPage() {
  const { role } = useRole();
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof statusTabs[number]>("all");

  const filtered = claims.filter((c) => {
    const matchesSearch = c.claimNumber.toLowerCase().includes(search.toLowerCase()) || c.clientName.toLowerCase().includes(search.toLowerCase());
    const matchesTab = tab === "all" || c.status === tab;
    return matchesSearch && matchesTab;
  });

  return (
    <div className="space-y-6">
      <PageHeader title="Claims" description={role === "admin" ? "Review and process insurance claims" : "Submit and track your claims"}>
        {role === "broker" && (
          <button className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-warning text-warning-foreground text-sm font-medium hover:opacity-90 transition-opacity">
            <Plus className="h-4 w-4" /> Submit Claim
          </button>
        )}
      </PageHeader>

      {/* Summary cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {[
          { label: "Total Claims", value: claims.length, color: "text-foreground" },
          { label: "Under Review", value: claims.filter(c => c.status === "under_review").length, color: "text-info" },
          { label: "Approved", value: claims.filter(c => c.status === "approved").length, color: "text-success" },
          { label: "Total Value", value: `£${(claims.reduce((s, c) => s + c.amount, 0) / 1000).toFixed(0)}k`, color: "text-warning" },
        ].map((s, i) => (
          <motion.div
            key={s.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.05 }}
            className="glass-card p-4"
          >
            <p className="text-xs text-muted-foreground">{s.label}</p>
            <p className={cn("text-xl font-bold mt-1", s.color)}>{s.value}</p>
          </motion.div>
        ))}
      </div>

      <div className="glass-card p-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search claims..."
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5 overflow-x-auto">
            {statusTabs.map((t) => (
              <button
                key={t}
                onClick={() => setTab(t)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize whitespace-nowrap",
                  tab === t ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground"
                )}
              >
                {t === "under_review" ? "Under Review" : t}
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="space-y-3">
        {filtered.map((claim, i) => (
          <motion.div
            key={claim.id}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.05 }}
            className="glass-card-hover p-5 cursor-pointer"
          >
            <div className="flex flex-col sm:flex-row sm:items-center gap-4">
              <div className="flex items-center gap-3 flex-1">
                <div className={cn(
                  "h-10 w-10 rounded-xl flex items-center justify-center",
                  claim.status === "approved" ? "bg-success/10" :
                  claim.status === "rejected" ? "bg-destructive/10" :
                  "bg-warning/10"
                )}>
                  <ShieldAlert className={cn(
                    "h-5 w-5",
                    claim.status === "approved" ? "text-success" :
                    claim.status === "rejected" ? "text-destructive" :
                    "text-warning"
                  )} />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <span className="font-mono text-xs font-medium">{claim.claimNumber}</span>
                    <StatusChip status={claim.status} />
                  </div>
                  <p className="text-xs text-muted-foreground mt-0.5">{claim.clientName} · {claim.type}</p>
                </div>
              </div>
              <div className="flex items-center gap-6 text-xs">
                <div>
                  <p className="text-muted-foreground">Claim Amount</p>
                  <p className="text-sm font-bold">£{claim.amount.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Incident Date</p>
                  <p className="font-medium">{new Date(claim.incidentDate).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" })}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Policy</p>
                  <p className="font-mono font-medium">{claim.policyNumber}</p>
                </div>
              </div>
            </div>
            <p className="text-xs text-muted-foreground mt-3 pl-13">{claim.description}</p>
          </motion.div>
        ))}
      </div>
    </div>
  );
}
