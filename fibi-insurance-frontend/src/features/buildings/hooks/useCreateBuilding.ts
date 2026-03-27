import { useMutation, useQueryClient } from "@tanstack/react-query"
import { createBuilding } from "../../../api/buildings.api";
import type { CreateBuildingRequest } from "../building.type";

export const useCreateBuilding = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ clientId, payload }: { clientId: string; payload: CreateBuildingRequest }) => createBuilding(clientId, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["buildings"] });
        }
    })
}