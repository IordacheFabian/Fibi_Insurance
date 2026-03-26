export default function DashboardPage() {
    return (
      <section className="page-surface">
        <div className="page-header">
          <div>
            <h2 className="page-title">Dashboard</h2>
            <p className="page-subtitle">
              Welcome to your insurance operations overview.
            </p>
          </div>
        </div>

        <div className="metric-grid">
          <article className="metric-card">
            <p className="metric-label">Active Clients</p>
            <p className="metric-value">248</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Open Claims</p>
            <p className="metric-value">31</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Buildings Covered</p>
            <p className="metric-value">612</p>
          </article>
        </div>

        <div className="panel" style={{ marginTop: "1rem" }}>
          <h3 className="page-title" style={{ fontSize: "1.1rem" }}>
            Quick Start
          </h3>
          <p className="page-subtitle" style={{ marginTop: "0.35rem" }}>
            Use the navigation menu to manage clients, inspect risk details, and
            process claims efficiently.
          </p>
        </div>
      </section>
    );
}