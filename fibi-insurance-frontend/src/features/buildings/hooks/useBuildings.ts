import { useQuery } from "@tanstack/react-query";
import { getBuildings } from "../../../api/buildings.api";

export function useBuildings(id: string) {
    return useQuery({
        queryKey: ["buildings", id],
        queryFn: () => getBuildings(id),
        enabled: !!id,
    })
}