import type { AddressDetailsDto, BuildingTypeValue } from "@/lib/buildings/building.type";
import type { ClientTypeValue, PagedResult } from "@/lib/clients/client.types";

export type { PagedResult } from "@/lib/clients/client.types";

export type PolicyStatusValue = number | "Draft" | "Active" | "Expired" | "Cancelled" | "draft" | "active" | "expired" | "cancelled" | `${number}`;

export type EndorsementTypeValue = number | "InsuredValueChange" | "PeriosExtension" | "RiskUpdate" | "ManualAdjustement" | `${number}`;

export type AdjustmentTypeValue = number | string;

export type PolicyStatusFilter = "draft" | "active" | "expired" | "cancelled";

export interface PolicyListItem {
	id: string;
	policyNumber: string;
	policyStatus: PolicyStatusValue;
	clientId: string;
	clientName: string;
	buildingId: string;
	buildingStreet: string;
	buildingNumber: string;
	cityName: string;
	currencyId: string;
	currencyCode: string;
	startDate: string;
	endDate: string;
	basePremium: number;
	finalPremium: number;
}

export interface PolicyClientDetails {
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
}

export interface PolicyBuildingDetails {
	id: string;
	address: AddressDetailsDto;
	owner: PolicyClientDetails;
	currencyId: string;
	currencyCode: string;
	currencyName: string;
	constructionYear: number;
	buildingType: BuildingTypeValue;
	numberOfFloors: number;
	surfaceArea: number;
	insuredValue: number;
	riskIndicators: string;
}

export interface PolicyBrokerDetails {
	id: string;
	brokerCode: string;
	name: string;
	brokerStatus: number | string;
}

export interface PolicyAdjustment {
	id: string;
	name: string;
	adjustmentType: AdjustmentTypeValue;
	percentage: number;
	amount: number;
}

export interface PolicyDetails {
	id: string;
	policyNumber: string;
	policyStatus: PolicyStatusValue;
	versionNumber: number;
	startDate: string;
	endDate: string;
	basePremium: number;
	finalPremium: number;
	currencyCode: string;
	currencyName: string;
	client: PolicyClientDetails;
	building: PolicyBuildingDetails;
	broker: PolicyBrokerDetails;
	policyAdjustments: PolicyAdjustment[];
	cancelledAt?: string | null;
	cancellationReason?: string | null;
}

export interface PolicyVersion {
	id: string;
	policyId: string;
	versionNumber: number;
	startDate: string;
	endDate: string;
	basePremium: number;
	finalPremium: number;
	currencyId: string;
	currencyCode: string;
	createdAt: string;
	createdBy: string;
}

export interface PolicyEndorsement {
	id: string;
	policyId: string;
	policyNumber?: string;
	clientName?: string;
	endorsementType?: EndorsementTypeValue | string;
	reason?: string;
	oldVersionNumber?: number;
	versionNumber: number;
	effectiveDate: string;
	previousFinalPremium?: number;
	newFinalPremium?: number;
	currencyCode?: string;
	policyStatus?: PolicyStatusValue;
	createdBy: string;
	createdAt: string;
}

export interface PolicyClaim {
	id: string;
	policyId: string;
	description: string;
	incidentDate: string;
	estimatedDamage: number;
	approvedAmount?: number | null;
	status: string;
	rejectionReason?: string | null;
	createdAt: string;
	reviewedAt?: string | null;
	approvedAt?: string | null;
	rejectedAt?: string | null;
	paidAt?: string | null;
}

export interface GetPoliciesParams {
	clientId?: string;
	brokerId?: string;
	policyStatus?: PolicyStatusFilter;
	startDate?: string;
	endDate?: string;
	pageNumber?: number;
	pageSize?: number;
}

export interface CreatePolicyDraftRequest {
	clientId: string;
	buildingId: string;
	currencyId: string;
	policyNumber?: string | null;
	basePremium: number;
	startDate: string;
	endDate: string;
	brokerId: string;
}

export interface CancelPolicyRequest {
	cancellationDate?: string | null;
	cancellationReason: string;
}

export interface CreatePolicyEndorsementRequest {
	endorsementType: EndorsementTypeValue;
	effectiveDate: string;
	reason: string;
	newBasePremium?: number | null;
	newStartDate?: string | null;
	newEndDate?: string | null;
	manualAdjustementPercentage?: number | null;
}

export interface CreateClaimRequest {
	description: string;
	incidentDate: string;
	estimatedDamage: number;
}

export type PolicyListResponse = PagedResult<PolicyListItem>;
