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
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-xl bg-white p-6 shadow"
    >
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div>
          <label className="mb-1 block text-sm font-medium">Name</label>
          <input
            className="w-full rounded-lg border px-3 py-2"
            value={form.name}
            onChange={handleChange("name")}
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium">Email</label>
          <input
            type="email"
            className="w-full rounded-lg border px-3 py-2"
            value={form.email}
            onChange={handleChange("email")}
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium">Phone number</label>
          <input
            className="w-full rounded-lg border px-3 py-2"
            value={form.phoneNumber}
            onChange={handleChange("phoneNumber")}
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium">CNP</label>
          <input
            className="w-full rounded-lg border px-3 py-2"
            value={form.identificationNumber}
            onChange={handleChange("identificationNumber")}
          />
        </div>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-lg bg-black px-4 py-2 text-white disabled:opacity-50"
      >
        {isSubmitting ? "Submitting..." : "Submit"}
      </button>
    </form>
  );
}
