import { getClientById } from "@/lib/clients/client.api";
import { ClientDetails, ClientTypeValue } from "@/lib/clients/client.types";
import { getPolicies, normalizePolicyStatus } from "@/lib/policies/policy.api";
import { useQuery } from "@tanstack/react-query";
import { AlertCircle, Building2, Calendar, FileText, Mail, Pencil, Phone } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { PageHeader } from "@/components/ui/PageHeader";
import { EmptyState } from "@/components/ui/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";
import { StatusChip } from "@/components/ui/StatusChip";
import { useRole } from "@/contexts/RoleContext";
import { formatMoney } from "@/lib/utils";

function getClientTypeLabel(clientType: ClientTypeValue): "individual" | "company" {
  if (typeof clientType === "number") {
    return clientType === 1 ? "company" : "individual";
  }

  const normalizedType = clientType.toLowerCase();

  if (normalizedType === "company" || normalizedType === "1") {
    return "company";
  }

  return "individual";
}

function getPrimaryCity(client: Pick<ClientDetails, "buildings">) {
  return client.buildings[0]?.cityName || "-";
}

function getClientInitials(name: string) {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function formatDateOnly(value: string) {
  const [year, month, day] = value.split("-").map(Number);

  if (!year || !month || !day) {
    return value;
  }

  return new Date(year, month - 1, day).toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

export default function ClientDetailsPage() {
  const { role } = useRole();

    const { id } = useParams<{ id: string }>();
    
    const {
        data: client,
        isLoading,
        isError,
        error,
        refetch,
    } = useQuery({
        queryKey: ["clients", id],
        queryFn: () => getClientById(id!),
        staleTime: 30000,
      enabled: Boolean(id),
    });

    const {
      data: policies = [],
      isLoading: arePoliciesLoading,
      isError: policiesError,
      error: policiesQueryError,
      refetch: refetchPolicies,
    } = useQuery({
      queryKey: ["clients", id, "policies", role],
      queryFn: () => getPolicies({ clientId: id! }, role),
      staleTime: 30000,
      enabled: Boolean(id),
    });

    if (isLoading) {
      return <div className="space-y-6"><p className="text-sm text-muted-foreground">Loading client details...</p></div>;
    }

    if (isError || !client) {
      return (
        <div className="space-y-6">
          <div className="flex items-center gap-3 rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-destructive">
            <AlertCircle className="h-5 w-5" />
            <div>
              <p className="font-medium">Could not load client details</p>
              <p className="text-sm">{error instanceof Error ? error.message : "The client details request failed."}</p>
            </div>
          </div>
        </div>
      );
    }

    const activePoliciesCount = policies.filter((policy) => normalizePolicyStatus(policy.policyStatus) === "active").length;

    return (
      <div className="space-y-6">
        <PageHeader title="Client Details" description="View and manage this client">
          <Link to={`/clients/${client.id}/edit`} className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
            <Pencil className="h-4 w-4" /> Edit Client
          </Link>
        </PageHeader>

        <div className="flex items-center gap-4">
          <div className="h-14 w-14 rounded-full bg-primary/10 flex items-center justify-center text-sm font-semibold text-primary">
            {getClientInitials(client.name)}
          </div>
          <div>
            <h2 className="text-2xl font-bold">{client.name}</h2>
            <p className="text-sm font-semibold capitalize text-muted-foreground">{getClientTypeLabel(client.clientType)}</p>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <div className="rounded-lg border border-border bg-card p-4 space-y-3">
            <h3 className="font-semibold">Contact</h3>
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Mail className="h-4 w-4" /> {client.email}
            </div>
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Phone className="h-4 w-4" /> {client.phoneNumber}
            </div>
            <div className="text-sm text-muted-foreground">Identification: {client.identificationNumber}</div>
            <div className="text-sm text-muted-foreground">Primary City: {getPrimaryCity(client)}</div>
          </div>

          <div className="rounded-lg border border-border bg-card p-4 space-y-3">
            <h3 className="font-semibold">Buildings</h3>
            {client.buildings.length === 0 ? (
              <p className="text-sm text-muted-foreground">No buildings registered for this client.</p>
            ) : (
              <div className="space-y-3">
                {client.buildings.map((building) => (
                  <div key={building.id} className="flex items-start gap-2 text-sm text-muted-foreground">
                    <Building2 className="h-4 w-4 mt-0.5" />
                    <div>
                      <div>{building.address}</div>
                      <div>{building.cityName}</div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="rounded-lg border border-border bg-card p-4 space-y-4">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <h3 className="font-semibold">Policies</h3>
              <p className="text-sm text-muted-foreground">All policies issued for this client.</p>
            </div>
            <div className="flex items-center gap-3 text-xs text-muted-foreground">
              <span>Total: {policies.length}</span>
              <span>Active: {activePoliciesCount}</span>
            </div>
          </div>

          {arePoliciesLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 3 }).map((_, index) => (
                <div key={index} className="rounded-lg border border-border/60 p-4 space-y-2">
                  <Skeleton className="h-4 w-36" />
                  <Skeleton className="h-4 w-56" />
                  <Skeleton className="h-4 w-40" />
                </div>
              ))}
            </div>
          ) : policiesError ? (
            <div className="flex items-center justify-between gap-4 rounded-lg border border-destructive/20 bg-destructive/5 p-4 text-destructive">
              <div>
                <p className="font-medium">Could not load client policies</p>
                <p className="text-sm">{policiesQueryError instanceof Error ? policiesQueryError.message : "The client policies request failed."}</p>
              </div>
              <button
                type="button"
                onClick={() => refetchPolicies()}
                className="h-9 rounded-lg border border-destructive/30 px-4 text-sm font-medium hover:bg-destructive/10 transition-colors"
              >
                Try again
              </button>
            </div>
          ) : policies.length === 0 ? (
            <EmptyState
              icon={FileText}
              title="No policies yet"
              description="This client does not have any policies assigned yet."
            />
          ) : (
            <div className="space-y-3">
              {policies.map((policy) => (
                <Link
                  key={policy.id}
                  to={`/policies/${policy.id}`}
                  className="block rounded-lg border border-border/60 p-4 transition-colors hover:bg-muted/30"
                >
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                    <div className="space-y-2">
                      <div className="flex items-center gap-2">
                        <FileText className="h-4 w-4 text-primary" />
                        <span className="font-mono text-sm font-semibold">{policy.policyNumber}</span>
                        <StatusChip status={normalizePolicyStatus(policy.policyStatus)} />
                      </div>
                      <p className="text-sm text-muted-foreground">
                        {`${policy.buildingStreet} ${policy.buildingNumber}`.trim()}, {policy.cityName}
                      </p>
                      <div className="flex items-center gap-2 text-xs text-muted-foreground">
                        <Calendar className="h-3.5 w-3.5" />
                        <span>{formatDateOnly(policy.startDate)} - {formatDateOnly(policy.endDate)}</span>
                      </div>
                    </div>

                    <div className="text-left lg:text-right">
                      <p className="text-xs text-muted-foreground">Final premium</p>
                      <p className="text-sm font-semibold">{formatMoney(policy.finalPremium, policy.currencyCode)}</p>
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    )
}