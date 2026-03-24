import { Link, useParams } from "react-router-dom";
import { useBuildings } from "./hooks/useBuildings";
import { buildingTypeLabels } from "../../util/BuildingTypeLabels";

export default function BuildingPage() {
    const { id = "" } = useParams();
    const {data: buildings, isLoading, isError, error } = useBuildings(id);

    if(isLoading) {
        return <div>Loading building details...</div>
    }

    if(isError) {
        return <div>Error loading building details: {error instanceof Error ? error.message : "Unknown error"}</div>
    }
    if(!buildings || buildings.length === 0) {
        return (
          <div>
            <h2 className="text-2xl font-bold">Buildings</h2>
            <p className="mt-2 text-gray-600">No buildings found.</p>
          </div>
        );
    }

    return (
      <div>
        <div>
          <h2 className="text-2xl font-bold">Buildings</h2>
          <Link
            to="/broker/buildings/new"
            className="rounded-lg bg-black px-4 py-2 text-white"
          >
            Add Building
          </Link>
        </div>

        <div className="overflow-hidden rounded-xl border bg-white">
          <table className="min-w-full border-collapse">
            <thead>
              <tr className="border-b bg-gray-50 text-left">
                <th className="px-4 py-3">Building-Type</th>
                <th className="px-4 py-3">Insured Value</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              {buildings.map((building) => (
                <tr key={building.id} className="border-b last:border-b-0">
                  <td className="px-4 py-3">
                    {buildingTypeLabels[building.buildingType] ?? "Unknown"}
                  </td>
                  <td className="px-4 py-3">{building.insuredValue}</td>
                  <td className="px-4 py-3">
                    <Link
                      to={`/broker/buildings/${building.id}`}
                      className="text-sm font-medium text-blue-600 hover:underline"
                    >
                      View Details
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    );
}