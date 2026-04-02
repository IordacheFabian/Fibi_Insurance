import { useState } from "react";
import { motion } from "framer-motion";
import { Plus, Search, Building2, MapPin } from "lucide-react";
import { PageHeader } from "@/components/ui/PageHeader";
import { StatusChip } from "@/components/ui/StatusChip";
import { buildings } from "@/data/sampleData";

export default function BuildingsPage() {
  const [search, setSearch] = useState("");

  const filtered = buildings.filter((b) =>
    b.name.toLowerCase().includes(search.toLowerCase()) ||
    b.city.toLowerCase().includes(search.toLowerCase()) ||
    b.clientName.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <PageHeader title="Buildings" description="Manage insured properties">
        <button className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
          <Plus className="h-4 w-4" /> Register Building
        </button>
      </PageHeader>

      <div className="glass-card p-4">
        <div className="relative max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search buildings..."
            className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        {filtered.map((b, i) => (
          <motion.div
            key={b.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.05 }}
            className="glass-card-hover p-5 cursor-pointer"
          >
            <div className="flex items-start justify-between mb-3">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center">
                  <Building2 className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <h3 className="text-sm font-semibold">{b.name}</h3>
                  <p className="text-xs text-muted-foreground">{b.clientName}</p>
                </div>
              </div>
              <StatusChip status={b.riskLevel} />
            </div>

            <div className="space-y-2 mb-4">
              <div className="flex items-center gap-2 text-xs text-muted-foreground">
                <MapPin className="h-3 w-3" /> {b.address}, {b.city}
              </div>
              <div className="flex items-center justify-between text-xs">
                <span className="text-muted-foreground capitalize">{b.type}</span>
                <span className="text-muted-foreground">Built {b.yearBuilt}</span>
              </div>
            </div>

            <div className="pt-3 border-t border-border/50 flex items-center justify-between">
              <div>
                <p className="text-xs text-muted-foreground">Insured Value</p>
                <p className="text-sm font-bold">£{b.insuredValue.toLocaleString()}</p>
              </div>
              <StatusChip status={b.policyStatus} />
            </div>
          </motion.div>
        ))}
      </div>
    </div>
  );
}
