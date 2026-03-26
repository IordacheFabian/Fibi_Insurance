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
                <span className="user-chip">
                    {user?.email} ({user?.role})
                </span>
                <button
                    onClick={logout}
                    className="btn btn-ghost"
                >
                    Logout
                </button>
            </div>
        </header>
    )
}