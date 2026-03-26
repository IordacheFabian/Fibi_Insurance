import type { Claim } from "../features/claims/claim.type";
import api from "./axios";

export const getClaims = async (): Promise<Claim[]> => {
    const response = await api.get("/admin/claims");
    return response.data.items;
}