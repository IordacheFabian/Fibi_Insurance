import { useDeferredValue, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { motion } from "framer-motion";
import { AlertCircle, Calendar, FileText, Plus, Search } from "lucide-react";
import { Link } from "react-router-dom";
import { PageHeader } from "@/components/ui/PageHeader";
import { EmptyState } from "@/components/ui/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";
import { StatusChip } from "@/components/ui/StatusChip";
import { getPolicies, normalizePolicyStatus } from "@/lib/policies/policy.api";
import { cn, formatMoney } from "@/lib/utils";

const statusTabs = ["all", "active", "draft", "expired", "cancelled"] as const;

function formatDateOnly(value: string) {
  const [year, month, day] = value.split("-").map(Number);

  if (!year || !month || !day) {
    return value;
  }

  return new Date(year, month - 1, day).toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

export default function PoliciesPage() {
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof statusTabs[number]>("all");
  const deferredSearch = useDeferredValue(search);

  const { data: policies = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ["policies", tab],
    queryFn: () => getPolicies(tab === "all" ? {} : { policyStatus: tab }),
    staleTime: 30000,
  });

  const filtered = useMemo(() => {
    const normalizedSearch = deferredSearch.trim().toLowerCase();

    return policies.filter((policy) => {
      if (!normalizedSearch) {
        return true;
      }

      const buildingAddress = `${policy.buildingStreet} ${policy.buildingNumber}`.trim().toLowerCase();

      return policy.policyNumber.toLowerCase().includes(normalizedSearch)
        || policy.clientName.toLowerCase().includes(normalizedSearch)
        || policy.cityName.toLowerCase().includes(normalizedSearch)
        || buildingAddress.includes(normalizedSearch)
        || policy.currencyCode.toLowerCase().includes(normalizedSearch);
    });
  }, [deferredSearch, policies]);

  const policyNumberShort = (policyNumber: string) => {
    if (policyNumber.length <= 18) {
      return policyNumber;
    }

    return `${policyNumber.slice(0, 12)}-${policyNumber.slice(12, 18)}`;
  };

  return (
    <div className="space-y-6">
      <PageHeader title="Policies" description="Manage insurance policies and coverage">
        <Link to="/policies/new" className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
          <Plus className="h-4 w-4" /> Create Policy
        </Link>
      </PageHeader>

      <div className="glass-card p-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search policies..."
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5">
            {statusTabs.map((t) => (
              <button
                key={t}
                onClick={() => setTab(t)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize",
                  tab === t ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground"
                )}
              >
                {t}
              </button>
            ))}
          </div>
        </div>
        <p className="mt-3 text-xs text-muted-foreground">Showing {filtered.length} of {policies.length} policies</p>
      </div>

      {isLoading ? (
        <div className="glass-card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="table-premium">
              <thead>
                <tr>
                  <th>Policy</th>
                  <th>Client</th>
                  <th>Building</th>
                  <th>City</th>
                  <th>Base Premium</th>
                  <th>Final Premium</th>
                  <th>Coverage</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {Array.from({ length: 6 }).map((_, index) => (
                  <tr key={index}>
                    <td><Skeleton className="h-4 w-28" /></td>
                    <td><Skeleton className="h-4 w-32" /></td>
                    <td><Skeleton className="h-4 w-36" /></td>
                    <td><Skeleton className="h-4 w-20" /></td>
                    <td><Skeleton className="h-4 w-24" /></td>
                    <td><Skeleton className="h-4 w-24" /></td>
                    <td><Skeleton className="h-4 w-40" /></td>
                    <td><Skeleton className="h-6 w-20 rounded-full" /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : isError ? (
        <div className="flex flex-col items-center justify-center gap-4 px-6 py-16 text-center glass-card">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
            <AlertCircle className="h-8 w-8" />
          </div>
          <div>
            <h3 className="text-lg font-semibold">Could not load policies</h3>
            <p className="text-sm text-muted-foreground mt-1 max-w-md">
              {error instanceof Error ? error.message : "The policy list request failed."}
            </p>
          </div>
          <button
            type="button"
            onClick={() => refetch()}
            className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors"
          >
            Try again
          </button>
        </div>
      ) : filtered.length === 0 ? (
        <EmptyState
          icon={FileText}
          title="No policies found"
          description={search.trim() ? "Try a different search term or switch to another status." : "No policies are available for this broker yet."}
        />
      ) : (
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass-card overflow-hidden"
        >
          <div className="overflow-x-auto">
            <table className="table-premium">
              <thead>
                <tr>
                  <th>Policy</th>
                  <th>Client</th>
                  <th>Building</th>
                  <th>City</th>
                  <th>Base Premium</th>
                  <th>Final Premium</th>
                  <th>Coverage</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((policy, index) => (
                  <motion.tr
                    key={policy.id}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.03 }}
                    className="hover:bg-muted/20 transition-colors"
                  >
                    <td>
                      <Link to={`/policies/${policy.id}`} className="flex items-center gap-2 hover:text-primary transition-colors">
                        <FileText className="h-4 w-4 text-primary" />
                        <span className="font-mono text-xs font-medium">{policyNumberShort(policy.policyNumber)}</span>
                      </Link>
                    </td>
                    <td className="text-sm">{policy.clientName}</td>
                    <td className="text-xs text-muted-foreground">{`${policy.buildingStreet} ${policy.buildingNumber}`.trim()}</td>
                    <td className="text-xs">{policy.cityName}</td>
                    <td className="text-sm font-semibold">{formatMoney(policy.basePremium, policy.currencyCode)}</td>
                    <td className="text-sm font-semibold">{formatMoney(policy.finalPremium, policy.currencyCode)}</td>
                    <td>
                      <div className="flex items-center gap-1 text-xs text-muted-foreground whitespace-nowrap">
                        <Calendar className="h-3 w-3" />
                        {formatDateOnly(policy.startDate)} - {formatDateOnly(policy.endDate)}
                      </div>
                    </td>
                    <td><StatusChip status={normalizePolicyStatus(policy.policyStatus)} /></td>
                  </motion.tr>
                ))}
              </tbody>
            </table>
          </div>
        </motion.div>
      )}
    </div>
  );
}
