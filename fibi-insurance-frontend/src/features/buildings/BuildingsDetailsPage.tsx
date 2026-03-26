import { useParams } from "react-router-dom";
import { buildingTypeLabels } from "../../util/BuildingTypeLabels";
import { useBuildingDetails } from "./hooks/useBuildingDetails";

export default function BuildingPage() {
  const { id = "" } = useParams();
  const { data: building, isLoading, isError, error } = useBuildingDetails(id);

  if (isLoading) {
    return <div className="state-box loading">Loading building details...</div>;
  }

  if (isError) {
    return (
      <div className="state-box error">
        Error loading building details:{" "}
        {error instanceof Error ? error.message : "Unknown error"}
      </div>
    );
  }
  if (!building) {
    return (
      <div className="state-box empty">
        <p>No buildings found.</p>
      </div>
    );
  }

  return (
    <section className="page-surface">
      <div className="page-header">
        <div>
          <h2 className="page-title">Building details</h2>
          <p className="page-subtitle">Full risk and structure overview.</p>
        </div>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>Building Type</th>
              <th>Insured Value</th>
              <th>Construction Year</th>
              <th>Number of Floors</th>
              <th>Risk Indicators</th>
              <th>Surface Area</th>
            </tr>
          </thead>
          <tbody>
              <tr key={building.id}>
                <td>
                  {buildingTypeLabels[building.buildingType] ?? "Unknown"}
                </td>
                <td>{building.insuredValue}</td>
                <td>{building.constructionYear}</td>
                <td>{building.numberOfFloors}</td>
                <td>{building.riskIndicators}</td>
                <td>{building.surfaceArea}</td>
              </tr>
          </tbody>
        </table>
      </div>
    </section>
  );
}
