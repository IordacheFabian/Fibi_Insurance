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
    <div className="login-page">
      <div className="login-backdrop">
        <div className="login-panel">
          <p className="login-accent">Broker Platform</p>
          <h1 className="login-heading">Manage clients, buildings, and claims in one place.</h1>
          <p className="login-copy">
            Secure access for brokers and admins with an interface designed for
            speed and clarity.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          <div>
            <h2 className="page-title">Welcome back</h2>
            <p className="page-subtitle">Sign in to continue to FiBI Insurance.</p>
          </div>

          <div className="form-field">
            <label className="form-label">Email</label>
            <input
              type="email"
              className="input-control"
              value={form.email}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, email: e.target.value }))
              }
            />
          </div>

          <div className="form-field">
            <label className="form-label">Password</label>
            <input
              type="password"
              className="input-control"
              value={form.password}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, password: e.target.value }))
              }
            />
          </div>

          {error && <p className="error-text">{error}</p>}

          <button className="btn btn-primary" type="submit">
            Sign in
          </button>
        </form>
      </div>
    </div>
  );
}
