import { useQuery } from "@tanstack/react-query";
import { getClientById } from "../../../api/clients.api";

export function useClientDetails(id: string) {
    return useQuery({
        queryKey: ["client", id],
        queryFn: () => getClientById(id),
        enabled: !!id,
    })
}