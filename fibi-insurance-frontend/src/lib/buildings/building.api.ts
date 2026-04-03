import { apiClient, getApiErrorMessage } from "../axios";
import { PagedResult } from "../clients/client.types";
import { getClients } from "../clients/client.api";
import { BrokerBuildingListItem, BuildingDetailsDto, BuildingListItem, CreateBuildingDto, UpdateBuildingDto } from "./building.type";

const DEFAULT_PAGE_SIZE = 100;

async function getBuildingsPage(clientId: string, pageNumber: number, pageSize: number): Promise<PagedResult<BuildingListItem>> {
    try {
        const { data } = await apiClient.get<PagedResult<BuildingListItem>>(`/api/brokers/clients/${clientId}/buildings`, {
            params: {
                pageNumber,
                pageSize,
            },
        });
        return data;
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to fetch buildings"));
    }
}

export async function getBuildingById(buildingId: string): Promise<BuildingDetailsDto> {
    try {
        const { data } = await apiClient.get<BuildingDetailsDto>(`/api/brokers/buildings/${buildingId}`);
        return data;
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to fetch building"));
    }
}

export async function createBuilding(buildingData: CreateBuildingDto): Promise<string> {
    try {
        const { data } = await apiClient.post<string>(`/api/brokers/clients/${buildingData.clientId}/buildings`, buildingData);
        return data;
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to create building"));
    }
}

export async function updateBuilding(buildingId: string, buildingData: UpdateBuildingDto): Promise<void> {
    try {
        await apiClient.put(`/api/brokers/buildings/${buildingId}`, buildingData);
    } catch (error) {
        throw new Error(getApiErrorMessage(error, "Failed to update building"));
    }
}

export async function getBuildings(pageSize = DEFAULT_PAGE_SIZE): Promise<BrokerBuildingListItem[]> {
    const clients = await getClients();

    const buildingSummaries = clients.flatMap((client) =>
        client.buildings.map((building) => ({
            clientId: client.id,
            clientName: client.name,
            buildingId: building.id,
        })),
    );

    const uniqueBuildingRefs = Array.from(
        new Map(buildingSummaries.map((item) => [item.buildingId, item])).values(),
    );

    const buildingDetails = await Promise.all(
        uniqueBuildingRefs.map(async (item) => {
            const detail = await getBuildingById(item.buildingId);

            return {
                id: detail.id,
                address: `${detail.address.street} ${detail.address.number}`.trim(),
                cityName: detail.address.cityName,
                buildingType: detail.buildingType,
                insuredValue: detail.insuredValue,
                currencyId: detail.currencyId,
                currencyCode: detail.currencyCode,
                currencyName: detail.currencyName,
                constructionYear: detail.constructionYear,
                ownerId: detail.owner.id,
                ownerName: detail.owner.name,
            } satisfies BrokerBuildingListItem;
        }),
    );

    return buildingDetails.sort((left, right) => left.cityName.localeCompare(right.cityName) || left.address.localeCompare(right.address));
}