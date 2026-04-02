import { motion } from "framer-motion";
import {
  BarChart, Bar, AreaChart, Area, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, PieChart, Pie, Cell,
} from "recharts";
import { PageHeader } from "@/components/ui/PageHeader";
import { chartDataMonthly, geographicData } from "@/data/sampleData";
import { Download, Calendar } from "lucide-react";

const COLORS = ["hsl(217, 91%, 60%)", "hsl(199, 89%, 48%)", "hsl(160, 84%, 39%)", "hsl(38, 92%, 50%)", "hsl(271, 81%, 56%)", "hsl(0, 72%, 51%)", "hsl(180, 70%, 45%)", "hsl(30, 80%, 55%)"];

const claimsBreakdown = [
  { name: "Water Damage", value: 32 },
  { name: "Fire", value: 18 },
  { name: "Storm", value: 25 },
  { name: "Theft", value: 12 },
  { name: "Subsidence", value: 13 },
];

const tooltipStyle = {
  backgroundColor: "hsl(223, 40%, 10%)",
  border: "1px solid hsl(217, 33%, 17%)",
  borderRadius: "8px",
  fontSize: "12px",
};

export default function ReportsPage() {
  return (
    <div className="space-y-6">
      <PageHeader title="Reports & Analytics" description="Portfolio insights and performance metrics">
        <div className="flex items-center gap-2">
          <button className="flex items-center gap-2 h-9 px-3 rounded-lg border border-border text-sm hover:bg-muted transition-colors">
            <Calendar className="h-4 w-4" /> Last 12 Months
          </button>
          <button className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
            <Download className="h-4 w-4" /> Export
          </button>
        </div>
      </PageHeader>

      {/* Executive Summary */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {[
          { label: "Total Premium Revenue", value: "£2.95M", change: "+15% YoY" },
          { label: "Claims Ratio", value: "28.4%", change: "-3.2% YoY" },
          { label: "Portfolio Growth", value: "+18%", change: "136 total policies" },
          { label: "Collection Rate", value: "94.2%", change: "+2.1% vs target" },
        ].map((s, i) => (
          <motion.div
            key={s.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.05 }}
            className="glass-card p-5"
          >
            <p className="text-xs text-muted-foreground">{s.label}</p>
            <p className="text-2xl font-bold mt-1">{s.value}</p>
            <p className="text-xs text-success mt-1">{s.change}</p>
          </motion.div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Premium collection trend */}
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">Premium Collection Trend</h3>
          <p className="text-xs text-muted-foreground mb-4">Monthly premiums collected</p>
          <ResponsiveContainer width="100%" height={250}>
            <AreaChart data={chartDataMonthly}>
              <defs>
                <linearGradient id="rpGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="hsl(160, 84%, 39%)" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="hsl(160, 84%, 39%)" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} tickFormatter={(v) => `£${(v / 1000).toFixed(0)}k`} />
              <Tooltip contentStyle={tooltipStyle} formatter={(v: number) => [`£${v.toLocaleString()}`, ""]} />
              <Area type="monotone" dataKey="premiums" stroke="hsl(160, 84%, 39%)" fill="url(#rpGrad)" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </motion.div>

        {/* Geographic */}
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">Premium by Region</h3>
          <p className="text-xs text-muted-foreground mb-4">Geographic distribution</p>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={geographicData}>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="region" tick={{ fontSize: 10, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} tickFormatter={(v) => `£${(v / 1000000).toFixed(1)}M`} />
              <Tooltip contentStyle={tooltipStyle} formatter={(v: number) => [`£${v.toLocaleString()}`, ""]} />
              <Bar dataKey="premium" fill="hsl(217, 91%, 60%)" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </motion.div>

        {/* Claims breakdown */}
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">Claims by Type</h3>
          <p className="text-xs text-muted-foreground mb-4">Distribution of claim categories</p>
          <div className="flex items-center gap-8">
            <ResponsiveContainer width="50%" height={200}>
              <PieChart>
                <Pie data={claimsBreakdown} cx="50%" cy="50%" innerRadius={50} outerRadius={75} paddingAngle={3} dataKey="value" stroke="none">
                  {claimsBreakdown.map((_, i) => <Cell key={i} fill={COLORS[i]} />)}
                </Pie>
                <Tooltip contentStyle={tooltipStyle} />
              </PieChart>
            </ResponsiveContainer>
            <div className="space-y-2">
              {claimsBreakdown.map((d, i) => (
                <div key={d.name} className="flex items-center gap-2 text-xs">
                  <span className="h-2 w-2 rounded-full shrink-0" style={{ backgroundColor: COLORS[i] }} />
                  <span className="text-muted-foreground">{d.name}</span>
                  <span className="ml-auto font-medium">{d.value}%</span>
                </div>
              ))}
            </div>
          </div>
        </motion.div>

        {/* New policies trend */}
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.5 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">New Policies</h3>
          <p className="text-xs text-muted-foreground mb-4">Monthly policy creation rate</p>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={chartDataMonthly}>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={tooltipStyle} />
              <Bar dataKey="newPolicies" fill="hsl(199, 89%, 48%)" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </motion.div>
      </div>
    </div>
  );
}
