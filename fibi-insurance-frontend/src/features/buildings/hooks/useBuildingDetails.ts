import { useQuery } from "@tanstack/react-query";
import { getBuildingDetails } from "../../../api/buildings.api";

export function useBuildingDetails(id: string) {
    return useQuery({
        queryKey: ["building", id],
        queryFn: () => getBuildingDetails(id),
        enabled: !!id,
    })
}