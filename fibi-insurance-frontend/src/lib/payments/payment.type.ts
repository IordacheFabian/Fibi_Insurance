export type PaymentMethodValue = number | "Cash" | "Card" | "BankTransfer" | "cash" | "card" | "banktransfer" | "bank_transfer" | `${number}`;

export type PaymentStatusValue = number | "Pending" | "Completed" | "Failed" | "pending" | "completed" | "failed" | `${number}`;

export type NormalizedPaymentStatus = "pending" | "paid" | "failed";

export type NormalizedPaymentMethod = "cash" | "card" | "bank_transfer";

export interface PaymentListItem {
	id: string;
	policyId: string;
	policyNumber: string;
	clientName: string;
	amount: number;
	currencyId: string;
	currencyCode: string;
	paymentDate: string;
	method: PaymentMethodValue;
	status: PaymentStatusValue;
}

export interface PaymentRecord {
	id: string;
	policyId: string;
	policyNumber: string;
	clientName: string;
	amount: number;
	currencyId: string;
	currencyCode: string;
	paymentDate: string;
	method: NormalizedPaymentMethod;
	methodLabel: string;
	status: NormalizedPaymentStatus;
	statusLabel: string;
}

export interface CreatePaymentRequest {
	amount: number;
	currencyId: string;
	paymentDate: string;
	method: "Cash" | "Card" | "BankTransfer";
	status: "Pending" | "Completed" | "Failed";
}
