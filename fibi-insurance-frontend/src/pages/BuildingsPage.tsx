import { useDeferredValue, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { motion } from "framer-motion";
import { AlertCircle, Building2, MapPin, Plus, Search, UserRound } from "lucide-react";
import { Link } from "react-router-dom";
import { PageHeader } from "@/components/ui/PageHeader";
import { EmptyState } from "@/components/ui/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";
import { getBuildings } from "@/lib/buildings/building.api";
import { BuildingTypeValue } from "@/lib/buildings/building.type";
import { cn, formatMoney } from "@/lib/utils";

const tabs = ["all", "residential", "commercial", "industrial", "mixed-use"] as const;

function getBuildingTypeLabel(buildingType: BuildingTypeValue) {
  if (typeof buildingType === "number") {
    if (buildingType === 1) {
      return "commercial";
    }

    if (buildingType === 2) {
      return "industrial";
    }

    if (buildingType === 3) {
      return "mixed-use";
    }

    return "residential";
  }

  const normalizedType = buildingType.toLowerCase();

  if (normalizedType === "commercial" || normalizedType === "1") {
    return "commercial";
  }

  if (normalizedType === "industrial" || normalizedType === "2") {
    return "industrial";
  }

  if (normalizedType === "mixeduse" || normalizedType === "mixed-use" || normalizedType === "3") {
    return "mixed-use";
  }

  return "residential";
}

export default function BuildingsPage() {
  const [search, setSearch] = useState("");
  const [tab, setTab] = useState<typeof tabs[number]>("all");
  const deferredSearch = useDeferredValue(search);

  const { data: buildings = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ["buildings"],
    queryFn: () => getBuildings(),
    staleTime: 30000,
  });

  const filtered = useMemo(() => {
    const normalizedSearch = deferredSearch.trim().toLowerCase();

    return buildings.filter((building) => {
      const buildingType = getBuildingTypeLabel(building.buildingType);
      const matchesSearch = 
        normalizedSearch.length === 0 ||
          building.cityName.toLowerCase().includes(normalizedSearch) ||
          building.address.toLowerCase().includes(normalizedSearch) ||
          buildingType.includes(normalizedSearch);
      
      const matchesTab = tab === "all" || buildingType === tab;

      return matchesSearch && matchesTab;
    });
  }, [buildings, deferredSearch, tab]);

  return (
    <div className="space-y-6">
      <PageHeader title="Buildings" description="Manage insured properties">
        <Link to="/buildings/new" className="flex items-center gap-2 h-9 px-4 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity">
          <Plus className="h-4 w-4" /> Register Building
        </Link>
      </PageHeader>

      <div className="glass-card p-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search buildings..."
              className="w-full h-9 pl-9 pr-4 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all"
            />
          </div>
          <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-0.5 flex-wrap">
            {tabs.map((t) => (
              <button
                key={t}
                onClick={() => setTab(t)}
                className={cn(
                  "px-3 py-1.5 rounded-md text-xs font-medium transition-all capitalize",
                  tab === t ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
                )}
              >
                {t}
              </button>
            ))}
          </div>
        </div>
        <p className="mt-3 text-xs text-muted-foreground">Showing {filtered.length} of {buildings.length} buildings</p>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {Array.from({ length: 6 }).map((_, index) => (
            <div key={index} className="glass-card p-5 space-y-4">
              <div className="flex items-start gap-3">
                <Skeleton className="h-10 w-10 rounded-xl" />
                <div className="space-y-2 flex-1">
                  <Skeleton className="h-4 w-36" />
                  <Skeleton className="h-3 w-48" />
                </div>
              </div>
              <Skeleton className="h-3 w-full" />
              <Skeleton className="h-3 w-2/3" />
              <div className="flex items-center justify-between pt-3 border-t border-border/50">
                <div className="space-y-2">
                  <Skeleton className="h-3 w-20" />
                  <Skeleton className="h-4 w-24" />
                </div>
                <Skeleton className="h-6 w-24 rounded-full" />
              </div>
            </div>
          ))}
        </div>
      ) : isError ? (
        <div className="flex flex-col items-center justify-center gap-4 px-6 py-16 text-center glass-card">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
            <AlertCircle className="h-8 w-8" />
          </div>
          <div>
            <h3 className="text-lg font-semibold">Could not load buildings</h3>
            <p className="text-sm text-muted-foreground mt-1 max-w-md">
              {error instanceof Error ? error.message : "The building list request failed."}
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
          icon={Building2}
          title="No buildings found"
          description={search.trim() ? "Try a different search term or switch to another building type." : "No buildings are available for your clients yet."}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {filtered.map((b, i) => (
            <motion.div
              key={b.id}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.05 }}
              className="glass-card-hover"
            >
              <Link to={`/buildings/${b.id}`} className="block p-5">
                <div className="flex items-start gap-3 mb-3">
                  <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center">
                    <Building2 className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <h3 className="text-sm font-semibold">{b.address}</h3>
                    <p className="text-xs text-muted-foreground">{b.cityName}</p>
                  </div>
                </div>

                <div className="space-y-2 mb-4">
                  <div className="flex items-center gap-2 text-xs text-muted-foreground">
                    <MapPin className="h-3 w-3" /> {b.cityName}
                  </div>
                  <div className="flex items-center gap-2 text-xs text-muted-foreground">
                    <UserRound className="h-3 w-3" /> {b.ownerName}
                  </div>
                  <div className="flex items-center justify-between text-xs">
                    <span className="text-muted-foreground capitalize">{getBuildingTypeLabel(b.buildingType)}</span>
                    <span className="text-muted-foreground">Built {b.constructionYear}</span>
                  </div>
                </div>

                <div className="pt-3 border-t border-border/50 flex items-end justify-between gap-3">
                  <div>
                    <p className="text-xs text-muted-foreground">Insured Value</p>
                    <p className="text-sm font-bold">{formatMoney(b.insuredValue, b.currencyCode)}</p>
                  </div>
                  <span className="text-xs text-muted-foreground">Owner buildings: {buildings.filter((building) => building.ownerId === b.ownerId).length}</span>
                </div>
              </Link>
            </motion.div>
          ))}
        </div>
      )}
    </div>
  );
}
