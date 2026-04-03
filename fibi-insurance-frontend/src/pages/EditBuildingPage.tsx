import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import BuildingForm, { BuildingFormValues, mapBuildingTypeFromApiValue, mapFormValuesToUpdatePayload } from "./BuildingForm";
import { getBuildingById, updateBuilding } from "@/lib/buildings/building.api";

export default function EditBuildingPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: building, isLoading, isError, error } = useQuery({
    queryKey: ["buildings", id],
    queryFn: () => getBuildingById(id!),
    enabled: Boolean(id),
    staleTime: 30000,
  });

  const updateBuildingMutation = useMutation({
    mutationFn: async (values: BuildingFormValues) => {
      if (!id) {
        throw new Error("Building id is missing");
      }

      await updateBuilding(id, mapFormValuesToUpdatePayload(id, values));
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["buildings"] }),
        queryClient.invalidateQueries({ queryKey: ["buildings", id] }),
      ]);
      navigate(`/buildings/${id}`);
    },
  });

  if (isLoading) {
    return <div className="space-y-6"><p className="text-sm text-muted-foreground">Loading building details...</p></div>;
  }

  if (isError || !building) {
    return <div className="space-y-6"><p className="text-sm text-destructive">{error instanceof Error ? error.message : "Failed to load building"}</p></div>;
  }

  return (
    <BuildingForm
      mode="edit"
      initialValues={{
        clientId: building.owner.id,
        currencyId: building.currencyId,
        countryId: "",
        countyId: "",
        cityId: building.address.cityId,
        street: building.address.street,
        number: building.address.number,
        constructionYear: String(building.constructionYear),
        buildingType: mapBuildingTypeFromApiValue(building.buildingType),
        numberOfFloors: String(building.numberOfFloors),
        surfaceArea: String(building.surfaceArea),
        insuredValue: String(building.insuredValue),
        riskIndicators: building.riskIndicators,
      }}
      isSubmitting={updateBuildingMutation.isPending}
      cancelPath={`/buildings/${id}`}
      onSubmit={async (values) => {
        await updateBuildingMutation.mutateAsync(values);
      }}
    />
  );
}