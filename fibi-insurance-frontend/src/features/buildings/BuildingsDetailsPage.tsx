import { useParams } from "react-router-dom";
import { buildingTypeLabels } from "../../util/BuildingTypeLabels";
import { useBuildingDetails } from "./hooks/useBuildingDetails";

export default function BuildingPage() {
  const { id = "" } = useParams();
  const { data: building, isLoading, isError, error } = useBuildingDetails(id);

  if (isLoading) {
    return <div>Loading building details...</div>;
  }

  if (isError) {
    return (
      <div>
        Error loading building details:{" "}
        {error instanceof Error ? error.message : "Unknown error"}
      </div>
    );
  }
  if (!building) {
    return (
      <div>
        <p className="mt-2 text-gray-600">No buildings found.</p>
      </div>
    );
  }

  return (
    <div>
      <div>
        <h2 className="text-2xl font-bold">Building Details</h2>
      </div>

      <div className="overflow-hidden rounded-xl border bg-white">
        <table className="min-w-full border-collapse">
          <thead>
            <tr className="border-b bg-gray-50 text-left">
              <th className="px-4 py-3">Building Type</th>
              <th className="px-4 py-3">Insured Value</th>
              <th className="px-4 py-3">Construction Year</th>
              <th className="px-4 py-3">Number of Floors</th>
              <th className="px-4 py-3">Risk Indicators</th>
              <th className="px-4 py-3">Surface Area</th>
            </tr>
          </thead>
          <tbody>
              <tr key={building.id} className="border-b last:border-b-0">
                <td className="px-4 py-3">
                  {buildingTypeLabels[building.buildingType] ?? "Unknown"}
                </td>
                <td className="px-4 py-3">{building.insuredValue}</td>
                <td className="px-4 py-3">{building.constructionYear}</td>
                <td className="px-4 py-3">{building.numberOfFloors}</td>
                <td className="px-4 py-3">{building.riskIndicators}</td>
                <td className="px-4 py-3">{building.surfaceArea}</td>
              </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}
