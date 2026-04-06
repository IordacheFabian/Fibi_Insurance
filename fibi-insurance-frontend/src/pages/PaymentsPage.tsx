import { FormEvent, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { motion } from "framer-motion";
import { AlertCircle, Calendar, CreditCard, Plus, Search } from "lucide-react";
import { Link } from "react-router-dom";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { PageHeader } from "@/components/ui/PageHeader";
import { Skeleton } from "@/components/ui/skeleton";
import { StatusChip } from "@/components/ui/StatusChip";
import { toast } from "@/components/ui/sonner";
import { createPayment, getPayments } from "@/lib/payments/payment.api";
import { getPolicies, normalizePolicyStatus } from "@/lib/policies/policy.api";
import { cn, formatMoney } from "@/lib/utils";

const statusTabs = ["all", "paid", "pending", "failed"] as const;

type PaymentFormState = {
  policyId: string;
  amount: string;
  paymentDate: string;
  method: "Cash" | "Card" | "BankTransfer";
  status: "Completed" | "Pending" | "Failed";
};

const initialPaymentForm: PaymentFormState = {
  policyId: "",
  amount: "",
  paymentDate: new Date().toISOString().slice(0, 10),
  method: "BankTransfer",
  status: "Completed",
};

function formatDateTime(value: string) {
  const parsedDate = new Date(value);

  if (Number.isNaN(parsedDate.getTime())) {
    return value;
  }

  return parsedDate.toLocaleString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export default function PaymentsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof statusTabs[number]>("all");
  const [isRecordDialogOpen, setIsRecordDialogOpen] = useState(false);
  const [paymentForm, setPaymentForm] = useState(initialPaymentForm);
  const [paymentError, setPaymentError] = useState("");

  const { data: payments = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ["payments"],
    queryFn: getPayments,
    staleTime: 30000,
  });

  const {
    data: policies = [],
    isLoading: arePoliciesLoading,
  } = useQuery({
    queryKey: ["policies"],
    queryFn: () => getPolicies(),
    staleTime: 30000,
  });

  const activePolicies = useMemo(
    () => policies.filter((policy) => normalizePolicyStatus(policy.policyStatus) === "active"),
    [policies],
  );

  const completedAmountByPolicy = useMemo(() => {
    return payments.reduce<Record<string, number>>((accumulator, payment) => {
      if (payment.status !== "paid") {
        return accumulator;
      }

      accumulator[payment.policyId] = (accumulator[payment.policyId] ?? 0) + payment.amount;
      return accumulator;
    }, {});
  }, [payments]);

  const filtered = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return payments.filter((payment) => {
      const matchesSearch = !normalizedSearch
        || payment.policyNumber.toLowerCase().includes(normalizedSearch)
        || payment.clientName.toLowerCase().includes(normalizedSearch)
        || payment.methodLabel.toLowerCase().includes(normalizedSearch)
        || payment.currencyCode.toLowerCase().includes(normalizedSearch);

      const matchesTab = tab === "all" || payment.status === tab;

      return matchesSearch && matchesTab;
    });
  }, [payments, search, tab]);

  const stats = useMemo(() => {
    const paidTransactions = payments.filter((payment) => payment.status === "paid").length;
    const pendingTransactions = payments.filter((payment) => payment.status === "pending").length;
    const failedTransactions = payments.filter((payment) => payment.status === "failed").length;
    const policiesWithBalance = activePolicies.filter((policy) => {
      const completedPaid = completedAmountByPolicy[policy.id] ?? 0;
      return completedPaid < policy.finalPremium;
    }).length;

    return [
      { label: "Paid Transactions", value: paidTransactions.toString(), color: "text-success" },
      { label: "Pending Transactions", value: pendingTransactions.toString(), color: "text-warning" },
      { label: "Failed Transactions", value: failedTransactions.toString(), color: "text-destructive" },
      { label: "Policies With Balance", value: policiesWithBalance.toString(), color: "text-foreground" },
    ];
  }, [activePolicies, completedAmountByPolicy, payments]);

  const selectedPolicy = activePolicies.find((policy) => policy.id === paymentForm.policyId) ?? null;
  const selectedPolicyRemainingPremium = selectedPolicy
    ? Math.max(0, selectedPolicy.finalPremium - (completedAmountByPolicy[selectedPolicy.id] ?? 0))
    : 0;

  const recordPaymentMutation = useMutation({
    mutationFn: async () => {
      if (!paymentForm.policyId) {
        throw new Error("Please select a policy");
      }

      const amount = Number(paymentForm.amount);
      if (!Number.isFinite(amount) || amount <= 0) {
        throw new Error("Payment amount must be greater than zero");
      }

      if (!paymentForm.paymentDate) {
        throw new Error("Payment date is required");
      }

      await createPayment(paymentForm.policyId, {
        amount,
        currencyId: selectedPolicy?.currencyId ?? "",
        paymentDate: new Date(`${paymentForm.paymentDate}T00:00:00`).toISOString(),
        method: paymentForm.method,
        status: paymentForm.status,
      });
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["payments"] }),
        queryClient.invalidateQueries({ queryKey: ["policies"] }),
      ]);

      setIsRecordDialogOpen(false);
      setPaymentError("");
      setPaymentForm(initialPaymentForm);
      toast.success("Payment recorded");
    },
    onError: (mutationError) => {
      setPaymentError(mutationError instanceof Error ? mutationError.message : "Failed to record payment");
    },
  });

  const handleOpenRecordDialog = () => {
    const defaultPolicy = activePolicies[0];

    setPaymentForm({
      ...initialPaymentForm,
      policyId: defaultPolicy?.id ?? "",
      amount: defaultPolicy ? String(Math.max(0, defaultPolicy.finalPremium - (completedAmountByPolicy[defaultPolicy.id] ?? 0))) : "",
    });
    setPaymentError("");
    setIsRecordDialogOpen(true);
  };

  const handleRecordPayment = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setPaymentError("");
    await recordPaymentMutation.mutateAsync();
  };

  return (
    <div className="space-y-6">
      <PageHeader title="Payments" description="Track premium collections and record policy payments">
        <button
          type="button"
          onClick={handleOpenRecordDialog}
          disabled={arePoliciesLoading || activePolicies.length === 0}
          className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-success text-success-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:cursor-not-allowed disabled:opacity-60"
        >
          <Plus className="h-4 w-4" /> Record Payment
        </button>
      </PageHeader>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {stats.map((stat, index) => (
          <motion.div
            key={stat.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.05 }}
            className="glass-card p-4"
          >
            <p className="text-xs text-muted-foreground">{stat.label}</p>
            <p className={cn("text-xl font-bold mt-1", stat.color)}>{stat.value}</p>
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
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search payments..."
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5">
            {statusTabs.map((status) => (
              <button
                key={status}
                onClick={() => setTab(status)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize",
                  tab === status ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
                )}
              >
                {status}
              </button>
            ))}
          </div>
        </div>
        <p className="mt-3 text-xs text-muted-foreground">Showing {filtered.length} of {payments.length} payments</p>
      </div>

      {isLoading ? (
        <div className="glass-card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="table-premium">
              <thead>
                <tr>
                  <th>Client</th>
                  <th>Policy</th>
                  <th>Amount</th>
                  <th>Payment Date</th>
                  <th>Method</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {Array.from({ length: 6 }).map((_, index) => (
                  <tr key={index}>
                    <td><Skeleton className="h-4 w-32" /></td>
                    <td><Skeleton className="h-4 w-28" /></td>
                    <td><Skeleton className="h-4 w-24" /></td>
                    <td><Skeleton className="h-4 w-36" /></td>
                    <td><Skeleton className="h-4 w-20" /></td>
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
            <h3 className="text-lg font-semibold">Could not load payments</h3>
            <p className="text-sm text-muted-foreground mt-1 max-w-md">
              {error instanceof Error ? error.message : "The payments request failed."}
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
          icon={CreditCard}
          title="No payments found"
          description={search.trim() ? "Try a different search term or switch to another status." : "No payments have been recorded yet."}
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
                  <th>Client</th>
                  <th>Policy</th>
                  <th>Amount</th>
                  <th>Payment Date</th>
                  <th>Method</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((payment, index) => (
                  <motion.tr
                    key={payment.id}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: index * 0.03 }}
                    className="hover:bg-muted/20 transition-colors"
                  >
                    <td className="text-sm font-medium">{payment.clientName}</td>
                    <td>
                      <Link to={`/policies/${payment.policyId}`} className="font-mono text-xs hover:text-primary transition-colors">
                        {payment.policyNumber}
                      </Link>
                    </td>
                    <td className="text-sm font-semibold">{formatMoney(payment.amount, payment.currencyCode)}</td>
                    <td>
                      <div className="flex items-center gap-1 text-xs text-muted-foreground whitespace-nowrap">
                        <Calendar className="h-3 w-3" />
                        {formatDateTime(payment.paymentDate)}
                      </div>
                    </td>
                    <td className="text-xs">{payment.methodLabel}</td>
                    <td><StatusChip status={payment.status} /></td>
                  </motion.tr>
                ))}
              </tbody>
            </table>
          </div>
        </motion.div>
      )}

      <Dialog open={isRecordDialogOpen} onOpenChange={setIsRecordDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Record payment</DialogTitle>
            <DialogDescription>
              Record a payment against an active policy.
            </DialogDescription>
          </DialogHeader>

          <form className="space-y-4" onSubmit={handleRecordPayment}>
            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground" htmlFor="payment-policy">
                Policy
              </label>
              <select
                id="payment-policy"
                value={paymentForm.policyId}
                onChange={(event) => {
                  const policyId = event.target.value;
                  const policy = activePolicies.find((item) => item.id === policyId);

                  setPaymentForm((current) => ({
                    ...current,
                    policyId,
                    amount: policy ? String(Math.max(0, policy.finalPremium - (completedAmountByPolicy[policy.id] ?? 0))) : current.amount,
                  }));
                }}
                disabled={recordPaymentMutation.isPending || activePolicies.length === 0}
                className="w-full h-10 rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
              >
                <option value="">Select a policy</option>
                {activePolicies.map((policy) => (
                  <option key={policy.id} value={policy.id}>
                    {policy.policyNumber} · {policy.clientName}
                  </option>
                ))}
              </select>
              {selectedPolicy ? (
                <p className="text-xs text-muted-foreground">
                  Remaining premium: {formatMoney(selectedPolicyRemainingPremium, selectedPolicy.currencyCode)}
                </p>
              ) : null}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="payment-amount">
                  Amount
                </label>
                <input
                  id="payment-amount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={paymentForm.amount}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, amount: event.target.value }))}
                  disabled={recordPaymentMutation.isPending}
                  className="w-full h-10 rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="payment-date">
                  Payment date
                </label>
                <input
                  id="payment-date"
                  type="date"
                  value={paymentForm.paymentDate}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, paymentDate: event.target.value }))}
                  disabled={recordPaymentMutation.isPending}
                  className="w-full h-10 rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="payment-method">
                  Method
                </label>
                <select
                  id="payment-method"
                  value={paymentForm.method}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, method: event.target.value as PaymentFormState["method"] }))}
                  disabled={recordPaymentMutation.isPending}
                  className="w-full h-10 rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                >
                  <option value="Cash">Cash</option>
                  <option value="Card">Card</option>
                  <option value="BankTransfer">Bank Transfer</option>
                </select>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="payment-status">
                  Status
                </label>
                <select
                  id="payment-status"
                  value={paymentForm.status}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, status: event.target.value as PaymentFormState["status"] }))}
                  disabled={recordPaymentMutation.isPending}
                  className="w-full h-10 rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                >
                  <option value="Completed">Completed</option>
                  <option value="Pending">Pending</option>
                  <option value="Failed">Failed</option>
                </select>
              </div>
            </div>

            {paymentError ? (
              <div className="rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-sm text-destructive">
                {paymentError}
              </div>
            ) : null}

            {activePolicies.length === 0 ? (
              <div className="rounded-lg border border-border bg-muted/30 px-3 py-2 text-sm text-muted-foreground">
                There are no active policies available for payment recording.
              </div>
            ) : null}

            <DialogFooter>
              <button
                type="button"
                onClick={() => setIsRecordDialogOpen(false)}
                className="h-10 rounded-lg border border-border px-4 text-sm font-medium hover:bg-muted transition-colors"
                disabled={recordPaymentMutation.isPending}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="h-10 rounded-lg gradient-success px-4 text-sm font-medium text-success-foreground hover:opacity-90 transition-opacity disabled:cursor-not-allowed disabled:opacity-60"
                disabled={recordPaymentMutation.isPending || activePolicies.length === 0}
              >
                {recordPaymentMutation.isPending ? "Recording..." : "Record payment"}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
