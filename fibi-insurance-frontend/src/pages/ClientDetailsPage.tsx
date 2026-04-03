import { getClientById } from "@/lib/clients/client.api";
import { ClientDetails, ClientTypeValue } from "@/lib/clients/client.types";
import { useQuery } from "@tanstack/react-query";
import { AlertCircle, Building2, Mail, Pencil, Phone } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { PageHeader } from "@/components/ui/PageHeader";

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

export default function ClientDetailsPage() {

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
      </div>
    )
}