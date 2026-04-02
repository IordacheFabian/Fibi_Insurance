import { useState } from "react";
import { motion } from "framer-motion";
import { Plus, Search, CreditCard, Calendar } from "lucide-react";
import { PageHeader } from "@/components/ui/PageHeader";
import { StatusChip } from "@/components/ui/StatusChip";
import { payments } from "@/data/sampleData";
import { cn } from "@/lib/utils";

const statusTabs = ["all", "paid", "pending", "overdue", "partial"] as const;

export default function PaymentsPage() {
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof statusTabs[number]>("all");

  const filtered = payments.filter((p) => {
    const matchesSearch = p.policyNumber.toLowerCase().includes(search.toLowerCase()) || p.clientName.toLowerCase().includes(search.toLowerCase());
    const matchesTab = tab === "all" || p.status === tab;
    return matchesSearch && matchesTab;
  });

  const totalDue = payments.filter(p => p.status !== "paid").reduce((s, p) => s + p.amount, 0);
  const totalPaid = payments.filter(p => p.status === "paid").reduce((s, p) => s + p.amount, 0);

  return (
    <div className="space-y-6">
      <PageHeader title="Payments" description="Track premiums, balances, and transactions">
        <button className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-success text-success-foreground text-sm font-medium hover:opacity-90 transition-opacity">
          <Plus className="h-4 w-4" /> Record Payment
        </button>
      </PageHeader>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {[
          { label: "Total Collected", value: `£${totalPaid.toLocaleString()}`, color: "text-success" },
          { label: "Outstanding", value: `£${totalDue.toLocaleString()}`, color: "text-warning" },
          { label: "Overdue", value: `£${payments.filter(p => p.status === "overdue").reduce((s, p) => s + p.amount, 0).toLocaleString()}`, color: "text-destructive" },
          { label: "Transactions", value: payments.length.toString(), color: "text-foreground" },
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
              placeholder="Search payments..."
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
                <th>Client</th>
                <th>Policy</th>
                <th>Amount</th>
                <th>Due Date</th>
                <th>Paid Date</th>
                <th>Method</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((pay, i) => (
                <motion.tr
                  key={pay.id}
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  transition={{ delay: i * 0.03 }}
                  className="cursor-pointer"
                >
                  <td className="text-sm font-medium">{pay.clientName}</td>
                  <td className="font-mono text-xs">{pay.policyNumber}</td>
                  <td className="text-sm font-semibold">£{pay.amount.toLocaleString()}</td>
                  <td className="text-xs text-muted-foreground">{new Date(pay.dueDate).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" })}</td>
                  <td className="text-xs text-muted-foreground">{pay.paidDate ? new Date(pay.paidDate).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" }) : "—"}</td>
                  <td className="text-xs">{pay.method}</td>
                  <td><StatusChip status={pay.status} /></td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>
      </motion.div>
    </div>
  );
}
