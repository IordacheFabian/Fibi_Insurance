import { FormEvent, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { motion } from "framer-motion";
import { AlertCircle, Coins, Percent, ShieldAlert, Plus, PencilLine } from "lucide-react";
import { toast } from "sonner";
import { EmptyState } from "@/components/ui/EmptyState";
import { PageHeader } from "@/components/ui/PageHeader";
import { Skeleton } from "@/components/ui/skeleton";
import { useRole } from "@/contexts/RoleContext";
import { getGeographyTree } from "@/lib/geographies/geography.api";
import type { GeographyTreeCountry } from "@/lib/geographies/geography.types";
import { createCurrency, getCurrencies, updateCurrency } from "@/lib/metadatas/currency.api";
import type { CreateCurrencyRequest, CurrencyDto, UpdateCurrencyRequest } from "@/lib/metadatas/currency.types";
import { createFeeConfiguration, getFeeConfigurations, updateFeeConfiguration } from "@/lib/metadatas/fee.api";
import type {
  CreateFeeConfigurationRequest,
  FeeConfigurationDto,
  FeeTypeValue,
  UpdateFeeConfigurationRequest,
} from "@/lib/metadatas/fee.types";
import { createRiskFactor, getRiskFactors, updateRiskFactor } from "@/lib/metadatas/risk-factor.api";
import type {
  CreateRiskFactorRequest,
  RiskFactorDto,
  RiskLevelValue,
  UpdateRiskFactorRequest,
} from "@/lib/metadatas/risk-factor.types";
import { cn } from "@/lib/utils";

const tabs = ["currencies", "fees", "risk_factors"] as const;
const feeTypeOptions = [
  { value: 0, label: "Broker commission" },
  { value: 1, label: "Risk adjustment" },
  { value: 2, label: "Admin fee" },
] as const;
const riskLevelOptions = [
  { value: 0, label: "Country" },
  { value: 1, label: "County" },
  { value: 2, label: "City" },
  { value: 3, label: "Building type" },
] as const;
const buildingTypeOptions = [
  { value: 0, label: "Residential" },
  { value: 1, label: "Commercial" },
  { value: 2, label: "Industrial" },
  { value: 3, label: "Mixed use" },
] as const;

type SettingsTab = typeof tabs[number];

type CurrencyFormState = {
  code: string;
  name: string;
  exchangeRateToBase: string;
  isActive: boolean;
};

type FeeFormState = {
  id?: string;
  name: string;
  feeType: string;
  percentage: string;
  effectiveFrom: string;
  effectiveTo: string;
  isActive: boolean;
};

type RiskFactorFormState = {
  id?: string;
  name: string;
  riskLevel: string;
  countryId: string;
  countyId: string;
  cityId: string;
  buildingType: string;
  adjustmentPercentage: string;
  isActive: boolean;
};

const initialCurrencyForm: CurrencyFormState = {
  code: "",
  name: "",
  exchangeRateToBase: "1",
  isActive: true,
};

const initialFeeForm: FeeFormState = {
  name: "",
  feeType: String(feeTypeOptions[0].value),
  percentage: "0",
  effectiveFrom: new Date().toISOString().slice(0, 10),
  effectiveTo: "",
  isActive: true,
};

const initialRiskFactorForm: RiskFactorFormState = {
  name: "",
  riskLevel: String(riskLevelOptions[0].value),
  countryId: "",
  countyId: "",
  cityId: "",
  buildingType: String(buildingTypeOptions[0].value),
  adjustmentPercentage: "0",
  isActive: true,
};

function formatDateOnly(value: string | null) {
  if (!value) {
    return "Open-ended";
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

function normalizeFeeType(value: FeeTypeValue) {
  if (typeof value === "number") {
    return value;
  }

  if (/^\d+$/.test(value)) {
    return Number(value);
  }

  const normalized = value.toLowerCase();
  if (normalized.includes("broker")) {
    return 0;
  }

  if (normalized.includes("risk")) {
    return 1;
  }

  return 2;
}

function getFeeTypeLabel(value: FeeTypeValue) {
  return feeTypeOptions.find((option) => option.value === normalizeFeeType(value))?.label ?? "Unknown";
}

function normalizeRiskLevel(value: RiskLevelValue) {
  if (typeof value === "number") {
    return value;
  }

  if (/^\d+$/.test(value)) {
    return Number(value);
  }

  const normalized = value.toLowerCase();
  if (normalized.includes("county")) {
    return 1;
  }

  if (normalized.includes("city")) {
    return 2;
  }

  if (normalized.includes("building")) {
    return 3;
  }

  return 0;
}

function getRiskLevelLabel(value: RiskLevelValue) {
  return riskLevelOptions.find((option) => option.value === normalizeRiskLevel(value))?.label ?? "Unknown";
}

function getStatusChip(isActive: boolean) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-2 rounded-full px-2.5 py-1 text-xs font-medium",
        isActive ? "bg-success/10 text-success" : "bg-muted text-muted-foreground",
      )}
    >
      <span className={cn("h-1.5 w-1.5 rounded-full", isActive ? "bg-success" : "bg-muted-foreground")} />
      {isActive ? "Active" : "Inactive"}
    </span>
  );
}

function getReferenceLabel(riskFactor: RiskFactorDto, geographyTree: GeographyTreeCountry[]) {
  const level = normalizeRiskLevel(riskFactor.riskLevel);

  if (level === 3) {
    return "Scoped by building type";
  }

  if (!riskFactor.referenceId) {
    return "Not set";
  }

  for (const country of geographyTree) {
    if (level === 0 && country.id === riskFactor.referenceId) {
      return country.name;
    }

    for (const county of country.counties) {
      if (level === 1 && county.id === riskFactor.referenceId) {
        return `${country.name} / ${county.name}`;
      }

      for (const city of county.cities) {
        if (level === 2 && city.id === riskFactor.referenceId) {
          return `${country.name} / ${county.name} / ${city.name}`;
        }
      }
    }
  }

  return riskFactor.referenceId;
}

function getSelectedReferenceId(form: RiskFactorFormState) {
  const riskLevel = Number(form.riskLevel);

  if (riskLevel === 0) {
    return form.countryId || null;
  }

  if (riskLevel === 1) {
    return form.countyId || null;
  }

  if (riskLevel === 2) {
    return form.cityId || null;
  }

  return null;
}

export default function SettingsPage() {
  const { role } = useRole();
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<SettingsTab>("currencies");
  const [currencyForm, setCurrencyForm] = useState<CurrencyFormState>(initialCurrencyForm);
  const [feeForm, setFeeForm] = useState<FeeFormState>(initialFeeForm);
  const [riskFactorForm, setRiskFactorForm] = useState<RiskFactorFormState>(initialRiskFactorForm);
  const [editingCurrency, setEditingCurrency] = useState<CurrencyDto | null>(null);
  const [editingFee, setEditingFee] = useState<FeeConfigurationDto | null>(null);
  const [editingRiskFactor, setEditingRiskFactor] = useState<RiskFactorDto | null>(null);
  const [currencyError, setCurrencyError] = useState("");
  const [feeError, setFeeError] = useState("");
  const [riskFactorError, setRiskFactorError] = useState("");

  const currenciesQuery = useQuery({
    queryKey: ["settings", "currencies"],
    queryFn: () => getCurrencies("admin", 100, undefined),
    enabled: role === "admin",
    staleTime: 30000,
  });

  const feesQuery = useQuery({
    queryKey: ["settings", "fees"],
    queryFn: () => getFeeConfigurations(100, undefined),
    enabled: role === "admin",
    staleTime: 30000,
  });

  const riskFactorsQuery = useQuery({
    queryKey: ["settings", "risk-factors"],
    queryFn: () => getRiskFactors(100, undefined),
    enabled: role === "admin",
    staleTime: 30000,
  });

  const geographyTreeQuery = useQuery({
    queryKey: ["settings", "geographies"],
    queryFn: () => getGeographyTree("admin"),
    enabled: role === "admin" && activeTab === "risk_factors",
    staleTime: 30000,
  });

  const countries = geographyTreeQuery.data ?? [];
  const selectedCountry = countries.find((country) => country.id === riskFactorForm.countryId) ?? null;
  const counties = selectedCountry?.counties ?? [];
  const selectedCounty = counties.find((county) => county.id === riskFactorForm.countyId) ?? null;
  const cities = selectedCounty?.cities ?? [];

  const refreshSettings = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["settings", "currencies"] }),
      queryClient.invalidateQueries({ queryKey: ["settings", "fees"] }),
      queryClient.invalidateQueries({ queryKey: ["settings", "risk-factors"] }),
    ]);
  };

  const currencyMutation = useMutation({
    mutationFn: async () => {
      const exchangeRateToBase = Number(currencyForm.exchangeRateToBase);
      if (!Number.isFinite(exchangeRateToBase) || exchangeRateToBase <= 0) {
        throw new Error("Exchange rate must be greater than zero.");
      }

      if (editingCurrency) {
        const payload: UpdateCurrencyRequest = {
          id: editingCurrency.id,
          name: currencyForm.name.trim(),
          exchangeRateToBase,
          isActive: currencyForm.isActive,
        };

        await updateCurrency(editingCurrency.id, payload);
        return;
      }

      const payload: CreateCurrencyRequest = {
        code: currencyForm.code.trim().toUpperCase(),
        name: currencyForm.name.trim(),
        exchangeRateToBase,
        isActive: currencyForm.isActive,
        createdAt: new Date().toISOString(),
        updatedAt: null,
      };

      await createCurrency(payload);
    },
    onSuccess: async () => {
      await refreshSettings();
      setCurrencyForm(initialCurrencyForm);
      setEditingCurrency(null);
      setCurrencyError("");
      toast.success(editingCurrency ? "Currency updated" : "Currency created");
    },
    onError: (error) => {
      setCurrencyError(error instanceof Error ? error.message : "Failed to save currency.");
    },
  });

  const feeMutation = useMutation({
    mutationFn: async () => {
      const percentage = Number(feeForm.percentage);
      if (!Number.isFinite(percentage) || percentage < 0) {
        throw new Error("Percentage must be zero or greater.");
      }

      if (!feeForm.effectiveFrom) {
        throw new Error("Effective from date is required.");
      }

      if (feeForm.effectiveTo && feeForm.effectiveTo < feeForm.effectiveFrom) {
        throw new Error("Effective to date must be on or after effective from date.");
      }

      if (editingFee) {
        const payload: UpdateFeeConfigurationRequest = {
          id: editingFee.id,
          name: feeForm.name.trim(),
          percentage,
          effectiveFrom: feeForm.effectiveFrom,
          effectiveTo: feeForm.effectiveTo || null,
          isActive: feeForm.isActive,
        };

        await updateFeeConfiguration(editingFee.id, payload);
        return;
      }

      const payload: CreateFeeConfigurationRequest = {
        name: feeForm.name.trim(),
        feeType: Number(feeForm.feeType),
        percentage,
        effectiveFrom: feeForm.effectiveFrom,
        effectiveTo: feeForm.effectiveTo || null,
        isActive: feeForm.isActive,
      };

      await createFeeConfiguration(payload);
    },
    onSuccess: async () => {
      await refreshSettings();
      setFeeForm(initialFeeForm);
      setEditingFee(null);
      setFeeError("");
      toast.success(editingFee ? "Fee updated" : "Fee created");
    },
    onError: (error) => {
      setFeeError(error instanceof Error ? error.message : "Failed to save fee configuration.");
    },
  });

  const riskFactorMutation = useMutation({
    mutationFn: async () => {
      const adjustmentPercentage = Number(riskFactorForm.adjustmentPercentage);
      if (!Number.isFinite(adjustmentPercentage) || adjustmentPercentage < -100 || adjustmentPercentage > 100) {
        throw new Error("Adjustment percentage must be between -100 and 100.");
      }

      const riskLevel = Number(riskFactorForm.riskLevel);

      if (editingRiskFactor) {
        const payload: UpdateRiskFactorRequest = {
          name: riskFactorForm.name.trim(),
          riskLevel,
          adjustementPercentage: adjustmentPercentage,
          isActive: riskFactorForm.isActive,
        };

        await updateRiskFactor(editingRiskFactor.id, payload);
        return;
      }

      const referenceId = getSelectedReferenceId(riskFactorForm);
      if (riskLevel !== 3 && !referenceId) {
        throw new Error("Please select the geography scope for this risk factor.");
      }

      const payload: CreateRiskFactorRequest = {
        name: riskFactorForm.name.trim(),
        level: riskLevel,
        referenceId,
        buildingType: riskLevel === 3 ? Number(riskFactorForm.buildingType) : null,
        adjustementPercentage: adjustmentPercentage,
        isActive: riskFactorForm.isActive,
      };

      await createRiskFactor(payload);
    },
    onSuccess: async () => {
      await refreshSettings();
      setRiskFactorForm(initialRiskFactorForm);
      setEditingRiskFactor(null);
      setRiskFactorError("");
      toast.success(editingRiskFactor ? "Risk factor updated" : "Risk factor created");
    },
    onError: (error) => {
      setRiskFactorError(error instanceof Error ? error.message : "Failed to save risk factor.");
    },
  });

  const currencies = useMemo(
    () => (currenciesQuery.data ?? []).slice().sort((left, right) => left.code.localeCompare(right.code)),
    [currenciesQuery.data],
  );

  const fees = useMemo(
    () => (feesQuery.data ?? []).slice().sort((left, right) => left.name.localeCompare(right.name)),
    [feesQuery.data],
  );

  const riskFactors = useMemo(
    () => (riskFactorsQuery.data ?? []).slice().sort((left, right) => left.name.localeCompare(right.name)),
    [riskFactorsQuery.data],
  );

  const openCurrencyEditor = (currency?: CurrencyDto) => {
    if (!currency) {
      setEditingCurrency(null);
      setCurrencyForm(initialCurrencyForm);
      setCurrencyError("");
      return;
    }

    setEditingCurrency(currency);
    setCurrencyForm({
      code: currency.code,
      name: currency.name,
      exchangeRateToBase: String(currency.exchangeRateToBase),
      isActive: currency.isActive,
    });
    setCurrencyError("");
  };

  const openFeeEditor = (fee?: FeeConfigurationDto) => {
    if (!fee) {
      setEditingFee(null);
      setFeeForm(initialFeeForm);
      setFeeError("");
      return;
    }

    setEditingFee(fee);
    setFeeForm({
      id: fee.id,
      name: fee.name,
      feeType: String(normalizeFeeType(fee.feeType)),
      percentage: String(fee.percentage),
      effectiveFrom: fee.effectiveFrom,
      effectiveTo: fee.effectiveTo ?? "",
      isActive: fee.isActive,
    });
    setFeeError("");
  };

  const openRiskFactorEditor = (riskFactor?: RiskFactorDto) => {
    if (!riskFactor) {
      setEditingRiskFactor(null);
      setRiskFactorForm(initialRiskFactorForm);
      setRiskFactorError("");
      return;
    }

    const level = normalizeRiskLevel(riskFactor.riskLevel);
    const nextForm: RiskFactorFormState = {
      id: riskFactor.id,
      name: riskFactor.name,
      riskLevel: String(level),
      countryId: "",
      countyId: "",
      cityId: "",
      buildingType: String(buildingTypeOptions[0].value),
      adjustmentPercentage: String(riskFactor.adjustementPercentage),
      isActive: riskFactor.isActive,
    };

    if (level === 0 && riskFactor.referenceId) {
      nextForm.countryId = riskFactor.referenceId;
    }

    if (level === 1 && riskFactor.referenceId) {
      for (const country of countries) {
        const county = country.counties.find((entry) => entry.id === riskFactor.referenceId);
        if (county) {
          nextForm.countryId = country.id;
          nextForm.countyId = county.id;
          break;
        }
      }
    }

    if (level === 2 && riskFactor.referenceId) {
      for (const country of countries) {
        for (const county of country.counties) {
          const city = county.cities.find((entry) => entry.id === riskFactor.referenceId);
          if (city) {
            nextForm.countryId = country.id;
            nextForm.countyId = county.id;
            nextForm.cityId = city.id;
            break;
          }
        }
      }
    }

    setEditingRiskFactor(riskFactor);
    setRiskFactorForm(nextForm);
    setRiskFactorError("");
  };

  const handleCurrencySubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setCurrencyError("");
    await currencyMutation.mutateAsync();
  };

  const handleFeeSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setFeeError("");
    await feeMutation.mutateAsync();
  };

  const handleRiskFactorSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setRiskFactorError("");
    await riskFactorMutation.mutateAsync();
  };

  if (role !== "admin") {
    return (
      <div className="space-y-6">
        <PageHeader title="Settings" description="System configuration is available only for administrators" />
        <div className="glass-card p-8">
          <EmptyState
            icon={ShieldAlert}
            title="Admin access required"
            description="Switch to an admin account to manage currencies, fee configurations, and risk factors."
          />
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Settings" description="Manage portfolio metadata, pricing inputs, and risk configuration" />

      <div className="flex items-center gap-1 rounded-lg bg-muted/50 p-0.5 w-fit">
        {tabs.map((tab) => (
          <button
            key={tab}
            type="button"
            onClick={() => setActiveTab(tab)}
            className={cn(
              "rounded-md px-4 py-2 text-sm font-medium capitalize transition-all",
              activeTab === tab ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
            )}
          >
            {tab.replace("_", " ")}
          </button>
        ))}
      </div>

      {activeTab === "currencies" && (
        <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
          <div className="glass-card overflow-hidden">
            <div className="flex items-center justify-between border-b border-border px-5 py-4">
              <div>
                <h2 className="text-lg font-semibold">Currencies</h2>
                <p className="text-sm text-muted-foreground">Maintain exchange rates and active settlement currencies.</p>
              </div>
              <button
                type="button"
                onClick={() => openCurrencyEditor()}
                className="flex items-center gap-2 rounded-lg border border-border px-4 py-2 text-sm font-medium transition-colors hover:bg-muted"
              >
                <Plus className="h-4 w-4" /> Add Currency
              </button>
            </div>

            {currenciesQuery.isLoading ? (
              <div className="space-y-3 p-5">
                {Array.from({ length: 5 }).map((_, index) => (
                  <Skeleton key={index} className="h-12 w-full" />
                ))}
              </div>
            ) : currenciesQuery.isError ? (
              <div className="flex items-center gap-3 p-5 text-destructive">
                <AlertCircle className="h-5 w-5" />
                <span>{currenciesQuery.error instanceof Error ? currenciesQuery.error.message : "Failed to load currencies."}</span>
              </div>
            ) : currencies.length === 0 ? (
              <div className="p-5">
                <EmptyState icon={Coins} title="No currencies" description="Add the first currency to configure settlement and pricing." />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="table-premium">
                  <thead>
                    <tr>
                      <th>Code</th>
                      <th>Name</th>
                      <th>Exchange Rate</th>
                      <th>Status</th>
                      <th className="text-right">Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {currencies.map((currency) => (
                      <tr key={currency.id}>
                        <td className="font-mono text-sm font-semibold">{currency.code}</td>
                        <td className="text-sm">{currency.name}</td>
                        <td className="text-sm font-medium">{currency.exchangeRateToBase.toFixed(4)}</td>
                        <td>{getStatusChip(currency.isActive)}</td>
                        <td className="text-right">
                          <button
                            type="button"
                            onClick={() => openCurrencyEditor(currency)}
                            className="inline-flex items-center gap-2 rounded-lg border border-border px-3 py-1.5 text-sm font-medium transition-colors hover:bg-muted"
                          >
                            <PencilLine className="h-4 w-4" /> Edit
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <motion.div initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} className="glass-card p-5">
            <h2 className="text-lg font-semibold">{editingCurrency ? `Edit ${editingCurrency.code}` : "Add Currency"}</h2>
            <p className="mt-1 text-sm text-muted-foreground">{editingCurrency ? "Update display name, rate, and activity state." : "Create a new currency available to administrators and brokers."}</p>

            <form onSubmit={handleCurrencySubmit} className="mt-5 space-y-4">
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <label htmlFor="currency-code" className="text-sm font-medium">Code</label>
                  <input
                    id="currency-code"
                    type="text"
                    maxLength={3}
                    value={currencyForm.code}
                    onChange={(event) => setCurrencyForm((current) => ({ ...current, code: event.target.value.toUpperCase() }))}
                    disabled={Boolean(editingCurrency) || currencyMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  />
                </div>
                <div className="space-y-2">
                  <label htmlFor="currency-rate" className="text-sm font-medium">Exchange Rate To Base</label>
                  <input
                    id="currency-rate"
                    type="number"
                    min="0.0001"
                    step="0.0001"
                    value={currencyForm.exchangeRateToBase}
                    onChange={(event) => setCurrencyForm((current) => ({ ...current, exchangeRateToBase: event.target.value }))}
                    disabled={currencyMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label htmlFor="currency-name" className="text-sm font-medium">Name</label>
                <input
                  id="currency-name"
                  type="text"
                  value={currencyForm.name}
                  onChange={(event) => setCurrencyForm((current) => ({ ...current, name: event.target.value }))}
                  disabled={currencyMutation.isPending}
                  className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                />
              </div>

              <label className="flex items-center gap-3 rounded-lg border border-border bg-muted/30 px-3 py-2 text-sm">
                <input
                  type="checkbox"
                  checked={currencyForm.isActive}
                  onChange={(event) => setCurrencyForm((current) => ({ ...current, isActive: event.target.checked }))}
                  disabled={currencyMutation.isPending}
                />
                Active currency
              </label>

              {currencyError && <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{currencyError}</div>}

              <div className="flex items-center gap-3">
                <button
                  type="submit"
                  disabled={currencyMutation.isPending}
                  className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-60"
                >
                  {currencyMutation.isPending ? "Saving..." : editingCurrency ? "Update Currency" : "Create Currency"}
                </button>
                {editingCurrency && (
                  <button
                    type="button"
                    onClick={() => openCurrencyEditor()}
                    className="rounded-lg border border-border px-4 py-2 text-sm font-medium hover:bg-muted"
                  >
                    Cancel edit
                  </button>
                )}
              </div>
            </form>
          </motion.div>
        </div>
      )}

      {activeTab === "fees" && (
        <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
          <div className="glass-card overflow-hidden">
            <div className="flex items-center justify-between border-b border-border px-5 py-4">
              <div>
                <h2 className="text-lg font-semibold">Fee Configurations</h2>
                <p className="text-sm text-muted-foreground">Manage portfolio-wide fee percentages and validity windows.</p>
              </div>
              <button
                type="button"
                onClick={() => openFeeEditor()}
                className="flex items-center gap-2 rounded-lg border border-border px-4 py-2 text-sm font-medium transition-colors hover:bg-muted"
              >
                <Plus className="h-4 w-4" /> Add Fee
              </button>
            </div>

            {feesQuery.isLoading ? (
              <div className="space-y-3 p-5">
                {Array.from({ length: 5 }).map((_, index) => (
                  <Skeleton key={index} className="h-12 w-full" />
                ))}
              </div>
            ) : feesQuery.isError ? (
              <div className="flex items-center gap-3 p-5 text-destructive">
                <AlertCircle className="h-5 w-5" />
                <span>{feesQuery.error instanceof Error ? feesQuery.error.message : "Failed to load fee configurations."}</span>
              </div>
            ) : fees.length === 0 ? (
              <div className="p-5">
                <EmptyState icon={Percent} title="No fee configurations" description="Add fee settings to support premium pricing and charges." />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="table-premium">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Type</th>
                      <th>Percentage</th>
                      <th>Effective Window</th>
                      <th>Status</th>
                      <th className="text-right">Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {fees.map((fee) => (
                      <tr key={fee.id}>
                        <td className="text-sm font-medium">{fee.name}</td>
                        <td className="text-sm">{getFeeTypeLabel(fee.feeType)}</td>
                        <td className="text-sm font-medium">{fee.percentage}%</td>
                        <td className="text-xs text-muted-foreground">{formatDateOnly(fee.effectiveFrom)} to {formatDateOnly(fee.effectiveTo)}</td>
                        <td>{getStatusChip(fee.isActive)}</td>
                        <td className="text-right">
                          <button
                            type="button"
                            onClick={() => openFeeEditor(fee)}
                            className="inline-flex items-center gap-2 rounded-lg border border-border px-3 py-1.5 text-sm font-medium transition-colors hover:bg-muted"
                          >
                            <PencilLine className="h-4 w-4" /> Edit
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <motion.div initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} className="glass-card p-5">
            <h2 className="text-lg font-semibold">{editingFee ? `Edit ${editingFee.name}` : "Add Fee Configuration"}</h2>
            <p className="mt-1 text-sm text-muted-foreground">{editingFee ? "Fee type is fixed after creation; update the current percentage and validity dates here." : "Create a fee rule with a type, percentage, and effective period."}</p>

            <form onSubmit={handleFeeSubmit} className="mt-5 space-y-4">
              <div className="space-y-2">
                <label htmlFor="fee-name" className="text-sm font-medium">Name</label>
                <input
                  id="fee-name"
                  type="text"
                  value={feeForm.name}
                  onChange={(event) => setFeeForm((current) => ({ ...current, name: event.target.value }))}
                  disabled={feeMutation.isPending}
                  className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                />
              </div>

              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <label htmlFor="fee-type" className="text-sm font-medium">Fee Type</label>
                  <select
                    id="fee-type"
                    value={feeForm.feeType}
                    onChange={(event) => setFeeForm((current) => ({ ...current, feeType: event.target.value }))}
                    disabled={Boolean(editingFee) || feeMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  >
                    {feeTypeOptions.map((option) => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </select>
                </div>
                <div className="space-y-2">
                  <label htmlFor="fee-percentage" className="text-sm font-medium">Percentage</label>
                  <input
                    id="fee-percentage"
                    type="number"
                    min="0"
                    max="100"
                    step="0.01"
                    value={feeForm.percentage}
                    onChange={(event) => setFeeForm((current) => ({ ...current, percentage: event.target.value }))}
                    disabled={feeMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  />
                </div>
              </div>

              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <label htmlFor="fee-effective-from" className="text-sm font-medium">Effective From</label>
                  <input
                    id="fee-effective-from"
                    type="date"
                    value={feeForm.effectiveFrom}
                    onChange={(event) => setFeeForm((current) => ({ ...current, effectiveFrom: event.target.value }))}
                    disabled={feeMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  />
                </div>
                <div className="space-y-2">
                  <label htmlFor="fee-effective-to" className="text-sm font-medium">Effective To</label>
                  <input
                    id="fee-effective-to"
                    type="date"
                    value={feeForm.effectiveTo}
                    onChange={(event) => setFeeForm((current) => ({ ...current, effectiveTo: event.target.value }))}
                    disabled={feeMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  />
                </div>
              </div>

              <label className="flex items-center gap-3 rounded-lg border border-border bg-muted/30 px-3 py-2 text-sm">
                <input
                  type="checkbox"
                  checked={feeForm.isActive}
                  onChange={(event) => setFeeForm((current) => ({ ...current, isActive: event.target.checked }))}
                  disabled={feeMutation.isPending}
                />
                Active fee configuration
              </label>

              {feeError && <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{feeError}</div>}

              <div className="flex items-center gap-3">
                <button
                  type="submit"
                  disabled={feeMutation.isPending}
                  className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-60"
                >
                  {feeMutation.isPending ? "Saving..." : editingFee ? "Update Fee" : "Create Fee"}
                </button>
                {editingFee && (
                  <button
                    type="button"
                    onClick={() => openFeeEditor()}
                    className="rounded-lg border border-border px-4 py-2 text-sm font-medium hover:bg-muted"
                  >
                    Cancel edit
                  </button>
                )}
              </div>
            </form>
          </motion.div>
        </div>
      )}

      {activeTab === "risk_factors" && (
        <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
          <div className="glass-card overflow-hidden">
            <div className="flex items-center justify-between border-b border-border px-5 py-4">
              <div>
                <h2 className="text-lg font-semibold">Risk Factors</h2>
                <p className="text-sm text-muted-foreground">Define geography and building-based premium adjustments.</p>
              </div>
              <button
                type="button"
                onClick={() => openRiskFactorEditor()}
                className="flex items-center gap-2 rounded-lg border border-border px-4 py-2 text-sm font-medium transition-colors hover:bg-muted"
              >
                <Plus className="h-4 w-4" /> Add Risk Factor
              </button>
            </div>

            {riskFactorsQuery.isLoading || geographyTreeQuery.isLoading ? (
              <div className="space-y-3 p-5">
                {Array.from({ length: 5 }).map((_, index) => (
                  <Skeleton key={index} className="h-12 w-full" />
                ))}
              </div>
            ) : riskFactorsQuery.isError ? (
              <div className="flex items-center gap-3 p-5 text-destructive">
                <AlertCircle className="h-5 w-5" />
                <span>{riskFactorsQuery.error instanceof Error ? riskFactorsQuery.error.message : "Failed to load risk factors."}</span>
              </div>
            ) : geographyTreeQuery.isError ? (
              <div className="flex items-center gap-3 p-5 text-destructive">
                <AlertCircle className="h-5 w-5" />
                <span>{geographyTreeQuery.error instanceof Error ? geographyTreeQuery.error.message : "Failed to load geographies."}</span>
              </div>
            ) : riskFactors.length === 0 ? (
              <div className="p-5">
                <EmptyState icon={ShieldAlert} title="No risk factors" description="Add risk factors to influence premium calculations by geography or building type." />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="table-premium">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Level</th>
                      <th>Scope</th>
                      <th>Adjustment</th>
                      <th>Status</th>
                      <th className="text-right">Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {riskFactors.map((riskFactor) => (
                      <tr key={riskFactor.id}>
                        <td className="text-sm font-medium">{riskFactor.name}</td>
                        <td className="text-sm">{getRiskLevelLabel(riskFactor.riskLevel)}</td>
                        <td className="text-xs text-muted-foreground">{getReferenceLabel(riskFactor, countries)}</td>
                        <td className="text-sm font-medium">{riskFactor.adjustementPercentage}%</td>
                        <td>{getStatusChip(riskFactor.isActive)}</td>
                        <td className="text-right">
                          <button
                            type="button"
                            onClick={() => openRiskFactorEditor(riskFactor)}
                            className="inline-flex items-center gap-2 rounded-lg border border-border px-3 py-1.5 text-sm font-medium transition-colors hover:bg-muted"
                          >
                            <PencilLine className="h-4 w-4" /> Edit
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <motion.div initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} className="glass-card p-5">
            <h2 className="text-lg font-semibold">{editingRiskFactor ? `Edit ${editingRiskFactor.name}` : "Add Risk Factor"}</h2>
            <p className="mt-1 text-sm text-muted-foreground">{editingRiskFactor ? "Risk scope remains fixed after creation; update name, adjustment, and activity state here." : "Choose a risk scope, then bind it to a geography or building type."}</p>

            <form onSubmit={handleRiskFactorSubmit} className="mt-5 space-y-4">
              <div className="space-y-2">
                <label htmlFor="risk-name" className="text-sm font-medium">Name</label>
                <input
                  id="risk-name"
                  type="text"
                  value={riskFactorForm.name}
                  onChange={(event) => setRiskFactorForm((current) => ({ ...current, name: event.target.value }))}
                  disabled={riskFactorMutation.isPending}
                  className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                />
              </div>

              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <label htmlFor="risk-level" className="text-sm font-medium">Risk Level</label>
                  <select
                    id="risk-level"
                    value={riskFactorForm.riskLevel}
                    onChange={(event) => setRiskFactorForm((current) => ({
                      ...current,
                      riskLevel: event.target.value,
                      countryId: "",
                      countyId: "",
                      cityId: "",
                    }))}
                    disabled={Boolean(editingRiskFactor) || riskFactorMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  >
                    {riskLevelOptions.map((option) => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </select>
                </div>
                <div className="space-y-2">
                  <label htmlFor="risk-adjustment" className="text-sm font-medium">Adjustment Percentage</label>
                  <input
                    id="risk-adjustment"
                    type="number"
                    min="-100"
                    max="100"
                    step="0.01"
                    value={riskFactorForm.adjustmentPercentage}
                    onChange={(event) => setRiskFactorForm((current) => ({ ...current, adjustmentPercentage: event.target.value }))}
                    disabled={riskFactorMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  />
                </div>
              </div>

              {Number(riskFactorForm.riskLevel) !== 3 && (
                <div className="space-y-4 rounded-lg border border-border bg-muted/20 p-4">
                  <div className="space-y-2">
                    <label htmlFor="risk-country" className="text-sm font-medium">Country</label>
                    <select
                      id="risk-country"
                      value={riskFactorForm.countryId}
                      onChange={(event) => setRiskFactorForm((current) => ({
                        ...current,
                        countryId: event.target.value,
                        countyId: "",
                        cityId: "",
                      }))}
                      disabled={Boolean(editingRiskFactor) || riskFactorMutation.isPending}
                      className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                    >
                      <option value="">Select country</option>
                      {countries.map((country) => (
                        <option key={country.id} value={country.id}>{country.name}</option>
                      ))}
                    </select>
                  </div>

                  {Number(riskFactorForm.riskLevel) >= 1 && (
                    <div className="space-y-2">
                      <label htmlFor="risk-county" className="text-sm font-medium">County</label>
                      <select
                        id="risk-county"
                        value={riskFactorForm.countyId}
                        onChange={(event) => setRiskFactorForm((current) => ({
                          ...current,
                          countyId: event.target.value,
                          cityId: "",
                        }))}
                        disabled={!riskFactorForm.countryId || Boolean(editingRiskFactor) || riskFactorMutation.isPending}
                        className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                      >
                        <option value="">Select county</option>
                        {counties.map((county) => (
                          <option key={county.id} value={county.id}>{county.name}</option>
                        ))}
                      </select>
                    </div>
                  )}

                  {Number(riskFactorForm.riskLevel) >= 2 && (
                    <div className="space-y-2">
                      <label htmlFor="risk-city" className="text-sm font-medium">City</label>
                      <select
                        id="risk-city"
                        value={riskFactorForm.cityId}
                        onChange={(event) => setRiskFactorForm((current) => ({ ...current, cityId: event.target.value }))}
                        disabled={!riskFactorForm.countyId || Boolean(editingRiskFactor) || riskFactorMutation.isPending}
                        className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                      >
                        <option value="">Select city</option>
                        {cities.map((city) => (
                          <option key={city.id} value={city.id}>{city.name}</option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>
              )}

              {Number(riskFactorForm.riskLevel) === 3 && (
                <div className="space-y-2">
                  <label htmlFor="risk-building-type" className="text-sm font-medium">Building Type</label>
                  <select
                    id="risk-building-type"
                    value={riskFactorForm.buildingType}
                    onChange={(event) => setRiskFactorForm((current) => ({ ...current, buildingType: event.target.value }))}
                    disabled={Boolean(editingRiskFactor) || riskFactorMutation.isPending}
                    className="h-10 w-full rounded-lg border border-border bg-muted/50 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-60"
                  >
                    {buildingTypeOptions.map((option) => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </select>
                </div>
              )}

              <label className="flex items-center gap-3 rounded-lg border border-border bg-muted/30 px-3 py-2 text-sm">
                <input
                  type="checkbox"
                  checked={riskFactorForm.isActive}
                  onChange={(event) => setRiskFactorForm((current) => ({ ...current, isActive: event.target.checked }))}
                  disabled={riskFactorMutation.isPending}
                />
                Active risk factor
              </label>

              {riskFactorError && <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">{riskFactorError}</div>}

              <div className="flex items-center gap-3">
                <button
                  type="submit"
                  disabled={riskFactorMutation.isPending}
                  className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-60"
                >
                  {riskFactorMutation.isPending ? "Saving..." : editingRiskFactor ? "Update Risk Factor" : "Create Risk Factor"}
                </button>
                {editingRiskFactor && (
                  <button
                    type="button"
                    onClick={() => openRiskFactorEditor()}
                    className="rounded-lg border border-border px-4 py-2 text-sm font-medium hover:bg-muted"
                  >
                    Cancel edit
                  </button>
                )}
              </div>
            </form>
          </motion.div>
        </div>
      )}
    </div>
  );
}
