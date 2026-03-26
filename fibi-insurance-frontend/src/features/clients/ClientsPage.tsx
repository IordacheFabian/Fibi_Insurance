import { useClients } from "./hooks/useClients";
import { Link } from "react-router-dom";

export default function ClientsPage() {
  const { data: clients, isLoading, isError, error} = useClients();

  if(isLoading) {
    return <div className="state-box loading">Loading clients...</div>
  }

  if(isError) {
    return <div className="state-box error">Error loading clients: {error instanceof Error ? error.message : "Unknown error"}</div>
  }

  if(!clients || clients.length === 0) {
    return (
      <section className="page-surface">
        <div className="page-header">
          <div>
            <h2 className="page-title">Clients</h2>
            <p className="page-subtitle">No clients found.</p>
          </div>
          <Link to="/broker/clients/new" className="btn btn-accent">
            Add Client
          </Link>
        </div>
        <div className="state-box empty">Add your first client to get started.</div>
      </section>
    )
  }


  return (
    <section className="page-surface">
      <div className="page-header">
        <div>
          <h2 className="page-title">Clients</h2>
          <p className="page-subtitle">Manage customer records and drill into details.</p>
        </div>
        <Link
          to="/broker/clients/new"
          className="btn btn-accent"
        >
          Add Client
        </Link>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Phone</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {clients.map((client) => (
              <tr key={client.id}>
                <td>{client.name}</td>
                <td>{client.email}</td>
                <td>{client.phoneNumber}</td>
                <td>
                  <Link
                    to={`/broker/clients/${client.id}`}
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
