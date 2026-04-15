import { FormEvent, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AlertCircle,
  CalendarDays,
  CreditCard,
  FileClock,
  FilePlus2,
  FileStack,
  Landmark,
  PencilLine,
  Shield,
  UserRound,
} from "lucide-react";
import { Link, useParams } from "react-router-dom";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { PageHeader } from "@/components/ui/PageHeader";
import { Skeleton } from "@/components/ui/skeleton";
import { StatusChip } from "@/components/ui/StatusChip";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { toast } from "@/components/ui/sonner";
import { useRole } from "@/contexts/RoleContext";
import { createPayment, getPolicyPayments } from "@/lib/payments/payment.api";
import {
  activatePolicy,
  cancelPolicy,
  createPolicyClaim,
  createPolicyEndorsement,
  getPolicyById,
  getPolicyClaims,
  getPolicyEndorsements,
  getPolicyVersions,
  normalizePolicyStatus,
} from "@/lib/policies/policy.api";
import { formatMoney } from "@/lib/utils";

type CancelFormState = {
  cancellationDate: string;
  cancellationReason: string;
};

type EndorsementFormState = {
  endorsementType: "0" | "1" | "3";
  effectiveDate: string;
  reason: string;
  newBasePremium: string;
  newStartDate: string;
  newEndDate: string;
  manualAdjustementPercentage: string;
};

type ClaimFormState = {
  description: string;
  incidentDate: string;
  estimatedDamage: string;
};

type PaymentFormState = {
  amount: string;
  paymentDate: string;
  method: "Cash" | "Card" | "BankTransfer";
  status: "Completed" | "Pending" | "Failed";
};

const initialCancelForm: CancelFormState = {
  cancellationDate: "",
  cancellationReason: "",
};

const initialEndorsementForm: EndorsementFormState = {
  endorsementType: "0",
  effectiveDate: "",
  reason: "",
  newBasePremium: "",
  newStartDate: "",
  newEndDate: "",
  manualAdjustementPercentage: "",
};

const initialClaimForm: ClaimFormState = {
  description: "",
  incidentDate: "",
  estimatedDamage: "",
};

const initialPaymentForm: PaymentFormState = {
  amount: "",
  paymentDate: new Date().toISOString().slice(0, 10),
  method: "BankTransfer",
  status: "Completed",
};

function formatDateOnly(value?: string | null) {
  if (!value) {
    return "-";
  }

  const [year, month, day] = value.split("-").map(Number);

  if (!year || !month || !day) {
    const parsedDate = new Date(value);
    if (Number.isNaN(parsedDate.getTime())) {
      return value;
    }

    return parsedDate.toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    });
  }

  return new Date(year, month - 1, day).toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

function formatDateTime(value?: string | null) {
  if (!value) {
    return "-";
  }

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

function formatStatusChipValue(status: string) {
  return status.trim().toLowerCase().replace(/\s+/g, "_");
}

function getEndorsementTypeLabel(value: EndorsementFormState["endorsementType"]) {
  if (value === "1") {
    return "Period extension";
  }

  if (value === "3") {
    return "Manual adjustment";
  }

  return "Insured value change";
}

export default function PolicyDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { role } = useRole();
  const queryClient = useQueryClient();

  const [isCancelDialogOpen, setIsCancelDialogOpen] = useState(false);
  const [isEndorsementDialogOpen, setIsEndorsementDialogOpen] = useState(false);
  const [isClaimDialogOpen, setIsClaimDialogOpen] = useState(false);
  const [isPaymentDialogOpen, setIsPaymentDialogOpen] = useState(false);
  const [cancelForm, setCancelForm] = useState(initialCancelForm);
  const [endorsementForm, setEndorsementForm] = useState(initialEndorsementForm);
  const [claimForm, setClaimForm] = useState(initialClaimForm);
  const [paymentForm, setPaymentForm] = useState(initialPaymentForm);
  const [cancelError, setCancelError] = useState("");
  const [endorsementError, setEndorsementError] = useState("");
  const [claimError, setClaimError] = useState("");
  const [paymentError, setPaymentError] = useState("");

  const { data: policy, isLoading, isError, error } = useQuery({
    queryKey: ["policies", role, id],
    queryFn: () => getPolicyById(id!, role),
    enabled: Boolean(id),
    staleTime: 30000,
  });

  const {
    data: versions = [],
    isLoading: areVersionsLoading,
    isError: areVersionsErrored,
    error: versionsError,
  } = useQuery({
    queryKey: ["policies", role, id, "versions"],
    queryFn: () => getPolicyVersions(id!, role),
    enabled: Boolean(id),
    staleTime: 30000,
  });

  const {
    data: endorsements = [],
    isLoading: areEndorsementsLoading,
    isError: areEndorsementsErrored,
    error: endorsementsError,
  } = useQuery({
    queryKey: ["policies", role, id, "endorsements"],
    queryFn: () => getPolicyEndorsements(id!, role),
    enabled: Boolean(id),
    staleTime: 30000,
  });

  const {
    data: claims = [],
    isLoading: areClaimsLoading,
    isError: areClaimsErrored,
    error: claimsError,
  } = useQuery({
    queryKey: ["policies", role, id, "claims"],
    queryFn: () => getPolicyClaims(id!, role),
    enabled: Boolean(id),
    staleTime: 30000,
  });

  const {
    data: payments = [],
    isLoading: arePaymentsLoading,
    isError: arePaymentsErrored,
    error: paymentsError,
  } = useQuery({
    queryKey: ["policies", role, id, "payments"],
    queryFn: () => getPolicyPayments(id!, role),
    enabled: Boolean(id),
    staleTime: 30000,
  });

  const refreshPolicyQueries = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["policies"] }),
      queryClient.invalidateQueries({ queryKey: ["policies", id] }),
      queryClient.invalidateQueries({ queryKey: ["policies", id, "versions"] }),
      queryClient.invalidateQueries({ queryKey: ["policies", id, "endorsements"] }),
      queryClient.invalidateQueries({ queryKey: ["policies", id, "claims"] }),
      queryClient.invalidateQueries({ queryKey: ["policies", id, "payments"] }),
      queryClient.invalidateQueries({ queryKey: ["payments"] }),
    ]);
  };

  const activatePolicyMutation = useMutation({
    mutationFn: async () => {
      if (!id) {
        throw new Error("Policy id is missing");
      }

      await activatePolicy(id);
    },
    onSuccess: async () => {
      await refreshPolicyQueries();
      toast.success("Policy activated");
    },
  });

  const cancelPolicyMutation = useMutation({
    mutationFn: async () => {
      if (!id) {
        throw new Error("Policy id is missing");
      }

      await cancelPolicy(id, {
        cancellationDate: cancelForm.cancellationDate || null,
        cancellationReason: cancelForm.cancellationReason,
      });
    },
    onSuccess: async () => {
      await refreshPolicyQueries();
      setIsCancelDialogOpen(false);
      setCancelForm(initialCancelForm);
      setCancelError("");
      toast.success("Policy cancelled");
    },
  });

  const createEndorsementMutation = useMutation({
    mutationFn: async () => {
      if (!id) {
        throw new Error("Policy id is missing");
      }

      const endorsementType = Number(endorsementForm.endorsementType) as 0 | 1 | 3;

      await createPolicyEndorsement(id, {
        endorsementType,
        effectiveDate: endorsementForm.effectiveDate,
        reason: endorsementForm.reason,
        newBasePremium: endorsementType === 0 ? Number(endorsementForm.newBasePremium) : null,
        newStartDate: endorsementType === 1 ? endorsementForm.newStartDate : null,
        newEndDate: endorsementType === 1 ? endorsementForm.newEndDate : null,
        manualAdjustementPercentage: endorsementType === 3 ? Number(endorsementForm.manualAdjustementPercentage) : null,
      });
    },
    onSuccess: async () => {
      await refreshPolicyQueries();
      setIsEndorsementDialogOpen(false);
      setEndorsementForm(initialEndorsementForm);
      setEndorsementError("");
      toast.success("Endorsement created");
    },
  });

  const createClaimMutation = useMutation({
    mutationFn: async () => {
      if (!id) {
        throw new Error("Policy id is missing");
      }

      await createPolicyClaim(id, {
        description: claimForm.description,
        incidentDate: claimForm.incidentDate,
        estimatedDamage: Number(claimForm.estimatedDamage),
      });
    },
    onSuccess: async () => {
      await refreshPolicyQueries();
      setIsClaimDialogOpen(false);
      setClaimForm(initialClaimForm);
      setClaimError("");
      toast.success("Claim submitted");
    },
  });

  const createPaymentMutation = useMutation({
    mutationFn: async () => {
      if (!id) {
        throw new Error("Policy id is missing");
      }

      if (!activeVersion?.currencyId) {
        throw new Error("Active policy version currency is not available.");
      }

      const amount = Number(paymentForm.amount);
      if (!Number.isFinite(amount) || amount <= 0) {
        throw new Error("Payment amount must be greater than zero.");
      }

      if (!paymentForm.paymentDate) {
        throw new Error("Payment date is required.");
      }

      await createPayment(id, {
        amount,
        currencyId: activeVersion.currencyId,
        paymentDate: new Date(`${paymentForm.paymentDate}T00:00:00`).toISOString(),
        method: paymentForm.method,
        status: paymentForm.status,
      });
    },
    onSuccess: async () => {
      await refreshPolicyQueries();
      setIsPaymentDialogOpen(false);
      setPaymentForm(initialPaymentForm);
      setPaymentError("");
      toast.success("Payment recorded");
    },
  });

  const policyStatus = useMemo(
    () => (policy ? normalizePolicyStatus(policy.policyStatus) : "draft"),
    [policy],
  );

  const activeVersion = useMemo(
    () => versions.find((version) => version.versionNumber === policy?.versionNumber) ?? null,
    [policy?.versionNumber, versions],
  );

  const totalAdjustmentAmount = useMemo(
    () => policy?.policyAdjustments.reduce((sum, adjustment) => sum + adjustment.amount, 0) ?? 0,
    [policy?.policyAdjustments],
  );

  const completedPaidAmount = useMemo(
    () => payments.filter((payment) => payment.status === "paid").reduce((sum, payment) => sum + payment.amount, 0),
    [payments],
  );

  const pendingPaymentAmount = useMemo(
    () => payments.filter((payment) => payment.status === "pending").reduce((sum, payment) => sum + payment.amount, 0),
    [payments],
  );

  const remainingPremium = useMemo(
    () => Math.max(0, (policy?.finalPremium ?? 0) - completedPaidAmount),
    [completedPaidAmount, policy?.finalPremium],
  );

  const handleCancelSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setCancelError("");

    try {
      if (!cancelForm.cancellationReason.trim()) {
        throw new Error("Cancellation reason is required.");
      }

      await cancelPolicyMutation.mutateAsync();
    } catch (submitError) {
      setCancelError(submitError instanceof Error ? submitError.message : "Failed to cancel policy.");
    }
  };

  const handleEndorsementSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setEndorsementError("");

    try {
      if (!endorsementForm.effectiveDate || !endorsementForm.reason.trim()) {
        throw new Error("Effective date and reason are required.");
      }

      if (endorsementForm.endorsementType === "0") {
        const newBasePremium = Number(endorsementForm.newBasePremium);
        if (!Number.isFinite(newBasePremium) || newBasePremium <= 0) {
          throw new Error("A valid new base premium is required.");
        }
      }

      if (endorsementForm.endorsementType === "1") {
        if (!endorsementForm.newStartDate || !endorsementForm.newEndDate) {
          throw new Error("New start date and new end date are required.");
        }

        if (endorsementForm.newEndDate <= endorsementForm.newStartDate) {
          throw new Error("The new end date must be after the new start date.");
        }
      }

      if (endorsementForm.endorsementType === "3") {
        const percentage = Number(endorsementForm.manualAdjustementPercentage);
        if (!Number.isFinite(percentage)) {
          throw new Error("A manual adjustment percentage is required.");
        }
      }

      if (!policy) {
        throw new Error("Policy details are not loaded.");
      }

      if (endorsementForm.effectiveDate < policy.startDate || endorsementForm.effectiveDate > policy.endDate) {
        throw new Error(`Effective date must be between ${formatDateOnly(policy.startDate)} and ${formatDateOnly(policy.endDate)}.`);
      }

      await createEndorsementMutation.mutateAsync();
    } catch (submitError) {
      setEndorsementError(submitError instanceof Error ? submitError.message : "Failed to create endorsement.");
    }
  };

  const handleClaimSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setClaimError("");

    try {
      if (!claimForm.description.trim() || !claimForm.incidentDate) {
        throw new Error("Description and incident date are required.");
      }

      const estimatedDamage = Number(claimForm.estimatedDamage);
      if (!Number.isFinite(estimatedDamage) || estimatedDamage <= 0) {
        throw new Error("Estimated damage must be greater than zero.");
      }

      await createClaimMutation.mutateAsync();
    } catch (submitError) {
      setClaimError(submitError instanceof Error ? submitError.message : "Failed to submit claim.");
    }
  };

  const handlePaymentSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setPaymentError("");

    try {
      const amount = Number(paymentForm.amount);

      if (!Number.isFinite(amount) || amount <= 0) {
        throw new Error("Payment amount must be greater than zero.");
      }

      if (!paymentForm.paymentDate) {
        throw new Error("Payment date is required.");
      }

      await createPaymentMutation.mutateAsync();
    } catch (submitError) {
      setPaymentError(submitError instanceof Error ? submitError.message : "Failed to record payment.");
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-64" />
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="rounded-lg border border-border bg-card p-4 space-y-3">
              <Skeleton className="h-5 w-28" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-3/4" />
            </div>
          ))}
        </div>
      </div>
    );
  }
    const policyNumberShort = (policyNumber: string) => {
      if (policyNumber.length <= 18) {
        return policyNumber;
      }

      return `${policyNumber.slice(0, 12)}-${policyNumber.slice(12, 18)}`;
    };


  if (isError || !policy) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-3 rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-destructive">
          <AlertCircle className="h-5 w-5" />
          <div>
            <p className="font-medium">Could not load policy details</p>
            <p className="text-sm">{error instanceof Error ? error.message : "The policy details request failed."}</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Policy Details" description="Review the active version, policy history, and backend actions">
        {role === "broker" && policyStatus === "draft" && (
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <button type="button" className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
                <Shield className="h-4 w-4" /> Activate Policy
              </button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Activate this policy?</AlertDialogTitle>
                <AlertDialogDescription>
                  The backend only allows activation for draft policies with valid dates and premium values.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Back</AlertDialogCancel>
                <AlertDialogAction
                  onClick={(event) => {
                    event.preventDefault();
                    void activatePolicyMutation.mutateAsync();
                  }}
                  disabled={activatePolicyMutation.isPending}
                >
                  {activatePolicyMutation.isPending ? "Activating..." : "Activate"}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        )}

        {role === "broker" && policyStatus === "active" && (
          <>
            <button
              type="button"
              onClick={() => {
                setEndorsementForm((current) => ({
                  ...current,
                  effectiveDate: policy.startDate,
                }));
                setEndorsementError("");
                setIsEndorsementDialogOpen(true);
              }}
              className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity"
            >
              <PencilLine className="h-4 w-4" /> Add Endorsement
            </button>
            <button
              type="button"
              onClick={() => setIsClaimDialogOpen(true)}
              className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors"
            >
              <FilePlus2 className="h-4 w-4" /> Submit Claim
            </button>
            <button
              type="button"
              onClick={() => {
                setPaymentForm((current) => ({
                  ...current,
                  amount: remainingPremium > 0 ? String(remainingPremium) : current.amount,
                  paymentDate: new Date().toISOString().slice(0, 10),
                }));
                setPaymentError("");
                setIsPaymentDialogOpen(true);
              }}
              disabled={remainingPremium <= 0 || !activeVersion?.currencyId}
              className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
            >
              <CreditCard className="h-4 w-4" /> Record Payment
            </button>
            <button
              type="button"
              onClick={() => setIsCancelDialogOpen(true)}
              className="flex items-center gap-2 h-9 px-4 rounded-lg border border-destructive/30 text-destructive text-sm font-medium hover:bg-destructive/5 transition-colors"
            >
              <AlertCircle className="h-4 w-4" /> Cancel Policy
            </button>
          </>
        )}

        <Link to="/policies" className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors">
          Back to Policies
        </Link>
      </PageHeader>

      <div className="flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h2 className="text-2xl font-bold font-mono">{policyNumberShort(policy.policyNumber)}</h2>
            <StatusChip status={policyStatus} />
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            Version {policy.versionNumber} · {formatDateOnly(policy.startDate)} to {formatDateOnly(policy.endDate)}
          </p>
        </div>
        <div className="text-sm text-muted-foreground">
          {role === "broker" && activatePolicyMutation.isError && activatePolicyMutation.error instanceof Error ? activatePolicyMutation.error.message : null}
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <div className="rounded-lg border border-border bg-card p-4 space-y-3">
          <h3 className="font-semibold">Coverage</h3>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <CalendarDays className="h-4 w-4" /> {formatDateOnly(policy.startDate)} to {formatDateOnly(policy.endDate)}
          </div>
          <div className="text-sm text-muted-foreground">Currency: {policy.currencyName} ({policy.currencyCode})</div>
          <div className="text-sm text-muted-foreground">Current version: {policy.versionNumber}</div>
        </div>

        <div className="rounded-lg border border-border bg-card p-4 space-y-3">
          <h3 className="font-semibold">Premium</h3>
          <div className="text-sm text-muted-foreground">Base premium: {formatMoney(policy.basePremium, policy.currencyCode)}</div>
          <div className="text-sm text-muted-foreground">Final premium: {formatMoney(policy.finalPremium, policy.currencyCode)}</div>
          <div className="text-sm text-muted-foreground">Adjustments total: {formatMoney(totalAdjustmentAmount, policy.currencyCode)}</div>
        </div>

        <div className="rounded-lg border border-border bg-card p-4 space-y-3">
          <h3 className="font-semibold">Client & Building</h3>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <UserRound className="h-4 w-4" />
            <Link to={`/clients/${policy.client.id}`} className="hover:text-foreground transition-colors">{policy.client.name}</Link>
          </div>
          <div className="text-sm text-muted-foreground">
            <Link to={`/buildings/${policy.building.id}`} className="hover:text-foreground transition-colors">
              {policy.building.address.street} {policy.building.address.number}, {policy.building.address.cityName}
            </Link>
          </div>
          <div className="text-sm text-muted-foreground">Client email: {policy.client.email}</div>
        </div>

        <div className="rounded-lg border border-border bg-card p-4 space-y-3">
          <h3 className="font-semibold">Broker</h3>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Landmark className="h-4 w-4" /> {policy.broker.name}
          </div>
          <div className="text-sm text-muted-foreground">Broker code: {policy.broker.brokerCode}</div>
          <div className="text-sm text-muted-foreground">Broker status: {String(policy.broker.brokerStatus)}</div>
        </div>
      </div>

      <Tabs defaultValue="adjustments" className="space-y-4">
        <TabsList className="grid h-auto grid-cols-2 md:grid-cols-5">
          <TabsTrigger value="adjustments">Adjustments</TabsTrigger>
          <TabsTrigger value="versions">Versions</TabsTrigger>
          <TabsTrigger value="endorsements">Endorsements</TabsTrigger>
          <TabsTrigger value="claims">Claims</TabsTrigger>
          <TabsTrigger value="payments">Payments</TabsTrigger>
        </TabsList>

        <TabsContent value="adjustments">
          {policy.policyAdjustments.length === 0 ? (
            <EmptyState icon={FileClock} title="No adjustments" description="The active version does not have any premium adjustments." />
          ) : (
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {policy.policyAdjustments.map((adjustment) => (
                <div key={adjustment.id} className="rounded-lg border border-border bg-card p-4 space-y-2">
                  <h3 className="font-semibold">{adjustment.name}</h3>
                  <p className="text-sm text-muted-foreground">Type: {String(adjustment.adjustmentType)}</p>
                  <p className="text-sm text-muted-foreground">Percentage: {adjustment.percentage}%</p>
                  <p className="text-sm font-semibold">Amount: {formatMoney(adjustment.amount, policy.currencyCode)}</p>
                </div>
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="versions">
          {areVersionsLoading ? (
            <div className="rounded-lg border border-border bg-card p-4 text-sm text-muted-foreground">Loading policy versions...</div>
          ) : areVersionsErrored ? (
            <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
              {versionsError instanceof Error ? versionsError.message : "Failed to load policy versions."}
            </div>
          ) : versions.length === 0 ? (
            <EmptyState icon={FileStack} title="No versions" description="This policy does not have any recorded versions yet." />
          ) : (
            <div className="grid gap-4 md:grid-cols-2">
              {versions
                .slice()
                .sort((left, right) => right.versionNumber - left.versionNumber)
                .map((version) => (
                  <div key={version.id} className="rounded-lg border border-border bg-card p-4 space-y-2">
                    <div className="flex items-center justify-between gap-3">
                      <h3 className="font-semibold">Version {version.versionNumber}</h3>
                      {activeVersion?.id === version.id && <StatusChip status="active" />}
                    </div>
                    <p className="text-sm text-muted-foreground">Coverage: {formatDateOnly(version.startDate)} to {formatDateOnly(version.endDate)}</p>
                    <p className="text-sm text-muted-foreground">Base premium: {formatMoney(version.basePremium, version.currencyCode)}</p>
                    <p className="text-sm text-muted-foreground">Final premium: {formatMoney(version.finalPremium, version.currencyCode)}</p>
                    <p className="text-sm text-muted-foreground">Created by: {version.createdBy}</p>
                    <p className="text-sm text-muted-foreground">Created at: {formatDateTime(version.createdAt)}</p>
                  </div>
                ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="endorsements">
          {areEndorsementsLoading ? (
            <div className="rounded-lg border border-border bg-card p-4 text-sm text-muted-foreground">Loading endorsements...</div>
          ) : areEndorsementsErrored ? (
            <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
              {endorsementsError instanceof Error ? endorsementsError.message : "Failed to load endorsements."}
            </div>
          ) : endorsements.length === 0 ? (
            <EmptyState icon={PencilLine} title="No endorsements" description="No endorsement history is recorded for this policy yet." />
          ) : (
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {endorsements
                .slice()
                .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
                .map((endorsement) => (
                  <div key={endorsement.id} className="rounded-lg border border-border bg-card p-4 space-y-2">
                    <h3 className="font-semibold">Version {endorsement.versionNumber}</h3>
                    <p className="text-sm text-muted-foreground">Effective date: {formatDateOnly(endorsement.effectiveDate)}</p>
                    <p className="text-sm text-muted-foreground">Created by: {endorsement.createdBy}</p>
                    <p className="text-sm text-muted-foreground">Created at: {formatDateTime(endorsement.createdAt)}</p>
                  </div>
                ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="claims">
          {areClaimsLoading ? (
            <div className="rounded-lg border border-border bg-card p-4 text-sm text-muted-foreground">Loading claims...</div>
          ) : areClaimsErrored ? (
            <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
              {claimsError instanceof Error ? claimsError.message : "Failed to load claims."}
            </div>
          ) : claims.length === 0 ? (
            <EmptyState icon={FilePlus2} title="No claims" description="No claims have been submitted against this policy yet." />
          ) : (
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {claims
                .slice()
                .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
                .map((claim) => (
                  <div key={claim.id} className="rounded-lg border border-border bg-card p-4 space-y-3">
                    <div className="flex items-center justify-between gap-3">
                      <h3 className="font-semibold">Claim</h3>
                      <StatusChip status={formatStatusChipValue(claim.status)} />
                    </div>
                    <p className="text-sm text-muted-foreground">{claim.description}</p>
                    <p className="text-sm text-muted-foreground">Incident date: {formatDateOnly(claim.incidentDate)}</p>
                    <p className="text-sm text-muted-foreground">Estimated damage: {formatMoney(claim.estimatedDamage, policy.currencyCode)}</p>
                    <p className="text-sm text-muted-foreground">Approved amount: {claim.approvedAmount == null ? "-" : formatMoney(claim.approvedAmount, policy.currencyCode)}</p>
                    <p className="text-sm text-muted-foreground">Submitted: {formatDateOnly(claim.createdAt)}</p>
                  </div>
                ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="payments">
          {arePaymentsLoading ? (
            <div className="rounded-lg border border-border bg-card p-4 text-sm text-muted-foreground">Loading payments...</div>
          ) : arePaymentsErrored ? (
            <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
              {paymentsError instanceof Error ? paymentsError.message : "Failed to load payments."}
            </div>
          ) : payments.length === 0 ? (
            <EmptyState icon={CreditCard} title="No payments" description="No payments have been recorded for this policy yet." />
          ) : (
            <div className="space-y-4">
              <div className="grid gap-4 md:grid-cols-3">
                <div className="rounded-lg border border-border bg-card p-4 space-y-2">
                  <h3 className="text-sm font-semibold">Completed</h3>
                  <p className="text-xl font-bold text-success">{formatMoney(completedPaidAmount, policy.currencyCode)}</p>
                </div>
                <div className="rounded-lg border border-border bg-card p-4 space-y-2">
                  <h3 className="text-sm font-semibold">Pending</h3>
                  <p className="text-xl font-bold text-warning">{formatMoney(pendingPaymentAmount, policy.currencyCode)}</p>
                </div>
                <div className="rounded-lg border border-border bg-card p-4 space-y-2">
                  <h3 className="text-sm font-semibold">Remaining</h3>
                  <p className="text-xl font-bold">{formatMoney(remainingPremium, policy.currencyCode)}</p>
                </div>
              </div>

              <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
                {payments
                  .slice()
                  .sort((left, right) => new Date(right.paymentDate).getTime() - new Date(left.paymentDate).getTime())
                  .map((payment) => (
                    <div key={payment.id} className="rounded-lg border border-border bg-card p-4 space-y-3">
                      <div className="flex items-center justify-between gap-3">
                        <h3 className="font-semibold">{formatMoney(payment.amount, payment.currencyCode)}</h3>
                        <StatusChip status={payment.status} />
                      </div>
                      <p className="text-sm text-muted-foreground">Method: {payment.methodLabel}</p>
                      <p className="text-sm text-muted-foreground">Payment date: {formatDateTime(payment.paymentDate)}</p>
                      <p className="text-sm text-muted-foreground">Recorded for {payment.policyNumber}</p>
                    </div>
                  ))}
              </div>
            </div>
          )}
        </TabsContent>
      </Tabs>

      <Dialog open={isCancelDialogOpen} onOpenChange={setIsCancelDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel policy</DialogTitle>
            <DialogDescription>
              Only active policies can be cancelled. The backend will default the cancellation date to today if you leave it blank.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleCancelSubmit} className="space-y-4">
            <div className="space-y-2">
              <label htmlFor="cancellationDate" className="text-sm font-medium">Cancellation Date</label>
              <input
                id="cancellationDate"
                type="date"
                value={cancelForm.cancellationDate}
                onChange={(event) => setCancelForm((current) => ({ ...current, cancellationDate: event.target.value }))}
                disabled={cancelPolicyMutation.isPending}
                className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
              />
            </div>

            <div className="space-y-2">
              <label htmlFor="cancellationReason" className="text-sm font-medium">Cancellation Reason</label>
              <textarea
                id="cancellationReason"
                value={cancelForm.cancellationReason}
                onChange={(event) => setCancelForm((current) => ({ ...current, cancellationReason: event.target.value }))}
                disabled={cancelPolicyMutation.isPending}
                maxLength={500}
                rows={4}
                className="w-full px-3 py-2 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60 resize-none"
              />
            </div>

            {cancelError && <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{cancelError}</div>}

            <DialogFooter>
              <button
                type="button"
                onClick={() => setIsCancelDialogOpen(false)}
                disabled={cancelPolicyMutation.isPending}
                className="h-10 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={cancelPolicyMutation.isPending}
                className="h-10 px-4 rounded-lg bg-destructive text-destructive-foreground text-sm font-medium hover:bg-destructive/90 transition-colors disabled:opacity-60"
              >
                {cancelPolicyMutation.isPending ? "Cancelling..." : "Confirm Cancellation"}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={isEndorsementDialogOpen} onOpenChange={setIsEndorsementDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create endorsement</DialogTitle>
            <DialogDescription>
              The backend currently supports insured value change, period extension, and manual adjustment endorsements.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleEndorsementSubmit} className="space-y-4">
            <div className="space-y-2">
              <label htmlFor="endorsementType" className="text-sm font-medium">Endorsement Type</label>
              <select
                id="endorsementType"
                value={endorsementForm.endorsementType}
                onChange={(event) => setEndorsementForm((current) => ({
                  ...current,
                  endorsementType: event.target.value as EndorsementFormState["endorsementType"],
                  newBasePremium: "",
                  newStartDate: "",
                  newEndDate: "",
                  manualAdjustementPercentage: "",
                }))}
                disabled={createEndorsementMutation.isPending}
                className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
              >
                <option value="0">Insured value change</option>
                <option value="1">Period extension</option>
                <option value="3">Manual adjustment</option>
              </select>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <label htmlFor="effectiveDate" className="text-sm font-medium">Effective Date</label>
                <input
                  id="effectiveDate"
                  type="date"
                  value={endorsementForm.effectiveDate}
                  onChange={(event) => setEndorsementForm((current) => ({ ...current, effectiveDate: event.target.value }))}
                  disabled={createEndorsementMutation.isPending}
                  required
                  min={policy.startDate}
                  max={policy.endDate}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
                <p className="text-xs text-muted-foreground">Policy coverage: {formatDateOnly(policy.startDate)} to {formatDateOnly(policy.endDate)}</p>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium">Type Summary</label>
                <div className="h-10 px-3 rounded-lg border border-border bg-muted/40 text-sm flex items-center text-muted-foreground">
                  {getEndorsementTypeLabel(endorsementForm.endorsementType)}
                </div>
              </div>
            </div>

            <div className="space-y-2">
              <label htmlFor="endorsementReason" className="text-sm font-medium">Reason</label>
              <textarea
                id="endorsementReason"
                value={endorsementForm.reason}
                onChange={(event) => setEndorsementForm((current) => ({ ...current, reason: event.target.value }))}
                disabled={createEndorsementMutation.isPending}
                rows={4}
                className="w-full px-3 py-2 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60 resize-none"
              />
            </div>

            {endorsementForm.endorsementType === "0" && (
              <div className="space-y-2">
                <label htmlFor="newBasePremium" className="text-sm font-medium">New Base Premium</label>
                <input
                  id="newBasePremium"
                  type="number"
                  min="0"
                  step="0.01"
                  value={endorsementForm.newBasePremium}
                  onChange={(event) => setEndorsementForm((current) => ({ ...current, newBasePremium: event.target.value }))}
                  disabled={createEndorsementMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>
            )}

            {endorsementForm.endorsementType === "1" && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label htmlFor="newStartDate" className="text-sm font-medium">New Start Date</label>
                  <input
                    id="newStartDate"
                    type="date"
                    value={endorsementForm.newStartDate}
                    onChange={(event) => setEndorsementForm((current) => ({ ...current, newStartDate: event.target.value }))}
                    disabled={createEndorsementMutation.isPending}
                    className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                  />
                </div>
                <div className="space-y-2">
                  <label htmlFor="newEndDate" className="text-sm font-medium">New End Date</label>
                  <input
                    id="newEndDate"
                    type="date"
                    value={endorsementForm.newEndDate}
                    onChange={(event) => setEndorsementForm((current) => ({ ...current, newEndDate: event.target.value }))}
                    disabled={createEndorsementMutation.isPending}
                    className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                  />
                </div>
              </div>
            )}

            {endorsementForm.endorsementType === "3" && (
              <div className="space-y-2">
                <label htmlFor="manualAdjustementPercentage" className="text-sm font-medium">Manual Adjustment Percentage</label>
                <input
                  id="manualAdjustementPercentage"
                  type="number"
                  step="0.01"
                  value={endorsementForm.manualAdjustementPercentage}
                  onChange={(event) => setEndorsementForm((current) => ({ ...current, manualAdjustementPercentage: event.target.value }))}
                  disabled={createEndorsementMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>
            )}

            {endorsementError && <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{endorsementError}</div>}

            <DialogFooter>
              <button
                type="button"
                onClick={() => setIsEndorsementDialogOpen(false)}
                disabled={createEndorsementMutation.isPending}
                className="h-10 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={createEndorsementMutation.isPending}
                className="h-10 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60"
              >
                {createEndorsementMutation.isPending ? "Saving..." : "Create Endorsement"}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={isClaimDialogOpen} onOpenChange={setIsClaimDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Submit claim</DialogTitle>
            <DialogDescription>
              Claims can only be created for active policies.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleClaimSubmit} className="space-y-4">
            <div className="space-y-2">
              <label htmlFor="claimDescription" className="text-sm font-medium">Description</label>
              <textarea
                id="claimDescription"
                value={claimForm.description}
                onChange={(event) => setClaimForm((current) => ({ ...current, description: event.target.value }))}
                disabled={createClaimMutation.isPending}
                rows={4}
                className="w-full px-3 py-2 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60 resize-none"
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <label htmlFor="incidentDate" className="text-sm font-medium">Incident Date</label>
                <input
                  id="incidentDate"
                  type="date"
                  value={claimForm.incidentDate}
                  onChange={(event) => setClaimForm((current) => ({ ...current, incidentDate: event.target.value }))}
                  disabled={createClaimMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>
              <div className="space-y-2">
                <label htmlFor="estimatedDamage" className="text-sm font-medium">Estimated Damage</label>
                <input
                  id="estimatedDamage"
                  type="number"
                  min="0"
                  step="0.01"
                  value={claimForm.estimatedDamage}
                  onChange={(event) => setClaimForm((current) => ({ ...current, estimatedDamage: event.target.value }))}
                  disabled={createClaimMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>
            </div>

            {claimError && <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{claimError}</div>}

            <DialogFooter>
              <button
                type="button"
                onClick={() => setIsClaimDialogOpen(false)}
                disabled={createClaimMutation.isPending}
                className="h-10 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={createClaimMutation.isPending}
                className="h-10 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60"
              >
                {createClaimMutation.isPending ? "Submitting..." : "Submit Claim"}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={isPaymentDialogOpen} onOpenChange={setIsPaymentDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Record payment</DialogTitle>
            <DialogDescription>
              Record a payment for this policy. Completed payments cannot exceed the remaining premium.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handlePaymentSubmit} className="space-y-4">
            <div className="rounded-lg border border-border bg-muted/30 p-3 text-sm text-muted-foreground">
              Remaining premium: {formatMoney(remainingPremium, policy.currencyCode)}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <label htmlFor="paymentAmount" className="text-sm font-medium">Amount</label>
                <input
                  id="paymentAmount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={paymentForm.amount}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, amount: event.target.value }))}
                  disabled={createPaymentMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>

              <div className="space-y-2">
                <label htmlFor="paymentDate" className="text-sm font-medium">Payment Date</label>
                <input
                  id="paymentDate"
                  type="date"
                  value={paymentForm.paymentDate}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, paymentDate: event.target.value }))}
                  disabled={createPaymentMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <label htmlFor="paymentMethod" className="text-sm font-medium">Method</label>
                <select
                  id="paymentMethod"
                  value={paymentForm.method}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, method: event.target.value as PaymentFormState["method"] }))}
                  disabled={createPaymentMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                >
                  <option value="BankTransfer">Bank Transfer</option>
                  <option value="Card">Card</option>
                  <option value="Cash">Cash</option>
                </select>
              </div>

              <div className="space-y-2">
                <label htmlFor="paymentStatus" className="text-sm font-medium">Status</label>
                <select
                  id="paymentStatus"
                  value={paymentForm.status}
                  onChange={(event) => setPaymentForm((current) => ({ ...current, status: event.target.value as PaymentFormState["status"] }))}
                  disabled={createPaymentMutation.isPending}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
                >
                  <option value="Completed">Completed</option>
                  <option value="Pending">Pending</option>
                  <option value="Failed">Failed</option>
                </select>
              </div>
            </div>

            {paymentError && <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{paymentError}</div>}

            <DialogFooter>
              <button
                type="button"
                onClick={() => setIsPaymentDialogOpen(false)}
                disabled={createPaymentMutation.isPending}
                className="h-10 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={createPaymentMutation.isPending || remainingPremium <= 0 || !activeVersion?.currencyId}
                className="h-10 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60"
              >
                {createPaymentMutation.isPending ? "Recording..." : "Record Payment"}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}