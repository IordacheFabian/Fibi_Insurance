import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { motion } from "framer-motion";
import {
  FileText,
  ShieldAlert,
  CreditCard,
  Users,
  TrendingUp,
  AlertTriangle,
  Activity,
  UserCog,
  BarChart3,
  DollarSign,
  Calendar,
  ArrowRight,
} from "lucide-react";
import {
  AreaChart,
  Area,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { useNavigate } from "react-router-dom";
import { EmptyState } from "@/components/ui/EmptyState";
import { KpiCard } from "@/components/ui/KpiCard";
import { Skeleton } from "@/components/ui/skeleton";
import { StatusChip } from "@/components/ui/StatusChip";
import { PageHeader } from "@/components/ui/PageHeader";
import { useRole } from "@/contexts/RoleContext";
import { getAdminClaims, normalizeClaimStatus } from "@/lib/claims/claim.api";
import { getPayments } from "@/lib/payments/payment.api";
import { getPolicies, normalizePolicyStatus } from "@/lib/policies/policy.api";
import { getReportsAnalytics } from "@/lib/reports/report.api";
import { formatMoney } from "@/lib/utils";

const container = {
  hidden: {},
  show: { transition: { staggerChildren: 0.08 } },
};

const item = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0 },
};

const COLORS = ["hsl(217, 91%, 60%)", "hsl(199, 89%, 48%)", "hsl(160, 84%, 39%)", "hsl(38, 92%, 50%)", "hsl(271, 81%, 56%)", "hsl(0, 72%, 51%)", "hsl(180, 70%, 45%)", "hsl(30, 80%, 55%)"];

const tooltipStyle = {
  backgroundColor: "hsl(223, 40%, 10%)",
  border: "1px solid hsl(217, 33%, 17%)",
  borderRadius: "8px",
  fontSize: "12px",
};

type DashboardActivity = {
  id: string;
  type: "policy" | "payment" | "claim";
  title: string;
  description: string;
  timestamp: string;
  status: string;
  href: string;
};

const dashboardRangePresets = [
  { key: "mtd", label: "Month to Date" },
  { key: "qtd", label: "Quarter to Date" },
  { key: "ytd", label: "Year to Date" },
] as const;

type DashboardRangePreset = typeof dashboardRangePresets[number]["key"];

function parseDashboardDate(value: string) {
  const parsed = new Date(value.includes("T") ? value : `${value}T00:00:00`);
  return Number.isNaN(parsed.getTime()) ? 0 : parsed.getTime();
}

function formatDashboardDate(value: string) {
  const timestamp = parseDashboardDate(value);
  if (!timestamp) {
    return "Unknown date";
  }

  return new Date(timestamp).toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

function formatDateInputValue(value: Date) {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, "0");
  const day = String(value.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function getCurrentMonthDateRange() {
  const today = new Date();
  const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);

  return {
    from: formatDateInputValue(monthStart),
    to: formatDateInputValue(today),
  };
}

function getQuarterToDateRange() {
  const today = new Date();
  const quarterStartMonth = Math.floor(today.getMonth() / 3) * 3;
  const quarterStart = new Date(today.getFullYear(), quarterStartMonth, 1);

  return {
    from: formatDateInputValue(quarterStart),
    to: formatDateInputValue(today),
  };
}

function getYearToDateRange() {
  const today = new Date();
  const yearStart = new Date(today.getFullYear(), 0, 1);

  return {
    from: formatDateInputValue(yearStart),
    to: formatDateInputValue(today),
  };
}

function getDashboardDateRange(preset: DashboardRangePreset) {
  if (preset === "qtd") {
    return getQuarterToDateRange();
  }

  if (preset === "ytd") {
    return getYearToDateRange();
  }

  return getCurrentMonthDateRange();
}

function isWithinDateRange(value: string, from: string, to: string) {
  const timestamp = parseDashboardDate(value);
  const start = parseDashboardDate(from);
  const end = parseDashboardDate(`${to}T23:59:59.999`);

  return timestamp >= start && timestamp <= end;
}

function formatDashboardDateRange(from: string, to: string) {
  const fromLabel = formatDashboardDate(from);
  const toLabel = formatDashboardDate(to);

  return `${fromLabel} - ${toLabel}`;
}

function formatPercent(value: number) {
  return `${value.toFixed(1)}%`;
}

function formatCount(value: number) {
  return new Intl.NumberFormat("en-GB", { maximumFractionDigits: 0 }).format(value);
}

function normalizeLabel(value: string) {
  return value.replace(/\s+/g, "_").toLowerCase();
}

function getClaimsCountByStatus(points: Array<{ name: string; value: number }>, statuses: string[]) {
  const lookup = new Set(statuses.map(normalizeLabel));
  return points.reduce((total, point) => total + (lookup.has(normalizeLabel(point.name)) ? point.value : 0), 0);
}

function getActivityIcon(type: DashboardActivity["type"]) {
  if (type === "claim") {
    return <ShieldAlert className="h-4 w-4 text-warning" />;
  }

  if (type === "payment") {
    return <CreditCard className="h-4 w-4 text-success" />;
  }

  return <FileText className="h-4 w-4 text-primary" />;
}

function buildBrokerActivities(
  policies: Awaited<ReturnType<typeof getPolicies>>,
  payments: Awaited<ReturnType<typeof getPayments>>,
): DashboardActivity[] {
  const policyActivities = policies.map((policy) => ({
    id: `policy-${policy.id}`,
    type: "policy" as const,
    title: `${policy.policyNumber} is ${normalizePolicyStatus(policy.policyStatus)}`,
    description: `${policy.clientName} · ${formatMoney(policy.finalPremium, policy.currencyCode)}`,
    timestamp: policy.startDate,
    status: normalizePolicyStatus(policy.policyStatus),
    href: `/policies/${policy.id}`,
  }));

  const paymentActivities = payments.map((payment) => ({
    id: `payment-${payment.id}`,
    type: "payment" as const,
    title: payment.status === "paid" ? "Payment recorded" : "Payment awaiting confirmation",
    description: `${payment.policyNumber} · ${formatMoney(payment.amount, payment.currencyCode)} · ${payment.clientName}`,
    timestamp: payment.paymentDate,
    status: payment.status,
    href: "/payments",
  }));

  return [...policyActivities, ...paymentActivities]
    .sort((left, right) => parseDashboardDate(right.timestamp) - parseDashboardDate(left.timestamp))
    .slice(0, 6);
}

function buildAdminActivities(
  claims: Awaited<ReturnType<typeof getAdminClaims>>,
  payments: Awaited<ReturnType<typeof getPayments>>,
): DashboardActivity[] {
  const claimActivities = claims.map((claim) => ({
    id: `claim-${claim.id}`,
    type: "claim" as const,
    title: `${claim.policyNumber} claim ${normalizeClaimStatus(claim.status)}`,
    description: `${claim.clientName} · ${formatMoney(claim.approvedAmount ?? claim.estimatedDamage, claim.currencyCode ?? "RON")}`,
    timestamp: claim.createdAt,
    status: normalizeClaimStatus(claim.status),
    href: "/claims",
  }));

  const paymentActivities = payments.map((payment) => ({
    id: `payment-${payment.id}`,
    type: "payment" as const,
    title: payment.status === "paid" ? "Payment collected" : "Payment update received",
    description: `${payment.policyNumber} · ${formatMoney(payment.amount, payment.currencyCode)} · ${payment.clientName}`,
    timestamp: payment.paymentDate,
    status: payment.status,
    href: "/payments",
  }));

  return [...claimActivities, ...paymentActivities]
    .sort((left, right) => parseDashboardDate(right.timestamp) - parseDashboardDate(left.timestamp))
    .slice(0, 6);
}

function DashboardLoadingState() {
  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Skeleton className="h-8 w-56" />
        <Skeleton className="h-4 w-80" />
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {Array.from({ length: 4 }).map((_, index) => (
          <div key={index} className="kpi-card space-y-3">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-8 w-32" />
            <Skeleton className="h-4 w-36" />
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

export default function DashboardPage() {
  const { role } = useRole();
  const navigate = useNavigate();
  const [activeRangePreset, setActiveRangePreset] = useState<DashboardRangePreset>("mtd");
  const currentRange = useMemo(() => getDashboardDateRange(activeRangePreset), [activeRangePreset]);

  const analyticsQuery = useQuery({
    queryKey: ["dashboard", "analytics", role, currentRange.from, currentRange.to],
    queryFn: () => getReportsAnalytics(role, currentRange),
    staleTime: 30000,
  });

  const paymentsQuery = useQuery({
    queryKey: ["dashboard", "payments", role],
    queryFn: () => getPayments(role === "admin" ? "admin" : "broker"),
    staleTime: 30000,
  });

  const policiesQuery = useQuery({
    queryKey: ["dashboard", "policies", role],
    queryFn: () => getPolicies(),
    enabled: role === "broker",
    staleTime: 30000,
  });

  const adminClaimsQuery = useQuery({
    queryKey: ["dashboard", "claims", role],
    queryFn: () => getAdminClaims(),
    enabled: role === "admin",
    staleTime: 30000,
  });

  const data = analyticsQuery.data;
  const payments = paymentsQuery.data ?? [];
  const policies = policiesQuery.data ?? [];
  const adminClaims = adminClaimsQuery.data ?? [];
  const filteredPayments = useMemo(
    () => payments.filter((payment) => isWithinDateRange(payment.paymentDate, currentRange.from, currentRange.to)),
    [currentRange.from, currentRange.to, payments],
  );
  const filteredPolicies = useMemo(
    () => policies.filter((policy) => isWithinDateRange(policy.startDate, currentRange.from, currentRange.to)),
    [currentRange.from, currentRange.to, policies],
  );
  const filteredAdminClaims = useMemo(
    () => adminClaims.filter((claim) => isWithinDateRange(claim.createdAt, currentRange.from, currentRange.to)),
    [adminClaims, currentRange.from, currentRange.to],
  );

  const isLoading = analyticsQuery.isLoading || paymentsQuery.isLoading || (role === "broker" && policiesQuery.isLoading) || (role === "admin" && adminClaimsQuery.isLoading);
  const isError = analyticsQuery.isError || paymentsQuery.isError || (role === "broker" && policiesQuery.isError) || (role === "admin" && adminClaimsQuery.isError);
  const error = analyticsQuery.error ?? paymentsQuery.error ?? policiesQuery.error ?? adminClaimsQuery.error;

  const pendingClaimsCount = useMemo(() => {
    if (!data) {
      return 0;
    }

    return getClaimsCountByStatus(data.claimsBreakdown, ["submitted", "under_review"]);
  }, [data]);

  const pendingPaymentsCount = useMemo(
    () => filteredPayments.filter((payment) => payment.status === "pending").length,
    [filteredPayments],
  );

  const recentActivities = useMemo(() => {
    if (role === "admin") {
      return buildAdminActivities(filteredAdminClaims, filteredPayments);
    }

    return buildBrokerActivities(filteredPolicies, filteredPayments);
  }, [filteredAdminClaims, filteredPayments, filteredPolicies, role]);

  const alertItems = useMemo(() => {
    if (!data) {
      return [] as Array<{ id: string; text: string; status: string }>;
    }

    const items: Array<{ id: string; text: string; status: string }> = [];

    if (pendingClaimsCount > 0) {
      items.push({
        id: "pending-claims",
        text: `${pendingClaimsCount} claim${pendingClaimsCount === 1 ? "" : "s"} waiting for review or approval`,
        status: pendingClaimsCount > 2 ? "warning" : "submitted",
      });
    }

    if (pendingPaymentsCount > 0) {
      items.push({
        id: "pending-payments",
        text: `${pendingPaymentsCount} payment${pendingPaymentsCount === 1 ? "" : "s"} still pending confirmation`,
        status: pendingPaymentsCount > 2 ? "warning" : "pending",
      });
    }

    if (data.summary.collectionRate < 75 && data.summary.totalPolicies > 0) {
      items.push({
        id: "collection-rate",
        text: `Collection rate is ${formatPercent(data.summary.collectionRate)} for the selected portfolio`,
        status: "overdue",
      });
    }

    if (items.length === 0) {
      items.push({
        id: "healthy",
        text: role === "admin" ? "No urgent portfolio alerts right now." : "No urgent broker alerts right now.",
        status: "active",
      });
    }

    return items;
  }, [data, pendingClaimsCount, pendingPaymentsCount, role]);

  const summaryCards = useMemo(() => {
    if (!data) {
      return [];
    }

    if (role === "broker") {
      return [
        {
          title: "Policies In Portfolio",
          value: formatCount(data.summary.totalPolicies),
          change: `${formatMoney(data.summary.totalWrittenPremium, data.currencyCode)} written premium`,
          changeType: "positive" as const,
          icon: FileText,
          gradient: "gradient-primary",
        },
        {
          title: "Pending Claims",
          value: formatCount(pendingClaimsCount),
          change: `${formatMoney(data.summary.totalClaimsIncurred, data.currencyCode)} incurred claims`,
          changeType: pendingClaimsCount > 0 ? "neutral" as const : "positive" as const,
          icon: ShieldAlert,
          gradient: "gradient-warning",
        },
        {
          title: "Pending Payments",
          value: formatCount(pendingPaymentsCount),
          change: `${formatMoney(data.summary.totalPremiumRevenue, data.currencyCode)} collected so far`,
          changeType: pendingPaymentsCount > 0 ? "negative" as const : "positive" as const,
          icon: CreditCard,
          gradient: "gradient-danger",
        },
        {
          title: "Broker Earnings",
          value: formatMoney(data.summary.totalBrokerEarnings, data.currencyCode),
          change: `${formatPercent(data.summary.collectionRate)} collection rate`,
          changeType: "positive" as const,
          icon: TrendingUp,
          gradient: "gradient-success",
        },
      ];
    }

    const activePolicies = data.brokerPerformance.reduce((total, broker) => total + broker.activePolicies, 0);

    return [
      {
        title: "Portfolio Policies",
        value: formatCount(data.summary.totalPolicies),
        change: `${formatMoney(data.summary.totalWrittenPremium, data.currencyCode)} written premium`,
        changeType: "positive" as const,
        icon: FileText,
        gradient: "gradient-primary",
      },
      {
        title: "Brokers In Portfolio",
        value: formatCount(data.brokerPerformance.length),
        change: `${formatCount(activePolicies)} active policies managed`,
        changeType: "positive" as const,
        icon: UserCog,
        gradient: "gradient-accent",
      },
      {
        title: "Pending Claims",
        value: formatCount(pendingClaimsCount),
        change: `${formatMoney(data.summary.totalClaimsIncurred, data.currencyCode)} incurred claims`,
        changeType: pendingClaimsCount > 0 ? "neutral" as const : "positive" as const,
        icon: ShieldAlert,
        gradient: "gradient-warning",
      },
      {
        title: "Collected Premium",
        value: formatMoney(data.summary.totalPremiumRevenue, data.currencyCode),
        change: `${formatMoney(data.summary.totalBrokerEarnings, data.currencyCode)} broker earnings`,
        changeType: "positive" as const,
        icon: DollarSign,
        gradient: "gradient-success",
      },
    ];
  }, [data, pendingClaimsCount, pendingPaymentsCount, role]);

  const actionItems = useMemo(() => {
    if (role === "admin") {
      return [
        { label: "Claims", icon: ShieldAlert, path: "/claims", gradient: "gradient-warning" },
        { label: "Payments", icon: CreditCard, path: "/payments", gradient: "gradient-success" },
        { label: "Reports", icon: BarChart3, path: "/reports", gradient: "gradient-primary" },
        { label: "Brokers", icon: Users, path: "/brokers", gradient: "gradient-accent" },
      ];
    }

    return [
      { label: "New Policy", icon: FileText, path: "/policies/new", gradient: "gradient-primary" },
      { label: "Add Client", icon: Users, path: "/clients/new", gradient: "gradient-accent" },
      { label: "Claims", icon: ShieldAlert, path: "/claims", gradient: "gradient-warning" },
      { label: "Payments", icon: CreditCard, path: "/payments", gradient: "gradient-success" },
    ];
  }, [role]);

  if (isLoading) {
    return <DashboardLoadingState />;
  }

  if (isError || !data) {
    return (
      <div className="space-y-6">
        <PageHeader
          title={role === "admin" ? "Admin Dashboard" : "Broker Dashboard"}
          description={role === "admin" ? "Portfolio-wide overview and broker performance" : "Your portfolio summary and recent activity"}
        />
        <div className="flex flex-col items-center justify-center gap-4 px-6 py-16 text-center glass-card">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
            <AlertTriangle className="h-8 w-8" />
          </div>
          <div>
            <h3 className="text-lg font-semibold">Could not load the dashboard</h3>
            <p className="text-sm text-muted-foreground mt-1 max-w-md">
              {error instanceof Error ? error.message : "The dashboard request failed."}
            </p>
          </div>
        </div>
      </div>
    );
  }

  const hasAnyData = data.summary.totalPolicies > 0 || data.monthly.some((point) => point.premiums > 0 || point.claims > 0 || point.newPolicies > 0);
  const brokerSnapshot = data.brokerPerformance[0];

  return (
    <div className="space-y-6">
      <PageHeader
        title={role === "admin" ? "Admin Dashboard" : "Broker Dashboard"}
        description={role === "admin" ? "Portfolio-wide overview and broker performance" : "Your live portfolio summary, collections, and earnings"}
      >
        <div className="flex flex-wrap items-center justify-end gap-2">
          <div className="inline-flex items-center gap-1 rounded-lg bg-muted/40 p-1 text-xs">
            {dashboardRangePresets.map((preset) => (
              <button
                key={preset.key}
                type="button"
                onClick={() => setActiveRangePreset(preset.key)}
                className={preset.key === activeRangePreset
                  ? "rounded-md bg-card px-2.5 py-1.5 font-medium text-foreground shadow-sm"
                  : "rounded-md px-2.5 py-1.5 text-muted-foreground transition-colors hover:text-foreground"}
              >
                {preset.label}
              </button>
            ))}
          </div>
          <div className="inline-flex items-center gap-2 rounded-lg border border-border bg-muted/40 px-3 py-2 text-xs text-muted-foreground">
            <Calendar className="h-3.5 w-3.5" />
            {formatDashboardDateRange(currentRange.from, currentRange.to)}
          </div>
        </div>
      </PageHeader>

      {!hasAnyData ? (
        <EmptyState
          icon={BarChart3}
          title="No dashboard data yet"
          description="Create policies, record payments, and process claims to populate the live dashboard."
        />
      ) : (
        <>

      {/* KPI Cards */}
      <motion.div
        variants={container}
        initial="hidden"
        animate="show"
        className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4"
      >
        {summaryCards.map((card, index) => (
          <KpiCard
            key={card.title}
            title={card.title}
            value={card.value}
            change={card.change}
            changeType={card.changeType}
            icon={card.icon}
            gradient={card.gradient}
            delay={index * 0.1}
          />
        ))}
      </motion.div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="glass-card p-6"
        >
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className="text-sm font-semibold">Premium vs Claims Trend</h3>
              <p className="text-xs text-muted-foreground mt-0.5">Collections and claims for the reporting window</p>
            </div>
            <div className="flex gap-4 text-xs">
              <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-primary" /> Premiums</span>
              <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-destructive" /> Claims</span>
            </div>
          </div>
          <ResponsiveContainer width="100%" height={280}>
            <AreaChart data={data.monthly}>
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
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} tickFormatter={(value) => formatMoney(value, data.currencyCode)} />
              <Tooltip
                contentStyle={tooltipStyle}
                formatter={(value: number, seriesName: string) => [formatMoney(value, data.currencyCode), seriesName === "premiums" ? "Premiums" : "Claims"]}
              />
              <Area type="monotone" dataKey="premiums" stroke="hsl(217, 91%, 60%)" fill="url(#premGrad)" strokeWidth={2} />
              <Area type="monotone" dataKey="claims" stroke="hsl(0, 72%, 51%)" fill="url(#claimGrad)" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="glass-card p-6"
        >
          <h3 className="text-sm font-semibold mb-1">New Policies</h3>
          <p className="text-xs text-muted-foreground mb-4">Monthly policy start trend</p>
          <ResponsiveContainer width="100%" height={280}>
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

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="glass-card p-6"
        >
          <h3 className="text-sm font-semibold mb-1">Claims Breakdown</h3>
          <p className="text-xs text-muted-foreground mb-4">Current claim pipeline distribution</p>
          <div className="flex items-center gap-8">
            <ResponsiveContainer width="50%" height={220}>
              <PieChart>
                <Pie data={data.claimsBreakdown} cx="50%" cy="50%" innerRadius={55} outerRadius={80} paddingAngle={3} dataKey="value" stroke="none">
                  {data.claimsBreakdown.map((_, index) => (
                    <Cell key={index} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip contentStyle={tooltipStyle} />
              </PieChart>
            </ResponsiveContainer>
            <div className="space-y-2 flex-1">
              {data.claimsBreakdown.map((point, index) => (
                <div key={point.name} className="flex items-center gap-2 text-xs">
                  <span className="h-2 w-2 rounded-full shrink-0" style={{ backgroundColor: COLORS[index % COLORS.length] }} />
                  <span className="text-muted-foreground">{point.name}</span>
                  <span className="ml-auto font-medium">{point.value}</span>
                </div>
              ))}
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.6 }}
          className="glass-card p-6"
        >
          <h3 className="text-sm font-semibold mb-1">Premium by City</h3>
          <p className="text-xs text-muted-foreground mb-4">Top locations by written premium</p>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={data.geographic}>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 17%)" opacity={0.3} />
              <XAxis dataKey="region" tick={{ fontSize: 10, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "hsl(215, 20%, 55%)" }} axisLine={false} tickLine={false} tickFormatter={(value) => formatMoney(value, data.currencyCode)} />
              <Tooltip contentStyle={tooltipStyle} formatter={(value: number) => [formatMoney(value, data.currencyCode), "Written premium"]} />
              <Bar dataKey="premium" fill="hsl(217, 91%, 60%)" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </motion.div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.7 }}
          className="lg:col-span-2 glass-card p-6"
        >
          <h3 className="text-sm font-semibold mb-4">Recent Activity</h3>
          <div className="space-y-0">
            {recentActivities.length === 0 ? (
              <div className="rounded-xl border border-dashed border-border p-6 text-sm text-muted-foreground">
                No recent activity is available for this dashboard yet.
              </div>
            ) : recentActivities.map((activity) => (
              <button
                key={activity.id}
                type="button"
                onClick={() => navigate(activity.href)}
                className="w-full text-left flex items-start gap-3 py-3 border-b border-border/50 last:border-0 hover:bg-muted/30 transition-colors rounded-lg px-2"
              >
                <div className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-muted">
                  {getActivityIcon(activity.type)}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium">{activity.title}</p>
                    <StatusChip status={activity.status} className="text-[10px]" />
                  </div>
                  <p className="text-xs text-muted-foreground truncate mt-1">{activity.description}</p>
                </div>
                <span className="text-xs text-muted-foreground whitespace-nowrap">{formatDashboardDate(activity.timestamp)}</span>
              </button>
            ))}
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.8 }}
          className="space-y-4"
        >
          <div className="glass-card p-6">
            <h3 className="text-sm font-semibold mb-3">Quick Actions</h3>
            <div className="grid grid-cols-2 gap-2">
              {actionItems.map((action) => (
                <button
                  key={action.label}
                  type="button"
                  onClick={() => navigate(action.path)}
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

          <div className="glass-card p-6">
            <h3 className="text-sm font-semibold mb-3 flex items-center gap-2">
              <AlertTriangle className="h-4 w-4 text-warning" />
              Alerts
            </h3>
            <div className="space-y-2">
              {alertItems.map((item) => (
                <div key={item.id} className="flex items-center gap-2 p-2.5 rounded-lg bg-muted/40 border border-border text-xs">
                  <StatusChip status={item.status} className="text-[10px] shrink-0" />
                  <span>{item.text}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="glass-card p-6 space-y-3">
            <div className="flex items-center justify-between gap-3">
              <div>
                <h3 className="text-sm font-semibold">{role === "admin" ? "Portfolio Snapshot" : "Earnings Snapshot"}</h3>
                <p className="text-xs text-muted-foreground mt-1">
                  {role === "admin" ? "Live view of broker production and commissions." : "How much you collected and earned from written policies."}
                </p>
              </div>
              <button type="button" onClick={() => navigate("/reports")} className="text-xs text-primary font-medium flex items-center gap-1">
                Reports <ArrowRight className="h-3.5 w-3.5" />
              </button>
            </div>

            <div className="rounded-xl border border-border bg-muted/20 p-4 space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Written premium</span>
                <span className="font-semibold">{formatMoney(data.summary.totalWrittenPremium, data.currencyCode)}</span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Collected premium</span>
                <span className="font-semibold">{formatMoney(data.summary.totalPremiumRevenue, data.currencyCode)}</span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Broker earnings</span>
                <span className="font-semibold text-primary">{formatMoney(data.summary.totalBrokerEarnings, data.currencyCode)}</span>
              </div>
              {brokerSnapshot && (
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted-foreground">Commission rate</span>
                  <span className="font-semibold">{formatPercent(brokerSnapshot.commissionPercentage)}</span>
                </div>
              )}
            </div>
          </div>
        </motion.div>
      </div>

      {role === "admin" && (
        <div className="grid grid-cols-1 gap-4">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.9 }}
            className="glass-card p-6"
          >
            <h3 className="text-sm font-semibold mb-4">Broker Performance</h3>
            <div className="space-y-4">
              {data.brokerPerformance.length === 0 ? (
                <div className="rounded-xl border border-dashed border-border p-6 text-sm text-muted-foreground">
                  No broker production is available for the selected reporting period.
                </div>
              ) : data.brokerPerformance.map((broker) => (
                <div key={broker.brokerId} className="flex items-center gap-4 p-3 rounded-xl border border-border hover:border-primary/20 transition-colors">
                  <div className="h-10 w-10 rounded-full gradient-primary flex items-center justify-center text-xs font-bold text-primary-foreground">
                    {broker.brokerName.split(" ").map((part) => part[0]).join("")}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium">{broker.brokerName}</p>
                    <p className="text-xs text-muted-foreground">
                      {broker.activePolicies} active policies · {formatMoney(broker.writtenPremium, data.currencyCode)} written · {formatMoney(broker.collectedPremium, data.currencyCode)} collected
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold text-primary">{formatMoney(broker.brokerEarnings, data.currencyCode)}</p>
                    <p className="text-[10px] text-muted-foreground uppercase tracking-wider">{formatPercent(broker.commissionPercentage)} commission</p>
                  </div>
                </div>
              ))}
            </div>
          </motion.div>
        </div>
      )}
        </>
      )}
    </div>
  );
}
