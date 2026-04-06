import type { PagedResult } from "@/lib/clients/client.types";

export type { PagedResult } from "@/lib/clients/client.types";

export type ClaimStatusValue = string | number;

export type NormalizedClaimStatus = "submitted" | "under_review" | "approved" | "rejected" | "paid";

export interface AdminClaimSummary {
	id: string;
	policyId: string;
	policyNumber: string;
	clientName: string;
	status: ClaimStatusValue;
	estimatedDamage: number;
	approvedAmount?: number | null;
	createdAt: string;
}

export interface ClaimListItem {
	id: string;
	reference: string;
	policyId: string;
	policyNumber: string;
	clientName: string;
	status: NormalizedClaimStatus;
	estimatedDamage: number;
	approvedAmount?: number | null;
	createdAt: string;
	incidentDate?: string | null;
	description?: string;
	rejectionReason?: string | null;
	currencyCode?: string;
	source: "admin" | "broker";
}

export interface ApproveClaimRequest {
	approvedAmount: number;
}

export interface RejectClaimRequest {
	reason: string;
}

export type ClaimsPageResponse = PagedResult<AdminClaimSummary>;
