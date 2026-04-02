import { motion } from "framer-motion";
import {
  FileText, ShieldAlert, CreditCard, Users, TrendingUp,
  AlertTriangle, Activity, Zap, Building2, UserCog, BarChart3, DollarSign,
} from "lucide-react";
import {
  AreaChart, Area, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from "recharts";
import { useRole } from "@/contexts/RoleContext";
import { KpiCard } from "@/components/ui/KpiCard";
import { StatusChip } from "@/components/ui/StatusChip";
import { PageHeader } from "@/components/ui/PageHeader";
import { recentActivities, chartDataMonthly, geographicData, brokers, claims, payments } from "@/data/sampleData";

const container = {
  hidden: {},
  show: { transition: { staggerChildren: 0.08 } },
};

const item = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0 },
};

const COLORS = ["hsl(217, 91%, 60%)", "hsl(199, 89%, 48%)", "hsl(160, 84%, 39%)", "hsl(38, 92%, 50%)", "hsl(271, 81%, 56%)", "hsl(0, 72%, 51%)", "hsl(180, 70%, 45%)", "hsl(30, 80%, 55%)"];

const policyDistribution = [
  { name: "Commercial", value: 35 },
  { name: "Residential", value: 28 },
  { name: "Industrial", value: 22 },
  { name: "Mixed-Use", value: 15 },
];

export default function DashboardPage() {
  const { role } = useRole();

  return (
    <div className="space-y-6">
      <PageHeader
        title={role === "admin" ? "Admin Dashboard" : "Broker Dashboard"}
        description={role === "admin" ? "Portfolio-wide overview and key performance indicators" : "Your portfolio summary and recent activity"}
      />

      {/* KPI Cards */}
      <motion.div
        variants={container}
        initial="hidden"
        animate="show"
        className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4"
      >
        {role === "broker" ? (
          <>
            <KpiCard title="Active Policies" value="18" change="+3 this month" changeType="positive" icon={FileText} gradient="gradient-primary" delay={0} />
            <KpiCard title="Pending Claims" value="3" change="2 under review" changeType="neutral" icon={ShieldAlert} gradient="gradient-warning" delay={0.1} />
            <KpiCard title="Overdue Payments" value="£16,875" change="1 client" changeType="negative" icon={CreditCard} gradient="gradient-danger" delay={0.2} />
            <KpiCard title="Total Premiums" value="£217,200" change="+12% vs last year" changeType="positive" icon={TrendingUp} gradient="gradient-success" delay={0.3} />
          </>
        ) : (
          <>
            <KpiCard title="Total Policies" value="136" change="+18 this quarter" changeType="positive" icon={FileText} gradient="gradient-primary" delay={0} />
            <KpiCard title="Active Brokers" value="3" change="All performing" changeType="positive" icon={UserCog} gradient="gradient-accent" delay={0.1} />
            <KpiCard title="Claims Pending" value="5" change="2 need approval" changeType="neutral" icon={ShieldAlert} gradient="gradient-warning" delay={0.2} />
            <KpiCard title="Premium Revenue" value="£2.95M" change="+15% YoY" changeType="positive" icon={DollarSign} gradient="gradient-success" delay={0.3} />
          </>
        )}
      </motion.div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Premium vs Claims trend */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="lg:col-span-2 glass-card p-6"
        >
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className="text-sm font-semibold">Premium vs Claims Trend</h3>
              <p className="text-xs text-muted-foreground mt-0.5">Monthly comparison for 2025</p>
            </div>
            <div className="flex gap-4 text-xs">
              <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-primary" /> Premiums</span>
              <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-destructive" /> Claims</span>
            </div>
          </div>
          <ResponsiveContainer width="100%" height={280}>
            <AreaChart data={chartDataMonthly}>
              <defs>
                <linearGradient id="premGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="hsl(217, 91%, 60%)" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="hsl(217, 91%, 60%)" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="claimGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="hsl(0, 72%, 51%)" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="hsl(0, 72%, 51%)" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} tickFormatter={(v) => `£${(v / 1000).toFixed(0)}k`} />
              <Tooltip
                contentStyle={{
                  backgroundColor: "hsl(223, 40%, 10%)",
                  border: "1px solid hsl(217, 33%, 17%)",
                  borderRadius: "8px",
                  fontSize: "12px",
                }}
                formatter={(v: number) => [`£${v.toLocaleString()}`, ""]}
              />
              <Area type="monotone" dataKey="premiums" stroke="hsl(217, 91%, 60%)" fill="url(#premGrad)" strokeWidth={2} />
              <Area type="monotone" dataKey="claims" stroke="hsl(0, 72%, 51%)" fill="url(#claimGrad)" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </motion.div>

        {/* Policy Distribution */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="glass-card p-6"
        >
          <h3 className="text-sm font-semibold mb-1">Policy Distribution</h3>
          <p className="text-xs text-muted-foreground mb-4">By property type</p>
          <ResponsiveContainer width="100%" height={200}>
            <PieChart>
              <Pie data={policyDistribution} cx="50%" cy="50%" innerRadius={55} outerRadius={80} paddingAngle={4} dataKey="value" stroke="none">
                {policyDistribution.map((_, idx) => (
                  <Cell key={idx} fill={COLORS[idx]} />
                ))}
              </Pie>
              <Tooltip
                contentStyle={{
                  backgroundColor: "hsl(223, 40%, 10%)",
                  border: "1px solid hsl(217, 33%, 17%)",
                  borderRadius: "8px",
                  fontSize: "12px",
                }}
              />
            </PieChart>
          </ResponsiveContainer>
          <div className="grid grid-cols-2 gap-2 mt-2">
            {policyDistribution.map((d, i) => (
              <div key={d.name} className="flex items-center gap-2 text-xs">
                <span className="h-2 w-2 rounded-full" style={{ backgroundColor: COLORS[i] }} />
                <span className="text-muted-foreground">{d.name}</span>
                <span className="ml-auto font-medium">{d.value}%</span>
              </div>
            ))}
          </div>
        </motion.div>
      </div>

      {/* Bottom section */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Recent Activity */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="lg:col-span-2 glass-card p-6"
        >
          <h3 className="text-sm font-semibold mb-4">Recent Activity</h3>
          <div className="space-y-0">
            {recentActivities.slice(0, 6).map((act, i) => (
              <div key={act.id} className="flex items-start gap-3 py-3 border-b border-border/50 last:border-0">
                <div className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-muted">
                  {act.type === "policy" && <FileText className="h-4 w-4 text-primary" />}
                  {act.type === "claim" && <ShieldAlert className="h-4 w-4 text-warning" />}
                  {act.type === "payment" && <CreditCard className="h-4 w-4 text-success" />}
                  {act.type === "client" && <Users className="h-4 w-4 text-info" />}
                  {act.type === "endorsement" && <Activity className="h-4 w-4 text-primary" />}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium">{act.title}</p>
                  <p className="text-xs text-muted-foreground truncate">{act.description}</p>
                </div>
                <span className="text-xs text-muted-foreground whitespace-nowrap">{act.time}</span>
              </div>
            ))}
          </div>
        </motion.div>

        {/* Quick Actions / Alerts */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.6 }}
          className="space-y-4"
        >
          {/* Quick Actions */}
          <div className="glass-card p-6">
            <h3 className="text-sm font-semibold mb-3">Quick Actions</h3>
            <div className="grid grid-cols-2 gap-2">
              {[
                { label: "New Policy", icon: FileText, gradient: "gradient-primary" },
                { label: "Add Client", icon: Users, gradient: "gradient-accent" },
                { label: "Submit Claim", icon: ShieldAlert, gradient: "gradient-warning" },
                { label: "Record Payment", icon: CreditCard, gradient: "gradient-success" },
              ].map((action) => (
                <button
                  key={action.label}
                  className="flex flex-col items-center gap-2 p-3 rounded-xl border border-border hover:border-primary/30 hover:bg-muted/50 transition-all duration-200 group"
                >
                  <div className={`h-9 w-9 rounded-lg ${action.gradient} flex items-center justify-center group-hover:scale-110 transition-transform`}>
                    <action.icon className="h-4 w-4 text-primary-foreground" />
                  </div>
                  <span className="text-xs font-medium">{action.label}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Alerts */}
          <div className="glass-card p-6">
            <h3 className="text-sm font-semibold mb-3 flex items-center gap-2">
              <AlertTriangle className="h-4 w-4 text-warning" />
              Alerts
            </h3>
            <div className="space-y-2">
              <div className="flex items-center gap-2 p-2.5 rounded-lg bg-destructive/5 border border-destructive/10 text-xs">
                <span className="h-1.5 w-1.5 rounded-full bg-destructive animate-pulse" />
                <span>1 payment overdue — Crown Estates</span>
              </div>
              <div className="flex items-center gap-2 p-2.5 rounded-lg bg-warning/5 border border-warning/10 text-xs">
                <span className="h-1.5 w-1.5 rounded-full bg-warning" />
                <span>2 claims awaiting review</span>
              </div>
              <div className="flex items-center gap-2 p-2.5 rounded-lg bg-info/5 border border-info/10 text-xs">
                <span className="h-1.5 w-1.5 rounded-full bg-info" />
                <span>1 policy draft pending activation</span>
              </div>
            </div>
          </div>
        </motion.div>
      </div>

      {/* Admin: Geographic & Broker performance */}
      {role === "admin" && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* Geographic */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.7 }}
            className="glass-card p-6"
          >
            <h3 className="text-sm font-semibold mb-1">Geographic Distribution</h3>
            <p className="text-xs text-muted-foreground mb-4">Policies by region</p>
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={geographicData} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} horizontal={false} />
                <XAxis type="number" tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
                <YAxis dataKey="region" type="category" tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} width={90} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(223, 40%, 10%)",
                    border: "1px solid hsl(217, 33%, 17%)",
                    borderRadius: "8px",
                    fontSize: "12px",
                  }}
                />
                <Bar dataKey="policies" fill="hsl(217, 91%, 60%)" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </motion.div>

          {/* Broker performance */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.8 }}
            className="glass-card p-6"
          >
            <h3 className="text-sm font-semibold mb-4">Broker Performance</h3>
            <div className="space-y-4">
              {brokers.map((broker) => (
                <div key={broker.id} className="flex items-center gap-4 p-3 rounded-xl border border-border hover:border-primary/20 transition-colors">
                  <div className="h-10 w-10 rounded-full gradient-primary flex items-center justify-center text-xs font-bold text-primary-foreground">
                    {broker.name.split(" ").map(n => n[0]).join("")}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium">{broker.name}</p>
                    <p className="text-xs text-muted-foreground">{broker.activePolicies} active policies · £{(broker.premiumTotal / 1000).toFixed(0)}k premiums</p>
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold text-primary">{broker.performance}%</p>
                    <p className="text-[10px] text-muted-foreground uppercase tracking-wider">Score</p>
                  </div>
                </div>
              ))}
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
}
