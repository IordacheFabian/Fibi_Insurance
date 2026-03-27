import { useState, type ChangeEvent, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import type { CreateBuildingRequest } from "./building.type";
import { getCities, getCounties, getCountries } from "../../api/addresses.api";

type Props = {
  onSubmit: (values: CreateBuildingRequest) => Promise<void>;
  isSubmitting?: boolean;
};

const initialValues: CreateBuildingRequest = {
  address: {
    street: "",
    number: "",
    cityId: "",
  },
  constructionYear: 0,
  buildingType: 0,
  numberOfFloors: 0,
  surfaceArea: 0,
  insuredValue: 0,
  riskIndicators: "",
};

const numericFields: Array<"constructionYear" | "buildingType" | "numberOfFloors" | "surfaceArea" | "insuredValue"> = [
  "constructionYear",
  "buildingType",
  "numberOfFloors",
  "surfaceArea",
  "insuredValue",
];

const isNumericField = (field: string): field is (typeof numericFields)[number] =>
  numericFields.includes(field as (typeof numericFields)[number]);

export default function CreateBuildingForm({ onSubmit, isSubmitting = false }: Props) {
  const [form, setForm] = useState<CreateBuildingRequest>(initialValues);
  const [countryId, setCountryId] = useState("");
  const [countyId, setCountyId] = useState("");
  const [error, setError] = useState("");
  const { data: countries = [], isLoading: isCountriesLoading, isError: isCountriesError } = useQuery({
    queryKey: ["countries"],
    queryFn: getCountries,
  });
  const { data: counties = [], isLoading: isCountiesLoading, isError: isCountiesError } = useQuery({
    queryKey: ["counties", countryId],
    queryFn: () => getCounties(countryId),
    enabled: !!countryId,
  });
  const { data: cities = [], isLoading: isCitiesLoading, isError: isCitiesError } = useQuery({
    queryKey: ["cities", countyId],
    queryFn: () => getCities(countyId),
    enabled: !!countyId,
  });

  const handleChange =
    (field: "constructionYear" | "buildingType" | "numberOfFloors" | "surfaceArea" | "insuredValue" | "riskIndicators") =>
    (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
      const rawValue = e.target.value;
      const value = isNumericField(field) ? Number(rawValue) || 0 : rawValue;
      setForm((prev) => ({ ...prev, [field]: value }));
    };

  const handleAddressChange =
    (field: "street" | "number" | "cityId") =>
    (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
      const value = e.target.value;
      setForm((prev) => ({ ...prev, address: { ...prev.address, [field]: value } }));
    };

  const handleCountryChange = (e: ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setCountryId(value);
    setCountyId("");
    setForm((prev) => ({ ...prev, address: { ...prev.address, cityId: "" } }));
  };

  const handleCountyChange = (e: ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setCountyId(value);
    setForm((prev) => ({ ...prev, address: { ...prev.address, cityId: "" } }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");

    try {
      await onSubmit(form);
    } catch {
      setError("Failed to create building. Please try again.");
    }
  };

  return (
    <form onSubmit={handleSubmit} className="form-card">
      <div className="form-grid">
        <div className="form-field">
          <label className="form-label">Street</label>
          <input
            className="input-control"
            value={form.address.street}
            onChange={handleAddressChange("street")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Number</label>
          <input
            className="input-control"
            value={form.address.number}
            onChange={handleAddressChange("number")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Construction year</label>
          <input
            type="number"
            className="input-control"
            value={form.constructionYear}
            onChange={handleChange("constructionYear")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Country</label>
          <select
            className="input-control"
            value={countryId}
            onChange={handleCountryChange}
            disabled={isCountriesLoading || isSubmitting}
          >
            <option value="">
              {isCountriesLoading ? "Loading countries..." : "Select a country"}
            </option>
            {countries.map((country) => (
              <option key={country.id} value={country.id}>
                {country.name}
              </option>
            ))}
          </select>
          {isCountriesError && <p className="error-text">Failed to load countries.</p>}
        </div>

        <div className="form-field">
          <label className="form-label">County</label>
          <select
            className="input-control"
            value={countyId}
            onChange={handleCountyChange}
            disabled={!countryId || isCountiesLoading || isSubmitting}
          >
            <option value="">
              {!countryId ? "Select a country first" : isCountiesLoading ? "Loading counties..." : "Select a county"}
            </option>
            {counties.map((county) => (
              <option key={county.id} value={county.id}>
                {county.name}
              </option>
            ))}
          </select>
          {isCountiesError && <p className="error-text">Failed to load counties.</p>}
        </div>

        <div className="form-field">
          <label className="form-label">City</label>
          <select
            className="input-control"
            value={form.address.cityId}
            onChange={handleAddressChange("cityId")}
            disabled={!countyId || isCitiesLoading || isSubmitting}
          >
            <option value="">
              {!countyId ? "Select a county first" : isCitiesLoading ? "Loading cities..." : "Select a city"}
            </option>
            {cities.map((city) => (
              <option key={city.id} value={city.id}>
                {city.name}
              </option>
            ))}
          </select>
          {isCitiesError && <p className="error-text">Failed to load cities.</p>}
        </div>

        <div className="form-field">
          <label className="form-label">Building type</label>
          <select
            className="input-control"
            value={form.buildingType}
            onChange={handleChange("buildingType")}
          >
            <option value={0}>Residential</option>
            <option value={1}>Commercial</option>
            <option value={2}>Industrial</option>
            <option value={3}>Mixed use</option>
          </select>
        </div>

        <div className="form-field">
          <label className="form-label">Number of floors</label>
          <input
            type="number"
            className="input-control"
            value={form.numberOfFloors}
            onChange={handleChange("numberOfFloors")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Surface area (sqm)</label>
          <input
            type="number"
            className="input-control"
            value={form.surfaceArea}
            onChange={handleChange("surfaceArea")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Insured value</label>
          <input
            type="number"
            className="input-control"
            value={form.insuredValue}
            onChange={handleChange("insuredValue")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Risk indicators</label>
          <textarea
            className="input-control"
            value={form.riskIndicators}
            onChange={handleChange("riskIndicators")}
          />
        </div>
      </div>

      {error && (
        <p className="error-text" style={{ marginTop: "0.8rem" }}>
          {error}
        </p>
      )}

      <button
        type="submit"
        disabled={isSubmitting}
        className="btn btn-primary"
        style={{ marginTop: "0.9rem" }}
      >
        {isSubmitting ? "Submitting..." : "Submit"}
      </button>
    </form>
  );
}