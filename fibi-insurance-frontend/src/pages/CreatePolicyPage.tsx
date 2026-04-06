import { ChangeEvent, FormEvent, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate } from "react-router-dom";
import { AlertCircle } from "lucide-react";
import { PageHeader } from "@/components/ui/PageHeader";
import { toast } from "@/components/ui/sonner";
import { useAuth } from "@/contexts/RoleContext";
import { getClients } from "@/lib/clients/client.api";
import { getCurrencies } from "@/lib/metadatas/currency.api";
import { createPolicyDraft } from "@/lib/policies/policy.api";

type PolicyDraftFormState = {
  clientId: string;
  buildingId: string;
  currencyId: string;
  basePremium: string;
  startDate: string;
  endDate: string;
};

const initialForm: PolicyDraftFormState = {
  clientId: "",
  buildingId: "",
  currencyId: "",
  basePremium: "",
  startDate: "",
  endDate: "",
};

export default function CreatePolicyPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user, isLoading: isAuthLoading } = useAuth();
  const [form, setForm] = useState<PolicyDraftFormState>(initialForm);
  const [error, setError] = useState("");

  const { data: clients = [], isLoading: isClientsLoading, isError: isClientsError, error: clientsError, refetch: refetchClients } = useQuery({
    queryKey: ["clients"],
    queryFn: () => getClients(),
    staleTime: 30000,
  });

  const { data: currencies = [], isLoading: isCurrenciesLoading, isError: isCurrenciesError, error: currenciesError, refetch: refetchCurrencies } = useQuery({
    queryKey: ["currencies"],
    queryFn: () => getCurrencies(),
    staleTime: 30000,
  });

  const createPolicyMutation = useMutation({
    mutationFn: createPolicyDraft,
    onSuccess: async (policy) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["policies"] }),
        queryClient.invalidateQueries({ queryKey: ["clients"] }),
      ]);
      toast.success("Policy draft created");
      navigate(`/policies/${policy.id}`);
    },
  });

  const selectedClient = useMemo(
    () => clients.find((client) => client.id === form.clientId) ?? null,
    [clients, form.clientId],
  );

  const availableBuildings = selectedClient?.buildings ?? [];
  const isLoading = isAuthLoading || isClientsLoading || isCurrenciesLoading;
  const isSubmitting = createPolicyMutation.isPending;

  const handleInputChange = (field: keyof PolicyDraftFormState) => (event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const value = event.target.value;

    setForm((current) => {
      if (field === "clientId") {
        return {
          ...current,
          clientId: value,
          buildingId: "",
        };
      }

      return {
        ...current,
        [field]: value,
      };
    });
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");

    try {
      if (!user?.brokerId) {
        throw new Error("The logged in broker profile is missing a broker id.");
      }

      if (!form.clientId || !form.buildingId || !form.currencyId || !form.basePremium || !form.startDate || !form.endDate) {
        throw new Error("All policy draft fields are required.");
      }

      const basePremium = Number(form.basePremium);
      if (!Number.isFinite(basePremium) || basePremium <= 0) {
        throw new Error("Base premium must be greater than zero.");
      }

      if (form.endDate <= form.startDate) {
        throw new Error("End date must be after start date.");
      }

      await createPolicyMutation.mutateAsync({
        clientId: form.clientId,
        buildingId: form.buildingId,
        currencyId: form.currencyId,
        basePremium,
        startDate: form.startDate,
        endDate: form.endDate,
        brokerId: user.brokerId,
      });
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : "Failed to create policy draft.");
    }
  };

  if (isLoading) {
    return <div className="space-y-6"><p className="text-sm text-muted-foreground">Loading policy setup data...</p></div>;
  }

  if (!user?.brokerId || isClientsError || isCurrenciesError) {
    return (
      <div className="space-y-6">
        <PageHeader title="Create Policy" description="Create a draft policy from the backend policy flow">
          <Link to="/policies" className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors">
            Back to Policies
          </Link>
        </PageHeader>

        <div className="flex flex-col items-center justify-center gap-4 px-6 py-16 text-center glass-card">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
            <AlertCircle className="h-8 w-8" />
          </div>
          <div>
            <h3 className="text-lg font-semibold">Could not load policy form</h3>
            <p className="text-sm text-muted-foreground mt-1 max-w-md">
              {!user?.brokerId
                ? "The current account does not have a broker id, so policy creation cannot be submitted."
                : clientsError instanceof Error
                  ? clientsError.message
                  : currenciesError instanceof Error
                    ? currenciesError.message
                    : "The policy setup requests failed."}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => {
                void refetchClients();
                void refetchCurrencies();
              }}
              className="h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors"
            >
              Try again
            </button>
            <Link to="/policies" className="h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium inline-flex items-center justify-center hover:opacity-90 transition-opacity">
              Back to Policies
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Create Policy" description="Create a draft policy and let the backend calculate the final premium">
        <Link to="/policies" className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors">
          Back to Policies
        </Link>
      </PageHeader>

      <form onSubmit={handleSubmit} className="glass-card p-5 space-y-5">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <label htmlFor="clientId" className="text-sm font-medium">Client</label>
            <select
              id="clientId"
              value={form.clientId}
              onChange={handleInputChange("clientId")}
              disabled={isSubmitting}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">Select a client</option>
              {clients.map((client) => (
                <option key={client.id} value={client.id}>{client.name}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label htmlFor="buildingId" className="text-sm font-medium">Building</label>
            <select
              id="buildingId"
              value={form.buildingId}
              onChange={handleInputChange("buildingId")}
              disabled={isSubmitting || !selectedClient}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">{selectedClient ? "Select a building" : "Choose a client first"}</option>
              {availableBuildings.map((building) => (
                <option key={building.id} value={building.id}>{building.address} · {building.cityName}</option>
              ))}
            </select>
            {selectedClient && availableBuildings.length === 0 && (
              <p className="text-xs text-muted-foreground">This client has no registered buildings yet.</p>
            )}
          </div>

          <div className="space-y-2">
            <label htmlFor="currencyId" className="text-sm font-medium">Currency</label>
            <select
              id="currencyId"
              value={form.currencyId}
              onChange={handleInputChange("currencyId")}
              disabled={isSubmitting}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">Select a currency</option>
              {currencies.map((currency) => (
                <option key={currency.id} value={currency.id}>{currency.code} · {currency.name}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label htmlFor="basePremium" className="text-sm font-medium">Base Premium</label>
            <input
              id="basePremium"
              type="number"
              min="0"
              step="0.01"
              value={form.basePremium}
              onChange={handleInputChange("basePremium")}
              disabled={isSubmitting}
              required
              placeholder="e.g. 1200"
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            />
          </div>

          <div className="space-y-2">
            <label htmlFor="startDate" className="text-sm font-medium">Start Date</label>
            <input
              id="startDate"
              type="date"
              value={form.startDate}
              onChange={handleInputChange("startDate")}
              disabled={isSubmitting}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            />
          </div>

          <div className="space-y-2">
            <label htmlFor="endDate" className="text-sm font-medium">End Date</label>
            <input
              id="endDate"
              type="date"
              value={form.endDate}
              onChange={handleInputChange("endDate")}
              disabled={isSubmitting}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            />
          </div>
        </div>

        {selectedClient && form.buildingId && (
          <div className="rounded-lg border border-border bg-card p-4 space-y-2 text-sm text-muted-foreground">
            <p><span className="font-medium text-foreground">Client:</span> {selectedClient.name}</p>
            <p>
              <span className="font-medium text-foreground">Building:</span>{" "}
              {availableBuildings.find((building) => building.id === form.buildingId)?.address ?? "-"}
            </p>
          </div>
        )}

        {error && (
          <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {error}
          </div>
        )}

        <div className="flex items-center justify-end gap-2">
          <button
            type="button"
            onClick={() => navigate("/policies")}
            disabled={isSubmitting}
            className="h-10 px-5 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSubmitting || !user?.brokerId}
            className="h-10 px-5 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {isSubmitting ? "Creating..." : "Create Draft Policy"}
          </button>
        </div>
      </form>
    </div>
  );
}