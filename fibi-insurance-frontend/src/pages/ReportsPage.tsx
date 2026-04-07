import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { motion } from "framer-motion";
import {
  BarChart, Bar, AreaChart, Area, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, PieChart, Pie, Cell,
} from "recharts";
import { PageHeader } from "@/components/ui/PageHeader";
import { EmptyState } from "@/components/ui/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";
import { getCurrencies } from "@/lib/metadatas/currency.api";
import { getReportsAnalytics } from "@/lib/reports/report.api";
import { formatMoney } from "@/lib/utils";
import { useRole } from "@/contexts/RoleContext";
import { AlertCircle, Download, Calendar, BarChart3 } from "lucide-react";

const COLORS = ["hsl(217, 91%, 60%)", "hsl(199, 89%, 48%)", "hsl(160, 84%, 39%)", "hsl(38, 92%, 50%)", "hsl(271, 81%, 56%)", "hsl(0, 72%, 51%)", "hsl(180, 70%, 45%)", "hsl(30, 80%, 55%)"];

const tooltipStyle = {
  backgroundColor: "hsl(223, 40%, 10%)",
  border: "1px solid hsl(217, 33%, 17%)",
  borderRadius: "8px",
  fontSize: "12px",
};

const rangePresets = [
  { key: "30d", label: "Last 30 Days" },
  { key: "qtd", label: "This Quarter" },
  { key: "ytd", label: "Year to Date" },
  { key: "12m", label: "Last 12 Months" },
] as const;

type RangePresetKey = typeof rangePresets[number]["key"];

function formatPercentage(value: number) {
  return `${value >= 0 ? "+" : ""}${value.toFixed(1)}%`;
}

function getDefaultDateRange() {
  const today = new Date();
  const to = today.toISOString().slice(0, 10);
  const fromDate = new Date(today.getFullYear(), today.getMonth() - 11, 1);
  const from = fromDate.toISOString().slice(0, 10);

  return { from, to };
}

function getPresetDateRange(preset: typeof rangePresets[number]["key"]) {
  const today = new Date();
  const to = today.toISOString().slice(0, 10);

  if (preset === "30d") {
    const fromDate = new Date(today);
    fromDate.setDate(today.getDate() - 29);
    return { from: fromDate.toISOString().slice(0, 10), to };
  }

  if (preset === "qtd") {
    const quarterStartMonth = Math.floor(today.getMonth() / 3) * 3;
    const fromDate = new Date(today.getFullYear(), quarterStartMonth, 1);
    return { from: fromDate.toISOString().slice(0, 10), to };
  }

  if (preset === "ytd") {
    const fromDate = new Date(today.getFullYear(), 0, 1);
    return { from: fromDate.toISOString().slice(0, 10), to };
  }

  return getDefaultDateRange();
}

export default function ReportsPage() {
  const { role } = useRole();
  const [dateRange, setDateRange] = useState(getDefaultDateRange);
  const [activePreset, setActivePreset] = useState<RangePresetKey | "custom">("12m");
  const [selectedCurrency, setSelectedCurrency] = useState<string>("");
  const [filterByCurrency, setFilterByCurrency] = useState(false);
  const { data: currencies = [] } = useQuery({
    queryKey: ["currencies", role],
    queryFn: () => getCurrencies(role),
    staleTime: 30000,
  });

  useEffect(() => {
    if (currencies.length === 0) {
      return;
    }

    const hasSelectedCurrency = currencies.some((currency) => currency.code === selectedCurrency);
    if (!selectedCurrency || !hasSelectedCurrency) {
      setSelectedCurrency(currencies[0].code);
    }
  }, [currencies, selectedCurrency]);

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ["reports", role, dateRange.from, dateRange.to, selectedCurrency, filterByCurrency],
    queryFn: () => getReportsAnalytics(role, { ...dateRange, currency: selectedCurrency || undefined, filterByCurrency }),
    enabled: currencies.length > 0,
    staleTime: 30000,
  });

  const summaryCards = useMemo(() => {
    if (!data) {
      return [];
    }

    return [
      {
        label: "Total Premium Revenue",
        value: formatMoney(data.summary.totalPremiumRevenue, data.currencyCode),
        change: `${data.summary.totalPolicies} total policies`,
      },
      {
        label: "Claims Ratio",
        value: `${data.summary.claimsRatio.toFixed(1)}%`,
        change: `${formatMoney(data.summary.totalClaimsIncurred, data.currencyCode)} incurred claims`,
      },
      {
        label: "Portfolio Growth",
        value: formatPercentage(data.summary.portfolioGrowth),
        change: `Based on policy starts over 12 months`,
      },
      {
        label: "Collection Rate",
        value: `${data.summary.collectionRate.toFixed(1)}%`,
        change: `${formatMoney(data.summary.totalPremiumRevenue, data.currencyCode)} collected`,
      },
    ];
  }, [data]);

  const handleExport = () => {
    if (!data) {
      return;
    }

    const exportPayload = {
      exportedAt: new Date().toISOString(),
      filters: {
        from: dateRange.from,
        to: dateRange.to,
        role,
        currency: data.currencyCode,
        filterByCurrency,
      },
      analytics: data,
    };

    const blob = new Blob([JSON.stringify(exportPayload, null, 2)], { type: "application/json" });
    const url = window.URL.createObjectURL(blob);
    const link = window.document.createElement("a");
    link.href = url;
    link.download = `reports-analytics-${dateRange.from}-to-${dateRange.to}.json`;
    link.click();
    window.URL.revokeObjectURL(url);
  };

  const handleResetRange = () => {
    setActivePreset("12m");
    setDateRange(getDefaultDateRange());
  };

  const handlePresetSelect = (preset: RangePresetKey) => {
    setActivePreset(preset);
    setDateRange(getPresetDateRange(preset));
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between gap-4">
          <div className="space-y-2">
            <Skeleton className="h-8 w-56" />
            <Skeleton className="h-4 w-80" />
          </div>
        </div>

        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="glass-card p-5 space-y-3">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-8 w-24" />
              <Skeleton className="h-4 w-40" />
            </div>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="glass-card p-6 space-y-4">
              <Skeleton className="h-5 w-40" />
              <Skeleton className="h-4 w-48" />
              <Skeleton className="h-64 w-full" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="space-y-6">
        <PageHeader title="Reports & Analytics" description="Portfolio insights and performance metrics" />
        <div className="flex flex-col items-center justify-center gap-4 px-6 py-16 text-center glass-card">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
            <AlertCircle className="h-8 w-8" />
          </div>
          <div>
            <h3 className="text-lg font-semibold">Could not load reports</h3>
            <p className="text-sm text-muted-foreground mt-1 max-w-md">
              {error instanceof Error ? error.message : "The reports request failed."}
            </p>
          </div>
        </div>
      </div>
    );
  }

  const hasAnyData = data.summary.totalPolicies > 0 || data.monthly.some((item) => item.premiums > 0 || item.claims > 0 || item.newPolicies > 0);

  return (
    <div className="space-y-6">
      <PageHeader title="Reports & Analytics" description="Portfolio insights and performance metrics">
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-2">
          <div className="flex items-center gap-1 rounded-lg border border-border bg-card p-1">
            {rangePresets.map((preset) => (
              <button
                key={preset.key}
                type="button"
                onClick={() => handlePresetSelect(preset.key)}
                className={`h-8 rounded-md px-3 text-xs font-medium transition-colors ${activePreset === preset.key ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-muted hover:text-foreground"}`}
              >
                {preset.label}
              </button>
            ))}
          </div>
          <div className="flex items-center gap-2 rounded-lg border border-border bg-card px-3 py-2 text-sm">
            <span className="text-muted-foreground">Currency</span>
            <select
              value={selectedCurrency}
              onChange={(event) => setSelectedCurrency(event.target.value)}
              className="bg-transparent text-sm outline-none"
            >
              {currencies.map((currency) => (
                <option key={currency.id} value={currency.code}>
                  {currency.code}
                </option>
              ))}
            </select>
          </div>
          <label className="flex items-center gap-2 rounded-lg border border-border bg-card px-3 py-2 text-sm">
            <input
              type="checkbox"
              checked={filterByCurrency}
              onChange={(event) => setFilterByCurrency(event.target.checked)}
              className="h-4 w-4 rounded border-border bg-transparent"
            />
            <span className="text-muted-foreground">Only selected currency</span>
          </label>
          <div className="flex items-center gap-2 rounded-lg border border-border bg-card px-3 py-2 text-sm">
            <Calendar className="h-4 w-4 text-muted-foreground" />
            <input
              type="date"
              value={dateRange.from}
              onChange={(event) => {
                setActivePreset("custom");
                setDateRange((current) => ({ ...current, from: event.target.value }));
              }}
              max={dateRange.to}
              className="bg-transparent text-sm outline-none"
            />
            <span className="text-muted-foreground">to</span>
            <input
              type="date"
              value={dateRange.to}
              onChange={(event) => {
                setActivePreset("custom");
                setDateRange((current) => ({ ...current, to: event.target.value }));
              }}
              min={dateRange.from}
              className="bg-transparent text-sm outline-none"
            />
          </div>
          <button type="button" onClick={handleResetRange} className="flex items-center gap-2 h-9 px-3 rounded-lg border border-border text-sm hover:bg-muted transition-colors">
            <Calendar className="h-4 w-4" /> Last 12 Months
          </button>
          <button type="button" onClick={handleExport} className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
            <Download className="h-4 w-4" /> Export
          </button>
          </div>
          <p className="px-1 text-xs text-muted-foreground">
            Leave <span className="font-medium text-foreground">Only selected currency</span> unchecked to convert all analytics into the chosen currency. Enable it to include only records originally written in that currency.
          </p>
        </div>
      </PageHeader>

      {!hasAnyData ? (
        <EmptyState
          icon={BarChart3}
          title="Not enough report data"
          description="Create policies, record payments, and process claims to populate the analytics dashboard."
        />
      ) : (
        <>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {summaryCards.map((s, i) => (
          <motion.div
            key={s.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.05 }}
            className="glass-card p-5"
          >
            <p className="text-xs text-muted-foreground">{s.label}</p>
            <p className="text-2xl font-bold mt-1">{s.value}</p>
            <p className="text-xs text-muted-foreground mt-1">{s.change}</p>
          </motion.div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">Premium Collection Trend</h3>
          <p className="text-xs text-muted-foreground mb-4">Monthly premiums collected</p>
          <ResponsiveContainer width="100%" height={250}>
            <AreaChart data={data.monthly}>
              <defs>
                <linearGradient id="rpGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="hsl(160, 84%, 39%)" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="hsl(160, 84%, 39%)" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} tickFormatter={(v) => formatMoney(v, data.currencyCode)} />
              <Tooltip contentStyle={tooltipStyle} formatter={(v: number) => [formatMoney(v, data.currencyCode), "Premiums"]} />
              <Area
                type="linear"
                dataKey="premiums"
                stroke="hsl(160, 84%, 39%)"
                fill="url(#rpGrad)"
                strokeWidth={2}
                isAnimationActive={false}
                dot={{ r: 3, strokeWidth: 0, fill: "hsl(160, 84%, 39%)" }}
                activeDot={{ r: 5 }}
              />
            </AreaChart>
          </ResponsiveContainer>
        </motion.div>

        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">Premium by City</h3>
          <p className="text-xs text-muted-foreground mb-4">Top locations by written premium</p>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={data.geographic}>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="region" tick={{ fontSize: 10, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} tickFormatter={(v) => formatMoney(v, data.currencyCode)} />
              <Tooltip contentStyle={tooltipStyle} formatter={(v: number) => [formatMoney(v, data.currencyCode), "Premium"]} />
              <Bar dataKey="premium" fill="hsl(217, 91%, 60%)" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </motion.div>

        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">Claims by Status</h3>
          <p className="text-xs text-muted-foreground mb-4">Current claim pipeline distribution</p>
          <div className="flex items-center gap-8">
            <ResponsiveContainer width="50%" height={200}>
              <PieChart>
                <Pie data={data.claimsBreakdown} cx="50%" cy="50%" innerRadius={50} outerRadius={75} paddingAngle={3} dataKey="value" stroke="none">
                  {data.claimsBreakdown.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                </Pie>
                <Tooltip contentStyle={tooltipStyle} />
              </PieChart>
            </ResponsiveContainer>
            <div className="space-y-2">
              {data.claimsBreakdown.map((d, i) => (
                <div key={d.name} className="flex items-center gap-2 text-xs">
                  <span className="h-2 w-2 rounded-full shrink-0" style={{ backgroundColor: COLORS[i % COLORS.length] }} />
                  <span className="text-muted-foreground">{d.name}</span>
                  <span className="ml-auto font-medium">{d.value}</span>
                </div>
              ))}
            </div>
          </div>
        </motion.div>

        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.5 }} className="glass-card p-6">
          <h3 className="text-sm font-semibold mb-1">New Policies</h3>
          <p className="text-xs text-muted-foreground mb-4">Monthly policy start trend</p>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={data.monthly}>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={tooltipStyle} />
              <Bar dataKey="newPolicies" fill="hsl(199, 89%, 48%)" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </motion.div>
      </div>
        </>
      )}
    </div>
  );
}
