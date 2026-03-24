import type { Building } from "../features/buildings/building.type";
import api from "./axios";

export const getBuildings = async (id: string): Promise<Building[]> => {
    const response = await api.get(`/brokers/clients/${id}/buildings`);
    return response.data.items;
}

export const getBuildingDetails = async (id: string): Promise<Building> => {
    const response = await api.get(`/brokers/buildings/${id}`);
    return response.data;
}