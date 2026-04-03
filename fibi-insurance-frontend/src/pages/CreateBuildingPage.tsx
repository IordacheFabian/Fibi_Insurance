import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import BuildingForm, { mapFormValuesToCreatePayload } from "./BuildingForm";
import { createBuilding } from "@/lib/buildings/building.api";

export default function CreateBuildingPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const createBuildingMutation = useMutation({
    mutationFn: async (payload: Parameters<typeof mapFormValuesToCreatePayload>[0]) => createBuilding(mapFormValuesToCreatePayload(payload)),
    onSuccess: async (buildingId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["buildings"] }),
        queryClient.invalidateQueries({ queryKey: ["clients"] }),
      ]);
      navigate(`/buildings/${buildingId}`);
    },
  });

  return (
    <BuildingForm
      mode="create"
      isSubmitting={createBuildingMutation.isPending}
      cancelPath="/buildings"
      onSubmit={async (values) => {
        await createBuildingMutation.mutateAsync(values);
      }}
    />
  );
}