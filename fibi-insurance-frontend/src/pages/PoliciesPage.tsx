import { useState } from "react";
import { motion } from "framer-motion";
import { Plus, Search, FileText, Calendar, User } from "lucide-react";
import { PageHeader } from "@/components/ui/PageHeader";
import { StatusChip } from "@/components/ui/StatusChip";
import { policies } from "@/data/sampleData";
import { cn } from "@/lib/utils";

const statusTabs = ["all", "active", "draft", "expired", "cancelled"] as const;

export default function PoliciesPage() {
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof statusTabs[number]>("all");

  const filtered = policies.filter((p) => {
    const matchesSearch = p.policyNumber.toLowerCase().includes(search.toLowerCase()) || p.clientName.toLowerCase().includes(search.toLowerCase());
    const matchesTab = tab === "all" || p.status === tab;
    return matchesSearch && matchesTab;
  });

  return (
    <div className="space-y-6">
      <PageHeader title="Policies" description="Manage insurance policies and coverage">
        <button className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
          <Plus className="h-4 w-4" /> Create Policy
        </button>
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
      </div>

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
                <th>Type</th>
                <th>Premium</th>
                <th>Coverage</th>
                <th>Version</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((policy, i) => (
                <motion.tr
                  key={policy.id}
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.03 }}
                  className="cursor-pointer"
                >
                  <td>
                    <div className="flex items-center gap-2">
                      <FileText className="h-4 w-4 text-primary" />
                      <span className="font-mono text-xs font-medium">{policy.policyNumber}</span>
                    </div>
                  </td>
                  <td className="text-sm">{policy.clientName}</td>
                  <td className="text-xs text-muted-foreground">{policy.buildingName}</td>
                  <td className="text-xs">{policy.type}</td>
                  <td className="text-sm font-semibold">£{policy.premium.toLocaleString()}</td>
                  <td>
                    <div className="flex items-center gap-1 text-xs text-muted-foreground">
                      <Calendar className="h-3 w-3" />
                      {new Date(policy.coverageStart).toLocaleDateString("en-GB", { month: "short", year: "numeric" })} – {new Date(policy.coverageEnd).toLocaleDateString("en-GB", { month: "short", year: "numeric" })}
                    </div>
                  </td>
                  <td>
                    <span className="inline-flex items-center justify-center h-5 w-5 rounded-full bg-muted text-[10px] font-bold">
                      v{policy.version}
                    </span>
                  </td>
                  <td><StatusChip status={policy.status} /></td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>
      </motion.div>
    </div>
  );
}
