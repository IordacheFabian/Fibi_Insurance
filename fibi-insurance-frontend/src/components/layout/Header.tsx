import { useAuth } from "../../hooks/useAuth";

export default function Header() {
    const { user, logout } = useAuth();

    return (
      <header className="app-header">
        <div className="brand">
          <span className="brand-mark">FI</span>
          <div>
            <p className="brand-title">FiBI Insurance</p>
            <p className="brand-subtitle">Risk and claims intelligence</p>
          </div>
        </div>

        <div className="header-actions">
          <span className="brand-mark">
            {user?.email.slice(0, 2).toUpperCase()}
          </span>
          <div>
            <p className="brand-subtitle">{user?.email.split("@")[0]}</p>
          </div>
          <button onClick={logout} className="btn btn-ghost">
            Logout
          </button>
        </div>
      </header>
    );
}