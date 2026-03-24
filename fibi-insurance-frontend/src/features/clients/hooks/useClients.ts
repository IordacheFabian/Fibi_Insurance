import { useQuery } from "@tanstack/react-query";
import { getClients } from "../../../api/clients.api";

export function useClients() {
    return useQuery({
        queryKey: ["clients"],
        queryFn: getClients,
    })
}