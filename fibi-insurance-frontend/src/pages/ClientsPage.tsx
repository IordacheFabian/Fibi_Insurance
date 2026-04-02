import { useDeferredValue, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { motion } from "framer-motion";
import { AlertCircle, Building2, Plus, Search, Users } from "lucide-react";
import { PageHeader } from "@/components/ui/PageHeader";
import { EmptyState } from "@/components/ui/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";
import { getClients } from "@/lib/api";
import type { ClientListItem, ClientTypeValue } from "@/lib/types";
import { cn } from "@/lib/utils";
import { Link } from "react-router-dom";

const tabs = ["all", "individual", "company"] as const;

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

function getClientInitials(name: string) {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function getPrimaryCity(client: ClientListItem) {
  return client.buildings[0]?.cityName || "-";
}

export default function ClientsPage() {
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof tabs[number]>("all");
  const deferredSearch = useDeferredValue(search);

  const { data: clients = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ["clients"],
    queryFn: () => getClients(),
    staleTime: 30000,
  });

  const filtered = useMemo(() => {
    const normalizedSearch = deferredSearch.trim().toLowerCase();

    return clients.filter((client) => {
      const clientType = getClientTypeLabel(client.clientType);
      const matchesSearch =
        normalizedSearch.length === 0 ||
        client.name.toLowerCase().includes(normalizedSearch) ||
        client.email.toLowerCase().includes(normalizedSearch) ||
        client.phoneNumber.toLowerCase().includes(normalizedSearch) ||
        client.identificationMasked.toLowerCase().includes(normalizedSearch);
      const matchesTab = tab === "all" || clientType === tab;

      return matchesSearch && matchesTab;
    });
  }, [clients, deferredSearch, tab]);

  return (
    <div className="space-y-6">
      <PageHeader title="Clients" description="Manage your client portfolio">
        <Link to="/clients/new" className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
          <Plus className="h-4 w-4" /> Add Client
        </Link>
      </PageHeader>

      {/* Filters */}
      <div className="glass-card p-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search clients..."
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5">
            {tabs.map((t) => (
              <button
                key={t}
                onClick={() => setTab(t)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize",
                  tab === t ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground"
                )}
              >
                {t}
              </button>
            ))}
          </div>
        </div>
        <p className="mt-3 text-xs text-muted-foreground">
          Showing {filtered.length} of {clients.length} clients
        </p>
      </div>

      {/* Table */}
      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass-card overflow-hidden"
      >
        {isLoading ? (
          <div className="p-4 space-y-3">
            {Array.from({ length: 6 }).map((_, index) => (
              <div key={index} className="grid grid-cols-6 gap-4 items-center rounded-lg border border-border/50 p-4">
                <div className="flex items-center gap-3 col-span-2">
                  <Skeleton className="h-9 w-9 rounded-full" />
                  <div className="space-y-2 w-full">
                    <Skeleton className="h-4 w-40" />
                    <Skeleton className="h-3 w-56" />
                  </div>
                </div>
                <Skeleton className="h-4 w-16" />
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-4 w-20" />
              </div>
            ))}
          </div>
        ) : isError ? (
          <div className="flex flex-col items-center justify-center gap-4 px-6 py-16 text-center">
            <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
              <AlertCircle className="h-8 w-8" />
            </div>
            <div>
              <h3 className="text-lg font-semibold">Could not load clients</h3>
              <p className="text-sm text-muted-foreground mt-1 max-w-md">
                {error instanceof Error ? error.message : "The client list request failed."}
              </p>
            </div>
            <button
              onClick={() => refetch()}
              className="flex items-center gap-2 h-9 px-4 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors"
            >
              Try again
            </button>
          </div>
        ) : filtered.length === 0 ? (
          <EmptyState
            icon={Users}
            title="No clients found"
            description={search.trim() ? "Try a different search term or switch to another client type." : "No clients are available in the database yet."}
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="table-premium">
              <thead>
                <tr>
                  <th>Client</th>
                  <th>Type</th>
                  <th>Identifier</th>
                  <th>Phone</th>
                  <th>Buildings</th>
                  <th>Primary City</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((client, i) => (
                  <motion.tr
                    key={client.id}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: i * 0.03 }}
                    className="cursor-pointer"
                  >
                    <td>
                      <div className="flex items-center gap-3">
                        <div className="h-9 w-9 rounded-full bg-primary/10 flex items-center justify-center text-xs font-semibold text-primary">
                          {getClientInitials(client.name)}
                        </div>
                        <div>
                          <p className="font-medium text-sm">{client.name}</p>
                          <p className="text-xs text-muted-foreground">{client.email}</p>
                        </div>
                      </div>
                    </td>
                    <td>
                      <span className="capitalize text-xs">{getClientTypeLabel(client.clientType)}</span>
                    </td>
                    <td className="font-mono text-xs text-muted-foreground">{client.identificationMasked}</td>
                    <td className="text-xs text-muted-foreground">{client.phoneNumber}</td>
                    <td>
                      <span className="flex items-center gap-1 text-xs">
                        <Building2 className="h-3 w-3 text-muted-foreground" /> {client.buildings.length}
                      </span>
                    </td>
                    <td className="text-xs text-muted-foreground">{getPrimaryCity(client)}</td>
                    <td>
                      <Link to={`/clients/${client.id}`} className="text-primary hover:underline">
                        View Details
                      </Link>
                    </td>
                  </motion.tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </motion.div>
    </div>
  );
}
