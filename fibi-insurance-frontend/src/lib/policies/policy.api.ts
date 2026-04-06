import { apiClient, getApiErrorMessage } from "../axios";
import type {
	CancelPolicyRequest,
	CreateClaimRequest,
	CreatePolicyDraftRequest,
	CreatePolicyEndorsementRequest,
	GetPoliciesParams,
	PolicyClaim,
	PolicyDetails,
	PolicyEndorsement,
	PolicyListItem,
	PolicyListResponse,
	PolicyStatusValue,
	PolicyVersion,
} from "./policy.types";

const DEFAULT_PAGE_SIZE = 100;

const POLICY_STATUS_BY_NUMBER = ["draft", "active", "expired", "cancelled"] as const;

export function normalizePolicyStatus(status: PolicyStatusValue): typeof POLICY_STATUS_BY_NUMBER[number] {
	if (typeof status === "number") {
		return POLICY_STATUS_BY_NUMBER[status] ?? "draft";
	}

	if (/^\d+$/.test(status)) {
		const numericStatus = Number(status);
		return POLICY_STATUS_BY_NUMBER[numericStatus] ?? "draft";
	}

	const normalizedStatus = status.toLowerCase();

	if (normalizedStatus === "active" || normalizedStatus === "draft" || normalizedStatus === "expired" || normalizedStatus === "cancelled") {
		return normalizedStatus;
	}

	return "draft";
}

function mapPolicyStatusToApiValue(status?: GetPoliciesParams["policyStatus"]): string | undefined {
	if (!status) {
		return undefined;
	}

	const normalizedStatus = normalizePolicyStatus(status);
	return normalizedStatus.charAt(0).toUpperCase() + normalizedStatus.slice(1);
}

async function getPoliciesPage(filters: GetPoliciesParams, pageNumber: number, pageSize: number): Promise<PolicyListResponse> {
	try {
		const { data } = await apiClient.get<PolicyListResponse>("/api/brokers/policies", {
			params: {
				clientId: filters.clientId,
				brokerId: filters.brokerId,
				policyStatus: mapPolicyStatusToApiValue(filters.policyStatus),
				startDate: filters.startDate,
				endDate: filters.endDate,
				pageNumber,
				pageSize,
			},
		});

		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch policies"));
	}
}

export async function getPolicies(filters: GetPoliciesParams = {}, pageSize = DEFAULT_PAGE_SIZE): Promise<PolicyListItem[]> {
	const policies: PolicyListItem[] = [];
	let pageNumber = filters.pageNumber ?? 1;
	let hasNextPage = true;

	while (hasNextPage) {
		const page = await getPoliciesPage(filters, pageNumber, filters.pageSize ?? pageSize);
		policies.push(...page.items);
		hasNextPage = page.hasNextPage;
		pageNumber += 1;
	}

	return policies;
}

export async function getPolicyById(policyId: string): Promise<PolicyDetails> {
	try {
		const { data } = await apiClient.get<PolicyDetails>(`/api/brokers/policies/${policyId}`);
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch policy"));
	}
}

export async function getPolicyEndorsements(policyId: string): Promise<PolicyEndorsement[]> {
	try {
		const { data } = await apiClient.get<PolicyEndorsement[]>(`/api/brokers/policies/${policyId}/endorsements`);
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch policy endorsements"));
	}
}

export async function getEndorsements(): Promise<PolicyEndorsement[]> {
	try {
		const { data } = await apiClient.get<PolicyEndorsement[]>("/api/brokers/endorsements");
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch endorsements"));
	}
}

export async function getPolicyVersions(policyId: string): Promise<PolicyVersion[]> {
	try {
		const { data } = await apiClient.get<PolicyVersion[]>(`/api/brokers/policies/${policyId}/versions`);
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch policy versions"));
	}
}

export async function getPolicyClaims(policyId: string): Promise<PolicyClaim[]> {
	try {
		const { data } = await apiClient.get<PolicyClaim[]>(`/api/brokers/policies/${policyId}/claims`);
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch policy claims"));
	}
}

export async function getClaimById(claimId: string): Promise<PolicyClaim> {
	try {
		const { data } = await apiClient.get<PolicyClaim>(`/api/brokers/claims/${claimId}`);
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch claim"));
	}
}

export async function createPolicyDraft(payload: CreatePolicyDraftRequest): Promise<PolicyDetails> {
	try {
		const { data } = await apiClient.post<PolicyDetails>("/api/brokers/policies", payload);
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to create policy draft"));
	}
}

export async function activatePolicy(policyId: string): Promise<void> {
	try {
		await apiClient.post(`/api/brokers/policies/${policyId}/activate`);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to activate policy"));
	}
}

export async function cancelPolicy(policyId: string, payload: CancelPolicyRequest): Promise<void> {
	try {
		await apiClient.post(`/api/brokers/policies/${policyId}/cancel`, payload);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to cancel policy"));
	}
}

export async function createPolicyEndorsement(policyId: string, payload: CreatePolicyEndorsementRequest): Promise<void> {
	try {
		await apiClient.post(`/api/brokers/policies/${policyId}/endorsements`, payload);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to create policy endorsement"));
	}
}

export async function createPolicyClaim(policyId: string, payload: CreateClaimRequest): Promise<PolicyClaim> {
	try {
		const { data } = await apiClient.post<PolicyClaim>(`/api/brokers/policies/${policyId}/claims`, payload);
		return data;
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to create claim"));
	}
}
