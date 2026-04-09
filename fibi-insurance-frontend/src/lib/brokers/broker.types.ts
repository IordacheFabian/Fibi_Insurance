import type { PagedResult } from "@/lib/clients/client.types";

export type { PagedResult } from "@/lib/clients/client.types";

export type BrokerStatusValue = string | number;

export type NormalizedBrokerStatus = "active" | "inactive";

export interface BrokerSummary {
	id: string;
	brokerCode: string;
	name: string;
	brokerStatus: BrokerStatusValue;
}

export interface BrokerDetails {
	id: string;
	brokerCode: string;
	name: string;
	email: string;
	phoneNumber?: string | null;
	brokerStatus: BrokerStatusValue;
	commissionPercentage?: number | null;
}

export interface BrokerListItem {
	id: string;
	brokerCode: string;
	name: string;
	email: string;
	phoneNumber?: string | null;
	status: NormalizedBrokerStatus;
	commissionPercentage?: number | null;
}

export interface CreateBrokerInput {
	brokerCode: string;
	name: string;
	email: string;
	phoneNumber: string;
	commissionPercentage?: number | null;
	password: string;
}

export interface UpdateBrokerInput {
	name: string;
	email: string;
	phoneNumber?: string;
	commissionPercentage?: number | null;
}

export type BrokerPageResponse = PagedResult<BrokerSummary>;
