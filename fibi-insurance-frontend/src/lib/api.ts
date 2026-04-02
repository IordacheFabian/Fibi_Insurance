import type { ClientDetails, ClientListItem, ClientTypeValue, CreateClient, CreateClientApiRequest, PagedResult, UpdateClientApiRequest } from "./types";
import { authFetch, resolveApiUrl } from "./auth.api";

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
    const searchParams = new URLSearchParams({
        pageNumber: String(pageNumber),
        pageSize: String(pageSize),
    });

    const response = await authFetch(resolveApiUrl(`/api/brokers/clients?${searchParams.toString()}`));

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Failed to fetch clients");
    }

    return response.json() as Promise<PagedResult<ClientListItem>>;
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

    const response = await authFetch(resolveApiUrl("/api/brokers/clients"), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Failed to create client");
    }

    return response.json() as Promise<string>;
}

export async function getClientById(clientId: string): Promise<ClientDetails> {
    const response = await authFetch(resolveApiUrl(`/api/brokers/clients/${clientId}`)); 
    
    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Failed to fetch client");
    }

    const client = await response.json() as ClientDetailsApiResponse;

    return {
        id: client.id,
        name: client.name,
        clientType: client.type,
        identificationNumber: client.identificationNumber,
        email: client.email,
        phoneNumber: client.phoneNumber,
        buildings: client.buildings ?? [],
    };
}

export async function updateClient(clientId: string, clientData: UpdateClientApiRequest): Promise<void> {
    const response = await authFetch(resolveApiUrl(`/api/brokers/clients/${clientId}`), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(clientData),
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Failed to update client");
    }
}