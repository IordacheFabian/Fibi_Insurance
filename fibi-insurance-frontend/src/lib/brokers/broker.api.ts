import { apiClient, getApiErrorMessage } from "@/lib/axios";
import type {
	BrokerDetails,
	BrokerListItem,
	BrokerPageResponse,
	BrokerStatusValue,
	CreateBrokerInput,
	NormalizedBrokerStatus,
	UpdateBrokerInput,
} from "./broker.types";

const DEFAULT_PAGE_SIZE = 100;

export function normalizeBrokerStatus(status: BrokerStatusValue): NormalizedBrokerStatus {
	if (typeof status === "number") {
		return status === 1 ? "inactive" : "active";
	}

	if (/^\d+$/.test(status)) {
		return Number(status) === 1 ? "inactive" : "active";
	}

	return status.toLowerCase() === "inactive" ? "inactive" : "active";
}

function mapBrokerDetails(details: BrokerDetails): BrokerListItem {
	return {
		id: details.id,
		brokerCode: details.brokerCode,
		name: details.name,
		email: details.email,
		phoneNumber: details.phoneNumber,
		status: normalizeBrokerStatus(details.brokerStatus),
		commissionPercentage: details.commissionPercentage,
	};
}

async function getBrokersPage(pageNumber: number, pageSize: number): Promise<BrokerPageResponse> {
	try {
		const { data } = await apiClient.get<BrokerPageResponse>("/api/admin/brokers", {
			params: {
				pageNumber,
				pageSize,
			},
		});

		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch brokers"));
	}
}

export async function getBrokerById(brokerId: string): Promise<BrokerListItem> {
	try {
		const { data } = await apiClient.get<BrokerDetails>(`/api/admin/brokers/${brokerId}`);
		return mapBrokerDetails(data);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch broker"));
	}
}

export async function getBrokers(pageSize = DEFAULT_PAGE_SIZE): Promise<BrokerListItem[]> {
	const brokerIds: string[] = [];
	let pageNumber = 1;
	let hasNextPage = true;

	while (hasNextPage) {
		const page = await getBrokersPage(pageNumber, pageSize);
		brokerIds.push(...page.items.map((broker) => broker.id));
		hasNextPage = page.hasNextPage;
		pageNumber += 1;
	}

	const details = await Promise.all(brokerIds.map((brokerId) => getBrokerById(brokerId)));

	return details.sort((left, right) => left.name.localeCompare(right.name));
}

export async function createBroker(payload: CreateBrokerInput): Promise<BrokerListItem> {
	try {
		const { data } = await apiClient.post<BrokerDetails>("/api/admin/brokers/registration", {
			brokerCode: payload.brokerCode.trim(),
			name: payload.name.trim(),
			email: payload.email.trim(),
			phoneNumber: payload.phoneNumber.trim(),
			brokerStatus: 0,
			commissionPercentage: payload.commissionPercentage ?? null,
			password: payload.password,
		});

		return mapBrokerDetails(data);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to create broker account"));
	}
}

export async function updateBroker(brokerId: string, payload: UpdateBrokerInput): Promise<void> {
	try {
		await apiClient.put(`/api/admin/brokers/${brokerId}`, {
			name: payload.name.trim(),
			email: payload.email.trim(),
			phoneNumber: payload.phoneNumber?.trim() || null,
			commissionPercentage: payload.commissionPercentage ?? null,
		});
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to update broker"));
	}
}

export async function activateBroker(brokerId: string): Promise<void> {
	try {
		await apiClient.post(`/api/admin/brokers/${brokerId}/activate`);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to activate broker"));
	}
}

export async function deactivateBroker(brokerId: string): Promise<void> {
	try {
		await apiClient.post(`/api/admin/brokers/${brokerId}/deactivate`);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to deactivate broker"));
	}
}
