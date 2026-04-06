import { useMemo, useState, type FormEvent } from "react";
import { motion } from "framer-motion";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertCircle, CheckCircle2, Clock3, CreditCard, Plus, Search, ShieldAlert, XCircle } from "lucide-react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/ui/PageHeader";
import { StatusChip } from "@/components/ui/StatusChip";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useRole } from "@/contexts/RoleContext";
import {
  approveClaim,
  getAdminClaims,
  getBrokerClaims,
  moveClaimToReview,
  payClaim,
  rejectClaim,
} from "@/lib/claims/claim.api";
import type { ClaimListItem, NormalizedClaimStatus } from "@/lib/claims/claim.types";
import { cn, formatMoney } from "@/lib/utils";

const statusTabs = ["all", "submitted", "under_review", "approved", "rejected", "paid"] as const;

type StatusTab = typeof statusTabs[number];
type ClaimDialogMode = "approve" | "reject";

function formatDateOnly(value?: string | null) {
  if (!value) {
    return "-";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleDateString("en-GB", {
    day: "numeric",
    month: "short",
    year: "numeric",
  });
}

function canMoveToReview(status: NormalizedClaimStatus) {
  return status === "submitted";
}

function canApproveOrReject(status: NormalizedClaimStatus) {
  return status === "submitted" || status === "under_review";
}

function canPay(status: NormalizedClaimStatus) {
  return status === "approved";
}

export default function ClaimsPage() {
  const { role } = useRole();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<StatusTab>("all");
  const [dialogMode, setDialogMode] = useState<ClaimDialogMode | null>(null);
  const [selectedClaim, setSelectedClaim] = useState<ClaimListItem | null>(null);
  const [approvedAmount, setApprovedAmount] = useState("");
  const [rejectionReason, setRejectionReason] = useState("");
  const [actionError, setActionError] = useState("");

  const {
    data: claims = [],
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ["claims", role],
    queryFn: () => (role === "admin" ? getAdminClaims() : getBrokerClaims()),
    staleTime: 30000,
  });

  const refreshClaims = async (claim?: ClaimListItem | null) => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["claims"] }),
      claim ? queryClient.invalidateQueries({ queryKey: ["policies", claim.policyId, "claims"] }) : Promise.resolve(),
    ]);
  };

  const actionMutation = useMutation({
    mutationFn: async (payload: { type: "review" | "approve" | "reject" | "pay"; claim: ClaimListItem; approvedAmount?: number; rejectionReason?: string }) => {
      if (payload.type === "review") {
        await moveClaimToReview(payload.claim.id);
        return;
      }

      if (payload.type === "approve") {
        await approveClaim(payload.claim.id, { approvedAmount: payload.approvedAmount ?? 0 });
        return;
      }

      if (payload.type === "reject") {
        await rejectClaim(payload.claim.id, { reason: payload.rejectionReason ?? "" });
        return;
      }

      await payClaim(payload.claim.id);
    },
    onSuccess: async (_data, variables) => {
      await refreshClaims(variables.claim);
      setDialogMode(null);
      setSelectedClaim(null);
      setApprovedAmount("");
      setRejectionReason("");
      setActionError("");

      if (variables.type === "review") {
        toast.success("Claim moved to review");
      } else if (variables.type === "approve") {
        toast.success("Claim approved");
      } else if (variables.type === "reject") {
        toast.success("Claim rejected");
      } else {
        toast.success("Claim marked as paid");
      }
    },
  });

  const filtered = useMemo(
    () => claims.filter((claim) => {
      const normalizedSearch = search.trim().toLowerCase();
      const matchesSearch =
        normalizedSearch.length === 0 ||
        claim.reference.toLowerCase().includes(normalizedSearch) ||
        claim.policyNumber.toLowerCase().includes(normalizedSearch) ||
        claim.clientName.toLowerCase().includes(normalizedSearch) ||
        claim.description?.toLowerCase().includes(normalizedSearch);
      const matchesTab = tab === "all" || claim.status === tab;
      return matchesSearch && matchesTab;
    }),
    [claims, search, tab],
  );

  const summary = useMemo(
    () => [
      { label: "Total Claims", value: claims.length, color: "text-foreground" },
      { label: "Under Review", value: claims.filter((claim) => claim.status === "under_review").length, color: "text-info" },
      { label: "Approved", value: claims.filter((claim) => claim.status === "approved").length, color: "text-success" },
      {
        label: role === "admin" ? "Exposure" : "Estimated Damage",
        value: formatMoney(claims.reduce((sum, claim) => sum + claim.estimatedDamage, 0), claims.find((claim) => claim.currencyCode)?.currencyCode ?? "RON"),
        color: "text-warning",
      },
    ],
    [claims, role],
  );

  const openDialog = (mode: ClaimDialogMode, claim: ClaimListItem) => {
    setDialogMode(mode);
    setSelectedClaim(claim);
    setApprovedAmount(claim.approvedAmount != null ? String(claim.approvedAmount) : String(claim.estimatedDamage));
    setRejectionReason("");
    setActionError("");
  };

  const closeDialog = () => {
    setDialogMode(null);
    setSelectedClaim(null);
    setApprovedAmount("");
    setRejectionReason("");
    setActionError("");
  };

  const handleReview = async (claim: ClaimListItem) => {
    try {
      await actionMutation.mutateAsync({ type: "review", claim });
    } catch (submitError) {
      toast.error(submitError instanceof Error ? submitError.message : "Failed to move claim to review");
    }
  };

  const handlePay = async (claim: ClaimListItem) => {
    try {
      await actionMutation.mutateAsync({ type: "pay", claim });
    } catch (submitError) {
      toast.error(submitError instanceof Error ? submitError.message : "Failed to mark claim as paid");
    }
  };

  const handleDialogSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setActionError("");

    if (!selectedClaim || !dialogMode) {
      return;
    }

    try {
      if (dialogMode === "approve") {
        const numericAmount = Number(approvedAmount);
        if (!Number.isFinite(numericAmount) || numericAmount <= 0) {
          throw new Error("Approved amount must be greater than zero.");
        }

        await actionMutation.mutateAsync({
          type: "approve",
          claim: selectedClaim,
          approvedAmount: numericAmount,
        });
        return;
      }

      if (!rejectionReason.trim()) {
        throw new Error("A rejection reason is required.");
      }

      await actionMutation.mutateAsync({
        type: "reject",
        claim: selectedClaim,
        rejectionReason: rejectionReason.trim(),
      });
    } catch (submitError) {
      setActionError(submitError instanceof Error ? submitError.message : "Failed to update claim.");
    }
  };

    const policyNumberShort = (policyNumber: string) => {
      if (policyNumber.length <= 18) {
        return policyNumber;
      }

      return `${policyNumber.slice(0, 12)}-${policyNumber.slice(12, 18)}`;
    };


  return (
    <div className="space-y-6">
      <PageHeader title="Claims" description={role === "admin" ? "Review, approve, reject, and settle insurance claims" : "Track claim activity across your policies"}>
        {role === "broker" && (
          <Link to="/policies" className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-warning text-warning-foreground text-sm font-medium hover:opacity-90 transition-opacity">
            <Plus className="h-4 w-4" /> Submit Claim
          </Link>
        )}
      </PageHeader>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {summary.map((item, index) => (
          <motion.div
            key={item.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.05 }}
            className="glass-card p-4"
          >
            <p className="text-xs text-muted-foreground">{item.label}</p>
            <p className={cn("text-xl font-bold mt-1", item.color)}>{item.value}</p>
          </motion.div>
        ))}
      </div>

      <div className="glass-card p-4">
        <div className="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search claims..."
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5 overflow-x-auto">
            {statusTabs.map((status) => (
              <button
                key={status}
                onClick={() => setTab(status)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize whitespace-nowrap",
                  tab === status ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
                )}
              >
                {status === "under_review" ? "Under Review" : status}
              </button>
            ))}
          </div>
          <button
            type="button"
            onClick={() => void refetch()}
            className="h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors"
          >
            Refresh
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="rounded-lg border border-border bg-card p-4 text-sm text-muted-foreground">Loading claims...</div>
      ) : isError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
          <div className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4" />
            <span>{error instanceof Error ? error.message : "Failed to load claims."}</span>
          </div>
        </div>
      ) : filtered.length === 0 ? (
        <div className="rounded-lg border border-border bg-card p-6 text-sm text-muted-foreground">
          {claims.length === 0
            ? role === "admin"
              ? "No claims have been recorded yet."
              : "No claims found. Submit claims from an active policy details page."
            : "No claims match the current filters."}
        </div>
      ) : (
        <div className="space-y-3">
          {filtered.map((claim, index) => (
            <motion.div
              key={claim.id}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.05 }}
              className="glass-card-hover p-5"
            >
              <div className="flex flex-col gap-4 lg:flex-row lg:items-center">
                <div className="flex items-center gap-3 flex-1">
                  <div className={cn(
                    "h-10 w-10 rounded-xl flex items-center justify-center",
                    claim.status === "approved" || claim.status === "paid" ? "bg-success/10" :
                    claim.status === "rejected" ? "bg-destructive/10" :
                    claim.status === "under_review" ? "bg-info/10" :
                    "bg-warning/10",
                  )}>
                    <ShieldAlert className={cn(
                      "h-5 w-5",
                      claim.status === "approved" || claim.status === "paid" ? "text-success" :
                      claim.status === "rejected" ? "text-destructive" :
                      claim.status === "under_review" ? "text-info" :
                      "text-warning",
                    )} />
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-xs font-medium">{claim.reference}</span>
                      <StatusChip status={claim.status} />
                    </div>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {claim.clientName} · <Link to={`/policies/${claim.policyId}`} className="hover:text-foreground transition-colors">{policyNumberShort(claim.policyNumber)}</Link>
                    </p>
                  </div>
                </div>
                <div className="flex flex-wrap items-center gap-6 text-xs">
                  <div>
                    <p className="text-muted-foreground">Estimated Damage</p>
                    <p className="text-sm font-bold">{formatMoney(claim.estimatedDamage, claim.currencyCode ?? "RON")}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Incident Date</p>
                    <p className="font-medium">{formatDateOnly(claim.incidentDate)}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Submitted</p>
                    <p className="font-medium">{formatDateOnly(claim.createdAt)}</p>
                  </div>
                </div>
              </div>

              <div className="mt-3 space-y-2 text-xs text-muted-foreground sm:pl-[3.25rem]">
                <p>{claim.description || "No claim description is available on this endpoint."}</p>
                <p>Approved amount: {claim.approvedAmount == null ? "-" : formatMoney(claim.approvedAmount, claim.currencyCode ?? "RON")}</p>
                {claim.rejectionReason ? <p>Rejection reason: {claim.rejectionReason}</p> : null}
              </div>

              {role === "admin" && (
                <div className="mt-4 flex flex-wrap gap-2 sm:pl-[3.25rem]">
                  {canMoveToReview(claim.status) ? (
                    <button
                      type="button"
                      onClick={() => void handleReview(claim)}
                      disabled={actionMutation.isPending}
                      className="inline-flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60"
                    >
                      <Clock3 className="h-4 w-4" /> Move To Review
                    </button>
                  ) : null}
                  {canApproveOrReject(claim.status) ? (
                    <button
                      type="button"
                      onClick={() => openDialog("approve", claim)}
                      disabled={actionMutation.isPending}
                      className="inline-flex items-center gap-2 h-9 px-4 rounded-lg bg-success text-success-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60"
                    >
                      <CheckCircle2 className="h-4 w-4" /> Approve
                    </button>
                  ) : null}
                  {canApproveOrReject(claim.status) ? (
                    <button
                      type="button"
                      onClick={() => openDialog("reject", claim)}
                      disabled={actionMutation.isPending}
                      className="inline-flex items-center gap-2 h-9 px-4 rounded-lg border border-destructive/30 text-destructive text-sm font-medium hover:bg-destructive/5 transition-colors disabled:opacity-60"
                    >
                      <XCircle className="h-4 w-4" /> Reject
                    </button>
                  ) : null}
                  {canPay(claim.status) ? (
                    <button
                      type="button"
                      onClick={() => void handlePay(claim)}
                      disabled={actionMutation.isPending}
                      className="inline-flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60"
                    >
                      <CreditCard className="h-4 w-4" /> Mark As Paid
                    </button>
                  ) : null}
                </div>
              )}
            </motion.div>
          ))}
        </div>
      )}

      <Dialog open={dialogMode !== null} onOpenChange={(open) => {
        if (!open) {
          closeDialog();
        }
      }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{dialogMode === "approve" ? "Approve claim" : "Reject claim"}</DialogTitle>
            <DialogDescription>
              {selectedClaim ? `${selectedClaim.reference} for policy ${policyNumberShort(selectedClaim.policyNumber)}` : "Update the selected claim."}
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleDialogSubmit} className="space-y-4">
            {dialogMode === "approve" ? (
              <div className="space-y-2">
                <label htmlFor="approvedAmount" className="text-sm font-medium">Approved Amount</label>
                <input
                  id="approvedAmount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={approvedAmount}
                  onChange={(event) => setApprovedAmount(event.target.value)}
                  disabled={actionMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>
            ) : (
              <div className="space-y-2">
                <label htmlFor="rejectionReason" className="text-sm font-medium">Rejection Reason</label>
                <textarea
                  id="rejectionReason"
                  rows={4}
                  value={rejectionReason}
                  onChange={(event) => setRejectionReason(event.target.value)}
                  disabled={actionMutation.isPending}
                  className="w-full px-3 py-2 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60 resize-none"
                />
              </div>
            )}

            {actionError ? <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{actionError}</div> : null}

            <DialogFooter>
              <button
                type="button"
                onClick={closeDialog}
                disabled={actionMutation.isPending}
                className="h-10 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={actionMutation.isPending}
                className={cn(
                  "h-10 px-4 rounded-lg text-sm font-medium transition-colors disabled:opacity-60",
                  dialogMode === "approve"
                    ? "bg-success text-success-foreground hover:bg-success/90"
                    : "bg-destructive text-destructive-foreground hover:bg-destructive/90",
                )}
              >
                {actionMutation.isPending
                  ? dialogMode === "approve"
                    ? "Approving..."
                    : "Rejecting..."
                  : dialogMode === "approve"
                    ? "Approve Claim"
                    : "Reject Claim"}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
