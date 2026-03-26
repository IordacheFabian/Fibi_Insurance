import { Link } from "react-router-dom";
import { useClaims } from "./hooks/useClaims";

export default function ClaimsPage() {
  const { data: claims, isLoading, isError, error } = useClaims();

  if (isLoading) {
    return <div className="state-box loading">Loading claims...</div>;
  }

  if (isError) {
    return (
      <div className="state-box error">
        Error loading claims:{" "}
        {error instanceof Error ? error.message : "Unknown error"}
      </div>
    );
  }

  if (!claims || claims.length === 0) {
    return (
      <section className="page-surface">
        <div className="page-header">
          <div>
            <h2 className="page-title">Claims</h2>
            <p className="page-subtitle">No claims found.</p>
          </div>
        </div>
        <div className="state-box empty">No claim records are available yet.</div>
      </section>
    );
  }

  return (
    <section className="page-surface">
      <div className="page-header">
        <div>
          <h2 className="page-title">Claims</h2>
          <p className="page-subtitle">Review and track current insurance claims.</p>
        </div>
        <Link
          to="/broker/claims/new"
          className="btn btn-accent"
        >
          Add Claim
        </Link>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>Policy Number</th>
              <th>Client Name</th>
              <th>Status</th>
              <th>Estimated Damage</th>
              <th>Created At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {claims.map((claim) => (
              <tr key={claim.id}>
                <td>{claim.policyNumber}</td>
                <td>{claim.clientName}</td>
                <td>{claim.status}</td>
                <td>{claim.estimatedDamage}</td>
                <td>{new Date(claim.createdAt).toLocaleDateString()}</td>
                <td>
                  <Link
                    to={`/broker/claims/${claim.id}`}
                    className="inline-link"
                  >
                    View Details
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
