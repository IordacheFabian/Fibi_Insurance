import { apiClient, getApiErrorMessage } from "@/lib/axios";
import { getPolicies, getPolicyClaims } from "@/lib/policies/policy.api";
import type { PolicyClaim } from "@/lib/policies/policy.types";
import type {
	AdminClaimSummary,
	ApproveClaimRequest,
	ClaimListItem,
	ClaimStatusValue,
	ClaimsPageResponse,
	NormalizedClaimStatus,
	RejectClaimRequest,
} from "./claim.types";

const DEFAULT_PAGE_SIZE = 100;

const CLAIM_STATUS_BY_NUMBER: Record<number, NormalizedClaimStatus> = {
	1: "submitted",
	2: "under_review",
	3: "approved",
	4: "rejected",
	5: "paid",
};

export function normalizeClaimStatus(status: ClaimStatusValue): NormalizedClaimStatus {
	if (typeof status === "number") {
		return CLAIM_STATUS_BY_NUMBER[status] ?? "submitted";
	}

	if (/^\d+$/.test(status)) {
		return CLAIM_STATUS_BY_NUMBER[Number(status)] ?? "submitted";
	}

	const normalized = status.replace(/([a-z])([A-Z])/g, "$1_$2").toLowerCase();

	if (normalized === "submitted" || normalized === "under_review" || normalized === "approved" || normalized === "rejected" || normalized === "paid") {
		return normalized;
	}

	return "submitted";
}

export function formatClaimReference(claimId: string): string {
	const [head] = claimId.split("-");
	return `CLM-${head.toUpperCase()}`;
}

function mapAdminClaim(claim: AdminClaimSummary): ClaimListItem {
	return {
		id: claim.id,
		reference: formatClaimReference(claim.id),
		policyId: claim.policyId,
		policyNumber: claim.policyNumber,
		clientName: claim.clientName,
		status: normalizeClaimStatus(claim.status),
		estimatedDamage: claim.estimatedDamage,
		approvedAmount: claim.approvedAmount,
		createdAt: claim.createdAt,
		source: "admin",
	};
}

function mapBrokerClaim(policyClaim: PolicyClaim, policyNumber: string, clientName: string, currencyCode: string): ClaimListItem {
	return {
		id: policyClaim.id,
		reference: formatClaimReference(policyClaim.id),
		policyId: policyClaim.policyId,
		policyNumber,
		clientName,
		status: normalizeClaimStatus(policyClaim.status),
		estimatedDamage: policyClaim.estimatedDamage,
		approvedAmount: policyClaim.approvedAmount,
		createdAt: policyClaim.createdAt,
		incidentDate: policyClaim.incidentDate,
		description: policyClaim.description,
		rejectionReason: policyClaim.rejectionReason,
		currencyCode,
		source: "broker",
	};
}

async function getAdminClaimsPage(pageNumber: number, pageSize: number): Promise<ClaimsPageResponse> {
	try {
		const { data } = await apiClient.get<ClaimsPageResponse>("/api/admin/claims", {
			params: { pageNumber, pageSize },
		});

		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch claims"));
	}
}

export async function getAdminClaims(pageSize = DEFAULT_PAGE_SIZE): Promise<ClaimListItem[]> {
	const claims: ClaimListItem[] = [];
	let pageNumber = 1;
	let hasNextPage = true;

	while (hasNextPage) {
		const page = await getAdminClaimsPage(pageNumber, pageSize);
		claims.push(...page.items.map(mapAdminClaim));
		hasNextPage = page.hasNextPage;
		pageNumber += 1;
	}

	return claims;
}

export async function getBrokerClaims(): Promise<ClaimListItem[]> {
	const policies = await getPolicies();
	const claimGroups = await Promise.all(
		policies.map(async (policy) => {
			const claims = await getPolicyClaims(policy.id);
			return claims.map((claim) => mapBrokerClaim(claim, policy.policyNumber, policy.clientName, policy.currencyCode));
		}),
	);

	return claimGroups
		.flat()
		.sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime());
}

export async function moveClaimToReview(claimId: string): Promise<void> {
	try {
		await apiClient.post(`/api/admin/claims/${claimId}/review`);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to move claim to review"));
	}
}

export async function approveClaim(claimId: string, payload: ApproveClaimRequest): Promise<void> {
	try {
		await apiClient.post(`/api/admin/claims/${claimId}/approve`, payload);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to approve claim"));
	}
}

export async function rejectClaim(claimId: string, payload: RejectClaimRequest): Promise<void> {
	try {
		await apiClient.post(`/api/admin/claims/${claimId}/reject`, payload);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to reject claim"));
	}
}

export async function payClaim(claimId: string): Promise<void> {
	try {
		await apiClient.post(`/api/admin/claims/${claimId}/pay`);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to mark claim as paid"));
	}
}
