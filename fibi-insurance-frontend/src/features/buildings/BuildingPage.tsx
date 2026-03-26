import { Link, useParams } from "react-router-dom";
import { useBuildings } from "./hooks/useBuildings";
import { buildingTypeLabels } from "../../util/BuildingTypeLabels";

export default function BuildingPage() {
    const { id = "" } = useParams();
    const {data: buildings, isLoading, isError, error } = useBuildings(id);

    if(isLoading) {
        return <div className="state-box loading">Loading building details...</div>
    }

    if(isError) {
        return <div className="state-box error">Error loading building details: {error instanceof Error ? error.message : "Unknown error"}</div>
    }
    if(!buildings || buildings.length === 0) {
        return (
          <section className="page-surface">
            <div className="page-header">
              <div>
                <h2 className="page-title">Buildings</h2>
                <p className="page-subtitle">No buildings found.</p>
              </div>
            </div>
            <div className="state-box empty">No insured building records for this client yet.</div>
          </section>
        );
    }

    return (
      <section className="page-surface">
        <div className="page-header">
          <div>
            <h2 className="page-title">Buildings</h2>
            <p className="page-subtitle">Client building portfolio and insured values.</p>
          </div>
          <Link
            to="/broker/buildings/new"
            className="btn btn-accent"
          >
            Add Building
          </Link>
        </div>

        <div className="table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Building type</th>
                <th>Insured value</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {buildings.map((building) => (
                <tr key={building.id}>
                  <td>
                    {buildingTypeLabels[building.buildingType] ?? "Unknown"}
                  </td>
                  <td>{building.insuredValue}</td>
                  <td>
                    <Link
                      to={`/broker/buildings/${building.id}`}
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