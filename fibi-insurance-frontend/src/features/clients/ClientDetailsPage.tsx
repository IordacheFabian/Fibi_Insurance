import { Link, useParams } from "react-router-dom";
import { useClientDetails } from "./hooks/useClientDetails";
import { clientTypeLabels } from "../../util/ClientTypeLabels";
import { buildingTypeLabels } from "../../util/BuildingTypeLabels";

export default function ClientDetailsPage() {
    const { id = "" } = useParams();
    const { data: client, isLoading, isError, error } = useClientDetails(id);

    if(isLoading) {
        return <div className="state-box loading">Loading client details...</div>
    }

    if(isError) {
        return <div className="state-box error">Error loading client details: {error instanceof Error ? error.message : "Unknown error"}</div>
    }

    if(!client) {
        return <div className="state-box empty">Client not found.</div>
    }

    return (
      <section className="page-surface">
        <div className="page-header">
          <div>
            <h2 className="page-title">{client.name}</h2>
            <p className="page-subtitle">Client details</p>
          </div>
        </div>

        <div className="detail-grid">
          <div className="detail-item">
            <p className="detail-label">Client Type</p>
            <p className="detail-value">{clientTypeLabels[Number(client.type)] ?? "Unknown"}</p>
          </div>
          <div className="detail-item">
            <p className="detail-label">Email</p>
            <p className="detail-value">{client.email}</p>
          </div>
          <div className="detail-item">
            <p className="detail-label">Phone number</p>
            <p className="detail-value">{client.phoneNumber}</p>
          </div>
          <div className="detail-item">
            <p className="detail-label">Identification number</p>
            <p className="detail-value">{client.identificationNumber}</p>
          </div>
        </div>

        <div className="panel" style={{ marginTop: "0.9rem" }}>
          <p className="detail-label">Buildings</p>
          {client.buildings.length > 0 ? (
            <ul className="list-plain">
              {client.buildings.map((building) => (
                <li key={building.clientId}>
                  {buildingTypeLabels[building.buildingType] ?? "Unknown"} -
                  Insured Value: {building.insuredValue}
                </li>
              ))}
            </ul>
          ) : (
            <p>No buildings found.</p>
          )}
          <Link
            to={`/broker/clients/${client.id}/buildings`}
            className="inline-link"
          >
            View Details
          </Link>
        </div>
      </section>
    );
}