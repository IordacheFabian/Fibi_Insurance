import { useDeferredValue, useMemo, useState, type FormEvent } from "react";
import { motion } from "framer-motion";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertCircle, Mail, Phone, Plus, Search, ShieldCheck, ShieldOff, UserCog } from "lucide-react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { PageHeader } from "@/components/ui/PageHeader";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "@/components/ui/sonner";
import { useRole } from "@/contexts/RoleContext";
import {
  activateBroker,
  createBroker,
  deactivateBroker,
  getBrokers,
  updateBroker,
} from "@/lib/brokers/broker.api";
import type { BrokerListItem, CreateBrokerInput, UpdateBrokerInput } from "@/lib/brokers/broker.types";
import { cn } from "@/lib/utils";

const statusTabs = ["all", "active", "inactive"] as const;

type StatusTab = typeof statusTabs[number];
type BrokerDialogMode = "create" | "edit";

type BrokerFormState = {
  brokerCode: string;
  name: string;
  email: string;
  phoneNumber: string;
  commissionPercentage: string;
  password: string;
};

const initialBrokerForm: BrokerFormState = {
  brokerCode: "",
  name: "",
  email: "",
  phoneNumber: "",
  commissionPercentage: "",
  password: "",
};

function getNextBrokerCode(brokers: BrokerListItem[]): string {
  const numericCodes = brokers
    .map((broker) => broker.brokerCode.trim().toUpperCase().match(/^BRK-(\d+)$/))
    .filter((match): match is RegExpMatchArray => Boolean(match))
    .map((match) => Number(match[1]))
    .filter((value) => Number.isFinite(value));

  const nextValue = numericCodes.length === 0 ? 1 : Math.max(...numericCodes) + 1;
  return `BRK-${String(nextValue).padStart(3, "0")}`;
}

function isBrokerCodeTaken(brokers: BrokerListItem[], brokerCode: string): boolean {
  const normalizedBrokerCode = brokerCode.trim().toUpperCase();
  return brokers.some((broker) => broker.brokerCode.trim().toUpperCase() === normalizedBrokerCode);
}

function getBrokerInitials(name: string): string {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}

function formatCommission(value?: number | null): string {
  if (value == null || Number.isNaN(value)) {
    return "Not set";
  }

  return `${value.toFixed(2)}%`;
}

function normalisePhoneNumber(value?: string | null): string {
  return value?.trim() || "No phone number";
}

function BrokerCardSkeleton() {
  return (
    <div className="glass-card p-6 space-y-4">
      <div className="flex items-start gap-4">
        <Skeleton className="h-12 w-12 rounded-2xl" />
        <div className="flex-1 space-y-2">
          <Skeleton className="h-4 w-40" />
          <Skeleton className="h-3 w-28" />
        </div>
      </div>
      <Skeleton className="h-16 w-full rounded-xl" />
      <div className="grid grid-cols-2 gap-3">
        <Skeleton className="h-10 w-full rounded-xl" />
        <Skeleton className="h-10 w-full rounded-xl" />
      </div>
    </div>
  );
}

export default function BrokersPage() {
  const { role } = useRole();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const deferredSearch = useDeferredValue(search);
  const [tab, setTab] = useState<StatusTab>("all");
  const [dialogMode, setDialogMode] = useState<BrokerDialogMode | null>(null);
  const [selectedBroker, setSelectedBroker] = useState<BrokerListItem | null>(null);
  const [formState, setFormState] = useState<BrokerFormState>(initialBrokerForm);
  const [formError, setFormError] = useState("");

  const {
    data: brokers = [],
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ["admin", "brokers"],
    queryFn: () => getBrokers(),
    enabled: role === "admin",
    staleTime: 30000,
  });

  const stats = useMemo(() => {
    const activeBrokers = brokers.filter((broker) => broker.status === "active");
    const inactiveBrokers = brokers.length - activeBrokers.length;
    const withCommission = brokers.filter((broker) => broker.commissionPercentage != null);
    const averageCommission = withCommission.length === 0
      ? "0.00%"
      : `${(withCommission.reduce((sum, broker) => sum + (broker.commissionPercentage ?? 0), 0) / withCommission.length).toFixed(2)}%`;

    return [
      { label: "Total Brokers", value: String(brokers.length), tone: "text-foreground" },
      { label: "Active", value: String(activeBrokers.length), tone: "text-success" },
      { label: "Inactive", value: String(inactiveBrokers), tone: "text-destructive" },
      { label: "Average Commission", value: averageCommission, tone: "text-primary" },
    ];
  }, [brokers]);

  const filteredBrokers = useMemo(() => {
    const normalizedSearch = deferredSearch.trim().toLowerCase();

    return brokers.filter((broker) => {
      const matchesSearch = normalizedSearch.length === 0
        || broker.name.toLowerCase().includes(normalizedSearch)
        || broker.email.toLowerCase().includes(normalizedSearch)
        || broker.brokerCode.toLowerCase().includes(normalizedSearch)
        || (broker.phoneNumber ?? "").toLowerCase().includes(normalizedSearch);

      const matchesTab = tab === "all" || broker.status === tab;

      return matchesSearch && matchesTab;
    });
  }, [brokers, deferredSearch, tab]);

  const suggestedBrokerCode = useMemo(() => getNextBrokerCode(brokers), [brokers]);

  const saveBrokerMutation = useMutation({
    mutationFn: async () => {
      const commissionPercentage = formState.commissionPercentage.trim()
        ? Number(formState.commissionPercentage)
        : null;

      if (!formState.name.trim()) {
        throw new Error("Broker name is required.");
      }

      if (!formState.email.trim()) {
        throw new Error("Broker email is required.");
      }

      if (commissionPercentage != null && (!Number.isFinite(commissionPercentage) || commissionPercentage < 0)) {
        throw new Error("Commission must be zero or greater.");
      }

      if (commissionPercentage != null && commissionPercentage > 100) {
        throw new Error("Commission must be 100 or less.");
      }

      if (dialogMode === "create") {
        if (!formState.brokerCode.trim()) {
          throw new Error("Broker code is required.");
        }

        if (isBrokerCodeTaken(brokers, formState.brokerCode)) {
          throw new Error(`Broker code ${formState.brokerCode.trim().toUpperCase()} is already taken. Try ${suggestedBrokerCode}.`);
        }

        if (formState.password.trim().length < 8) {
          throw new Error("Password must be at least 8 characters long.");
        }

        const payload: CreateBrokerInput = {
          brokerCode: formState.brokerCode,
          name: formState.name,
          email: formState.email,
          phoneNumber: formState.phoneNumber,
          commissionPercentage,
          password: formState.password,
        };

        await createBroker(payload);
        return;
      }

      if (!selectedBroker) {
        throw new Error("No broker selected.");
      }

      const payload: UpdateBrokerInput = {
        name: formState.name,
        email: formState.email,
        phoneNumber: formState.phoneNumber,
        commissionPercentage,
      };

      await updateBroker(selectedBroker.id, payload);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["admin", "brokers"] });
      setDialogMode(null);
      setSelectedBroker(null);
      setFormState(initialBrokerForm);
      setFormError("");
      toast.success(dialogMode === "create" ? "Broker created" : "Broker updated");
    },
    onError: (mutationError) => {
      const message = mutationError instanceof Error ? mutationError.message : "Failed to save broker";

      if (dialogMode === "create" && /broker code already exists/i.test(message)) {
        setFormState((current) => ({
          ...current,
          brokerCode: suggestedBrokerCode,
        }));
        setFormError(`Broker code is already taken. I suggested ${suggestedBrokerCode} instead.`);
        return;
      }

      setFormError(message);
    },
  });

  const statusMutation = useMutation({
    mutationFn: async ({ brokerId, nextStatus }: { brokerId: string; nextStatus: "active" | "inactive" }) => {
      if (nextStatus === "active") {
        await activateBroker(brokerId);
        return;
      }

      await deactivateBroker(brokerId);
    },
    onSuccess: async (_data, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["admin", "brokers"] });
      toast.success(variables.nextStatus === "active" ? "Broker activated" : "Broker deactivated");
    },
    onError: (mutationError) => {
      toast.error(mutationError instanceof Error ? mutationError.message : "Failed to update broker status");
    },
  });

  const openCreateDialog = () => {
    setDialogMode("create");
    setSelectedBroker(null);
    setFormState({
      ...initialBrokerForm,
      brokerCode: suggestedBrokerCode,
    });
    setFormError("");
  };

  const openEditDialog = (broker: BrokerListItem) => {
    setDialogMode("edit");
    setSelectedBroker(broker);
    setFormState({
      brokerCode: broker.brokerCode,
      name: broker.name,
      email: broker.email,
      phoneNumber: broker.phoneNumber ?? "",
      commissionPercentage: broker.commissionPercentage != null ? String(broker.commissionPercentage) : "",
      password: "",
    });
    setFormError("");
  };

  const closeDialog = () => {
    if (saveBrokerMutation.isPending) {
      return;
    }

    setDialogMode(null);
    setSelectedBroker(null);
    setFormState(initialBrokerForm);
    setFormError("");
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setFormError("");
    await saveBrokerMutation.mutateAsync();
  };

  if (role !== "admin") {
    return (
      <div className="space-y-6">
        <PageHeader title="Brokers" description="Broker administration is available only for admin accounts" />
        <div className="glass-card p-6">
          <EmptyState
            icon={UserCog}
            title="Admin access required"
            description="Switch to an admin account to view, create, and manage broker profiles."
          />
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Brokers" description="Create brokers, maintain contact details, and control account availability.">
        <button
          type="button"
          onClick={openCreateDialog}
          className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity"
        >
          <Plus className="h-4 w-4" /> Add Broker
        </button>
      </PageHeader>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {stats.map((item, index) => (
          <motion.div
            key={item.label}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.05 }}
            className="glass-card p-4"
          >
            <p className="text-xs text-muted-foreground">{item.label}</p>
            <p className={cn("text-xl font-bold mt-1", item.tone)}>{item.value}</p>
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
              placeholder="Search by code, name, email, or phone"
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5 overflow-x-auto">
            {statusTabs.map((status) => (
              <button
                key={status}
                type="button"
                onClick={() => setTab(status)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize whitespace-nowrap",
                  tab === status ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
                )}
              >
                {status}
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
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {Array.from({ length: 6 }, (_, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.04 }}
            >
              <BrokerCardSkeleton />
            </motion.div>
          ))}
        </div>
      ) : isError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
          <div className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4" />
            <span>{error instanceof Error ? error.message : "Failed to load brokers."}</span>
          </div>
        </div>
      ) : filteredBrokers.length === 0 ? (
        <div className="glass-card p-6">
          <EmptyState
            icon={UserCog}
            title={brokers.length === 0 ? "No brokers yet" : "No brokers match the current filters"}
            description={brokers.length === 0
              ? "Create the first broker profile to start assigning accounts to your sales team."
              : "Try a different search term or switch the status filter."}
          />
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {filteredBrokers.map((broker, index) => (
            <motion.div
              key={broker.id}
              initial={{ opacity: 0, y: 14 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.05 }}
              className="glass-card-hover p-6"
            >
              <div className="flex items-start gap-4">
                <div className={cn(
                  "h-12 w-12 rounded-2xl flex items-center justify-center text-sm font-bold",
                  broker.status === "active"
                    ? "gradient-primary text-primary-foreground"
                    : "bg-muted text-muted-foreground",
                )}>
                  {getBrokerInitials(broker.name) || "BR"}
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <h3 className="font-semibold truncate">{broker.name}</h3>
                      <p className="text-xs text-muted-foreground mt-1">{broker.brokerCode}</p>
                    </div>
                    <span className={cn(
                      "px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-[0.18em] border",
                      broker.status === "active"
                        ? "border-success/20 bg-success/10 text-success"
                        : "border-destructive/20 bg-destructive/10 text-destructive",
                    )}>
                      {broker.status}
                    </span>
                  </div>

                  <div className="mt-4 space-y-2 text-sm">
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Mail className="h-4 w-4" />
                      <span className="truncate">{broker.email}</span>
                    </div>
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Phone className="h-4 w-4" />
                      <span className="truncate">{normalisePhoneNumber(broker.phoneNumber)}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="mt-5 rounded-2xl border border-border/60 bg-background/40 p-4">
                <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Commission</p>
                <p className="mt-2 text-2xl font-bold text-primary">{formatCommission(broker.commissionPercentage)}</p>
                <p className="mt-1 text-xs text-muted-foreground">
                  {broker.status === "active"
                    ? "Broker can create and service policies."
                    : "Broker is currently unavailable for new work."}
                </p>
              </div>

              <div className="mt-5 grid grid-cols-2 gap-3">
                <button
                  type="button"
                  onClick={() => openEditDialog(broker)}
                  className="h-10 rounded-xl border border-border text-sm font-medium hover:bg-muted transition-colors"
                >
                  Edit Details
                </button>
                <button
                  type="button"
                  disabled={statusMutation.isPending}
                  onClick={() => void statusMutation.mutateAsync({
                    brokerId: broker.id,
                    nextStatus: broker.status === "active" ? "inactive" : "active",
                  })}
                  className={cn(
                    "h-10 rounded-xl text-sm font-medium transition-colors disabled:opacity-60",
                    broker.status === "active"
                      ? "bg-destructive/10 text-destructive hover:bg-destructive/15"
                      : "bg-success/10 text-success hover:bg-success/15",
                  )}
                >
                  <span className="inline-flex items-center gap-2">
                    {broker.status === "active" ? <ShieldOff className="h-4 w-4" /> : <ShieldCheck className="h-4 w-4" />}
                    {broker.status === "active" ? "Deactivate" : "Activate"}
                  </span>
                </button>
              </div>
            </motion.div>
          ))}
        </div>
      )}

      <Dialog open={dialogMode != null} onOpenChange={(open) => { if (!open) closeDialog(); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{dialogMode === "create" ? "Add Broker" : "Edit Broker"}</DialogTitle>
            <DialogDescription>
              {dialogMode === "create"
                ? "Create a broker profile that can later be linked to broker authentication."
                : "Update broker contact details and commission settings."}
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={(event) => void handleSubmit(event)} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <label className="space-y-2 text-sm">
                <span className="font-medium">Broker Code</span>
                <input
                  type="text"
                  value={formState.brokerCode}
                  onChange={(event) => setFormState((current) => ({ ...current, brokerCode: event.target.value }))}
                  disabled={dialogMode === "edit"}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  placeholder="BRK-002"
                />
                {dialogMode === "create" ? (
                  <p className="text-xs text-muted-foreground">Suggested next code: {suggestedBrokerCode}</p>
                ) : null}
              </label>

              <label className="space-y-2 text-sm">
                <span className="font-medium">Commission %</span>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={formState.commissionPercentage}
                  onChange={(event) => setFormState((current) => ({ ...current, commissionPercentage: event.target.value }))}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
                  placeholder="10"
                />
              </label>
            </div>

            <label className="space-y-2 text-sm block">
              <span className="font-medium">Broker Name</span>
              <input
                type="text"
                value={formState.name}
                onChange={(event) => setFormState((current) => ({ ...current, name: event.target.value }))}
                className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
                placeholder="North Region Brokers"
              />
            </label>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <label className="space-y-2 text-sm">
                <span className="font-medium">Email</span>
                <input
                  type="email"
                  value={formState.email}
                  onChange={(event) => setFormState((current) => ({ ...current, email: event.target.value }))}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
                  placeholder="broker@insurance.local"
                />
              </label>

              <label className="space-y-2 text-sm">
                <span className="font-medium">Phone Number</span>
                <input
                  type="text"
                  value={formState.phoneNumber}
                  onChange={(event) => setFormState((current) => ({ ...current, phoneNumber: event.target.value }))}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
                  placeholder="0700000000"
                />
              </label>
            </div>

            {dialogMode === "create" ? (
              <label className="space-y-2 text-sm block">
                <span className="font-medium">Initial Password</span>
                <input
                  type="password"
                  value={formState.password}
                  onChange={(event) => setFormState((current) => ({ ...current, password: event.target.value }))}
                  className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
                  placeholder="Minimum 8 characters"
                />
                <p className="text-xs text-muted-foreground">This creates the broker login account at the same time as the broker profile.</p>
              </label>
            ) : null}

            {formError ? (
              <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-3 text-sm text-destructive">
                {formError}
              </div>
            ) : null}

            <DialogFooter>
              <button
                type="button"
                onClick={closeDialog}
                className="h-10 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={saveBrokerMutation.isPending}
                className="h-10 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60"
              >
                {saveBrokerMutation.isPending ? "Saving..." : dialogMode === "create" ? "Create Broker + Account" : "Save Changes"}
              </button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
