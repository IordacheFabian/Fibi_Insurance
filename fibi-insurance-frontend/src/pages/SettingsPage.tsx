import { motion } from "framer-motion";
import { PageHeader } from "@/components/ui/PageHeader";
import { Settings, DollarSign, AlertTriangle, Shield, Globe } from "lucide-react";
import { cn } from "@/lib/utils";
import { useState } from "react";

const tabs = ["currencies", "fees", "risk_factors", "roles"] as const;

const currencies = [
  { code: "GBP", name: "British Pound", symbol: "£", active: true },
  { code: "USD", name: "US Dollar", symbol: "$", active: true },
  { code: "EUR", name: "Euro", symbol: "€", active: true },
  { code: "CHF", name: "Swiss Franc", symbol: "CHF", active: false },
];

const fees = [
  { name: "Admin Fee", percentage: 2.5, flat: 50, applies: "All policies" },
  { name: "Broker Commission", percentage: 10, flat: 0, applies: "All policies" },
  { name: "Stamp Duty", percentage: 0.5, flat: 0, applies: "Commercial only" },
  { name: "Processing Fee", percentage: 0, flat: 25, applies: "Endorsements" },
];

const riskFactors = [
  { name: "Building Age", weight: 1.5, category: "Property" },
  { name: "Flood Zone", weight: 2.0, category: "Location" },
  { name: "Construction Type", weight: 1.2, category: "Property" },
  { name: "Claims History", weight: 1.8, category: "History" },
  { name: "Security Features", weight: 0.8, category: "Property" },
];

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState<typeof tabs[number]>("currencies");

  return (
    <div className="space-y-6">
      <PageHeader title="Settings" description="Configure system parameters and metadata" />

      <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5 w-fit">
        {tabs.map((t) => (
          <button
            key={t}
            onClick={() => setActiveTab(t)}
            className={cn(
              "px-4 py-2 rounded-md text-sm font-medium transition-all capitalize",
              activeTab === t ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground"
            )}
          >
            {t.replace("_", " ")}
          </button>
        ))}
      </div>

      {activeTab === "currencies" && (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="glass-card overflow-hidden">
          <table className="table-premium">
            <thead><tr><th>Code</th><th>Currency</th><th>Symbol</th><th>Status</th></tr></thead>
            <tbody>
              {currencies.map((c) => (
                <tr key={c.code}>
                  <td className="font-mono font-semibold text-sm">{c.code}</td>
                  <td className="text-sm">{c.name}</td>
                  <td className="text-lg">{c.symbol}</td>
                  <td>
                    <span className={cn(
                      "status-chip",
                      c.active ? "bg-success/10 text-success" : "bg-muted text-muted-foreground"
                    )}>
                      <span className={cn("h-1.5 w-1.5 rounded-full", c.active ? "bg-success" : "bg-muted-foreground")} />
                      {c.active ? "Active" : "Inactive"}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </motion.div>
      )}

      {activeTab === "fees" && (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="glass-card overflow-hidden">
          <table className="table-premium">
            <thead><tr><th>Fee Name</th><th>Percentage</th><th>Flat Amount</th><th>Applies To</th></tr></thead>
            <tbody>
              {fees.map((f) => (
                <tr key={f.name}>
                  <td className="text-sm font-medium">{f.name}</td>
                  <td className="font-mono text-sm">{f.percentage}%</td>
                  <td className="font-mono text-sm">{f.flat > 0 ? `£${f.flat}` : "—"}</td>
                  <td className="text-xs text-muted-foreground">{f.applies}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </motion.div>
      )}

      {activeTab === "risk_factors" && (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="grid grid-cols-1 md:grid-cols-2 gap-3">
          {riskFactors.map((r, i) => (
            <div key={r.name} className="glass-card p-4 flex items-center gap-4">
              <div className="h-10 w-10 rounded-xl bg-warning/10 flex items-center justify-center">
                <AlertTriangle className="h-5 w-5 text-warning" />
              </div>
              <div className="flex-1">
                <p className="text-sm font-medium">{r.name}</p>
                <p className="text-xs text-muted-foreground">{r.category}</p>
              </div>
              <div className="text-right">
                <p className="text-lg font-bold">{r.weight}x</p>
                <p className="text-[10px] text-muted-foreground uppercase">Weight</p>
              </div>
            </div>
          ))}
        </motion.div>
      )}

      {activeTab === "roles" && (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {[
            { role: "Broker", permissions: ["Manage clients", "Create policies", "Submit claims", "Record payments", "View reports"], color: "primary" },
            { role: "Administrator", permissions: ["All broker permissions", "Manage brokers", "Approve claims", "Configure settings", "Portfolio oversight"], color: "warning" },
          ].map((r) => (
            <div key={r.role} className="glass-card p-6">
              <div className="flex items-center gap-3 mb-4">
                <div className={cn("h-10 w-10 rounded-xl flex items-center justify-center", r.color === "primary" ? "bg-primary/10" : "bg-warning/10")}>
                  <Shield className={cn("h-5 w-5", r.color === "primary" ? "text-primary" : "text-warning")} />
                </div>
                <h3 className="font-semibold">{r.role}</h3>
              </div>
              <ul className="space-y-2">
                {r.permissions.map((p) => (
                  <li key={p} className="flex items-center gap-2 text-sm text-muted-foreground">
                    <span className="h-1 w-1 rounded-full bg-muted-foreground" />
                    {p}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </motion.div>
      )}
    </div>
  );
}
