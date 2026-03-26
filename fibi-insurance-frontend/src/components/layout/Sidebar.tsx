import { NavLink } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

export default function Sidebar() {
    const { user } = useAuth();

    const navClassName = ({ isActive }: { isActive: boolean }) =>
      `nav-link${isActive ? " is-active" : ""}`;

    return (
      <aside className="app-sidebar">
        <div>
          <div className="sidebar-title">Control Center</div>
          <p className="sidebar-subtitle">FiBI Insurance Workspace</p>
        </div>

        <nav className="nav-section">
          <NavLink to="/" className={navClassName}>
            <span className="nav-icon">DB</span>
            Dashboard
          </NavLink>

          {user?.role === "Broker" && (
            <>
              <NavLink
                to="/broker/clients"
                className={navClassName}
              >
                <span className="nav-icon">CL</span>
                Clients
              </NavLink>
              <NavLink
                to="/broker/policies"
                className={navClassName}
              >
                <span className="nav-icon">PO</span>
                Policies
              </NavLink>
              <NavLink
                to="/admin/claims"
                className={navClassName}
              >
                <span className="nav-icon">CM</span>
                Claims
              </NavLink>
            </>
          )}

          {user?.role === "Admin" && (
            <>
              <NavLink
                to="/admin/brokers"
                className={navClassName}
              >
                <span className="nav-icon">BR</span>
                Brokers
              </NavLink>
              <NavLink
                to="/admin/reports/country"
                className={navClassName}
              >
                <span className="nav-icon">RP</span>
                Reports
              </NavLink>
            </>
          )}
        </nav>
      </aside>
    );
}