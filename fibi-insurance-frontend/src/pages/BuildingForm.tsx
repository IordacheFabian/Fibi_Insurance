import { ChangeEvent, FormEvent, useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { PageHeader } from "@/components/ui/PageHeader";
import { getClients } from "@/lib/clients/client.api";
import { getCurrencies } from "@/lib/metadatas/currency.api";
import { getGeographyTree } from "@/lib/geographies/geography.api";
import type { GeographyTreeCountry } from "@/lib/geographies/geography.types";
import type { CreateBuildingDto, UpdateBuildingDto } from "@/lib/buildings/building.type";

export type BuildingFormValues = {
  clientId: string;
  currencyId: string;
  countryId: string;
  countyId: string;
  cityId: string;
  street: string;
  number: string;
  constructionYear: string;
  buildingType: "residential" | "commercial" | "industrial" | "mixed-use";
  numberOfFloors: string;
  surfaceArea: string;
  insuredValue: string;
  riskIndicators: string;
};

type Props = {
  mode?: "create" | "edit";
  title?: string;
  description?: string;
  initialValues?: BuildingFormValues;
  isSubmitting?: boolean;
  cancelPath?: string;
  onSubmit: (values: BuildingFormValues) => Promise<void>;
};

const emptyValues: BuildingFormValues = {
  clientId: "",
  currencyId: "",
  countryId: "",
  countyId: "",
  cityId: "",
  street: "",
  number: "",
  constructionYear: "",
  buildingType: "residential",
  numberOfFloors: "",
  surfaceArea: "",
  insuredValue: "",
  riskIndicators: "",
};

function findLocationIds(geographyTree: GeographyTreeCountry[], cityId: string) {
  for (const country of geographyTree) {
    for (const county of country.counties) {
      const city = county.cities.find((item) => item.id === cityId);
      if (city) {
        return {
          countryId: country.id,
          countyId: county.id,
        };
      }
    }
  }

  return null;
}

function mapBuildingTypeToApiValue(buildingType: BuildingFormValues["buildingType"]): 0 | 1 | 2 | 3 {
  if (buildingType === "commercial") {
    return 1;
  }

  if (buildingType === "industrial") {
    return 2;
  }

  if (buildingType === "mixed-use") {
    return 3;
  }

  return 0;
}

export function mapBuildingTypeFromApiValue(buildingType: number | string): BuildingFormValues["buildingType"] {
  if (buildingType === 1 || buildingType === "1" || buildingType === "Commercial" || buildingType === "commercial") {
    return "commercial";
  }

  if (buildingType === 2 || buildingType === "2" || buildingType === "Industrial" || buildingType === "industrial") {
    return "industrial";
  }

  if (buildingType === 3 || buildingType === "3" || buildingType === "MixedUse" || buildingType === "mixed-use") {
    return "mixed-use";
  }

  return "residential";
}

export function mapFormValuesToCreatePayload(values: BuildingFormValues): CreateBuildingDto {
  return {
    clientId: values.clientId,
    currencyId: values.currencyId,
    address: {
      street: values.street.trim(),
      number: values.number.trim(),
      cityId: values.cityId,
    },
    constructionYear: Number(values.constructionYear),
    buildingType: mapBuildingTypeToApiValue(values.buildingType),
    numberOfFloors: Number(values.numberOfFloors),
    surfaceArea: Number(values.surfaceArea),
    insuredValue: Number(values.insuredValue),
    riskIndicators: values.riskIndicators.trim(),
  };
}

export function mapFormValuesToUpdatePayload(id: string, values: BuildingFormValues): UpdateBuildingDto {
  return {
    id,
    currencyId: values.currencyId,
    address: {
      street: values.street.trim(),
      number: values.number.trim(),
      cityId: values.cityId,
    },
    constructionYear: Number(values.constructionYear),
    buildingType: mapBuildingTypeToApiValue(values.buildingType),
    numberOfFloors: Number(values.numberOfFloors),
    surfaceArea: Number(values.surfaceArea),
    insuredValue: Number(values.insuredValue),
    riskIndicatiors: values.riskIndicators.trim(),
  };
}

export default function BuildingForm({ mode = "create", title, description, initialValues, isSubmitting = false, cancelPath = "/buildings", onSubmit }: Props) {
  const navigate = useNavigate();
  const [form, setForm] = useState<BuildingFormValues>(initialValues ?? emptyValues);
  const [error, setError] = useState("");

  const { data: clients = [], isLoading: isClientsLoading } = useQuery({
    queryKey: ["clients", "building-form"],
    queryFn: () => getClients(),
    staleTime: 30000,
  });

  const { data: currencies = [], isLoading: isCurrenciesLoading } = useQuery({
    queryKey: ["currencies", "active"],
    queryFn: () => getCurrencies(),
    staleTime: 30000,
  });

  const { data: geographyTree = [], isLoading: isGeographiesLoading } = useQuery({
    queryKey: ["geographies", "tree"],
    queryFn: () => getGeographyTree(),
    staleTime: 30000,
  });

  useEffect(() => {
    setForm(initialValues ?? emptyValues);
  }, [initialValues]);

  useEffect(() => {
    if (!initialValues?.cityId || geographyTree.length === 0) {
      return;
    }

    const locationIds = findLocationIds(geographyTree, initialValues.cityId);
    if (!locationIds) {
      return;
    }

    setForm((current) => {
      if (current.countryId === locationIds.countryId && current.countyId === locationIds.countyId) {
        return current;
      }

      return {
        ...current,
        countryId: locationIds.countryId,
        countyId: locationIds.countyId,
      };
    });
  }, [geographyTree, initialValues?.cityId, initialValues]);

  const countyOptions = useMemo(
    () => geographyTree.find((country) => country.id === form.countryId)?.counties ?? [],
    [geographyTree, form.countryId],
  );

  const cityOptions = useMemo(
    () => countyOptions.find((county) => county.id === form.countyId)?.cities ?? [],
    [countyOptions, form.countyId],
  );

  const isEditMode = mode === "edit";
  const isLoading = isClientsLoading || isCurrenciesLoading || isGeographiesLoading;

  const handleTextChange = (field: keyof BuildingFormValues) => (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { value } = event.target;

    setForm((current) => {
      if (field === "countryId") {
        return {
          ...current,
          countryId: value,
          countyId: "",
          cityId: "",
        };
      }

      if (field === "countyId") {
        return {
          ...current,
          countyId: value,
          cityId: "",
        };
      }

      return {
        ...current,
        [field]: value,
      };
    });
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError("");

    const numericFields: Array<keyof Pick<BuildingFormValues, "constructionYear" | "numberOfFloors" | "surfaceArea" | "insuredValue">> = [
      "constructionYear",
      "numberOfFloors",
      "surfaceArea",
      "insuredValue",
    ];

    if (numericFields.some((field) => !Number.isFinite(Number(form[field])))) {
      setError("Construction year, floors, surface area, and insured value must be valid numbers.");
      return;
    }

    try {
      await onSubmit(form);
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : "Failed to save building.");
    }
  };

  if (isLoading) {
    return <div className="space-y-6"><p className="text-sm text-muted-foreground">Loading building form options...</p></div>;
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={title ?? (isEditMode ? "Update Building" : "Register Building")}
        description={description ?? (isEditMode ? "Edit building details, address, and currency" : "Create a new insured property")}
      />

      <form onSubmit={handleSubmit} className="glass-card p-5 space-y-5">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2 md:col-span-2">
            <label htmlFor="clientId" className="text-sm font-medium">Owner</label>
            <select
              id="clientId"
              value={form.clientId}
              onChange={handleTextChange("clientId")}
              disabled={isSubmitting || isEditMode}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">Select client</option>
              {clients.map((client) => (
                <option key={client.id} value={client.id}>{client.name}</option>
              ))}
            </select>
            {isEditMode && <p className="text-xs text-muted-foreground">The building owner cannot be changed from this screen.</p>}
          </div>

          <div className="space-y-2">
            <label htmlFor="currencyId" className="text-sm font-medium">Currency</label>
            <select
              id="currencyId"
              value={form.currencyId}
              onChange={handleTextChange("currencyId")}
              disabled={isSubmitting}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">Select currency</option>
              {currencies.map((currency) => (
                <option key={currency.id} value={currency.id}>{currency.code} - {currency.name}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label htmlFor="buildingType" className="text-sm font-medium">Building Type</label>
            <select
              id="buildingType"
              value={form.buildingType}
              onChange={handleTextChange("buildingType")}
              disabled={isSubmitting}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="residential">Residential</option>
              <option value="commercial">Commercial</option>
              <option value="industrial">Industrial</option>
              <option value="mixed-use">Mixed-Use</option>
            </select>
          </div>

          <div className="space-y-2">
            <label htmlFor="countryId" className="text-sm font-medium">Country</label>
            <select
              id="countryId"
              value={form.countryId}
              onChange={handleTextChange("countryId")}
              disabled={isSubmitting}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">Select country</option>
              {geographyTree.map((country) => (
                <option key={country.id} value={country.id}>{country.name}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label htmlFor="countyId" className="text-sm font-medium">County</label>
            <select
              id="countyId"
              value={form.countyId}
              onChange={handleTextChange("countyId")}
              disabled={isSubmitting || !form.countryId}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">Select county</option>
              {countyOptions.map((county) => (
                <option key={county.id} value={county.id}>{county.name}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label htmlFor="cityId" className="text-sm font-medium">City</label>
            <select
              id="cityId"
              value={form.cityId}
              onChange={handleTextChange("cityId")}
              disabled={isSubmitting || !form.countyId}
              required
              className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60"
            >
              <option value="">Select city</option>
              {cityOptions.map((city) => (
                <option key={city.id} value={city.id}>{city.name}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label htmlFor="street" className="text-sm font-medium">Street</label>
            <input id="street" type="text" value={form.street} onChange={handleTextChange("street")} required disabled={isSubmitting} className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60" />
          </div>

          <div className="space-y-2">
            <label htmlFor="number" className="text-sm font-medium">Number</label>
            <input id="number" type="text" value={form.number} onChange={handleTextChange("number")} required disabled={isSubmitting} className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60" />
          </div>

          <div className="space-y-2">
            <label htmlFor="constructionYear" className="text-sm font-medium">Construction Year</label>
            <input id="constructionYear" type="number" value={form.constructionYear} onChange={handleTextChange("constructionYear")} required disabled={isSubmitting} className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60" />
          </div>

          <div className="space-y-2">
            <label htmlFor="numberOfFloors" className="text-sm font-medium">Number of Floors</label>
            <input id="numberOfFloors" type="number" value={form.numberOfFloors} onChange={handleTextChange("numberOfFloors")} required disabled={isSubmitting} className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60" />
          </div>

          <div className="space-y-2">
            <label htmlFor="surfaceArea" className="text-sm font-medium">Surface Area (m2)</label>
            <input id="surfaceArea" type="number" value={form.surfaceArea} onChange={handleTextChange("surfaceArea")} required disabled={isSubmitting} className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60" />
          </div>

          <div className="space-y-2">
            <label htmlFor="insuredValue" className="text-sm font-medium">Insured Value</label>
            <input id="insuredValue" type="number" value={form.insuredValue} onChange={handleTextChange("insuredValue")} required disabled={isSubmitting} className="w-full h-10 px-3 rounded-lg bg-muted/50 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60" />
          </div>

          <div className="space-y-2 md:col-span-2">
            <label htmlFor="riskIndicators" className="text-sm font-medium">Risk Indicators</label>
            <textarea id="riskIndicators" value={form.riskIndicators} onChange={handleTextChange("riskIndicators")} rows={4} disabled={isSubmitting} className="w-full px-3 py-2 rounded-lg bg-muted/50 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 transition-all disabled:opacity-60" />
          </div>
        </div>

        {error && (
          <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {error}
          </div>
        )}

        <div className="flex items-center justify-end gap-2 pt-1">
          <button type="button" onClick={() => navigate(cancelPath)} disabled={isSubmitting} className="h-10 px-5 rounded-lg border border-border text-sm font-medium hover:bg-muted transition-colors disabled:opacity-60 disabled:cursor-not-allowed">
            Cancel
          </button>
          <button type="submit" disabled={isSubmitting} className="h-10 px-5 rounded-lg gradient-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-60 disabled:cursor-not-allowed">
            {isSubmitting ? (isEditMode ? "Saving..." : "Creating...") : (isEditMode ? "Save Changes" : "Create Building")}
          </button>
        </div>
      </form>
    </div>
  );
}