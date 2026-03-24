import { Link, useParams } from "react-router-dom";
import { useClientDetails } from "./hooks/useClientDetails";
import { clientTypeLabels } from "../../util/ClientTypeLabels";
import { buildingTypeLabels } from "../../util/BuildingTypeLabels";

export default function ClientDetailsPage() {
    const { id = "" } = useParams();
    const { data: client, isLoading, isError, error } = useClientDetails(id);

    if(isLoading) {
        return <div>Loading client details...</div>
    }

    if(isError) {
        return <div>Error loading client details: {error instanceof Error ? error.message : "Unknown error"}</div>
    }

    if(!client) {
        return <div>Client not found.</div>
    }

    return (
      <div className="space-y-6">
        <div>
          <h2 className="text-2xl font-bold">{client.name}</h2>
          <p className="text-gray-600">Client details</p>
        </div>
        <div>
          <p className="text-sm text-gray-500">Client Type</p>
          <p>{clientTypeLabels[Number(client.type)] ?? "Unknown"}</p>
        </div>
        <div className="grid grid-cols-1 gap-4 rounded-xl bg-white p-6 shadow md:grid-cols-2">
          <div>
            <p className="text-sm text-gray-500">Email</p>
            <p>{client.email}</p>
          </div>
        </div>

        <div>
          <p className="text-sm text-gray-500">Phone number</p>
          <p>{client.phoneNumber}</p>
        </div>

        <div>
          <p className="text-sm text-gray-500">Identification number</p>
          <p>{client.identificationNumber}</p>
        </div>

        <div>
          <p className="text-sm text-gray-500">Buildings</p>
          {client.buildings.length > 0 ? (
            <ul className="list-disc pl-5">
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
            className="text-sm font-medium text-blue-600 hover:underline"
          >
            View Details
          </Link>
        </div>
      </div>
    );
}