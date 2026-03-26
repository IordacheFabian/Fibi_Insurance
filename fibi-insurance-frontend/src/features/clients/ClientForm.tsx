import { useState, type ChangeEvent, type FormEvent } from "react";
import type { CreateClientRequest } from "./client.types";

type Props = {
  onSubmit: (values: CreateClientRequest) => Promise<void>;
  isSubmitting?: boolean;
};

const initialValues: CreateClientRequest = {
  name: "",
  type: "",
  identificationNumber: "",
  email: "",
  phoneNumber: "",
};

export default function ClientForm({ onSubmit, isSubmitting = false }: Props) {
  const [form, setForm] = useState<CreateClientRequest>(initialValues);
  const [error, setError] = useState("");

  const handleChange =
    (field: keyof CreateClientRequest) =>
    (e: ChangeEvent<HTMLInputElement>) => {
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
    };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");

    try {
      await onSubmit(form);
    } catch {
      setError("Failed to create client. Please try again.");
    }
  };

  return (
    <form onSubmit={handleSubmit} className="form-card">
      <div className="form-grid">
        <div className="form-field">
          <label className="form-label">Name</label>
          <input
            className="input-control"
            value={form.name}
            onChange={handleChange("name")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Email</label>
          <input
            type="email"
            className="input-control"
            value={form.email}
            onChange={handleChange("email")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">Phone number</label>
          <input
            className="input-control"
            value={form.phoneNumber}
            onChange={handleChange("phoneNumber")}
          />
        </div>

        <div className="form-field">
          <label className="form-label">CNP</label>
          <input
            className="input-control"
            value={form.identificationNumber}
            onChange={handleChange("identificationNumber")}
          />
        </div>
      </div>

      {error && <p className="error-text" style={{ marginTop: "0.8rem" }}>{error}</p>}

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
