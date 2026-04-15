import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { motion } from "framer-motion";
import { AlertCircle, ArrowRight, GitBranch, Search } from "lucide-react";
import { Link } from "react-router-dom";
import { EmptyState } from "@/components/ui/EmptyState";
import { PageHeader } from "@/components/ui/PageHeader";
import { StatusChip } from "@/components/ui/StatusChip";
import { useRole } from "@/contexts/RoleContext";
import { getEndorsements, normalizePolicyStatus } from "@/lib/policies/policy.api";
import type { EndorsementTypeValue, PolicyEndorsement } from "@/lib/policies/policy.types";
import { cn, formatMoney } from "@/lib/utils";

const statusTabs = ["all", "active", "expired", "cancelled"] as const;

function formatDateOnly(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}

function formatEndorsementType(value?: EndorsementTypeValue | string) {
  if (value == null) {
    return "Endorsement";
  }

  if (value === 0 || value === "0") {
    return "Insured value change";
  }

  if (value === 1 || value === "1") {
    return "Period extension";
  }

  if (value === 2 || value === "2") {
    return "Risk update";
  }

  if (value === 3 || value === "3") {
    return "Manual adjustment";
  }

  const normalized = String(value)
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/_/g, " ")
    .replace(/Perios/gi, "Period")
    .replace(/Adjustement/gi, "Adjustment")
    .trim();

  if (!normalized) {
    return "Endorsement";
  }

  return normalized.charAt(0).toUpperCase() + normalized.slice(1);
}

function getPremiumDelta(endorsement: PolicyEndorsement) {
  return (endorsement.newFinalPremium ?? 0) - (endorsement.previousFinalPremium ?? 0);
}

export default function EndorsementsPage() {
  const { role } = useRole();
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof statusTabs[number]>("all");

  const { data: endorsements = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ["endorsements", role],
    queryFn: () => getEndorsements(role),
    staleTime: 30000,
  });

  const filtered = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return endorsements.filter((endorsement) => {
      const status = normalizePolicyStatus(endorsement.policyStatus ?? "draft");
      const matchesTab = tab === "all" || status === tab;
      const matchesSearch =
        normalizedSearch.length === 0 ||
        endorsement.policyNumber?.toLowerCase().includes(normalizedSearch) ||
        endorsement.clientName?.toLowerCase().includes(normalizedSearch) ||
        endorsement.reason?.toLowerCase().includes(normalizedSearch) ||
        formatEndorsementType(endorsement.endorsementType).toLowerCase().includes(normalizedSearch);

      return matchesTab && Boolean(matchesSearch);
    });
  }, [endorsements, search, tab]);

  const summary = useMemo(
    () => [
      { label: "Total Endorsements", value: endorsements.length, color: "text-foreground" },
      { label: "Active Policies", value: endorsements.filter((endorsement) => normalizePolicyStatus(endorsement.policyStatus ?? "draft") === "active").length, color: "text-success" },
      {
        label: "Premium Increases",
        value: endorsements.filter((endorsement) => getPremiumDelta(endorsement) > 0).length,
        color: "text-warning",
      },
      {
        label: "Premium Reductions",
        value: endorsements.filter((endorsement) => getPremiumDelta(endorsement) < 0).length,
        color: "text-info",
      },
    ],
    [endorsements],
  );

   const policyNumberShort = (policyNumber: string) => {
     if (policyNumber.length <= 18) {
       return policyNumber;
     }

     return `${policyNumber.slice(0, 12)}-${policyNumber.slice(12, 18)}`;
   };

  return (
    <div className="space-y-6">
      <PageHeader title="Endorsements" description={role === "admin" ? "Portfolio-wide policy modification history" : "Track policy modifications and version changes across your book"} />

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {summary.map((item, index) => (
          <motion.div
            key={item.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.05 }}
            className="glass-card p-4"
          >
            <p className="text-xs text-muted-foreground">{item.label}</p>
            <p className={cn("text-xl font-bold mt-1", item.color)}>{item.value}</p>
          </motion.div>
        ))}
      </div>

      <div className="glass-card p-4">
        <div className="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search endorsements..."
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5 overflow-x-auto">
            {statusTabs.map((status) => (
              <button
                key={status}
                onClick={() => setTab(status)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize whitespace-nowrap",
                  tab === status ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
                )}
              >
                {status}
              </button>
            ))}
          </div>
          <button
            type="button"
            onClick={() => void refetch()}
            className="h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors"
          >
            Refresh
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="rounded-lg border border-border bg-card p-4 text-sm text-muted-foreground">Loading endorsements...</div>
      ) : isError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive flex items-center gap-2">
          <AlertCircle className="h-4 w-4" />
          <span>{error instanceof Error ? error.message : "Failed to load endorsements."}</span>
        </div>
      ) : filtered.length === 0 ? (
        <EmptyState
          icon={GitBranch}
          title="No endorsements found"
          description={endorsements.length === 0 ? "No endorsements have been created yet." : "No endorsements match the current filters."}
        />
      ) : (
        <div className="space-y-4">
          {filtered.map((endorsement, index) => {
            const status = normalizePolicyStatus(endorsement.policyStatus ?? "draft");
            const premiumDelta = getPremiumDelta(endorsement);

            return (
              <motion.div
                key={endorsement.id}
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: index * 0.1 }}
                className="glass-card-hover p-5 relative"
              >
                {index < filtered.length - 1 && (
                  <div className="absolute left-[29px] top-[60px] bottom-[-20px] w-px bg-border" />
                )}

                <div className="flex items-start gap-4">
                  <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center shrink-0 relative z-10">
                    <GitBranch className="h-5 w-5 text-primary" />
                  </div>

                  <div className="flex-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <Link to={`/policies/${endorsement.policyId}`} className="font-mono text-xs font-medium hover:text-primary transition-colors">
                        {policyNumberShort(endorsement.policyNumber ?? "Policy")}
                      </Link>
                      <span className="flex items-center gap-1 text-xs text-muted-foreground">
                        v{endorsement.oldVersionNumber ?? Math.max(0, endorsement.versionNumber - 1)} <ArrowRight className="h-3 w-3" /> v{endorsement.versionNumber}
                      </span>
                      <StatusChip status={status} />
                    </div>
                    <p className="text-sm font-medium mt-1">{endorsement.clientName ?? "Unknown client"}</p>
                    <p className="text-xs text-muted-foreground mt-0.5">{endorsement.reason ?? formatEndorsementType(endorsement.endorsementType)}</p>
                    <p className="text-xs text-muted-foreground mt-1">{formatEndorsementType(endorsement.endorsementType)}</p>

                    <div className="flex items-center gap-6 mt-3 text-xs flex-wrap">
                      <div>
                        <span className="text-muted-foreground">Effective: </span>
                        <span className="font-medium">{formatDateOnly(endorsement.effectiveDate)}</span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Premium Impact: </span>
                        <span className={cn("font-semibold", premiumDelta > 0 ? "text-warning" : premiumDelta < 0 ? "text-success" : "text-muted-foreground")}>
                          {premiumDelta > 0 ? "+" : premiumDelta < 0 ? "-" : ""}
                          {formatMoney(Math.abs(premiumDelta), endorsement.currencyCode ?? "RON")}
                        </span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Created by: </span>
                        <span className="font-medium">{endorsement.createdBy}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </motion.div>
            );
          })}
        </div>
      )}
    </div>
  );
}
