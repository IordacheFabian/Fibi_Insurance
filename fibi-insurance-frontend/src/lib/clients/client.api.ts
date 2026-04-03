import type { ClientDetails, ClientListItem, ClientTypeValue, CreateClient, CreateClientApiRequest, PagedResult, UpdateClientApiRequest } from "./client.types";
import { apiClient, getApiErrorMessage } from "../axios";

const DEFAULT_PAGE_SIZE = 100;

type ClientDetailsApiResponse = {
    id: string;
    type: ClientTypeValue;
    name: string;
    identificationNumber: string;
    email: string;
    phoneNumber: string;
    buildings: Array<{
        id: string;
        address: string;
        cityName: string;
    }>;
};

async function getClientsPage(pageNumber: number, pageSize: number): Promise<PagedResult<ClientListItem>> {
    try {
        const { data } = await apiClient.get<PagedResult<ClientListItem>>("/api/brokers/clients", {
            params: {
                pageNumber,
                pageSize,
            },
        });

        return data;
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to fetch clients"));
    }
}

export async function getClients(pageSize = DEFAULT_PAGE_SIZE): Promise<ClientListItem[]> {
    const clients: ClientListItem[] = [];
    let pageNumber = 1;
    let hasNextPage = true;

    while (hasNextPage) {
        const page = await getClientsPage(pageNumber, pageSize);
        clients.push(...page.items);
        hasNextPage = page.hasNextPage;
        pageNumber += 1;
    }

    return clients;
}

function mapClientTypeToApiValue(clientType: CreateClient["clientType"]): 0 | 1 {
    return clientType === "company" ? 1 : 0;
}

export async function createClient(clientData: CreateClient) {
    const payload: CreateClientApiRequest = {
        clientType: mapClientTypeToApiValue(clientData.clientType),
        name: clientData.name,
        identificationNumber: clientData.identification,
        email: clientData.email,
        phoneNumber: clientData.phoneNumber,
    };

    try {
        const { data } = await apiClient.post<string>("/api/brokers/clients", payload);
        return data;
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to create client"));
    }
}

export async function getClientById(clientId: string): Promise<ClientDetails> {
    try {
        const { data: client } = await apiClient.get<ClientDetailsApiResponse>(`/api/brokers/clients/${clientId}`);

        return {
            id: client.id,
            name: client.name,
            clientType: client.type,
            identificationNumber: client.identificationNumber,
            email: client.email,
            phoneNumber: client.phoneNumber,
            buildings: client.buildings ?? [],
        };
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to fetch client"));
    }
}

export async function updateClient(clientId: string, clientData: UpdateClientApiRequest): Promise<void> {
    try {
        await apiClient.put(`/api/brokers/clients/${clientId}`, clientData);
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to update client"));
    }
}