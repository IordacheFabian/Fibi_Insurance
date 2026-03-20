import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { useState } from "react";

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [form, setForm] = useState({
    email: "",
    password: "",
  });

  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    try {
      await login(form);
      navigate("/");
    } catch {
      setError("Invalid credentials.");
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 px-4">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-md rounded-2xl bg-white p-8 shadow"
      >
        <h1 className="mb-6 text-2xl font-bold">Login</h1>

        <div className="mb-4">
          <label className="mb-1 block text-sm font-medium">Email</label>
          <input
            type="email"
            className="w-full rounded-lg border px-3 py-2"
            value={form.email}
            onChange={(e) =>
              setForm((prev) => ({ ...prev, email: e.target.value }))
            }
          />
        </div>

        <div className="mb-4">
          <label className="mb-1 block text-sm font-medium">Password</label>
          <input
            type="password"
            className="w-full rounded-lg border px-3 py-2"
            value={form.password}
            onChange={(e) =>
              setForm((prev) => ({ ...prev, password: e.target.value }))
            }
          />
        </div>

        {error && <p className="mb-4 text-sm text-red-600">{error}</p>}

        <button className="w-full rounded-lg bg-black px-4 py-2 text-white">
          Sign in
        </button>
      </form>
    </div>
  );
}
