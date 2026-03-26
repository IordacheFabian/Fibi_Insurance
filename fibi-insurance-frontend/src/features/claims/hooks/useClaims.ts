import { useQuery } from "@tanstack/react-query";
import { getClaims } from "../../../api/claims.api";

export function useClaims() {
    return useQuery({
        queryKey: ["claims"],
        queryFn: getClaims,
    })
}