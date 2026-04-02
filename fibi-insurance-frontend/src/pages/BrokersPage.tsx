import { motion } from "framer-motion";
import { PageHeader } from "@/components/ui/PageHeader";
import { brokers } from "@/data/sampleData";
import { Plus, UserCog, TrendingUp } from "lucide-react";

export default function BrokersPage() {
  return (
    <div className="space-y-6">
      <PageHeader title="Brokers" description="Manage broker accounts and performance">
        <button className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
          <Plus className="h-4 w-4" /> Add Broker
        </button>
      </PageHeader>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        {brokers.map((broker, i) => (
          <motion.div
            key={broker.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.1 }}
            className="glass-card-hover p-6 cursor-pointer"
          >
            <div className="flex items-start gap-4 mb-4">
              <div className="h-12 w-12 rounded-full gradient-primary flex items-center justify-center text-sm font-bold text-primary-foreground">
                {broker.name.split(" ").map(n => n[0]).join("")}
              </div>
              <div className="flex-1">
                <h3 className="font-semibold">{broker.name}</h3>
                <p className="text-xs text-muted-foreground">{broker.email}</p>
              </div>
              <div className="text-right">
                <p className="text-2xl font-bold text-primary">{broker.performance}%</p>
                <p className="text-[10px] text-muted-foreground uppercase tracking-wider">Score</p>
              </div>
            </div>

            <div className="grid grid-cols-3 gap-3 pt-4 border-t border-border/50">
              <div>
                <p className="text-xs text-muted-foreground">Clients</p>
                <p className="text-sm font-bold">{broker.clients}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Active Policies</p>
                <p className="text-sm font-bold">{broker.activePolicies}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Premiums</p>
                <p className="text-sm font-bold">£{(broker.premiumTotal / 1000).toFixed(0)}k</p>
              </div>
            </div>

            <div className="mt-4 pt-3 border-t border-border/50">
              <div className="flex items-center justify-between text-xs">
                <span className="text-muted-foreground">Performance</span>
                <span className="font-medium">{broker.performance}%</span>
              </div>
              <div className="mt-2 h-1.5 rounded-full bg-muted overflow-hidden">
                <motion.div
                  initial={{ width: 0 }}
                  animate={{ width: `${broker.performance}%` }}
                  transition={{ duration: 1, delay: 0.5 + i * 0.1 }}
                  className="h-full rounded-full gradient-primary"
                />
              </div>
            </div>
          </motion.div>
        ))}
      </div>
    </div>
  );
}
