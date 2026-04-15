import { apiClient, getApiErrorMessage } from "@/lib/axios";
import type {
	CreatePaymentRequest,
	NormalizedPaymentMethod,
	NormalizedPaymentStatus,
	PaymentListItem,
	PaymentMethodValue,
	PaymentRecord,
	PaymentStatusValue,
} from "./payment.type";

export type PaymentsRole = "broker" | "admin";

const PAYMENT_STATUS_BY_NUMBER = ["pending", "paid", "failed"] as const;
const PAYMENT_METHOD_BY_NUMBER = ["cash", "card", "bank_transfer"] as const;

export function normalizePaymentStatus(status: PaymentStatusValue): NormalizedPaymentStatus {
	if (typeof status === "number") {
		return PAYMENT_STATUS_BY_NUMBER[Math.max(status - 1, 0)] ?? "pending";
	}

	if (/^\d+$/.test(status)) {
		return PAYMENT_STATUS_BY_NUMBER[Math.max(Number(status) - 1, 0)] ?? "pending";
	}

	const normalized = status.toLowerCase();

	if (normalized === "completed") {
		return "paid";
	}

	if (normalized === "failed") {
		return "failed";
	}

	return "pending";
}

export function normalizePaymentMethod(method: PaymentMethodValue): NormalizedPaymentMethod {
	if (typeof method === "number") {
		return PAYMENT_METHOD_BY_NUMBER[Math.max(method - 1, 0)] ?? "cash";
	}

	if (/^\d+$/.test(method)) {
		return PAYMENT_METHOD_BY_NUMBER[Math.max(Number(method) - 1, 0)] ?? "cash";
	}

	const normalized = method.toLowerCase().replace(/\s+/g, "_");

	if (normalized === "banktransfer") {
		return "bank_transfer";
	}

	if (normalized === "card" || normalized === "bank_transfer") {
		return normalized;
	}

	return "cash";
}

export function getPaymentMethodLabel(method: PaymentMethodValue) {
	const normalizedMethod = normalizePaymentMethod(method);

	if (normalizedMethod === "bank_transfer") {
		return "Bank Transfer";
	}

	return normalizedMethod.charAt(0).toUpperCase() + normalizedMethod.slice(1);
}

export function getPaymentStatusLabel(status: PaymentStatusValue) {
	const normalizedStatus = normalizePaymentStatus(status);

	if (normalizedStatus === "paid") {
		return "Paid";
	}

	return normalizedStatus.charAt(0).toUpperCase() + normalizedStatus.slice(1);
}

function getPaymentsPath(role: PaymentsRole) {
	return role === "admin" ? "/api/admin/payments" : "/api/brokers/payments";
}

function mapPaymentRecord(payment: PaymentListItem): PaymentRecord {
	return {
		...payment,
		method: normalizePaymentMethod(payment.method),
		methodLabel: getPaymentMethodLabel(payment.method),
		status: normalizePaymentStatus(payment.status),
		statusLabel: getPaymentStatusLabel(payment.status),
	};
}

export async function getPayments(role: PaymentsRole = "broker"): Promise<PaymentRecord[]> {
	try {
		const { data } = await apiClient.get<PaymentListItem[]>(getPaymentsPath(role));
		return data.map(mapPaymentRecord);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch payments"));
	}
}

export async function getPolicyPayments(policyId: string, role: PaymentsRole = "broker"): Promise<PaymentRecord[]> {
	try {
		const path = role === "admin"
			? `/api/admin/policies/${policyId}/payments`
			: `/api/brokers/policies/${policyId}/payments`;
		const { data } = await apiClient.get<PaymentListItem[]>(path);
		return data.map(mapPaymentRecord);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to fetch policy payments"));
	}
}

export async function createPayment(policyId: string, payload: CreatePaymentRequest): Promise<PaymentRecord> {
	try {
		const { data } = await apiClient.post<PaymentListItem>(`/api/brokers/policies/${policyId}/payments`, payload);
		return mapPaymentRecord(data);
	} catch (error) {
		throw new Error(getApiErrorMessage(error, "Failed to record payment"));
	}
}
