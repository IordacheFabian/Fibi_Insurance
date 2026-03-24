import type { Client, CreateClientRequest } from "../features/clients/client.types";
import api from "./axios";


export const getClients = async (): Promise<Client[]> => {
    const response = await api.get("/brokers/clients");
    return response.data.items;
}

export const getClientById = async (id: string): Promise<Client> => {
    const response = await api.get(`/brokers/clients/${id}`);
    return response.data;
}

export const createClient = async (payload: CreateClientRequest): Promise<Client> => {
    const response = await api.post("/brokers/clients", payload);
    return response.data;
}