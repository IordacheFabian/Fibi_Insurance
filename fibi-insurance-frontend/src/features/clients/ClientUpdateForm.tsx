import { useState, type ChangeEvent, type FormEvent } from "react";
import { isAxiosError } from "axios";
import type {  UpdateClientRequest } from "./client.types";

type Props = {
  onSubmit: (values: UpdateClientRequest) => Promise<void>;
  initialValues?: UpdateClientRequest;
  isSubmitting?: boolean;
};

const emptyValues: UpdateClientRequest = {
  name: "",
  email: "",
  phoneNumber: "",
};

export default function UpdateClientForm({ onSubmit, initialValues, isSubmitting = false }: Props) {
  const [form, setForm] = useState<UpdateClientRequest>(initialValues ?? emptyValues);
  const [error, setError] = useState("");

  const handleChange =
    (field: keyof UpdateClientRequest) =>
    (e: ChangeEvent<HTMLInputElement>) => {
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
    };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");

    try {
      await onSubmit(form);
    } catch (err) {
      if (isAxiosError(err)) {
        const apiMessage =
          (err.response?.data as { message?: string; title?: string; error?: string } | undefined)?.message ??
          (err.response?.data as { message?: string; title?: string; error?: string } | undefined)?.title ??
          (err.response?.data as { message?: string; title?: string; error?: string } | undefined)?.error;

        setError(apiMessage ?? "Failed to update client. Please try again.");
        return;
      }

      setError("Failed to update client. Please try again.");
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
